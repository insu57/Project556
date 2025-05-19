using System;
using UnityEngine;

public class InventoryItem
{
    private IItemData _itemData;

    private int _idx;

    public Guid Id { get; }
    public int Width => _itemData.ItemWidth;
    public int Height => _itemData.ItemHeight;
    public Vector2 SizeVector => new(_itemData.ItemWidth, _itemData.ItemHeight);
    public int Idx => _idx;
    
    public InventoryItem(IItemData itemData)
    {
        this._itemData = itemData;
        Id = Guid.NewGuid(); 
        //초기화따로...? pickUp 아이템 따로?
    }
    public void MoveItem(int idx)
    {
        //슬롯 위치 변경
        _idx = idx;
    }
}