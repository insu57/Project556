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
    private Dictionary<Guid, ItemDragHandler> _itemDragHandlers = new(); 
    //drag handler 관리는?? 각 인벤토리에서???
    //inventory UI - 데이터 분리????
    
    public float CellSize => _uiManager.CellSize;

    [SerializeField] private ItemDragHandler itemDragHandlerPrefab; 
    private InventoryItem _currentDragItem; //현재 드래그 중인 아이템
    private bool _targetIsAvailable; //타겟 슬롯(Cell)이 유효한지?
    private bool _targetIsGearSlot; //타겟이 GearSlot인지
    private RectTransform _matchRT; //해당 Match Slot의 RectTransform
    private CellData _targetGearSlot; //타겟인 GearSlot CellData
    private Inventory _targetInventory; //타겟인 Inventory
    private RectTransform _targetSlotRT; //타겟 Inventory의 SlotRT
    private int _targetFirstIdx; //타겟의 Cell Idx(아이템 좌상단, 첫번째 인덱스)
    private Guid _targetCellItemID; //타겟인 Cell의 아이템ID(Stack아이템 용)
    
    //ItemDragger List?
    //test
    [SerializeField] private ItemDragHandler itemDragHandlerTest;
    [SerializeField] private BaseItemDataSO pistolTestData;
    [SerializeField] private ItemDragHandler itemDragHandlerTest2;
    [SerializeField] private BaseItemDataSO itemDataTest;
    [SerializeField] private ItemDragHandler itemDragHandlerTest3;
    [SerializeField] private ItemDragHandler itemDragHandlerTest4;
    [SerializeField] private BaseItemDataSO backpackTestData;
    [SerializeField] private BaseItemDataSO rigTestData;
    [SerializeField] private ItemDragHandler itemDragHandlerTest5;
    [SerializeField] private BaseItemDataSO bullet556TestData;
    [SerializeField] private ItemDragHandler itemDragHandlerTest6;
    
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
        _inventoryManager.OnAddItemToInventory += HandleOnAddItemToInventory;

    }

    private void OnDestroy()
    {
        //Event unsubscribe
        _inventoryManager.OnSetInventory -= HandleOnSetInventory;
        _inventoryManager.OnAddItemToInventory -= HandleOnAddItemToInventory;
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
    
    private void HandleOnDragItem(ItemDragHandler itemDrag, Vector2 mousePos, Guid instanceID)
    {
        var slotInfo = _uiManager.GetItemSlotRT(mousePos); 
        if (!slotInfo.matchSlot) //No Match Slot...
        {
            _uiManager.ClearShowAvailable();
            return;
        }
        
        _targetIsGearSlot = slotInfo.isGearSlot;
        _matchRT = slotInfo.matchSlot;

        InventoryItem dragItem;
        if (itemDrag.InventoryRT)
        {
            var originInven = _invenMap[itemDrag.InventoryRT];
            dragItem =  originInven.ItemDict[instanceID].item;
        }
        else
        {
            dragItem = _inventoryManager.ItemDict[instanceID].item;
        }
        
        if (slotInfo.isGearSlot)//GearSlot인지 확인
        {
            _targetIsAvailable = CheckGearSlot(slotInfo.matchSlot, dragItem);
            _uiManager.ShowSlotAvailable(_targetIsAvailable, slotInfo.matchSlot.position, slotInfo.matchSlot.sizeDelta);
        }
        else
        {
            _targetIsAvailable = CheckInventory(slotInfo.matchSlot, mousePos, dragItem);
        }
    }

    private void HandleOnEndDragItem(ItemDragHandler itemDrag, Vector2 mousePos, Guid instanceID)
    {
        _uiManager.ClearShowAvailable();
       
        //가방, 리그 등 이동할 때 고려(아이템 하위 인벤토리 관련).
        //내용물은 어떻게????? Inventory(Mono) InventoryItem(not mono) ItemDragHandler(Mono)
        //ItemDragHandler -> Inventory 자식
        //InventoryItem (Inventory.ItemDict 의 value 중 하나...)
        //특히 가방 인벤토리에 가방 자신이 이동 -> 막기
        
        if (!_targetIsAvailable) //unavailable itemDrag 원래대로
        {
            Debug.Log("Target Is Not Available");
            itemDrag.ReturnItemDrag();
            return;
        }
        
        var originInvenRT = itemDrag.InventoryRT; 
        //gearSlot인데 왜???Loot로? 버그 ???????????
        //stack관련 오류로 추정
        
        if (!originInvenRT) //GearSlot에서 이동한 경우
        {
            var (item,cell) = _inventoryManager.ItemDict[instanceID];
            
            if (!_targetGearSlot.IsEmpty && item.IsStackable) //target이 inven인지 gearSlot인지 고려 추가
            {
                var targetCellItem = _inventoryManager.ItemDict[_targetCellItemID].item;
                //한번 더 itemDataID check...(Available체크 할 때 1차 검사)
                if (item.ItemData.ItemDataID == targetCellItem.ItemData.ItemDataID)
                {
                    var itemStackAmount = item.CurrentStackAmount; //drag 아이템의 스택
                    var targetCellStackAmount =  targetCellItem.CurrentStackAmount; // 타겟 Cell 아이템 스택
                    var maxStackAmount = item.MaxStackAmount; //drag 아이템의 최대 스택
                    var remainingTargetCellAmount = maxStackAmount - targetCellStackAmount; 
                    //타겟 Cell 아이템의 최대까지 남은 스택
                    
                    if (remainingTargetCellAmount < itemStackAmount)
                    {
                        //drag 아이템의 스택이 더 많으면
                        item.SetStackAmount(-remainingTargetCellAmount);//타겟 Cell의 부족한 스택만큼 빼기
                        itemDrag.SetStackAmountText(item.CurrentStackAmount);
                        targetCellItem.SetStackAmount(remainingTargetCellAmount); //Max까지
                        _itemDragHandlers[targetCellItem.InstanceID].SetStackAmountText(targetCellItem.CurrentStackAmount);
                        itemDrag.ReturnItemDrag();
                    }
                    else
                    {
                        //drag 아이템 스택이 더 적으면
                        _inventoryManager.RemoveGearItem(cell, instanceID); //drag아이템 기존 슬롯에서 제거
                        var itemDragHandler = _itemDragHandlers[instanceID];
                        itemDragHandler.gameObject.SetActive(false);
                        _itemDragHandlers.Remove(instanceID); //drag Handler 비활성화...(추후 풀링으로 관리)
                        targetCellItem.SetStackAmount(itemStackAmount); //드래그 아이템의 스택만큼 추가
                    }
                    return;
                }
                
            }
            
            _inventoryManager.RemoveGearItem(cell, item.InstanceID); //아이템을 기존 슬롯에서 제거하고
            
            if (_targetIsGearSlot)
            {
                _inventoryManager.SetGearItem(_targetGearSlot, _currentDragItem);
                Vector2 targetPos = new Vector2(_matchRT.sizeDelta.x, -_matchRT.sizeDelta.y) / 2;
                itemDrag.SetItemDragPos(targetPos, _matchRT.sizeDelta, _matchRT, 
                    null);
            }
            else
            {
                var targetPos = _targetInventory.MoveItem(item, _targetFirstIdx, _targetSlotRT);
                var itemSize = new Vector2(item.ItemCellCount.x, item.ItemCellCount.y)
                               * _uiManager.CellSize;
                itemDrag.SetItemDragPos(targetPos, itemSize, _targetInventory.ItemRT, _matchRT );
            }
        }
        else //인벤토리에서 이동한 경우.
        {
            var originInven = _invenMap[originInvenRT];
            var item = originInven.ItemDict[instanceID].item;
            originInven.RemoveItem(instanceID);

            if (item.IsStackable)
            {
                //var targetCellItem = _targetInventory.ItemDict[_targetCellItemID].item;
                //if (item.ItemData.ItemDataID == targetCellItem.ItemData.ItemDataID) //같은 아이템인지 검사
                //{
                    
               // }
            }
            
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

    private bool CheckGearSlot(RectTransform matchRT, InventoryItem dragItem)
    {
        if (!_gearSlotsMap[matchRT].IsEmpty) //Empty가 아닐 때
        {
            if (!dragItem.IsStackable) return false;  //stackable이 아니면 false 
            
            var targetCellItemID =  _gearSlotsMap[matchRT].InstanceID;
            var targetCellItem = _inventoryManager.ItemDict[targetCellItemID].item;
            if (dragItem.ItemData.ItemDataID != targetCellItem.ItemData.ItemDataID) return false; 
            //아이템 데이터 ID 비교, 다르면 false
            if(targetCellItem.CurrentStackAmount >= dragItem.MaxStackAmount) return false; //target이 최대 Stack이면 false
            
            _targetCellItemID = targetCellItemID;
            return true;
        }
        
        //Empty일 때...
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
            var rigID = _inventoryManager.ChestRigSlot.InstanceID;
            var rigItemType =  _inventoryManager.ItemDict[rigID].item.GearType;
            if(rigItemType == GearType.ArmoredRig) return false;
        }
        _targetGearSlot = _gearSlotsMap[matchRT];
        return true;//Slot Available      
    }

    private bool CheckInventory(RectTransform matchRT, Vector2 mousePos, InventoryItem dragItem)
    {
        var targetInven = _invenMap[matchRT];
        
        var itemCount = dragItem.ItemCellCount;
        
        //간소화?
        var (firstIdx, firstIdxPos, mathSlotRT ,status, cellCount, targetCellItemID) = 
            targetInven.CheckSlot(mousePos, itemCount, dragItem);
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
            if (dragItem.IsStackable) _targetCellItemID = targetCellItemID;
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
                //초기화 어떻게...?
                //itemData-> DragHandler...(init, 배치)
                //InventoryManager -> ?
                //Stackable Item....
                
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
                _itemDragHandlers.Add(pistolTest.InstanceID, itemDragHandlerTest);

                var test = new InventoryItem(itemDataTest);
                itemDragHandlerTest2.Init(test, this, _uiManager.transform);
                size = new Vector2(test.ItemCellCount.x, test.ItemCellCount.y) * _uiManager.CellSize;
                result = inventory.AddItem(test);
                if (result.isAvailable)
                {
                    itemDragHandlerTest2.SetItemDragPos(result.pos, size, inventory.ItemRT, 
                        _uiManager.LootSlotParent); 
                }
                _itemDragHandlers.Add(test.InstanceID, itemDragHandlerTest2);

                var bullet = new InventoryItem(bullet556TestData);
                itemDragHandlerTest5.Init(bullet, this, _uiManager.transform);
                size = new Vector2(bullet.ItemCellCount.x, bullet.ItemCellCount.y) * _uiManager.CellSize;
                result = inventory.AddItem(bullet);
                if (result.isAvailable)
                {
                    itemDragHandlerTest5.SetItemDragPos(result.pos, size, inventory.ItemRT,
                        _uiManager.LootSlotParent);
                }
                bullet.SetStackAmount(50); //50
                _itemDragHandlers.Add(bullet.InstanceID, itemDragHandlerTest5);
                itemDragHandlerTest5.SetStackAmountText(bullet.CurrentStackAmount);

                var bullet2 = new InventoryItem(bullet556TestData);
                itemDragHandlerTest6.Init(bullet2, this, _uiManager.transform);
                result = inventory.AddItem(bullet2);
                if (result.isAvailable)
                {
                    itemDragHandlerTest6.SetItemDragPos(result.pos, size, inventory.ItemRT,
                        _uiManager.LootSlotParent);
                }
                bullet2.SetStackAmount(20); //20
                _itemDragHandlers.Add(bullet2.InstanceID, itemDragHandlerTest6);
                itemDragHandlerTest6.SetStackAmountText(bullet2.CurrentStackAmount);
                
                var backpack = new InventoryItem(backpackTestData);
                itemDragHandlerTest3.Init(backpack, this, _uiManager.transform);
                size = _uiManager.BackpackSlotRT.sizeDelta;
                Vector2 pos =
                    new Vector2(_uiManager.BackpackSlotRT.sizeDelta.x, -_uiManager.BackpackSlotRT.sizeDelta.y) / 2;
                itemDragHandlerTest3.SetItemDragPos(pos, size,
                    _uiManager.BackpackSlotRT, null);
                _inventoryManager.SetGearItem(_inventoryManager.BackpackSlot, backpack);
                _itemDragHandlers.Add(backpack.InstanceID, itemDragHandlerTest3);
                
                var rig = new InventoryItem(rigTestData);
                itemDragHandlerTest4.Init(rig, this, _uiManager.transform);
                size = _uiManager.RigSlotRT.sizeDelta;
                pos = new Vector2(_uiManager.RigSlotRT.sizeDelta.x, -_uiManager.RigSlotRT.sizeDelta.y) / 2;
                itemDragHandlerTest4.SetItemDragPos(pos, size,
                    _uiManager.RigSlotRT, null);
                _inventoryManager.SetGearItem(_inventoryManager.ChestRigSlot, rig);
                _itemDragHandlers.Add(rig.InstanceID, itemDragHandlerTest4);
                
                break;
        }
    }

    private void HandleOnAddItemToInventory(GearType inventoryType, Vector2 pos, InventoryItem item)
        //인벤토리에 아이템 추가(아이템 줍기, 보상 받기 등)
    {
        var itemDragHandlerInstance = Instantiate(itemDragHandlerPrefab);
        Vector2 size = new Vector2(item.ItemCellCount.x, item.ItemCellCount.y) * _uiManager.CellSize;
        itemDragHandlerInstance.Init(item, this, _uiManager.transform);
        
        switch (inventoryType)
        {
            case GearType.ArmoredRig: 
            case GearType.UnarmoredRig:
                itemDragHandlerInstance.SetItemDragPos(pos, size, 
                    _inventoryManager.RigInventory.ItemRT, _uiManager.RigInvenParent);
                break;
            case GearType.Backpack:
                itemDragHandlerInstance.SetItemDragPos(pos, size, 
                    _inventoryManager.BackpackInventory.ItemRT, _uiManager.BackpackInvenParent);
                break;
            case GearType.None:
                //Unavailable 표시 -> UI Manager
                break;
        }
    }
}
