using System;
using UnityEngine;

namespace Item
{
    public class LootCrate : MonoBehaviour
    {
        private SpriteRenderer _crateSpriteRenderer;
        private BoxCollider2D _crateBoxCollider2D;
        private Inventory _lootInventory;
        
        [SerializeField] private BaseCrateDataSO crateData;

        private void Awake()
        {
            //추후 StageManger에서
            TryGetComponent(out _crateSpriteRenderer);
            TryGetComponent(out _crateBoxCollider2D);
            
            _crateSpriteRenderer.sprite = crateData.CrateSprite;
            _crateBoxCollider2D.size = _crateSpriteRenderer.bounds.size;
            
            _lootInventory = Instantiate(crateData.InventoryPrefab, transform).GetComponent<Inventory>();
            _lootInventory.gameObject.SetActive(false);
            _lootInventory.Init(50, Guid.Empty); //개선?
            //닫고 다시 열 때 안보임
            //리그 해제 적용 오류 
        }

        public Inventory GetLootInventory()
        {
            return _lootInventory;
        }
    
    }
}
