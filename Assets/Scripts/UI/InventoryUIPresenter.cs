using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class InventoryUIPresenter : MonoBehaviour
{
    private InventoryManager _inventoryManager;
    private UIManager _uiManager;

    private readonly Dictionary<RectTransform, CellData> _gearSlotsMap = new();
    private readonly Dictionary<RectTransform, Inventory> _invenMap = new();

    public float CellSize => _uiManager.CellSize;

    private InventoryItem _currentDragItem; //현재 드래그 중인 아이템
    private bool _targetIsAvailable;
    private bool _targetIsGearSlot;
    private RectTransform _matchRT;
    private CellData _targetGearSlot;
    private Inventory _targetInventory;
    private RectTransform _targetSlotRT;
    private int _targetFirstIdx;
    
    //ItemDragger List?
    //test
    [SerializeField] private ItemDragHandler itemDragHandlerTest;
    [SerializeField] private BaseItemDataSO pistolTestData;
    [SerializeField] private ItemDragHandler itemDragHandlerTest2;
    [SerializeField] private BaseItemDataSO itemDataTest;
    
    private void Awake()
    {
        _inventoryManager = GetComponent<InventoryManager>();
        _uiManager = FindFirstObjectByType<UIManager>();
    }
    
    private void Start()
    {
        //Left Panel Init
        _inventoryManager.HeadwearSlot.SetCellRT(_uiManager.HeadwearSlotRT);
        _gearSlotsMap[_uiManager.HeadwearSlotRT] = _inventoryManager.HeadwearSlot;

        _inventoryManager.EyewearSlot.SetCellRT(_uiManager.EyewearSlotRT);
        _gearSlotsMap[_uiManager.EyewearSlotRT] = _inventoryManager.EyewearSlot;

        _inventoryManager.BodyArmorSlot.SetCellRT(_uiManager.BodyArmorSlotRT);
        _gearSlotsMap[_uiManager.BodyArmorSlotRT] = _inventoryManager.BodyArmorSlot;

        _inventoryManager.PrimaryWeaponSlot.SetCellRT(_uiManager.PWeaponSlotRT);
        _gearSlotsMap[_uiManager.PWeaponSlotRT] = _inventoryManager.PrimaryWeaponSlot;

        _inventoryManager.SecondaryWeaponSlot.SetCellRT(_uiManager.SWeaponSlotRT);
        _gearSlotsMap[_uiManager.SWeaponSlotRT] = _inventoryManager.SecondaryWeaponSlot;

        //Mid Panel Init
        _inventoryManager.ChestRigSlot.SetCellRT(_uiManager.RigSlotRT);
        _gearSlotsMap[_uiManager.RigSlotRT] = _inventoryManager.ChestRigSlot;

        _inventoryManager.BackpackSlot.SetCellRT(_uiManager.BackpackSlotRT);
        _gearSlotsMap[_uiManager.BackpackSlotRT] = _inventoryManager.BackpackSlot;

        for (int i = 0; i < 4; i++)
        {
            _inventoryManager.PocketSlots[i].SetCellRT(_uiManager.PocketsSlotRT[i]);
            _gearSlotsMap[_uiManager.PocketsSlotRT[i]] = _inventoryManager.PocketSlots[i];
        }
        
        //Inventory 추가 **Inventory 초기 상태는 null!
        _invenMap[_uiManager.RigInvenParent] = _inventoryManager.RigInventory;
        _invenMap[_uiManager.BackpackInvenParent] = _inventoryManager.BackpackInventory;
        _invenMap[_uiManager.LootSlotParent] = _inventoryManager.LootInventory;
        
        //Event
        //InventoryManager
        _inventoryManager.OnSetInventory += HandleOnSetInventory;
       
    }

    private void OnDestroy()
    {
        //Event unsubscribe
        _inventoryManager.OnSetInventory -= HandleOnSetInventory;
    }

    public void InitItemDragHandler(ItemDragHandler itemDrag)//아이템 줍기 등에서 생성...맵에서 상자열 때 생성...
    {
        itemDrag.OnPointerDownEvent += HandleOnPointerEnter;
        itemDrag.OnDragEvent += HandleOnDragItem;
        itemDrag.OnEndDragEvent += HandleOnEndDragItem;
    }

    public void OnDisableItemDragHandler(ItemDragHandler itemDrag)
    {
        itemDrag.OnPointerDownEvent -= HandleOnPointerEnter;
        itemDrag.OnDragEvent -= HandleOnDragItem;
        itemDrag.OnEndDragEvent -= HandleOnEndDragItem;
    }
    
    //OnPointerEnter -> 해당 아이템, 기존 슬롯 정보 캐싱
    //OnDrag -> 해당 슬롯 정보 캐싱
    //OnEndDrag -> 이동/원복 처리
    private void HandleOnPointerEnter(ItemDragHandler itemDrag, Guid itemID)
    {
        var originInvenRT = itemDrag.InventoryRT;
        
        InventoryItem item;
        
        if (!originInvenRT)//기존 위치 인벤토리 체크
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

            item = inventory.ItemDict[itemID].item; //Inventory의 ItemDict
        }

        if (item != null)
        {
            _currentDragItem = item;
        }
    }
    
    private void HandleOnDragItem(ItemDragHandler itemDrag, Vector2 mousePos, Guid itemID)
    {
        var slotInfo = _uiManager.GetItemSlotRT(mousePos); 
        if (!slotInfo.matchSlot) //No Match Slot...
        {
            _uiManager.ClearShowAvailable();
            return;
        }
        
        _targetIsGearSlot = slotInfo.isGearSlot;
        _matchRT = slotInfo.matchSlot;
        
        if (slotInfo.isGearSlot)
        {
            _targetIsAvailable = CheckGearSlot(slotInfo.matchSlot);
            _uiManager.ShowSlotAvailable(_targetIsAvailable, slotInfo.matchSlot.position, slotInfo.matchSlot.sizeDelta);
        }
        else
        {
            _targetIsAvailable = CheckInventory(slotInfo.matchSlot, itemDrag.InventoryRT, mousePos, itemID);
        }
    }

    private void HandleOnEndDragItem(ItemDragHandler itemDrag, Vector2 mousePos, Guid itemID)
    {
        _uiManager.ClearShowAvailable();
        //실제 아이템 이동...
        if (!_targetIsAvailable)
        {
            Debug.Log("Target Is Not Available");
            itemDrag.ReturnItemDrag();
            return;
        }
        
        var originInvenRT = itemDrag.InventoryRT;
        if (!originInvenRT)
        {
            var gearSlot = _inventoryManager.ItemDict[itemID];
            _inventoryManager.RemoveGearItem(gearSlot.cell, gearSlot.item.Id);
            
            if (_targetIsGearSlot)
            {
                _inventoryManager.SetGearItem(_targetGearSlot, _currentDragItem);
                Vector2 targetPos = new Vector2(_matchRT.sizeDelta.x, -_matchRT.sizeDelta.y) / 2;
                itemDrag.SetItemDragPos(targetPos, _matchRT.sizeDelta, _matchRT, 
                    null);
            }
            else
            {
                var targetPos = _targetInventory.MoveItem(gearSlot.item, _targetFirstIdx, _targetSlotRT);
                var itemSize = new Vector2(gearSlot.item.ItemCellCount.x, gearSlot.item.ItemCellCount.y)
                               * _uiManager.CellSize;
                itemDrag.SetItemDragPos(targetPos, itemSize, _targetInventory.ItemRT, _matchRT );
            }
        }
        else
        {
            var originInven = _invenMap[originInvenRT];
            var item = originInven.ItemDict[itemID].item;
            originInven.RemoveItem(itemID);

            if (_targetIsGearSlot)
            {

                _inventoryManager.SetGearItem(_targetGearSlot, item);
                Vector2 targetPos = new Vector2(_matchRT.sizeDelta.x, -_matchRT.sizeDelta.y) / 2;
                itemDrag.SetItemDragPos(targetPos, _matchRT.sizeDelta, _matchRT, 
                    null);
            }
            else
            {
                var targetPos = _targetInventory.MoveItem(item, _targetFirstIdx, _targetSlotRT);
                var itemSize = new Vector2(item.ItemCellCount.x, item.ItemCellCount.y) * _uiManager.CellSize;
                itemDrag.SetItemDragPos(targetPos, itemSize, _targetInventory.ItemRT, _matchRT );
            }
        }
    }

    private bool CheckGearSlot(RectTransform matchRT)
    {
        if(!_gearSlotsMap[matchRT].IsEmpty) return false;
        
        GearType gearType = _currentDragItem.GearType; //드래그 중인 아이템의 타입
                
        var slotCell = _gearSlotsMap[matchRT];  //체크 중인 슬롯의 Cell
        GearType slotGearType = slotCell.GearType; //슬롯의  타입
                
        if(slotGearType != gearType) return false; //아이템과 슬롯의 타입이 다르면 불가.
                
        if (gearType is GearType.ArmoredRig) //방탄리그 - 방탄조끼의 제한... 개선 어떻게?
        {
            if(!_inventoryManager.BodyArmorSlot.IsEmpty) return false; //방탄복이 장착된 상태면 방탄 리그 불가.
        }
        else if (gearType is GearType.BodyArmor) //방탄복일 때
        {
            if (_inventoryManager.ChestRigSlot.IsEmpty)
            { 
                _targetGearSlot = _inventoryManager.BodyArmorSlot;
                return true; //Slot Available      
            }
            //장착된 리그가 방탄 리그면 불가.
            var rigID = _inventoryManager.ChestRigSlot.Id;
            var rigItemType =  _inventoryManager.ItemDict[rigID].item.GearType;
            if(rigItemType == GearType.ArmoredRig) return false;
        }
        _targetGearSlot = _gearSlotsMap[matchRT];
        return true;//Slot Available      
    }

    private bool CheckInventory(RectTransform matchRT, RectTransform originInvenRT, Vector2 mousePos, Guid id)
    {
        var targetInven = _invenMap[matchRT];
        
        Vector2Int itemCount;
        
        if (originInvenRT)
        {
            var originInven = _invenMap[originInvenRT];
            itemCount = originInven.ItemDict[id].item.ItemCellCount;
        }
        else
        {
            itemCount = _inventoryManager.ItemDict[id].item.ItemCellCount;
        }
        
        var (firstIdx, firstIdxPos, mathSlotRT ,status, cellCount) = 
            targetInven.CheckSlot(mousePos, itemCount, id);
        var cell = cellCount * _uiManager.CellSize;
        //cell의 크기...(MatchCell...)
        
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
        _uiManager.ShowSlotAvailable(isAvailable, firstIdxPos, cell); //cell크기...??
        if (isAvailable)
        {
            _targetInventory = targetInven;
            _targetFirstIdx = firstIdx;
            _targetSlotRT = mathSlotRT;
        }
        return isAvailable;
    }
    
    private void HandleOnSetInventory(GameObject inventoryPrefab, GearType gearType)
    {
        Inventory inventory;
        switch (gearType)
        {
            case GearType.ArmoredRig:
            case GearType.UnarmoredRig:    
                inventory = _uiManager.SetRigSlot(inventoryPrefab);
                inventory.Init(_uiManager.CellSize);
                _inventoryManager.SetInventoryData(inventory, gearType);
                _invenMap[_uiManager.RigInvenParent] = inventory;
                break;
            case GearType.Backpack:
                inventory = _uiManager.SetBackpackSlot(inventoryPrefab);
                inventory.Init(_uiManager.CellSize);
                _inventoryManager.SetInventoryData(inventory, gearType);
                _invenMap[_uiManager.BackpackInvenParent] = inventory;
                break;
            case GearType.None: //LootInventory
                inventory = _uiManager.SetLootSlot(inventoryPrefab); 
                //문제...새로 생성, 이미 생성됨...구분하기?? UI-Data 분리?
                inventory.Init(_uiManager.CellSize);
                _inventoryManager.SetInventoryData(inventory, gearType);
                _invenMap[_uiManager.LootSlotParent] = inventory;
                
                
                //test
                //ItemDrag초기화...? ItemDrag(아이템 주울 때, 퀘스트 보상 등 인벤으로 추가할때...)
                var pistolTest = new InventoryItem(pistolTestData);
                itemDragHandlerTest.Init(pistolTest, this, _uiManager.transform);
                Vector2 size = new Vector2(pistolTest.ItemCellCount.x, pistolTest.ItemCellCount.y) * _uiManager.CellSize;
                var result = inventory.AddItem(pistolTest);
                //itemDrag -> 크기 조절... 수정??
                if (result.isAvailable)
                {
                    itemDragHandlerTest.SetItemDragPos(result.pos, size, inventory.ItemRT,
                        _uiManager.LootSlotParent);
                }

                var test = new InventoryItem(itemDataTest);
                itemDragHandlerTest2.Init(test, this, _uiManager.transform);
                size = new Vector2(test.ItemCellCount.x, test.ItemCellCount.y) * _uiManager.CellSize;
                result = inventory.AddItem(test);
                if (result.isAvailable)
                {
                    itemDragHandlerTest2.SetItemDragPos(result.pos, size, inventory.ItemRT, 
                        _uiManager.LootSlotParent); 
                }
                
                break;
        }
    }
}
