using System;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerManager : MonoBehaviour, IDamageable
{
    [SerializeField] private float playerHealth = 100f; //추후 SO에서 받아오게 수정 예정
    [SerializeField] private WeaponData currentWeaponData;//현재 직렬화(추후 인벤토리에서)
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
    
    private float _currentHealth;
    private float _currentTotalArmor;
    private CurrentWeaponIdx _currentWeaponIdx = CurrentWeaponIdx.Unarmed; //초기 Unarmed
    public event Action<float, float> OnPlayerHealthChanged;

    public bool IsUnarmed { private set; get; }
    
    public bool CanItemInteract { get; private set; }
    private int _currentItemInteractIdx;
    private readonly List<(bool available, ItemInteractType type)> _currentItemInteractList = new();
    private ItemPickUp _currentItemPickUp;
    private CellData _pickupTargetCell;
    private (int firstIdx, RectTransform slotRT) _pickupTargetSlotInfo;
    private bool _pickupTargetIsPocket;

    //UI Manager 개선?
    private void Awake()
    {
        _itemUIManager = FindFirstObjectByType<ItemUIManager>();
        TryGetComponent(out _playerAnimation);
        TryGetComponent(out _playerWeapon);
        _playerWeapon.Init(_itemUIManager);
        _playerWeapon.OnShowMuzzleFlash += HandleOnShowMuzzleFlash;
        
        _mainCamera = Camera.main;
        _inventoryManager = FindFirstObjectByType<InventoryManager>(); //개선점???
        _inventoryManager.OnUpdateArmorAmount += HandleOnUpdateArmorAmount;
        _inventoryManager.OnUnequipWeapon += HandleOnUnequipWeapon;
        
        IsUnarmed = true;
        
        _currentHealth = playerHealth;
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

    private void HandleOnUnequipWeapon(CurrentWeaponIdx weaponIdx)
    {
        if (_currentWeaponIdx == weaponIdx)
        {
            ChangeCurrentWeapon(null);
        }
    }
    
    public bool CheckIsAutomatic()
    {
        return currentWeaponData.CanFullAuto;
    }

    public bool CheckIsOneHanded()
    {
        return currentWeaponData.IsOneHanded;
    }
    
    public void Shoot(bool isFlipped, float shootAngle) //사격
    {
        _playerWeapon.Shoot(isFlipped, shootAngle);
    }

    public void Reload()
    {
        _playerWeapon.Reload();
    }

    public void HandleOnChangeWeapon(CurrentWeaponIdx weaponIdx)
    {
        InventoryItem weaponItem;
        WeaponData weaponData;
        
        switch (weaponIdx)
        {
            case CurrentWeaponIdx.Primary:
                if (_inventoryManager.PrimaryWeaponItem == null)
                {
                    ChangeCurrentWeapon(null);
                    _currentWeaponIdx = CurrentWeaponIdx.Unarmed;
                    return;
                }
                weaponItem = _inventoryManager.PrimaryWeaponItem;
                weaponData = weaponItem.ItemData as WeaponData;
                if (weaponData)
                {
                    ChangeCurrentWeapon(weaponData);
                }
                else
                {
                    ChangeCurrentWeapon(null);
                    weaponIdx = CurrentWeaponIdx.Unarmed;
                }
                break;
            case CurrentWeaponIdx.Secondary:
                if (_inventoryManager.SecondaryWeaponItem == null)
                {
                    ChangeCurrentWeapon(null);
                    _currentWeaponIdx = CurrentWeaponIdx.Unarmed;
                    return;
                }
                weaponItem = _inventoryManager.SecondaryWeaponItem;
                weaponData = weaponItem.ItemData as WeaponData;
                if (weaponData)
                {
                    ChangeCurrentWeapon(weaponData);
                }
                else
                {
                    ChangeCurrentWeapon(null);
                    weaponIdx = CurrentWeaponIdx.Unarmed;
                }
                break;
            case CurrentWeaponIdx.Unarmed: //비무장
                ChangeCurrentWeapon(null);
                break;
        }
        _currentWeaponIdx = weaponIdx;
    }

    private void ChangeCurrentWeapon(WeaponData newWeaponData) //무기 교체
    {
        if (!newWeaponData)
        {
            IsUnarmed = true;
            _playerAnimation.ChangeWeapon(WeaponType.Unarmed);
            oneHandSprite.enabled = false;
            twoHandSprite.enabled = false;
            return;
        }
        
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
        
        currentWeaponData = newWeaponData; //현재 무기데이터 
        _playerWeapon.ChangeWeaponData(newWeaponData); //변경
        _playerAnimation.ChangeWeapon(weaponType); //애니메이션 변경
        
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
            var item = itemPickUp.GetItemData();
            
            bool canEquip = false;
            //장착
            bool isGear = item.GearType != GearType.None;
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

            if (_inventoryManager.BackpackInventory)
            {
                var (isAvailable, firstIdx, slotRT) = 
                    _inventoryManager.BackpackInventory.CheckCanAddItem(item);
                canPickup = isAvailable;
                _pickupTargetSlotInfo = (firstIdx, slotRT);
                _pickupTargetIsPocket = false;
            }
            else if (_inventoryManager.RigInventory)
            {
                var (isAvailable, firstIdx, sloRT) = 
                    _inventoryManager.RigInventory.CheckCanAddItem(item);
                canPickup = isAvailable;
                _pickupTargetSlotInfo = (firstIdx, sloRT);
                _pickupTargetIsPocket = false;
            }
            else //리그, 가방에 공간이 없을 때
            {
                var checkCell = _inventoryManager.CheckCanEquipItem(item.GearType);
                if(checkCell is not null 
                   && item.ItemHeight == 1 && item.ItemWidth == 1) 
                {
                    canPickup = true; //Cell크기 고려
                    _pickupTargetCell = checkCell;
                    _pickupTargetIsPocket = true;
                }
            }
            
            _currentItemInteractList.Clear();
            _currentItemInteractIdx = 0;
            if(isGear) _currentItemInteractList.Add((canEquip, ItemInteractType.Equip));
            _currentItemInteractList.Add((canPickup, ItemInteractType.PickUp));
            //버그... 장착할 수 없을때
            
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
        var itemData = _currentItemPickUp.GetItemData();
        var isGear = itemData.GearType != GearType.None;
        //개선방안???? bool List에서 개선..?
        //장비 - 장착/획득 순서
        //장비x - 획득만
        var (isAvailable, type) = _currentItemInteractList[_currentItemInteractIdx];
        
        if (!isAvailable) return;

        var item = new InventoryItem(itemData);
        switch (type)
        {
            case ItemInteractType.Equip:
                //Event
                _inventoryManager.EquipGearItem(_pickupTargetCell, item); //사이즈 준내큼
                if (_pickupTargetCell == _inventoryManager.PrimaryWeaponSlot)
                {
                    HandleOnChangeWeapon(CurrentWeaponIdx.Primary);
                }
                else if (_pickupTargetCell == _inventoryManager.SecondaryWeaponSlot)
                {
                    HandleOnChangeWeapon(CurrentWeaponIdx.Secondary);
                }
                break;
            case ItemInteractType.PickUp:
                if (!_pickupTargetIsPocket)
                {
                    _inventoryManager.AddItemToInventory(_pickupTargetSlotInfo.firstIdx, _pickupTargetSlotInfo.slotRT, item);
                }
                else _inventoryManager.EquipGearItem(_pickupTargetCell, item);
                break;
            default:
                Debug.LogWarning("ItemInteractType Error: None.");
                break;
        }
        
        Destroy(_currentItemPickUp.gameObject); //수정
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
