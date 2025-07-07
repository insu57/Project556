using System;
using UnityEngine;

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
    
    public ItemInstance(IItemData itemData) //new...Init
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
    
    public void ChangeStackAmount(int stackAmount)
    {
        CurrentStackAmount += stackAmount;
    }

    public void ChangeDurability(float durability)
    {
        Durability += durability;
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