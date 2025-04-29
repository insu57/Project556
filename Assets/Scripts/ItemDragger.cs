using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragger : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    Canvas _canvas;
    RectTransform _selfRt, _parentRt;
    Vector2 _pointerOffset;

    void Awake()
    {
        _selfRt   = GetComponent<RectTransform>();
        _canvas   = GetComponentInParent<Canvas>();
        _parentRt = _canvas.transform as RectTransform;
    }

    // 드래그 시작: 부모 로컬좌표계에서의 offset 계산
    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _parentRt, eventData.position, eventData.pressEventCamera,
            out Vector2 localMousePos);

        // 마우스 위치와 현재 anchoredPosition 차이
        _pointerOffset = localMousePos - _selfRt.anchoredPosition;
    }

    // 드래그 중: 같은 방식으로 새 위치 계산 → clamp → 적용
    public void OnDrag(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentRt, eventData.position, eventData.pressEventCamera,
                out Vector2 localMousePos))
            return;

        // offset 보정 후 원 위치
        Vector2 rawPos = localMousePos - _pointerOffset;

        // 부모 rect 내에서 '완전 노출' 되도록 min/max 계산
        // pivot.x * width 만큼 왼쪽/아래로 밀리고,
        // (1-pivot) * width 만큼 오른쪽/위로 밀린다
        Vector2 min = _parentRt.rect.min 
                      + Vector2.Scale(_selfRt.pivot, _selfRt.rect.size);
        Vector2 max = _parentRt.rect.max 
                      - Vector2.Scale(Vector2.one - _selfRt.pivot, _selfRt.rect.size);

        float x = Mathf.Clamp(rawPos.x, min.x, max.x);
        float y = Mathf.Clamp(rawPos.y, min.y, max.y);

        _selfRt.anchoredPosition = new Vector2(x, y);
    }
}
