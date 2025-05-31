using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
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
    private float _panelSlotPadding; //slotSize * 3
    
    [Header("Player Inventory")]
    //[SerializeField] private RectTransform contentRT;
    //[SerializeField] private float defaultHeight = 900f;
    [Header("Left Panel")]
    [SerializeField] private RectTransform leftPanel;
    [SerializeField] private Inventory leftInventory;
    [SerializeField] private RectTransform headwearSlot;
    [SerializeField] private RectTransform eyewearSlot;
    [SerializeField] private RectTransform bodyArmorSlot;
    [SerializeField] private RectTransform primaryWeaponSlot;
    [SerializeField] private RectTransform secondaryWeaponSlot;
    
    public RectTransform HeadwearRT => headwearSlot;
    public RectTransform EyewearRT => eyewearSlot;
    public RectTransform BodyArmorRT => bodyArmorSlot;
    public RectTransform PWeaponRT => primaryWeaponSlot;
    public RectTransform SWeaponRT => secondaryWeaponSlot;
    
    [Header("Middle Panel")]
    [SerializeField] private RectTransform middlePanel;
    [SerializeField] private Inventory midInventory;
    [SerializeField] private RectTransform chestRigSlot;
    [SerializeField] private RectTransform chestRigParent;
    [SerializeField] private RectTransform rigInvenParent;
    private GameObject _rigSlotInstance;
    [SerializeField] private RectTransform backpackSlot;
    [SerializeField] private RectTransform backpackParent;
    [SerializeField] private RectTransform packInvenParent;
    private GameObject _backpackSlotInstance;
    [SerializeField] private float minMiddlePanelItemHeight = 250f;
    [FormerlySerializedAs("pocketsRT")] [SerializeField, Space] private RectTransform pocketsParent;
    [SerializeField] private List<RectTransform> pockets = new List<RectTransform>();
    
    public RectTransform RigRT => chestRigSlot;
    public RectTransform BackpackRT => backpackSlot;
    public List<RectTransform> PocketsRT => pockets;
    
    [Header("Right Panel")]
    [SerializeField] private RectTransform rightPanel;
    //[SerializeField] private RectTransform lootSlotRT;\
    [SerializeField] private RectTransform lootSlotParent;
    [SerializeField] private float minLootSlotHeight = 800f;
    
    
    private GameObject _lootSlotInstance;
    
    [SerializeField, Space] private Image slotAvailable;
    //아니면 슬롯 색상 변경?
    
    private List<RectTransform> _panels = new List<RectTransform>(); //패널
    private List<RectTransform> _leftSlots = new List<RectTransform>();
    private List<RectTransform> _midSlots = new List<RectTransform>();
    
    //test
    [SerializeField, Space] private ItemDragger test01;
    [SerializeField] private ItemDragger test02;
    //[SerializeField] private BaseItemDataSO test01Data;
    
    private void Awake()
    {
        _panelSlotPadding = slotSize * 3;
        
        //수동추가?(번거롭고 실수가능) list로 직렬화?(구분안됨) 자식객체로?
        //수동 -> 하나 추가/제거 할 때 문제... 인벤(데이터) UI 둘 다 수정 -> 간단하긴함
        //panel RT -> panel 자식인 인벤토리 구분 -> 각 슬롯
        _panels.Add(leftPanel);
        _panels.Add(middlePanel);
        _panels.Add(rightPanel);
        
        _leftSlots.Add(headwearSlot);
        _leftSlots.Add(eyewearSlot);
        _leftSlots.Add(bodyArmorSlot);
        _leftSlots.Add(primaryWeaponSlot);
        _leftSlots.Add(secondaryWeaponSlot);
        
        _midSlots.Add(chestRigParent);
        _midSlots.Add(pocketsParent);
        _midSlots.Add(backpackParent);
        
        
        
        //test ItemDragger -> 초기화는 어떻게...?
        //test01.Init(test01Data, );
    }

    public void InitLeftSlots()
    {
        
    }

    public void InitItemDragger()
    {
        
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
        RectTransform matchPanel = null;
        foreach (var panel in _panels)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(panel, position))
            {
                Debug.Log("CheckRectTransform:" + panel);
                matchPanel = panel;
            }
        }

        if (!matchPanel) return;
        
        if (matchPanel == leftPanel)
        {
            foreach (var slot in _leftSlots)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(slot, position))
                {
                    Debug.Log("CheckSlot LeftPanel:" + slot);
                }
            }
        }
        else if (matchPanel == middlePanel)
        {
            foreach (var slot in _midSlots)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(slot, position))
                {
                    Debug.Log("CheckSlot MidPanel:" + slot);
                }
            }  
        }
        else if (matchPanel == rightPanel)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(lootSlotParent, position))
            {
                Debug.Log("CheckSlot RightPanel:" + lootSlotParent);
            }
        }
    }
    
    public Inventory SetRigSlot(GearData rigData)
    {
        if (_rigSlotInstance)
        {
            Destroy(_rigSlotInstance);
        }

        if (rigData)
        {
            GameObject slotPrefab = rigData.SlotPrefab;
            //chestRigImage.sprite = rigData.ItemSprite;
            _rigSlotInstance = Instantiate(slotPrefab, rigInvenParent);
            Inventory inventory = _rigSlotInstance.GetComponent<Inventory>(); //다른방식?
            float slotPrefabHeight = inventory.Height;
            if (slotPrefabHeight > minMiddlePanelItemHeight)
            {
                chestRigParent.sizeDelta = new Vector2(chestRigParent.sizeDelta.x, slotPrefabHeight + _panelSlotPadding);
            }
            else
            {
                chestRigParent.sizeDelta = new Vector2(chestRigParent.sizeDelta.x, minMiddlePanelItemHeight);
            }

            return inventory;
        }

        return null;

    }
    public Inventory SetBackpackSlot(GearData backpackData)
    {
        if (_backpackSlotInstance)
        {
            Destroy(_backpackSlotInstance);
        }

        if (backpackData)
        {
            GameObject slotPrefab = backpackData.SlotPrefab;
            //backpackImage.sprite = backpackData.ItemSprite;
            _backpackSlotInstance = Instantiate(slotPrefab, packInvenParent);
            //높이체크..?
            Inventory inventory = _backpackSlotInstance.GetComponent<Inventory>();
            float slotPrefabHeight = inventory.Height;
                //slotPrefab.GetComponent<RectTransform>().rect.height - _middlePanelItemPadding;
            if (slotPrefabHeight > minMiddlePanelItemHeight)
            {
                backpackParent.sizeDelta = new Vector2(backpackParent.sizeDelta.x, slotPrefabHeight + _panelSlotPadding);
            }
            else
            {
                backpackParent.sizeDelta = new Vector2(backpackParent.sizeDelta.x, minMiddlePanelItemHeight);
            }
            //if()
            
            return inventory;
        }
        return null;
    }

    public Inventory SetLootSlot(GameObject lootInventoryPrefab)
    {
        if (_lootSlotInstance)
        {
            Destroy(_lootSlotInstance); //오브젝트 풀링처럼 관리? (한 스테이지에서는 루팅 인벤토리관련 메모리에?)
        }

        if (lootInventoryPrefab)
        {
            _lootSlotInstance = Instantiate(lootInventoryPrefab, lootSlotParent);
            Inventory inventory = _lootSlotInstance.GetComponent<Inventory>();
            float slotPrefabHeight = inventory.Height + _panelSlotPadding;
            if (slotPrefabHeight > minLootSlotHeight)
            {
                lootSlotParent.sizeDelta = new Vector2(lootSlotParent.sizeDelta.x, slotPrefabHeight);
            }
            else
            {
                lootSlotParent.sizeDelta = new Vector2(lootSlotParent.sizeDelta.x, minLootSlotHeight);
            }
            return inventory;
        }
        return null;
    }
    
}
