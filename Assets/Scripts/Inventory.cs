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
    
    private Dictionary<RectTransform, (List<CellData> cells, Vector2Int count)> _slotDict // Slot -> CellData List
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

    public (Vector2 firstIdxPos, SlotStatus status, Vector2 cellCount) CheckSlot(Vector2 mousePos, Vector2Int itemCellCount, Guid id)
    {
        foreach (var slotData in slotDataList)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(slotData.slotRT, mousePos)) continue;
            var matchSlot = slotData.slotRT;
            
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(matchSlot, mousePos, null,
                    out var localPoint)) continue;
            
            //Slot -> LocalPosition
            var (cells, slotCount) = _slotDict[matchSlot];
            GetFirstCellIdx(localPoint, slotCount, itemCellCount, out var firstIdx);

            if (firstIdx < 0) continue; //-1: SlotIdx out of bounds
            var firstX = firstIdx % slotCount.x;
            var firstY = firstIdx / slotCount.x;
            
            var firstIdxPos = cells[firstIdx].CellRT.position; //아이템 첫번째 슬롯의 Position(World)
            
            for (int y = 0; y < itemCellCount.y; y++) //cell체크...
            {
                for (int x = 0; x < itemCellCount.x; x++)
                {
                    var idx = firstIdx + x + y * slotCount.x;
                    if (firstX + x >= slotCount.x || firstY + y >= slotCount.y) //out of bounds
                    {
                        //Slot을 벗어난 아이템의 Cell을 계산
                        var overCell = Vector2Int.zero;
                        if (firstX + itemCellCount.x >= slotCount.x)
                        {
                            overCell.x = firstX + itemCellCount.x - slotCount.x;
                        }

                        if (firstY + itemCellCount.y >= slotCount.y)
                        {
                            overCell.y = firstY + itemCellCount.y - slotCount.y;
                        }
                        return (firstIdxPos, SlotStatus.Unavailable, itemCellCount - overCell);
                        //넘어간 만큼 줄이기
                    }
                    
                    if (!cells[idx].IsEmpty && cells[idx].Id != id) //empty가 아닐 때, ID가 동일하면 available.
                    {
                        return (firstIdxPos, SlotStatus.Unavailable, itemCellCount);
                    }
                }
            }
            return (firstIdxPos, SlotStatus.Available, itemCellCount);
        }
        return (Vector2.zero, SlotStatus.None, itemCellCount); //No Match Slot!
    }
    
    private void GetFirstCellIdx(Vector2 localPoint, Vector2Int slotSize, Vector2Int itemCellCount , out int firstIdx)
    {
        int width = slotSize.x;
        int height = slotSize.y;
        Vector2 firstPos = localPoint - new Vector2(itemCellCount.x * _cellSize / 2f , -itemCellCount.y * _cellSize / 2f) 
                             + new Vector2(_cellSize, -_cellSize)/2f;
        // 중심(아이템)에서 좌상단 MinPosition(가로세로 절반)(좌상단Cell, 첫번째Cell) + 해당 Cell의 Center
        // ***y하단 -> '-'(음수)
        
        int x = (int) (firstPos.x / _cellSize);
        int y = (int)(-firstPos.y / _cellSize);//y는 음수
        //x,y 체크...
        firstIdx = x + width * y;
        if (x < 0 || x >= slotSize.x || y < 0 || y >= slotSize.y) firstIdx = -1;
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
