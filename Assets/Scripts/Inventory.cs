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
    
    [SerializeField] private List<SlotData> slotDataList = new();
    [SerializeField, Space] private RectTransform itemRT;
    private RectTransform _inventoryRT;
    
    private Dictionary<RectTransform, (List<CellData> cells, Vector2Int size)> _slotDict // Slot -> CellData List
        = new();

    public float Width { private set; get; }
    public float Height { private set; get; }
    private float _cellSize;
    public RectTransform InventoryRT => _inventoryRT;
    public RectTransform ItemRT => itemRT; //아이템 배치 RectTransform
    public Dictionary<Guid, InventoryItem> ItemDict { get; } = new();

    //스테이지에서 버리고 줍는것 생각하기...(인스턴스 생성관련...)

    public void Init(float slotSize)
    {
        _cellSize = slotSize;
    }
    
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
                CellData cellData = new CellData(GearType.None);
                cellData.SetCellRT(child);
                cellDataList.Add(cellData);
            }
            _slotDict[slotRT] = (cellDataList, slotData.cellCount);
            //_slotDict.Add(slotRT,{cellDataList, slotData.cellCount});
            
            //CellData... slot - width&height
        }
    }

    private void Start()
    {
        //Debug.Log(gameObject.name);
        foreach (var slot in _slotDict)
        {
            //Debug.Log(slot.Key + " " + slot.Value);
        }
    }

    public void CheckSlot(Vector2 mousePos, Vector2Int itemSize, out Vector2 firstIdxPos)
    {
        Vector3 cellPos = Vector3.zero;
        foreach (var slotData in slotDataList)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(slotData.slotRT, mousePos))
            {
                var matchSlot = slotData.slotRT;
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(matchSlot, mousePos, null,
                        out var localPoint)) continue;
                //Slot -> Pos...
                var cellsInfo = _slotDict[matchSlot];
                var cells = cellsInfo.cells;
                var slotSize = cellsInfo.size;
                GetCellIdx(localPoint, slotSize, itemSize, out var idx, out var firstPos);
                //firstIdxPos = firstPos;//Idx가 안맞는듯...수정필요
                if (idx >= 0)
                {
                   cellPos = cells[idx].CellRT.position;
                   //firstIdxPos = cellPos;
                }
                
                //localPoint, 
                Debug.Log(this.name + ": " + matchSlot+ " firstPos: " + firstPos +" idx: "+idx);
                //Firs.
            }
        }
        //firstIdxPos = Vector2.zero;
        firstIdxPos = cellPos;
    }

    public void GetCellIdx(Vector2 localPoint, Vector2Int slotSize, Vector2Int itemSize ,out int idx, out Vector2 firstPos)
    {
        int width = slotSize.x;
        int height = slotSize.y;
        Vector2 firstPoint = localPoint - new Vector2(itemSize.x * _cellSize / 2f , -itemSize.y * _cellSize / 2f) 
                             + new Vector2(_cellSize, -_cellSize)/2f;
        Debug.Log("local: " + localPoint + " first: " + firstPoint); //기준을 어디로..?
        int x = (int) (firstPoint.x / _cellSize);
        int y = (int)(-firstPoint.y / _cellSize);//y는 음수
       
        //기존 InventoryUI코드 참조... 아이템 중간 Pos -> MinCell MaxCell...
        idx = x + width * y;
        if (idx < 0 || idx > width * height) idx = -1;
        firstPos = firstPoint;
    }

    public void MoveItem(Vector2 mousePos)
    {
        
    }
    
    public void AddItem(InventoryItem item, RectTransform slotRT, int idx)
    {
        
        item.MoveItem(slotRT, idx);
        //좌상단부터 슬롯의 빈 곳(가능한 위치)에 넣기 -> 추후 추가
        
        
    }
    
}
