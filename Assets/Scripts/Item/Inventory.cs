using System;
using System.Collections.Generic;
using Item;
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
        public RectTransform itemRT;
        public Vector2Int cellCount;
    }
    
    [SerializeField] private List<SlotData> slotDataList = new();
    private RectTransform _inventoryRT;
    
    public Dictionary<RectTransform, (List<CellData> cells, Vector2Int slotCount, RectTransform slotItemRT)> 
        SlotDict { get; } = new(); // Slot -> CellData List

    public Guid ItemInstanceID { get; private set; }
    public float Width { private set; get; }
    public float Height { private set; get; }
    private float _cellSize;
    public Dictionary<Guid, (ItemInstance item, RectTransform slotRT, int firstIdx)> ItemDict { get; } = new();
    public event Action<Guid> OnItemRemovedCheckQuickSlot;
    //기존 슬롯->아이템 정보...
    //스테이지에서 버리고 줍는것 생각하기...(인스턴스 생성관련...)

    public void Init(float cellSize, Guid itemInstanceID)
    {
        _cellSize = cellSize; //CellSize 개선... (UI/데이터 나누기...)
        ItemInstanceID = itemInstanceID; //인벤토리 아이템ID(Rig, Backpack 등 인벤토리가 있는 아이템) / LootCrate같은 경우는 Empty.
        
        TryGetComponent(out _inventoryRT);
        Width = _inventoryRT.rect.width;
        Height = _inventoryRT.rect.height;
        
        //인벤토리 초기화
        foreach (var slotData in slotDataList)
        {
            var slotRT = slotData.slotRT;
            //슬롯의 RectTransform
            
            List<CellData> cellDataList = new List<CellData>();
            
            for (int y = 0; y < slotData.cellCount.y; y++)
            {
                for (int x = 0; x < slotData.cellCount.x; x++)
                {
                    var  minPos = new Vector2(x * _cellSize, y * -_cellSize);
                    CellData cellData = new CellData(GearType.None, minPos); //초기화
                    cellDataList.Add(cellData);
                }
            }
            
            SlotDict[slotRT] = (cellDataList, slotData.cellCount, slotData.itemRT); //Dictionary 설정
        }
    }
    
    //record 타입.(생성자와 프로퍼티 선언을 동시에, Positioning Record.
    //init-only 프로퍼티 때문에 상단의 선언이 필요(IsExternalInit { })
    public record InventorySlotCheckResult( //CheckSlot의 result
        int FirstIdx, 
        Vector2 FirstIdxPos,
        RectTransform MatchSlotRT,
        SlotStatus SlotStatus,
        Vector2 CellCount,
        Guid TargetCellItemID
    );
    
    public InventorySlotCheckResult CheckSlot(Vector2 mousePos, ItemInstance item) //Mouse -> Slot -> Cell
    {
        var itemCellCount = item.ItemCellCount;//아이템 칸 크기
        
        foreach (var slotData in slotDataList) //slot 
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(slotData.slotRT, mousePos)) continue;
            var matchSlot = slotData.slotRT; //Mouse -> 인벤토리의 Slot
            
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(matchSlot, mousePos, null,
                    out var localPoint)) continue; //Slot Local point
            
            var (cells, slotCount, _) = SlotDict[matchSlot]; //슬롯의 정보
            
            GetFirstCellIdx(localPoint, slotCount, itemCellCount, out var firstIdx);
            //FirstIdx(해당 슬롯의 local Point로 아이템의 첫번째 인덱스(좌상단)

            if (firstIdx < 0) continue; //-1: SlotIdx out of bounds
            var firstX = firstIdx % slotCount.x;
            var firstY = firstIdx / slotCount.x;
    
            Vector2 localPos = cells[firstIdx].CellLocalPos; 
            Vector3 firstIdxPos = matchSlot.TransformPoint(localPos);//CellRT.position; //아이템 첫번째 슬롯의 Position(World)
            
            if (!cells[firstIdx].IsEmpty && item.IsStackable) //빈 상태가 아닐 때 stackable 아이템이라면
            {
                var targetCellItemID = cells[firstIdx].InstanceID; //해당 Cell에 있는 아이템 ID
                var targetCellItem = ItemDict[targetCellItemID]; 
                
                if (targetCellItem.item.ItemData.ItemDataID == item.ItemData.ItemDataID)//타겟 CellItem과 ItemDataID 비교
                {
                    //서로 같은 아이템 데이터라면
                    if (firstIdx == targetCellItem.firstIdx) //첫번째 인덱스가 동일하면(위치 동일)
                    {
                        if (targetCellItem.item.CurrentStackAmount < targetCellItem.item.MaxStackAmount) 
                        {
                            //타겟 CellItem이 max stack보다 적은 stack이면
                            
                            return new InventorySlotCheckResult (firstIdx, firstIdxPos, slotData.slotRT, 
                                SlotStatus.Available, itemCellCount, targetCellItemID);
                            //타겟CellItemID와 함께 available 리턴
                        }
                    }
                } 
                
            }
            
            for (int y = 0; y < itemCellCount.y; y++) //cell체크...
            {
                for (int x = 0; x < itemCellCount.x; x++)
                {
                    var idx = firstIdx + x + y * slotCount.x;
                    if (firstX + x >= slotCount.x || firstY + y >= slotCount.y) //out of bounds 슬롯을 벗어남
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
                        //넘어간 만큼 줄이기(슬롯의 경계에 걸린 경우)
                    }
                    
                    if (cells[idx].IsEmpty) continue; //Empty면 continue
                    if (cells[idx].InstanceID == item.InstanceID) continue; //ID가 동일하면 (같은 아이템 인스턴스)
                    
                    //위 조건에 걸리면 unavailable
                    return new InventorySlotCheckResult(-1, firstIdxPos, slotData.slotRT, 
                        SlotStatus.Unavailable, itemCellCount, Guid.Empty); //다른 아이템과 겹친 경우
                }
            }
            
            //조건에 문제가 없다면
            return new InventorySlotCheckResult (firstIdx, firstIdxPos, slotData.slotRT, 
                SlotStatus.Available, itemCellCount, Guid.Empty);//빈 Cell이므로 Guid.Empty
        }
        //해당되는 SlotRT가 없다면 none.=
        return new InventorySlotCheckResult (-1, Vector2.zero, null, 
            SlotStatus.None, itemCellCount, Guid.Empty); //No Match Slot! 아무것도 없음.
    }
    
    private void GetFirstCellIdx(Vector2 localPoint, Vector2Int slotSize, Vector2Int itemCellCount , out int firstIdx)
    {
        int width = slotSize.x;
        Vector2 firstPos = localPoint - new Vector2(itemCellCount.x * _cellSize / 2f , -itemCellCount.y * _cellSize / 2f) 
                             + new Vector2(_cellSize, -_cellSize)/2f;
        // 중심(아이템)에서 좌상단 MinPosition(가로세로 절반)(좌상단Cell, 첫번째Cell) + 해당 Cell의 Center
        int x = (int) (firstPos.x / _cellSize);
        int y = (int)(-firstPos.y / _cellSize);//y는 음수
        firstIdx = x + width * y;
        if (x < 0 || x >= slotSize.x || y < 0 || y >= slotSize.y) firstIdx = -1;
    }
    
    public (Vector2 pos, RectTransform slotItemRT) AddItem(ItemInstance item, int firstIdx, RectTransform slotRT) //Available인경우만
    {
        var itemCount = item.ItemCellCount;

        var (cells, slotCount, itemRT) = SlotDict[slotRT];
        
        for (int y = 0; y < itemCount.y; y++)//배치
        {
            for (int x = 0; x < itemCount.x; x++)
            {
                var idx = firstIdx + x + y * slotCount.x;
                cells[idx].SetEmpty(false, item.InstanceID);//Cell 채우기
            }
        }

        ItemDict[item.InstanceID] = (item, slotRT, firstIdx);
       
        var minPos = cells[firstIdx].CellLocalPos; //CellRT.anchoredPosition;
        var maxPos = cells[firstIdx + itemCount.x -1 + slotCount.x * (itemCount.y - 1)]
            .CellLocalPos + new Vector2(_cellSize, -_cellSize); //아이템 우하단의 인덱스(최대)
        var targetPos = (minPos + maxPos) / 2;
        
        return (targetPos, itemRT);
    }

    public (bool isAvailable, int firstIdx, RectTransform slotRT) CheckCanAddItem(Vector2Int itemCellCount)
    {
        foreach (var slotData in slotDataList)
        {
            var (cells, slotCount, _) = SlotDict[slotData.slotRT]; //슬롯정보

            for (int h = 0; h < slotCount.y; h++)
            {
                for (int w = 0; w < slotCount.x; w++)
                {
                    int firstIdx = w + h * slotCount.x; //빈 슬롯 체크 시작 Idx
                    bool isAvailable = true;
                    
                    for (int y = h; y < h + itemCellCount.y; y++)
                    {
                        for (int x = w; x < w + itemCellCount.x; x++)
                        {
                            var idx = x + slotCount.x * y;

                            if (x < slotCount.x && y < slotCount.y && cells[idx].IsEmpty) continue;
                            //out of bounds가 아닌지, empty인지 검사. 위 조건에 해당하지 않으면 Out of Bounds이다.
                            
                            isAvailable = false; //아니라면 unavailable
                            break;
                        }
                        if(!isAvailable) break; //루프 벗어나기
                    }
                    
                    if (isAvailable)
                    {
                        return (true, firstIdx, slotData.slotRT);
                    }
                }
            }
        }

        return (false, -1, null);
    }

    public (Vector2 pos, RectTransform slotItemRT) 
        MoveItem(ItemInstance item, int firstIdx, RectTransform slotRT, bool hasRotated)//같은 인벤토리에서 이동
    {
        var itemCount = item.ItemCellCount;

        var originItemCount = itemCount;
        var originSlotRT = ItemDict[item.InstanceID].slotRT;
        var originFirstIdx = ItemDict[item.InstanceID].firstIdx;
        
        if(hasRotated)
            originItemCount = new Vector2Int(itemCount.y, itemCount.x);

        for (int y = 0; y < originItemCount.y; y++)
        {
            for (int x = 0; x < originItemCount.x; x++)
            {
                var idx = originFirstIdx + x  + y * SlotDict[originSlotRT].slotCount.x;
                SlotDict[originSlotRT].cells[idx].SetEmpty(true, item.InstanceID);
            }
        }
        
        var (cells, slotCount, itemRT) = SlotDict[slotRT];
        
        for (int y = 0; y < itemCount.y; y++)//새로운 위치로 이동
        {
            for (int x = 0; x < itemCount.x; x++)
            {
                var idx = firstIdx + x + y * slotCount.x;
                cells[idx].SetEmpty(false, item.InstanceID);//Cell 채우기
            }
        }
        
        ItemDict[item.InstanceID] = (item, slotRT, firstIdx);
       
        var minPos = cells[firstIdx].CellLocalPos; //CellRT.anchoredPosition;
        var maxPos = cells[firstIdx + itemCount.x -1 + slotCount.x * (itemCount.y - 1)]
            .CellLocalPos + new Vector2(_cellSize, -_cellSize); //아이템 우하단의 인덱스(최대)
        var targetPos = (minPos + maxPos) / 2;
        
        return (targetPos, itemRT);
    }
    
    public void RemoveItem(Guid id, bool hasRotated)
    {
        //아이템 제거
        var item = ItemDict[id].item;
        var slotRT = ItemDict[id].slotRT; //아이템의 SlotRT
        var firstIdx = ItemDict[id].firstIdx; //아이템의 첫번째 인덱스
        var itemCount = ItemDict[id].item.ItemCellCount; //아이템 Cell개수
        
        if(hasRotated) //중간에 회전했다면 원래 상태에서의 Cell로
            itemCount = new Vector2Int(ItemDict[id].item.ItemCellCount.y, ItemDict[id].item.ItemCellCount.x);
        
        var slotData = SlotDict[slotRT];

        for (int y = 0; y < itemCount.y; y++)
        {
            for (int x = 0; x < itemCount.x; x++)
            {
                var idx = firstIdx + x + y * slotData.slotCount.x;
                slotData.cells[idx].SetEmpty(true, id); //empty로 변경
            } //회전된 아이템에서 버그... -> 들고있을때 회전 후 옮기기(회전 전 기준으로 비워야함....)
        }
        ItemDict.Remove(id); //Dict에서 제거
        if(item.ItemData is IConsumableItem) OnItemRemovedCheckQuickSlot?.Invoke(id);//소비 아이템이라면 
    }
}
