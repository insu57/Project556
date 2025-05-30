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
        public bool isGearSlot;
        public GearType gearType;
    }
    
    [SerializeField] private List<SlotData> slotDataList = new List<SlotData>();
    [SerializeField, Space] private RectTransform itemRT;
    private RectTransform _inventoryRT;
    
    //private List<CellData> _cellData = new List<CellData>();
   // private List<RectTransform> _slotList = new List<RectTransform>();
    //private Dictionary<RectTransform, Vector2Int> 
    private Dictionary<RectTransform, (List<CellData> cells, Vector2Int size)> _slotDict // Slot -> CellData List
        = new Dictionary<RectTransform, (List<CellData>, Vector2Int)>();
    private Dictionary<Guid, InventoryItem> _itemDict = new Dictionary<Guid, InventoryItem>();
    
    public float Width { private set; get; }
    public float Height { private set; get; }
    public RectTransform InventoryRT => _inventoryRT;
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
                //cellData.Init(child);
                cellDataList.Add(cellData);
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

    public void CheckSlot(Vector2 mousePos)
    {
        foreach (var slotData in slotDataList)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(slotData.slotRT, mousePos))
            {
                Debug.Log(slotData);
            }
        }
    }

    public void AddItem(InventoryItem item, RectTransform slotRT, int idx)
    {
        //어디에? -> 일단은 지정해서...
        //test?
        //var slotRT01 = slotDataList[0].slotRT;
        //idx -> 아이템 좌상단 기준...
        //_slotDict[slotRT].c
        item.MoveItem(slotRT, idx);
        
        //for
        //좌상단부터 슬롯의 빈 곳(가능한 위치)에 넣기 -> 추후 추가
        
        
    }
    
}
