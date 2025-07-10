using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemInstance
{
    private RectTransform _slotRT;

    public Guid InstanceID { get; }
    public Vector2Int ItemCellCount { get ; private set; }

    public IItemData ItemData { get; }

    public GearType GearType => ItemData.GearType;
    public bool IsStackable => ItemData.IsStackable;
    public int MaxStackAmount => ItemData.MaxStackAmount;
    public int CurrentStackAmount { get; private set; }
    public bool IsRotated { get; private set; }
    public Inventory ItemInventory {get; private set;} 
    public float Durability { get; private set; }

    protected ItemInstance(IItemData itemData) //new...Init
    {
        ItemData = itemData;
        InstanceID = Guid.NewGuid();
        CurrentStackAmount = IsStackable ? 0 : 1; //Stackable 이면 0부터
        ItemCellCount = new Vector2Int(ItemData.ItemWidth, ItemData.ItemHeight);
        Durability = 100f;
    }

    public void SetItemInventory(Inventory itemInventory)
    {
        if(InstanceID != itemInventory.ItemInstanceID) return;
        ItemInventory = itemInventory;
    }
    
    public void AdjustStackAmount(int stackAmount)
    {
        CurrentStackAmount += stackAmount;
    }

    public void AdjustDurability(float durability)
    {
        Durability += durability;
    }

    public void SetStackAmount(int stackAmount)
    {
        CurrentStackAmount = stackAmount;
    }

    public void SetDurability(float durability)
    {
        Durability = durability;   
    }
    
    public void RotateItem()
    {
        IsRotated = !IsRotated;
        if (!IsRotated) //일반
        {
            ItemCellCount = new Vector2Int(ItemData.ItemWidth, ItemData.ItemHeight);
        }
        else //회전
        {
            ItemCellCount = new Vector2Int(ItemData.ItemHeight, ItemData.ItemWidth);       
        }
    }

    public static ItemInstance CreateItemInstance(IItemData itemData) //ItemInstance 생성(ItemPickup 등에서 사용) 
    {
        if (itemData is WeaponData weaponData) //Weapon
        {
            var weaponInstance = new WeaponInstance(weaponData);
            var randMagCount = Random.Range(1, weaponData.DefaultMagazineSize + 1); //무작위 탄 개수
            weaponInstance.SetMagazineCount(randMagCount);
         
            return weaponInstance;
        }
        
        var itemInstance = new ItemInstance(itemData);

        if (itemData.IsStackable) //stackable인 경우
        {
            var randStack = Random.Range(1, itemData.MaxStackAmount + 1);
            itemInstance.SetStackAmount(randStack);
        }

        return itemInstance;
    }
   
}