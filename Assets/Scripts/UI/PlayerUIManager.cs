using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PlayerUIManager : MonoBehaviour
    {
        [SerializeField] private Image healthBar;
        [SerializeField] private TMP_Text ammoText;
    
        [SerializeField, Space] private RectTransform pickupUI;
        private readonly List<TMP_Text> _pickupTextList = new();
        [SerializeField] private TMP_Text equipText;
        [SerializeField] private TMP_Text pickupText;
        [SerializeField] private float pickupTextSize = 50f;
        [SerializeField] private RectTransform itemInteractUI;
        [SerializeField] private Image pickupHighlight;
        [SerializeField] private Color pickupHighlightAvailableColor;
        [SerializeField] private Color pickupHighlightUnavailableColor;
        private List<(bool isAvailable, ItemInteractType type)> _pickupAvailableList;
        private int _pickupTextListCount;
        private int _pickupCurrentIdx;
        [SerializeField] private GameObject noAmmoWarning;

        private void Awake()
        {
            //ItemInteract UI
            _pickupTextList.Add(equipText);
            _pickupTextList.Add(pickupText);
        }
    
        public void UpdateHealthBar(float health, float maxHealth)
        {
            healthBar.fillAmount = health / maxHealth;
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
    
        public void ShowItemPickup (Vector2 position, List<(bool, ItemInteractType)> availableList)
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
                    case ItemInteractType.Equip:
                        equipText.gameObject.SetActive(true);
                        break;
                    case ItemInteractType.PickUp:
                        pickupText.gameObject.SetActive(true);
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
    }
}
