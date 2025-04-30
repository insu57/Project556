using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragger : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    private RectTransform _rectTransform;
    private Canvas _rootCanvas;
    private RectTransform _rootRect;
    private Vector2 _pointerOffset;
    private Vector2 _pointerDownPos;
    private CanvasGroup _canvasGroup;
    
    private Vector2 _halfSize;
    private Vector2 _screenHalfSize;
    
    //anchor min 0.5, 0.5 max 0.5, 0.5 pivot 0.5, 0.5  
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _rootCanvas = GetComponentInParent<Canvas>();
        _rootRect = _rootCanvas.transform as RectTransform;
        _canvasGroup = GetComponent<CanvasGroup>();
        
        _halfSize = _rectTransform.sizeDelta * 0.5f;
        _screenHalfSize = new Vector2(960f, 540f); //1920x1080 Half
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                _rectTransform, eventData.position, eventData.pressEventCamera, out var globalMousePos))
        {
            _rectTransform.position = globalMousePos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _rectTransform.anchoredPosition = _pointerDownPos;
        _canvasGroup.blocksRaycasts = true;
    }
    
}
