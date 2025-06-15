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
    
    private readonly Dictionary<RectTransform, (List<CellData> cells, Vector2Int slotCount)> _slotDict 
        = new(); // Slot -> CellData List
    //Slot 리팩터링????? class??? 단순하게 SlotRT, idx DragHandler에...
    //드래그할 때 기존 슬롯 정보에 접근해서 비워야함....

    public float Width { private set; get; }
    public float Height { private set; get; }
    private float _cellSize;
    public RectTransform InventoryRT => _inventoryRT;
    public RectTransform ItemRT => itemRT; //아이템 배치 RectTransform
    private RectTransform _matchSlotRT;
    public Dictionary<Guid, (InventoryItem item, RectTransform slotRT, int firstIdx)> ItemDict { get; } = new();
    //기존 슬롯->아이템 정보...
    //스테이지에서 버리고 줍는것 생각하기...(인스턴스 생성관련...)

    public void Init(float cellSize)
    {
        _cellSize = cellSize; //CellSize 개선... (UI/데이터 나누기...)
    }
    
    private void Awake()
    {
        _inventoryRT = GetComponent<RectTransform>();
        Width = _inventoryRT.rect.width;
        Height = _inventoryRT.rect.height;

        foreach (var slotData in slotDataList)
        {
            var slotRT = slotData.slotRT;
            
            List<CellData> cellDataList = new List<CellData>();
            for (int i = 0; i < slotRT.childCount; i++)
            {
                var child = slotRT.GetChild(i) as RectTransform;
                CellData cellData = new CellData(GearType.None);
                cellData.SetCellRT(child);
                cellDataList.Add(cellData);
            }
            _slotDict[slotRT] = (cellDataList, slotData.cellCount);
        }
    }

    private void Start()
    {
        
    }
    
    public (Vector2 firstIdxPos, SlotStatus status, Vector2 cellCount) CheckSlot
        (Vector2 mousePos, Vector2Int itemCellCount, Guid id)
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
            _matchSlotRT = slotData.slotRT;
            return (firstIdxPos, SlotStatus.Available, itemCellCount);
        }
        return (Vector2.zero, SlotStatus.None, itemCellCount); //No Match Slot!
    }
    
    private void GetFirstCellIdx(Vector2 localPoint, Vector2Int slotSize, Vector2Int itemCellCount , out int firstIdx)
    {
        int width = slotSize.x;
        //int height = slotSize.y;
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
    
    //LootInven 아이템 초기화?
    public (bool isAvailable, Vector2 pos) AddItem(InventoryItem item)
    {
        //중첩for문 개선???
        Canvas.ForceUpdateCanvases();
        foreach (var slotData in slotDataList)
        {
            var slotCell = _slotDict[slotData.slotRT];
            var cells = slotCell.cells;
            var slotCount = slotCell.slotCount;
            var itemCount = item.ItemCellCount;

            for (int h = 0; h < slotCount.y; h++)
            {
                for (int w = 0; w < slotCount.x; w++)
                {
                    int firstIdx = w + h * slotCount.x; //빈 슬롯 체크 시작 Idx

                    bool isAvailable = true;
                    for (int y = 0; y < itemCount.x; y++)
                    {
                        for (int x = 0; x < itemCount.x; x++)
                        {
                            var idx = firstIdx + x + y * slotCount.x; //슬롯 체크 Idx
                            if (idx < cells.Count && cells[idx].IsEmpty) continue; //out of bounds가 아닌지, Empty인지 검사
                            isAvailable = false; //아니라면 unavailable
                            break;
                        }
                        if(!isAvailable) break; //루프 벗어나기
                    }
                    
                    
                    if (isAvailable)
                    {
                        for (int y = 0; y < itemCount.y; y++)//배치
                        {
                            for (int x = 0; x < itemCount.x; x++)
                            {
                                var idx = firstIdx + x + y * slotCount.x;
                                cells[idx].SetEmpty(false, item.Id);//Cell 채우기
                            }
                        }

                        ItemDict[item.Id] = (item, slotData.slotRT, firstIdx);
                        Debug.Log(ItemDict[item.Id] + " id: " + item.Id);
                        var minPos = cells[firstIdx].CellRT.anchoredPosition;
                        var maxPos = cells[firstIdx + itemCount.x -1 + slotCount.x * (itemCount.y - 1)]
                            .MaxPos; //아이템 우하단의 인덱스(최대)
                        var targetPos = (minPos + maxPos) / 2;
                        return (true, targetPos);
                    }
                }
            }
        }

        return (false, Vector2.zero);
    }

    public void RemoveItem(Guid id)
    {
        Debug.Log($"Removing item {id}");
        Debug.Log(ItemDict[id]);
        var slotRT = ItemDict[id].slotRT;
        var firstIdx = ItemDict[id].firstIdx;
        var itemCount = ItemDict[id].item.ItemCellCount;
        var slotData = _slotDict[slotRT];

        for (int y = 0; y < itemCount.y; y++)
        {
            for (int x = 0; x < itemCount.x; x++)
            {
                var idx = firstIdx + x + y * slotData.slotCount.x;
                slotData.cells[idx].SetEmpty(true, id);
            }
        }
        
        ItemDict.Remove(id);
    }
    
}
