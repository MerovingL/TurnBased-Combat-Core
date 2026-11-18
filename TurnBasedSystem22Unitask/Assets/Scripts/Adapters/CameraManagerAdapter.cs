using UnityEngine;
using Cysharp.Threading.Tasks;
public class CameraManagerAdapter : MonoBehaviour,
    ICameraService
{
    [SerializeField] private CameraManager cameraManager;

    public UniTask BattlePresentationZoom()=> cameraManager.BattlePresentationZoom();
    public UniTask BattlePresentationFromBackground() => cameraManager.BattlePresentationFromBackground();
    public UniTask BattlePresentationFromZeroPlane() => cameraManager.BattlePresentationFromZeroPlane();
    public UniTask BattlePresentationFromZeroPlane(Transform targ = null, Vector3 offs = default(Vector3)) => cameraManager.BattlePresentationFromZeroPlane(targ, offs);
    public UniTask HeroTurnMode(Transform heroTr = null) => cameraManager.HeroTurnMode(heroTr);
    public void EnemyTurnMode() => cameraManager.EnemyTurnMode();
    public void RunPeaceMode() => cameraManager.RunPeaceMode();
    public UniTask LensFade(float targ, float dur) => cameraManager.LensFade(targ, dur);
}
