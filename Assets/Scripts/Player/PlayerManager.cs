using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player
{
    public class PlayerManager : MonoBehaviour
    {
        //PlayerData로 이동...
        
        [SerializeField] private SpriteRenderer oneHandSprite;
        [SerializeField] private Transform oneHandMuzzleTransform;
        [SerializeField] private SpriteRenderer twoHandSprite;
        [SerializeField] private Transform twoHandMuzzleTransform;
        [SerializeField] private GameObject muzzleFlashVFX;
    
        private Camera _mainCamera;
        
        private PlayerData _playerData;
        private PlayerWeapon _playerWeapon;
        private PlayerAnimation _playerAnimation;
        private PlayerControl _playerControl;
        
        private InventoryManager _inventoryManager;
    
        private WeaponInstance _currentWeaponItem;
        private EquipWeaponIdx _equipWeaponIdx = EquipWeaponIdx.Unarmed; //초기 Unarmed

        [SerializeField] private AudioSource loopSource;
        [SerializeField] private AudioSource oneShotSource;
        public AudioSource OneShotSource => oneShotSource;
        public float LastFootstepTime { get; set; }
        
        public event Action<bool, int> OnUpdateMagazineCountUI;
    
        public event Action<Vector2, List<(bool available, ItemInteractType type)>> OnShowItemPickup;
        public event Action<int> OnScrollItemPickup;
        public event Action OnHideItemPickup;
        public event Action OnReloadNoAmmo;
        public event Action<AmmoCategory, FireMode> OnToggleFireMode;
        public event Action<bool> OnShowAmmoIndicator;

        private bool _canItemInteract;
        private int _currentItemInteractIdx;
        private readonly List<(bool available, ItemInteractType type)> _currentItemInteractList = new();
        private ItemPickUp _currentItemPickUp;
        private CellData _pickupTargetCell;
        private (int firstIdx, RectTransform slotRT, Inventory inventory) _pickupTargetInvenSlotInfo;
        private bool _pickupTargetIsPocket;

        //UI Manager 개선?
        private void Awake()
        {
            TryGetComponent(out _playerData);
            TryGetComponent(out _playerAnimation);
            TryGetComponent(out _playerControl);
            TryGetComponent(out _playerWeapon);
            var stageManager = FindAnyObjectByType<StageManager>();
            
            _playerControl.MoveSpeed = _playerData.MoveSpeed;
            _playerControl.SprintSpeedMultiplier = _playerData.SprintSpeedMultiplier;
            _playerControl.JumpSpeed = _playerData.JumpSpeed; //개선?
            
            //SMB 
            var reloadAnimationBehaviour = _playerAnimation.UpperAnimator.GetBehaviour<ReloadAnimationBehaviour>();
            reloadAnimationBehaviour.Init(_playerControl, this);
            var sprintAnimationBehaviours =  
                _playerAnimation.LowerAnimator.GetBehaviours<MoveAnimationBehaviour>();
            foreach (var behaviour in sprintAnimationBehaviours)
            {
                behaviour.Init(this, _playerData, stageManager);
            }
            
            _mainCamera = Camera.main;
            _inventoryManager = FindFirstObjectByType<InventoryManager>(); //개선점???
        }

        private void Start()
        {
            ChangeCurrentWeapon(null); //비무장 초기화
        }

        private void OnEnable()
        {
            //플레이어 조작
            _playerControl.OnPlayerMove += HandleOnPlayerMoveAction;
            _playerControl.OnPlayerSprint += HandleOnPlayerSprintAction;
            _playerControl.OnPlayerReload += HandleOnPlayerReloadAction;
            _playerControl.OnFieldInteract += GetFieldItem;
            _playerControl.OnScrollInteractMenu += ScrollItemPickup;
            _playerControl.OnChangeWeaponAction += HandleOnChangeWeapon;
            _playerControl.OnQuickSlotAction += HandleOnUseQuickSlot;
            _playerControl.OnShootAction += Shoot;
            _playerControl.OnReloadEndAction += HandleOnReloadEnd;
            _playerControl.OnToggleFireModeAction += HandleOnToggleFireMode;
                
            //무기
            _playerWeapon.OnShowMuzzleFlash += HandleOnShowMuzzleFlash;
            
            //데이터
            _playerData.OnStaminaEmpty += HandleOnStaminaEmpty;
            
            //인벤토리
            _inventoryManager.OnUpdateArmorAmount += HandleOnUpdateArmorAmount;
            _inventoryManager.OnUnequipWeapon += HandleOnUnequipWeapon;
            _inventoryManager.OnItemEffectStatAdjust += HandleOnItemEffectStatAdjust;
            _inventoryManager.OnItemEffectStatPerSecond += HandleOnItemEffectStatPerSecond;
        }
        
        private void OnDisable()
        {
            _playerControl.OnPlayerMove -= HandleOnPlayerMoveAction;
            _playerControl.OnPlayerSprint -= HandleOnPlayerSprintAction;
            _playerControl.OnPlayerReload -= HandleOnPlayerReloadAction;
            _playerControl.OnFieldInteract -= GetFieldItem;
            _playerControl.OnScrollInteractMenu -=  ScrollItemPickup;
            _playerControl.OnChangeWeaponAction -= HandleOnChangeWeapon;
            _playerControl.OnQuickSlotAction -= HandleOnUseQuickSlot;
            _playerControl.OnShootAction -= Shoot;
            _playerControl.OnReloadEndAction -= HandleOnReloadEnd;
            _playerControl.OnToggleFireModeAction -= HandleOnToggleFireMode;
            
            _playerWeapon.OnShowMuzzleFlash -= HandleOnShowMuzzleFlash;

            _playerData.OnStaminaEmpty -= HandleOnStaminaEmpty;
            
            _inventoryManager.OnUpdateArmorAmount -= HandleOnUpdateArmorAmount;
            _inventoryManager.OnUnequipWeapon -= HandleOnUnequipWeapon;
            _inventoryManager.OnItemEffectStatAdjust -= HandleOnItemEffectStatAdjust;
            _inventoryManager.OnItemEffectStatPerSecond -= HandleOnItemEffectStatPerSecond;
        }

        private void HandleOnShowMuzzleFlash()
        {
            muzzleFlashVFX.SetActive(true);
        }

        private void HandleOnUpdateArmorAmount(float amount)
        {
            _playerData.UpdateTotalArmor(amount);
        }

        private void HandleOnUnequipWeapon(EquipWeaponIdx weaponIdx)
        {
            if (_equipWeaponIdx == weaponIdx)
            {
                ChangeCurrentWeapon(null);
            }
        }

        private void HandleOnPlayerMoveAction(bool isMove)
        {
            _playerAnimation.ChangeAnimationMove(isMove);
        }

        private void HandleOnPlayerSprintAction(bool isSprint)
        {
            if (!_playerData.CanSprint) isSprint = false;
            _playerAnimation.ChangeAnimationSprint(isSprint);
            _playerData.SprintStaminaSpend(isSprint);
        }

        private void HandleOnStaminaEmpty()
        {
            _playerAnimation.ChangeAnimationSprint(false);
            _playerData.SprintStaminaSpend(false);
        }
        
        private void HandleOnPlayerReloadAction()
        {
            _playerAnimation.ChangeAnimationReload();
        }

        private bool CheckIsAutomatic()
        {
            if (_currentWeaponItem is { ItemData: WeaponData weaponData })
                return weaponData.CanFullAuto;
            return false;
        }

        private bool CheckIsOneHanded()
        {
            if (_currentWeaponItem is { ItemData: WeaponData weaponData })
                return weaponData.IsOneHanded;
            return false;
        }
        
        //개선?
        private void PlayShootSFX()
        {
            AudioManager.Instance.PlaySFX(oneShotSource, SFXType.Weapon ,_currentWeaponItem.WeaponData.ShootSFX);
        }

        public float PlayReloadSFX()
        {
            AudioManager.Instance.PlaySFX(oneShotSource, SFXType.Weapon, _currentWeaponItem.WeaponData.ReloadSFX);
            return _currentWeaponItem.WeaponData.ReloadTime;
        }

        private void Shoot(bool isFlipped, float shootAngle) //사격
        {
            //총알 데이터..?
            if(GetCurrentWeaponMagazineCount() <= 0) return;
           
            bool isShoot = _playerWeapon.Shoot(isFlipped, shootAngle);
        
            if (!isShoot) return;
            PlayShootSFX();
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

        private void HandleOnReloadEnd() //개선?
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
                    _inventoryManager.LoadAmmo(weaponData.AmmoCaliber, ammoToRefill);

                if (canReload)
                {
                    _currentWeaponItem.ReloadAmmo(currentAmmo + reloadAmmo);
                    OnUpdateMagazineCountUI?.Invoke(_currentWeaponItem.IsFullyLoaded(), _currentWeaponItem.CurrentMagazineCount);
                    _inventoryManager.UpdateWeaponMagCount(_currentWeaponItem.InstanceID);
                }
                else
                {
                    OnReloadNoAmmo?.Invoke(); //장전할 탄이 하나도 없으면 경고문구 띄우기 -> 개선??
                }
                //장전 제어?
            }
        }

        private void HandleOnToggleFireMode() //이미지로 표시(장탄수 옆에 추가)
        {
            if (_currentWeaponItem == null) return;
            AudioManager.Instance.PlaySFX(oneShotSource, SFXType.Weapon, SFX.Selector);
            _currentWeaponItem.ToggleFireMode();
            _playerControl.CurrentFireMode = _currentWeaponItem.CurrentFireMode; //일단은 단발/연발만.
            OnToggleFireMode?.Invoke(_currentWeaponItem.AmmoCategory, _currentWeaponItem.CurrentFireMode);
        }
        
        private void HandleOnChangeWeapon(EquipWeaponIdx weaponIdx)
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
                default: return;
            }
            _equipWeaponIdx = weaponIdx;
        }

        private void ChangeCurrentWeapon(WeaponInstance weaponItem) //무기 교체
        {
            if (weaponItem == null)
            {
                //IsUnarmed = true;
                _playerControl.IsUnarmed = true;
                _playerAnimation.ChangeWeapon(WeaponType.Unarmed);
                oneHandSprite.enabled = false;
                twoHandSprite.enabled = false;
                //OnUpdateMagazineCountUI?.Invoke(false, 0);
                OnShowAmmoIndicator?.Invoke(false);
                return;
            }

            var newWeaponData = weaponItem.WeaponData;
            var weaponType = newWeaponData.WeaponType; //무기 타입
   
            _playerControl.IsUnarmed = false;
            _playerControl.IsOneHanded = CheckIsOneHanded();
            _playerControl.CurrentFireMode = _currentWeaponItem.CurrentFireMode;
            //_playerControl.IsAutomatic = CheckIsAutomatic();
            
            if (weaponType == WeaponType.Pistol) //한손무기
            {
                oneHandSprite.sprite = newWeaponData.ItemSprite; //스프라이트 위치
                oneHandSprite.enabled = true;
                twoHandSprite.enabled = false;
                oneHandMuzzleTransform.localPosition = newWeaponData.MuzzlePosition;
                _playerWeapon.SetMuzzleTransform(oneHandMuzzleTransform); //총구위치 설정
            
                muzzleFlashVFX.transform.SetParent(oneHandMuzzleTransform);
            }
            else //양손무기
            {
                twoHandSprite.sprite = newWeaponData.ItemSprite;
                twoHandSprite.enabled = true;
                oneHandSprite.enabled = false;
                twoHandMuzzleTransform.localPosition = newWeaponData.MuzzlePosition;
                _playerWeapon.SetMuzzleTransform(twoHandMuzzleTransform);
            
                muzzleFlashVFX.transform.SetParent(twoHandMuzzleTransform);
            }
            muzzleFlashVFX.transform.localRotation = Quaternion.identity;
            muzzleFlashVFX.transform.localPosition = newWeaponData.MuzzleFlashOffset;

            _playerWeapon.ChangeWeaponData(newWeaponData); //변경
            _playerAnimation.ChangeWeapon(weaponType); //애니메이션 변경
            OnShowAmmoIndicator?.Invoke(true);
            OnUpdateMagazineCountUI?.Invoke(_currentWeaponItem.IsFullyLoaded(), GetCurrentWeaponMagazineCount());
            OnToggleFireMode?.Invoke(_currentWeaponItem.AmmoCategory, _currentWeaponItem.CurrentFireMode);
            
        }

        private void HandleOnUseQuickSlot(QuickSlotIdx slotIdx)
        {
            if(!_inventoryManager.QuickSlotDict.TryGetValue(slotIdx, out var quickSlotInfo)) return;
            var (id, _) = quickSlotInfo;
            if(id == Guid.Empty) return; //빈 퀵슬롯이면 return
            _inventoryManager.UseQuickSlotItem(slotIdx);
        }

        private void ScrollItemPickup(float scrollDeltaY)
        {
            if(!_canItemInteract) return;
            
            _currentItemInteractIdx += (int)scrollDeltaY;
            if (_currentItemInteractIdx < 0) _currentItemInteractIdx = _currentItemInteractList.Count - 1;
            else if (_currentItemInteractIdx >= _currentItemInteractList.Count) _currentItemInteractIdx = 0;
            //범위 넘기면 처리
      
            OnScrollItemPickup?.Invoke(_currentItemInteractIdx);
        }

        private void HandleOnItemEffectStatAdjust(StatAdjustAmount statAdjustAmount, float useDuration)
        {
            _playerData.ItemEffectAdjustStat(statAdjustAmount, useDuration);
        }

        private void HandleOnItemEffectStatPerSecond(StatEffectPerSecond statEffectPerSecond)
        {
            _playerData.ItemEffectAdjustStatPerSecond(statEffectPerSecond);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Item"))
            {
                //pick up ui
                _canItemInteract = true;
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
                
                OnShowItemPickup?.Invoke(pos, _currentItemInteractList);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Item"))
            {
                _canItemInteract = false;
                OnHideItemPickup?.Invoke();
            }
        }

        private void GetFieldItem()
        {
            if(!_canItemInteract) return;
            
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
        
            _canItemInteract = false;
            
            OnHideItemPickup?.Invoke();
        
            ObjectPoolingManager.Instance.ReleaseItemPickUp(_currentItemPickUp);
        }

        
    }
}
