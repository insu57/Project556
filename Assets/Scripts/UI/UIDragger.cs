using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace UI
{
    public class UIDragger : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        [SerializeField] private RectTransform parentRT;
        private Canvas _rootCanvas;
        private RectTransform _rootCanvasRT;
    
        private Vector2 _pointerOffset;

        private Vector2 _halfSize;
        private Vector2 _screenHalfSize;
    
        //anchor min 0.5, 0.5 max 0.5, 0.5 pivot 0.5, 0.5  
        private void Awake()
        {
            _rootCanvas = GetComponentInParent<Canvas>();
            _rootCanvasRT = _rootCanvas.transform as RectTransform;
        
            _halfSize = parentRT.sizeDelta * 0.5f;
            _screenHalfSize = new Vector2(960f, 540f); //1920x1080 Half
        }
    
        public void OnPointerDown(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRT, eventData.position, 
                eventData.pressEventCamera, out _pointerOffset);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rootCanvasRT, eventData.position, 
                    eventData.pressEventCamera, out var localPoint)) return;
            
            Vector2 newPos = localPoint - _pointerOffset;
            newPos = new Vector2(
                Mathf.Clamp(newPos.x, -_screenHalfSize.x + _halfSize.x, _screenHalfSize.x - _halfSize.x), 
                Mathf.Clamp(newPos.y, -_screenHalfSize.y + _halfSize.y, _screenHalfSize.y - _halfSize.y));

            parentRT.localPosition = newPos;
        }
    }
}
    
