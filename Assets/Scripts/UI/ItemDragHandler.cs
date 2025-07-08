using System;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ItemDragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler, 
    IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{
    private RectTransform _itemRT;
    private RectTransform _itemParentRT;
    private RectTransform _inventoryRT;

    public RectTransform InventoryRT => _inventoryRT;
    private Transform _itemDraggingParent;
    private Vector2 _pointerDownPos;
    private Vector2 _defaultImageSize;
    private Vector2 _slotImageSize;
    private bool _isRotated;
    
    private CanvasGroup _canvasGroup;

    private InventoryUIPresenter _inventoryUIPresenter;
    private ItemUIManager _itemUIManager;
    private int _widthCell;
    private int _heightCell;
    private Guid _id;
   
    private int _idx;
    [SerializeField] private Image itemImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image highlightImage;
    [FormerlySerializedAs("stackText")] [SerializeField] private TMP_Text countText;
    
    private InputAction _rotateItemAction;
    private bool _isDragging;
    public event Action<ItemDragHandler, Guid> OnPointerDownEvent;
    public event Action<ItemDragHandler, Vector2, Guid> OnDragEvent;
    public event Action<ItemDragHandler> OnEndDragEvent;
    public event Action<ItemDragHandler> OnRotateItemEvent;
    
    //이벤트 구독
    private void OnEnable()
    {
        if (!_inventoryUIPresenter) return;
        _inventoryUIPresenter.OnDisableItemDragHandler(this);//이벤트 중복 구독 방지
        _rotateItemAction.performed -= OnRotateItemAction;
        
        _inventoryUIPresenter.InitItemDragHandler(this);
        _rotateItemAction.performed += OnRotateItemAction;
        _rotateItemAction.Enable();
    }

    private void OnDisable()
    {
        if (!_inventoryUIPresenter) return;
        _inventoryUIPresenter.OnDisableItemDragHandler(this);
        _rotateItemAction.performed -= OnRotateItemAction;
        _rotateItemAction.Disable();
    }

    public void Init(ItemInstance item, InventoryUIPresenter presenter, InputAction itemRotateAction, 
        Transform uiParent)
    {
        _inventoryUIPresenter = presenter;
        _inventoryUIPresenter.InitItemDragHandler(this); //Enable인데 시작하면 안됨/disable이면 enable될 때 한번 더 -> 두번 호출
        var cellSize = presenter.CellSize;

        _itemRT = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        
        var itemData = item.ItemData; //CellCount
        _widthCell = itemData.ItemWidth;
        _heightCell = itemData.ItemHeight;
  
        _defaultImageSize = new Vector2(_widthCell * cellSize, _heightCell * cellSize); //기본 크기
      
        _itemRT.sizeDelta = _defaultImageSize; //이미지 스프라이트 설정
        itemImage.sprite = itemData.ItemSprite;
        itemImage.rectTransform.sizeDelta = _defaultImageSize;

        _id = item.InstanceID;
        _itemDraggingParent = uiParent;
        
        if(item.IsStackable) countText.enabled = true; //Stack 표시용 TMP Text
        
        _rotateItemAction = itemRotateAction;
        _rotateItemAction.performed += OnRotateItemAction;
    }
    public void SetItemDragPos(Vector2 targetPos, Vector2 size, RectTransform itemParentRT, RectTransform inventoryRT)
    {
        _itemParentRT = itemParentRT;
        Vector2 pivotDelta = _itemParentRT.pivot - _itemRT.pivot; //pivot 차이
        Vector2 parentSize = itemParentRT.sizeDelta; //부모의 사이즈(위치 보정용)
        Vector2 offset = new Vector2(pivotDelta.x * parentSize.x, pivotDelta.y * parentSize.y); //pivot offest(위치보정)
        
        _itemRT.SetParent(itemParentRT); //부모 설정
        _itemRT.localScale = Vector3.one; //world - canvas(overlay) scale 차이 -> 조정
        _itemRT.anchoredPosition = targetPos + offset; //위치보정
        _itemRT.sizeDelta = size;
        _slotImageSize = size;
        backgroundImage.rectTransform.sizeDelta = Vector2.zero;
        
        itemImage.rectTransform.sizeDelta = !_isRotated ? size : new Vector2(size.y, size.x); //회전된 상태면 size 반대로
        _inventoryRT = inventoryRT; //인벤토리의 RectTransform. null이면 GearSlot.
    }
    
    public void SetItemDragRotate(bool isRotated, Vector2 size)
    {
        _itemRT.sizeDelta = size;
        _defaultImageSize = size; //아이템 크기와 기본 이미지 사이즈 변경.(가로세로가 바뀌기 때문에 갱신)
         if (_inventoryRT) //회전할 때 원래 GearSlot이 아니면
         {
             _slotImageSize = size; //slot에서의 크기 변경(GearSlot은 Rotate가 안되기때문에 제외)
         }
        
        backgroundImage.rectTransform.sizeDelta = Vector2.zero; //stretch 이미지 갱신
        
        _isRotated = isRotated;
        float rotateZ = !isRotated ? 0 : 90;
        itemImage.rectTransform.rotation = Quaternion.Euler(0, 0, rotateZ); 
    }

    public void SetStackAmountText(int amount)
    {
        countText.text = amount.ToString();
    }

    //장전세분화?
    public void SetMagazineCountText(bool hasChamber, int amount)
    {
        if (hasChamber)
        {
            countText.text = amount + "<size=%75>+1</size>";
        }
        else
        {
            countText.text = amount.ToString();
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        highlightImage.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlightImage.enabled = false;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = false;
        _pointerDownPos = _itemRT.anchoredPosition;
        transform.SetParent(_itemDraggingParent); //Drag시 부모변경(RectMask때문에)
        
        _itemRT.sizeDelta = _defaultImageSize; // 클릭 시 원래 아이템 사이즈로
        backgroundImage.enabled = false; //배경 끄기
        
        itemImage.rectTransform.sizeDelta = !_isRotated ? _defaultImageSize 
             :new Vector2(_defaultImageSize.y, _defaultImageSize.x);
        
        OnPointerDownEvent?.Invoke(this, _id);
        _isDragging = true;
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = true;
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                _itemRT, eventData.position, eventData.pressEventCamera, out var globalMousePos))
        {
            _itemRT.position = globalMousePos; //item Drag Handler 위치 -> GlobalMousePos
        }
        
        //Events invoke
        OnDragEvent?.Invoke(this, globalMousePos, _id);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                _itemRT, eventData.position, eventData.pressEventCamera, out var globalMousePos))
        {
            _itemRT.position = globalMousePos; 
        }
        
        OnEndDragEvent?.Invoke(this);
        backgroundImage.enabled = true; //배경 다시 키기
        
        _canvasGroup.blocksRaycasts = true;
        
        _isDragging = false;
    }

    private void OnRotateItemAction(InputAction.CallbackContext context)
    {
        if (_isDragging)
        {
            OnRotateItemEvent?.Invoke(this);
        }
    }
    
    public void ReturnItemDrag() //회전된 상태 return -> 작음
    {
        _itemRT.SetParent(_itemParentRT);
        _itemRT.anchoredPosition = _pointerDownPos;
        
        _itemRT.sizeDelta = _slotImageSize; //현재 슬롯에 따라 크기 조절(GearSlot의 고정 이미지 사이즈 때문에)
        backgroundImage.rectTransform.sizeDelta = Vector2.zero;
        
        itemImage.rectTransform.sizeDelta = 
            !_isRotated ? _slotImageSize : new Vector2(_slotImageSize.y, _slotImageSize.x);
    }
}
