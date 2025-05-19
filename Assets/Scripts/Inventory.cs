using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private Dictionary<Guid, InventoryItem> _itemDataDictionary = new Dictionary<Guid, InventoryItem>();

    public void Init()
    {
        
    }   
}
