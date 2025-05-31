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
    public CellData HeadwearSlot {get; set;}
    private InventoryItem _eyewearData;
    public CellData EyewearSlot { get; set; }
    private InventoryItem _bodyArmorData;
    public CellData BodyArmorSlot { get; set; }
    private InventoryItem _primaryWeaponData;
    public CellData PrimaryWeaponSlot { get; set; }
    private InventoryItem _secondaryWeaponData;
    public CellData SecondaryWeaponSlot { get; set; }

    //Middle Panel
    private InventoryItem _chestRigData;
    public CellData ChestRigSlot { get; set; }
    private InventoryItem _backpackData;
    public CellData BackpackSlot { get; set; }
    private Inventory _rigInventory;
    private Inventory _backpackInventory;
    //Right Panel
    private Inventory _lootInventory;

    public CellData[] PocketSlots { get; private set; } = new CellData[4];
    private Dictionary<Guid, CellData> _pocketItemDict = new Dictionary<Guid, CellData>();
    
    private UIManager _uiManager;

    public event Action<Vector2> OnCheckSlot; 
    
    //test
    [SerializeField] private GearData raidPack01Test;
    [SerializeField] private GearData rig01Test;
    [SerializeField] private GameObject crate01Test;
    [SerializeField] private BaseItemDataSO pistol1Test;
    [SerializeField] private BaseItemDataSO bandageTest;
    [SerializeField] private ItemDragger itemDragger01;
    
    private void Awake()
    {
        
    }

    private void Start()
    {
        //test
        InventoryItem raidPackItem = new InventoryItem(raidPack01Test);
        SetBackpack(raidPackItem);
        InventoryItem rig01TestItem = new InventoryItem(rig01Test);
        SetRig(rig01TestItem);
        SetLootInventory(crate01Test);

        InventoryItem pistol1TestItem = new InventoryItem(pistol1Test);
        itemDragger01.Init(pistol1TestItem, this, _uiManager.GetComponent<RectTransform>());
    }
    
    public void Init(UIManager uiManager)
    {
        _uiManager = uiManager;
        
    }

    public void AddItemToRig(InventoryItem item)
    {
        
    }
    //Presenter 이벤트 처리...
    public void SetRig(InventoryItem rig)
    {
        _chestRigData = rig;
        if (_chestRigData == null)
        {
            _rigInventory = _uiManager.SetRigSlot(null);
        }
        else
        {
            GearData rigData = rig.ItemData as GearData;
            _rigInventory = _uiManager.SetRigSlot(rigData);
        }
    }
    
    public void SetBackpack(InventoryItem backpack)
    {
        _backpackData = backpack;
        if (backpack == null)//슬롯 비우기
        {
            //_backpackSlot 슬롯 Empty로
            _backpackInventory = _uiManager.SetBackpackSlot(null);
        }
        else
        {
            GearData backpackData = backpack.ItemData as GearData;
            //슬롯 empty false로
            _backpackInventory = _uiManager.SetBackpackSlot(backpackData);
        }
    }

    public void SetLootInventory(GameObject lootInventoryPrefab) //나중에 LootDataClass로 바꾸는것 감안할것
    {
        //var lootInventory = lootInventoryPrefab.GetComponent<Inventory>();
        //_lootInventory = lootInventory;
        
        _lootInventory =  _uiManager.SetLootSlot(lootInventoryPrefab);
    }
    
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
    
    
}
