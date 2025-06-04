using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDragger : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform _itemRT;
    private RectTransform _itemParentRT;
    private RectTransform _inventoryRT;
    public RectTransform InventoryRT => _inventoryRT;
    private Transform _itemDraggingParent;
    private Vector2 _pointerOffset;
    private Vector2 _pointerDownPos;
    private CanvasGroup _canvasGroup;

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
    
    
    private void Awake()
    {
        _itemRT = GetComponent<RectTransform>();
        //_rootCanvas = GetComponentInParent<Canvas>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _itemImage = GetComponent<Image>();
    }

    public void Init(InventoryItem item, UIManager uiManager, RectTransform itemParent, RectTransform inventoryRT)
    {
        _uiManager = uiManager;
        _slotSize = uiManager.SlotSize;
        //_inventoryRT = inventoryRT;

        var itemData = item.ItemData;
        //_gearType = itemData.GearType;
        _widthSize = itemData.ItemWidth;
        _heightSize = itemData.ItemHeight;
        Vector2 imageSize = new Vector2(_widthSize * _slotSize, _heightSize * _slotSize);
        _itemRT.sizeDelta = imageSize;
        _itemImage.sprite = itemData.ItemSprite;

        _id = item.Id;
        
        //_itemDefaultParent = transform.parent;
        _itemParentRT = itemParent;
        _itemDraggingParent = uiManager.gameObject.transform;
        _inventoryRT = inventoryRT;
    }

    public void MoveItemDragger(Vector2 pos, RectTransform itemParentRT, RectTransform inventoryRT)
    {
        _itemRT.anchoredPosition = pos;
        _itemParentRT = itemParentRT;
        transform.SetParent(_itemParentRT);
        _inventoryRT = inventoryRT; //null -> GearSlot
        //_itemDefaultParent = transform.parent;
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
        //_inventoryManager.CheckSlotAvailable(globalMousePos);
        _uiManager.CheckItemSlot(this, _pointerDownPos, globalMousePos, _id);
            
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
        
        //check position...
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _itemParentRT, eventData.position, eventData.pressEventCamera, out var localPos
            )) return;
        
        
        //var firstSlot = GetFirstSlotPos(localPos);
        //_itemRT.anchoredPosition = _inventoryUI.ItemMove(_pointerDownPos, firstSlot.Item1, _id); //바꾸기...
        
        //_inventoryUI.DisableSlotAvailable();
        _canvasGroup.blocksRaycasts = true;
        //_itemImage.maskable = true;
       
    }

    
}
