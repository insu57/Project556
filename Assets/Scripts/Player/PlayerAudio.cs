using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [SerializeField] private AudioSource oneShotAudioSource;
    [SerializeField] private AudioSource loopAudioSource;

    public void PlayOneShotSFX(SFXType type , SFX sfx)
    {
        AudioManager.Instance.PlaySFX(oneShotAudioSource, type ,sfx);
    }

    //loop....
}
