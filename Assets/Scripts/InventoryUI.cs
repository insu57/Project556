using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class InventoryItem
{
    //guid로 아이템 구분
    private string _id;
    private IItemData _itemData;
    private List<int> _slotIndex;//슬롯 정보
    
    public InventoryItem(IItemData itemData)
    {
        this._itemData = itemData;
        _slotIndex = new List<int>();
        _id = Guid.NewGuid().ToString();
    }

    public void MoveItem(int firstIdx, int lastIdx)
    {
        //슬롯 위치 변경
        //좌상단 -> 우하단
    }
}

public class InventoryUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject inventoryGrid;
    [SerializeField] private GameObject inventorySlotPrefab;
    [SerializeField] private int inventoryXSize;
    [SerializeField] private int inventoryYSize;
    public RectTransform InventoryGridRT { get; private set; }
    
    private SlotData[] _slotDataArray;
    //Array로 수정(크기가 거의 안변하기 때문에 배열로) 변할 일이 많아지면 List + Span?(2021+)
    private List<InventoryItem> _itemDataList = new List<InventoryItem>(); //IItemData, Guid, Slot정보
    
    //TEMP Test
    [SerializeField] private ItemDragger item;
    [SerializeField] private ItemDragger itemPistol;
    [SerializeField] private BaseItemDataSO bandageData;
    [SerializeField] private BaseItemDataSO m1911a1Data;
    
    
    private Vector2 _pointerOffset;
    private static readonly Vector2 StartMinPos = new Vector2(0, 0);
    private static readonly Vector2 StartMaxPos = new Vector2(100, -100);
    private static readonly Vector2 StartImagePos = new Vector2(50, -50);

    
    
    private struct SlotData
    {
        public bool IsEmpty;
        public readonly Vector2 MinPosition;
        public readonly Vector2 MaxPosition;
        public readonly Vector2 ImagePosition;
        public int ItemIdx;
        
        public SlotData( Vector2 minPosition, Vector2 maxPosition, Vector2 imagePosition)
        {
            IsEmpty = true;
            MinPosition = minPosition;
            MaxPosition = maxPosition;
            ImagePosition = imagePosition;
            ItemIdx = -1;
        }
        //
    }
        
    private void Awake()
    {
        InventoryGridRT = inventoryGrid.GetComponent<RectTransform>();
    }
    
    private void Start()
    {
        
        //한칸 당 100, 인벤토리 크기에 따라 inventoryGrid크기 조절
        var gridRect = inventoryGrid.GetComponent<RectTransform>();
        
        gridRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, inventoryXSize*100);
        gridRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, inventoryYSize*100);
    
        //Index 0 (pivot 0.5, 0.5) -> 50, -50
        // (0, 0) ~ (100, -100) Slot RectTransform 범위...
        _slotDataArray = new SlotData[inventoryXSize*inventoryYSize];
        
        Vector2 cellOffset = new Vector2(100, -100);
        
        for (int y = 0; y < inventoryYSize; y++)
        {
            for (int x = 0; x < inventoryXSize; x++)
            {
                Instantiate(inventorySlotPrefab, inventoryGrid.transform);
                
                Vector2 offset = new Vector2(x, y) * cellOffset;
                Vector2 minPos = StartMinPos + offset;
                Vector2 maxPos = StartMaxPos + offset;
                Vector2 imagePos = StartImagePos + offset;
                
                SlotData slotData = new SlotData(minPos, maxPos, imagePos);
                
                _slotDataArray[x + y * inventoryYSize] = slotData;
            } 
        }
        
        //아이템 테스트
        RectTransform itemRectTransform = item.GetComponent<RectTransform>();
        item.Init(bandageData);
        _itemDataList.Add(new InventoryItem(bandageData));
        
        // Class InventoryItem_슬롯 정보, Guid(아이템 구분) -> (필드, 상자 등 -> 플레이어, Guid를 복사해서 구분?)
        RectTransform itemPistolRT = itemPistol.GetComponent<RectTransform>();
        itemPistol.Init(m1911a1Data);
        _itemDataList.Add(new InventoryItem(m1911a1Data));
        
        //0~N*M
        int index = 23;

        ref var temp = ref _slotDataArray[index];
        temp.IsEmpty = false;
        itemRectTransform.anchoredPosition = _slotDataArray[index].ImagePosition;
        
        // 슬롯의 인덱스(위치)를 알면 아이템도 알고 아이템을 알면 슬롯도 ???? 단방향으로만 설계?
        
        temp = ref _slotDataArray[1];
        temp.IsEmpty = false;
        temp = ref _slotDataArray[2];
        temp.IsEmpty = false;
        itemPistolRT.anchoredPosition = (_slotDataArray[1].MinPosition + _slotDataArray[2].MaxPosition) / 2;
        //anchor?
        
        //NxM 인벤토리(N*M만큼 Cell생성), AxB크기 아이템, 드래그 이동(마우스 입력처리), 칸 점유 정보
        //i, j -> 100, -100
    }

    private int GetSlotIndex(Vector2 pos)
    {
        int x = (int)(pos.x / 100);
        int y = -(int)(pos.y / 100);
        return x + y * inventoryYSize;
    }

    public bool SlotEmptyCheck(Vector2 pos)
    {
        //Empty Check...현재 아이템의 정보도 필요...(크기)
        int idx = GetSlotIndex(pos);
        //슬롯에 아이템 정보? 어떤 아이템이 있는가? Idx?
        
        if (idx < 0 || idx >= inventoryXSize * inventoryYSize)
        {
            return false;
        }
        
        bool isEmpty = _slotDataArray[idx].IsEmpty;
        
        return isEmpty;
    }

    public Vector2 ItemMove(Vector2 startPos, Vector2 endPos)
    {
        int startIdx = GetSlotIndex(startPos); //아이템 정보..
        int endIdx = GetSlotIndex(endPos);
        
        ref var temp = ref _slotDataArray[startIdx];
        temp.IsEmpty = true;
        
        temp = ref _slotDataArray[endIdx];
        temp.IsEmpty = false;
        
        return _slotDataArray[endIdx].ImagePosition;
    }
    
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
    }
}
