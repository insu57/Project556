using System;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    [SerializeField] private BaseItemDataSO itemData;
    private ItemInstance _itemInstance;
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _collider;

    public void Init(ItemInstance item) //StageManager(맵 초기화), 아이템 드랍 등 -> Init)
    {
        _itemInstance = item;
        _spriteRenderer.sprite = item.ItemData.ItemSprite;
        _collider.size = _spriteRenderer.sprite.bounds.size;
    }
    
    private void Awake()
    {
        TryGetComponent(out _spriteRenderer);
        TryGetComponent(out _collider);
    }
    private void Start()
    {
        if (_itemInstance == null && itemData)
        {
            SetItemData(itemData);
        }
    }

    private void SetItemData(BaseItemDataSO newItemData)
    {
        if(itemData is null) return;
        itemData = newItemData;
        _spriteRenderer.sprite = newItemData.ItemSprite;
        _collider.size = _spriteRenderer.sprite.bounds.size;
        _itemInstance = ItemInstance.CreateItemInstance(itemData);
    }

    public ItemInstance GetItemInstance()
    {
        return _itemInstance;
    }
}
