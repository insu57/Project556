using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


namespace System.Runtime.CompilerServices //c# 9.0 {init} 키워드 때문에
{
    // internal 또는 public 상관없음
    public static class IsExternalInit { }
}



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
    
    //public class Check
    
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
    //public RectTransform InventoryRT => _inventoryRT;
    public RectTransform ItemRT => itemRT; //아이템 배치 RectTransform
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

        //인벤토리 초기화
        foreach (var slotData in slotDataList)
        {
            var slotRT = slotData.slotRT;
            //슬롯의 RectTransform
            
            List<CellData> cellDataList = new List<CellData>();
            //CellData List
            for (int i = 0; i < slotRT.childCount; i++) //SlotRT의 자식들(Cell)
            {
                var child = slotRT.GetChild(i) as RectTransform;
                CellData cellData = new CellData(GearType.None); //초기화
                cellData.SetCellRT(child); //RT설정
                cellDataList.Add(cellData);
            }
            _slotDict[slotRT] = (cellDataList, slotData.cellCount); //Dictionary 설정
        }
    }

    private void Start()
    {
        
    }
    
    //record 타입.(생성자와 프로퍼티 선언을 동시에, Positioning Record.
    //init-only 프로퍼티 때문에 상단의 선언이 필요(IsExternalInit { })
    public record InventorySlotCheckResult(
        int FirstIdx, 
        Vector2 FirstIdxPos,
        RectTransform MatchSlotRT,
        SlotStatus SlotStatus,
        Vector2 CellCount,
        Guid TargetCellItemID
    );
    
    public InventorySlotCheckResult CheckSlot(Vector2 mousePos, Vector2Int itemCellCount, InventoryItem item)
    {
        foreach (var slotData in slotDataList)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(slotData.slotRT, mousePos)) continue;
            var matchSlot = slotData.slotRT; //Mouse -> 인벤토리의 Slot
            
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(matchSlot, mousePos, null,
                    out var localPoint)) continue; //Slot Local point
            
            var (cells, slotCount) = _slotDict[matchSlot];
            GetFirstCellIdx(localPoint, slotCount, itemCellCount, out var firstIdx);
            //FirstIdx

            if (firstIdx < 0) continue; //-1: SlotIdx out of bounds
            var firstX = firstIdx % slotCount.x;
            var firstY = firstIdx / slotCount.x;
            
            var firstIdxPos = cells[firstIdx].CellRT.position; //아이템 첫번째 슬롯의 Position(World)
            var targetCellItemID = cells[firstIdx].InstanceID;
            //아이템 스택 검사...? id, 어떤 inven인지만 알면,,
            // targetInven에 뭐가 있는지...
            
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
                        return new InventorySlotCheckResult(-1, firstIdxPos, slotData.slotRT, 
                            SlotStatus.Unavailable, itemCellCount - overCell, Guid.Empty);
                        //넘어간 만큼 줄이기
                    }

                    //추가 검토 필요... max amount 등
                    if (cells[idx].IsEmpty) continue; //Empty면 continue
                    if (cells[idx].InstanceID == item.InstanceID) continue; //ID가 동일하면 (같은 아이템 인스턴스)
                    if (item.IsStackable && item.ItemData.ItemDataID ==
                        ItemDict[cells[idx].InstanceID].item.ItemData.ItemDataID) 
                        //Stackable일 때 같은 아이템 데이터면 -> 개선점???
                    {
                        continue;
                    }
                    
                    //위 조건에 걸리면 unavailable
                    return new InventorySlotCheckResult(-1, firstIdxPos, slotData.slotRT, 
                        SlotStatus.Unavailable, itemCellCount, Guid.Empty);
                }
            }
            //조건에 문제가 없다면
            return new InventorySlotCheckResult (firstIdx, firstIdxPos, slotData.slotRT, 
                SlotStatus.Available, itemCellCount, targetCellItemID);
        }
        //해당되는 SlotRT가 없다면 none.
        return new InventorySlotCheckResult (-1, Vector2.zero, null, 
            SlotStatus.None, itemCellCount, Guid.Empty); //No Match Slot!
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
            var (cells, slotCount) = _slotDict[slotData.slotRT];
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
                                cells[idx].SetEmpty(false, item.InstanceID);//Cell 채우기
                            }
                        }

                        ItemDict[item.InstanceID] = (item, slotData.slotRT, firstIdx);
                        Debug.Log(ItemDict[item.InstanceID] + " id: " + item.InstanceID);
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

    public InventoryItem GetItemData(Guid id)
    {
        var (item, slotRT, firstIdx) = ItemDict[id];
        
        return item;
    }
    
    public void RemoveItem(Guid id)
    {
        //아이템 제거
        Debug.Log($"Removing item {id}");
        var slotRT = ItemDict[id].slotRT; //아이템의 SlotRT
        var firstIdx = ItemDict[id].firstIdx; //아이템의 첫번째 인덱스
        var itemCount = ItemDict[id].item.ItemCellCount; //아이템 Cell개수
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

    //MoveItem -> 해당 Cell이 available 일 때만 호출
    public Vector2 MoveItem(InventoryItem item, int firstIdx, RectTransform targetSlot)
    {
        //아이템 이동
        Debug.Log($"Moving item {item.ItemData.ItemName}, ID: {item.InstanceID}");
        var id = item.InstanceID;
        ItemDict.Add(id, (item, targetSlot, firstIdx));
        
        var (cells, slotCount) = _slotDict[targetSlot];
        var itemCount = item.ItemCellCount;
        for (int y = 0; y < itemCount.y; y++)
        {
            for (int x = 0; x < itemCount.x; x++)
            {
                var idx = firstIdx + x + y * slotCount.x;
                cells[idx].SetEmpty(false, id);
            }
        }
        var minPos = cells[firstIdx].CellRT.anchoredPosition;
        var maxPos = cells[firstIdx + itemCount.x - 1 + slotCount.x * (itemCount.y - 1)].MaxPos;
        //버그있음...
        return (minPos + maxPos) / 2;
    }
    
}
