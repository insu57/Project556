using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    [SerializeField] private BaseItemDataSO itemData;
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _collider;
    public GameObject ItemPrefab { get; private set; }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<BoxCollider2D>();
    }
    private void Start()
    {
        ItemPrefab = itemData.ItemPrefab;
        _spriteRenderer.sprite = itemData.ItemSprite;
        _collider.size = itemData.PickUpColliderSize;
    }
}
