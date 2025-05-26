using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private GameObject playerUI;
    [SerializeField] private RectTransform pickupUI;
    
    [SerializeField] private TMP_Text pickupText;
    [SerializeField] private TMP_Text equipText;
    [SerializeField] private float pickupTextSize = 50f;
    [SerializeField] private RectTransform itemInteractUI;

    [SerializeField, Space] private float slotSize = 50f;
    
    [Header("Player Inventory")]
    //[SerializeField] private RectTransform contentRT;
    //[SerializeField] private float defaultHeight = 900f;
    [Header("Left Panel")]
    [SerializeField] private RectTransform leftPanel;
    [SerializeField] private Image headwear;
    [SerializeField] private Image eyewear;
    [SerializeField] private Image bodyArmor;
    [SerializeField] private Image primaryWeapon;
    [SerializeField] private Image secondaryWeapon;
    
    [Header("Middle Panel")]
    [SerializeField] private RectTransform middlePanel;
    [SerializeField] private Image chestRig;
    [SerializeField] private RectTransform chestRigRT;
    [SerializeField] private RectTransform rigSlotParent;
    private GameObject _rigSlotInstance;
    [SerializeField] private Image backpack;
    [SerializeField] private RectTransform backpackRT;
    [SerializeField] private RectTransform backpackSlotParent;
    private GameObject _backpackSlotInstance;
    [SerializeField] private float minMiddlePanelItemHeight = 250f;
    private float _middlePanelItemPadding; //slotSize * 2
    [SerializeField, Space] private List<Image> pockets = new List<Image>();
    
    [Header("Right Panel")]
    [SerializeField] private RectTransform rightPanel;
    //[SerializeField] private RectTransform crateRT;
    [SerializeField] private RectTransform crateSlotParent;
    private GameObject _crateSlotInstance;
    
    [SerializeField, Space] private Image slotAvailable;
    //아니면 슬롯 색상 변경?
    
    private List<RectTransform> _panels = new List<RectTransform>();
    
    //test
    [SerializeField, Space] private ItemDragger test01;
    [SerializeField] private ItemDragger test02;
    
    private void Awake()
    {
        _middlePanelItemPadding = slotSize * 2;
        _panels.Add(leftPanel);
        _panels.Add(middlePanel);
        _panels.Add(rightPanel);
    }

    public void UpdateAmmoText(int currentAmmo)
    {
        ammoText.text = currentAmmo.ToString();
    }

    public void OpenPlayerUI(bool isOpen) //PlayerUI
    {
        //
        playerUI.SetActive(isOpen);
    }
    
    public void ShowItemPickup(bool show, Vector2 position)
    {
        pickupUI.gameObject.SetActive(show);
        if (show)
        {
            pickupUI.position = position;
        }
        //아이템에 따라 달라질필요있음.(장착 상태, 종류에 따라)
    }

    public void ScrollItemPickup(float y)
    {
        var uiPos = itemInteractUI.anchoredPosition;
        uiPos.y += y * 50; //50 -> 칸의 크기 
        if (uiPos.y > 0)
        {
            uiPos.y = -50;//변경될 예정(스크롤 목록의 마지막 y위치)
        }
        else if (uiPos.y < -50)
        {
            uiPos.y = 0;
        }

        itemInteractUI.anchoredPosition =  uiPos;
    }

    public void CheckRectTransform(Vector2 position)
    {
        foreach (var panel in _panels)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(panel, position))
            {
                Debug.Log("CheckRectTransform:" + panel);
            }
        }
    }
    
    public void SetRigSlot(GameObject slotPrefab)
    {
        if (_rigSlotInstance)
        {
            Destroy(_rigSlotInstance);
        }
        _rigSlotInstance = Instantiate(slotPrefab, rigSlotParent);
        
    }
    public void SetBackpackSlot(GearData backpackData)
    {
        if (_backpackSlotInstance)
        {
            Destroy(_backpackSlotInstance);
        }

        if (backpackData)
        {
            GameObject slotPrefab = backpackData.SlotPrefab;
            backpack.sprite = backpackData.ItemSprite;
            _backpackSlotInstance = Instantiate(slotPrefab, backpackSlotParent);
            Inventory inventory = _backpackSlotInstance.GetComponent<Inventory>();
            float slotPrefabHeight = inventory.Height;
                //slotPrefab.GetComponent<RectTransform>().rect.height - _middlePanelItemPadding;
            if (slotPrefabHeight > 0)
            {
                backpackRT.sizeDelta = new Vector2(backpackRT.sizeDelta.x, minMiddlePanelItemHeight + slotPrefabHeight);
            }
            else
            {
                backpackRT.sizeDelta = new Vector2(backpackRT.sizeDelta.x, minMiddlePanelItemHeight);
            }
            //if()
        }
        
    }
    
}
