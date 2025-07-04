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
    public InventoryItem HeadwearItem { private set; get; }
    public CellData HeadwearSlot { get; } = new(GearType.HeadWear, Vector2.zero);
    public InventoryItem EyewearItem { private set; get; }
    public CellData EyewearSlot { get; } = new(GearType.EyeWear, Vector2.zero);
    public InventoryItem BodyArmorItem { private set; get; }
    public CellData BodyArmorSlot { get; } = new(GearType.BodyArmor, Vector2.zero);
    public InventoryItem PrimaryWeaponItem { private set; get; }
    public CellData PrimaryWeaponSlot { get; } = new(GearType.Weapon, Vector2.zero);
    public InventoryItem SecondaryWeaponItem { private set; get; }
    public CellData SecondaryWeaponSlot { get; } = new(GearType.Weapon, Vector2.zero);

    //Middle Panel
    public InventoryItem ChestRigItem { private set; get; }
    public CellData ChestRigSlot { get; } = new(GearType.ArmoredRig, Vector2.zero);
    public CellData[] PocketSlots { get; private set; } = new CellData[4];
    public InventoryItem BackpackItem { private set; get; }
    public CellData BackpackSlot { get; } = new(GearType.Backpack, Vector2.zero);
    public Inventory BackpackInventory { get; private set; }
    public Inventory RigInventory { get; private set; }

    //Right Panel
    public Inventory LootInventory { get; private set; }
    
    public Dictionary<Guid, (InventoryItem item, CellData cell)> ItemDict { get; } = new();

    public event Action<GameObject, InventoryItem> OnInitInventory;  //인벤토리 오브젝트, 인벤토리 타입(구분) 
    public event Action<InventoryItem> OnShowInventory;
    public event Action<CellData, InventoryItem> OnEquipFieldItem;
    public event Action<GearType, Vector2, RectTransform ,InventoryItem> OnAddItemToInventory;
    public event Action<float> OnUpdateArmorAmount;
    
    private void Awake()
    {
        for (int i = 0; i < 4; i++) //PocketSlot 4
        {
            PocketSlots[i] = new CellData(GearType.None, Vector2.zero);
        }
    }
    
    public void SetInventoryData(Inventory inventory, GearType gearType) 
    {
        //인벤토리 설정
        switch (gearType)
        {
            case GearType.ArmoredRig: 
            case GearType.UnarmoredRig:
                RigInventory = inventory;
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

    private void SetGearItemData(CellData cell, InventoryItem item)
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

    public void SetGearItem(CellData gearSlot, InventoryItem item)
    {
        ItemDict[item.InstanceID] = (item, gearSlot);
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
                //SetInventorySlot(gearData.SlotPrefab, item);
                //인벤토리 초기화 여부... 어떻게?
                break;
        }
        //장비 능력치, 무기 설정
        //무기 관련 구현(교체)
        //현재 무기 장착해제 -> PlayerManager 비무장으로
        //
        OnUpdateArmorAmount?.Invoke(GetTotalArmorAmount());
    }

    public void RemoveGearItem(CellData gearSlot, Guid itemID)
    {
        var gearType = ItemDict[itemID].item.GearType;
        //Debug.Log($"Removing item : {ItemDict[itemID].item.ItemData.ItemName}, {itemID}");
        if (gearType is GearType.ArmoredRig or GearType.UnarmoredRig or GearType.Backpack)
        {
            var inventory = ItemDict[itemID].item.ItemInventory;
            inventory.gameObject.SetActive(false); //비활성
        }
        
        ItemDict.Remove(itemID);
        gearSlot.SetEmpty(true, Guid.Empty);
        
        //Inventory있는 경우...처리
        //리그, 가방 -> 무조건 있음... 단순 비활성?
        
        //장비 능력치 등 처리
    }

    public void EquipGearItem(CellData gearSlot, InventoryItem item)
    {
        SetGearItem(gearSlot, item);
        //event...
        OnEquipFieldItem?.Invoke(gearSlot, item);
    }

    public float GetTotalArmorAmount()
    {
        float totalArmor = 0;
        if(HeadwearItem.ItemData is GearData headwearData) totalArmor += headwearData.ArmorAmount;
        if(EyewearItem.ItemData is GearData eyewearData) totalArmor += eyewearData.ArmorAmount;
        if(BodyArmorItem.ItemData is GearData bodyArmorData) totalArmor += bodyArmorData.ArmorAmount;
        if(ChestRigItem.GearType is GearType.ArmoredRig &&
           ChestRigItem.ItemData is GearData chestRigData ) totalArmor += chestRigData.ArmorAmount;
        return totalArmor;
    }
    
    public void AddItemToInventory(int firstIdx, RectTransform slotRT, InventoryItem item)
    {
        //item따라...
        //DragHandler -> ObjectPooling(active, inactive 로 구분, inactive인 경우 리셋...)

        if (BackpackInventory)
        {
            //var item = new InventoryItem(itemData);
            var (pos, itemRT) = BackpackInventory.AddItem(item, firstIdx, slotRT);
            OnAddItemToInventory?.Invoke(GearType.Backpack, pos, itemRT, item);
            return;
        }

        if (RigInventory)
        {
            //var item = new InventoryItem(itemData);
            var (pos, itemRT) = RigInventory.AddItem(item, firstIdx, slotRT);
            OnAddItemToInventory?.Invoke(GearType.ArmoredRig, pos, itemRT,item);
            return;
        }

        OnAddItemToInventory?.Invoke(GearType.None, Vector2.zero, null, null);
    }
}
