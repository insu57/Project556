using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

namespace UI
{
    public enum ItemDragAction { Rotate, QuickAddItem, QuickDropItem, SetQuickSlot}
    public class ItemDragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler, 
        IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerClickHandler
    {
        private RectTransform _itemRT;
        private RectTransform _itemParentRT;
        private CanvasGroup _canvasGroup;
        public RectTransform InventoryRT { get; private set; }

        private Transform _itemDraggingParent;
        private Vector2 _pointerDownPos;
        private Vector2 _defaultImageSize;
        private Vector2 _slotImageSize;
        private bool _isRotated;

        private InventoryUIPresenter _inventoryUIPresenter;
        private ItemUIManager _itemUIManager;

        public Guid InstanceID { private set; get; }

        [SerializeField] private Image itemImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image highlightImage;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private Image keyImage;
        [SerializeField] private List<Sprite> keySprites;
        
        private Dictionary<ItemDragAction, InputAction> _inputActions;
    
        private bool _isOnPointerEnter;
        private bool _isDragging;
        public event Action<ItemDragHandler> OnPointerDownEvent;
        public event Action<ItemDragHandler, Vector2> OnDragEvent;
        public event Action<ItemDragHandler> OnEndDragEvent;
        public event Action<ItemDragHandler> OnRotateItem;
        public event Action<ItemDragHandler> OnQuickAddItem;
        public event Action<ItemDragHandler> OnQuickDropItem;
        public event Action<ItemDragHandler, QuickSlotIdx> OnSetQuickSlot;
        public event Action<ItemDragHandler> OnOpenItemContextMenu;
        public event Action<ItemDragHandler> OnShowItemInfo;
        
        private void Awake()
        {
            TryGetComponent(out _canvasGroup);
        }
        
        //이벤트 구독
        private void OnEnable()
        {
            if (!_inventoryUIPresenter) return;
            _inventoryUIPresenter.InitItemDragHandler(this);
            SubscribeInputEvents();
        }

        private void OnDisable()
        {
            if (!_inventoryUIPresenter) return;
            _inventoryUIPresenter.OnDisableItemDragHandler(this);
            UnsubscribeInputEvents();
        }

        private void SubscribeInputEvents()
        {
            _inputActions[ItemDragAction.Rotate].performed += OnRotateItemAction;
            _inputActions[ItemDragAction.QuickAddItem].performed += OnQuickAddItemAction;
            _inputActions[ItemDragAction.QuickDropItem].performed += OnQuickDropItemAction;
            _inputActions[ItemDragAction.SetQuickSlot].performed += OnSetQuickSlotAction;
        }

        private void UnsubscribeInputEvents()
        {
            _inputActions[ItemDragAction.Rotate].performed -= OnRotateItemAction;
            _inputActions[ItemDragAction.QuickAddItem].performed -= OnQuickAddItemAction;
            _inputActions[ItemDragAction.QuickDropItem].performed -= OnQuickDropItemAction;
            _inputActions[ItemDragAction.SetQuickSlot].performed -= OnSetQuickSlotAction;
        }

        public void Init(ItemInstance item, InventoryUIPresenter presenter, 
            Dictionary<ItemDragAction, InputAction> inputActions, Transform uiParent)
        {
            _inventoryUIPresenter = presenter;
            _inventoryUIPresenter.InitItemDragHandler(this); //Enable인데 시작하면 안됨/disable이면 enable될 때 한번 더 -> 두번 호출
            var cellSize = presenter.CellSize;

            _itemRT = GetComponent<RectTransform>();
        
            var itemData = item.ItemData; //CellCount
            var widthCell = itemData.ItemWidth;
            var heightCell = itemData.ItemHeight;
            _defaultImageSize = new Vector2(widthCell * cellSize, heightCell * cellSize); //기본 크기
      
            _itemRT.sizeDelta = _defaultImageSize; //이미지 스프라이트 설정
            itemImage.sprite = itemData.ItemSprite;
            itemImage.rectTransform.sizeDelta = _defaultImageSize;

            InstanceID = item.InstanceID;
            _itemDraggingParent = uiParent;
        
            if(item.IsStackable) countText.enabled = true; //Stack 표시용 TMP Text
            if (item is WeaponInstance) countText.enabled = true;
            keyImage.enabled = false;
            
            _inputActions = inputActions;
            SubscribeInputEvents();
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
            InventoryRT = inventoryRT; //인벤토리의 RectTransform. null이면 GearSlot.
        }
    
        public void SetItemDragRotate(bool isRotated, Vector2 size)
        {
            _itemRT.sizeDelta = size;
            _defaultImageSize = size; //아이템 크기와 기본 이미지 사이즈 변경.(가로세로가 바뀌기 때문에 갱신)
            if (InventoryRT) //회전할 때 원래 GearSlot이 아니면
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
        public void SetMagazineCountText(bool isFullyLoaded, int amount) //방식 변경? 꽉찼을때만?
        {
            if (isFullyLoaded)
            {
                countText.text = amount - 1 + "<size=75%>+1</size>";
            }
            else
            {
                countText.text = amount.ToString();
            }
        }

        public void SetQuickSlotKey(int keyNum)
        {
            int idx = keyNum - 4;//4~7
            keyImage.sprite = keySprites[idx];
            keyImage.enabled = true;
        }

        public void ClearQuickSlotKey()
        {
            keyImage.enabled = false;
        }
        
        //뒤에 있는 Cell 하이라이트 문제(안꺼짐)...raycast를 막으면 click 이벤트 불가...
        public void OnPointerEnter(PointerEventData eventData)
        {
            if(_isDragging) return;
            highlightImage.enabled = true;
            _isOnPointerEnter = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            highlightImage.enabled = false;
            _isOnPointerEnter = false;
        }
    
        public void OnPointerDown(PointerEventData eventData)
        {
            highlightImage.enabled = false;
            _pointerDownPos = _itemRT.anchoredPosition;
            transform.SetParent(_itemDraggingParent); //Drag시 부모변경(RectMask때문에)
        
            _itemRT.sizeDelta = _defaultImageSize; // 클릭 시 원래 아이템 사이즈로
            
            itemImage.raycastTarget = false;
            //배경 투명화
            var transparent = backgroundImage.color;
            transparent.a = 0;
            backgroundImage.color = transparent;
        
            itemImage.rectTransform.sizeDelta = !_isRotated ? _defaultImageSize 
                :new Vector2(_defaultImageSize.y, _defaultImageSize.x);
        
            OnPointerDownEvent?.Invoke(this);
            _isDragging = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            itemImage.raycastTarget = true;
            _isDragging = false;
            //배경 투명화
            var transparent = backgroundImage.color;
            transparent.a = 1;
            backgroundImage.color = transparent;
        
            ReturnItemDrag();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
             
            if (eventData.clickCount == 2)//double click
            {
                Debug.Log("double click");
                OnShowItemInfo?.Invoke(this);
            }

            if (eventData.button == PointerEventData.InputButton.Right) //right-click
            {
                OnOpenItemContextMenu?.Invoke(this);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    _itemRT, eventData.position, eventData.pressEventCamera, out var globalMousePos))
            {
                _itemRT.position = globalMousePos; //item Drag Handler 위치 -> GlobalMousePos
            }
        
            //Events invoke
            OnDragEvent?.Invoke(this, globalMousePos);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            itemImage.raycastTarget = true;
            //배경 투명화
            var transparent = backgroundImage.color;
            transparent.a = 1;
            backgroundImage.color = transparent;
            
            _isDragging = false;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    _itemRT, eventData.position, eventData.pressEventCamera, out var globalMousePos))
            {
                _itemRT.position = globalMousePos; 
            }
        
            OnEndDragEvent?.Invoke(this);
            
        }

        private void OnRotateItemAction(InputAction.CallbackContext context)
        {
            //Keyboard 'R'
            if (_isDragging)
            {
                OnRotateItem?.Invoke(this);
            }
        }

        private void OnQuickAddItemAction(InputAction.CallbackContext context)
        {
            //LCtrl + Mouse0
            if (_isOnPointerEnter)
            {
                OnQuickAddItem?.Invoke(this);
            }
        }

        private void OnQuickDropItemAction(InputAction.CallbackContext context)
        {
            //Keyboard 'Delete'
            if (_isOnPointerEnter)
            {
                OnQuickDropItem?.Invoke(this);
            }
        }

        private void OnSetQuickSlotAction(InputAction.CallbackContext context)
        {
            if (!_isOnPointerEnter) return;
            if (context.control is not KeyControl key) return;
            
            int keyNum = key.keyCode - Key.Digit1 + 1; //4~7
            OnSetQuickSlot?.Invoke(this, (QuickSlotIdx)keyNum);
        }
    
        public void ReturnItemDrag() //회전된 상태 return -> 작음
        {
            _itemRT.SetParent(_itemParentRT);
            _itemRT.anchoredPosition = _pointerDownPos;
        
            _itemRT.sizeDelta = _slotImageSize; //현재 슬롯에 따라 크기 조절(GearSlot의 고정 이미지 사이즈 때문에)
            backgroundImage.rectTransform.sizeDelta = Vector2.zero;
        
            itemImage.rectTransform.sizeDelta =  //회전 여부에 따라
                !_isRotated ? _slotImageSize : new Vector2(_slotImageSize.y, _slotImageSize.x);
        }
    }
}