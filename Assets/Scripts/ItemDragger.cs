using System;
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
    
    private InventoryUI _inventoryUI;
    private InventoryItem _item;
    //private IItemData _itemData;
    private int _widthSize;
    private int _heightSize;
    private Guid _id;
    private int _idx;
    //[SerializeField] private Image outline;
    private Image _itemImage;
    [SerializeField] private Image highlight;
    
    //anchor min 0.5, 0.5 max 0.5, 0.5 pivot 0.5, 0.5  
    private void Awake()
    {
        _itemRT = GetComponent<RectTransform>();
        //_rootCanvas = GetComponentInParent<Canvas>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _itemImage = GetComponent<Image>();
    }

    public void Init(IItemData itemData, Guid id)
    {
        _inventoryUI = GetComponentInParent<InventoryUI>();
        _inventoryRT = _inventoryUI.InventoryGridRT;
        
        _widthSize = itemData.ItemWidth;
        _heightSize = itemData.ItemHeight;
        Vector2 imageSize = new Vector2(_widthSize * 100, _heightSize * 100);
        _itemRT.sizeDelta = imageSize;
        _itemImage.sprite = itemData.ItemSprite;

        _id = id;
    }

    private (Vector2, Guid) GetFirstSlotPos(Vector2 mousePos)
    {
        float x = mousePos.x - 100 * _widthSize / 2f + 50;
        float y = mousePos.y - (-100 * _heightSize / 2f + 50) ;//
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
            //_inventoryUI
            Debug.Log(GetFirstSlotPos(localPos).Item1);
            
            _inventoryUI.CheckSlotAvailable(GetFirstSlotPos(localPos));
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //check position...
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _inventoryRT, eventData.position, eventData.pressEventCamera, out var localPoint
            )) return;

        _itemRT.anchoredPosition = _inventoryUI.ItemMove(_pointerDownPos, localPoint, _id);
        
        _inventoryUI.DisableSlotAvailable();
        _canvasGroup.blocksRaycasts = true;
    }

    
}
