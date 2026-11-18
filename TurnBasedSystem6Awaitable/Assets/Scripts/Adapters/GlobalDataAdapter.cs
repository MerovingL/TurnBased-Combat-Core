using System.Collections.Generic;
using UnityEngine;

public class GlobalDataAdapter : MonoBehaviour,
    IDeveloperSettings,
    IHeroControlService,
    IHeroParty,
    IManualService,
    ISaveLoadSystem,
    ILevelSystem,
    IRageSystem,
    ILayerProvider
    
{
    [SerializeField] private GlobalData _globalData;

    // IDeveloperSettings
    public bool SkipManual => _globalData.skipManual;
    public bool SkipBattlePresentation => _globalData.skipBattlePresentation;
    public bool DoNotDetectEnemies => _globalData.doNotDetectEnemies;
    public bool IsAiDisabled => _globalData.isAiDisabled;

    // Hero control
    public bool CanControl
    {
        get => _globalData.canControl; 
        set => _globalData.canControl = value;
    }
    public void SetControl(bool enabled) => _globalData.canControl = enabled;
    public void StopHeroes() => _globalData.StopHeroes();
    public void UnstopHeroes() => _globalData.UnStopHeroes();

    // IHeroParty
    public IReadOnlyList<GameObject> Heroes => _globalData.Heroes;
    public IReadOnlyList<HP> HeroHps => _globalData.heroHps;
    public IReadOnlyList<Heroes> HeroClass => _globalData.heroClass;

    // IManualService
    public void RunManual() => _globalData.manualSystem.Run();
    public void RunBaseAttackManual() => _globalData.manualSystem.RunBaseAttack();
    public bool MoveByMouseManualState => _globalData.manualSystem.moveByMouseManualState;


    // ISaveLoadSystem
    public Awaitable LoadGame() => _globalData.LoadGame(MyDataPersistenceManager.instance.currentProfileId);

    // ILevelSystem
    public bool WasPassivePanelOpened => !_globalData.levelSystem.passivePanelWasNotOpend;
    public bool WasPassivePanelEdited => _globalData.levelSystem.passivePanelIsEdited;
    public void SetInteractablePassives(bool val) => _globalData.hudManager.SetInteractablePassives(val);

    // IRageSystem
    public void Reset() => _globalData.rageSystem.Reset();
    public void TurnOffSkulls() => _globalData.rageSystem.TurnOffSkulls();

    // ILayerProvider
    public LayerMask EnemyMask => _globalData.enemyMask;
    public LayerMask OptimisedGroupMask => _globalData.optimisedGroupMask;
    public LayerMask DefaultMask => _globalData.defaultMask;
    public LayerMask UnaddedObjectMask => _globalData.unaddedObjectMask;
    public int EnemyLayer => _globalData.enemyLayer;
}
