using UnityEngine;
using UnityEngine.AI;

// Enums
public enum enemyTypes
{
    small,
    medium,
    bigWalker
}

public enum battlePresentations
{
    auto,
    no,
    leftRight,
    fromZero,
    fromBg

}

// Combat
public class Ai : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public NavMeshObstacle navMeshObstacle;
    public HP hp;
    public bool disabled;
    public Vector3 distanceToTarget;
    public Awaitable Run(float offset = 0f) => Awaitable.NextFrameAsync();
    public void enableHpClass() { }
    public void TurnOffNavMeshAgent() { }
    public void TurnOffObstacleCarve() { }
    public bool CheckForState(enemyTypes type) => false;

}

public class AiMob : MonoBehaviour
{
    public bool skipOneTurn;
    public void DistToTarg() { }
}

public class Actor : MonoBehaviour
{
    public Awaitable Run() => Awaitable.NextFrameAsync();
}

public class HP : MonoBehaviour
{
    public float hp;
    public bool dead;
    public bool passiveWarrorRiseTrigger;
    public Effects effects;

    public void RemoveHp(float amount) { }
    public void ResetWarriorRisePassive() { }
    public void ReturnSizeAndHp(bool fast) { }
    public Awaitable PassiveWarriorRiseEnd() => Awaitable.NextFrameAsync();
    public Awaitable PassiveWarriorRegen() => Awaitable.NextFrameAsync();
    public Awaitable RessurectAndHeal(bool lvlUp, bool load) => Awaitable.NextFrameAsync();
}
public class Heroes : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public NavMeshObstacle obstacle;
    public int invis;
    public bool disabled;

    public void MoveToWinPosition() { }
}

public class Group : Actor
{
    public enum showTypes
    {
        idle,
        moveFromLeftCol,
        moveFromRightCol
    }

    public enum ActivationTypes
    {
        defaultType,
        byColl
    }
    public showTypes showType;
    public ActivationTypes activationType;
    public bool topGroup;
    public battlePresentations battlePresentation;
    public bool skipBattlePresentation;
    public Transform fromZeroTargetPresentaionContainer;
    public Vector3 fromZeroPresentationOffset;

    public void GetControl() { }
    public void DeOptimize() { }
}

public class Effects : MonoBehaviour
{
    public bool stuned;
    public void EndStun() { }
    public void EndBlind() { }
    public void EndPositiveEffects() { }
}

public class ShrineEffect : MonoBehaviour
{
    public Awaitable Run() => Awaitable.NextFrameAsync();
}

public class TurnCheck : MonoBehaviour
{
    public void RunTurnCheck() { }
}

public class BonusSpawn : MonoBehaviour
{
    public bool doNotSpawnBonus;
}

public class MovieStart : MonoBehaviour { }

public class Status : MonoBehaviour
{
    public void SetActivated() { }
}

public class StunEffect : MonoBehaviour { }

// System
public class MyDataPersistenceManager
{
    public static MyDataPersistenceManager instance = new MyDataPersistenceManager();
    public string currentProfileId;
}

