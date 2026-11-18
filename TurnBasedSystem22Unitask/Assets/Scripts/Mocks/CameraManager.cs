using UnityEngine;
using Cysharp.Threading.Tasks;

public class CameraManager : MonoBehaviour
{
    public UniTask BattlePresentationZoom() => UniTask.NextFrame();
    public UniTask BattlePresentationFromBackground() => UniTask.NextFrame();
    public UniTask BattlePresentationFromZeroPlane() => UniTask.NextFrame();
    public UniTask BattlePresentationFromZeroPlane(Transform target = null, Vector3 offset = default) => UniTask.NextFrame();
    public UniTask HeroTurnMode(Transform heroTransform = null) => UniTask.NextFrame();
    public void EnemyTurnMode() { }
    public void RunPeaceMode() { }
    public UniTask LensFade(float target, float duration) => UniTask.NextFrame();
}