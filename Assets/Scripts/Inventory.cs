using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Inventory: MonoBehaviour
{
    [Serializable]
    private struct SlotData
    {
        public RectTransform slotRT;
        public Vector2Int cellCount;
    }
    
    [SerializeField] private List<SlotData> slotDataList = new List<SlotData>();
    [SerializeField] private RectTransform itemRT;
    private RectTransform _inventoryRT;
    
    //private List<CellData> _cellData = new List<CellData>();
   // private List<RectTransform> _slotList = new List<RectTransform>();
    //private Dictionary<RectTransform, Vector2Int> 
    private Dictionary<RectTransform, (List<CellData> cells, Vector2Int size)> _slotDict 
        = new Dictionary<RectTransform, (List<CellData>, Vector2Int)>();
    private Dictionary<Guid, InventoryItem> _itemDict = new Dictionary<Guid, InventoryItem>();
    
    public float Width { private set; get; }
    public float Height { private set; get; }
    public RectTransform ItemRT => itemRT; //아이템 배치 RectTransform
    //스테이지에서 버리고 줍는것 생각하기...(인스턴스 생성관련...)
    private void Awake()
    {
        _inventoryRT = GetComponent<RectTransform>();
        Width = _inventoryRT.rect.width;
        Height = _inventoryRT.rect.height;

        foreach (var slotData in slotDataList)
        {
            var slotRT = slotData.slotRT;

            //CellData[] cellDataArray = slotDefinition.slotRT.
            List<CellData> cellDataList = new List<CellData>();
            for (int i = 0; i < slotRT.childCount; i++)
            {
                var child = slotRT.GetChild(i) as RectTransform;
                CellData cellData = new CellData(child);
                cellDataList.Add(cellData);
                //slot의 width height는???
            }
            _slotDict[slotRT] = (cellDataList, slotData.cellCount);
            //_slotDict.Add(slotRT,{cellDataList, slotData.cellCount});
            
            //CellData... slot - width&height
        }
    }

    private void Start()
    {
        Debug.Log(gameObject.name);
        foreach (var slot in _slotDict)
        {
            Debug.Log(slot.Key + " " + slot.Value);
        }
    }
    
}
