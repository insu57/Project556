using System;
using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InventoryItem
{
    private IItemData _itemData;

    private int _idx;

    public Guid Id { get; }
    public int Width => _itemData.ItemWidth;
    public int Height => _itemData.ItemHeight;
    public Vector2 SizeVector => new(_itemData.ItemWidth, _itemData.ItemHeight);
    public int Idx => _idx;
    
    public InventoryItem(IItemData itemData)
    {
        this._itemData = itemData;
        Id = Guid.NewGuid();
    }

    public void MoveItem(int idx)
    {
        //슬롯 위치 변경
        _idx = idx;
    }
}

public class InventoryUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //없으면 생성 및 초기화, 있으면 활성/비활성?
    [SerializeField] private GameObject inventoryGrid;
    [SerializeField] private GameObject inventorySlotPrefab;
    [SerializeField] private int inventoryXSize;
    [SerializeField] private int inventoryYSize;
    [SerializeField] private Image slotAvailable; //SlotAvailable Indicator
    
    public RectTransform InventoryGridRT { get; private set; }
    
    private SlotData[] _slotDataArray;
    //Array로 수정(크기가 거의 안변하기 때문에 배열로) 변할 일이 많아지면 List + Span?(2021+)
    private Dictionary<Guid, InventoryItem> _itemDataDictionary = new Dictionary<Guid, InventoryItem>(); 
    
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
        //public int ItemIdx;
        public Guid Id;
        
        public SlotData( Vector2 minPosition, Vector2 maxPosition, Vector2 imagePosition)
        {
            IsEmpty = true;
            MinPosition = minPosition;
            MaxPosition = maxPosition;
            ImagePosition = imagePosition;
            Id = Guid.Empty;
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
        _slotDataArray = new SlotData[inventoryXSize * inventoryYSize];
        
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
                
                _slotDataArray[x + y * inventoryXSize] = slotData;
            } 
        }
        
        //아이템 테스트
        RectTransform itemRectTransform = item.GetComponent<RectTransform>();
        InventoryItem bandageItem = new InventoryItem(bandageData);
        bandageItem.MoveItem(23);
        item.Init(bandageData, bandageItem.Id);
        //_itemDataList.Add(bandageItem);
        _itemDataDictionary.Add(bandageItem.Id, bandageItem);
        
        // Class InventoryItem_슬롯 정보, Guid(아이템 구분) -> (필드, 상자 등 -> 플레이어, Guid를 복사해서 구분?)
        RectTransform itemPistolRT = itemPistol.GetComponent<RectTransform>();
        
        InventoryItem pistolItem = new InventoryItem(m1911a1Data);
        pistolItem.MoveItem(1);
        itemPistol.Init(m1911a1Data, pistolItem.Id);
        //_itemDataList.Add(pistolItem);
        _itemDataDictionary.Add(pistolItem.Id, pistolItem);
        
        //0~N*M
        int index = 23;

        SetSlot(index, false, bandageItem.Id);
        itemRectTransform.anchoredPosition = _slotDataArray[index].ImagePosition;
        
        // 슬롯의 인덱스(위치)를 알면 아이템도 알고 아이템을 알면 슬롯도 ???? 단방향으로만 설계?
        
        SetSlot(1, false, pistolItem.Id);
        SetSlot(2, false, pistolItem.Id);
        itemPistolRT.anchoredPosition = (_slotDataArray[1].MinPosition + _slotDataArray[2].MaxPosition) / 2;
        //anchor?
        
        //NxM 인벤토리(N*M만큼 Cell생성), AxB크기 아이템, 드래그 이동(마우스 입력처리), 칸 점유 정보
        //i, j -> 100, -100
    }

    private int GetSlotIndex(Vector2 pos)
    {
        int x = (int)(pos.x / 100);
        int y = -(int)(pos.y / 100);
        if (x < 0 || x >= inventoryXSize || y < 0 || y >= inventoryYSize) //범위 밖(인벤토리 밖)은 예외처리
        {
            return -1;
        }
        return x + y * inventoryXSize;
    }

    private void SetSlot(int idx , bool isEmpty, Guid id)
    {
        ref var slotData = ref _slotDataArray[idx];
        slotData.IsEmpty = isEmpty;
        slotData.Id = isEmpty ? Guid.Empty : id;
    }
    
    public bool SlotEmptyCheck(Vector2 pos)
    {
        //Empty Check...현재 아이템의 정보도 필요...(크기)
        int idx = GetSlotIndex(pos);
        //슬롯에 아이템 정보? 어떤 아이템이 있는가? Idx?
        
        if (idx < 0)
        {
            return false;
        }
        
        bool isEmpty = _slotDataArray[idx].IsEmpty;
        
        return isEmpty;
    }

    public Vector2 ItemMove(Vector2 originPos, Vector2 targetPos, Guid id)
    {
        slotAvailable.enabled = false;

        //버그가...?
        if (CheckSlotAvailable((targetPos, id)))//자기 자신의 원래 슬롯은??
        {
            InventoryItem dragItem = _itemDataDictionary[id];
            int width = dragItem.Width;
            int height = dragItem.Height;
            int originFirstIdx = dragItem.Idx;//?
            int targetFirstIdx = GetSlotIndex(targetPos);
            //기존 슬롯 Empty
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    int idx = originFirstIdx + h * width + w;
                    SetSlot(idx, true, Guid.Empty);
                }
            }
            //이동 슬롯
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    int idx = targetFirstIdx + h * inventoryXSize + w;
                    Debug.Log("Drag!...Idx: " + idx);//실제 이동하는 위치 -> 마우스 기준, SlotAvailable -> 서로 다름...(수정 필)
                    SetSlot(idx, false, id);
                }
            }

            int targetEndIdx = targetFirstIdx + (width - 1) + (height - 1) * inventoryXSize;
            Vector2 minPos = _slotDataArray[targetFirstIdx].MinPosition;
            Vector2 maxPos = _slotDataArray[targetEndIdx].MaxPosition;
            dragItem.MoveItem(targetFirstIdx);
            return (minPos + maxPos) / 2;
        }
        return originPos;
    }

    public bool CheckSlotAvailable((Vector2 itemPos, Guid id) itemPosData) //들고 있는 아이템을 넣을 공간이 있는지 체크
    {
        //mousePos -> 커서 위치(아이템의 중심 pivot 0.5 0.5)
        //좌상단 가장 작은 인덱스 기준
        var itemPos = itemPosData.itemPos;
        var id = itemPosData.id;
        //Debug.Log("MousePos: " + itemPos);
        int firstIdx = GetSlotIndex(itemPos);
        if (firstIdx < 0) //범위 밖
        {
            slotAvailable.enabled = false;
            return false;
        }
        //나머지 슬롯도 체크!
        //bool isAvailable = true;
        InventoryItem dragItem = _itemDataDictionary[id]; //현재 들고있는 아이템
        
        for (int h = 0; h < dragItem.Height; h++)
        {
            for (int w = 0; w < dragItem.Width; w++)
            {
                //
                Vector2 pos = _slotDataArray[firstIdx].ImagePosition + new Vector2(w * 100, h * -100);
                
                int slotIdx = GetSlotIndex(pos);
                
                if (slotIdx < 0 || (!_slotDataArray[slotIdx].IsEmpty && _slotDataArray[slotIdx].Id != dragItem.Id ))
                {
                    //slotAvailable.color = new Color32(255, 0, 0, 60); //Red 불투명 -> 추후 Global Data에서?
                    //isAvailable = false;
                    Debug.Log("unavailable! - Index: " + slotIdx);
                    ShowSlotAvailable(_slotDataArray[firstIdx].MinPosition, dragItem.SizeVector, false);
                    return false;
                }
            }
            //if(!isAvailable) break;
        }
        ShowSlotAvailable(_slotDataArray[firstIdx].MinPosition, dragItem.SizeVector, true);
        
        return true;
    }

    private void ShowSlotAvailable(Vector2 pos, Vector2 size, bool isAvailable)
    {
        slotAvailable.enabled = true;
        slotAvailable.rectTransform.anchoredPosition = pos;
        if (isAvailable)
        {
            slotAvailable.color = new Color32(0, 255, 0, 60);
        }
        else
        {
            slotAvailable.color = new Color32(255, 0, 0, 60);
        }
        slotAvailable.rectTransform.sizeDelta = size * 100;
    }
    
    public void DisableSlotAvailable()
    {
        slotAvailable.enabled = false;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
    }
}
