using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragger : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    private RectTransform _rectTransform;
    private Canvas _rootCanvas;
    private Vector2 _pointerOffset;

    private Vector2 _halfSize;
    private Vector2 _screenHalfSize;
    
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _rootCanvas = GetComponentInParent<Canvas>();
        
        _halfSize = _rectTransform.sizeDelta * 0.5f;
        _screenHalfSize = new Vector2(960f, 540f); //1920x1080 Half
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rectTransform, eventData.position, eventData.pressEventCamera, out _pointerOffset);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(!_rootCanvas) return;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rootCanvas.transform as RectTransform, eventData.position, eventData.pressEventCamera,
                out var localPointerPos))
        {
            Vector2 newPosition = localPointerPos - _pointerOffset;
            newPosition = new Vector2(
                Mathf.Clamp(newPosition.x, -_screenHalfSize.x + _halfSize.x, _screenHalfSize.x - _halfSize.x), 
                Mathf.Clamp(newPosition.y, -_screenHalfSize.y + _halfSize.y, _screenHalfSize.y - _halfSize.y));  //값 조절 필요
            _rectTransform.anchoredPosition = newPosition;
        }
    }
}
