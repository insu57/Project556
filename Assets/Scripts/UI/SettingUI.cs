using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SettingUI : MonoBehaviour
    {
        [SerializeField] private GameObject settingUI;
        public bool SettingUIOpen => settingUI.activeSelf;
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
}
