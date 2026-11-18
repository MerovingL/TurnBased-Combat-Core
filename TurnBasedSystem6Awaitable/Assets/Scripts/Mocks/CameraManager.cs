using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Awaitable BattlePresentationZoom() => Awaitable.NextFrameAsync();
    public Awaitable BattlePresentationFromBackground() => Awaitable.NextFrameAsync();
    public Awaitable BattlePresentationFromZeroPlane() => Awaitable.NextFrameAsync();
    public Awaitable BattlePresentationFromZeroPlane(Transform target = null, Vector3 offset = default) => Awaitable.NextFrameAsync();
    public Awaitable HeroTurnMode(Transform heroTransform = null) => Awaitable.NextFrameAsync();
    public void EnemyTurnMode() { }
    public void RunPeaceMode() { }
    public Awaitable LensFade(float target, float duration) => Awaitable.NextFrameAsync();
}