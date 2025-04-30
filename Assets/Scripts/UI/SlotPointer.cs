using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotPointer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private Image _slotImage;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color highlightColor;
    
    private InventoryUI _inventoryUI;

    public bool IsEmpty = true;
    
    private void Awake()
    {
        _slotImage = GetComponent<Image>();
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
    
    //슬롯 위치 Check..?
    //다른 코드 참조!!!
    //슬롯마다? 부모 인벤토리 클래스에서? 어디서 관리하나?
    //슬롯의 상태...IsEmpty?
}
