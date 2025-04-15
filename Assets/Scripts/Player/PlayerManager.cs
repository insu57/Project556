using TMPro;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private int playerHealth = 100; //추후 SO에서 받아오게 수정 예정

    [SerializeField] private PlayerWeapon currentWeapon;
    
    private int _currentHealth;
    
    
    private void Awake()
    {
        
    }

    private void Start()
    {
        
    }

    public bool CheckIsAutomatic()
    {
        return currentWeapon.WeaponData.IsAutomatic;
    }
    
    public void Shoot(bool isFlipped, float shootAngle)
    {
        currentWeapon.Shoot(isFlipped, shootAngle);
    }

    public void Reload()
    {
        currentWeapon.Reload();
    }

    private void WeaponChange(PlayerWeapon newWeapon)
    {
        currentWeapon = newWeapon;
    }
}
