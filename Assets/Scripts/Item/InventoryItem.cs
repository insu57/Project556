using System;
using UnityEngine;

public class InventoryItem
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
    public int CurrentMagazineCount { get; private set; }
    public int MaxMagazineCount { get; private set; }
    //장탄수(무기)
    
    public InventoryItem(IItemData itemData) //new...Init
    {
        ItemData = itemData;
        InstanceID = Guid.NewGuid();
        CurrentStackAmount = IsStackable ? 0 : 1; //Stackable 이면 0부터
        ItemCellCount = new Vector2Int(ItemData.ItemWidth, ItemData.ItemHeight);
        Durability = 100f;
        
        if (itemData is not WeaponData weaponData) return;
        CurrentMagazineCount = weaponData.DefaultMagazineSize;
        MaxMagazineCount = weaponData.DefaultMagazineSize;

        //초기화따로...? pickUp 아이템 따로?
    }

    public void SetItemInventory(Inventory itemInventory)
    {
        if(InstanceID != itemInventory.ItemInstanceID) return;
        ItemInventory = itemInventory;
    }
    
    public void AddStackAmount(int stackAmount)
    {
        CurrentStackAmount += stackAmount;
    }

    public void DecreaseDurability(float damage)
    {
        Durability -= damage;
    }

    //WeaponInstance????
    public void UseAmmo() 
    {
        if (ItemData is not WeaponData) return;
        CurrentMagazineCount--;
    }

    public void ReloadAmmo() //주머니 탄 소모...
    {
        if (ItemData is not WeaponData weaponData) return;
        if (weaponData.IsOpenBolt)
            CurrentMagazineCount = MaxMagazineCount;
        else CurrentMagazineCount = MaxMagazineCount + 1;
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
   
}