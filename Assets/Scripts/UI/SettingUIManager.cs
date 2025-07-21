using UnityEngine;
using UnityEngine.UI;

public class SettingUIManager : MonoBehaviour
{
    [SerializeField] private GameObject settingUI;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    public void OpenSettingUI(bool isOpen)
    {
        settingUI.SetActive(isOpen);
    }

    private void OnEnable()
    {
        masterVolumeSlider.onValueChanged
            .AddListener(value => AudioManager.Instance.AdjustVolume(AudioType.Master, value));
        bgmVolumeSlider.onValueChanged
            .AddListener(value => AudioManager.Instance.AdjustVolume(AudioType.BGM, value));
        sfxVolumeSlider.onValueChanged
            .AddListener(value => AudioManager.Instance.AdjustVolume(AudioType.SFX, value));
    }

    private void OnDisable()
    {
        masterVolumeSlider.onValueChanged
            .RemoveListener(value => AudioManager.Instance.AdjustVolume(AudioType.Master, value));
        bgmVolumeSlider.onValueChanged
            .RemoveListener(value => AudioManager.Instance.AdjustVolume(AudioType.BGM, value));
        sfxVolumeSlider.onValueChanged
            .RemoveListener(value => AudioManager.Instance.AdjustVolume(AudioType.SFX, value));
    }
}
