using System;
using UnityEngine;

public class InventoryItem
{
    private IItemData _itemData;
    private RectTransform _slotRT;
    private int _idx;

    public Guid Id { get; }
    public int Width => _itemData.ItemWidth;
    public int Height => _itemData.ItemHeight;
    public Vector2 SizeVector => new(_itemData.ItemWidth, _itemData.ItemHeight);
    public RectTransform SlotRT => _slotRT;
    public int Idx => _idx;
    public IItemData ItemData => _itemData;
    
    public InventoryItem(IItemData itemData)
    {
        this._itemData = itemData;
        Id = Guid.NewGuid(); 
        //초기화따로...? pickUp 아이템 따로?
        
    }

    public void MoveItem(RectTransform slotRT, int idx)
    {
        //슬롯 위치 변경
        _slotRT = slotRT;
        _idx = idx;
    }
}