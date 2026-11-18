using UnityEngine;

public interface ICameraService
{
    Awaitable BattlePresentationZoom();
    Awaitable BattlePresentationFromBackground();
    Awaitable BattlePresentationFromZeroPlane();
    Awaitable BattlePresentationFromZeroPlane(Transform targ = null, Vector3 offs = default(Vector3));
    Awaitable HeroTurnMode(Transform mercTr = null);
    void EnemyTurnMode();
    void RunPeaceMode();
    Awaitable LensFade(float targ, float du);
}
