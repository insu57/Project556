using System;
using UI;
using UnityEngine;

namespace Item
{
    [Serializable]
    public struct LootCrateItemInput //상자에 들어갈 아이템 
    {
        public BaseItemDataSO itemData;
        public int stackAmount;
    }
    
    public class LootCrate : MonoBehaviour
    {
        private SpriteRenderer _crateSpriteRenderer;
        private BoxCollider2D _crateBoxCollider2D;
        private Inventory _lootInventory; //상자 인벤토리
        private InventoryUIPresenter _inventoryUIPresenter;
        [SerializeField] private CrateData crateData; //상자 데이터
        [SerializeField] private LootCrateItemInput[] lootCrateItemInputs; //상자 아이템(추후 Stage Manager초기화)
        public string CrateName => crateData.CrateName;
        private void Awake()
        {
            //추후 StageManger에서
            TryGetComponent(out _crateSpriteRenderer);
            TryGetComponent(out _crateBoxCollider2D);
            
            _crateSpriteRenderer.sprite = crateData.CrateSprite; //크기 조절
            _crateBoxCollider2D.size = _crateSpriteRenderer.bounds.size;
            
            _lootInventory = Instantiate(crateData.InventoryPrefab, transform).GetComponent<Inventory>(); //상자 인벤토리 초기화
            _lootInventory.gameObject.SetActive(false);
            _lootInventory.Init(GameManager.Instance.CellSize, Guid.Empty);
            //StageManager 초기화 함수  수정예정
            
            _inventoryUIPresenter = FindAnyObjectByType<InventoryUIPresenter>();
        }

        private void Start()
        {
            _inventoryUIPresenter.SetItemToLootCrate(lootCrateItemInputs, _lootInventory); //아이템 초기화(ItemDrag)
        }

        public Inventory GetLootInventory() //상자 인벤토리
        {
            return _lootInventory;
        }
    }
}
