using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private float slotSize = 50f;
    public float SlotSize => slotSize;
    
    //Left Panel
    private InventoryItem _headwearData;
    private CellData _headwearSlot;
    private InventoryItem _bodyArmorData;
    private CellData _bodyArmorSlot;
    private InventoryItem _primaryWeaponData;
    private CellData _primaryWeaponSlot;
    private InventoryItem _secondaryWeaponData;
    private CellData _secondaryWeaponSlot;
    //Middle Panel
    private InventoryItem _chestRigData;
    private CellData _chestRigSlot;
    private InventoryItem _backpackData;
    private CellData _backpackSlot;
    private Inventory _rigInventory;
    private Inventory _backpackInventory;
    //Right Panel
    private Inventory _lootInventory;
    
    private CellData[] _pocketSlots = new CellData[4];
    private Dictionary<Guid, CellData> _pocketItemDict = new Dictionary<Guid, CellData>();
    
    
    private UIManager _uiManager;
    
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

    public bool CheckSlotAvailable()
    {
        //좌측 인벤토리(장비 슬롯)
        //중간 인벤토리(리그, 가방 인벤토리, 주머니 슬롯 4개)
        //우측 인벤토리(적 시체, 상자)
        //1. 어떤 인벤토리인지...
        return false;
    }
    
    private void Start()
    {
        //test
        InventoryItem raidPackItem = new InventoryItem(raidPack01Test);
        SetBackpack(raidPackItem);
    }
}
