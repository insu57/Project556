using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotPointer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private Image _slotImage;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color highlightColor;
    
    private void Awake()
    {
        _slotImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        _slotImage.color = defaultColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _slotImage.color = highlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _slotImage.color = defaultColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        
    }
    
}
