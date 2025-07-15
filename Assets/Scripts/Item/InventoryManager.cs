using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    //Left Panel
    public ItemInstance HeadwearItem { private set; get; }
    public CellData HeadwearSlot { get; } = new(GearType.HeadWear, Vector2.zero);
    public ItemInstance EyewearItem { private set; get; }
    public CellData EyewearSlot { get; } = new(GearType.EyeWear, Vector2.zero);
    public ItemInstance BodyArmorItem { private set; get; }
    public CellData BodyArmorSlot { get; } = new(GearType.BodyArmor, Vector2.zero);
    public ItemInstance PrimaryWeaponItem { private set; get; }
    public CellData PrimaryWeaponSlot { get; } = new(GearType.Weapon, Vector2.zero);
    public ItemInstance SecondaryWeaponItem { private set; get; }
    public CellData SecondaryWeaponSlot { get; } = new(GearType.Weapon, Vector2.zero);
    public Dictionary<Guid, (ItemInstance item, CellData cell)> ItemDict { get; } = new();
    
    //Middle Panel
    public ItemInstance ChestRigItem { private set; get; }
    public CellData ChestRigSlot { get; } = new(GearType.ArmoredRig, Vector2.zero);
    public CellData[] PocketSlots { get; private set; } = new CellData[4];
    public ItemInstance BackpackItem { private set; get; }
    public CellData BackpackSlot { get; } = new(GearType.Backpack, Vector2.zero);
    public Inventory BackpackInventory { get; private set; }
    public Inventory RigInventory { get; private set; }

    //Right Panel
    public Inventory LootInventory { get; private set; }
    
    //QuickSlot
    public Dictionary<QuickSlotIdx, (Guid ID, Inventory inventory)> QuickSlotDict { get; } = new();
   

    //ItemUI Presenter
    public event Action<GameObject, ItemInstance> OnInitInventory;  //인벤토리 오브젝트, 인벤토리 타입(구분) 
    public event Action<ItemInstance> OnShowInventory;
    public event Action<CellData, ItemInstance> OnEquipFieldItem;
    public event Action<GearType, Vector2, RectTransform ,ItemInstance> OnAddFieldItemToInventory;
    public event Action<Guid, int> OnUpdateItemStack;
    public event Action<Guid, bool, int> OnUpdateWeaponItemMagCount;
    public event Action<Guid> OnRemoveItem;
    public event Action<Guid, QuickSlotIdx> OnRemoveQuickSlotItem;
    
    //PlayerManager
    public event Action<float> OnUpdateArmorAmount;
    public event Action<EquipWeaponIdx> OnUnequipWeapon;
    
    
    private void Awake()
    {
        for (int i = 0; i < 4; i++) //PocketSlot 4
        {
            PocketSlots[i] = new CellData(GearType.None, Vector2.zero);
        }
        
        QuickSlotDict.Add(QuickSlotIdx.QuickSlot4, (Guid.Empty, null));
        QuickSlotDict.Add(QuickSlotIdx.QuickSlot5, (Guid.Empty, null));
        QuickSlotDict.Add(QuickSlotIdx.QuickSlot6, (Guid.Empty, null));
        QuickSlotDict.Add(QuickSlotIdx.QuickSlot7, (Guid.Empty, null));
    }
    
    public void SetInventoryData(Inventory inventory, GearType gearType) 
    {
        //인벤토리 설정
        switch (gearType)
        {
            case GearType.ArmoredRig: 
            case GearType.UnarmoredRig:
                RigInventory = inventory;
                inventory.OnItemRemoved += CheckRemoveItemIsQuickSlot;
                break;
            case GearType.Backpack:
                BackpackInventory = inventory;
                break;
            case GearType.None:
                LootInventory = inventory;
                break;
        }
    }

    public CellData CheckCanEquipItem(GearType gearType) //GearSlot 장착 가능 여부 확인
    {
        switch (gearType)
        {
            case GearType.ArmoredRig:
                if(!BodyArmorSlot.IsEmpty || !ChestRigSlot.IsEmpty) return null;
                return ChestRigSlot; 
            case GearType.UnarmoredRig:
                return ChestRigSlot.IsEmpty ? ChestRigSlot : null;
            case GearType.BodyArmor:
                if (ChestRigSlot.IsEmpty) return BodyArmorSlot.IsEmpty ? BodyArmorSlot : null;
               
                var rigID = ChestRigSlot.InstanceID;
                var rigItemType = ItemDict[rigID].item.GearType;
                if (rigItemType is GearType.ArmoredRig) return null;
                return BodyArmorSlot.IsEmpty ? BodyArmorSlot : null;
            case GearType.Backpack:
                return BackpackSlot.IsEmpty ? BackpackSlot : null;
            case GearType.HeadWear:
                return HeadwearSlot.IsEmpty ? HeadwearSlot : null;
            case GearType.EyeWear:
                return EyewearSlot.IsEmpty ? EyewearSlot : null;
            case GearType.Weapon:
                if (PrimaryWeaponSlot.IsEmpty)
                {
                    return PrimaryWeaponSlot;
                }
                if (SecondaryWeaponSlot.IsEmpty)
                {
                    return SecondaryWeaponSlot;
                }
                return null;
            case GearType.None:
                //if()
                for (int i = 0; i < 4; i++)
                {
                    if (PocketSlots[i].IsEmpty) return PocketSlots[i];
                }
                return null;
        }
        return null;
    }

    public void SetGearItem(CellData gearSlot, ItemInstance item)
    {
        ItemDict[item.InstanceID] = (item, gearSlot);
        Debug.Log($"Setting item : {item.ItemData.ItemName}, {item.InstanceID}, Count : {ItemDict.Count}");
        gearSlot.SetEmpty(false, item.InstanceID);
        
        SetGearItemData(gearSlot, item);

        switch (item.GearType)
        {
            case GearType.ArmoredRig:
            case GearType.UnarmoredRig:
            case GearType.Backpack:
                if (item.ItemInventory) //이미 인벤토리를 생성했다면
                {
                    OnShowInventory?.Invoke(item);
                    return;
                }
                var gearData = item.ItemData as GearData;
                if(gearData) OnInitInventory?.Invoke(gearData.SlotPrefab, item); 
                break;
        }
        //장비 능력치, 무기 설정
        OnUpdateArmorAmount?.Invoke(GetTotalArmorAmount());
    }

    public void RemoveGearItem(CellData gearSlot, Guid itemID)
    {
        var gearType = ItemDict[itemID].item.GearType;
        
        switch (gearType)//슬롯 종류
        {
            case GearType.ArmoredRig or GearType.UnarmoredRig or GearType.Backpack:
            {
                //씬이동 시?... 데이터 저장(하위 아이템별?)
                var inventory = ItemDict[itemID].item.ItemInventory;
                inventory.gameObject.SetActive(false); //비활성
                if (gearType is GearType.ArmoredRig or GearType.UnarmoredRig)
                {
                    inventory.OnItemRemoved -= CheckRemoveItemIsQuickSlot;
                }
                break;
            }
            case GearType.Weapon:
            {
                //비우기 무장해제
                var cell = ItemDict[itemID].cell;
                if (cell == PrimaryWeaponSlot)
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
                CheckRemoveItemIsQuickSlot(itemID);
                break;
            }
        }

        ItemDict.Remove(itemID);
        gearSlot.SetEmpty(true, Guid.Empty);
        SetGearItemData(gearSlot, null);
    }

    private void CheckRemoveItemIsQuickSlot(Guid id)
    {
        for (var idx = QuickSlotIdx.QuickSlot4; idx <= QuickSlotIdx.QuickSlot7; idx++)
        {
            var (itemID, inventory) = QuickSlotDict[idx];
            if(!itemID.Equals(id)) continue;
            QuickSlotDict[idx] = (Guid.Empty, null); //불가
            OnRemoveQuickSlotItem?.Invoke(id, idx);
            //return;
        }
    }
    
    public void EquipFieldItem(CellData gearSlot, ItemInstance item)
    {
        SetGearItem(gearSlot, item);
        //event...
        OnEquipFieldItem?.Invoke(gearSlot, item);
    }

    public float GetTotalArmorAmount()
    {
        float totalArmor = 0;
        if(HeadwearItem is { ItemData: GearData headwearData }) totalArmor += headwearData.ArmorAmount;
        if(EyewearItem is {ItemData: GearData eyewearData}) totalArmor += eyewearData.ArmorAmount;
        if(BodyArmorItem is { ItemData: GearData bodyArmorData }) totalArmor += bodyArmorData.ArmorAmount;
        if(ChestRigItem is { GearType: GearType.ArmoredRig, ItemData: GearData chestRigData } )
            totalArmor += chestRigData.ArmorAmount;
        return totalArmor;
    }
    
    public void AddFieldItemToInventory(int firstIdx, RectTransform slotRT, Inventory inventory  ,ItemInstance item)
    {
        GearType inventoryType = GearType.None;
        if(inventory == BackpackInventory)  inventoryType = GearType.Backpack;
        else if(inventory == RigInventory) inventoryType = GearType.ArmoredRig;
        else Debug.LogWarning("Add Field Item To Inventory Error : Inventory is null.");
        
        var (pos, itemRT) = inventory.AddItem(item, firstIdx, slotRT);
        OnAddFieldItemToInventory?.Invoke(inventoryType, pos, itemRT, item);
    }
    
    //QuickSlot...

    public (bool canReload, int reloadAmmo) 
        LoadAmmo(AmmoCaliber ammoCaliber, int neededAmmo, Guid weaponID) //탄종구분 - 탄 구분(zero sivert처럼)?
    {
        int reloadAmmo = 0;
        if (RigInventory)
        {
            foreach (var (_, (cells, _, _)) in RigInventory.SlotDict)
            {
                foreach (var cell in cells)
                {
                    if(cell.IsEmpty) continue;
                    var (item, _, _) = RigInventory.ItemDict[cell.InstanceID];
                    if(item.ItemData is not AmmoData ammoData) continue;
                    if(ammoData.AmmoCaliber != ammoCaliber) continue;
                    Debug.Log($"Rig, Use Ammo : {ammoData.AmmoCaliber}");
                    var stackAmount = item.CurrentStackAmount;
                    if (stackAmount > neededAmmo) //스택이 더 많을때(장전에 필요한 것보다 많음)
                    {
                        Debug.Log($"Rig, over stack : {ammoData.ItemName}, {neededAmmo}, {stackAmount}");
                        reloadAmmo += neededAmmo;
                        item.AdjustStackAmount(-neededAmmo); //스택에서 요구치만큼 차감
                        OnUpdateItemStack?.Invoke(item.InstanceID, item.CurrentStackAmount);
                        return (true, reloadAmmo); //요구치 전부
                    }

                    //item 삭제...
                    Debug.Log($"Rig, less stack : {ammoData.ItemName}, {neededAmmo}, {stackAmount}");
                    neededAmmo -= stackAmount; //요구치 스택만큼 감소
                    reloadAmmo += stackAmount; //장전할 탄약에 스택만큼 추가
                    OnRemoveItem?.Invoke(item.InstanceID); //아이템 제거
                    RigInventory.ItemDict.Remove(item.InstanceID); //ItemDict제거, CellData 초기화...
                    cell.SetEmpty(true, Guid.Empty);
                }
            }
        }

        foreach (var pocket in PocketSlots)
        {
            if(pocket.IsEmpty) continue;
            var (item, _) = ItemDict[pocket.InstanceID];
            if(item.ItemData is not AmmoData ammoData) continue;
            if(ammoData.AmmoCaliber != ammoCaliber) continue;
            Debug.Log($"Pocket, Use Ammo : {ammoData.AmmoCaliber}");
            var stackAmount = item.CurrentStackAmount;
            if (stackAmount > neededAmmo)
            {
                reloadAmmo += neededAmmo;
                item.AdjustStackAmount(-neededAmmo); //스택에서 요구치만큼 차감
                OnUpdateItemStack?.Invoke(item.InstanceID, item.CurrentStackAmount);
                return (true, reloadAmmo); //요구치 전부
            }
            //item 삭제...
            neededAmmo -= stackAmount; //요구치 스택만큼 감소
            reloadAmmo += stackAmount; //장전할 탄약에 스택만큼 추가
            OnRemoveItem?.Invoke(item.InstanceID); //아이템 제거
            ItemDict.Remove(item.InstanceID);
            pocket.SetEmpty(true, Guid.Empty);
        }
        
        if (reloadAmmo <= 0)
        {
            Debug.Log($"No Ammo : {ammoCaliber}");
            return (false, 0);
        }
        Debug.Log($"Reload Ammo : {ammoCaliber}, {reloadAmmo}");
        return (true, reloadAmmo);
    }

    public void UpdateWeaponMagCount(Guid id)
    {
        var (item, _) = ItemDict[id];
        if (item is WeaponInstance weapon)
        {
            OnUpdateWeaponItemMagCount?.Invoke(id, weapon.IsFullyLoaded() ,weapon.CurrentMagazineCount);
        }
    }
    
    private void SetGearItemData(CellData cell, ItemInstance item) //아이템 데이터 참조
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
