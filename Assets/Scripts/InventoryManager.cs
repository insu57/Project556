using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
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
    public Dictionary<Guid, (InventoryItem item, CellData cell)> ItemDict { get; } = new();
    public event Action<GameObject, GearType> OnSetInventory;  //인벤토리 오브젝트, 인벤토리 타입(구분)
    public event Action<GearType, Vector2, InventoryItem> OnAddItemToInventory; //인벤토리 타입, ItemPos, 아이템데이터
    
    //test
    [SerializeField] private GearData raidPack01Test;
    [SerializeField] private GearData rig01Test;
    [SerializeField] private GameObject crate01Test;
    [SerializeField] private BaseItemDataSO pistol1Test;
    [SerializeField] private BaseItemDataSO bandageTest;
    [SerializeField] private ItemDragHandler itemDragger01;
    public InventoryItem PistolTest { get; private set; }

    private void Awake()
    {
        for (int i = 0; i < 4; i++) //PocketSlot 4
        {
            PocketSlots[i] = new CellData(GearType.None);
        }
        
        PistolTest = new InventoryItem(pistol1Test);
    }

    private void Start()
    {
        //test
        SetInventorySlot(crate01Test, GearType.None);
    }
    
    public void Init()
    {
        //_uiManager = uiManager;
    }

    //Presenter 이벤트 처리...
    
    private void SetInventorySlot(GameObject inventoryPrefab, GearType gearType)
    {
        OnSetInventory?.Invoke(inventoryPrefab, gearType); //인벤토리 프리팹 생성/초기화
    }

    public void SetInventoryData(Inventory inventory, GearType gearType)
    {
        //인벤토리 설정
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

    public void SetGearItem(CellData gearSlot, InventoryItem item)
    {
        //GearDict[gearSlot] = item; //아이템을 제거할 때는?
        ItemDict[item.InstanceID] = (item, gearSlot);
        gearSlot.SetEmpty(false, item.InstanceID);

        GearData gearData;
        switch (item.GearType)
        {
            case GearType.ArmoredRig:
            case GearType.UnarmoredRig:
            case GearType.Backpack:
                gearData = item.ItemData as GearData;
                if(gearData) if (gearData) SetInventorySlot(gearData.SlotPrefab, item.GearType);
                break;
        }
        //장비 능력치, 무기 설정
        //inventory -> player??
    }

    public void RemoveGearItem(CellData gearSlot, Guid itemID)
    {
        ItemDict.Remove(itemID);
        gearSlot.SetEmpty(true, itemID);
        //Inventory있는 경우...처리
        //장비 능력치 등 처리
    }

    public void AddItemToInventory(InventoryItem item)
    {
        //item따라...
        //DragHandler -> ObjectPooling(active, inactive 로 구분, inactive인 경우 리셋...)
        //DragHandler -> inventoryUIPresenter에서
        //Inventory...없으면 null, backpack -> rig 순. 
        //LootInventory -> 맵 초기화에서...일괄
        
        if (BackpackInventory)
        {
            var (isAvailable, pos) = BackpackInventory.AddItem(item);
            if (isAvailable)
            {
                OnAddItemToInventory?.Invoke(GearType.ArmoredRig, pos, item);
                return;
            }
        }

        if (RigInventory)
        {
            var (isAvailable, pos) = RigInventory.AddItem(item);
            if (isAvailable)
            {
                OnAddItemToInventory?.Invoke(GearType.Backpack, pos, item);
                return;
            }
        }
        
        OnAddItemToInventory?.Invoke(GearType.None, Vector2.zero, item);
        //unavailable 표시... -> UI?
    }
}
