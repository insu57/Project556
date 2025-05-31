using System.Collections.Generic;
using UnityEngine;

public class InventoryUIPresenter
{
    private InventoryManager _inventoryManager;
    private UIManager _uiManager;
    
    private Dictionary<RectTransform, CellData> _leftGearSlots = new Dictionary<RectTransform, CellData>();
    private Dictionary<RectTransform, CellData> _midGearSlots = new Dictionary<RectTransform, CellData>();
    
    
    public InventoryUIPresenter(InventoryManager inventoryManager, UIManager uiManager)
    {
        _inventoryManager = inventoryManager;
        _uiManager = uiManager;

        //Left Panel Init
        inventoryManager.HeadwearSlot = new CellData(uiManager.HeadwearRT);
        _leftGearSlots[uiManager.HeadwearRT] = inventoryManager.HeadwearSlot;
        inventoryManager.EyewearSlot = new CellData(uiManager.EyewearRT);
        _leftGearSlots[uiManager.EyewearRT] = inventoryManager.EyewearSlot;
        inventoryManager.BodyArmorSlot = new CellData(uiManager.BodyArmorRT);
        _leftGearSlots[uiManager.BodyArmorRT] = inventoryManager.BodyArmorSlot;
        inventoryManager.PrimaryWeaponSlot = new CellData(uiManager.PWeaponRT);
        _leftGearSlots[uiManager.PWeaponRT] = inventoryManager.PrimaryWeaponSlot;
        inventoryManager.SecondaryWeaponSlot = new CellData(uiManager.SWeaponRT);
        _leftGearSlots[uiManager.SWeaponRT] = inventoryManager.SecondaryWeaponSlot;
        
        //Mid Panel Init
        inventoryManager.ChestRigSlot = new CellData(uiManager.RigRT);
        _midGearSlots[uiManager.RigRT] = inventoryManager.ChestRigSlot;
        inventoryManager.BackpackSlot = new CellData(uiManager.BackpackRT);
        _midGearSlots[uiManager.BackpackRT] = inventoryManager.BackpackSlot;
        
        for (int i = 0; i < 4; i++)
        {
            inventoryManager.PocketSlots[i] = new CellData(uiManager.PocketsRT[i]);
            _midGearSlots[uiManager.PocketsRT[i]] = inventoryManager.PocketSlots[i];
        }
        
        //Event
        _inventoryManager.OnCheckSlot += HandleOnCheckSlot;

    }

    private void HandleOnCheckSlot(Vector2 position)
    {
        //ItemDragger -> OnCheckSlot
        _uiManager.CheckRectTransform(position);
    }
}
