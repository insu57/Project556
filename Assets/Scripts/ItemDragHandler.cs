using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform _itemRT;
    private RectTransform _itemParentRT;
    private RectTransform _inventoryRT;

    public RectTransform InventoryRT => _inventoryRT;
    private Transform _itemDraggingParent;
    private Vector2 _pointerDownPos;
    private Vector2 _defaultImageSize;
    private Vector2 _slotImageSize;
    
    private CanvasGroup _canvasGroup;

    private InventoryUIPresenter _inventoryUIPresenter;
    private UIManager _uiManager;
    private int _widthCell;
    private int _heightCell;
    private Guid _id;
   
    private int _idx;
    private Image _itemImage;
    [SerializeField] private Image highlight;
    [SerializeField] private TMP_Text stackText;
    //private float _cellSize;
    
    public event Action<ItemDragHandler, Guid> OnPointerDownEvent;
    public event Action<ItemDragHandler, Vector2, Guid> OnDragEvent;
    public event Action<ItemDragHandler, Vector2, Guid> OnEndDragEvent;
    
    private void Awake()
    {
        _itemRT = GetComponent<RectTransform>();
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
        var cellSize = presenter.CellSize;

        var itemData = item.ItemData; //CellCount
        _widthCell = itemData.ItemWidth;
        _heightCell = itemData.ItemHeight;
  
        _defaultImageSize = new Vector2(_widthCell * cellSize, _heightCell * cellSize); //기본 크기
        _itemRT.sizeDelta = _defaultImageSize;
        _itemImage.sprite = itemData.ItemSprite;

        _id = item.Id;
        _itemDraggingParent = uiParent;
        
        
    }
    public void SetItemDragPos(Vector2 targetPos, Vector2 size, RectTransform itemParentRT, RectTransform inventoryRT)
    {
        _itemParentRT = itemParentRT;
        Vector2 pivotDelta = _itemParentRT.pivot - _itemRT.pivot; //pivot 차이
        Vector2 parentSize = itemParentRT.sizeDelta; //부모의 사이즈(위치 보정용)
        Vector2 offset = new Vector2(pivotDelta.x * parentSize.x, pivotDelta.y * parentSize.y); //pivot offest(위치보정)
        
        _itemRT.SetParent(itemParentRT); //부모 설정
        _itemRT.anchoredPosition = targetPos + offset; //위치보정
        _itemRT.sizeDelta = size;
        _slotImageSize = size;
        _inventoryRT = inventoryRT; //인벤토리의 RectTransform. null이면 GearSlot.
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
        
        _itemRT.sizeDelta = _defaultImageSize; //사이즈 조정.. 어떻게?
        
        OnPointerDownEvent?.Invoke(this, _id);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                _itemRT, eventData.position, eventData.pressEventCamera, out var globalMousePos))
        {
            _itemRT.position = globalMousePos; //item Drag Handler 위치 -> GlobalMousePos
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
        
        _canvasGroup.blocksRaycasts = true;
    }

    public void ReturnItemDrag()
    {
        _itemRT.SetParent(_itemParentRT);
        _itemRT.anchoredPosition = _pointerDownPos;
        _itemRT.sizeDelta = _slotImageSize;
    }
}
