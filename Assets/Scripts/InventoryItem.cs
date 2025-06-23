using System;
using UnityEngine;

public class InventoryItem
{
    private readonly IItemData _itemData;
    private RectTransform _slotRT;

    public Guid InstanceID { get; }
    public Vector2Int ItemCellCount { get ; private set; }

    public IItemData ItemData => _itemData;
    public GearType GearType => _itemData.GearType;
    public bool IsStackable => _itemData.IsStackable;
    public int MaxStackAmount => _itemData.MaxStackAmount;
    public int CurrentStackAmount { get; private set; }
    public bool IsRotated { get; private set; }
    public Inventory ItemInventory {get; private set;}
    
    public InventoryItem(IItemData itemData) //new...Init
    {
        this._itemData = itemData;
        InstanceID = Guid.NewGuid();
        CurrentStackAmount = IsStackable ? 0 : 1; //Stackable 이면 0부터
        ItemCellCount = new Vector2Int(_itemData.ItemWidth, _itemData.ItemHeight);
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
        // 0이하? -> 처리
    }

    public void RotateItem()
    {
        IsRotated = !IsRotated;
        if (!IsRotated) //일반
        {
            ItemCellCount = new Vector2Int(_itemData.ItemWidth, _itemData.ItemHeight);
        }
        else //회전
        {
            ItemCellCount = new Vector2Int(_itemData.ItemHeight, _itemData.ItemWidth);       
        }
    }
   
}