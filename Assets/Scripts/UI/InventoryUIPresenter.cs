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

    public float SlotSize => _uiManager.SlotSize;
    //ItemDragger List?
    //test
    [SerializeField] private ItemDragHandler itemDragHandlerTest;
    [SerializeField] private BaseItemDataSO pistolTestData;
    private InventoryItem pistolTest;
    private void Awake()
    {
        _inventoryManager = GetComponent<InventoryManager>();
        //_inventoryManager.Init();
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
        //UIManager
        _uiManager.OnCheckGearSlot += HandleOnCheckGearSlot;
        _uiManager.OnCheckInventoryCell += HandleOnCheckInventoryCell;
        //ItemDragger...
        
       
    }

    private void OnDestroy()
    {
        //Event unsubscribe
        _inventoryManager.OnSetInventory -= HandleOnSetInventory;
        _uiManager.OnCheckGearSlot -= HandleOnCheckGearSlot;
        _uiManager.OnCheckInventoryCell -= HandleOnCheckInventoryCell;
    }

    public void InitItemDragHandler(ItemDragHandler itemDrag)//아이템 줍기 등에서 생성...맵에서 상자열 때 생성...
    {
        itemDrag.OnDragEvent += HandleOnDragItem;
        itemDrag.OnEndDragEvent += HandleOnEndDragItem;
    }

    public void OnDisableItemDragHandler(ItemDragHandler itemDrag)
    {
        itemDrag.OnDragEvent -= HandleOnDragItem;
        itemDrag.OnEndDragEvent -= HandleOnEndDragItem;
    }
    
    private void GetItemRectTransform(Vector2 originPos)
    {
        
    }

    private void HandleOnDragItem(ItemDragHandler itemDrag, Vector2 mousePos, Guid itemID)
    {
        var slotInfo = _uiManager.CheckItemSlot(mousePos);
        if (!slotInfo.matchSlot)
        {
            _uiManager.ClearShowAvailable();
            return;
        }
        if (slotInfo.isGearSlot)
        {
            var isAvailable = 
                CheckGearSlot(slotInfo.matchSlot, itemDrag.InventoryRT, mousePos, itemID); //Available
            //Debug.Log("IsAvailable: " + isAvailable);
            _uiManager.ShowSlotAvailable(isAvailable, slotInfo.matchSlot.position, slotInfo.matchSlot.sizeDelta);
        }
        else
        {
            CheckInventory(slotInfo.matchSlot, itemDrag.InventoryRT, mousePos, itemID);
        }
        //Debug.Log("MatchSlot: " + slotInfo.matchSlot.name);
        
        
    }

    private void HandleOnEndDragItem(ItemDragHandler itemDrag, Vector2 mousePos, Guid itemID)
    {
        Debug.Log("EndDragItem: " + itemDrag.name);
        _uiManager.ClearShowAvailable();
    }

    private bool CheckGearSlot(RectTransform matchRT, RectTransform originInvenRT, Vector2 mousePos, Guid id)
    {
        if(!_gearSlotsMap[matchRT].IsEmpty) return false;
        
        InventoryItem item;
        if (!originInvenRT)//기존 위치 인벤토리 체크
        {
            if(!_inventoryManager.ItemDict.ContainsKey(id)) Debug.LogError("Item not found...!: " + id);
            item = _inventoryManager.ItemDict[id]; //GearSlots Item Dict
        }
        else
        {
            if(!_invenMap.ContainsKey(originInvenRT)) Debug.LogError("Inventory not found...!: " + id);
            var inventory = _invenMap[originInvenRT];
            item = inventory.ItemDict[id]; //Inventory의 ItemDict
        }

        if (item == null)
        {
            Debug.LogError("Item not found...!: " + id);
            return false;
        }

        GearType gearType = item.GearType; //드래그 중인 아이템의 타입
                
        var slotCell = _gearSlotsMap[matchRT];  //체크 중인 슬롯의 Cell
        GearType slotGearType = slotCell.GearType; //슬롯의  타입
                
        if(slotGearType != gearType) return false; //아이템과 슬롯의 타입이 다르면 불가.
                
        if (gearType is GearType.ArmoredRig) //방탄리그 - 방탄조끼의 제한... 개선 어떻게?
        {
            if(!_inventoryManager.BodyArmorSlot.IsEmpty) return false; //방탄복이 장착된 상태면 방탄 리그 불가.
        }
        else if (gearType is GearType.BodyArmor) //방탄복일 때
        {
            if (_inventoryManager.ChestRigSlot.IsEmpty) return true; //Slot Available      
            //장착된 리그가 방탄 리그면 불가.
            var rigID = _inventoryManager.ChestRigSlot.Id;
            var rigItemType =  _inventoryManager.ItemDict[rigID].GearType;
            if(rigItemType == GearType.ArmoredRig) return false;
        }

        return true;//Slot Available      
    }

    private void CheckInventory(RectTransform matchRT, RectTransform originInvenRT, Vector2 mousePos, Guid id)
    {
        var targetInven = _invenMap[matchRT];
        var originInven = _invenMap[originInvenRT];
        var itemSize = originInven.ItemDict[id].SizeVector;
        targetInven.CheckSlot(mousePos, itemSize);
    }
    
    private void HandleOnCheckGearSlot(RectTransform matchRT, RectTransform originInvenRT, Vector2 mousePos, Guid id)
    {
        //ItemDragger -> OnCheck
        //OnDrag-> 확인...
        //EndDrag -> 확정...
        if (_gearSlotsMap.ContainsKey(matchRT))
        {
            if (!_gearSlotsMap[matchRT].IsEmpty) return;
            //return -> 불가능...표시(붉은색...)
            //OnDrag -> 임시저장, EndDrag -> 이동 or 복귀?

            InventoryItem item; //현재 드래그 중인 아이템
            if (!originInvenRT)//기존 위치 인벤토리 체크
            {
                if(!_inventoryManager.ItemDict.ContainsKey(id)) Debug.LogError("Item not found...!: " + id);
                item = _inventoryManager.ItemDict[id]; //GearSlots Item Dict
            }
            else
            {
                if(!_invenMap.ContainsKey(originInvenRT)) Debug.LogError("Inventory not found...!: " + id);
                var inventory = _invenMap[originInvenRT];
                item = inventory.ItemDict[id]; //Inventory의 ItemDict
            }

            if (item != null)
            {
                GearType gearType = item.GearType; //드래그 중인 아이템의 타입
                
                var slotCell = _gearSlotsMap[matchRT];  //체크 중인 슬롯의 Cell
                GearType slotGearType = slotCell.GearType; //슬롯의  타입
                
                if(slotGearType != gearType) return; //아이템과 슬롯의 타입이 다르면 불가.
                
                if (gearType is GearType.ArmoredRig) //방탄리그 - 방탄조끼의 제한... 개선 어떻게?
                {
                    if(!_inventoryManager.BodyArmorSlot.IsEmpty) return; //방탄복이 장착된 상태면 방탄 리그 불가.
                }
                else if (gearType is GearType.BodyArmor) //방탄복일 때
                {
                    if (!_inventoryManager.ChestRigSlot.IsEmpty) //장착된 리그가 방탄 리그면 불가.
                    {
                        var rigID = _inventoryManager.ChestRigSlot.Id;
                        var rigItemType =  _inventoryManager.ItemDict[rigID].GearType;
                        if(rigItemType == GearType.ArmoredRig) return; 
                    }
                }

                var imagePos = new Vector2(matchRT.sizeDelta.x, -matchRT.sizeDelta.y) / 2;
                _uiManager.MoveCurrentItemDragger(imagePos, matchRT, null);
                //GearSlot -> matchSlot의 자식으로 - 크기: 슬롯 크기에 따라
                //InvenSlot -> Inven.ItemRT의 자식으로 - 크기: Cell개수에 따라(1x2 -> 50x100)
                Debug.Log("Min: " + slotCell.MinPosition);
                Debug.Log("Max: " + slotCell.MaxPosition);
                //기존 슬롯(Cell) 비우기...
                // _inventoryManager.SetGear(gearType, item); ->EndDrag에서...->Event?
            }
            
            //ItemDragger -> TargetPosition설정.(bool isAvailable로 false면 원래 위치(PointerDownPos), true면 TargetPos
            //Width&Height(SizeDelta) -> 이것도 bool isGearSlot?? -> 그냥 Size Vector2로 하는게 나을듯. 
            //GearSlot 일 때 size, cell기준 Default size...? Cell -> 50*50(Cell 개수에 따라 증가) GearSlot 100*100, 
            //무기 400*100
            if (!originInvenRT)//InvenRT가 없음 -> GearSlot에서 이동
            {
               
                
                
                
                
                //SetGear -> 비울때... 크기도 맞춰야...
            }
            else
            {
                //RT -> Inventory??

                //Inven...
            }
            //기존 아이템...
            //IsEmpty = false, type 다름...->들고있는거 정보필요 타입, 크기
            //기존의 인벤토리 정보!!!!
            
           
        }
    }

    private void HandleOnCheckInventoryCell(RectTransform matchRT, RectTransform inventoryRT, Vector2 originPos, Guid id)
    {
        
    }

    private void HandleOnSetInventory(GameObject inventoryPrefab, GearType gearType)
    {
        Inventory inventory;
        switch (gearType)
        {
            case GearType.ArmoredRig:
            case GearType.UnarmoredRig:    
                inventory = _uiManager.SetRigSlot(inventoryPrefab);
                inventory.Init(_uiManager.SlotSize);
                _inventoryManager.SetInventoryData(inventory, gearType);
                _invenMap[_uiManager.RigInvenParent] = inventory;
                break;
            case GearType.Backpack:
                inventory = _uiManager.SetBackpackSlot(inventoryPrefab);
                inventory.Init(_uiManager.SlotSize);
                _inventoryManager.SetInventoryData(inventory, gearType);
                _invenMap[_uiManager.BackpackInvenParent] = inventory;
                break;
            case GearType.None: //LootInventory
                inventory = _uiManager.SetLootSlot(inventoryPrefab); 
                //문제...새로 생성, 이미 생성됨...구분하기?? UI-Data 분리?
                inventory.Init(_uiManager.SlotSize);
                _inventoryManager.SetInventoryData(inventory, gearType);
                _invenMap[_uiManager.LootSlotParent] = inventory;
                
                
                //test
                pistolTest = new InventoryItem(pistolTestData);
                itemDragHandlerTest.Init(pistolTest, this, _inventoryManager.LootInventory.ItemRT,
                    _uiManager.LootSlotParent, _uiManager.transform);

                inventory.ItemDict[pistolTest.Id] = pistolTest;
                //pistolTest.
                //주웠을때... 
                //Inventory -> itemRT
                break;
        }
    }
}
