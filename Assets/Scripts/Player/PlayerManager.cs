using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private int playerHealth = 100; //추후 SO에서 받아오게 수정 예정
    
    [SerializeField] PlayerWeapon _currentWeapon;
    [SerializeField] private Transform oneHandWeaponTransform;
    [SerializeField] private Transform twoHandWeaponTransform;
    
    private UIManager _uiManager;
    private PlayerAnimation _playerAnimation;
    
    private int _currentHealth;
    
    
    private void Awake()
    {
        _uiManager = FindFirstObjectByType<UIManager>();
        _playerAnimation = GetComponent<PlayerAnimation>();
    }

    private void Start()
    {
        _currentWeapon.Init(_uiManager);
    }

    public bool CheckIsAutomatic()
    {
        return _currentWeapon.WeaponData.IsAutomatic;
    }

    public bool CheckIsOneHanded()
    {
        return _currentWeapon.WeaponData.IsOneHanded;
    }
    
    public void Shoot(bool isFlipped, float shootAngle)
    {
        _currentWeapon.Shoot(isFlipped, shootAngle);
    }

    public void Reload()
    {
        _currentWeapon.Reload();
    }

    private void WeaponChange(GameObject newWeaponGO)
    {
        if (_currentWeapon)
        {
            Destroy(_currentWeapon.gameObject);
        }

        PlayerWeapon newWeaponPrefab = newWeaponGO.GetComponent<PlayerWeapon>(); 
        WeaponType weaponType = newWeaponPrefab.WeaponData.WeaponType;
        Transform parent = weaponType == WeaponType.Pistol
            ? oneHandWeaponTransform : twoHandWeaponTransform;
      
        PlayerWeapon newWeapon = Instantiate(newWeaponPrefab, parent);
      
        _currentWeapon = newWeapon;
        _currentWeapon.Init(_uiManager);
        
        _playerAnimation.ChangeWeapon(weaponType);
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Item"))
        {
            ItemPickUp newItem = other.GetComponent<ItemPickUp>();
            if (newItem)
            {
                GameObject newItemGO = newItem.ItemPrefab;
                WeaponChange(newItemGO);
            }
            Destroy(other.gameObject);
        }
    }
}
