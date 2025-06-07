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
    
    private UIManager _uiManager;
    private PlayerWeapon _playerWeapon;
    private PlayerAnimation _playerAnimation;
    private InventoryManager _inventoryManager;
    private InventoryUIPresenter _inventoryUIPresenter;
    
    private int _currentHealth;
    public bool CanItemInteract { get; private set; }
    
    private void Awake()
    {
        _uiManager = FindFirstObjectByType<UIManager>();
        _playerAnimation = GetComponent<PlayerAnimation>();
        _playerWeapon = GetComponent<PlayerWeapon>();
        //_inventoryManager = GetComponent<InventoryManager>();
        //_inventoryManager.Init();
        
        //_inventoryUIPresenter = new InventoryUIPresenter(_inventoryManager, _uiManager);
        
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
            Vector2 pos = Camera.main.WorldToScreenPoint(other.transform.position);
            
            _uiManager.ShowItemPickup(true, pos);
            
            return;//임시 -> 인벤토리(가방)에 넣기(상호작용 키 누르면)
            ItemPickUp newItem = other.GetComponent<ItemPickUp>();
            if (newItem)
            {
                IItemData newItemData = newItem.GetItemData();
                if (newItemData is WeaponData weaponData) //임시 - 무기 교체. -> 인벤토리 추가로 변경
                {
                    WeaponChange(weaponData);
                }
            }
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Item"))
        {
            CanItemInteract = false;
            _uiManager.ShowItemPickup(false, Vector2.zero);
        }
    }
}
