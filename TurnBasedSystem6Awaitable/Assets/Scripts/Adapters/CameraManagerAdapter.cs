using UnityEngine;

public class CameraManagerAdapter : MonoBehaviour,
    ICameraService
    
{
    [SerializeField] private CameraManager cameraManager;
    
    public Awaitable BattlePresentationZoom()=> cameraManager.BattlePresentationZoom();
    public Awaitable BattlePresentationFromBackground() => cameraManager.BattlePresentationFromBackground();
    public Awaitable BattlePresentationFromZeroPlane() => cameraManager.BattlePresentationFromZeroPlane();
    public Awaitable BattlePresentationFromZeroPlane(Transform targ = null, Vector3 offs = default(Vector3)) => cameraManager.BattlePresentationFromZeroPlane(targ, offs);
    public Awaitable HeroTurnMode(Transform heroTr = null) => cameraManager.HeroTurnMode(heroTr);
    public void EnemyTurnMode() => cameraManager.EnemyTurnMode();
    public void RunPeaceMode() => cameraManager.RunPeaceMode();
    public Awaitable LensFade(float targ, float dur) => cameraManager.LensFade(targ, dur);
}
