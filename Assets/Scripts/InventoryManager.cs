using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private float slotSize = 50f;
    public float SlotSize => slotSize;
    
    //Left Panel
    private InventoryItem _headwearData;
    public CellData HeadwearSlot { get; } = new(GearType.HeadWear);
    private InventoryItem _eyewearData;
    public CellData EyewearSlot { get; } = new(GearType.EyeWear);
    private InventoryItem _bodyArmorData;
    public CellData BodyArmorSlot { get; } = new(GearType.BodyArmor);
    private InventoryItem _primaryWeaponData;
    public CellData PrimaryWeaponSlot { get; } = new(GearType.Weapon);
    private InventoryItem _secondaryWeaponData;
    public CellData SecondaryWeaponSlot { get; } = new(GearType.Weapon);

    //Middle Panel
    private InventoryItem _chestRigData;
    public CellData ChestRigSlot { get; } = new(GearType.ArmoredRig);
    private InventoryItem _backpackData;
    public CellData BackpackSlot { get; } = new(GearType.Backpack);
    public Inventory BackpackInventory { get; private set; }

    public Inventory RigInventory { get; private set; }

    //Right Panel
    public Inventory LootInventory { get; private set; }

    public CellData[] PocketSlots { get; private set; } = new CellData[4];
    private Dictionary<Guid, CellData> _pocketItemDict = new();
    
    private UIManager _uiManager;

    public event Action<Vector2> OnCheckSlot;
    public event Action<GameObject, GearType> OnSetInventory; 
    
    //test
    [SerializeField] private GearData raidPack01Test;
    [SerializeField] private GearData rig01Test;
    [SerializeField] private GameObject crate01Test;
    [SerializeField] private BaseItemDataSO pistol1Test;
    [SerializeField] private BaseItemDataSO bandageTest;
    [SerializeField] private ItemDragger itemDragger01;
    public InventoryItem PistolTest { get; private set; }

    private void Awake()
    {
        
        
    }

    private void Start()
    {
        //test
        InventoryItem raidPackItem = new InventoryItem(raidPack01Test);
        //SetBackpack(raidPackItem);
        SetGear(raidPackItem);
        InventoryItem rig01TestItem = new InventoryItem(rig01Test);
        //SetRig(rig01TestItem);
        SetGear(rig01TestItem);
        //SetLootInventory(crate01Test);
        SetInventorySlot(crate01Test, GearType.None);

        
        //itemDragger01.Init(pistol1TestItem, _uiManager);

    }
    
    public void Init(UIManager uiManager)
    {
        _uiManager = uiManager;
        for (int i = 0; i < 4; i++) //PocketSlot 4
        {
            PocketSlots[i] = new CellData(GearType.None);
        }
        PistolTest = new InventoryItem(pistol1Test);
    }

    public void AddItemToRig(InventoryItem item)
    {
        
    }
    //Presenter 이벤트 처리...
    
    public bool CheckSlotAvailable(Vector2 position)
    {
        //좌측 인벤토리(장비 슬롯)
        //중간 인벤토리(리그, 가방 인벤토리, 주머니 슬롯 4개)
        //우측 인벤토리(적 시체, 상자)
        //1. 어떤 인벤토리인지...
        
        OnCheckSlot?.Invoke(position);
        //_uiManager.CheckRectTransform(position);
        return false;
    }

    public void SetInventorySlot(GameObject inventoryPrefab, GearType gearType)
    {
        OnSetInventory?.Invoke(inventoryPrefab, gearType);;
    }

    public void SetInventoryData(Inventory inventory, GearType gearType)
    {
        switch (gearType)
        {
            case GearType.ArmoredRig:
            case GearType.UnarmoredRig:
                RigInventory = inventory;
                break;
            case GearType.Backpack:
                BackpackInventory = inventory;
                break;
            case GearType.None:
                LootInventory = inventory;
                break;
        }
    }
    

    public void SetGear(InventoryItem item) //슬롯이 비었는가 -> SetGear(비었다고생각) 슬롯체크할 때 비었는지 종류맞는지 등 검사
    {
        GearType gearType = item.ItemData.GearType;
        GearData gearData;
        switch (gearType)
        {
            case GearType.HeadWear:
                _headwearData = item;
                break;
            case GearType.EyeWear:
                _eyewearData = item;
                break;
            case GearType.BodyArmor:
                _bodyArmorData = item;
                break;
            case GearType.ArmoredRig:
            case GearType.UnarmoredRig:    
                _chestRigData = item;
                gearData = item.ItemData as GearData;
                if (gearData) SetInventorySlot(gearData.SlotPrefab, gearType);
                break;
            case GearType.Backpack:
                _backpackData = item;
                gearData = item.ItemData as GearData;
                if (gearData) SetInventorySlot(gearData.SlotPrefab, gearType);
                break;
        }
    }

    public void SetWeapon(InventoryItem item, bool isPrimary)
    {
        if (isPrimary)
        {
            _primaryWeaponData = item;
        }
        else
        {
            _secondaryWeaponData = item;
        }
        
    }
    
}
