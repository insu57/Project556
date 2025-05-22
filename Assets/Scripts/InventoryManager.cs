using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class InventoryManager : MonoBehaviour
{
    private InventoryItem _headwearData;
    private SlotData _headwearSlot;
    private InventoryItem _bodyArmorData;
    private SlotData _bodyArmorSlot;
    private InventoryItem _chestRigData;
    private SlotData _chestRigSlot;
    private InventoryItem _primaryWeaponData;
    private SlotData _primaryWeaponSlot;
    private InventoryItem _secondaryWeaponData;
    private SlotData _secondaryWeaponSlot;
    private InventoryItem _backpackData;
    private SlotData _backpackSlot;

    private SlotData[] _pocketSlots = new SlotData[4];
    private Dictionary<Guid, SlotData> _pocketItemDict = new Dictionary<Guid, SlotData>();
    
    private UIManager _uiManager;

    private Inventory _rigInventory;
    private Inventory _backpackInventory;
    //private Inventory 
    //test
    [SerializeField] private GearData raidPack01Test;
    
    public void Init(UIManager uiManager)
    {
        _uiManager = uiManager;
    }

    public void SetBackpack(InventoryItem backpack)
    {
        _backpackData = backpack;
        if (backpack == null)//슬롯 비우기
        {
            //_backpackSlot 슬롯 Empty로
            _uiManager.SetBackpackSlot(null);
        }
        else
        {
            GearData backpackData = backpack.ItemData as GearData;
            //슬롯 empty false로
            _uiManager.SetBackpackSlot(backpackData);
        }
    }
    
    private void Start()
    {
        //test
        InventoryItem raidPackItem = new InventoryItem(raidPack01Test);
        SetBackpack(raidPackItem);
    }
}
