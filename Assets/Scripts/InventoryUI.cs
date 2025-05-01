using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;


public class InventoryUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //8x8 test... 
    
    [SerializeField] private GameObject inventoryGrid;
    [SerializeField] private GameObject inventorySlotPrefab;
    [SerializeField] private int inventoryXSize;
    [SerializeField] private int inventoryYSize;
    public RectTransform InventoryGridRT { get; private set; }

    private List<SlotData> _slotDataList = new List<SlotData>();
    //List or Array 고민... (인벤토리 생성 초기 이후 크기가 변하지 않을 수 있음...)
    
    [SerializeField] private GameObject item;
    private Canvas _rootCanvas;
    private RectTransform _itemRectTransform;
    private Vector2 _pointerOffset;
    private static readonly Vector2 StartMinPos = new Vector2(0, 0);
    private static readonly Vector2 StartMaxPos = new Vector2(100, -100);
    private static readonly Vector2 StartImagePos = new Vector2(50, -50);

    private struct SlotData
    {
        public bool IsEmpty;
        public Vector2 MinPosition;
        public Vector2 MaxPosition;
        public Vector2 ImagePosition;

        public SlotData(bool isEmpty, Vector2 minPosition, Vector2 maxPosition, Vector2 imagePosition)
        {
            IsEmpty = isEmpty;
            MinPosition = minPosition;
            MaxPosition = maxPosition;
            ImagePosition = imagePosition;
        }
        //
    }
        
    private void Awake()
    {
        _rootCanvas = GetComponentInParent<Canvas>();
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
        
        Vector2 cellOffset = new Vector2(100, -100);
        
        //
        for (int y = 0; y < inventoryYSize; y++)
        {
            for (int x = 0; x < inventoryXSize; x++)
            {
                Instantiate(inventorySlotPrefab, inventoryGrid.transform);
                
                Vector2 offset = new Vector2(x, y) * cellOffset;
                Vector2 minPos = StartMinPos + offset;
                Vector2 maxPos = StartMaxPos + offset;
                Vector2 imagePos = StartImagePos + offset;
                
                _slotDataList.Add(
                    new SlotData( true, minPos, maxPos, imagePos));
            } 
        }
        _itemRectTransform = item.GetComponent<RectTransform>();
        //0~N*M
        const int index = 23;
        int a = index % inventoryXSize;
        int b = index / inventoryXSize;
        var temp = _slotDataList[index];
        temp.IsEmpty = false;
        _slotDataList[index] = temp;
        _itemRectTransform.anchoredPosition = _slotDataList[index].ImagePosition;
        Debug.Log(_slotDataList[index].IsEmpty);
        //NxM 인벤토리(N*M만큼 Cell생성), AxB크기 아이템, 드래그 이동(마우스 입력처리), 칸 점유 정보
        //i, j -> 100, -100

        //int x = index % inventoryXSize;
        //int y = index / inventoryYSize;

        //Vector2 zeroPos = new Vector2(50, -50);
        //_itemRectTransform.anchoredPosition = zeroPos + new Vector2(x * 100, y * -100);

    }

    private int GetSlotIndex(Vector2 pos)
    {
        int x = (int)(pos.x / 100);
        int y = -(int)(pos.y / 100);
        return x + y * inventoryYSize;
    }

    public bool SlotEmptyCheck(Vector2 pos)
    {
        int idx = GetSlotIndex(pos);
        
        if (idx < 0 || idx >= inventoryXSize * inventoryYSize)
        {
            return false;
        }
        
        bool isEmpty = _slotDataList[idx].IsEmpty;
        
        return isEmpty;
    }

    public Vector2 ItemMove(Vector2 startPos, Vector2 endPos)
    {
        int startIdx = GetSlotIndex(startPos);
        int endIdx = GetSlotIndex(endPos);
        
        var temp = _slotDataList[startIdx];
        temp.IsEmpty = true;
        _slotDataList[startIdx] = temp;
        
        temp = _slotDataList[endIdx];
        temp.IsEmpty = false;
        _slotDataList[endIdx] = temp;
        
        return _slotDataList[endIdx].ImagePosition;
    }
    
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
    }
}
