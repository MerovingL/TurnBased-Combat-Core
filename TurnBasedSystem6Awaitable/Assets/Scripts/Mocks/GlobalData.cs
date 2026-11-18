using System.Collections.Generic;
using UnityEngine;

public class GlobalData : MonoBehaviour
{
    [Header("Debug")]
    public bool skipManual;
    public bool skipBattlePresentation;
    public bool doNotDetectEnemies;
    public bool isAiDisabled;

    [Header("Party")]
    public List<GameObject> Heroes = new List<GameObject>();
    public List<HP> heroHps = new List<HP>();
    public List<Heroes> heroClass = new List<Heroes>();


    [Header("Dependencies")]
    public HudManager hudManager;
    public RageSystem rageSystem;
    public ManualSystem manualSystem;
    public LevelSystem levelSystem;

    [Header("Layers")]
    public LayerMask enemyMask;
    public LayerMask optimisedGroupMask;
    public LayerMask defaultMask;
    public LayerMask unaddedObjectMask;
    public int enemyLayer;

    [Header("Runtime State")]
    [ReadOnly] public bool canControl;
    public void StopHeroes() { }

    public void UnStopHeroes() { }
    public Awaitable LoadGame(string profideID) => Awaitable.NextFrameAsync();
}