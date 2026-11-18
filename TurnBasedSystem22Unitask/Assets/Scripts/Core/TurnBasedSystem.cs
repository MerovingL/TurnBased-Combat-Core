
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

public enum BattleStates
{
    Peace,
    Start,
    HeroesPrepare,
    Hero00,
    Hero01,
    Hero02,
    AiPrepare,
    Ai,
    Lose,
    Win,
    Ally
}

public class TurnBasedSystem : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private MonoBehaviour _cameraServiceSource;
    private ICameraService _cameraService;
    [SerializeField] private MonoBehaviour _soundServiceSource;
    private ISoundService _soundService;
    [SerializeField] private MonoBehaviour _hudServiceSource;
    private IPeaceUi _peaceUiService;
    private ITurnUi _turnUiService;
    private IHpUi _hpUiService;
    private ISkillUi _skillUiService;
    private IBonusUi _bonusUiService;
    private IHelpersUi _helpersUiService;
    [SerializeField] private MonoBehaviour _globalDataSource;
    private IDeveloperSettings _developerSettings;
    private IHeroControlService _heroControlService;
    private IHeroParty _heroParty;
    private IManualService _manualService;
    private ISaveLoadSystem _saveLoadSystem;
    private ILevelSystem _levelSystem;
    private IRageSystem _rageSystem;
    private ILayerProvider _layerProvider;

    [field: Header("References")]
    [field: SerializeField] public GameObject RoadBlock { get; private set; }

    [Header("Settings")]
    [SerializeField] private float _globalTurnDuration;
    public float GlobalTurnDuration => _globalTurnDuration;
    private float _privateTurnOffset;
    [SerializeField] private float _enemyListRadius;

    [Header("Runtime Lists")]
    [SerializeField, ReadOnly] private List<Ai> _enemies;
    [SerializeField, ReadOnly] private List<Ai> _allies;
    [SerializeField, ReadOnly] private List<Actor> _actors;
    [SerializeField, ReadOnly] private List<Actor> _bosses;
    [SerializeField, ReadOnly] private List<Ai> _bigWalkers;
    [SerializeField, ReadOnly] private List<Ai> _smallEnemies;
    [SerializeField, ReadOnly] private List<HP> _destroyableObjects;
    [SerializeField, ReadOnly] private List<GameObject> _bonuses;
    [SerializeField, ReadOnly] private List<Actor> _colliderActors;
    [SerializeField, ReadOnly] private List<TurnCheck> _turnCheckList;
    public IReadOnlyList<Ai> Enemies => _enemies;
    public IReadOnlyList<Ai> Allies => _allies;
    public IReadOnlyList<Actor> Actors => _actors;
    public IReadOnlyList<Actor> Bosses => _bosses;
    public IReadOnlyList<Ai> BigWalkers => _bigWalkers;
    public IReadOnlyList<Ai> SmallEnemies => _smallEnemies;
    public IReadOnlyList<HP> DestroyableObjects => _destroyableObjects;
    public IReadOnlyList<GameObject> Bonuses => _bonuses;
    public IReadOnlyList<Actor> ColliderActors => _colliderActors;
    public IReadOnlyList<TurnCheck> TurnCheckList => _turnCheckList;

    [field: Header("Runtime State")]
    [field: SerializeField, ReadOnly] public BattleStates CurrentState { get; private set; }
    [field: SerializeField, ReadOnly] public int HeroIndex { get; private set; }
    [field: SerializeField, ReadOnly] public int TurnNumber { get; private set; }
    [field: SerializeField, ReadOnly] public bool IsFirstTurnTriggered { get; private set; }
    [field: SerializeField, ReadOnly] public bool IsCameraControlYielded { get; set; }
    [field: SerializeField, ReadOnly] public bool IsPaused { get; set; }//for Horn scenes
    private Func<UniTask> _endBattleScene;    //used in horn scene - update like in pr boss and use actor list, and remove this then

    [ReadOnly] public GameObject currentHero;
    private bool _manual;
    private Vector3 _spherePosition;
    private Collider[] _enemySearchResults = new Collider[10];

    private void Awake()
    {
        _cameraService = ResolveService<ICameraService>(_cameraServiceSource, nameof(_cameraServiceSource));

        _soundService = ResolveService<ISoundService>(_soundServiceSource, nameof(_soundServiceSource));

        _peaceUiService = ResolveService<IPeaceUi>(_hudServiceSource, nameof(_hudServiceSource));
        _turnUiService = ResolveService<ITurnUi>(_hudServiceSource, nameof(_hudServiceSource));
        _hpUiService = ResolveService<IHpUi>(_hudServiceSource, nameof(_hudServiceSource));
        _skillUiService = ResolveService<ISkillUi>(_hudServiceSource, nameof(_hudServiceSource));
        _bonusUiService = ResolveService<IBonusUi>(_hudServiceSource, nameof(_hudServiceSource));
        _helpersUiService = ResolveService<IHelpersUi>(_hudServiceSource, nameof(_hudServiceSource));

        _developerSettings = ResolveService<IDeveloperSettings>(_globalDataSource, nameof(_globalDataSource));
        _heroControlService = ResolveService<IHeroControlService>(_globalDataSource, nameof(_globalDataSource));
        _heroParty = ResolveService<IHeroParty>(_globalDataSource, nameof(_globalDataSource));
        _manualService = ResolveService<IManualService>(_globalDataSource, nameof(_globalDataSource));
        _saveLoadSystem = ResolveService<ISaveLoadSystem>(_globalDataSource, nameof(_globalDataSource));
        _levelSystem = ResolveService<ILevelSystem>(_globalDataSource, nameof(_globalDataSource));
        _rageSystem = ResolveService<IRageSystem>(_globalDataSource, nameof(_globalDataSource));
        _layerProvider = ResolveService<ILayerProvider>(_globalDataSource, nameof(_globalDataSource));

    }

    private T ResolveService<T>(MonoBehaviour source, string fieldName) where T : class
    {
        if (source is T service)
        {
            return service;
        }

        Debug.LogError($"{name}: {fieldName} does not implement {typeof(T).Name}", this);
        return null;
    }

    void Start()
    {
        _hpUiService.HidePartyHp();
        _hpUiService.HideEnemyHp();
        TurnNumber = 1;
        _enemies.Clear();
        _actors.Clear();
        _destroyableObjects.Clear();
        _bonuses.Clear();
        _cameraService.RunPeaceMode();
    }

    public async UniTask Run()
    {
        var token = destroyCancellationToken;
        try
        {
            while (true)
            {
                BattleStates nextState = await Step(CurrentState);
                if (nextState == CurrentState)
                {
                    await UniTask.NextFrame(token);//defence from eternal loop
                }
                CurrentState = nextState;
            }
        }
        catch
        (OperationCanceledException)
        { }//at onDestroy
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    async UniTask<BattleStates> Step(BattleStates state)
    {
        switch (state)
        {
            case BattleStates.Peace:
                return await StepPeace();
            case BattleStates.Start:
                return await StepStart();
            case BattleStates.AiPrepare:
                return await StepAiPrepare();
            case BattleStates.Ai:
                return await StepAiTurn();
            case BattleStates.Ally:
                return await StepAllyTurn();
            case BattleStates.HeroesPrepare:
                return await StepHeroPrepare();
            case BattleStates.Hero00:
                return await StepHero00Turn();
            case BattleStates.Hero01:
                return await StepHero01Turn();
            case BattleStates.Hero02:
                return await StepHero02Turn();
            case BattleStates.Lose:
                return await StepLose();
            case BattleStates.Win:
                return await StepWin();

            default: return state;
        }
    }

    private async UniTask<BattleStates> StepPeace()
    {
        var token = destroyCancellationToken;

        if (!_manualService.MoveByMouseManualState && !_developerSettings.SkipManual)
            _manualService.RunManual();

        foreach (var el in _heroParty.HeroClass)
            el.enabled = true;

        IsFirstTurnTriggered = false;//to not move camera if single PR

        _enemies.Clear();
        _destroyableObjects.Clear();
        _bonuses.Clear();
        _actors.Clear();

        // Enemy search
        while (_enemies.Count == 0 && _bosses.Count == 0)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: token);//little opt, repeat 10 times per second only

            TargList();//Search mobs and actors

            if (_actors.Count > 0)//Peace actor activator, run actor 4 times to make shure all awailable actions of actors in it and its roots are done
                for (int i = 0; i < 4; i++)
                    await RunActor();

            foreach (var el in _enemies)//get ctrl on mobs from coll triggers
            {
                var group = el.GetComponentInParent<Group>();
                if (group && group.showType is Group.showTypes.moveFromLeftCol or Group.showTypes.moveFromRightCol)
                    group.GetControl();
            }

            if (_enemies.Count > 1 || _bosses.Count > 0)//start battle music and presentation
            {
                SetCursorVisibility(false);
                _soundService.BattleMusicSmooth();
                await BattlePresentation();
            }
        }

        _soundService.PeaceMusicVolumeDown(true);//to stop peace music in case if some actor play and volume down peace without stop
        _soundService.EventVolumeDown(true);//todoSound.i.eventMusicLongPlayer.VolumeDown(true);

        return BattleStates.Start;
    }

    private async UniTask<BattleStates> StepStart()
    {
        var token = destroyCancellationToken;

        if (_levelSystem.WasPassivePanelOpened)
        {
            _levelSystem.SetInteractablePassives(false);//if lvl up awaits new skills pick
        }

        SetCursorVisibility(false);
        _peaceUiService.FadePeaceUi(1);
        _peaceUiService.SetActivePeaceUi(false);

        await UniTask.NextFrame(token);

        _heroControlService.StopHeroes();

        await UniTask.NextFrame(token);

        foreach (var el in _heroParty.HeroClass)
        {
            var obs = el.obstacle;
            obs.enabled = true;
            obs.carving = true;
        }

        await UniTask.NextFrame(token);

        RoadBlock.transform.position = new Vector3(_heroParty.Heroes[0].transform.position.x + 23, 0, 8);

        return BattleStates.AiPrepare;
    }

    private async UniTask<BattleStates> StepAiPrepare()
    {
        if (CheckWin())
        {
            return BattleStates.Win;
        }
        if (CheckLose())
        {
            return BattleStates.Lose;
        }

        _turnUiService.FadeMainMenuButton(1);
        _heroControlService.CanControl = false;
        SetCursorVisibility(false);
        HeroIndex = _heroParty.Heroes.Count;//to block check pass error

        TargList();//Get mob list

        foreach (var el in _enemies)
            el.enableHpClass();

        _hpUiService.HideEnemyHp();

        // Check and get Ctrl over Coll trig move from Left or right mobs during battle
        foreach (var el in _enemies)
        {
            if (el.TryGetComponent(out Group group) && group.showType is Group.showTypes.moveFromLeftCol or Group.showTypes.moveFromRightCol)
            {
                group.GetControl();
            }
        }

        // Turn on coll of alive heroes to recive aoe damage 
        foreach (var el in _heroParty.HeroHps)
        {
            if (!el.dead)
            {
                el.GetComponent<Collider>().enabled = true;
            }
        }

        // Boss turn start
        var bossLCopy = new List<Actor>(_bosses);
        foreach (var el in bossLCopy)
        {
            if (!el.GetComponent<Effects>().stuned)
            {
                await el.GetComponent<Ai>().Run();
            }
        }

        // Actor run
        await RunActor();

        TargList();//if actor summoned new monsters add it to the list

        return CheckWin() ? BattleStates.Win : CheckLose() ? BattleStates.Lose : BattleStates.Ai;
    }
    private async UniTask<BattleStates> StepAiTurn()
    {
        var token = destroyCancellationToken;

        foreach (var el in _enemies)// turn off all nav and carve for small & med to let big walkers move without obstacles
        {
            el.TurnOffNavMeshAgent();
            el.TurnOffObstacleCarve();
        }

        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: token);//little delay after hero turn and enemy obst off

        // Ai camera
        _hpUiService.ShowPartyHp();
        _cameraService.EnemyTurnMode();

        // check and activate turn base skills
        foreach (var el in _turnCheckList)
        {
            el.RunTurnCheck();
        }

        // Prepare distance to targ
        var AILCopy = new List<Ai>(_enemies);//Work with copy of col list, to prevent errors when modifying list in iteration
        foreach (var el in AILCopy)
        {
            if (el.TryGetComponent(out AiMob aiMob))
            {
                aiMob.DistToTarg();
            }
        }
        AILCopy.Sort((a, b) => a.distanceToTarget.sqrMagnitude.CompareTo(b.distanceToTarget.sqrMagnitude));

        // Big walkers turn
        var bigEnemyTasks = new List<UniTask>();
        foreach (var el in AILCopy)
        {
            if (el.CheckForState(enemyTypes.bigWalker) && CheckAIState(el))
            {
                bigEnemyTasks.Add(el.Run());
            }
        }

        await UniTask.WhenAll(bigEnemyTasks);

        // Check states
        if (CheckWin())
        {
            return BattleStates.Win;
        }
        if (CheckLose())
        {
            return BattleStates.Lose;
        }

        // TurnOffNavMesh for Ai
        foreach (var el in AILCopy)//turn of nav to Big to let small and med to move under them
        {
            if (el.CheckForState(enemyTypes.bigWalker)) el.TurnOffNavMeshAgent();
        }
        // Ai main turn
        var mainEnemyTasks = new List<UniTask>();
        foreach (var el in AILCopy)
        {
            if (!((el.CheckForState(enemyTypes.small) || el.CheckForState(enemyTypes.medium)) && CheckAIState(el)))
            {
                continue;
            }
            if (el.TryGetComponent(out AiMob mob) && mob.skipOneTurn)
            {
                mob.skipOneTurn = false;
            }
            else
            {
                mainEnemyTasks.Add(el.Run(_privateTurnOffset * mainEnemyTasks.Count));
            }
        }

        await UniTask.WhenAll(mainEnemyTasks);

        // End negative effects
        EffectsOnEnemies();

        // Ai Off nav On obst
        foreach (var el in _enemies)
            el.TurnOffNavMeshAgent();
        await UniTask.NextFrame(token);
        foreach (var el in _enemies)
            el.TurnOffObstacleCarve();

        IsFirstTurnTriggered = true;

        return CheckWin() ? BattleStates.Win : CheckLose() ? BattleStates.Lose : _enemies.Count == 0 && _bosses.Count == 0 && _actors.Count > 0 ? BattleStates.AiPrepare : BattleStates.Ally;
    }

    private async UniTask<BattleStates> StepAllyTurn()
    {
        _hpUiService.HidePartyHp();// Turn off col to see less numbers, whe do not need to see ally hp when ally does damage
        _hpUiService.ShowEnemyHp();// Show enemy hp

        RoadBlockOff(false);

        var allyCopyL = new List<Ai>(_allies);
        var aiTasks = new List<UniTask>();
        foreach (var el in allyCopyL)
        {
            if (CheckAIState(el)) aiTasks.Add(el.Run(aiTasks.Count * _privateTurnOffset));
        }
        await UniTask.WhenAll(aiTasks);

        TargList();// Refresh enemy list

        if (_enemies.Count == 0 && _bosses.Count == 0 && _actors.Count > 0)// If some actor in battle waiting for casting (if ally killed all visible mobs)
        {
            return BattleStates.AiPrepare;
        }

        _hpUiService.ShowPartyHp();

        return BattleStates.HeroesPrepare;
    }

    private async UniTask<BattleStates> StepHeroPrepare()
    {
        TargList();

        // Turn off colliders to select mobs behind mercs
        foreach (var el in _heroParty.Heroes)
        {
            el.GetComponent<Collider>().enabled = false;// We use layer mask now in hud, need check
        }

        // UI
        _hpUiService.ShowPartyHp();
        SetCursorVisibility(true);

        if (CheckWin())
        {
            return BattleStates.Win;
        }

        if (!_manual)
        {
            _manual = true;
            _manualService.RunBaseAttackManual();
        }

        _skillUiService.SetInactiveAllHeroSkillButtons();
        _skillUiService.MoveUpSkillPanel();
        _turnUiService.SetInteractableEndTurnButton(false);
        _turnUiService.SetActiveEndTurnButton(true);
        _turnUiService.UnfadeMainMenuButton(1);

        // End effects on heroes
        foreach (var el in _heroParty.HeroHps)
        {
            el.effects.EndPositiveEffects();
        }

        await UniTask.NextFrame(destroyCancellationToken);
        return BattleStates.Hero00;
    }

    private async UniTask<BattleStates> StepHero00Turn()
    {
        HeroIndex = 0;

        await _heroParty.HeroHps[0].PassiveWarriorRiseEnd();// Сhecking "Rise" pass

        if (!_heroParty.HeroHps[0].dead)// Skip turn if hero is dead
        {
            await _heroParty.HeroHps[0].PassiveWarriorRegen();// Checking "Regen" pass
        }

        await RunHeroTurn(HeroIndex);

        return CheckWin() ? BattleStates.Win : BattleStates.Hero01;// Switch to next currentHero if not win
    }
    private async UniTask<BattleStates> StepHero01Turn()
    {
        HeroIndex = 1;

        await RunHeroTurn(HeroIndex);

        return CheckWin() ? BattleStates.Win : BattleStates.Hero02;// Switch to next currentHero if not win
    }
    private async UniTask<BattleStates> StepHero02Turn()
    {
        HeroIndex = 2;

        await RunHeroTurn(HeroIndex);

        return CheckWin() ? BattleStates.Win : CheckLose() ? BattleStates.Lose : BattleStates.AiPrepare;// Switch to next currentHero if not win
    }
    async UniTask RunHeroTurn(int heroIndex)
    {
        if (_heroParty.HeroHps[heroIndex].dead)
            return;

        var token = destroyCancellationToken;

        _hpUiService.ShowEnemyHp();// Show enemy col
        _skillUiService.SetInactiveAllHeroSkillButtons();// Tturn off all skill button gameobjects
        _skillUiService.SetInterctableSkillButtons(heroIndex, false);// Set button comps not interactable
        _skillUiService.DecreaseSkillCooldowns(heroIndex);// Decreace coold down of skilles for 1
        _skillUiService.SetActiveSkillButtons(heroIndex, true);// Turn on current hero skill button's gameobjects
        currentHero = _heroParty.Heroes[heroIndex];// Set current currentHero
        _cameraService.HeroTurnMode(currentHero.transform).Forget();// Hero turn cam mode
        await _heroParty.Heroes[heroIndex].GetComponent<ShrineEffect>().Run();// Run shrine effects

        if (CheckWin())// If shrine like thunderbolt killed last mob change state to win
        {
            return;//BattleStates.Win;
        }

        _skillUiService.SetInterctableSkillButtons(heroIndex, true);
        _skillUiService.SetSkillButtonsFadeDuration(0.1f, 1);
        await GiveBattleControl(heroIndex);// Give full control to player
        await UniTask.WaitUntil(() => CheckState(heroIndex), cancellationToken: token);// Here player do his turn 
        await HeroTurnEnd(heroIndex);// Make all finish turn technical staff
        _hpUiService.HideEnemyHp();
        await RunActor();// Check if some actor conditions are completed
        await UniTask.WaitWhile(() => _bonusUiService.BonusQueueCount > 0 || _bonusUiService.BonusIsAdding, cancellationToken: token);// Waiting ui bonus animations
    }

    private async UniTask<BattleStates> StepLose()
    {
        _skillUiService.ResetAllCooldowns();
        _peaceUiService.SetActivePeaceUi(true);
        _levelSystem.SetInteractablePassives(true);
        RoadBlockOff(true);
        _rageSystem.Reset();

        await _cameraService.LensFade(1f, 1.66f);

        await _saveLoadSystem.LoadGame();

        return BattleStates.Peace;
    }

    private async UniTask<BattleStates> StepWin(bool load = false)
    {
        TargList();

        if (!CheckWin())
            return BattleStates.AiPrepare;

        var token = destroyCancellationToken;

        //Hide, clear and reset battle UI
        _skillUiService.MoveUpSkillPanel();
        _skillUiService.ResetAllCooldowns();
        _levelSystem.SetInteractablePassives(true);//turn on pass skill buttons
        _hpUiService.HidePartyHp();//hide col numbers
        _hpUiService.HideEnemyHp();//hide en numbers
        _helpersUiService.HideHelpers();//hide attak radius helper and move radius helper
        RoadBlockOff(true);//turn of road block
        _heroParty.HeroHps[0].ResetWarriorRisePassive();//reset warrior rise passive skill
        _heroControlService.CanControl = false;//temporary turn off ctrl
        _rageSystem.TurnOffSkulls();//hide skulls
        _rageSystem.Reset();//reset rage system

        await UniTask.WaitWhile(() => _skillUiService.SkillExecuting, cancellationToken: token);//waiting to complete last skill

        // Clear battle remains
        foreach (var el in _destroyableObjects)//kill barrikades
        {
            if (el.TryGetComponent(out BonusSpawn bs))
            {
                bs.doNotSpawnBonus = true;// Do not spawn bonus from destroyable objects when clear scene
            }

            el.RemoveHp(9999);
        }

        foreach (var el in _bonuses)//destroy bonus gameobjects in scene
        {
            Destroy(el);
        }

        foreach (var el in _bonusUiService.BonusButtons)//destroy achived bonus skills in ui
        {
            Destroy(el.gameObject);
        }
        _bonusUiService.ClearBonusButtons();//

        foreach (TurnCheck el in _turnCheckList)//destroy turn based skills
        {
            Destroy(el.gameObject);
        }
        _turnCheckList.Clear();//clear turn based skill list

        // End battle scene
        if (_endBattleScene != null)
        {
            await _endBattleScene();
            _endBattleScene = null;
        }

        // Hold TB if pause for Horn episodes
        await UniTask.WaitWhile(() => IsPaused, cancellationToken: token);

        // Heal and ressurect
        var taskL = new List<UniTask>();
        foreach (var el in _heroParty.HeroHps)
        {
            taskL.Add(el.RessurectAndHeal(false, load));
        }
        await UniTask.WhenAll(taskL);//wait ressurect before move to poses, to avoid moving tombstome

        // Return if growUp bonus
        foreach (var el in _heroParty.HeroHps)
        {
            el.ReturnSizeAndHp(false);
        }

        // Set defaut peace stuff and reset invis
        foreach (var el in _heroParty.HeroClass)
        {
            el.GetComponent<Collider>().enabled = true;
            el.enabled = true;
            el.invis = 0;
        }

        // Set move right pose
        if (!load)
        {
            foreach (var el in _heroParty.HeroClass)
            {
                el.MoveToWinPosition();
            }
        }

        //Set peace mode
        _peaceUiService.SetActivePeaceUi(true);// Set peace UI
        CurrentState = BattleStates.Peace;// Set turn base system to peace mode
        _cameraService.RunPeaceMode();// Change camera to front peace mode
        _soundService.PlayPeaceMusicSmooth();// Start peace music

        // Return control
        _helpersUiService.HideHelpers();//doublicate to awoid stuck in helper
        _peaceUiService.UnfadePeaceUi(1);
        _helpersUiService.HideHelpers(0.33f);//doublicate to awoid stuck in helper
        SetCursorVisibility(true);//Cursor.lockState = CursorLockMode.None;
        _heroControlService.CanControl = true;

        return BattleStates.Peace;
    }

    void RoadBlockOn()
    {
        RoadBlock.SetActive(true);
        foreach (var el in RoadBlock.GetComponentsInChildren<Collider>(true))
            el.gameObject.SetActive(true);
    }

    void RoadBlockOff(bool full)
    {
        if (full)
        {
            RoadBlock.SetActive(false);
        }
        else
        {
            foreach (var el in RoadBlock.GetComponentsInChildren<Collider>())
            {
                el.gameObject.SetActive(false);
            }
        }
    }

    public void SetEndBattleScene(Func<UniTask> action) => _endBattleScene = action;

    public async UniTask HeroTurnEnd(int heroIndex)
    {
        var token = destroyCancellationToken;
        _heroControlService.CanControl = false;
        if (_levelSystem.WasPassivePanelOpened && _levelSystem.WasPassivePanelEdited)//if new pass skill was and you opend pas panel, and if you edited it, set not interact it
        {
            _levelSystem.SetInteractablePassives(false);
        }

        _turnUiService.ButtonsDisabled = false;
        _helpersUiService.HideHelpers();

        _skillUiService.SetInterctableSkillButtons(heroIndex, false);
        _turnUiService.SetInteractableEndTurnButton(false);
        _skillUiService.CancelCurrentSkill(true, false);
        _rageSystem.Reset();//when you end turn by space, but have killed by shrine without using skill directly for example

        if (_heroParty.HeroHps[heroIndex].dead)
        {
            return;
        }

        _heroParty.HeroClass[heroIndex].navMeshAgent.enabled = false;
        _heroParty.HeroClass[heroIndex].enabled = false;

        await UniTask.NextFrame(token);

        _heroParty.HeroClass[heroIndex].obstacle.enabled = true;
        _heroParty.HeroClass[heroIndex].obstacle.carving = true;

        RoadBlockOff(false);
    }

    async UniTask GiveBattleControl(int heroIndex)
    {
        var token = destroyCancellationToken;

        RoadBlockOn();//block right

        _turnUiService.ChangeState(heroIndex);
        _helpersUiService.ActivateHelpers();
        _turnUiService.SetInteractableEndTurnButton(true);
        _heroParty.HeroHps[heroIndex].enabled = false;//temporary to disable move delta to awoid little move when activating nav

        //Turn on currentHero navigation
        _heroParty.HeroClass[heroIndex].obstacle.enabled = false;
        await UniTask.NextFrame(token);
        _heroParty.HeroClass[heroIndex].navMeshAgent.enabled = true;
        await UniTask.NextFrame(token);//time to move to nav mesh

        _heroParty.HeroHps[heroIndex].enabled = true;
        _heroParty.HeroClass[heroIndex].enabled = true;
        _heroControlService.CanControl = true;
    }

    async UniTask BattlePresentation()
    {
        if (_developerSettings.SkipBattlePresentation)
        {
            return;
        }

        Group presentationGroup = null;

        if (_enemies.Count > 0)
        {
            presentationGroup = _enemies.First().GetComponentsInParent<Group>().Last();
        }
        else if (_bosses.Count > 0)
        {
            presentationGroup = _bosses.First().GetComponentsInParent<Group>().Last();
        }
        if (presentationGroup == null)
        {
            return;
        }
        if (!presentationGroup.topGroup)
        {
            return;
        }
        if (presentationGroup.showType != Group.showTypes.idle)
        {
            return;
        }
        if (presentationGroup.activationType == Group.ActivationTypes.byColl)
        {
            return;
        }
        if (presentationGroup.skipBattlePresentation)
        {
            return;
        }

        //Turn off player control
        _heroControlService.CanControl = false;
        _heroControlService.StopHeroes();
        _hpUiService.HideEnemyHp();

        //Select cam effect
        int battlePresentationNumber = presentationGroup.battlePresentation switch
        {
            battlePresentations.leftRight => 1,
            battlePresentations.fromBg => 2,
            battlePresentations.fromZero => 3,
            _ => Random.Range(1, 4)//0 reserved
        };

        if (!IsCameraControlYielded)
        {
            if (battlePresentationNumber == 0)//reserve
            {
            }
            else if (battlePresentationNumber == 1)
            {
                await _cameraService.BattlePresentationZoom();//Move camera over enemies from left to right
            }
            else if (battlePresentationNumber == 2)
            {
                await _cameraService.BattlePresentationFromBackground();//Move camera from background to enemies 
            }
            else if (battlePresentationNumber == 3)
            {
                if (presentationGroup.fromZeroTargetPresentaionContainer)//if group have target to zoom
                {
                    await _cameraService.BattlePresentationFromZeroPlane(presentationGroup.fromZeroTargetPresentaionContainer, presentationGroup.fromZeroPresentationOffset);//zoom to target if this presentation has special target
                }
                else
                {
                    await _cameraService.BattlePresentationFromZeroPlane();//default 
                }
            }
        }

        //Return player control
        _heroControlService.CanControl = false;
    }

    void EffectsOnEnemies()
    {
        foreach (var el in _enemies)
        {
            EndEffects(el);
        }

        foreach (var el in _bosses)
        {
            EndEffects(el);
        }

        static void EndEffects(Component enemy)
        {
            var effects = enemy.GetComponent<Effects>();
            effects.EndStun();
            effects.EndBlind();
        }
    }

    async UniTask RunActor()
    {

        if (_actors.Count == 0)
        {
            return;
        }

        var token = destroyCancellationToken;

        RoadBlockOff(false);

        // Turn on coll of alive mercs to take aoe damage
        List<bool> heroesCollState = new List<bool>();
        foreach (var el in _heroParty.HeroHps)
        {
            Collider col = el.GetComponent<Collider>();
            heroesCollState.Add(col.enabled);
            col.enabled = !el.dead;
        }

        // Cursor
        bool isCursorVisible = Cursor.visible;
        CancellationTokenSource cursorCts = CancellationTokenSource.CreateLinkedTokenSource(token);
        CursorInv(cursorCts.Token).Forget();// to prevent cursor flickering

        // Control
        bool canControl = _heroControlService.CanControl;
        _heroControlService.CanControl = false;

            // HideUI
        _peaceUiService.FadePeaceUi(1);

        // Doublicate actor list
        var actorLCopy = new List<Actor>(_actors);

        // Stop mercs but with exclutions
        foreach (var el in actorLCopy)//When movie close shutter, and heroes move right, do not stop it, movie will do it after shutter close
        {
            if (el.GetComponent<MovieStart>())
            {
                continue;
            }

            if (el.TryGetComponent(out Group group)
                && (group.showType == Group.showTypes.idle || group.activationType == Group.ActivationTypes.byColl))
            {
                continue;
            }

            _heroControlService.StopHeroes();

        }

        // Turn off all Ai nav to let all actors using nav finish all moves without probems with avoidens
        foreach (var el in _enemies)
        {
            el.navMeshAgent.enabled = false;
        }

        // Turn of var shown and save its status
        _hpUiService.HideEnemyHp();
        bool panelIsUp = _skillUiService.IsSkillPanelShowed;

        if (panelIsUp)
        {
            _skillUiService.MoveDownSkillPanel(CurrentState == BattleStates.Peace);
        }

        // Run actor
        foreach (var el in actorLCopy)
        {
            await el.Run();
        }

        if (panelIsUp && (CurrentState == BattleStates.Hero00 || CurrentState == BattleStates.Hero01 || CurrentState == BattleStates.Hero02))
        {
            _skillUiService.MoveUpSkillPanel(CurrentState == BattleStates.Peace);
        }

        // Return peace UI if peace
        if (CurrentState == BattleStates.Peace && _enemies.Count == 0 && _bosses.Count == 0)
        {
            _peaceUiService.UnfadePeaceUi();
            RoadBlockOff(true);
        }

        // Return nav
        foreach (var el in _enemies)
        {
            el.navMeshAgent.enabled = false;
        }

        await UniTask.NextFrame(token);

        foreach (var el in _enemies)
        {
            el.navMeshObstacle.enabled = true;
        }

        // Return mercs colliders state
        for (int i = 0; i < _heroParty.HeroHps.Count; i++)
        {
            if (!_heroParty.HeroHps[i].dead)
                _heroParty.HeroHps[i].GetComponent<Collider>().enabled = heroesCollState[i];
        }

        // Return control
        cursorCts.Cancel();
        cursorCts.Dispose();

        SetCursorVisibility(isCursorVisible);// Rerurn cursor;
        _heroControlService.CanControl = canControl;
        _heroControlService.UnstopHeroes();

        // Set peace camera if peace mode
        if (_enemies.Count == 0 && _actors.Count == 0 && _bosses.Count == 0 && _bigWalkers.Count == 0)
            _cameraService.RunPeaceMode();

        await UniTask.NextFrame(token);
    }

    async UniTaskVoid CursorInv(CancellationToken token)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: token);
            Cursor.visible = false;
        }
        catch (OperationCanceledException) { }
    }

    void SetCursorVisibility(bool val)
    {
        Cursor.visible = val;
    }

    bool CheckState(int heroIndex)
    {
        if (_turnUiService.TurnComplete
            || _heroParty.HeroClass[heroIndex].disabled 
            || _heroParty.HeroHps[heroIndex].dead)
            return true;
        else
            return false;
    }

    public bool CheckWin()
    {
        if (_enemies.Count == 0
            && _actors.Count == 0
            && _bigWalkers.Count == 0
            && _bosses.Count == 0)
            return true;
        else
            return false;
    }

    public bool CheckLose()
    {
        if (_heroParty.HeroHps[0].hp > 0)
            return false;
        if (_heroParty.HeroHps[0].passiveWarrorRiseTrigger)
            return false;
        if (_heroParty.HeroHps[1].hp > 0)
            return false;
        if (_heroParty.HeroHps[2].hp > 0)
            return false;
        return true;
    }

    bool CheckAIState(Ai comp)
    {
        if (!_developerSettings.IsAiDisabled
            && !comp.disabled
            && !comp.GetComponent<StunEffect>()
            && !comp.hp.dead)
            return true;
        return false;
    }

    public void TurnOffDesObjColls()
    {
        foreach (var el in _destroyableObjects)
            el.GetComponent<Collider>().enabled = false;
    }
    public void TurnOnDesObjColls()
    {
        foreach (var el in _destroyableObjects)
            el.GetComponent<Collider>().enabled = true;
    }

    public void TurnOffEnemyColls()
    {
        foreach (var el in _enemies)
            el.GetComponent<Collider>().enabled = false;
    }
    public void TurnOnEnemyColls()
    {
        foreach (var el in _enemies)
            el.GetComponent<Collider>().enabled = true;
    }

    public void AddEnemy(Ai ai)
    {
        _enemies.Add(ai);
        ai.gameObject.layer = _layerProvider.EnemyLayer;
    }
    public void AddActor(Actor a)
    {
        _actors.Add(a);
    }
    public void AddAlly(Ai ai)
    {
        _allies.Add(ai);
    }
    public void AddDestroyableObject(HP hp)
    {
        _destroyableObjects.Add(hp);
    }
    public void AddActivatedByTrigger(Actor actor)
    {
        _colliderActors.Add(actor);
    }
    public void AddBonus(GameObject item)
    {
        _bonuses.Add(item);
    }
    public void AddTurnBasedSkill(TurnCheck item)
    {
        _turnCheckList.Add(item);
    }
    public void AddBoss(Actor item)
    {
        _bosses.Add(item);
    }

    public void RemoveActivatedByTrigger(Actor actor)
    {
        _colliderActors.Remove(actor);
    }
    public void RemoveEnemy(Ai item)
    {
        _enemies.Remove(item);
    }
    public void RemoveActor(Actor item)
    {
        _actors.Remove(item);
    }
    public void RemoveTurnBasedSkill(TurnCheck item)
    {
        _turnCheckList.Remove(item);
    }
    public void RemoveBoss(Actor item)
    {
        _bosses.Remove(item);
    }

    public void ClearAllLists()
    {
        _enemies.Clear();
        _allies.Clear();
        _actors.Clear();
        _bosses.Clear();
        _bigWalkers.Clear();
        _smallEnemies.Clear();
        _destroyableObjects.Clear();
    }
    public void ClearEnemyList()
    {
        _enemies.Clear();
    }
    public void SortEnemiesByX()
    {
        _enemies.Sort((a, b) => a.distanceToTarget.sqrMagnitude.CompareTo(b.distanceToTarget.sqrMagnitude));
    }

    public void DestroyTurnBasedSkills()
    {
        foreach (var el in _turnCheckList)
        {
            Destroy(el.gameObject);
        }
        _turnCheckList.Clear();
    }

    public void TargList()
    {
        if (_developerSettings.DoNotDetectEnemies)
        {
            return;
        }

        //Do not search enemy if all heroes are hiding 
        bool allHeroesInvisible = true;
        foreach (var el in _heroParty.HeroClass)
        {
            if (el.invis <= 0)
            {
                allHeroesInvisible = false;
                break;
            }
        }

        if (allHeroesInvisible)
        {
            return;
        }

        //Check alive currentHero to get coll sphere pos from his pos
        for (int i = 0; i < _heroParty.HeroHps.Count; i++)
            if (!_heroParty.HeroHps[i].dead)
            {
                _spherePosition = new Vector3(_heroParty.HeroHps[i].transform.position.x, 0f, 9f);
                break;
            }

        int count;

        //Deoptimise
        count = Physics.OverlapSphereNonAlloc(_spherePosition, _enemyListRadius * 4, _enemySearchResults, _layerProvider.OptimisedGroupMask);
        for (int i = 0; i < count; i++)
        {
            if (!_enemySearchResults[i].CompareTag("Actor"))
            {
                _enemySearchResults[i].enabled = false;
            }
            _enemySearchResults[i].GetComponent<Group>().DeOptimize();
        }

        //Get closest unadded Actor in case group has tag Actor
        count = Physics.OverlapSphereNonAlloc(_spherePosition, _enemyListRadius, _enemySearchResults, _layerProvider.DefaultMask);
        for (int i = 0; i < count; i++)
        {
            if (!_enemySearchResults[i].CompareTag("Actor"))
            {
                continue;
            }
            AddActor(_enemySearchResults[i].GetComponent<Actor>());
            _enemySearchResults[i].GetComponent<Status>().SetActivated();
            _enemySearchResults[i].enabled = false;
            return;
        }

        //Get closest unadded mob in case group have visible mobs
        count = Physics.OverlapSphereNonAlloc(_spherePosition, _enemyListRadius, _enemySearchResults, _layerProvider.UnaddedObjectMask);

        for (int i = 0; i < count; i++)
        {
            if (_enemySearchResults[i].CompareTag("DesObj"))
            {
                continue;
            }

            AddActor(_enemySearchResults[i].GetComponentsInParent<Group>().Last());

            if (_actors.Last().TryGetComponent(out Collider c))
            {
                c.enabled = false;
            }

            return;
        }
    }
}

