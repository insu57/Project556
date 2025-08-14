using System;
using System.Collections.Generic;
using Item;
using UnityEngine;

public class InventoryManager : MonoBehaviour //인벤토리 매니저(장비슬롯 정보, 인벤토리 관리)
{
    //Left Panel
    public ItemInstance HeadwearItem { get; private set; } //아이템 데이터
    public CellData HeadwearSlot { get; } = new(GearType.HeadWear, Vector2.zero); //해당 Cell 데이터
    public ItemInstance EyewearItem { get; private set; }
    public CellData EyewearSlot { get; } = new(GearType.EyeWear, Vector2.zero);
    public ItemInstance BodyArmorItem { get; private set; }
    public CellData BodyArmorSlot { get; } = new(GearType.BodyArmor, Vector2.zero);
    public ItemInstance PrimaryWeaponItem { get; private set; }
    public CellData PrimaryWeaponSlot { get; } = new(GearType.Weapon, Vector2.zero);
    public ItemInstance SecondaryWeaponItem { get; private set; }
    public CellData SecondaryWeaponSlot { get; } = new(GearType.Weapon, Vector2.zero);
    public Dictionary<Guid, (ItemInstance item, CellData cell)> ItemDict { get; } = new(); //장비 슬롯 아이템 Dictionary
    
    //Middle Panel
    public ItemInstance ChestRigItem { get;  private set; }
    public CellData ChestRigSlot { get; } = new(GearType.ArmoredRig, Vector2.zero);
    public CellData[] PocketSlots { get; } = new CellData[4]; //주머니 CellData
    public ItemInstance BackpackItem { get; private set;  }
    public CellData BackpackSlot { get; } = new(GearType.Backpack, Vector2.zero);
    public Inventory BackpackInventory { get; private set; } //장비 인벤토리
    public Inventory RigInventory { get; private set; }

    //Right Panel
    public Inventory LootInventory { get; private set; } //루팅 인벤토리
    
    //QuickSlot
    public Dictionary<QuickSlotIdx, (Guid ID, Inventory inventory)> QuickSlotDict { get; } = new(); //퀵슬롯 Dictionary
   

    //To ItemUI Presenter
    public event Action<GameObject, ItemInstance> OnInitInventory;  //인벤토리 초기화 이벤트.  (인벤토리, 해당 인벤토리 아이템.)
    //장비 인벤토리 관련 초기화 개선 필요(Stage초기화)
    public event Action<ItemInstance> OnShowInventory; //인벤토리 active(이미 초기화가 된 경우)
    public event Action<LootCrate> OnSetLootInventory; //Loot인벤토리 처리
    public event Action<CellData, ItemInstance> OnEquipFieldItem; //필드 아이템 장착 이벤트.(장비 Cell, 아이템)
    public event Action<GearType, Vector2, RectTransform ,ItemInstance> OnAddFieldItemToInventory;
    //필드아이템 획득.(인벤토리 장비타입, 슬롯에서 위치, 아이템RT(부모), 아이템)
    public event Action<Guid, int> OnUpdateItemStack; //아이템 스택량 ItemDragUI 업데이트
    public event Action<Guid, bool, int> OnUpdateWeaponItemMagCount; //무기 장탄량 업데이트(ItemDrag)
    public event Action<Guid> OnRemoveItemFromPlayer; //아이템 필드로 떨구기
    public event Action<Guid, QuickSlotIdx> OnRemoveQuickSlotItem; //퀵슬롯에서 제거
    public event Action<float, GearType> OnRemoveItemInventory; //인벤토리 제거 시 UI 재정렬(float 인벤토리 높이(y))
    
    //To PlayerManager
    public event Action<float> OnUpdateArmorAmount; //방어도 총합 업데이트
    public event Action<EquipWeaponIdx> OnUnequipWeapon; // 무기 장비 해제 이벤트
    public event Action<IConsumableItem> OnUseItem; //소비 아이템 사용 이벤트
    
    private void Awake()
    {
        for (int i = 0; i < 4; i++) //PocketSlot 4 초기화
        {
            PocketSlots[i] = new CellData(GearType.None, Vector2.zero);
        }
        
        QuickSlotDict.Add(QuickSlotIdx.QuickSlot4, (Guid.Empty, null));
        QuickSlotDict.Add(QuickSlotIdx.QuickSlot5, (Guid.Empty, null));
        QuickSlotDict.Add(QuickSlotIdx.QuickSlot6, (Guid.Empty, null));
        QuickSlotDict.Add(QuickSlotIdx.QuickSlot7, (Guid.Empty, null)); //QuickSlot Dictionary 초기화
    }
    
    public void SetInventoryData(Inventory inventory, GearType gearType)  //인벤토리 데이터 할당
    {
        //인벤토리 설정
        switch (gearType)
        {
            case GearType.ArmoredRig: 
            case GearType.UnarmoredRig:
                RigInventory = inventory; //리그 인벤토리
                inventory.OnItemRemovedCheckQuickSlot += CheckRemoveItemIsQuickSlot; //아이템이 이동했을때 퀵슬롯 체크.
                break;
            case GearType.Backpack:
                BackpackInventory = inventory; //가방 인벤토리
                break;
            case GearType.None: //Loot는 제거?
                LootInventory = inventory;
                break;
        }
    }

    public void SetLootInventory(LootCrate lootCrate) //LootInventory 설정
    {
        var inventory = lootCrate?.GetLootInventory(); //LootCrate의 인벤토리
        LootInventory = inventory; //할당
        OnSetLootInventory?.Invoke(lootCrate); //UI 인벤토리 설정
    }

    public CellData CheckCanEquipItem(GearType gearType, Vector2Int itemCellCount) //GearSlot 장착 가능 여부 확인
    {
        switch (gearType)
        {
            case GearType.ArmoredRig:
                if(!BodyArmorSlot.IsEmpty || !ChestRigSlot.IsEmpty) return null;//BodyArmor와 동시장착 불가
                return ChestRigSlot; 
            case GearType.UnarmoredRig:
                return ChestRigSlot.IsEmpty ? ChestRigSlot : null; //리그 슬롯 체크
            case GearType.BodyArmor:
                if (ChestRigSlot.IsEmpty) return BodyArmorSlot.IsEmpty ? BodyArmorSlot : null; 
                //리그가 비었으면 방탄복 슬롯만 체크
               
                var rigID = ChestRigSlot.InstanceID;
                var rigItemType = ItemDict[rigID].item.GearType;
                if (rigItemType is GearType.ArmoredRig) return null; //방탄리그와 동시장착 불가
                return BodyArmorSlot.IsEmpty ? BodyArmorSlot : null;
            case GearType.Backpack:
                return BackpackSlot.IsEmpty ? BackpackSlot : null; //각 슬롯 체크
            case GearType.HeadWear:
                return HeadwearSlot.IsEmpty ? HeadwearSlot : null;
            case GearType.EyeWear:
                return EyewearSlot.IsEmpty ? EyewearSlot : null;
            case GearType.Weapon:
                if (PrimaryWeaponSlot.IsEmpty) //무기슬롯 1,2 체크
                {
                    return PrimaryWeaponSlot;
                }
                if (SecondaryWeaponSlot.IsEmpty)
                {
                    return SecondaryWeaponSlot;
                }
                break;
            case GearType.None: //Pockets
                if (itemCellCount.x > 1 || itemCellCount.y > 1) return null;//크기 초과(1x1 크기제한)
                for (int i = 0; i < 4; i++)
                {
                    if (PocketSlots[i].IsEmpty) return PocketSlots[i];
                }
                break;
        }
        return null;
    }

    public void SetGearItem(CellData gearSlot, ItemInstance item) //장비 아이템 장착
    {
        ItemDict[item.InstanceID] = (item, gearSlot); //ItemDict 추가(아이템데이터, 장비 Cell)
        gearSlot.SetEmpty(false, item.InstanceID); //Cell설정
        
        SetGearItemData(gearSlot, item); //슬롯-데이터 할당

        switch (item.GearType)
        {
            case GearType.ArmoredRig:
            case GearType.UnarmoredRig:
            case GearType.Backpack:
                if (item.ItemInventory) //이미 인벤토리를 생성했다면
                {
                    OnShowInventory?.Invoke(item); //인벤토리 UI 활성화 이벤트
                    break;
                }
                var gearData = item.ItemData as GearData; 
                if(gearData) OnInitInventory?.Invoke(gearData.SlotPrefab, item); //아니라면 초기화 이벤트
                break;
        }
        //장비 능력치, 무기 설정
        OnUpdateArmorAmount?.Invoke(GetTotalArmorAmount()); //방어도 업데이트 이벤트
    }

    public void RemoveGearItem(Guid itemID) //장비 장착 해제
    {
        var gearType = ItemDict[itemID].item.GearType; //해당 장비 타입
        var item = ItemDict[itemID].item;
        
        switch (gearType)//장비 타입(슬롯 종류)
        {
            case GearType.ArmoredRig or GearType.UnarmoredRig or GearType.Backpack: //인벤토리
            {
                //데이터 저장 관련 구현 필요(씬 이동 등)
                var inventory = ItemDict[itemID].item.ItemInventory; //장비 인벤토리
                inventory.gameObject.SetActive(false); //비활성
                if (gearType is GearType.ArmoredRig or GearType.UnarmoredRig) //아이템 이동 시 퀵슬롯체크 이벤트 구독해제
                {
                    inventory.OnItemRemovedCheckQuickSlot -= CheckRemoveItemIsQuickSlot;
                }
                OnRemoveItemInventory?.Invoke(inventory.Height, gearType);//ItemUI재정렬

                if (gearType is GearType.ArmoredRig or GearType.UnarmoredRig) RigInventory = null; //인벤토리 할당 초기화
                else BackpackInventory = null;
                break;
            }
            case GearType.Weapon:
            {
                //비우기 무장해제
                var cell = ItemDict[itemID].cell; //해당 무기 Cell
                if (cell == PrimaryWeaponSlot) //슬롯별 무기장비해제 이벤트 호출
                {
                    OnUnequipWeapon?.Invoke(EquipWeaponIdx.Primary);
                }
                else if (cell == SecondaryWeaponSlot)
                {
                    OnUnequipWeapon?.Invoke(EquipWeaponIdx.Secondary);
                }

                break;
            }
            case GearType.None: //Pocket 
            {
                if(item.ItemData is IConsumableItem) 
                    CheckRemoveItemIsQuickSlot(itemID); //퀵슬롯에 등록 되어있는지 체크
                break;
            }
        }
        var gearSlot =  ItemDict[itemID].cell; //해당Cell
        ItemDict.Remove(itemID); //ItemDict에서 제거
        gearSlot.SetEmpty(true, Guid.Empty); //Cell Empty로
        SetGearItemData(gearSlot, null); //Cell null 초기화
    }

    private void CheckRemoveItemIsQuickSlot(Guid id) //아이템 제거(이동)시 퀵슬롯으로 등록되었는지 체크
    {
        for (var slotIdx = QuickSlotIdx.QuickSlot4; slotIdx <= QuickSlotIdx.QuickSlot7; slotIdx++) //4~7번 퀵슬롯 체크
        {
            var (itemID, inventory) = QuickSlotDict[slotIdx]; //퀵슬롯 Dict
            if(!itemID.Equals(id)) continue; //퀵슬롯의 ID와 동일한지 비교
            QuickSlotDict[slotIdx] = (Guid.Empty, null); //동일하면 퀵슬롯에서 비우기
            OnRemoveQuickSlotItem?.Invoke(id, slotIdx); //퀵슬롯 제거 이벤트
        }
    }
    
    public void EquipFieldItem(CellData gearSlot, ItemInstance item) //필드 아이템 장착
    {
        SetGearItem(gearSlot, item); //장비 장착 메서드
        OnEquipFieldItem?.Invoke(gearSlot, item); //아이템 장착(ItemDrag UI) 이벤트
    }

    public void UseQuickSlotItem(QuickSlotIdx idx) //퀵슬롯 아이템 사용
    {
        var (id, inventory) = QuickSlotDict[idx]; //해당 퀵슬롯 정보
        
        UseItem(id, inventory); //아이템 사용 메서드
    }

    public void UseItem(Guid id, Inventory inventory)//아이템 사용
    {
        ItemInstance item;
        if (!inventory) item = ItemDict[id].item; //리그 인벤토리 인지
        else  item = inventory.ItemDict[id].item; //주머니인지 

        if (item.CurrentStackAmount > 1) //아이템 스택이 1보다 크다면
        {
            item.AdjustStackAmount(-1); //스택감소
            OnUpdateItemStack?.Invoke(id, item.CurrentStackAmount); //UI스택표시 이벤트
        }
        else
        {
            if(!inventory) RemoveGearItem(id); //인벤토리 여부에 따라 각각 제거 메서드
            else inventory.RemoveItem(id, false);
            
            OnRemoveItemFromPlayer?.Invoke(id); //아이템 제거(플레이어) ItemDrag UI 이벤트
        }
        
        //플레이어 효과 적용
        if (item.ItemData is IConsumableItem consumableItem) //소비 아이템
        {
            OnUseItem?.Invoke(consumableItem); //아이템 효과 이벤트
        }
        //추가 효과.상태이상회복(출혈 등), 상태이상추가(취기 등)) 
    }

    private float GetTotalArmorAmount() //장비 방어도 총합
     {
        float totalArmor = 0;
        if(HeadwearItem is { ItemData: GearData headwearData }) totalArmor += headwearData.ArmorAmount;
        if(EyewearItem is {ItemData: GearData eyewearData}) totalArmor += eyewearData.ArmorAmount;
        if(BodyArmorItem is { ItemData: GearData bodyArmorData }) totalArmor += bodyArmorData.ArmorAmount;
        if(ChestRigItem is { GearType: GearType.ArmoredRig, ItemData: GearData chestRigData } )
            totalArmor += chestRigData.ArmorAmount;
        //각 장비별 방어도 총합 구하기
        return totalArmor;
    }
    
    public void AddFieldItemToInventory(int firstIdx, RectTransform slotRT, Inventory inventory  ,ItemInstance item)
    {
        //필드 아이템 줍기(인벤토리)
        GearType inventoryType = GearType.None;
        if(inventory == BackpackInventory)  inventoryType = GearType.Backpack; //장비 인벤토리가 있는지 체크
        else if(inventory == RigInventory) inventoryType = GearType.ArmoredRig;
        else Debug.LogWarning("Add Field Item To Inventory Error : Inventory is null.");
        
        var (pos, itemRT) = inventory.AddItem(item, firstIdx, slotRT); //인벤토리에 추가
        
        OnAddFieldItemToInventory?.Invoke(inventoryType, pos, itemRT, item); //ItemDrag 추가 이벤트
    }

    private static AmmoData CheckItemAmmoCaliber(ItemInstance item, AmmoCaliber caliber)
    {
        if (item.ItemData is not AmmoData ammoData) return null;
        return ammoData.AmmoCaliber == caliber ? ammoData : null;
    }
    
    public bool CheckHaveMatchAmmo(AmmoCaliber ammoCaliber) //해당 구경의 AmmoData 아이템이 있는지 체크
    {
        if (RigInventory)
        {
            foreach (var (_, (cells, _, _)) in RigInventory.SlotDict)
            {
                foreach (var cell in cells)
                {
                    if(cell.IsEmpty) continue; //비었다면 건너뛰기
                    var (item, _, _) = RigInventory.ItemDict[cell.InstanceID]; //해당 Cell의 아이템
                    if(CheckItemAmmoCaliber(item, ammoCaliber)) return true;
                }
            }
        }
        foreach (var pocket in PocketSlots)
        {
            if(pocket.IsEmpty) continue; //비었다면 스킵. 상단 코드와 같은 방식 -> 개선?
            var (item, _) = ItemDict[pocket.InstanceID];
            if(CheckItemAmmoCaliber(item, ammoCaliber)) return true;
        }
        return false;
    }
    
    public (bool canReload, int reloadAmmo, AmmoData ammoData) 
        LoadAmmo(AmmoCaliber ammoCaliber, int neededAmmo) //장전 (탄 소모)  /탄종구분 -> 탄 구분 변경 필요 (ex M855, M855A1...)
    {
        int reloadAmmo = 0; //장전할 탄 수량(0~neededAmmo)
        AmmoData ammoData = null; //해당 탄 구경의 Data -> Data단위로 구분으로 수정예정. (Hold R로 탄 변경(같은 구경이라면))

        if (RigInventory)
        {
            foreach (var (_, (cells, _, _)) in RigInventory.SlotDict) //리그 인벤 슬롯 Cell검사
            {
                foreach (var cell in cells)
                {
                    if(cell.IsEmpty) continue; //비었다면 건너뛰기
                    var (item, _, _) = RigInventory.ItemDict[cell.InstanceID]; //해당 Cell의 아이템
                    ammoData = CheckItemAmmoCaliber(item, ammoCaliber);
                    if(!ammoData) continue;
                    if (UseAmmo(item)) return (true, reloadAmmo, ammoData); //탄 사용(요구 탄 수량보다 많은 경우)
                    RigInventory.RemoveItem(item.InstanceID, false);//ItemDict 및 Cell에서 아이템 제거
                }
            }
        }

        foreach (var pocket in PocketSlots)
        {
            if(pocket.IsEmpty) continue; //비었다면 스킵. 상단 코드와 같은 방식 -> 개선?
            var (item, _) = ItemDict[pocket.InstanceID];
            ammoData = CheckItemAmmoCaliber(item, ammoCaliber);
            if(!ammoData) continue;
            if (UseAmmo(item)) return (true, reloadAmmo, ammoData); //탄 사용(요구 탄 수량보다 많은 경우)
            RemoveGearItem(item.InstanceID);//ItemDict 및 Cell에서 아이템 제거
        }
        
        if (reloadAmmo <= 0) //장전할 탄이 없다면
        {
            Debug.Log($"No Ammo : {ammoCaliber}");
            return (false, 0, null);
        }
        
        return (true, reloadAmmo, ammoData); //누적된 장전탄약 

        bool UseAmmo(ItemInstance item) //지역 함수(인벤토리에서 탄 소모)
        {
            var stackAmount = item.CurrentStackAmount; //아이템의 스택
            if (stackAmount > neededAmmo) //필요치보다 스택이 많을 경우
            {
                reloadAmmo += neededAmmo; //장전할 탄 수량에 추가
                item.AdjustStackAmount(-neededAmmo); //스택에서 요구치만큼 차감
                OnUpdateItemStack?.Invoke(item.InstanceID, item.CurrentStackAmount); //스택 표시 갱신
                return true;
            }
            //필요치가 더 많다면 -> item 삭제
            neededAmmo -= stackAmount; //요구치 스택만큼 감소
            reloadAmmo += stackAmount; //장전할 탄약에 스택만큼 추가
            OnRemoveItemFromPlayer?.Invoke(item.InstanceID); //아이템 제거(ItemDragHandler UI)
            return false;
        }
    }

    public void UpdateWeaponMagCount(Guid id) //무기 장탄량 ItemDrag UI 업데이트
    {
        var (item, _) = ItemDict[id];
        if (item is WeaponInstance weapon) //무기인지 체크
        {
            OnUpdateWeaponItemMagCount?.Invoke(id, weapon.IsFullyLoaded() ,weapon.CurrentMagazineCount); //이벤트 호출
        }
    }
    
    private void SetGearItemData(CellData cell, ItemInstance item) //아이템 데이터 할당
    {
        if (cell == HeadwearSlot)
        {
            HeadwearItem = item; 
            return;
        }
        if (cell == EyewearSlot)
        {
            EyewearItem = item;
            return ;
        }

        if (cell == BodyArmorSlot)
        {
            BodyArmorItem = item;
            return;
        }

        if (cell == PrimaryWeaponSlot)
        {
            PrimaryWeaponItem = item;
            return;
        }

        if (cell == SecondaryWeaponSlot)
        {
            SecondaryWeaponItem = item;
            return;
        }

        if (cell == ChestRigSlot)
        {
            ChestRigItem = item;
            return;
        }

        if (cell == BackpackSlot)
        {
            BackpackItem = item;
            return;
        }
    }
}
