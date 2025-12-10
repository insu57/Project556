using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    private WeaponData _weaponData;
    private AmmoData _ammoData;

    public void SetWeapon(WeaponData weaponData, AmmoData ammoData)
    {
        _weaponData = weaponData;
        _ammoData = ammoData;
    }
    
    
    
}
