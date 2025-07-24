using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioSource bgmSource;
    
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup masterGroup;
    [SerializeField] private AudioMixerGroup bgmGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    [SerializeField] private float minVolumedB = -80f;
    [SerializeField] private float maxVolumedB = 0f;
    
    private const string MasterStr = "Master";
    private const string BGMStr = "BGM";
    private const string SFXStr = "SFX";
    //audio
    [SerializeField] private AudioClipsData audioClipsData;
    
    private Dictionary<BGM, AudioClip> _bgmMap;
    private Dictionary<SFX,AudioClip> _footstepSFXMap;
    private Dictionary<SFX, AudioClip> _weaponSFXMap;
    private Dictionary<SFX,AudioClip> _uiSfxClipMap;

    protected override void Awake()
    {
        base.Awake();
        _bgmMap = audioClipsData.BGMClipDict;
        _footstepSFXMap = audioClipsData.FootstepSFXMap;
        _weaponSFXMap = audioClipsData.WeaponSFXMap;
        _uiSfxClipMap =  audioClipsData.UISfxClipMap;
        
    }

    public void AdjustVolume(AudioType type ,float volume)
    {
        var volumedB = Mathf.Lerp(minVolumedB, maxVolumedB, volume);
        switch (type)
        {
            case AudioType.Master: audioMixer.SetFloat(MasterStr, volumedB); break;
            case AudioType.BGM: audioMixer.SetFloat(BGMStr, volumedB); break;
            case AudioType.SFX:  audioMixer.SetFloat(SFXStr, volumedB); break;
            default: return;
        }
    }
    
    public void PlayBGM(BGM bgm)
    {
        bgmSource.outputAudioMixerGroup = bgmGroup;
        bgmSource.clip = _bgmMap[bgm];
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlaySFX(AudioSource source  ,SFXType sfxType, SFX sfx) //pooling?
    {
        AudioClip clip = null;

        switch (sfxType)
        {
            case SFXType.Footstep: _footstepSFXMap.TryGetValue(sfx, out clip); break;
            case SFXType.Weapon: _weaponSFXMap.TryGetValue(sfx, out clip); break;
            case SFXType.UI: _uiSfxClipMap.TryGetValue(sfx, out clip); break;
        }

        if (!clip)
        {
            Debug.LogError($"{sfxType} not found");
            return;
        }

        //var source = ObjectPoolingManager.Instance.GetAudioSource();
        source.outputAudioMixerGroup = sfxGroup;
        source.PlayOneShot(clip);
        //StartCoroutine(ReleaseAudioSource(source, clip.length));
    }

    private IEnumerator ReleaseAudioSource(AudioSource src, float delay)
    {
        yield return new WaitForSeconds(delay);
        ObjectPoolingManager.Instance.ReleaseAudioSource(src);
    }
}
