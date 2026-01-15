using System;
using Player;
using UnityEngine;

public class ItemPickUp : MonoBehaviour, IFieldInteractable //오브젝트 풀링으로 관리.
{
    [SerializeField] private BaseItemDataSO itemData; //아이템 데이터.(StageManager에서 주입.  SerializeField(테스트 용))
    private ItemInstance _itemInstance;
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _collider;
    [SerializeField] private Material stencilHideMaterial;

    public void Init(ItemInstance item) //StageManager(맵 초기화), 아이템 드랍 등 -> Init)
    {
        _itemInstance = item;
        _spriteRenderer.sprite = item.ItemData.ItemSprite; //스프라이트 설정
        _collider.size = _spriteRenderer.sprite.bounds.size; //스프라이트 크기에 맞게 Collider크기조정
    }
    
    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _spriteRenderer.material = stencilHideMaterial;
        TryGetComponent(out _collider);
    }
    private void Start()
    {
        if (_itemInstance == null && itemData) //SerializeField로 데이터를 받으면
        {
            SetItemData(itemData);
        }
    }

    private void SetItemData(BaseItemDataSO newItemData) //아이템 데이터 초기화
    {
        if(itemData is null) return;
        itemData = newItemData;
        _spriteRenderer.sprite = newItemData.ItemSprite;
        _collider.size = _spriteRenderer.sprite.bounds.size * 2f;//Collider 크기(Sprite보다 크게)
        _itemInstance = ItemInstance.CreateItemInstance(itemData);
    }
    
    public void PlayerGetFieldInteractInfo(PlayerInteract playerInteract)
    {
        playerInteract.GetFieldItemData(_itemInstance, gameObject.transform.position);
    }
}
