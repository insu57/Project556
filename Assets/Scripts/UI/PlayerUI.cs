using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField] private Image healthBar;
        [SerializeField] private Image staminaBar;
        [SerializeField] private Image energyBar;
        [SerializeField] private Image hydrationBar;
        [SerializeField] private Image fireModeImage;
        [SerializeField] private TMP_Text ammoText;
        [SerializeField] private List<FireModeImage> fireModeImageData;
        private readonly Dictionary<AmmoCategory, Dictionary<FireMode, Sprite>> _fireModeSpriteDict = new();
        [SerializeField] private GameObject noAmmoWarning;
        
        [SerializeField, Space] private TMP_Text healthTxt;
        [SerializeField] private TMP_Text staminaTxt;
        [SerializeField] private TMP_Text energyTxt;
        [SerializeField] private TMP_Text hydrationTxt;
        
        [SerializeField, Space] private RectTransform fieldInteractUI;
        private readonly List<TMP_Text> _fieldInteractTextList = new();
        [SerializeField] private TMP_Text equipText;
        [SerializeField] private TMP_Text pickupText;
        [SerializeField] private TMP_Text useText;
        [SerializeField] private TMP_Text openText;
        [SerializeField] private float fieldInteractTextSize = 50f;
        [SerializeField] private RectTransform currentInteract;
        [SerializeField] private Image interactHighlight;
        [SerializeField] private Color interactHighlightAvailableColor;
        [SerializeField] private Color interactHighlightUnavailableColor;
        private List<(bool isAvailable, InteractType type)> _interactAvailableList;
        
        private Camera _mainCamera;
        
        private void Awake()
        {
            //ItemInteract UI
            _fieldInteractTextList.Add(equipText);
            _fieldInteractTextList.Add(pickupText);
            _fieldInteractTextList.Add(useText);
            _fieldInteractTextList.Add(openText);

            foreach (var data in fireModeImageData)
            {
                _fireModeSpriteDict[data.AmmoCategory] = data.FireModeSpriteDict;
            }
            
            _mainCamera = Camera.main;
        }
    
        public void UpdateHealthUI(float health, float maxHealth)
        {
            healthBar.fillAmount = health / maxHealth;
            healthTxt.text = $"{health:F0} / {maxHealth:F0}";
        }

        public void UpdateStaminaUI(float stamina, float maxStamina)
        {
            staminaBar.fillAmount = stamina / maxStamina;
            staminaTxt.text = $"{stamina:F0} / {maxStamina:F0}";
        }

        public void UpdateEnergyUI(float energy, float maxEnergy)
        {
            energyBar.fillAmount = energy / maxEnergy;
            energyTxt.text = $"{energy:F0} / {maxEnergy:F0}";
        }

        public void UpdateHydrationUI(float hydration, float maxHydration)
        {
            hydrationBar.fillAmount = hydration / maxHydration;
            hydrationTxt.text = $"{hydration:F0} / {maxHydration:F0}";
        }
        
        public void UpdateAmmoText(bool isFullyLoaded, int ammo) 
        {
            ammoText.text = ammo.ToString();
            if (isFullyLoaded)
            {
                ammoText.text = ammo - 1 + "<size=75%>+1</size>";
            }
            else
            {
                ammoText.text = ammo.ToString();
            }
        }

        public void ShowNoAmmoWarning()
        {
            StartCoroutine(NoAmmoWarningCoroutine());
        }

        private IEnumerator NoAmmoWarningCoroutine()
        {
            noAmmoWarning.SetActive(true);
            yield return new WaitForSeconds(1f);
            noAmmoWarning.SetActive(false);
        }

        public void ToggleFireModeImage(AmmoCategory ammoCategory,FireMode fireMode)
        {
            //Pistol 제외한 다른 탄종 이미지 추가 예정
            var dict = _fireModeSpriteDict[AmmoCategory.Pistol];
            fireModeImage.sprite = dict[fireMode];
        }

        public void ShowAmmoIndicator(bool isShow)
        {
            fireModeImage.enabled = isShow;
            ammoText.enabled = isShow;
        }
    }
}
