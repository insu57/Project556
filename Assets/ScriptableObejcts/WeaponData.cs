using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    [SerializeField] private string weaponName;
    [SerializeField] private GunType gunType;
    [SerializeField] private AmmoType ammoType;
    [SerializeField] private int defaultMagazineSize;
    [SerializeField] private int damage;
    [SerializeField] private bool isAutomatic = false;
    [SerializeField] private float fireRate;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed; //Bullet에 따로?

    public string WeaponName => weaponName;
    public GunType GunType => gunType;
    public AmmoType AmmoType => ammoType;
    public int DefaultMagazineSize => defaultMagazineSize;
    public int Damage => damage;
    public bool IsAutomatic => isAutomatic;
    public float FireRate => fireRate;
    public GameObject BulletPrefab => bulletPrefab;
    public float BulletSpeed => bulletSpeed;
    [ShowInInspector] private float RPM => 1 / FireRate * 60;

}
