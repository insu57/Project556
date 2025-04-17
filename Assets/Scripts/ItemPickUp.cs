using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;
    private SpriteRenderer _spriteRenderer;

    public GameObject ItemPrefab => itemPrefab;
}
