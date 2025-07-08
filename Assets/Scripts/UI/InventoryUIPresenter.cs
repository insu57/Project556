using System;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class InventoryUIPresenter : MonoBehaviour
{
    private InventoryManager _inventoryManager;
    private ItemUIManager _itemUIManager;
    private UIControl _uiControl;

    private readonly Dictionary<RectTransform, CellData> _gearSlotsMap = new();
    private readonly Dictionary<CellData, RectTransform> _gearRTMap = new();
    private readonly Dictionary<RectTransform, Inventory> _invenMap = new();

    private readonly Dictionary<Guid, ItemDragHandler> _itemDragHandlers = new();
    //drag handler 관리는?? 각 인벤토리에서???
    //inventory UI - 데이터 분리????

    public float CellSize => _itemUIManager.CellSize;
    
    private ItemInstance _currentDragItem; //현재 드래그 중인 아이템
    private bool _rotatedOnClick; //클릭 시 회전 상태
    private bool _targetIsAvailable; //타겟 슬롯(Cell)이 유효한지?
    private bool _targetIsGearSlot; //타겟이 GearSlot인지
    private RectTransform _matchRT; //해당 Match Slot의 RectTransform
    private CellData _targetGearSlot; //타겟인 GearSlot CellData
    private Inventory _targetInventory; //타겟인 Inventory
    private RectTransform _targetSlotRT; //타겟 Inventory의 SlotRT
    private int _targetFirstIdx; //타겟의 Cell Idx(아이템 좌상단, 첫번째 인덱스)
    private Guid _targetCellItemID; //타겟인 Cell의 아이템ID(Stack아이템 용)
    
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
        _itemUIManager = FindFirstObjectByType<ItemUIManager>();
        _itemUIManager.TryGetComponent(out _uiControl); //개선필요
    }

    private void Start()
    {
        //Left Panel Init
        _gearSlotsMap[_itemUIManager.HeadwearSlotRT] = _inventoryManager.HeadwearSlot;
        _gearSlotsMap[_itemUIManager.EyewearSlotRT] = _inventoryManager.EyewearSlot;
        _gearSlotsMap[_itemUIManager.BodyArmorSlotRT] = _inventoryManager.BodyArmorSlot;
        _gearSlotsMap[_itemUIManager.PWeaponSlotRT] = _inventoryManager.PrimaryWeaponSlot;
        _gearSlotsMap[_itemUIManager.SWeaponSlotRT] = _inventoryManager.SecondaryWeaponSlot;
        
        _gearRTMap[_inventoryManager.HeadwearSlot] = _itemUIManager.HeadwearSlotRT;
        _gearRTMap[_inventoryManager.EyewearSlot] = _itemUIManager.EyewearSlotRT;
        _gearRTMap[_inventoryManager.BodyArmorSlot] = _itemUIManager.BodyArmorSlotRT;
        _gearRTMap[_inventoryManager.PrimaryWeaponSlot] = _itemUIManager.PWeaponSlotRT;
        _gearRTMap[_inventoryManager.SecondaryWeaponSlot] = _itemUIManager.SWeaponSlotRT;

        //Mid Panel Init
        _gearSlotsMap[_itemUIManager.RigSlotRT] = _inventoryManager.ChestRigSlot;
        _gearSlotsMap[_itemUIManager.BackpackSlotRT] = _inventoryManager.BackpackSlot;
        _gearRTMap[_inventoryManager.ChestRigSlot] = _itemUIManager.RigSlotRT;
        _gearRTMap[_inventoryManager.BackpackSlot] = _itemUIManager.BackpackSlotRT;
        for (int i = 0; i < 4; i++)
        {
            _gearSlotsMap[_itemUIManager.PocketsSlotRT[i]] = _inventoryManager.PocketSlots[i];
            _gearRTMap[_inventoryManager.PocketSlots[i]] = _itemUIManager.PocketsSlotRT[i];
        }

        //Inventory 추가 **Inventory 초기 상태는 null!
        _invenMap[_itemUIManager.RigInvenParent] = null;
        _invenMap[_itemUIManager.BackpackInvenParent] = null;
        _invenMap[_itemUIManager.LootSlotParent] = null;

        //Event
        //InventoryManager
        _inventoryManager.OnInitInventory += HandleOnInitInventory;
        _inventoryManager.OnShowInventory += HandleOnShowInventory;
        _inventoryManager.OnEquipFieldItem += HandleOnEquipItem;
        _inventoryManager.OnAddItemToInventory += HandleOnAddItemToInventory;
        _inventoryManager.OnUpdateItemStack += HandleOnUpdateItemStack;
        _inventoryManager.OnRemoveItem += HandleOnRemoveItem;
        
        //test
        HandleOnInitInventory(crate01Test, null); //Loot
        SetItem(pistolTestData);
        SetItem(itemDataTest);
        SetStackableItem(bullet556TestData, 50);
        SetStackableItem(bullet556TestData, 10);
        SetGearItem(backpackTestData, _itemUIManager.BackpackSlotRT);
        SetGearItem(rigTestData, _itemUIManager.RigSlotRT);
        SetItem(rigTanTestData);
    }

    private void OnDestroy()
    {
        //Event unsubscribe
        _inventoryManager.OnInitInventory -= HandleOnInitInventory;
        _inventoryManager.OnShowInventory -= HandleOnShowInventory;
        _inventoryManager.OnEquipFieldItem -= HandleOnEquipItem;
        _inventoryManager.OnAddItemToInventory -= HandleOnAddItemToInventory;
        _inventoryManager.OnUpdateItemStack -= HandleOnUpdateItemStack;
        _inventoryManager.OnRemoveItem -= HandleOnRemoveItem;
    }

    public void InitItemDragHandler(ItemDragHandler itemDrag) //아이템 줍기 등에서 생성...맵에서 상자열 때 생성...
    {
        itemDrag.OnPointerDownEvent += HandleOnPointerEnter;
        itemDrag.OnDragEvent += HandleOnDragItem;
        itemDrag.OnEndDragEvent += HandleOnEndDragItem;
        itemDrag.OnRotateItemEvent += HandleOnRotateItem;
    }

    public void OnDisableItemDragHandler(ItemDragHandler itemDrag)
    {
        itemDrag.OnPointerDownEvent -= HandleOnPointerEnter;
        itemDrag.OnDragEvent -= HandleOnDragItem;
        itemDrag.OnEndDragEvent -= HandleOnEndDragItem;
        itemDrag.OnRotateItemEvent -= HandleOnRotateItem;
    }

    //OnPointerEnter -> 해당 아이템, 기존 슬롯 정보 캐싱
    //OnDrag -> 해당 슬롯 정보 캐싱
    //OnEndDrag -> 이동/원복 처리
    private void HandleOnPointerEnter(ItemDragHandler itemDrag, Guid itemID)
    {
        var originInvenRT = itemDrag.InventoryRT;

        ItemInstance item;

        if (!originInvenRT) //기존 위치 인벤토리 체크
        {
            if (!_inventoryManager.ItemDict.TryGetValue(itemID, out var inventoryItem))
            {
                Debug.LogError("Item not found...!: " + itemID);
                return;
            }

            item = inventoryItem.item; //GearSlots Item Dict
        }
        else
        {
            if (!_invenMap.TryGetValue(originInvenRT, out var inventory))
            {
                Debug.LogError("Inventory not found...!: " + itemID);
                return;
            }

            if (!inventory.ItemDict.TryGetValue(itemID, out var inventoryItem))
            {
                Debug.LogError($"Inventory:{inventory} - Item not found...!: " + itemID);
                return;
            }
            item = inventory.ItemDict[itemID].item; //Inventory의 ItemDict
        }

        if (item != null)
        {
            _currentDragItem = item;
            _rotatedOnClick = item.IsRotated;
        }
    }

    private void HandleOnDragItem(ItemDragHandler itemDrag, Vector2 mousePos, Guid instanceID)
    {

        var slotInfo = _itemUIManager.GetItemSlotRT(mousePos);
        if (!slotInfo.matchSlot) //No Match Slot...
        {
            _itemUIManager.ClearShowAvailable();
            _targetIsAvailable = false;
            return;
        }

        _targetIsGearSlot = slotInfo.isGearSlot;
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
            _itemUIManager.ShowSlotAvailable(_targetIsAvailable, slotInfo.matchSlot.position, slotInfo.matchSlot.sizeDelta);
        }
        else
        {
            _targetIsAvailable = CheckInventory(slotInfo.matchSlot, mousePos, dragItem);
        }
    }

    private void HandleOnEndDragItem(ItemDragHandler itemDrag) //아이템 드래그 End
    {
        _itemUIManager.ClearShowAvailable();

        //가방, 리그 등 이동할 때 고려(아이템 하위 인벤토리 관련).
        //내용물은 어떻게????? Inventory(Mono) InventoryItem(not mono) ItemDragHandler(Mono)
        //특히 가방 인벤토리에 가방 자신이 이동 -> 막기

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
            if (_targetIsGearSlot) //타겟이 GearSlot
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

            if (remainingTargetCellAmount < itemStackAmount)
            {
                //drag 아이템의 스택이 더 많으면
                _currentDragItem.ChangeStackAmount(-remainingTargetCellAmount); //타겟 Cell의 부족한 스택만큼 빼기
                itemDrag.SetStackAmountText(_currentDragItem.CurrentStackAmount);
                targetCellItem.ChangeStackAmount(remainingTargetCellAmount); //Max까지
                _itemDragHandlers[targetCellItem.InstanceID].SetStackAmountText(targetCellItem.CurrentStackAmount);
                itemDrag.ReturnItemDrag();
            }
            else
            {
                //drag 아이템 스택이 더 적으면
                var originCell = _inventoryManager.ItemDict[_currentDragItem.InstanceID].cell;
                _inventoryManager.RemoveGearItem(originCell, _currentDragItem.InstanceID); //drag아이템 기존 슬롯에서 제거
                var itemDragHandler = _itemDragHandlers[_currentDragItem.InstanceID];
               
                ObjectPoolingManager.Instance.ReleaseItemDragHandler(itemDragHandler);
                _itemDragHandlers.Remove(_currentDragItem.InstanceID); //drag Handler 비활성화...(추후 풀링으로 관리)

                targetCellItem.ChangeStackAmount(itemStackAmount); //드래그 아이템의 스택만큼 추가
                _itemDragHandlers[targetCellItem.InstanceID].SetStackAmountText(targetCellItem.CurrentStackAmount);
            }

            return;
        }

        if (!originInvenRT)
        {
            var originGearCell = _inventoryManager.ItemDict[_currentDragItem.InstanceID].cell;

            _inventoryManager.RemoveGearItem(originGearCell, _currentDragItem.InstanceID); //기존 GearSlot에서 제거

            //rig, backpack 장착해제 -> 인벤토리 해제...

        }
        else
        {
            var originInven = _invenMap[originInvenRT];

            bool hasRotated = _rotatedOnClick != _currentDragItem.IsRotated; //드래그 중에 회전했는지 체크
            originInven.RemoveItem(_currentDragItem.InstanceID, hasRotated); //기존 Inven에서 제거
        }


        if (_targetIsGearSlot) //GearSlot일 때
        {
            if (_currentDragItem.IsRotated) //회전된 상태면 기본 상태로
            {
                HandleOnRotateItem(itemDrag); 
            }

            _inventoryManager.SetGearItem(_targetGearSlot, _currentDragItem); //장비 설정
            Vector2 targetPos = new Vector2(_matchRT.sizeDelta.x, -_matchRT.sizeDelta.y) / 2; //좌표
            itemDrag.SetItemDragPos(targetPos, _matchRT.sizeDelta, _matchRT, //itemDrag 설정
                null);
        }
        else
        {
            var (targetPos, itemRT) =
                _targetInventory.MoveItem(_currentDragItem, _targetFirstIdx, _targetSlotRT); //target인벤으로 이동
            var itemSize = new Vector2(_currentDragItem.ItemCellCount.x, _currentDragItem.ItemCellCount.y)
                           * _itemUIManager.CellSize;

            itemDrag.SetItemDragPos(targetPos, itemSize, itemRT, _matchRT);
        }
    }

    private void HandleOnRotateItem(ItemDragHandler itemDrag)
    {
        if (_currentDragItem == null) return;
       
        _itemUIManager.ClearShowAvailable();
        _targetIsAvailable = false; //CellCheck 다시
        _currentDragItem.RotateItem();

        var size = new Vector2(_currentDragItem.ItemCellCount.x, _currentDragItem.ItemCellCount.y) * CellSize;
        var isOriginGearSlot = !itemDrag.InventoryRT; //InventoryRT가 없으면 GearSlot에서 움직이는 것
        itemDrag.SetItemDragRotate(_currentDragItem.IsRotated, size);
        //GearSlot rotate 해제...
    }

    private bool CheckGearSlot(RectTransform matchRT, ItemInstance dragItem)
    {
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

        if (dragItem.InstanceID == targetInven.ItemInstanceID) return false; // 아이템이 자기 자신의 인벤토리에 들어갈려고 하면 false;

        //간소화?
        var (firstIdx, firstIdxPos, mathSlotRT, status, cellCount, targetCellItemID) =
            targetInven.CheckSlot(mousePos, dragItem);
        var cell = cellCount * _itemUIManager.CellSize;

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

        _itemUIManager.ShowSlotAvailable(isAvailable, firstIdxPos, cell);
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

        var inventory = _itemUIManager.SetInventorySlot(inventoryPrefab, gearType, instanceID, true);

        OnSetInventory(item, gearType, inventory);//인벤토리 Set
    }

    private void HandleOnShowInventory(ItemInstance item)
    {
        var inventory = item.ItemInventory;
        var gearType = item.GearType;
        var instanceID = item.InstanceID;
        _itemUIManager.SetInventorySlot(inventory.gameObject, gearType, instanceID, false);
        
        OnSetInventory(item, gearType, inventory);
    }

    private void OnSetInventory(ItemInstance item, GearType gearType, Inventory inventory)
    {
        switch (gearType)
        {
            case GearType.ArmoredRig:
            case GearType.UnarmoredRig:
                _inventoryManager.SetInventoryData(inventory, gearType);
                _invenMap[_itemUIManager.RigInvenParent] = inventory;
                item?.SetItemInventory(inventory);
                break;
            case GearType.Backpack:
                _inventoryManager.SetInventoryData(inventory, gearType);
                _invenMap[_itemUIManager.BackpackInvenParent] = inventory;
                item?.SetItemInventory(inventory);
                break;
            case GearType.None: //LootInventory
                _inventoryManager.SetInventoryData(inventory, gearType);
                _invenMap[_itemUIManager.LootSlotParent] = inventory;
                
                //itemDragHandler -> pooling 개선
                break;
        }
    }

    private void HandleOnEquipItem(CellData gearSlot, ItemInstance item)//장착
    {
        var gearSlotRT = _gearRTMap[gearSlot];
       
        var itemDragHandlerInstance = ObjectPoolingManager.Instance.GetItemDragHandler();
        _itemDragHandlers[item.InstanceID] = itemDragHandlerInstance;
        var size = gearSlotRT.sizeDelta;
       
        itemDragHandlerInstance.Init(item, this, _uiControl.ItemRotateAction, _itemUIManager.transform);
        
        var targetPos = new Vector2(size.x, -size.y) / 2;
        itemDragHandlerInstance.SetItemDragPos(targetPos, size, gearSlotRT, null);
        
    }
    
    private void HandleOnAddItemToInventory(GearType inventoryType, Vector2 pos, RectTransform itemRT ,ItemInstance item)
        //인벤토리에 아이템 추가(아이템 줍기, 보상 받기 등)
    {
        var itemDragHandlerInstance = ObjectPoolingManager.Instance.GetItemDragHandler();
        _itemDragHandlers[item.InstanceID] = itemDragHandlerInstance;
        Vector2 size = new Vector2(item.ItemCellCount.x, item.ItemCellCount.y) * _itemUIManager.CellSize;
        itemDragHandlerInstance.Init(item, this, _uiControl.ItemRotateAction, _itemUIManager.transform);
        
        switch (inventoryType)
        {
            case GearType.ArmoredRig: 
            case GearType.UnarmoredRig:
                itemDragHandlerInstance.SetItemDragPos(pos, size, itemRT, _itemUIManager.RigInvenParent);
                break;
            case GearType.Backpack:
                itemDragHandlerInstance.SetItemDragPos(pos, size, itemRT, _itemUIManager.BackpackInvenParent);
                break;
            case GearType.None:
                //Unavailable 표시 -> UI Manager
                break;
        }
    }

    private void HandleOnUpdateItemStack(Guid id, int stackAmount)
    {
        _itemDragHandlers[id].SetStackAmountText(stackAmount);
    }

    private void HandleOnRemoveItem(Guid id)
    {
        var itemDragHandler = _itemDragHandlers[id];
        ObjectPoolingManager.Instance.ReleaseItemDragHandler(itemDragHandler);
        _itemDragHandlers.Remove(id);
    }
    
    //임시?
    private void SetItem(BaseItemDataSO itemData)
    {
        var inventory = _inventoryManager.LootInventory;
        var (isAvailable, firstIdx, slotRT) = inventory.CheckCanAddItem(itemData);
        
        if(!isAvailable) return;
        
        ItemInstance invenItem;
        if (itemData is WeaponData weaponData) invenItem = new WeaponInstance(weaponData);
        else invenItem = new ItemInstance(itemData);
        var itemDrag = ObjectPoolingManager.Instance.GetItemDragHandler();
        
        itemDrag.Init(invenItem, this, _uiControl.ItemRotateAction, _itemUIManager.transform);
        var size = new Vector2(invenItem.ItemCellCount.x, invenItem.ItemCellCount.y) *  _itemUIManager.CellSize;
        
        var (pos,itemRT) = inventory.AddItem(invenItem, firstIdx, slotRT);;
        itemDrag.SetItemDragPos(pos, size,itemRT,_itemUIManager.LootSlotParent);
        _itemDragHandlers.Add(invenItem.InstanceID, itemDrag);
    }

    private void SetStackableItem(BaseItemDataSO itemData, int stackAmount)
    {
        if(!itemData.IsStackable) return;
        
        var inventory = _inventoryManager.LootInventory;
        var (isAvailable, firstIdx, slotRT) = inventory.CheckCanAddItem(itemData);
        
        if(!isAvailable) return;
        var invenItem = new ItemInstance(itemData);
        var itemDrag = ObjectPoolingManager.Instance.GetItemDragHandler();
        
        itemDrag.Init(invenItem, this, _uiControl.ItemRotateAction, _itemUIManager.transform);
        var size = new Vector2(invenItem.ItemCellCount.x, invenItem.ItemCellCount.y) *  _itemUIManager.CellSize;
        
        var (pos,itemRT) = inventory.AddItem(invenItem, firstIdx, slotRT);
        itemDrag.SetItemDragPos(pos, size, itemRT,_itemUIManager.LootSlotParent);
        invenItem.ChangeStackAmount(stackAmount);
        _itemDragHandlers.Add(invenItem.InstanceID, itemDrag);
        itemDrag.SetStackAmountText(stackAmount);
    }

    private void SetGearItem(BaseItemDataSO itemData, RectTransform gearSlot) //처리?
    {
        if (!_gearSlotsMap.TryGetValue(gearSlot, out var gearCell)) return;
        
        ItemInstance invenItem;
        if (itemData is WeaponData weaponData) invenItem = new WeaponInstance(weaponData);
        else  invenItem = new ItemInstance(itemData);
        
        var itemDrag = ObjectPoolingManager.Instance.GetItemDragHandler();
        
        itemDrag.Init(invenItem, this, _uiControl.ItemRotateAction, _itemUIManager.transform);
        var size = gearSlot.sizeDelta;
        var pos = new Vector2(gearSlot.sizeDelta.x, -gearSlot.sizeDelta.y) / 2;
        itemDrag.SetItemDragPos(pos, size, gearSlot, null);
        _inventoryManager.SetGearItem(gearCell, invenItem);
        _itemDragHandlers.Add(invenItem.InstanceID, itemDrag);
    }
}
