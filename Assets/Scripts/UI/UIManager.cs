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

    [Header("Slot")]
    [SerializeField, Space] private float slotSize = 50f;
    private float _panelSlotPadding; //slotSize * 3
    public float SlotSize => slotSize;
    public Vector2 GearSlotSize => new Vector2(slotSize, slotSize) * 2;
    public Vector2 WeaponSlotSize => new Vector2(slotSize * 4, slotSize) * 2;
    [SerializeField] private Color availableColor;
    [SerializeField] private Color unavailableColor;
    
    [Header("Player Inventory")]
    [Header("Left Panel")]
    [SerializeField] private RectTransform leftPanel;
    //[SerializeField] private Inventory leftInventory;
    [SerializeField] private RectTransform headwearSlot;
    [SerializeField] private RectTransform eyewearSlot;
    [SerializeField] private RectTransform bodyArmorSlot;
    [SerializeField] private RectTransform primaryWeaponSlot;
    [SerializeField] private RectTransform secondaryWeaponSlot;
    [SerializeField] private RectTransform leftPanelItemParentRT;
    public RectTransform HeadwearSlotRT => headwearSlot;
    public RectTransform EyewearSlotRT => eyewearSlot;
    public RectTransform BodyArmorSlotRT => bodyArmorSlot;
    public RectTransform PWeaponSlotRT => primaryWeaponSlot;
    public RectTransform SWeaponSlotRT => secondaryWeaponSlot;
    public RectTransform LeftPanelItemParentRT => leftPanelItemParentRT;
    
    [Header("Middle Panel")]
    [SerializeField] private RectTransform middlePanel;
    //[SerializeField] private Inventory midInventory;
    [SerializeField] private RectTransform chestRigSlot;
    [SerializeField] private RectTransform chestRigParent;
    [SerializeField] private RectTransform rigInvenParent;
    private GameObject _rigSlotInstance;
    [SerializeField] private RectTransform backpackSlot;
    [SerializeField] private RectTransform backpackParent;
    [SerializeField] private RectTransform packInvenParent;
    private GameObject _backpackSlotInstance;
    [SerializeField] private float minMiddlePanelItemHeight = 250f;
    [SerializeField, Space] private RectTransform pocketsParent;
    [SerializeField] private List<RectTransform> pockets = new();
    [SerializeField] private RectTransform midPanelItemRT;
    
    public RectTransform RigSlotRT => chestRigSlot;
    public RectTransform BackpackSlotRT => backpackSlot;
    public List<RectTransform> PocketsSlotRT => pockets;
    public RectTransform RigInvenParent => rigInvenParent;
    public RectTransform BackpackInvenParent => packInvenParent;
    
    [Header("Right Panel")]
    [SerializeField] private RectTransform rightPanel;
    [SerializeField] private RectTransform lootSlotParent;
    [SerializeField] private float minLootSlotHeight = 800f;
    public RectTransform LootSlotParent => lootSlotParent;
    
    private GameObject _lootSlotInstance;
    
    [SerializeField, Space] private Image slotAvailable;
    [FormerlySerializedAs("itemDraggerPrefab")] [SerializeField] private ItemDragHandler itemDragHandlerPrefab;
    private ItemDragHandler _currentItemDragHandler;
    //아니면 슬롯 색상 변경?
    
    public event Action<RectTransform, RectTransform, Vector2, Guid> OnCheckGearSlot;
    public event Action<RectTransform, RectTransform, Vector2, Guid> OnCheckInventoryCell;
    private readonly List<RectTransform> _panelsRT = new List<RectTransform>(); //패널
    private readonly List<RectTransform> _gearSlotRT = new List<RectTransform>();
    private readonly List<RectTransform> _inventoriesRT = new List<RectTransform>();
    
    //test
    [SerializeField, Space] private ItemDragHandler test01;
    [SerializeField] private ItemDragHandler test02;
    //[SerializeField] private BaseItemDataSO test01Data;
    
    private void Awake()
    {
        _panelSlotPadding = slotSize * 3;
        
        _panelsRT.Add(leftPanel);
        _panelsRT.Add(middlePanel);
        _panelsRT.Add(rightPanel);
        
        _gearSlotRT.Add(headwearSlot);
        _gearSlotRT.Add(eyewearSlot);
        _gearSlotRT.Add(bodyArmorSlot);
        _gearSlotRT.Add(primaryWeaponSlot);
        _gearSlotRT.Add(secondaryWeaponSlot);
        _gearSlotRT.Add(chestRigSlot);
        _gearSlotRT.Add(backpackSlot);
        foreach (var pocketRT in pockets)
        {
            _gearSlotRT.Add(pocketRT);
        }
        
        _inventoriesRT.Add(rigInvenParent);
        _inventoriesRT.Add(packInvenParent);
        _inventoriesRT.Add(lootSlotParent);
    }

    public ItemDragHandler InitItemDragger(InventoryItem item, RectTransform itemParentRT, RectTransform inventoryRT)
    {
        var itemDragger = Instantiate(itemDragHandlerPrefab, itemParentRT);
        //itemDragger.Init(item, this, itemParentRT, inventoryRT, transform);
        return itemDragger;
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

    public void ShowSlotAvailable(bool isAvailable, Vector2 position, Vector2 size)
    {
        var slotColor = isAvailable ? availableColor : unavailableColor;
        slotAvailable.enabled = true;
        slotAvailable.color = slotColor;
        slotAvailable.rectTransform.position = position;
        slotAvailable.rectTransform.sizeDelta = size;
    }

    public void ClearShowAvailable()
    {
        slotAvailable.enabled = false;
    }
    
    
    public void MoveCurrentItemDragger(Vector2 position, RectTransform itemParentRT, RectTransform inventoryRT)
    {
        _currentItemDragHandler.SetTargetPosItemDragger(position, itemParentRT, inventoryRT, true);
    }

    public RectTransform GetItemInventoryRT(Vector2 originPos)
    {
        foreach (var inventory in _inventoriesRT)
        {
            if(!RectTransformUtility.RectangleContainsScreenPoint(inventory, originPos)) continue;
            var matchSlot = inventory;
            return matchSlot;
        }
        //Inventory->ItemDict(Inventories) or InventoryManager -> ItemDict(GearSlot...)
        return null;
    }
    
    public (RectTransform matchSlot, bool isGearSlot) CheckItemSlot(Vector2 mousePos)
    {
        //RectTransform matchSlot;
        //_currentItemDragHandler = draggingItem;

        //var originInvenRT = draggingItem.InventoryRT;
        foreach (var slot in _gearSlotRT)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(slot, mousePos)) continue;
            //matchSlot = slot;
            //OnCheckGearSlot?.Invoke(matchSlot, originInvenRT, mousePos, id); 
            return (slot, true);
        }

        foreach (var inventory in _inventoriesRT)
        {
            if(!RectTransformUtility.RectangleContainsScreenPoint(inventory, mousePos)) continue;
            //      Debug.Log(inventory.name);
            //matchSlot = inventory;
            //OnCheckInventoryCell?.Invoke(matchSlot,originInvenRT, mousePos, id); 
            return (inventory, false); //(inventory, matchSlot)
        }
        return (null,  false);
    }
    
    public Inventory SetRigSlot(GameObject rigInvenPrefab)
    {
        if (_rigSlotInstance)
        {
            Destroy(_rigSlotInstance);
        }

        if (rigInvenPrefab)
        {
            //Debug.Log("SetRigSlot");
            _rigSlotInstance = Instantiate(rigInvenPrefab, rigInvenParent);
            Inventory inventory = _rigSlotInstance.GetComponent<Inventory>(); //다른방식?
            float slotPrefabHeight = inventory.Height;
            rigInvenParent.sizeDelta = new Vector2(inventory.Width, inventory.Height);
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
    public Inventory SetBackpackSlot(GameObject backpackInvenPrefab)
    {
        if (_backpackSlotInstance)
        {
            Destroy(_backpackSlotInstance);
        }

        if (backpackInvenPrefab)
        {
            _backpackSlotInstance = Instantiate(backpackInvenPrefab, packInvenParent);
            Inventory inventory = _backpackSlotInstance.GetComponent<Inventory>();
            float slotPrefabHeight = inventory.Height;
            packInvenParent.sizeDelta = new Vector2(inventory.Width, inventory.Height);
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
            lootSlotParent.sizeDelta = new Vector2(inventory.Width, inventory.Height);
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
