using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerManager : MonoBehaviour, IDamageable
{
    [SerializeField] private float playerHealth = 100f; //추후 SO에서 받아오게 수정 예정
    [SerializeField] private float playerStamina = 100f;
    [SerializeField] private float playerHydration = 100f;
    [SerializeField] private float playerEnergy = 100f;
    
    [SerializeField] private SpriteRenderer oneHandSprite;
    [SerializeField] private Transform oneHandMuzzleTransform;
    [SerializeField] private SpriteRenderer twoHandSprite;
    [SerializeField] private Transform twoHandMuzzleTransform;
    [SerializeField] private GameObject muzzleFlashVFX;
    
    private Camera _mainCamera;
    private ItemUIManager _itemUIManager;
    private PlayerWeapon _playerWeapon;
    private PlayerAnimation _playerAnimation;
    private InventoryManager _inventoryManager;
    private InventoryUIPresenter _inventoryUIPresenter;
    
    [ShowInInspector] private float _currentHealth;
    [ShowInInspector] private float _currentTotalArmor;
    [ShowInInspector] private float _currentStamina;
    [ShowInInspector] private float _currentHydration;
    [ShowInInspector] private float _currentEnergy;
    [ShowInInspector] private float _currentTotalWeight;
    
    private WeaponInstance _currentWeaponItem;
    private EquipWeaponIdx _equipWeaponIdx = EquipWeaponIdx.Unarmed; //초기 Unarmed
    public event Action<float, float> OnPlayerHealthChanged;
    public event Action<bool, int> OnUpdateMagazineCountUI;
    public event Action OnReloadNoAmmo;

   
    public bool IsUnarmed { private set; get; }
    
    public bool CanItemInteract { get; private set; }
    private int _currentItemInteractIdx;
    private readonly List<(bool available, ItemInteractType type)> _currentItemInteractList = new();
    private ItemPickUp _currentItemPickUp;
    private CellData _pickupTargetCell;
    private (int firstIdx, RectTransform slotRT, Inventory inventory) _pickupTargetInvenSlotInfo;
    private bool _pickupTargetIsPocket;

    //UI Manager 개선?
    private void Awake()
    {
        _itemUIManager = FindFirstObjectByType<ItemUIManager>();
        TryGetComponent(out _playerAnimation);
        TryGetComponent(out _playerWeapon);
        _playerWeapon.OnShowMuzzleFlash += HandleOnShowMuzzleFlash;
        
        _mainCamera = Camera.main;
        _inventoryManager = FindFirstObjectByType<InventoryManager>(); //개선점???
        _inventoryManager.OnUpdateArmorAmount += HandleOnUpdateArmorAmount;
        _inventoryManager.OnUnequipWeapon += HandleOnUnequipWeapon;
        
        ChangeCurrentWeapon(null); //비무장 초기화
         
        _currentHealth = playerHealth; //체력 초기화
        OnPlayerHealthChanged?.Invoke(_currentHealth, playerHealth);
    }

    private void OnDisable()
    {
        _playerWeapon.OnShowMuzzleFlash -= HandleOnShowMuzzleFlash;
        _inventoryManager.OnUpdateArmorAmount -= HandleOnUpdateArmorAmount;
        _inventoryManager.OnUnequipWeapon -= HandleOnUnequipWeapon;
    }

    private void HandleOnShowMuzzleFlash()
    {
        muzzleFlashVFX.SetActive(true);
    }

    private void HandleOnUpdateArmorAmount(float amount)
    {
        _currentTotalArmor = amount;
    }

    private void HandleOnUnequipWeapon(EquipWeaponIdx weaponIdx)
    {
        if (_equipWeaponIdx == weaponIdx)
        {
            ChangeCurrentWeapon(null);
        }
    }

    public bool CheckIsAutomatic()
    {
        if (_currentWeaponItem is { ItemData: WeaponData weaponData })
            return weaponData.CanFullAuto;
        return false;
    }

    public bool CheckIsOneHanded()
    {
        if (_currentWeaponItem is { ItemData: WeaponData weaponData })
            return weaponData.IsOneHanded;
        return false;
    }
    
    public void Shoot(bool isFlipped, float shootAngle) //사격
    {
        //총알 데이터..?
        if(GetCurrentWeaponMagazineCount() <= 0) return;
        bool isShoot = _playerWeapon.Shoot(isFlipped, shootAngle);
        
        if (!isShoot) return;
        _currentWeaponItem.UseAmmo();
        OnUpdateMagazineCountUI?.Invoke(_currentWeaponItem.IsFullyLoaded(), GetCurrentWeaponMagazineCount());
        _inventoryManager.UpdateWeaponMagCount(_currentWeaponItem.InstanceID);
    }

    private int GetCurrentWeaponMagazineCount()
    {
        if (_currentWeaponItem != null)
        {
            return _currentWeaponItem.CurrentMagazineCount;
        }
        return -1;
    }
    
    public void Reload() //여기서?
    {
		//Inven -> weapon
        if (_currentWeaponItem != null)
        {
            var weaponData = _currentWeaponItem.WeaponData;
            var magazineSize = weaponData.DefaultMagazineSize;
            var currentAmmo = _currentWeaponItem.CurrentMagazineCount;
            int ammoToRefill;

            if (weaponData.IsOpenBolt)
            {
                ammoToRefill = magazineSize - currentAmmo;
            }
            else
            {
                if(currentAmmo == 0) ammoToRefill = magazineSize;
                else ammoToRefill = magazineSize + 1 - currentAmmo;//약실 한 발 고려
            }
            
            var (canReload, reloadAmmo)  = 
                _inventoryManager.LoadAmmo(weaponData.AmmoCaliber, ammoToRefill, _currentWeaponItem.InstanceID);

            if (canReload)
            {
                _currentWeaponItem.ReloadAmmo();
                OnUpdateMagazineCountUI?.Invoke(_currentWeaponItem.IsFullyLoaded(), currentAmmo + reloadAmmo);
                _inventoryManager.UpdateWeaponMagCount(_currentWeaponItem.InstanceID);
            }
            
            //장전할 탄이 하나도 없으면 경고문구 띄우기
            //장전 제어?
        }
    }

    public void HandleOnChangeWeapon(EquipWeaponIdx weaponIdx)
    {
        switch (weaponIdx)
        {
            case EquipWeaponIdx.Primary:
                _currentWeaponItem = _inventoryManager.PrimaryWeaponItem as WeaponInstance;
                ChangeCurrentWeapon(_currentWeaponItem);
                if (_currentWeaponItem == null)
                {
                    weaponIdx = EquipWeaponIdx.Unarmed;
                }
                break;
            case EquipWeaponIdx.Secondary:
                _currentWeaponItem = _inventoryManager.SecondaryWeaponItem as WeaponInstance;
                ChangeCurrentWeapon(_currentWeaponItem);
                if (_currentWeaponItem == null)
                {
                    weaponIdx = EquipWeaponIdx.Unarmed;
                }
                break;
            case EquipWeaponIdx.Unarmed: //비무장
                _currentWeaponItem = null;
                ChangeCurrentWeapon(null);
                break;
        }
        _equipWeaponIdx = weaponIdx;
    }

    private void ChangeCurrentWeapon(WeaponInstance weaponItem) //무기 교체
    {
        if (weaponItem == null)
        {
            IsUnarmed = true;
            _playerAnimation.ChangeWeapon(WeaponType.Unarmed);
            oneHandSprite.enabled = false;
            twoHandSprite.enabled = false;
            OnUpdateMagazineCountUI?.Invoke(false, 0);
            return;
        }

        var newWeaponData = weaponItem.WeaponData;
        
        WeaponType weaponType = newWeaponData.WeaponType; //무기 타입
        IsUnarmed = false;
        if (weaponType == WeaponType.Pistol) //한손무기
        {
            oneHandSprite.sprite = newWeaponData.ItemSprite; //스프라이트 위치
            oneHandSprite.enabled = true;
            twoHandSprite.enabled = false;
            oneHandMuzzleTransform.localPosition = newWeaponData.MuzzlePosition;
            _playerWeapon.SetMuzzleTransform(oneHandMuzzleTransform); //총구위치 설정
            
            muzzleFlashVFX.transform.SetParent(oneHandMuzzleTransform);
            muzzleFlashVFX.transform.localRotation = Quaternion.identity;
            muzzleFlashVFX.transform.localPosition = newWeaponData.MuzzleFlashOffset;
        }
        else //양손무기
        {
            twoHandSprite.sprite = newWeaponData.ItemSprite;
            twoHandSprite.enabled = true;
            oneHandSprite.enabled = false;
            twoHandMuzzleTransform.localPosition = newWeaponData.MuzzlePosition;
            _playerWeapon.SetMuzzleTransform(twoHandMuzzleTransform);
            
            muzzleFlashVFX.transform.SetParent(twoHandMuzzleTransform);
            muzzleFlashVFX.transform.localRotation = Quaternion.identity;
            muzzleFlashVFX.transform.localPosition = newWeaponData.MuzzleFlashOffset;
        }
        
        _playerWeapon.ChangeWeaponData(newWeaponData); //변경
        _playerAnimation.ChangeWeapon(weaponType); //애니메이션 변경
        OnUpdateMagazineCountUI?.Invoke(_currentWeaponItem.IsFullyLoaded(), GetCurrentWeaponMagazineCount());
    }

    public void HandleOnUseQuickSlot(QuickSlotIdx slotIdx)
    {
        if(!_inventoryManager.QuickSlotDict.TryGetValue(slotIdx, out var quickSlotInfo)) return;
        var (id, inventory) = quickSlotInfo;
        var (item, _, _) = inventory.ItemDict[id];
        Debug.Log($"Quick Slot {slotIdx} Use : {item.ItemData.ItemName}");
    }
    
    public void ScrollItemPickup(float scrollDeltaY)
    {
        _currentItemInteractIdx += (int)scrollDeltaY;
        if (_currentItemInteractIdx < 0) _currentItemInteractIdx = _currentItemInteractList.Count - 1;
        else if (_currentItemInteractIdx >= _currentItemInteractList.Count) _currentItemInteractIdx = 0;
        //범위 넘기면 처리
      
        _itemUIManager.ScrollItemPickup(_currentItemInteractIdx);//ItemPickup UI 스크롤(획득/장착 등...)
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Item"))
        {
            //pick up ui
            CanItemInteract = true;
            Vector2 pos = _mainCamera.WorldToScreenPoint(other.transform.position);
            
            other.TryGetComponent<ItemPickUp>(out var itemPickUp);
            _currentItemPickUp = itemPickUp;  //장착-획득 여부... -> InventoryManager참조...
            var item = itemPickUp.GetItemInstance();
            var itemData = item.ItemData;
            bool canEquip = false;
            //장착
            bool isGear = item.GearType != GearType.None; //Gear인지 체크
            if (item.GearType is not GearType.None)
            {
                var checkCell = _inventoryManager.CheckCanEquipItem(item.GearType);
                if (checkCell is not null)
                {
                    canEquip = true;
                    _pickupTargetCell = checkCell;
                }
            }
            //inventory
            bool canPickup = false;

            Inventory inventory = null;

            if (item.ItemData is AmmoData) //Ammo면 리그부터 검사
            {
                if(_inventoryManager.RigInventory) inventory = _inventoryManager.RigInventory;
                else if(_inventoryManager.BackpackInventory) inventory = _inventoryManager.BackpackInventory;
            }
            else //아니라면 가방부터
            {
                if(_inventoryManager.BackpackInventory) inventory = _inventoryManager.BackpackInventory;
                else if(_inventoryManager.RigInventory) inventory = _inventoryManager.RigInventory;
            }

            if (inventory) //가방이나 리그를 장착한 상태라면 (해당 inventory가 있음)
            {
                var (isAvailable, firstIdx, slotRT) = inventory.CheckCanAddItem(itemData); //빈 공간 검사
                canPickup = isAvailable;
                _pickupTargetInvenSlotInfo = (firstIdx, slotRT, inventory);
                _pickupTargetIsPocket = false;
            }
            else //리그, 가방에 공간이 없을 때
            {
                var checkCell = _inventoryManager.CheckCanEquipItem(item.GearType);
                if(checkCell is not null 
                   && itemData.ItemHeight == 1 && itemData.ItemWidth == 1) 
                {
                    canPickup = true; //Cell크기 고려
                    _pickupTargetCell = checkCell;
                    _pickupTargetIsPocket = true;
                }
            }
            
            _currentItemInteractList.Clear();
            _currentItemInteractIdx = 0;
            if(isGear) _currentItemInteractList.Add((canEquip, ItemInteractType.Equip)); //Pickup UI 리스트
            _currentItemInteractList.Add((canPickup, ItemInteractType.PickUp));
            
            _itemUIManager.ShowItemPickup(pos, _currentItemInteractList); //이벤트로 수정 예정
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Item"))
        {
            CanItemInteract = false;
            _itemUIManager.HideItemPickup();
        }
    }

    public void GetFieldItem()
    {
        var item = _currentItemPickUp.GetItemInstance();
        
        //장비 - 장착/획득 순서
        //장비x - 획득만
        var (isAvailable, type) = _currentItemInteractList[_currentItemInteractIdx];
        
        if (!isAvailable) return;
      
        switch (type)
        {
            case ItemInteractType.Equip:
                //Event
                _inventoryManager.EquipFieldItem(_pickupTargetCell, item); //사이즈 준내큼
                if (_pickupTargetCell == _inventoryManager.PrimaryWeaponSlot)
                {
                    HandleOnChangeWeapon(EquipWeaponIdx.Primary);
                }
                else if (_pickupTargetCell == _inventoryManager.SecondaryWeaponSlot)
                {
                    HandleOnChangeWeapon(EquipWeaponIdx.Secondary);
                }
                break;
            case ItemInteractType.PickUp:
                if (!_pickupTargetIsPocket)
                {
                    var (firstIdx, slotRT, inventory) = _pickupTargetInvenSlotInfo;
                    _inventoryManager.AddFieldItemToInventory(firstIdx, slotRT, inventory ,item);
                }
                else _inventoryManager.EquipFieldItem(_pickupTargetCell, item);
                break;
            default:
                Debug.LogWarning("ItemInteractType Error: None.");
                break;
        }
        
        CanItemInteract = false;
        _itemUIManager.HideItemPickup();
        
        ObjectPoolingManager.Instance.ReleaseItemPickUp(_currentItemPickUp);
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        OnPlayerHealthChanged?.Invoke(_currentHealth, playerHealth);
        if (_currentHealth <= 0)
        {
            Debug.Log("Player Dead: Health");
        }
    }
}
