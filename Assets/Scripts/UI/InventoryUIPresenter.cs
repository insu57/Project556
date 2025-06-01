using System.Collections.Generic;
using UnityEngine;

public class InventoryUIPresenter
{
    private InventoryManager _inventoryManager;
    private UIManager _uiManager;
    
    private Dictionary<RectTransform, CellData> _gearSlots = new Dictionary<RectTransform, CellData>();
    private Dictionary<RectTransform, Inventory> _inventoriesSlots = new Dictionary<RectTransform, Inventory>();
    private RectTransform _rigInventoryKey;
    private RectTransform _backpackInventoryKey;
    private RectTransform _lootInventoryKey;
    
    public InventoryUIPresenter(InventoryManager inventoryManager, UIManager uiManager)
    {
        _inventoryManager = inventoryManager;
        _uiManager = uiManager;

        //Left Panel Init
        inventoryManager.HeadwearSlot = new CellData(uiManager.HeadwearRT, GearType.HeadWear);
        //inventoryManager.HeadwearSlot.SetGearSlot(G);
        _gearSlots[uiManager.HeadwearRT] = inventoryManager.HeadwearSlot;
        inventoryManager.EyewearSlot = new CellData(uiManager.EyewearRT, GearType.EyeWear);
        _gearSlots[uiManager.EyewearRT] = inventoryManager.EyewearSlot;
        inventoryManager.BodyArmorSlot = new CellData(uiManager.BodyArmorRT, GearType.ArmorVest);
        _gearSlots[uiManager.BodyArmorRT] = inventoryManager.BodyArmorSlot;
        inventoryManager.PrimaryWeaponSlot = new CellData(uiManager.PWeaponRT, GearType.Weapon);
        _gearSlots[uiManager.PWeaponRT] = inventoryManager.PrimaryWeaponSlot;
        inventoryManager.SecondaryWeaponSlot = new CellData(uiManager.SWeaponRT, GearType.Weapon);
        _gearSlots[uiManager.SWeaponRT] = inventoryManager.SecondaryWeaponSlot;
        
        //Mid Panel Init
        inventoryManager.ChestRigSlot = new CellData(uiManager.RigRT, GearType.ArmoredRig);
        _gearSlots[uiManager.RigRT] = inventoryManager.ChestRigSlot;
        inventoryManager.BackpackSlot = new CellData(uiManager.BackpackRT, GearType.Backpack);
        _gearSlots[uiManager.BackpackRT] = inventoryManager.BackpackSlot;
        for (int i = 0; i < 4; i++)
        {
            inventoryManager.PocketSlots[i] = new CellData(uiManager.PocketsRT[i], GearType.None);
            _gearSlots[uiManager.PocketsRT[i]] = inventoryManager.PocketSlots[i];
        }
        //Inventory 추가
        _inventoriesSlots[uiManager.RigInvenParent] = _inventoryManager.RigInventory;
        _rigInventoryKey = uiManager.RigInvenParent;
        _inventoriesSlots[uiManager.PackInvenParent] = _inventoryManager.BackpackInventory;;
        _backpackInventoryKey = uiManager.PackInvenParent;
        _inventoriesSlots[uiManager.LootSlotParent] = _inventoryManager.LootInventory;
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
            if (_gearSlots.ContainsKey(matchRT))
            {
                Debug.Log("GearSlot: "+_gearSlots[matchRT].GearType + " "  + _gearSlots[matchRT].IsEmpty);
            }
        }
        else
        {
            if (_inventoriesSlots.ContainsKey(matchRT))
            {
                //Debug.Log(_inventoriesSlots[matchRT].name);
            }
        }
    }

    private void HandleOnSetInventory(GameObject inventoryPrefab, GearType gearType)
    {
        switch (gearType)
        {
            case GearType.ArmoredRig:
                //_inventoryManager.RigInventory = _uiManager.SetRigSlot(inventoryPrefab); //어떻게 더 개선??
                _inventoriesSlots[_rigInventoryKey] = _uiManager.SetRigSlot(inventoryPrefab);
                //_rigInventory = 
                break;
            case GearType.Backpack:
                _uiManager.SetBackpackSlot(inventoryPrefab);
                break;
            case GearType.None: //LootInventory
                _uiManager.SetLootSlot(inventoryPrefab);
                break;
        }
    }
}
