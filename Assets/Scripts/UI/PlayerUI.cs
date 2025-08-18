using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
        
        [SerializeField, Space] private TMP_Text healthTxt;
        [SerializeField] private TMP_Text staminaTxt;
        [SerializeField] private TMP_Text energyTxt;
        [SerializeField] private TMP_Text hydrationTxt;
        
        [SerializeField, Space] private RectTransform pickupUI;
        private readonly List<TMP_Text> _pickupTextList = new();
        [SerializeField] private TMP_Text equipText;
        [SerializeField] private TMP_Text pickupText;
        [SerializeField] private TMP_Text useText;
        [SerializeField] private TMP_Text openText;
        [SerializeField] private float pickupTextSize = 50f;
        [SerializeField] private RectTransform itemInteractUI;
        [SerializeField] private Image pickupHighlight;
        [SerializeField] private Color pickupHighlightAvailableColor;
        [SerializeField] private Color pickupHighlightUnavailableColor;
        private List<(bool isAvailable, InteractType type)> _pickupAvailableList;
        private int _pickupTextListCount;
        private int _pickupCurrentIdx;
        [SerializeField] private GameObject noAmmoWarning;
        
        private void Awake()
        {
            //ItemInteract UI
            _pickupTextList.Add(equipText);
            _pickupTextList.Add(pickupText);
            _pickupTextList.Add(useText);
            _pickupTextList.Add(openText);

            foreach (var data in fireModeImageData)
            {
                _fireModeSpriteDict[data.AmmoCategory] = data.FireModeSpriteDict;
            }
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
    
        public void ShowItemPickup (Vector3 position, List<(bool, InteractType)> availableList)
        {
            //설정...아이템 따라
            pickupUI.gameObject.SetActive(true);
            itemInteractUI.anchoredPosition = Vector2.zero;

            pickupUI.position = position; //개선...WorldCanvas로 따로?
        
            _pickupAvailableList = availableList;

            foreach (var text in _pickupTextList)
            {
                text.gameObject.SetActive(false);
            }
        
            foreach (var (_, type) in _pickupAvailableList) //여기가 문제... 색만 변경
            {
                switch (type)
                {
                    case InteractType.Equip:
                        equipText.gameObject.SetActive(true);
                        break;
                    case InteractType.PickUp:
                        pickupText.gameObject.SetActive(true);
                        break;
                    case InteractType.Use:
                        useText.gameObject.SetActive(true);
                        break;
                    case InteractType.Open:
                        openText.gameObject.SetActive(true);
                        break;
                    default:
                        Debug.LogWarning("ItemInteractType Error: None.");
                        break;
                }
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(pickupUI);
        
            pickupHighlight.color = _pickupAvailableList[0].isAvailable
                ? pickupHighlightAvailableColor : pickupHighlightUnavailableColor;
        }

        public void HideItemPickup()
        {
            pickupUI.gameObject.SetActive(false); //버그?
        }
    
        public void ScrollItemPickup(int idx)
        {
            var uiPos = itemInteractUI.anchoredPosition;

            uiPos.y = -idx * pickupTextSize;
            var isAvailable = _pickupAvailableList[idx];
            pickupHighlight.color = isAvailable.isAvailable ? pickupHighlightAvailableColor : pickupHighlightUnavailableColor;
            itemInteractUI.anchoredPosition =  uiPos;
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
