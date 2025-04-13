using TMPro;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private int playerHealth = 100; //추후 SO에서 받아오게 수정 예정

    [SerializeField] private PlayerWeapon currentWeapon;
    
    private int _currentHealth;
    private int _currentMagazineAmmo;
    [SerializeField] private TMP_Text currentMagazineAmmoText;
    
    public Transform MuzzleTransform => currentWeapon?.MuzzleTransform;
    
    private void Awake()
    {
        
    }

    private void Start()
    {
        _currentMagazineAmmo = currentWeapon.WeaponData.DefaultMagazineSize;
        currentMagazineAmmoText.text = _currentMagazineAmmo.ToString();
    }

    public bool Shoot()
    {
        if(_currentMagazineAmmo <= 0) return false;
        _currentMagazineAmmo--;
        currentMagazineAmmoText.text = _currentMagazineAmmo.ToString();
        return true;
    }

    public void Reload()
    {
        _currentMagazineAmmo = currentWeapon.WeaponData.DefaultMagazineSize;
        currentMagazineAmmoText.text = _currentMagazineAmmo.ToString();
    }
}
