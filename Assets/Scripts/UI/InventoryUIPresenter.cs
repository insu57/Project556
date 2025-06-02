using System.Collections.Generic;
using UnityEngine;

public class InventoryUIPresenter
{
    private readonly InventoryManager _inventoryManager;
    private readonly UIManager _uiManager;
    
    private Dictionary<RectTransform, CellData> _gearSlotsMap = new();
    private Dictionary<RectTransform, int> _inventoriesMap = new();
    
    private RectTransform _rigInventoryKey;
    private RectTransform _backpackInventoryKey;
    private RectTransform _lootInventoryKey;
    
    public InventoryUIPresenter(InventoryManager inventoryManager, UIManager uiManager)
    {
        _inventoryManager = inventoryManager;
        _uiManager = uiManager;

        //Left Panel Init
        inventoryManager.HeadwearSlot.SetCellRT(uiManager.HeadwearRT);
        _gearSlotsMap[uiManager.HeadwearRT] = inventoryManager.HeadwearSlot;
        
        inventoryManager.EyewearSlot.SetCellRT(uiManager.EyewearRT);
        _gearSlotsMap[uiManager.EyewearRT] = inventoryManager.EyewearSlot;
        
        inventoryManager.BodyArmorSlot.SetCellRT(uiManager.BodyArmorRT);
        _gearSlotsMap[uiManager.BodyArmorRT] = inventoryManager.BodyArmorSlot;
        
        inventoryManager.PrimaryWeaponSlot.SetCellRT(uiManager.PWeaponRT);
        _gearSlotsMap[uiManager.PWeaponRT] = inventoryManager.PrimaryWeaponSlot;
        
        inventoryManager.SecondaryWeaponSlot.SetCellRT(uiManager.SWeaponRT);
        _gearSlotsMap[uiManager.SWeaponRT] = inventoryManager.SecondaryWeaponSlot;
        
        //Mid Panel Init
        inventoryManager.ChestRigSlot.SetCellRT(uiManager.RigRT);
        _gearSlotsMap[uiManager.RigRT] = inventoryManager.ChestRigSlot;
        
        inventoryManager.BackpackSlot.SetCellRT(uiManager.BackpackRT);
        _gearSlotsMap[uiManager.BackpackRT] = inventoryManager.BackpackSlot;
        
        for (int i = 0; i < 4; i++)
        {
            inventoryManager.PocketSlots[i].SetCellRT(uiManager.PocketsRT[i]);
            _gearSlotsMap[uiManager.PocketsRT[i]] = inventoryManager.PocketSlots[i];
        }
        //Inventory 추가
        _rigInventoryKey = uiManager.RigInvenParent;
        _backpackInventoryKey = uiManager.PackInvenParent;
        _lootInventoryKey = uiManager.LootSlotParent;
        
        //Event
        //InventoryManager
        _inventoryManager.OnCheckSlot += HandleOnCheckSlot;
        _inventoryManager.OnSetInventory += HandleOnSetInventory;
        //UIManager
        _uiManager.OnCheckRectTransform += HandleOnCheckRectTransform;
        
        
    }

    private void HandleOnCheckSlot(Vector2 position)
    {
        //ItemDragger -> OnCheckSlot
        _uiManager.CheckRectTransform(position);
    }

    private void HandleOnCheckRectTransform(bool isGearSlot, RectTransform matchRT)
    {
        //ItemDragger -> OnCheckRectTransform
        Debug.Log("HandleOnCheckRT!: "+"isGearSlot: "+isGearSlot+" matchRT: "+matchRT+"");
        if (isGearSlot)
        {
            if (_gearSlotsMap.ContainsKey(matchRT))
            {
                Debug.Log("GearSlot: "+_gearSlotsMap[matchRT].GearType + " "  + _gearSlotsMap[matchRT].IsEmpty);
            }
        }
        else
        {
            if (_inventoriesMap.ContainsKey(matchRT))
            {
                //Debug.Log(_inventoriesSlots[matchRT].name);
            }
        }
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
                break;
            case GearType.Backpack:
                inventory = _uiManager.SetBackpackSlot(inventoryPrefab);
                _inventoryManager.SetInventoryData(inventory, gearType);
                break;
            case GearType.None: //LootInventory
                inventory = _uiManager.SetLootSlot(inventoryPrefab);
                _inventoryManager.SetInventoryData(inventory, gearType);
                
                //test

                var itemDragger = _uiManager.InitItemDragger(_inventoryManager.PistolTest, _lootInventoryKey); 
                //주웠을때...
                
                itemDragger.SetInventoryRT(_inventoryManager.LootInventory.ItemRT);
                //ItemDragger설정...
                //Inventory -> itemRT
                break;
        }
    }
}
