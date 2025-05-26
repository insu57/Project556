using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Inventory: MonoBehaviour
{
    [Serializable]
    private struct SlotInfo
    {
        public RectTransform slotRT;
        public Vector2Int cellCount;
    }
    
    [SerializeField] private List<SlotInfo> slotDefinitions = new List<SlotInfo>();
    [SerializeField] private RectTransform itemRT;
    private RectTransform _inventoryRT;
    
    private List<CellData> _cellData = new List<CellData>();
    private Dictionary<Guid, InventoryItem> _itemDict = new Dictionary<Guid, InventoryItem>();
    
    public float Width { private set; get; }
    public float Height { private set; get; }

    private void Awake()
    {
        _inventoryRT = GetComponent<RectTransform>();
        Width = _inventoryRT.rect.width;
        Height = _inventoryRT.rect.height;
        
    }
    
}
