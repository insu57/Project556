using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUIPresenter
{
    private readonly InventoryManager _inventoryManager;
    private readonly UIManager _uiManager;

    private readonly Dictionary<RectTransform, CellData> _gearSlotsMap = new();
    private readonly Dictionary<RectTransform, Inventory> _invenMap = new();
    
    public InventoryUIPresenter(InventoryManager inventoryManager, UIManager uiManager)
    {
        _inventoryManager = inventoryManager;
        _uiManager = uiManager;

        //Left Panel Init
        inventoryManager.HeadwearSlot.SetCellRT(uiManager.HeadwearSlotRT);
        _gearSlotsMap[uiManager.HeadwearSlotRT] = inventoryManager.HeadwearSlot;

        inventoryManager.EyewearSlot.SetCellRT(uiManager.EyewearSlotRT);
        _gearSlotsMap[uiManager.EyewearSlotRT] = inventoryManager.EyewearSlot;

        inventoryManager.BodyArmorSlot.SetCellRT(uiManager.BodyArmorSlotRT);
        _gearSlotsMap[uiManager.BodyArmorSlotRT] = inventoryManager.BodyArmorSlot;

        inventoryManager.PrimaryWeaponSlot.SetCellRT(uiManager.PWeaponSlotRT);
        _gearSlotsMap[uiManager.PWeaponSlotRT] = inventoryManager.PrimaryWeaponSlot;

        inventoryManager.SecondaryWeaponSlot.SetCellRT(uiManager.SWeaponSlotRT);
        _gearSlotsMap[uiManager.SWeaponSlotRT] = inventoryManager.SecondaryWeaponSlot;

        //Mid Panel Init
        inventoryManager.ChestRigSlot.SetCellRT(uiManager.RigSlotRT);
        _gearSlotsMap[uiManager.RigSlotRT] = inventoryManager.ChestRigSlot;

        inventoryManager.BackpackSlot.SetCellRT(uiManager.BackpackSlotRT);
        _gearSlotsMap[uiManager.BackpackSlotRT] = inventoryManager.BackpackSlot;

        for (int i = 0; i < 4; i++)
        {
            inventoryManager.PocketSlots[i].SetCellRT(uiManager.PocketsSlotRT[i]);
            _gearSlotsMap[uiManager.PocketsSlotRT[i]] = inventoryManager.PocketSlots[i];
        }
        
        //Inventory 추가 **Inventory 초기 상태는 null!
        _invenMap[uiManager.RigInvenParent] = inventoryManager.RigInventory;
        _invenMap[uiManager.BackpackInvenParent] = inventoryManager.BackpackInventory;
        _invenMap[uiManager.LootSlotParent] = inventoryManager.LootInventory;
        
        
        
        //Event
        //InventoryManager
        _inventoryManager.OnSetInventory += HandleOnSetInventory;
        //UIManager
        _uiManager.OnCheckGearSlot += HandleOnCheckGearSlot;
        _uiManager.OnCheckInventoryCell += HandleOnCheckInventoryCell;


    }

    private void GetItemRectTransform(Vector2 originPos)
    {
        
    }
    
    private void HandleOnCheckGearSlot(RectTransform matchRT, RectTransform originInvenRT, Vector2 mousePos, Guid id)
    {
        //ItemDragger -> OnCheck
        //OnDrag-> 확인...
        //EndDrag -> 확정...
        if (_gearSlotsMap.ContainsKey(matchRT))
        {
            if(!_gearSlotsMap[matchRT].IsEmpty) return; 
            //return -> 불가능...표시(붉은색...)
            //OnDrag -> 임시저장, EndDrag -> 이동 or 복귀?

            InventoryItem item;
            if (!originInvenRT)
            {
                if(!_inventoryManager.ItemDict.ContainsKey(id)) Debug.LogError("Item not found...!: " + id);
                item = _inventoryManager.ItemDict[id];
            }
            else
            {
                if(!_invenMap.ContainsKey(originInvenRT)) Debug.LogError("Inventory not found...!: " + id);
                var inventory = _invenMap[originInvenRT];
                item = inventory.ItemDict[id];
            }

            if (item != null)
            {
                GearType gearType = item.GearType; //드래그 중인 아이템의 타입
                
                var slotCell = _gearSlotsMap[matchRT];  //체크 중인 슬롯의 Cell
                GearType slotGearType = slotCell.GearType; //슬롯의  타입
                
                if(slotGearType != gearType) return; //아이템과 슬롯의 타입이 다르면 불가...
                
                if (slotGearType is GearType.ArmoredRig) //방탄리그 - 방탄조끼의 제한... 개선 어떻게?
                {
                    if(!_inventoryManager.BodyArmorSlot.IsEmpty) return;
                }
                else if (slotGearType is GearType.BodyArmor)
                {
                    if (!_inventoryManager.ChestRigSlot.IsEmpty) //장착된 리그가 방탄 리그면 불가.
                    {
                        var rigID = _inventoryManager.ChestRigSlot.Id;
                        var rigItemType =  _inventoryManager.ItemDict[rigID].GearType;
                        if(rigItemType == GearType.ArmoredRig) return; 
                    }
                }
                _inventoryManager.SetGear(gearType, item);
                
                //_uiManager.MoveCurrentItemDragger(slotCell.ImagePosition, _uiManager.LeftPanelItemParentRT, null);
                //기존 슬롯(Cell) 비우기...
            }
            
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
                _inventoryManager.SetInventoryData(inventory, gearType);
                _invenMap[_uiManager.RigInvenParent] = inventory;
                break;
            case GearType.Backpack:
                inventory = _uiManager.SetBackpackSlot(inventoryPrefab);
                _inventoryManager.SetInventoryData(inventory, gearType);
                _invenMap[_uiManager.BackpackInvenParent] = inventory;
                break;
            case GearType.None: //LootInventory
                inventory = _uiManager.SetLootSlot(inventoryPrefab);
                _inventoryManager.SetInventoryData(inventory, gearType);
                _invenMap[_uiManager.LootSlotParent] = inventory;
                //test
                _uiManager.InitItemDragger(_inventoryManager.PistolTest
                    , _inventoryManager.LootInventory.ItemRT, _uiManager.LootSlotParent);
                inventory.ItemDict[_inventoryManager.PistolTest.Id] = _inventoryManager.PistolTest;
                //주웠을때... 
                //Inventory -> itemRT
                break;
        }
    }
}
