using System;
using UnityEngine;

public class InventoryItem
{
    private readonly IItemData _itemData;
    private RectTransform _slotRT;
    private int _firstIdx;

    public Guid Id { get; }
    public Vector2Int ItemCellCount => new(_itemData.ItemWidth, _itemData.ItemHeight);
    //public RectTransform SlotRT => _slotRT;
    public int FirstIdx => _firstIdx;
    public IItemData ItemData => _itemData;
    public GearType GearType => _itemData.GearType;
    private Inventory _itemInventory;
    
    public InventoryItem(IItemData itemData) //new...Init
    {
        this._itemData = itemData;
        Id = Guid.NewGuid(); 
        //초기화따로...? pickUp 아이템 따로?
    }

    public void SetItemIdx(int idx)
    {
        _firstIdx = idx;
    }
   
}