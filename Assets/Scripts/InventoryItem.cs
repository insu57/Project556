using System;
using UnityEngine;

public class InventoryItem
{
    private readonly IItemData _itemData;
    private RectTransform _slotRT;

    public Guid Id { get; }
    public Vector2Int ItemCellCount => new(_itemData.ItemWidth, _itemData.ItemHeight);

    public IItemData ItemData => _itemData;
    public GearType GearType => _itemData.GearType;
    public bool IsStackable => _itemData.IsStackable;
    public int MaxStackAmount => _itemData.MaxStackAmount;
    public int CurrentStackAmount { get; private set; }
    //private Inventory _itemInventory;
    
    public InventoryItem(IItemData itemData) //new...Init
    {
        this._itemData = itemData;
        Id = Guid.NewGuid();
        //초기화따로...? pickUp 아이템 따로?
    }

    public void SetStackAmount(int stackAmount)
    {
        CurrentStackAmount += stackAmount;
        // 0이하? -> 처리
    }
   
}