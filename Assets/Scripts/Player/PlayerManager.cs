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
    
    private int _currentHealth;
    
    
    private void Awake()
    {
        _uiManager = FindFirstObjectByType<UIManager>();
        _playerAnimation = GetComponent<PlayerAnimation>();
        _playerWeapon = GetComponent<PlayerWeapon>();
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
    
    public void Shoot(bool isFlipped, float shootAngle)
    {
        _playerWeapon.Shoot(isFlipped, shootAngle);
    }

    public void Reload()
    {
        _playerWeapon.Reload();
    }

    private void WeaponChange(WeaponData newWeaponData)
    {
        WeaponType weaponType = newWeaponData.WeaponType;
       
        if (weaponType == WeaponType.Pistol)
        {
            oneHandSprite.sprite = newWeaponData.ItemSprite;
            oneHandSprite.enabled = true;
            twoHandSprite.enabled = false;
            oneHandMuzzleTransform.localPosition = newWeaponData.MuzzlePosition;
            _playerWeapon.SetMuzzleTransform(oneHandMuzzleTransform);
        }
        else
        {
            twoHandSprite.sprite = newWeaponData.ItemSprite;
            twoHandSprite.enabled = true;
            oneHandSprite.enabled = false;
            twoHandMuzzleTransform.localPosition = newWeaponData.MuzzlePosition;
            _playerWeapon.SetMuzzleTransform(twoHandMuzzleTransform);
        }
        
        currentWeaponData = newWeaponData;
        _playerWeapon.Init(_uiManager, currentWeaponData);
        _playerAnimation.ChangeWeapon(weaponType);
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Item"))
        {
            ItemPickUp newItem = other.GetComponent<ItemPickUp>();
            if (newItem)
            {
                IItemData newItemData = newItem.GetItemData();
                if (newItemData is WeaponData weaponData)
                {
                    WeaponChange(weaponData);
                }
            }
            Destroy(other.gameObject);
        }
    }
}
