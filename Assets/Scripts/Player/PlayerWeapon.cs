using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    //변경? -> 들고있는건 Sprite만 정보는 다른 클래스로?
    [SerializeField] private Transform muzzleTransform;
    [SerializeField] private WeaponData weaponData;
    
    public Transform MuzzleTransform => muzzleTransform;
    public WeaponData WeaponData => weaponData;
}
