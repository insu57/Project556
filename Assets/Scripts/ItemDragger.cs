using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDragger : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform _itemRT;
    //private RectTransform _inventoryRT;
    private RectTransform _inventoryRT;
    private Vector2 _pointerOffset;
    private Vector2 _pointerDownPos;
    private CanvasGroup _canvasGroup;
    //private Image
    private Transform _itemDefaultParent;
    private Transform _itemDraggingParent;
    
    private InventoryManager _inventoryManager;
    //private InventoryUI _inventoryUI;
    private InventoryItem _item;
    //private IItemData _itemData;
    private int _widthSize;
    private int _heightSize;
    private Guid _id;
    private int _idx;
    //[SerializeField] private Image outline;
    private Image _itemImage;
    [SerializeField] private Image highlight;
    private float _slotSize;

    //test
    [SerializeField] private List<RectTransform> panels;
    
    private void Awake()
    {
        _itemRT = GetComponent<RectTransform>();
        //_rootCanvas = GetComponentInParent<Canvas>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _itemImage = GetComponent<Image>();
    }

    public void Init(IItemData itemData, Guid id, InventoryManager inventoryManager, RectTransform inventoryRT)
    {
        //_inventoryManager = GetComponentInParent<InventoryManager>();
        //_inventoryUI = GetComponentInParent<InventoryUI>();
        _inventoryManager = inventoryManager;
        _slotSize = inventoryManager.SlotSize;
        _inventoryRT = inventoryRT;
        
        _widthSize = itemData.ItemWidth;
        _heightSize = itemData.ItemHeight;
        Vector2 imageSize = new Vector2(_widthSize * _slotSize, _heightSize * _slotSize);
        _itemRT.sizeDelta = imageSize;
        _itemImage.sprite = itemData.ItemSprite;

        _id = id;
        
        _itemDefaultParent = transform.parent;
        _itemDraggingParent = inventoryRT;
    }

    private (Vector2, Guid) GetFirstSlotPos(Vector2 mousePos)
    {
        float x = mousePos.x + (-_slotSize * _widthSize + _slotSize) / 2f;
        float y = mousePos.y - (-_slotSize * _heightSize + _slotSize) / 2f;//
        return (new Vector2(x, y), _id);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        highlight.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlight.enabled = false;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = false;
        _pointerDownPos = _itemRT.anchoredPosition;
        //_itemImage.maskable = false;
        transform.SetParent(_itemDraggingParent);
        
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                _itemRT, eventData.position, eventData.pressEventCamera, out var globalMousePos))
        {
            _itemRT.position = globalMousePos;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _inventoryRT, eventData.position, eventData.pressEventCamera, out var localPos))
        {
            //다른 RT에서도 인벤토리 슬롯 구분... 위의 globalMouse로 check...-> 어떤 인벤인지...
            //_inventoryUI
            //Debug.Log(GetFirstSlotPos(localPos).Item1);
            
            //_inventoryUI.CheckSlotAvailable(GetFirstSlotPos(localPos));
        }
        
        //test
        foreach (var panel in panels)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(panel, eventData.position, null))
            {
                Debug.Log("RectContainsScreenPoint!: "+panel+" Position: "+eventData.position);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(_itemDefaultParent);
        
        //check position...
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _inventoryRT, eventData.position, eventData.pressEventCamera, out var localPos
            )) return;
        var firstSlot = GetFirstSlotPos(localPos);
        //_itemRT.anchoredPosition = _inventoryUI.ItemMove(_pointerDownPos, firstSlot.Item1, _id);
        
        //_inventoryUI.DisableSlotAvailable();
        _canvasGroup.blocksRaycasts = true;
        //_itemImage.maskable = true;
       
    }

    
}
