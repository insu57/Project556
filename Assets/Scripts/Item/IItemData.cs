using UnityEngine;

namespace Item
{
    public interface IItemData //아이템 데이터 인터페이스
    {
        public string ItemDataID { get; }
        public string ItemName { get; }
        public Sprite ItemSprite { get; }
        public int ItemWidth { get; } //아이템 크기
        public int ItemHeight { get; }
        public GearType GearType { get; }
        public float ItemWeight { get; } //아이템 무게(관련 시스템 구현 필요)
        public bool IsStackable { get; }
        public int MaxStackAmount { get; }
    }
}
 