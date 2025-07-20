using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioClipsData", menuName = "Scriptable Objects/AudioClipsData")]
public class AudioClipsData : ScriptableObject
{
    //개선 필요...
    
    [Serializable]
    private struct SFXClipMap
    {
        public AudioClip clip;
        public SFX sfx;
    }
    [Serializable]
    private struct BGMClipMap
    {
        public AudioClip clip;
        public BGM bgm;
    }
    
    [Header("BGM")] 
    [SerializeField] private BGMClipMap[] bgmClipMap;
    public Dictionary<BGM, AudioClip> BGMClipDict => BGMClipMapping(bgmClipMap);

    private static Dictionary<BGM, AudioClip> BGMClipMapping(BGMClipMap[] clipMap)
    {
        var result = new Dictionary<BGM, AudioClip>();
        foreach (var map in clipMap)
        {
            result.Add(map.bgm, map.clip);
        }
        return result;
    }
    
    [Header("SFX")]
    [SerializeField] private SFXClipMap[] sfxClipMap;
    public Dictionary<SFX, AudioClip> SFXClips => SFXClipMapping(sfxClipMap);

    private static Dictionary<SFX, AudioClip> SFXClipMapping(SFXClipMap[] clipMap)
    {
        var result = new Dictionary<SFX, AudioClip>();
        foreach (var map in clipMap)
        {
            result.Add(map.sfx, map.clip);
        }
        return result;
    }
}
