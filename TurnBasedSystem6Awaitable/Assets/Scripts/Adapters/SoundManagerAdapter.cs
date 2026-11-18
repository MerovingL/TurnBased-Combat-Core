using UnityEngine;

public class SoundManagerAdapter : MonoBehaviour,
    ISoundService
{
    [SerializeField] private SoundManager _soundManager;
    public void BattleMusicSmooth()=>_soundManager.BattleMusicSmooth();
    public void PlayPeaceMusicSmooth() => _soundManager.PeaceMusicSmooth();
    public void PeaceMusicVolumeDown(bool stop) => _soundManager.peaceMusicPlayer.VolumeDown(stop);
    public void EventVolumeDown(bool stop) => _soundManager.eventMusicLongPlayer.VolumeDown(stop);
}
