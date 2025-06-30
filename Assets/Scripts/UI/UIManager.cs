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
    
    [SerializeField, Space] private RectTransform pickupUI;
    private List<TMP_Text> _pickupTextList = new();
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
    
    [Header("Slot")]
    [SerializeField, Space] private float cellSize = 50f;
    private float _panelSlotPadding; //slotSize * 3
    public float CellSize => cellSize;

    [SerializeField] private Color availableColor;
    [SerializeField] private Color unavailableColor;
    
    [Header("Player Inventory")]
    [Header("Left Panel")]
    [SerializeField] private RectTransform leftPanel;
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
    
    [Header("Middle Panel")]
    [SerializeField] private RectTransform middlePanel;
    [SerializeField] private RectTransform chestRigSlot;
    [SerializeField] private RectTransform chestRigParent;
    [SerializeField] private RectTransform rigInvenParent;
    [SerializeField] private RectTransform rigItemRT;
    private GameObject _rigSlotInstance;
    [SerializeField] private RectTransform backpackSlot;
    [SerializeField] private RectTransform backpackParent;
    [SerializeField] private RectTransform packInvenParent;
    [SerializeField] private RectTransform backpackItemRT;
    private GameObject _backpackSlotInstance;
    [SerializeField] private float minMiddlePanelItemHeight = 250f;
    [SerializeField, Space] private RectTransform pocketsParent;
    [SerializeField] private List<RectTransform> pockets = new();
    [SerializeField] private RectTransform pocketsItemRT;
    
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
    [SerializeField] private ItemDragHandler itemDragHandlerPrefab;
    
    private readonly List<RectTransform> _gearSlotRT = new();
    private readonly List<RectTransform> _inventoriesRT = new();
    //public readonly Dictionary<RectTransform, RectTransform> SlotItemRT = new();
    private void Awake()
    {
        _panelSlotPadding = cellSize * 3;
        
        //RectTransform Init
        _gearSlotRT.Add(headwearSlot);
        _gearSlotRT.Add(eyewearSlot);
        _gearSlotRT.Add(bodyArmorSlot);
        _gearSlotRT.Add(primaryWeaponSlot);
        _gearSlotRT.Add(secondaryWeaponSlot);
        //SlotItemRT[headwearSlot] = leftPanelItemParentRT;
        //SlotItemRT[eyewearSlot] = leftPanelItemParentRT;
        //SlotItemRT[bodyArmorSlot] = leftPanelItemParentRT;
        //SlotItemRT[primaryWeaponSlot] = leftPanelItemParentRT;
        //SlotItemRT[secondaryWeaponSlot] =  leftPanelItemParentRT;
        
        _gearSlotRT.Add(chestRigSlot);
        _gearSlotRT.Add(backpackSlot);
        //SlotItemRT[chestRigSlot] = rigItemRT;
        //SlotItemRT[backpackSlot] = backpackSlot;
        
        foreach (var pocketRT in pockets)
        {
            _gearSlotRT.Add(pocketRT);
            //SlotItemRT[pocketRT] = pocketsItemRT;
        }
        
        _inventoriesRT.Add(rigInvenParent);
        _inventoriesRT.Add(packInvenParent);
        _inventoriesRT.Add(lootSlotParent);
        
        
        //ItemInteract UI
        _pickupTextList.Add(equipText);
        _pickupTextList.Add(pickupText);
    }

    public void UpdateAmmoText(int currentAmmo)
    {
        ammoText.text = currentAmmo.ToString();
    }

    public void OpenPlayerUI(bool isOpen) //PlayerUI
    {
        playerUI.SetActive(isOpen);
    }
    
    public void ShowItemPickup (Vector2 position, bool isGear, List<(bool, ItemInteractType)> availableList)
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
        
        foreach (var (isAvailable, type) in _pickupAvailableList) //비활성은?
        {
            switch (type)
            {
                case ItemInteractType.Equip:
                    equipText.gameObject.SetActive(isAvailable);
                    break;
                case ItemInteractType.PickUp:
                    pickupText.gameObject.SetActive(isAvailable);
                    break;
                default:
                    Debug.LogWarning("ItemInteractType Error: None.");
                    break;
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(pickupUI);
        
        pickupHighlight.color = _pickupAvailableList[0].isAvailable
            ? pickupHighlightAvailableColor : pickupHighlightUnavailableColor;
        
        //개선점? 장비하기/획득하기(Gear는 둘 다)...개선을 어떻게? List형식?
        //아이템에 따라 달라질필요있음.(장착 상태, 종류에 따라)
    }

    public void HideItemPickup()
    {
        pickupUI.gameObject.SetActive(false);
    }
    
    public void ScrollItemPickup(int idx)
    {
        var uiPos = itemInteractUI.anchoredPosition;

        uiPos.y = -idx * pickupTextSize;
        var isAvailable = _pickupAvailableList[idx];
        pickupHighlight.color = isAvailable.isAvailable ? pickupHighlightAvailableColor : pickupHighlightUnavailableColor;
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
    
    public (RectTransform matchSlot, bool isGearSlot) GetItemSlotRT(Vector2 mousePos)
    {
        foreach (var slot in _gearSlotRT)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(slot, mousePos)) continue;
            return (slot, true);
        }

        foreach (var inventory in _inventoriesRT)
        {
            if(!RectTransformUtility.RectangleContainsScreenPoint(inventory, mousePos)) continue;
            return (inventory, false);
        }
        return (null,  false);
    }

    //변형...
    public Inventory SetInventorySlot(GameObject inventoryGO, GearType itemType, Guid instanceID, bool isInit)
    {
        //초기화...

        if (!inventoryGO) return null;
        
        Inventory inventory = null;
        float slotPrefabHeight;
        
        switch (itemType)
        {
            case GearType.ArmoredRig:
            case GearType.UnarmoredRig:
                if (isInit)
                {
                    _rigSlotInstance = Instantiate(inventoryGO, rigInvenParent);
                    _rigSlotInstance.TryGetComponent(out inventory);
                    inventory.Init(CellSize, instanceID);
                }
                else
                {
                    _rigSlotInstance = inventoryGO;
                    _rigSlotInstance.TryGetComponent(out inventory);
                    _rigSlotInstance.SetActive(true);
                    _rigSlotInstance.transform.SetParent(rigInvenParent);
                }
                
                slotPrefabHeight = inventory.Height;
                    
                rigInvenParent.sizeDelta = new Vector2(inventory.Width, inventory.Height);
                if (slotPrefabHeight > minMiddlePanelItemHeight)
                {
                    chestRigParent.sizeDelta = 
                        new Vector2(chestRigParent.sizeDelta.x, slotPrefabHeight + _panelSlotPadding);
                }
                else
                {
                    chestRigParent.sizeDelta = new Vector2(chestRigParent.sizeDelta.x, minMiddlePanelItemHeight);
                }
                break;
            case GearType.Backpack:
                _backpackSlotInstance = Instantiate(inventoryGO, packInvenParent);
                _backpackSlotInstance.TryGetComponent(out inventory);
                inventory.Init(CellSize, instanceID);
                slotPrefabHeight = inventory.Height;
                    
                packInvenParent.sizeDelta = new Vector2(inventory.Width, inventory.Height);
                if (slotPrefabHeight > minMiddlePanelItemHeight)
                {
                    backpackParent.sizeDelta = 
                        new Vector2(backpackParent.sizeDelta.x, slotPrefabHeight + _panelSlotPadding);
                }
                else
                {
                    backpackParent.sizeDelta = new Vector2(backpackParent.sizeDelta.x, minMiddlePanelItemHeight);
                }
                break;
            case GearType.None:
                _lootSlotInstance = Instantiate(inventoryGO, lootSlotParent);
                _lootSlotInstance.TryGetComponent(out inventory);
                inventory.Init(CellSize, instanceID);
                slotPrefabHeight = inventory.Height + _panelSlotPadding;
                    
                lootSlotParent.sizeDelta = new Vector2(inventory.Width, inventory.Height);
                if (slotPrefabHeight > minLootSlotHeight)
                {
                    lootSlotParent.sizeDelta = new Vector2(lootSlotParent.sizeDelta.x, slotPrefabHeight);
                }
                else
                {
                    lootSlotParent.sizeDelta = new Vector2(lootSlotParent.sizeDelta.x, minLootSlotHeight);
                }
                break;
        }
        return inventory;
    }
    
    
}
