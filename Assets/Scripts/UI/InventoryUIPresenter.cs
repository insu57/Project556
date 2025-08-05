using System;
using System.Collections.Generic;
using Item;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class InventoryUIPresenter : MonoBehaviour
{
    private InventoryManager _inventoryManager;
    private ItemUI _itemUI;
    private UIControl _uiControl;

    private readonly Dictionary<RectTransform, CellData> _gearSlotsMap = new();
    private readonly Dictionary<CellData, RectTransform> _gearRTMap = new();
    private readonly Dictionary<RectTransform, Inventory> _invenMap = new();
    private readonly Dictionary<Guid, ItemDragHandler> _itemDragHandlers = new();

    public static float CellSize => GameManager.Instance.CellSize;
    //ItemDrag
    private ItemInstance _currentDragItem; //현재 드래그 중인 아이템
    private bool _rotatedOnClick; //클릭 시 회전 상태
    private bool _targetIsAvailable; //타겟 슬롯(Cell)이 유효한지?
    private RectTransform _matchRT; //해당 Match Slot의 RectTransform
    private CellData _targetGearSlot; //타겟인 GearSlot CellData
    private Inventory _targetInventory; //타겟인 Inventory
    private RectTransform _targetSlotRT; //타겟 Inventory의 SlotRT
    private int _targetFirstIdx; //타겟의 Cell Idx(아이템 좌상단, 첫번째 인덱스)
    private Guid _targetCellItemID; //타겟인 Cell의 아이템ID(Stack아이템 용)
    //ItemContextMenu
    private ItemInstance _currentCotextMenuItem;
    
    //test
    [SerializeField, Space] private GameObject crate01Test;
    [SerializeField] private BaseItemDataSO pistolTestData;
    [SerializeField] private BaseItemDataSO itemDataTest;
    [SerializeField] private BaseItemDataSO backpackTestData;
    [SerializeField] private BaseItemDataSO rigTestData;
    [SerializeField] private BaseItemDataSO bullet556TestData;
    [SerializeField] private BaseItemDataSO rigTanTestData;
    
    private void Awake()
    {
        TryGetComponent(out _inventoryManager);
        _itemUI = FindFirstObjectByType<ItemUI>();
        _itemUI.TryGetComponent(out _uiControl); //개선필요
    }

    private void Start()
    {
        //Left Panel Init
        _gearSlotsMap[_itemUI.HeadwearSlotRT] = _inventoryManager.HeadwearSlot;
        _gearSlotsMap[_itemUI.EyewearSlotRT] = _inventoryManager.EyewearSlot;
        _gearSlotsMap[_itemUI.BodyArmorSlotRT] = _inventoryManager.BodyArmorSlot;
        _gearSlotsMap[_itemUI.PWeaponSlotRT] = _inventoryManager.PrimaryWeaponSlot;
        _gearSlotsMap[_itemUI.SWeaponSlotRT] = _inventoryManager.SecondaryWeaponSlot;
        
        _gearRTMap[_inventoryManager.HeadwearSlot] = _itemUI.HeadwearSlotRT;
        _gearRTMap[_inventoryManager.EyewearSlot] = _itemUI.EyewearSlotRT;
        _gearRTMap[_inventoryManager.BodyArmorSlot] = _itemUI.BodyArmorSlotRT;
        _gearRTMap[_inventoryManager.PrimaryWeaponSlot] = _itemUI.PWeaponSlotRT;
        _gearRTMap[_inventoryManager.SecondaryWeaponSlot] = _itemUI.SWeaponSlotRT;

        //Mid Panel Init
        _gearSlotsMap[_itemUI.RigSlotRT] = _inventoryManager.ChestRigSlot;
        _gearSlotsMap[_itemUI.BackpackSlotRT] = _inventoryManager.BackpackSlot;
        _gearRTMap[_inventoryManager.ChestRigSlot] = _itemUI.RigSlotRT;
        _gearRTMap[_inventoryManager.BackpackSlot] = _itemUI.BackpackSlotRT;
        for (int i = 0; i < 4; i++)
        {
            _gearSlotsMap[_itemUI.PocketsSlotRT[i]] = _inventoryManager.PocketSlots[i];
            _gearRTMap[_inventoryManager.PocketSlots[i]] = _itemUI.PocketsSlotRT[i];
        }

        //Inventory 추가 **Inventory 초기 상태는 null!
        _invenMap[_itemUI.RigInvenParent] = null;
        _invenMap[_itemUI.BackpackInvenParent] = null;
        _invenMap[_itemUI.LootSlotParent] = null;
        
        //test
        HandleOnInitInventory(crate01Test, null); //Loot
        SetItemToInventory(pistolTestData);
        SetItemToInventory(itemDataTest);
        SetStackableItem(bullet556TestData, 50);
        SetStackableItem(bullet556TestData, 10);
        SetGearItem(backpackTestData, _itemUI.BackpackSlotRT);
        SetGearItem(rigTestData, _itemUI.RigSlotRT);
        SetItemToInventory(rigTanTestData);
    }

    private void OnEnable()
    {
        //Event
        //InventoryManager
        _inventoryManager.OnInitInventory += HandleOnInitInventory;
        _inventoryManager.OnShowInventory += HandleOnShowInventory;
        _inventoryManager.OnSetLootInventory += HandleOnSetLootInventory;
        _inventoryManager.OnEquipFieldItem += HandleOnEquipFieldItem;
        _inventoryManager.OnAddFieldItemToInventory += HandleOnAddFieldItemToInventory;
        _inventoryManager.OnUpdateItemStack += HandleOnUpdateItemStack;
        _inventoryManager.OnUpdateWeaponItemMagCount += HandleOnUpdateWeaponItemMagCount;
        _inventoryManager.OnRemoveItemFromPlayer += HandleOnRemoveItemHandler;
        _inventoryManager.OnRemoveQuickSlotItem += HandleOnRemoveQuickSlotItem;
        
        //ItemUIManager
        _itemUI.OnItemContextMenuClick += HandleOnItemContextMenuClick;
        _itemUI.OnCloseItemContextMenu += HandleOnCloseItemContextMenu;
        
        //UIControl
        _uiControl.OnCloseItemUI += HandleOnCloseItemUI;
    }
    
    private void OnDisable()
    {
        //Event unsubscribe
        //InventoryManager
        _inventoryManager.OnInitInventory -= HandleOnInitInventory;
        _inventoryManager.OnShowInventory -= HandleOnShowInventory;
        _inventoryManager.OnSetLootInventory -= HandleOnSetLootInventory;
        _inventoryManager.OnEquipFieldItem -= HandleOnEquipFieldItem;
        _inventoryManager.OnAddFieldItemToInventory -= HandleOnAddFieldItemToInventory;
        _inventoryManager.OnUpdateItemStack -= HandleOnUpdateItemStack;
        _inventoryManager.OnUpdateWeaponItemMagCount -= HandleOnUpdateWeaponItemMagCount;
        _inventoryManager.OnRemoveItemFromPlayer -= HandleOnRemoveItemHandler;
        _inventoryManager.OnRemoveQuickSlotItem -= HandleOnRemoveQuickSlotItem;
        
        //ItemUIManager
        _itemUI.OnItemContextMenuClick -= HandleOnItemContextMenuClick;
        _itemUI.OnCloseItemContextMenu -= HandleOnCloseItemContextMenu;
        
        //UIControl
        _uiControl.OnCloseItemUI -= HandleOnCloseItemUI;
    }

    public void InitItemDragHandler(ItemDragHandler itemDrag) //아이템 줍기 등에서 생성...맵에서 상자열 때 생성...
    {
        itemDrag.OnPointerDownEvent += HandleOnPointerEnter;
        itemDrag.OnDragEvent += HandleOnDragItem;
        itemDrag.OnEndDragEvent += HandleOnEndDragItem;
        itemDrag.OnRotateItem += HandleOnRotateItem;
        itemDrag.OnQuickAddItem += HandleOnQuickAddItem;
        itemDrag.OnQuickDropItem += HandleOnQuickDropItem;
        itemDrag.OnSetQuickSlot += HandleOnSetQuickSlot;
        itemDrag.OnOpenItemContextMenu += HandleOnOpenItemContextMenu;
        itemDrag.OnShowItemInfo += HandleOnShowItemInfo;
    }

    public void OnDisableItemDragHandler(ItemDragHandler itemDrag)
    {
        itemDrag.OnPointerDownEvent -= HandleOnPointerEnter;
        itemDrag.OnDragEvent -= HandleOnDragItem;
        itemDrag.OnEndDragEvent -= HandleOnEndDragItem;
        itemDrag.OnRotateItem -= HandleOnRotateItem;
        itemDrag.OnQuickAddItem -= HandleOnQuickAddItem;
        itemDrag.OnQuickDropItem -= HandleOnQuickDropItem;
        itemDrag.OnSetQuickSlot -= HandleOnSetQuickSlot;
        itemDrag.OnOpenItemContextMenu -= HandleOnOpenItemContextMenu;
        itemDrag.OnShowItemInfo -= HandleOnShowItemInfo;
    }
    
    //OnPointerEnter -> 해당 아이템, 기존 슬롯 정보 캐싱
    //OnDrag -> 해당 슬롯 정보 캐싱
    //OnEndDrag -> 이동/원복 처리
    private void HandleOnPointerEnter(ItemDragHandler itemDrag)
    {
        var originInvenRT = itemDrag.InventoryRT;
        var instanceID = itemDrag.InstanceID;
        
        ItemInstance item;

        if (!originInvenRT) //기존 위치 인벤토리 체크
        {
            if (!_inventoryManager.ItemDict.TryGetValue(instanceID, out var inventoryItem))
            {
                Debug.LogError("Item not found...!: " + instanceID);
                return;
            }

            item = inventoryItem.item; //GearSlots Item Dict
        }
        else
        {
            if (!_invenMap.TryGetValue(originInvenRT, out var inventory))
            {
                Debug.LogError("Inventory not found...!: " + instanceID);
                return;
            }

            if (!inventory.ItemDict.TryGetValue(instanceID, out var inventoryItem))
            {
                Debug.LogError($"Inventory:{inventory} - Item not found...!: " + instanceID);
                return;
            }
            item = inventory.ItemDict[instanceID].item; //Inventory의 ItemDict
        }

        if (item != null)
        {
            _currentDragItem = item;
            _rotatedOnClick = item.IsRotated;
        }
    }

    private void HandleOnDragItem(ItemDragHandler itemDrag, Vector2 mousePos)
    {
        var instanceID = itemDrag.InstanceID;
        var slotInfo = _itemUI.GetItemSlotRT(mousePos);
        if (!slotInfo.matchSlot) //No Match Slot...
        {
            _itemUI.ClearShowAvailable();
            _targetIsAvailable = false;
            return;
        }

        //_targetIsGearSlot = slotInfo.isGearSlot;
        _matchRT = slotInfo.matchSlot;

        ItemInstance dragItem;
        if (itemDrag.InventoryRT)
        {
            var originInven = _invenMap[itemDrag.InventoryRT];
            dragItem = originInven.ItemDict[instanceID].item;
        }
        else
        {
            dragItem = _inventoryManager.ItemDict[instanceID].item;
        }

        if (slotInfo.isGearSlot) //GearSlot인지 확인
        {
            _targetIsAvailable = CheckGearSlot(slotInfo.matchSlot, dragItem);
            _itemUI.ShowSlotAvailable(_targetIsAvailable, slotInfo.matchSlot.position, slotInfo.matchSlot.sizeDelta);
        }
        else
        {
            _targetIsAvailable = CheckInventory(slotInfo.matchSlot, mousePos, dragItem);
        }
    }

    //정리?
    private void HandleOnEndDragItem(ItemDragHandler itemDrag) //아이템 드래그 End
    {
        _itemUI.ClearShowAvailable();

        //가방, 리그 등 이동할 때 고려(아이템 하위 인벤토리 관련). -> 저장은?

        if (!_targetIsAvailable) //unavailable itemDrag 원래대로
        {
            if (_rotatedOnClick != _currentDragItem.IsRotated)
            {
                HandleOnRotateItem(itemDrag); //초기 회전상태로
            }

            itemDrag.ReturnItemDrag();
            return;
        }

        var originInvenRT = itemDrag.InventoryRT;

        if (_targetCellItemID != Guid.Empty) //타겟CellItem이 동일한 stackable 아이템
        {
            ItemInstance targetCellItem;
            if (!_targetInventory) //타겟이 GearSlot
            {
                targetCellItem = _inventoryManager.ItemDict[_targetCellItemID].item;
            }
            else //타겟이 인벤토리
            {
                targetCellItem = _targetInventory.ItemDict[_targetCellItemID].item;
            }

            var itemStackAmount = _currentDragItem.CurrentStackAmount; //drag 아이템의 스택
            var targetCellStackAmount = targetCellItem.CurrentStackAmount; // 타겟 Cell 아이템 스택
            var maxStackAmount = _currentDragItem.MaxStackAmount; //drag 아이템의 최대 스택
            var remainingTargetCellAmount = maxStackAmount - targetCellStackAmount;
            //타겟 Cell 아이템의 최대까지 남은 스택

            if (remainingTargetCellAmount < itemStackAmount)//drag 아이템의 스택이 더 많으면 (target의 필요한 스택보다)
            {
                _currentDragItem.AdjustStackAmount(-remainingTargetCellAmount); //타겟 Cell의 부족한 스택만큼 빼기
                itemDrag.SetStackAmountText(_currentDragItem.CurrentStackAmount);
                targetCellItem.AdjustStackAmount(remainingTargetCellAmount); //Max까지
                _itemDragHandlers[targetCellItem.InstanceID].SetStackAmountText(targetCellItem.CurrentStackAmount);
                itemDrag.ReturnItemDrag();
            }
            else  //drag 아이템 스택이 더 적으면
            {
                if (!originInvenRT) //GearSlot
                {
                    var originCell = _inventoryManager.ItemDict[_currentDragItem.InstanceID].cell;
                    _inventoryManager.RemoveGearItem(_currentDragItem.InstanceID); //drag아이템 기존 슬롯에서 제거
                }
                else //Inventory
                {
                    var originInven = _invenMap[originInvenRT];

                    bool hasRotated = _rotatedOnClick != _currentDragItem.IsRotated; //드래그 중에 회전했는지 체크
                    originInven.RemoveItem(_currentDragItem.InstanceID, hasRotated); //기존 Inven에서 제거
                }
                
                var itemDragHandler = _itemDragHandlers[_currentDragItem.InstanceID];
               
                ObjectPoolingManager.Instance.ReleaseItemDragHandler(itemDragHandler);
                _itemDragHandlers.Remove(_currentDragItem.InstanceID); //drag Handler 비활성화...(추후 풀링으로 관리)

                targetCellItem.AdjustStackAmount(itemStackAmount); //드래그 아이템의 스택만큼 추가
                _itemDragHandlers[targetCellItem.InstanceID].SetStackAmountText(targetCellItem.CurrentStackAmount);
            }

            return;
        }

        //stackable이 아닌 경우, stackable이지만 target이 다른 아이템인 경우
        //기존 인벤토리(슬롯)에서 제거
        if (originInvenRT && _targetInventory && _invenMap[originInvenRT] == _targetInventory)//같은 인벤토리에서 이동
        {
            bool hasRotated = _rotatedOnClick != _currentDragItem.IsRotated; //드래그 중 회전 체크
            var (targetPos, itemRT) = 
                _targetInventory.MoveItem(_currentDragItem, _targetFirstIdx, _targetSlotRT, hasRotated);
            var itemSize = new Vector2(_currentDragItem.ItemCellCount.x, _currentDragItem.ItemCellCount.y)
                           * ItemUI.CellSize;
            itemDrag.SetItemDragPos(targetPos, itemSize, itemRT, _matchRT);
            return; //제거없이 이동만...
        }
        
        if (!originInvenRT) //GearSlot
        {
            _inventoryManager.RemoveGearItem(_currentDragItem.InstanceID); //기존 GearSlot에서 제거

            //rig, backpack 장착해제 -> 인벤토리 해제...
        }
        else //Inventory
        {
            var originInven = _invenMap[originInvenRT];

            bool hasRotated = _rotatedOnClick != _currentDragItem.IsRotated; //드래그 중에 회전했는지 체크
            originInven.RemoveItem(_currentDragItem.InstanceID, hasRotated); //기존 Inven에서 제거
        }

        if (!_targetInventory) //GearSlot일 때
        {
            if (_currentDragItem.IsRotated) //회전된 상태면 기본 상태로
            {
                HandleOnRotateItem(itemDrag); 
            }

            SetGearItem(itemDrag, _targetGearSlot, _currentDragItem);
            
        }
        else
        {
            var (targetPos, itemRT) =
                _targetInventory.AddItem(_currentDragItem, _targetFirstIdx, _targetSlotRT); //target인벤으로 이동
            var itemSize = new Vector2(_currentDragItem.ItemCellCount.x, _currentDragItem.ItemCellCount.y)
                           * ItemUI.CellSize;

            itemDrag.SetItemDragPos(targetPos, itemSize, itemRT, _matchRT);
        }
    }

    private void HandleOnRotateItem(ItemDragHandler itemDrag)
    {
        if (_currentDragItem == null) return;
       
        _itemUI.ClearShowAvailable();
        _targetIsAvailable = false; //CellCheck 다시
        _currentDragItem.RotateItem();

        var size = new Vector2(_currentDragItem.ItemCellCount.x, _currentDragItem.ItemCellCount.y) * CellSize;
        itemDrag.SetItemDragRotate(_currentDragItem.IsRotated, size);
        //GearSlot rotate 해제...
    }

    private void SetGearItem(ItemDragHandler itemDrag, CellData gearSlot ,ItemInstance itemInstance)
    {
        _inventoryManager.SetGearItem(gearSlot, itemInstance); //장비 설정
        var gearRT = _gearRTMap[gearSlot];
        var size = gearRT.sizeDelta;
        var pos = new Vector2(size.x, -size.y) / 2; //좌표
        itemDrag.SetItemDragPos(pos, size, gearRT,null);//itemDrag 설정
    }
    
    private void HandleOnQuickAddItem(ItemDragHandler itemDrag)//빠른 획득
    {
        var instanceID = itemDrag.InstanceID;
        var inventoryRT = itemDrag.InventoryRT;
       
        if(inventoryRT != _itemUI.LootSlotParent) return; //Loot인벤이 아니면 무시
      
        var lootInven = _inventoryManager.LootInventory;
        var item = lootInven.ItemDict[instanceID].item;
        
        var backpackInven = _inventoryManager.BackpackInventory;
        var rigInven = _inventoryManager.RigInventory;
        Inventory targetInven = null;
        RectTransform targetRT = null;
        if (backpackInven)
        {
            targetInven = backpackInven;
            targetRT = _itemUI.BackpackInvenParent;
        }
        else if (rigInven)
        {
            targetInven = rigInven;
            targetRT = _itemUI.RigInvenParent;
        }
        
        if (targetInven != null) //backpack or rig Inven
        {
            var (isAvailable,  firstIdx, slotRT) = targetInven.CheckCanAddItem(item.ItemData);
            if(!isAvailable) return;
            var (targetPos, itemRT) = targetInven.AddItem(item, firstIdx, slotRT);
            var itemSize = new Vector2(item.ItemCellCount.x, item.ItemCellCount.y) * CellSize;
            itemDrag.SetItemDragPos(targetPos, itemSize, itemRT, targetRT);
           
        }
        else
        {
            var cell = _inventoryManager.CheckCanEquipItem(GearType.None, item.ItemCellCount); //Pocket
            if(cell == null) return;
            SetGearItem(itemDrag, cell, item);
        }
        lootInven.RemoveItem(instanceID, false);
    }
    
    private void HandleOnQuickDropItem(ItemDragHandler itemDrag)
    {
        var instanceID = itemDrag.InstanceID;
        var inventoryRT = itemDrag.InventoryRT;
        
        if(inventoryRT == _itemUI.LootSlotParent) return;
        ItemInstance item;
        if (inventoryRT)
        {
            var inventory = _invenMap[inventoryRT];
            item = inventory.ItemDict[instanceID].item;
        }
        else item = _inventoryManager.ItemDict[instanceID].item;
        
        RemoveItem(instanceID);
        
        ItemPickUp itemPickUp =  ObjectPoolingManager.Instance.GetItemPickUp(); //Position?
        itemPickUp.transform.position = gameObject.transform.position + new Vector3(0, .5f, 0);
        itemPickUp.Init(item);
        if(item.IsRotated) item.RotateItem(); //회전했으면 기본으로
    }
    
    //아이템 사용 연동? -> 어떤 방법을?
    private void HandleOnSetQuickSlot(ItemDragHandler itemDrag, QuickSlotIdx quickSlotIdx) 
    {
        var instanceID = itemDrag.InstanceID;
        var inventoryRT = itemDrag.InventoryRT;
        
        //아이템 사용 시 개선?
        ItemInstance item;
        Inventory targetInven;
        if (inventoryRT == _itemUI.RigInvenParent)
        {
            item = _inventoryManager.RigInventory.ItemDict[instanceID].item;
            targetInven = _inventoryManager.RigInventory;
        }
        else if (!inventoryRT)
        {
            item = _inventoryManager.ItemDict[instanceID].item;
            targetInven = null;
        }
        else return;
        
        if (item.ItemData is IConsumableItem)
        {
            for (var idx = QuickSlotIdx.QuickSlot4; idx <= QuickSlotIdx.QuickSlot7; idx++)
            {
                if (item.InstanceID != _inventoryManager.QuickSlotDict[idx].ID) continue; 
                //이미 기존 퀵슬롯에 있을 때...
                _inventoryManager.QuickSlotDict[idx] = (Guid.Empty, null);
                _itemUI.ClearQuickSlot((int)idx);
                itemDrag.ClearQuickSlotKey();
                
                if (idx == quickSlotIdx) return; //같은 퀵슬롯으로 Invoke(등록 해제), 새로 등록하는 하단 코드를 실행하지않음
            }
            
            itemDrag.SetQuickSlotKey((int)quickSlotIdx);
            _inventoryManager.QuickSlotDict[quickSlotIdx] = (instanceID, targetInven);
            _itemUI.SetQuickSlot((int)quickSlotIdx, item.IsStackable,
                item.ItemData.ItemSprite, item.CurrentStackAmount);
        }
    }

    private void HandleOnOpenItemContextMenu(ItemDragHandler itemDrag) //ContextMenu 열기(ItemDrag 우클릭)
    {
        var instanceID = itemDrag.InstanceID;
        ItemInstance item;
        if (itemDrag.InventoryRT) //InvenRT따라 
        {
            var inven = _invenMap[itemDrag.InventoryRT];
            item = inven.ItemDict[instanceID].item;
        }
        else
        {
            item = _inventoryManager.ItemDict[instanceID].item;
        }

        if (item == _currentCotextMenuItem)
        {
            HandleOnCloseItemContextMenu();
            return;
        }
        
        _currentCotextMenuItem = item;
        
        bool isGear = item.GearType != GearType.None;
        bool isAvailable; 
        if (isGear)
        {
            var cell = _inventoryManager.CheckCanEquipItem(item.GearType, item.ItemCellCount);
            isAvailable = cell != null; //null이 아니면 유효.(장착가능)
        }
        else
        {
            isAvailable = item.ItemData is MedicalData or FoodData;//사용가능한 아이템 만...(의약품, 음식)..
        }
        
        _itemUI.OpenItemContextMenu(itemDrag.transform.position, isAvailable, isGear);//List 설정
        
    }

    private void HandleOnCloseItemContextMenu()
    {
        _currentCotextMenuItem = null;
        _itemUI.CloseItemContextMenu();
    }
    
    private void HandleOnItemContextMenuClick(ItemContextType contextType)
    {
        var instanceID = _currentCotextMenuItem.InstanceID;
        var itemDrag = _itemDragHandlers[instanceID];
        switch (contextType)
        {
            case ItemContextType.Info:
                _itemUI.OpenItemInfo(_currentCotextMenuItem);
                break;
            case ItemContextType.Use:
                //use
                var inventoryRT = itemDrag.InventoryRT;
                var inventory = inventoryRT ? _invenMap[inventoryRT] : null;
                _inventoryManager.UseItem(instanceID, inventory);
                break;
            case ItemContextType.Equip:
                var cell = _inventoryManager
                    .CheckCanEquipItem(_currentCotextMenuItem.GearType, _currentCotextMenuItem.ItemCellCount);
                SetGearItem(itemDrag, cell, _currentCotextMenuItem);
                break;
            case ItemContextType.Drop:
                HandleOnQuickDropItem(itemDrag);
                break;
        }
    }

    private void RemoveItem(Guid instanceID)
    {
        var itemDrag = _itemDragHandlers[instanceID];
        var inventoryRT = itemDrag.InventoryRT;
        if(!inventoryRT) _inventoryManager.RemoveGearItem(instanceID);
        else _invenMap[inventoryRT].RemoveItem(instanceID, false);
        HandleOnRemoveItemHandler(instanceID);
    }
    
    private void HandleOnShowItemInfo(ItemDragHandler itemDrag)
    {
        var id = itemDrag.InstanceID;
        
        ItemInstance item;
        if(itemDrag.InventoryRT) item = _invenMap[itemDrag.InventoryRT].ItemDict[id].item;
        else item = _inventoryManager.ItemDict[id].item;
        
        _itemUI.OpenItemInfo(item);
    }
    
    private bool CheckGearSlot(RectTransform matchRT, ItemInstance dragItem)
    {
        _targetInventory = null;
        if (!_gearSlotsMap[matchRT].IsEmpty) //Empty가 아닐 때
        {
            if (!dragItem.IsStackable) return false; //stackable이 아니면 false 

            var targetCellItemID = _gearSlotsMap[matchRT].InstanceID;
            var targetCellItem = _inventoryManager.ItemDict[targetCellItemID].item;
            if (dragItem.ItemData.ItemDataID != targetCellItem.ItemData.ItemDataID) return false;
            //아이템 데이터 ID 비교, 다르면 false
            if (targetCellItem.CurrentStackAmount >= dragItem.MaxStackAmount) return false; //target이 최대 Stack이면 false

            _targetGearSlot = _gearSlotsMap[matchRT]; //타겟 GearSlot
            _targetCellItemID = targetCellItemID; //해당 CellItem ID
            return true;
        }

        //Empty일 때...
        GearType itemType = _currentDragItem.GearType; //드래그 중인 아이템의 타입

        var slotCell = _gearSlotsMap[matchRT]; //체크 중인 슬롯의 Cell
        GearType slotGearType = slotCell.GearType; //슬롯의  타입

        if (slotGearType == GearType.None) //Pocket 1x1 사이즈...
        {
            if (dragItem.ItemCellCount.x > 1 || dragItem.ItemCellCount.y > 1) return false; //1x1보다 크면 false
        }
        else if (itemType is GearType.ArmoredRig or GearType.UnarmoredRig) //아이템이 리그일 때
        {
            if (slotGearType is not (GearType.ArmoredRig or GearType.UnarmoredRig)) return false; //슬롯이 리그가 아니면 false
        }
        else if (slotGearType != itemType) return false; //아이템과 슬롯의 타입이 다르면 불가.

        if (itemType is GearType.ArmoredRig) //방탄리그 - 방탄조끼의 제한... 개선 어떻게?
        {
            if (!_inventoryManager.BodyArmorSlot.IsEmpty) return false; //방탄복이 장착된 상태면 방탄 리그 불가.
        }
        else if (itemType is GearType.BodyArmor) //방탄복일 때
        {
            if (_inventoryManager.ChestRigSlot.IsEmpty) //리그가 빈 상태면 true
            {
                _targetGearSlot = _inventoryManager.BodyArmorSlot;
                return true; //Slot Available      
            }

            var rigID = _inventoryManager.ChestRigSlot.InstanceID;
            var rigItemType = _inventoryManager.ItemDict[rigID].item.GearType;
            if (rigItemType == GearType.ArmoredRig) return false; //장착된 리그가 방탄 리그면 불가. false
        }

        _targetGearSlot = _gearSlotsMap[matchRT]; // target GearSlot
        _targetCellItemID = Guid.Empty; //빈 ID 할당
        return true; //Slot Available      
    }

    private bool CheckInventory(RectTransform matchRT, Vector2 mousePos, ItemInstance dragItem)
    {
        //인벤토리 - 아이템 자신의 인벤토리 안으로 들어 가는 것 방지...
        var targetInven = _invenMap[matchRT];

        if (dragItem.InstanceID == targetInven.ItemInstanceID) return false; 
        // 아이템이 자기 자신의 인벤토리에 들어갈려고 하면 false;

        //간소화?
        var (firstIdx, firstIdxPos, mathSlotRT, status, cellCount, targetCellItemID) =
            targetInven.CheckSlot(mousePos, dragItem);
        var cell = cellCount * ItemUI.CellSize;

        bool isAvailable = false;

        switch (status)
        {
            case SlotStatus.None: //No Match Slot
                return false;
            case SlotStatus.Available:
                isAvailable = true;
                break;
            case SlotStatus.Unavailable:
                break;
        }

        _itemUI.ShowSlotAvailable(isAvailable, firstIdxPos, cell);
        if (isAvailable)
        {
            _targetInventory = targetInven;
            _targetFirstIdx = firstIdx;
            _targetSlotRT = mathSlotRT;
            _targetCellItemID = targetCellItemID; //Guid.Empty가 아니면 같은 stackable 아이템인 경우.
        }

        return isAvailable;
    }

    private void HandleOnInitInventory(GameObject inventoryPrefab, ItemInstance item)
    {
        GearType gearType;
        Guid instanceID;

        if (item == null) //아이템이 없으면 LootInventorySlot
        {
            gearType = GearType.None;
            instanceID = Guid.Empty;
        }
        else
        {
            gearType = item.GearType;
            instanceID = item.InstanceID;
        }

        var inventory = _itemUI.SetInventorySlot(inventoryPrefab, gearType, instanceID, true);

        OnSetInventory(item, gearType, inventory);//인벤토리 Set
    }

    private void HandleOnShowInventory(ItemInstance item)
    {
        var inventory = item.ItemInventory;
        var gearType = item.GearType;
        var instanceID = item.InstanceID;
        _itemUI.SetInventorySlot(inventory.gameObject, gearType, instanceID, false);
        
        OnSetInventory(item, gearType, inventory);
    }

    private void HandleOnSetLootInventory(LootCrate lootCrate)
    {
        var inventory = lootCrate?.GetLootInventory();
        _invenMap[_itemUI.LootSlotParent] = inventory;
        _itemUI.SetLootInventory(lootCrate);
    }

    private void OnSetInventory(ItemInstance item, GearType gearType, Inventory inventory)
    {
        switch (gearType)
        {
            case GearType.ArmoredRig:
            case GearType.UnarmoredRig:
                _inventoryManager.SetInventoryData(inventory, gearType);
                _invenMap[_itemUI.RigInvenParent] = inventory;
                item?.SetItemInventory(inventory);
                break;
            case GearType.Backpack:
                _inventoryManager.SetInventoryData(inventory, gearType);
                _invenMap[_itemUI.BackpackInvenParent] = inventory;
                item?.SetItemInventory(inventory);
                break;
            case GearType.None: //LootInventory
                _inventoryManager.SetInventoryData(inventory, gearType);
                _invenMap[_itemUI.LootSlotParent] = inventory;
                break;
        }
    }

    private ItemDragHandler InitItemDragHandler(ItemInstance item) //ItemDragHandler초기화
    {
        var itemDragHandler = ObjectPoolingManager.Instance.GetItemDragHandler();
        _itemDragHandlers[item.InstanceID] = itemDragHandler;
        
        var itemDragActionMap = new Dictionary<ItemDragAction, InputAction>
        {
            { ItemDragAction.Rotate, _uiControl.ItemRotateAction },
            { ItemDragAction.QuickAddItem, _uiControl.QuickAddItemAction },
            { ItemDragAction.QuickDropItem, _uiControl.QuickDropItemAction },
            { ItemDragAction.SetQuickSlot, _uiControl.SetQuickSlotAction }
        };
        
        itemDragHandler.Init(item, this, itemDragActionMap, _itemUI.transform);;
        
        if (item.IsStackable) //개선?
        {
            itemDragHandler.SetStackAmountText(item.CurrentStackAmount);
        }
        else if (item is WeaponInstance weapon)
        {
            itemDragHandler.SetMagazineCountText(weapon.IsFullyLoaded(), weapon.CurrentMagazineCount);
        }
        return itemDragHandler;
    }

    private void HandleOnEquipFieldItem(CellData gearSlot, ItemInstance item)//장착
    {
        var gearSlotRT = _gearRTMap[gearSlot];

        var itemDragHandlerInstance = InitItemDragHandler(item);
       
        var size = gearSlotRT.sizeDelta;
        
        var targetPos = new Vector2(size.x, -size.y) / 2;
        itemDragHandlerInstance.SetItemDragPos(targetPos, size, gearSlotRT, null);
    }
    
    private void HandleOnAddFieldItemToInventory(GearType inventoryType, Vector2 pos, RectTransform itemRT ,ItemInstance item)
        //인벤토리에 아이템 추가(아이템 줍기, 보상 받기 등)
    {
        var itemDragHandlerInstance = InitItemDragHandler(item);
        
        Vector2 size = new Vector2(item.ItemCellCount.x, item.ItemCellCount.y) * ItemUI.CellSize;
        
        switch (inventoryType)
        {
            case GearType.ArmoredRig: 
            case GearType.UnarmoredRig:
                itemDragHandlerInstance.SetItemDragPos(pos, size, itemRT, _itemUI.RigInvenParent);
                break;
            case GearType.Backpack:
                itemDragHandlerInstance.SetItemDragPos(pos, size, itemRT, _itemUI.BackpackInvenParent);
                break;
            case GearType.None:
                //Unavailable 표시 -> UI Manager
                break;
        }
    }

    private void HandleOnUpdateItemStack(Guid id, int stackAmount)
    {
        _itemDragHandlers[id].SetStackAmountText(stackAmount);
        foreach (var (idx, (quickSlotID,_)) in _inventoryManager.QuickSlotDict)
        {
            if (!id.Equals(quickSlotID)) continue;
            
            _itemUI.UpdateQuickSlotCount((int)idx, stackAmount);
            return;
        }
    }

    private void HandleOnUpdateWeaponItemMagCount(Guid id, bool isFullyLoaded ,int magazineCount)
    {
        _itemDragHandlers[id].SetMagazineCountText(isFullyLoaded, magazineCount);
    }
    
    private void HandleOnRemoveItemHandler(Guid id)
    {
        var itemDragHandler = _itemDragHandlers[id];
        ObjectPoolingManager.Instance.ReleaseItemDragHandler(itemDragHandler);
        _itemDragHandlers.Remove(id);
    }

    private void HandleOnRemoveQuickSlotItem(Guid id, QuickSlotIdx idx)
    {
        //Use나 Drop이 아니라 단순히 Move인 경우
        if (_itemDragHandlers.TryGetValue(id, out var itemDrag))
        {
            itemDrag.ClearQuickSlotKey();
        }
        _itemUI.ClearQuickSlot((int)idx);
    }

    public void SetItemToLootCrate(LootCrateItemInput[] lootCrateItemInputs, Inventory inventory) //LootCrate 아이템 초기화
    {
        foreach (var lootCrateItemInput in lootCrateItemInputs)
        {
            var itemData = lootCrateItemInput.itemData; //아이템
            var (isAvailable, firstIdx, slotRT) = inventory.CheckCanAddItem(itemData);
            if (!isAvailable) return;

            var invenItem = ItemInstance.CreateItemInstance(itemData);
            var itemDrag = InitItemDragHandler(invenItem);
            var size = new Vector2(invenItem.ItemCellCount.x, invenItem.ItemCellCount.y) * ItemUI.CellSize;

            var (pos, itemRT) = inventory.AddItem(invenItem, firstIdx, slotRT);
            itemDrag.SetItemDragPos(pos, size, itemRT, _itemUI.LootSlotParent);
            if (itemData.IsStackable) //stackable일 경우.
            {
                var stackAmount = lootCrateItemInput.stackAmount;
                if (stackAmount < 1) stackAmount = 1;
                else if (stackAmount > itemData.MaxStackAmount) stackAmount = itemData.MaxStackAmount;
                //최소최대 수량 제한
                invenItem.SetStackAmount(stackAmount);
                itemDrag.SetStackAmountText(stackAmount);
            }
        }
    }

    private void HandleOnCloseItemUI()
    {
        if(!_inventoryManager.LootInventory) return;
        _inventoryManager.LootInventory.gameObject.SetActive(false);
        _inventoryManager.SetLootInventory(null);
    }
    
    //임시?
    private void SetItemToInventory(BaseItemDataSO itemData)
    {
        var inventory = _inventoryManager.LootInventory;
        var (isAvailable, firstIdx, slotRT) = inventory.CheckCanAddItem(itemData);
        
        if(!isAvailable) return;

        var invenItem = ItemInstance.CreateItemInstance(itemData);
        var itemDrag = InitItemDragHandler(invenItem); 
        
        var size = new Vector2(invenItem.ItemCellCount.x, invenItem.ItemCellCount.y) *  ItemUI.CellSize;
        
        var (pos,itemRT) = inventory.AddItem(invenItem, firstIdx, slotRT);
        itemDrag.SetItemDragPos(pos, size,itemRT,_itemUI.LootSlotParent);
    }

    private void SetStackableItem(BaseItemDataSO itemData, int stackAmount)
    {
        if(!itemData.IsStackable) return;
        
        var inventory = _inventoryManager.LootInventory;
        var (isAvailable, firstIdx, slotRT) = inventory.CheckCanAddItem(itemData);
        
        if(!isAvailable) return;
        
        var invenItem = ItemInstance.CreateItemInstance(itemData);
        var itemDrag = InitItemDragHandler(invenItem);
        
        var size = new Vector2(invenItem.ItemCellCount.x, invenItem.ItemCellCount.y) *  ItemUI.CellSize;
        
        var (pos,itemRT) = inventory.AddItem(invenItem, firstIdx, slotRT);
        itemDrag.SetItemDragPos(pos, size, itemRT,_itemUI.LootSlotParent);
      
        invenItem.SetStackAmount(stackAmount);
        itemDrag.SetStackAmountText(stackAmount);
    }

    private void SetGearItem(BaseItemDataSO itemData, RectTransform gearSlot) //처리?
    {
        if (!_gearSlotsMap.TryGetValue(gearSlot, out var gearCell)) return;
        
        var invenItem = ItemInstance.CreateItemInstance(itemData);

        var itemDrag = InitItemDragHandler(invenItem);
        
        SetGearItem(itemDrag, gearCell, invenItem);
    }
}
