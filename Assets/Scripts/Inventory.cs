using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory: MonoBehaviour
{
    [SerializeField] private float width;
    [SerializeField] private float height;

    //private SlotData[] _slotDataArray;
    //private Dictionary<Guid, InventoryItem> _inventoryItemDict = new Dictionary<Guid, InventoryItem>();
    private List<SlotData[]> _slotDataArrayList = new List<SlotData[]>();
    private List<Dictionary<Guid, InventoryItem>> _itemDataDictList = new List<Dictionary<Guid, InventoryItem>>();
    
    public void Init()
    {
        //
    }
}
