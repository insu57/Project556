using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioMixer audioMixer;
    //audio
    [SerializeField] private AudioClipsData audioClipsData;
}
