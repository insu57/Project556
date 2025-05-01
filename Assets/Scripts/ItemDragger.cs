using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragger : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    private RectTransform _itemRT;
    private RectTransform _inventoryRT;
    private Canvas _rootCanvas;
    private Vector2 _pointerOffset;
    private Vector2 _pointerDownPos;
    private CanvasGroup _canvasGroup;
    
    private InventoryUI _inventoryUI;
    
    //anchor min 0.5, 0.5 max 0.5, 0.5 pivot 0.5, 0.5  
    private void Awake()
    {
        _itemRT = GetComponent<RectTransform>();
        _rootCanvas = GetComponentInParent<Canvas>();
        _canvasGroup = GetComponent<CanvasGroup>();
        
        _inventoryUI = GetComponentInParent<InventoryUI>();
        _inventoryRT = _inventoryUI.GetComponent<RectTransform>();
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
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //check position
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _inventoryUI.InventoryGridRT, eventData.position, eventData.pressEventCamera, out var localPoint
            )) return;

        if (!_inventoryUI.SlotEmptyCheck(localPoint)) //Empty가 아니거나 인벤토리 밖
        {
            _itemRT.anchoredPosition = _pointerDownPos;
        }
        else
        {
           _itemRT.anchoredPosition = _inventoryUI.ItemMove(_pointerDownPos, localPoint);
        }

        _canvasGroup.blocksRaycasts = true;
    }
    
}
