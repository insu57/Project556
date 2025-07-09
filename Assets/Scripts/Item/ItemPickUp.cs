using System;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    [SerializeField] private BaseItemDataSO itemData;
    private ItemInstance _itemInstance;
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _collider;
    //public Guid ID { get; private set; }
    //stack amount, weapon mag count
    public void Init(BaseItemDataSO itemData) //추후 사용(MapManager?(맵 초기화), 아이템 드랍 등 -> Init)
    {
        this.itemData = itemData;
        //ID = Guid.NewGuid();
        //Guid 생성? (필드 아이템)
        SetItemData(itemData);
    }
    
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<BoxCollider2D>();
        //ID = Guid.Empty;
    }
    private void Start()
    {
        SetItemData(itemData);
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
