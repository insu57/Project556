using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private int playerHealth = 100; //추후 SO에서 받아오게 수정 예정
    [SerializeField] private WeaponData currentWeaponData;//현재 직렬화(추후 인벤토리에서)
    [SerializeField] private SpriteRenderer oneHandSprite;
    [SerializeField] private Transform oneHandMuzzleTransform;
    [SerializeField] private SpriteRenderer twoHandSprite;
    [SerializeField] private Transform twoHandMuzzleTransform;
    
    private Camera _mainCamera;
    private UIManager _uiManager;
    private PlayerWeapon _playerWeapon;
    private PlayerAnimation _playerAnimation;
    private InventoryManager _inventoryManager;
    private InventoryUIPresenter _inventoryUIPresenter;
    
    private int _currentHealth;
    public bool CanItemInteract { get; private set; }
    private IItemData _currentPickupItemData;
    private bool _currentItemCanEquip;
    private bool _currentItemCanPickup;
    
    private void Awake()
    {
        _uiManager = FindFirstObjectByType<UIManager>();
        //_playerAnimation = GetComponent<PlayerAnimation>();
        TryGetComponent(out _playerAnimation);
        //_playerWeapon = GetComponent<PlayerWeapon>();
        TryGetComponent(out _playerWeapon);
        
        _mainCamera = Camera.main;
        _inventoryManager = FindFirstObjectByType<InventoryManager>(); //개선점???
        
    }

    private void Start()
    {
        WeaponChange(currentWeaponData);
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

    private void WeaponChange(WeaponData newWeaponData) //무기 교체
    {
        WeaponType weaponType = newWeaponData.WeaponType; //무기 타입
       
        if (weaponType == WeaponType.Pistol) //한손무기
        {
            oneHandSprite.sprite = newWeaponData.ItemSprite; //스프라이트 위치
            oneHandSprite.enabled = true;
            twoHandSprite.enabled = false;
            oneHandMuzzleTransform.localPosition = newWeaponData.MuzzlePosition;
            _playerWeapon.SetMuzzleTransform(oneHandMuzzleTransform); //총구위치 설정
        }
        else //양손무기
        {
            twoHandSprite.sprite = newWeaponData.ItemSprite;
            twoHandSprite.enabled = true;
            oneHandSprite.enabled = false;
            twoHandMuzzleTransform.localPosition = newWeaponData.MuzzlePosition;
            _playerWeapon.SetMuzzleTransform(twoHandMuzzleTransform);
        }
        
        currentWeaponData = newWeaponData; //현재 무기데이터 
        _playerWeapon.Init(_uiManager, currentWeaponData); //초기화
        _playerAnimation.ChangeWeapon(weaponType); //애니메이션 변경
        
    }

    public void ScrollItemPickup(float y)
    {
        _uiManager.ScrollItemPickup(y);//ItemPickup UI 스크롤(획득/장착 등...)
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Item"))
        {
            //pick up ui
            CanItemInteract = true;
            Vector2 pos = _mainCamera.WorldToScreenPoint(other.transform.position);
            
            
            other.TryGetComponent<ItemPickUp>(out var itemPickUp);
            //_currentItemPickUp = itemPickUp;  //장착-획득 여부... -> InventoryManager참조...
            var item = itemPickUp.GetItemData();
            
            //isGear에 따른 상태?
            //enum? class?     UIManger
            //<-PlayerControl -(인덱스)->  PlayerManager
            
            List<bool> availableList = new(); // 
            
            
            bool canEquip = false;
            //장착
            bool isGear = item.GearType != GearType.None;
            if (item.GearType is not GearType.None)
            {
                var checkCell = _inventoryManager.CheckCanEquipItem(item.GearType);
                if (checkCell is not null) canEquip = true;
            }
            //inventory
            bool canPickup = false;

            if (_inventoryManager.BackpackInventory)
            {
                var (isAvailable, firstIdx, sloRT) = 
                    _inventoryManager.BackpackInventory.CheckCanAddItem(item);
                canPickup = isAvailable;
            }
            else if (_inventoryManager.RigInventory)
            {
                var (isAvailable, firstIdx, sloRT) = 
                    _inventoryManager.RigInventory.CheckCanAddItem(item);
                canPickup = isAvailable;
            }
            else //리그, 가방에 공간이 없을 때
            {
                var checkCell = _inventoryManager.CheckCanEquipItem(item.GearType);
                if(checkCell is not null 
                   && item.ItemHeight == 1 && item.ItemWidth == 1) canPickup = true; //Cell크기 고려
            }
            _uiManager.ShowItemPickup(pos, isGear, canEquip, canPickup); //이벤트로 수정 예정
            
            return;//임시 -> 인벤토리(가방)에 넣기(상호작용 키 누르면)
            
            //ObjectPooling?
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Item"))
        {
            CanItemInteract = false;
            _uiManager.HideItemPickup();
        }
    }

    public void GetFieldItem()
    {
        var isGear = _currentPickupItemData.GearType != GearType.None;
        if (isGear)
        {
            
        }
    }
}
