using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform _itemRT;
    private RectTransform _itemParentRT;
    private RectTransform _inventoryRT;
    private RectTransform _targetItemParentRT;
    private RectTransform _targetInventoryRT;
    public RectTransform InventoryRT => _inventoryRT;
    private Transform _itemDraggingParent;
    private Vector2 _pointerOffset;
    private Vector2 _pointerDownPos;
    private Vector2 _targetPos;
   // private bool _isAvailable;
    
    private CanvasGroup _canvasGroup;

    private InventoryUIPresenter _inventoryUIPresenter;
    private UIManager _uiManager;
    private int _widthSize;
    private int _heightSize;
    private Guid _id;
   
    private int _idx;
    private Image _itemImage;
    [SerializeField] private Image highlight;
    private float _cellSize;
    
    public event Action<ItemDragHandler, Guid> OnPointerDownEvent;
    public event Action<ItemDragHandler, Vector2, Guid> OnDragEvent;
    public event Action<ItemDragHandler, Vector2, Guid> OnEndDragEvent;
    
    private void Awake()
    {
        _itemRT = GetComponent<RectTransform>();
        //_rootCanvas = GetComponentInParent<Canvas>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _itemImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (_inventoryUIPresenter)
        {
            _inventoryUIPresenter.InitItemDragHandler(this);
        }
    }

    private void OnDisable()
    {
        if (_inventoryUIPresenter)
        {
            _inventoryUIPresenter.OnDisableItemDragHandler(this);
        }
    }

    public void Init(InventoryItem item, InventoryUIPresenter presenter, Transform uiParent)
    {
        _inventoryUIPresenter = presenter;
        _inventoryUIPresenter.InitItemDragHandler(this);
        _cellSize = presenter.CellSize;

        var itemData = item.ItemData;
        _widthSize = itemData.ItemWidth;
        _heightSize = itemData.ItemHeight;
  
        Vector2 imageSize = new Vector2(_widthSize * _cellSize, _heightSize * _cellSize);
        _itemRT.sizeDelta = imageSize;
        _itemImage.sprite = itemData.ItemSprite;

        _id = item.Id;
        
        //_itemParentRT = itemParent;
        //_itemRT.SetParent(itemParent);
        _itemDraggingParent = uiParent;
        //_inventoryRT = inventoryRT;
        //_isAvailable = false;
    }
    
    public void SetItemDragPos(Vector2 targetPos, Vector2 size, RectTransform itemParentRT, RectTransform inventoryRT)
    {
        _itemRT.SetParent(itemParentRT);
        _itemRT.anchoredPosition = targetPos;
        _itemRT.sizeDelta = size;
        _inventoryRT = inventoryRT;
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
        transform.SetParent(_itemDraggingParent); //Drag시 부모변경(RectMask때문에)
        
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(_itemRT, eventData.position,
                eventData.pressEventCamera,
                out var globalMousePos))
        {
            OnPointerDownEvent?.Invoke(this, _id);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                _itemRT, eventData.position, eventData.pressEventCamera, out var globalMousePos))
        {
            _itemRT.position = globalMousePos;
        }
        
        //EVENT invoke
        OnDragEvent?.Invoke(this, globalMousePos, _id);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                _itemRT, eventData.position, eventData.pressEventCamera, out var globalMousePos))
        {
            _itemRT.position = globalMousePos;
        }
        
        OnEndDragEvent?.Invoke(this, globalMousePos, _id);
        
        //_isAvailable = false; //다시 초기화
        _canvasGroup.blocksRaycasts = true;
    }

    public void ReturnItemDrag()
    {
        _itemRT.SetParent(_itemParentRT);
        _itemRT.anchoredPosition = _pointerDownPos;
    }
}
