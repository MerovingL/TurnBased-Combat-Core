using Cysharp.Threading.Tasks;
using UnityEngine;
public interface ICameraService
{
    UniTask BattlePresentationZoom();
    UniTask BattlePresentationFromBackground();
    UniTask BattlePresentationFromZeroPlane();
    UniTask BattlePresentationFromZeroPlane(Transform targ = null, Vector3 offs = default(Vector3));
    UniTask HeroTurnMode(Transform mercTr = null);
    void EnemyTurnMode();
    void RunPeaceMode();
    UniTask LensFade(float targ, float du);
}
