using System;
using Item;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemInstance //생성된 아이템 인스턴스
{
    public Guid InstanceID { get; } //인스턴스ID
    public Vector2Int ItemCellCount { get ; private set; } //아이템 크기
    public IItemData ItemData { get; } //아이템 데이터

    public GearType GearType => ItemData.GearType; //장비타입
    public bool IsStackable => ItemData.IsStackable; //스택가능 여부
    public int MaxStackAmount => ItemData.MaxStackAmount; //최대 스택량
    public int CurrentStackAmount { get; private set; } //현재 스택량
    public bool IsRotated { get; private set; } //회전 여부(ItemDrag)
    public Inventory ItemInventory {get; private set;} //장비 인벤토리
    public float Durability { get; private set; } //내구도 (구현 필요)

    protected ItemInstance(IItemData itemData) //Init 인스턴스 생성 시
    {
        ItemData = itemData;
        InstanceID = Guid.NewGuid(); //새로운 ID
        CurrentStackAmount = IsStackable ? 0 : 1; //Stackable 이면 0부터 -> 수정?
        ItemCellCount = new Vector2Int(ItemData.ItemWidth, ItemData.ItemHeight);
        Durability = 100f;
    }

    public void SetItemInventory(Inventory itemInventory) //인벤토리 초기화 시 대입
    {
        if(InstanceID != itemInventory.ItemInstanceID) return;
        ItemInventory = itemInventory;
    }
    
    public void AdjustStackAmount(int stackAmount) //스택수치 조정
    {
        CurrentStackAmount += stackAmount;
    }

    public void AdjustDurability(float durability)
    {
        Durability += durability;
    }

    public void SetStackAmount(int stackAmount) //스택 수치 설정(초기화 시)
    {
        CurrentStackAmount = stackAmount;
    }

    public void SetDurability(float durability)
    {
        Durability = durability;   
    }
    
    public void RotateItem() //아이템 회전
    {
        IsRotated = !IsRotated;
        if (!IsRotated) //일반
        {
            ItemCellCount = new Vector2Int(ItemData.ItemWidth, ItemData.ItemHeight);
        }
        else //회전
        {
            ItemCellCount = new Vector2Int(ItemData.ItemHeight, ItemData.ItemWidth);       
        }
    }

    public static ItemInstance CreateItemInstance(IItemData itemData) //ItemInstance 생성(ItemPickup 등에서 사용) 
    {
        if (itemData is WeaponData weaponData) //Weapon
        {
            var weaponInstance = new WeaponInstance(weaponData); //무기 인스턴스로 생성
            var randMagCount = Random.Range(1, weaponData.DefaultMagazineSize + 1); //무작위 탄 개수
            weaponInstance.SetMagazineCount(randMagCount);
         
            return weaponInstance;
        }
        
        //일반 아이템 인스턴스
        var itemInstance = new ItemInstance(itemData);

        if (itemData.IsStackable) //stackable인 경우
        {
            var randStack = Random.Range(1, itemData.MaxStackAmount + 1); //무작위 스택
            itemInstance.SetStackAmount(randStack);
        }

        return itemInstance;
    }
   
}