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
    private bool _isAvailable;
    
    private CanvasGroup _canvasGroup;

    private InventoryUIPresenter _inventoryUIPresenter;
    private UIManager _uiManager;
    private int _widthSize;
    private int _heightSize;
    private Guid _id;
    //private GearType _gearType;
    private int _idx;
    //[SerializeField] private Image outline;
    private Image _itemImage;
    [SerializeField] private Image highlight;
    private float _slotSize;
    
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

    public void Init(InventoryItem item, InventoryUIPresenter presenter
        , RectTransform itemParent, RectTransform inventoryRT, Transform uiParent)
    {
        //_uiManager = uiManager;
        _inventoryUIPresenter = presenter;
        _inventoryUIPresenter.InitItemDragHandler(this);
        _slotSize = presenter.SlotSize;
        //_inventoryRT = inventoryRT;

        var itemData = item.ItemData;
        //_gearType = itemData.GearType;
        _widthSize = itemData.ItemWidth;
        _heightSize = itemData.ItemHeight;
        Debug.Log(_slotSize);
        Vector2 imageSize = new Vector2(_widthSize * _slotSize, _heightSize * _slotSize);
        _itemRT.sizeDelta = imageSize;
        _itemImage.sprite = itemData.ItemSprite;

        _id = item.Id;
        
        _itemParentRT = itemParent;
        _itemDraggingParent = uiParent;
        _inventoryRT = inventoryRT;
        _isAvailable = false;
    }
    
    public void SetTargetPosItemDragger(Vector2 targetPos, RectTransform itemParentRT,
        RectTransform inventoryRT, bool isAvailable)//크기는??? GearSlot vs InvenSlot 
    {
        //_itemRT.anchoredPosition = pos;
        _isAvailable = isAvailable;
        if (!isAvailable) return;
        _targetPos = targetPos;
        //이것도 EndDrag에서...
        _targetItemParentRT = itemParentRT;
        //transform.SetParent(_itemParentRT);//부모설정
        _targetInventoryRT = inventoryRT; //null -> GearSlot
        
    }
    
    private (Vector2 pos, Guid id) GetFirstSlotPos(Vector2 mousePos)
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
        transform.SetParent(_itemDraggingParent);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                _itemRT, eventData.position, eventData.pressEventCamera, out var globalMousePos))
        {
            _itemRT.position = globalMousePos;
        }
        //_inventoryManager.CheckSlotAvailable(globalMousePos);
        
        //EVENT invoke
        OnDragEvent?.Invoke(this, globalMousePos, _id);
        //_uiManager.CheckItemSlot(this,globalMousePos, _id);
            
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _itemParentRT, eventData.position, eventData.pressEventCamera, out var localPos))
        {
            //다른 RT에서도 인벤토리 슬롯 구분... 위의 globalMouse로 check...-> 어떤 인벤인지...
            //_inventoryUI
            //Debug.Log(GetFirstSlotPos(localPos).Item1);
            
            //_inventoryUI.CheckSlotAvailable(GetFirstSlotPos(localPos));
        }
        
        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(_itemParentRT);
        
        
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                _itemRT, eventData.position, eventData.pressEventCamera, out var globalMousePos))
        {
            _itemRT.position = globalMousePos;
        }
        
        OnEndDragEvent?.Invoke(this, globalMousePos, _id);
        
        if (_isAvailable) //현재 슬롯이 가능한경우
        {
            _itemParentRT = _targetItemParentRT;
            transform.SetParent(_itemParentRT);
            _itemRT.anchoredPosition = _targetPos;
            _inventoryRT = _targetInventoryRT;
        }
        else //불가능하면 원래 위치로...
        {
            _itemRT.anchoredPosition = _pointerDownPos;
        }
        
        //check position...
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _itemParentRT, eventData.position, eventData.pressEventCamera, out var localPos
            )) return;
        
        
        //var firstSlot = GetFirstSlotPos(localPos);
        //_itemRT.anchoredPosition = _inventoryUI.ItemMove(_pointerDownPos, firstSlot.Item1, _id); //바꾸기...
        //_inventoryUI.DisableSlotAvailable();
        
        _isAvailable = false; //다시 초기화
        _canvasGroup.blocksRaycasts = true;
    }

    
}
