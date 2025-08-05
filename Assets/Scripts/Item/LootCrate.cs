using System;
using UI;
using UnityEngine;

namespace Item
{
    [Serializable]
    public struct LootCrateItemInput
    {
        public BaseItemDataSO itemData;
        public int stackAmount;
    }
    
    public class LootCrate : MonoBehaviour
    {
        private SpriteRenderer _crateSpriteRenderer;
        private BoxCollider2D _crateBoxCollider2D;
        private Inventory _lootInventory;
        private InventoryUIPresenter _inventoryUIPresenter;
        [SerializeField] private BaseCrateDataSO crateData;
        [SerializeField] private LootCrateItemInput[] lootCrateItemInputs;
        public string CrateName => crateData.CrateName;
        private void Awake()
        {
            //추후 StageManger에서
            TryGetComponent(out _crateSpriteRenderer);
            TryGetComponent(out _crateBoxCollider2D);
            
            _crateSpriteRenderer.sprite = crateData.CrateSprite;
            _crateBoxCollider2D.size = _crateSpriteRenderer.bounds.size;
            
            _lootInventory = Instantiate(crateData.InventoryPrefab, transform).GetComponent<Inventory>();
            _lootInventory.gameObject.SetActive(false);
            _lootInventory.Init(GameManager.Instance.CellSize, Guid.Empty);
      
            //개선?
            _inventoryUIPresenter = FindAnyObjectByType<InventoryUIPresenter>();
        }

        private void Start()
        {
            _inventoryUIPresenter.SetItemToLootCrate(lootCrateItemInputs, _lootInventory);
        }

        public Inventory GetLootInventory()
        {
            return _lootInventory;
        }
    }
}
