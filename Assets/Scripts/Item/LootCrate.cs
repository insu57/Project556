using UnityEngine;

namespace Item
{
    public class LootCrate : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer crateSpriteRenderer;
        [SerializeField] private BoxCollider2D crateBoxCollider2D;
        private Inventory _lootInventory;
        
        [SerializeField] private BaseCrateDataSO crateData;

        private void Awake()
        {
            //추후 StageManger에서
            crateSpriteRenderer.sprite = crateData.CrateSprite;
            crateBoxCollider2D.size = crateSpriteRenderer.bounds.size;
            if (!_lootInventory)
            {
                _lootInventory = Instantiate(crateData.InventoryPrefab, transform).GetComponent<Inventory>();
                _lootInventory.gameObject.SetActive(false);
            }
        }
    
    }
}
