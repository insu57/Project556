using System;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    [SerializeField] private BaseItemDataSO itemData;
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _collider;
    public Guid Id { get; private set; }
    //public GameObject ItemPrefab { get; private set; }

    public void Init(BaseItemDataSO itemData) //추후 사용(MapManager?(맵 초기화), 아이템 드랍 등 -> Init)
    {
        this.itemData = itemData;
        Id = Guid.NewGuid();
        //Guid 생성? (필드 아이템)
    }
    
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<BoxCollider2D>();
    }
    private void Start()
    {
        //ItemPrefab = itemData.ItemPrefab;
        _spriteRenderer.sprite = itemData.ItemSprite;
        _collider.size = itemData.PickUpColliderSize;
    }

    public IItemData GetItemData()
    {
        return itemData;
    }
}
