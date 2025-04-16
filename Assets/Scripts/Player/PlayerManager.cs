using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private int playerHealth = 100; //추후 SO에서 받아오게 수정 예정

    //[SerializeField] private GameObject currentWeaponGO;
    [SerializeField] PlayerWeapon _currentWeapon;
    [SerializeField] private Transform oneHandWeaponTransform;
    [SerializeField] private Transform twoHandWeaponTransform;
    
    private PlayerAnimation _playerAnimation;
    
    private int _currentHealth;
    
    
    private void Awake()
    {
        _playerAnimation = GetComponent<PlayerAnimation>();
    }

    private void Start()
    {
        _currentWeapon.Init();
    }

    public bool CheckIsAutomatic()
    {
        return _currentWeapon.WeaponData.IsAutomatic;
    }
    
    public void Shoot(bool isFlipped, float shootAngle)
    {
        _currentWeapon.Shoot(isFlipped, shootAngle);
    }

    public void Reload()
    {
        _currentWeapon.Reload();
    }

    private void WeaponChange(PlayerWeapon newWeapon)
    {
        if (_currentWeapon)
        {
            Destroy(_currentWeapon.gameObject);
        }
        
        _currentWeapon = newWeapon;
        GameObject currentWeaponGO = newWeapon.gameObject;
        _currentWeapon.Init();

        WeaponType newWeaponType = newWeapon.WeaponData.WeaponType;
        currentWeaponGO.transform.SetParent(newWeaponType == WeaponType.Pistol
            ? oneHandWeaponTransform : twoHandWeaponTransform); //맵에 있는 무기와 실제로 들고있을 무기 별개로 두고 Instantiate
        _playerAnimation.ChangeWeapon(newWeaponType);
       
        currentWeaponGO.transform.localPosition = Vector3.zero;
        currentWeaponGO.transform.localRotation = Quaternion.identity;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Item"))
        {
            PlayerWeapon newWeapon = other.GetComponent<PlayerWeapon>();
            if (newWeapon)
            {
                WeaponChange(newWeapon);
            }
        }
    }
}
