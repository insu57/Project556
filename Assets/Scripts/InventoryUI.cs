using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //8x8 test... 
    [SerializeField] private GameObject inventoryGrid;
    [SerializeField] private GameObject inventoryCellPrefab;
    [SerializeField] private int inventoryXSize;
    [SerializeField] private int inventoryYSize;
    
    [SerializeField] private GameObject item;
    private Canvas _rootCanvas;
    private RectTransform _itemRectTransform;
    private Vector2 _pointerOffset;

    private void Awake()
    {
        _rootCanvas = GetComponentInParent<Canvas>();
    }
    
    private void Start()
    {
        
        //한칸 당 100, 인벤토리 크기에 따라 inventoryGrid크기 조절
        var gridRect = inventoryGrid.GetComponent<RectTransform>();
        
        gridRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, inventoryXSize*100);
        gridRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, inventoryYSize*100);

        for (int i = 0; i < inventoryXSize*inventoryYSize; i++)
        {
            Instantiate(inventoryCellPrefab, inventoryGrid.transform);
        }
        //인벤토리 크기 만큼 Cell생성
        
        
        //NxM 인벤토리(N*M만큼 Cell생성), AxB크기 아이템, 드래그 이동(마우스 입력처리), 칸 점유 정보
        //i, j -> 100, -100
        _itemRectTransform = item.GetComponent<RectTransform>();
        //15번째로...(0~63)
        const int index = 23;
        int x = index % inventoryXSize;
        int y = index / inventoryYSize;
        var vector2 = _itemRectTransform.anchoredPosition;
        vector2.x = x * 100;
        vector2.y = y * -100;
        //_itemRectTransform.anchoredPosition = vector2;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
    }
}
