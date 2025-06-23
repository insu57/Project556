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
    public CellData HeadwearSlot { get; } = new(GearType.HeadWear, Vector2.zero);
    private InventoryItem _eyewearData;
    public CellData EyewearSlot { get; } = new(GearType.EyeWear, Vector2.zero);
    private InventoryItem _bodyArmorData;
    public CellData BodyArmorSlot { get; } = new(GearType.BodyArmor, Vector2.zero);
    private InventoryItem _primaryWeaponData;
    public CellData PrimaryWeaponSlot { get; } = new(GearType.Weapon, Vector2.zero);
    private InventoryItem _secondaryWeaponData;
    public CellData SecondaryWeaponSlot { get; } = new(GearType.Weapon, Vector2.zero);

    //Middle Panel
    private InventoryItem _chestRigData;
    public CellData ChestRigSlot { get; } = new(GearType.ArmoredRig, Vector2.zero);
    public CellData[] PocketSlots { get; private set; } = new CellData[4];
    private InventoryItem _backpackData;
    public CellData BackpackSlot { get; } = new(GearType.Backpack, Vector2.zero);
    public Inventory BackpackInventory { get; private set; }
    public Inventory RigInventory { get; private set; }

    //Right Panel
    public Inventory LootInventory { get; private set; }
    
    
    
    public Dictionary<Guid, (InventoryItem item, CellData cell)> ItemDict { get; } = new();
    public event Action<GameObject, InventoryItem> OnSetInventory;  //인벤토리 오브젝트, 인벤토리 타입(구분)
    public event Action<GearType, Vector2, RectTransform ,InventoryItem> OnAddItemToInventory; 
    //인벤토리 타입, ItemPos, ItemRT(ItemDragHandler의 부모) ,아이템데이터

    private void Awake()
    {
        for (int i = 0; i < 4; i++) //PocketSlot 4
        {
            PocketSlots[i] = new CellData(GearType.None, Vector2.zero);
        }
        
    }
    
    public void Init()
    {
        //_uiManager = uiManager;
    }

    //Presenter 이벤트 처리...
    
    private void SetInventorySlot(GameObject inventoryPrefab, InventoryItem item)
    {
        OnSetInventory?.Invoke(inventoryPrefab, item); //인벤토리 프리팹 생성/초기화
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
        ItemDict[item.InstanceID] = (item, gearSlot);
        gearSlot.SetEmpty(false, item.InstanceID);

        GearData gearData;
        switch (item.GearType)
        {
            case GearType.ArmoredRig:
            case GearType.UnarmoredRig:
            case GearType.Backpack:
                gearData = item.ItemData as GearData;
                if(gearData) SetInventorySlot(gearData.SlotPrefab, item);
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
            var (isAvailable, pos, itemRT) = BackpackInventory.AddItem(item);
            if (isAvailable)
            {
                OnAddItemToInventory?.Invoke(GearType.ArmoredRig, pos, itemRT,item);
                return;
            }
        }

        if (RigInventory)
        {
            var (isAvailable, pos, itemRT) = RigInventory.AddItem(item);
            if (isAvailable)
            {
                OnAddItemToInventory?.Invoke(GearType.Backpack, pos, itemRT,item);
                return;
            }
        }
        
        OnAddItemToInventory?.Invoke(GearType.None, Vector2.zero, null ,item);
        //unavailable 표시... -> UI?
    }
}
