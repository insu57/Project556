using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Item;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = System.Object;

namespace Player
{
    public class PlayerManager : MonoBehaviour //개선필요(
    {
        [SerializeField] private SpriteRenderer oneHandSprite; //한손무기 Sprite
        [SerializeField] private Transform oneHandMuzzleTransform; //한손무기 Muzzle Transform
        [SerializeField] private SpriteRenderer twoHandSprite; //두손무기 Sprite
        [SerializeField] private Transform twoHandMuzzleTransform; //두손무기 Muzzle Transform
        [SerializeField] private GameObject muzzleFlashVFX; //총구화염VFX
    
        private PlayerData _playerData;
        private PlayerWeapon _playerWeapon;
        private PlayerAnimation _playerAnimation;
        private PlayerControl _playerControl;
        private PlayerInteract _playerInteract;
        private InventoryManager _inventoryManager;
    
        private WeaponInstance _currentWeaponItem; //현재 장착한 무기 아이템
        private EquipWeaponIdx _equipWeaponIdx = EquipWeaponIdx.Unarmed; //초기 Unarmed

        [SerializeField] private AudioSource loopSource;
        [SerializeField] private AudioSource oneShotSource; //PlayOneShot Source
        public AudioSource OneShotSource => oneShotSource;
        public float LastFootstepTime { get; set; } //마지막 재생된 발소리 시간
        
        public event Action<bool, int> OnUpdateMagazineCountUI; //장탄수 UI 업데이트
        public event Action OnReloadNoAmmo; //남은 탄약이 없을 때 경고
        public event Action<AmmoCategory, FireMode> OnToggleFireMode; //FireMode변경
        public event Action<bool> OnShowAmmoIndicator; //무기 전환 시 장탄, 조정관 표시

        //UI Manager 개선?
        private void Awake()
        {
            TryGetComponent(out _playerData);
            TryGetComponent(out _playerAnimation);
            TryGetComponent(out _playerControl);
            TryGetComponent(out _playerWeapon);
            TryGetComponent(out _playerInteract);
            var stageManager = FindAnyObjectByType<StageManager>();
            _inventoryManager = FindFirstObjectByType<InventoryManager>(); //개선점???
            
            _playerControl.MoveSpeed = _playerData.MoveSpeed;
            _playerControl.SprintSpeedMultiplier = _playerData.SprintSpeedMultiplier;
            _playerControl.JumpForce = _playerData.JumpForce; //개선?
            
            //SMB Init
            var reloadAnimationBehaviour = _playerAnimation.UpperAnimator.GetBehaviour<ReloadAnimationBehaviour>();
            reloadAnimationBehaviour.Init(_playerControl, this);
            var sprintAnimationBehaviours =  
                _playerAnimation.LowerAnimator.GetBehaviours<MoveAnimationBehaviour>();
            foreach (var behaviour in sprintAnimationBehaviours)
            {
                behaviour.Init(this, _playerData, stageManager);
            }
            var loadAmmoAnimationBehaviours = _playerAnimation.UpperAnimator.GetBehaviours<LoadAmmoAnimationBehaviour>();
            foreach (var behaviour in loadAmmoAnimationBehaviours)
            {
                behaviour.Init(this);
            }
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
            _playerControl.OnFieldInteract += FieldInteract;
            _playerControl.OnScrollInteractMenu += ScrollItemPickup;
            _playerControl.OnChangeWeaponAction += HandleOnChangeWeapon;
            _playerControl.OnQuickSlotAction += HandleOnUseQuickSlot;
            _playerControl.OnShootAction += Shoot;
            _playerControl.OnBurstShootAction += BurstShoot;
            _playerControl.OnReloadEndAction += HandleOnReloadEnd;
            _playerControl.OnToggleFireModeAction += HandleOnToggleFireMode;
                
            //무기
            _playerWeapon.OnShowMuzzleFlash += HandleOnShowMuzzleFlash;
            _playerWeapon.OnEndWeaponAction += HandleOnEndWeaponAction;
            
            //데이터
            _playerData.OnStaminaEmpty += HandleOnStaminaEmpty;
            
            //인벤토리
            _inventoryManager.OnUpdateArmorAmount += HandleOnUpdateArmorAmount;
            _inventoryManager.OnUnequipWeapon += HandleOnUnequipWeapon;
            _inventoryManager.OnUseItem += HandleOnUseItem;
            _inventoryManager.OnEquipFieldWeapon += HandleOnChangeWeapon;
        }
        
        private void OnDisable()
        {
            _playerControl.OnPlayerMove -= HandleOnPlayerMoveAction;
            _playerControl.OnPlayerSprint -= HandleOnPlayerSprintAction;
            _playerControl.OnPlayerReload -= HandleOnPlayerReloadAction;
            _playerControl.OnFieldInteract -= FieldInteract;
            _playerControl.OnScrollInteractMenu -=  ScrollItemPickup;
            _playerControl.OnChangeWeaponAction -= HandleOnChangeWeapon;
            _playerControl.OnQuickSlotAction -= HandleOnUseQuickSlot;
            _playerControl.OnShootAction -= Shoot;
            _playerControl.OnBurstShootAction -= BurstShoot;
            _playerControl.OnReloadEndAction -= HandleOnReloadEnd;
            _playerControl.OnToggleFireModeAction -= HandleOnToggleFireMode;
            
            _playerWeapon.OnShowMuzzleFlash -= HandleOnShowMuzzleFlash;
            _playerWeapon.OnEndWeaponAction -= HandleOnEndWeaponAction;

            _playerData.OnStaminaEmpty -= HandleOnStaminaEmpty;
            
            _inventoryManager.OnUpdateArmorAmount -= HandleOnUpdateArmorAmount;
            _inventoryManager.OnUnequipWeapon -= HandleOnUnequipWeapon;
            _inventoryManager.OnUseItem -= HandleOnUseItem;
            _inventoryManager.OnEquipFieldWeapon -= HandleOnChangeWeapon;
        }

        private void HandleOnShowMuzzleFlash() //사격 시 총구화염 VFX 오브젝트(애니메이션 자동 재생)
        {
            muzzleFlashVFX.SetActive(true);
        }

        private void HandleOnUpdateArmorAmount(float amount) //총 방어도 업데이트
        {
            _playerData.UpdateTotalArmor(amount);
        }

        private void HandleOnUnequipWeapon(EquipWeaponIdx weaponIdx) //들고있는 무기 장착해제 시
        {
            if (_equipWeaponIdx == weaponIdx)
            {
                ChangeCurrentWeapon(null);
            }
        }

        private void HandleOnPlayerMoveAction(bool isMove) //이동 전환
        {
            _playerAnimation.ChangeAnimationMove(isMove);
        }

        private void HandleOnPlayerSprintAction(bool isSprint) //달리기 전환
        {
            if (!_playerData.CanSprint) isSprint = false;
            _playerAnimation.ChangeAnimationSprint(isSprint);
            _playerData.SprintStaminaSpend(isSprint);
        }

        private void HandleOnStaminaEmpty() //스태미나가 없을 때
        {
            _playerAnimation.ChangeAnimationSprint(false);
        }
        
        private void HandleOnPlayerReloadAction() //장전 시 애니메이션 재생
        {
            if (_currentWeaponItem.IsFullyLoaded()) return;//탄창이 가득이면 장전하지않음
            if (!CheckHaveAmmo()) return; 
            //장탄이 있으면
            _playerAnimation.ChangeAnimationReload();//장전 애니메이션 전환
            _playerControl.SetReloadState(true); //장전시 조작제한
        }

        private bool CheckWeaponIsOneHanded()
        {
            return _currentWeaponItem != null && _currentWeaponItem.WeaponData.IsOneHanded;
        }

        public bool CheckWeaponHasNotDetachMag() //내장탄창인지 체크
        {
            return _currentWeaponItem != null && !_currentWeaponItem.WeaponData.HasDetachableMagazine;
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

        public float PlayLoadAmmoSFX()
        {
            AudioManager.Instance.PlaySFX(oneShotSource, SFXType.Weapon, _currentWeaponItem.WeaponData.LoadAmmoSFX);
            return _currentWeaponItem.WeaponData.FireRate;
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

        private void BurstShoot(int burstCount, bool isFlipped, float shootAngle)
        {
            if (GetCurrentWeaponMagazineCount() < burstCount) burstCount = GetCurrentWeaponMagazineCount();

            StartCoroutine(BurstRoutine(burstCount, isFlipped, shootAngle));
        }

        private IEnumerator BurstRoutine(int burstCount, bool isFlipped, float shootAngle)
        {
            _playerControl.InShooting = true;
            for (int i = 0; i < burstCount; i++)
            {
                Shoot(isFlipped, shootAngle);
                yield return new WaitForSeconds(_currentWeaponItem.WeaponData.FireRate);
            }
            _playerControl.InShooting = false;
        }

        private int GetCurrentWeaponMagazineCount()
        {
            if (_currentWeaponItem != null)
            {
                return _currentWeaponItem.CurrentMagazineCount;
            }
            return -1;
        }

        private void HandleOnEndWeaponAction(WeaponActionType weaponActionType)
        {
            _playerAnimation.ChangeAnimationLoadAmmo(weaponActionType);
        }

        private bool CheckHaveAmmo()
        {
            var weaponData = _currentWeaponItem.WeaponData;
            var canReload = _inventoryManager.CheckHaveMatchAmmo(weaponData.AmmoCaliber); //탄이 한 발이라도 있는지 체크
            if (canReload) //장탄이 한발이라도 있다면 장전가능
            {
                return true;
            }
            OnReloadNoAmmo?.Invoke();
            return false;
        }
        
        private void HandleOnReloadEnd() //개선?
        {
            //Inven -> weapon
            if (_currentWeaponItem == null) return;
            
            var weaponData = _currentWeaponItem.WeaponData;
            var currentAmmo = _currentWeaponItem.CurrentMagazineCount;
            var ammoToRefill = _currentWeaponItem.GetAmmoToRefill();
            
            var (canReload, reloadAmmo, ammoData)  = 
                _inventoryManager.LoadAmmo(weaponData.AmmoCaliber, ammoToRefill); //Ammo 아이템 처리

            if (canReload)
            {
                _currentWeaponItem.ReloadAmmo(ammoData, currentAmmo + reloadAmmo); //무기에 장전
                OnUpdateMagazineCountUI?.Invoke(_currentWeaponItem.IsFullyLoaded(), _currentWeaponItem.CurrentMagazineCount);
                _inventoryManager.UpdateWeaponMagCount(_currentWeaponItem.InstanceID); //ItemDrag장탄표시
                _playerWeapon.SetAmmoData(ammoData);
                if (_currentWeaponItem.WeaponData.HasDetachableMagazine) return;
                
                var needAmmo = _currentWeaponItem.NeedAmmo();
                if (needAmmo > 0) //총알이 더 필요하다면 계속 장전
                {
                    _playerAnimation.ChangeAnimationReload();
                }
                else
                {
                    _playerControl.SetReloadState(false);//장전종료
                }
            }
            else
            {
                _playerControl.SetReloadState(false); //장전 불가 시 종료
            }
            //장전 관련 추가?(약실비었으면 다른 애니메이션)
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
                _playerControl.IsUnarmed = true;
                _playerAnimation.ChangeWeapon(WeaponType.Unarmed);
                oneHandSprite.enabled = false;
                twoHandSprite.enabled = false;
                OnShowAmmoIndicator?.Invoke(false);
                return;
            }

            var newWeaponData = weaponItem.WeaponData;
            var weaponType = newWeaponData.WeaponType; //무기 타입
   
            _playerControl.IsUnarmed = false;
            _playerControl.IsOneHanded = CheckWeaponIsOneHanded();
            _playerControl.CurrentFireMode = _currentWeaponItem.CurrentFireMode;
  
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

            _playerWeapon.ChangeWeaponData(newWeaponData, weaponItem.CurrentAmmoData); //변경
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
            _playerInteract.ScrollItemPickup(scrollDeltaY);
        }

        private void HandleOnItemEffectStatAdjust(StatAdjustAmount statAdjustAmount, float useDuration)
        {
            _playerData.ItemEffectAdjustStat(statAdjustAmount, useDuration);
        }

        private void HandleOnItemEffectStatPerSecond(StatEffectPerSecond statEffectPerSecond)
        {
            _playerData.ItemEffectAdjustStatPerSecond(statEffectPerSecond);
        }

        private void HandleOnUseItem(IConsumableItem consumableItem)
        {
            foreach (var adjustAmount in consumableItem.AdjustAmount)
            {
                HandleOnItemEffectStatAdjust(adjustAmount, consumableItem.UseDuration);
            }

            foreach (var effectPerSecond in consumableItem.EffectPerSecond)
            {
                HandleOnItemEffectStatPerSecond(effectPerSecond); //초당효과 중복 제한... 같은 종류면 덮어씌우기?
            }
        }
      
        private void FieldInteract()
        {
            if(!_playerInteract.CanInteract) return;

            var (isAvailable, interactType) = _playerInteract.CurrentFieldInteract;
        
            if (!isAvailable) return;

            var currentFieldItem = _playerInteract.CurrentFieldItem;
            var currentLootInventory = _playerInteract.CurrentLootInventory;
            var currentFieldInteractable = _playerInteract.CurrentFieldInteractable;
            
            switch (interactType)
            {
                case InteractType.Equip: //장착
                    if(currentFieldItem == null) return; //null체크
                    _inventoryManager.EquipFieldItem(currentFieldItem);
                    break;
                case InteractType.PickUp: //획득
                    if(currentFieldItem == null) return; //null체크
                    _inventoryManager.AddFieldItem(currentFieldItem);
                    break;
                case InteractType.Use: //사용
                    if (currentFieldItem?.ItemData is IConsumableItem consumableItem) //소비아이템 이면
                    {
                        HandleOnUseItem(consumableItem);
                    }
                    break;
                case InteractType.Open: //상자 열기
                    if(!currentLootInventory) return;
                    var crate = currentFieldInteractable as LootCrate;
                    var crateName = crate?.CrateName;
                    _inventoryManager.SetLootInventory(currentLootInventory, crateName);
                    _playerControl.OnOpenCrate();
                    break;
                default:
                    Debug.LogWarning("ItemInteractType Error: None.");
                    break;
            }

            if (currentFieldInteractable is ItemPickUp itemPickUp)
            {
                _playerInteract.RemoveFieldInteractable(itemPickUp.gameObject);
                ObjectPoolingManager.Instance.ReleaseItemPickUp(itemPickUp);
            }
            else if (currentFieldInteractable is LootCrate lootCrate)
            {
                _playerInteract.RemoveFieldInteractable(lootCrate.gameObject);
            }
        }
    }
}
