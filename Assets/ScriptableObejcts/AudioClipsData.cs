using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioClipsData", menuName = "Scriptable Objects/AudioClipsData")]
public class AudioClipsData : ScriptableObject
{
    //개선 필요?
    [Serializable]
    private struct SFXClipMap //SFX클립 매핑
    {
        public AudioClip clip;
        public SFX sfx;
    }
    [Serializable]
    private struct BGMClipMap //BGM클립 매핑
    {
        public AudioClip clip;
        public BGM bgm;
    }
    
    [Header("BGM")] 
    [SerializeField] private BGMClipMap[] bgmClipMap; //bgm클립 매핑배열
    public Dictionary<BGM, AudioClip> BGMClipDict => BGMClipMapping(bgmClipMap); //룩업테이블 Dictionary초기화

    private static Dictionary<BGM, AudioClip> BGMClipMapping(BGMClipMap[] clipMap) //매핑 메서드
    {
        var result = new Dictionary<BGM, AudioClip>(); //Key, Value
        foreach (var map in clipMap)
        {
            result.Add(map.bgm, map.clip);
        }
        return result;
    }
    
    [Header("SFX")]
    [SerializeField] private SFXClipMap[] footstepSFXClipMap; //발소리 SFX 매핑 배열. 위 배열과 동일한 방식
    public Dictionary<SFX, AudioClip> FootstepSFXMap => SFXClipMapping(footstepSFXClipMap);
    [SerializeField] private SFXClipMap[] weaponSFXClipMap;
    public Dictionary<SFX, AudioClip> WeaponSFXMap => SFXClipMapping(weaponSFXClipMap);
    [SerializeField] private SFXClipMap[] uiSFXClipMap;
    public Dictionary<SFX,AudioClip> UISfxClipMap => SFXClipMapping(uiSFXClipMap);

    private static Dictionary<SFX, AudioClip> SFXClipMapping(SFXClipMap[] clipMap) //SFX매핑 메서드
    {
        var result = new Dictionary<SFX, AudioClip>();
        foreach (var map in clipMap)
        {
            result.Add(map.sfx, map.clip);
        }
        return result;
    }
}
