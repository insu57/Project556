using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    [SerializeField] private string weaponName;
    [FormerlySerializedAs("gunType")] [SerializeField] private WeaponType weaponType;
    [SerializeField] private AmmoType ammoType;
    [SerializeField] private int defaultMagazineSize;
    [SerializeField] private int damage;
    [SerializeField] private bool isAutomatic = false;
    [SerializeField] private bool isOneHanded = false;
    [SerializeField] private float fireRate;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed; //Bullet에 따로?
    
    
    public string WeaponName => weaponName;
    public WeaponType WeaponType => weaponType;
    public AmmoType AmmoType => ammoType;
    public int DefaultMagazineSize => defaultMagazineSize;
    public int Damage => damage;
    public bool IsAutomatic => isAutomatic;
    public bool IsOneHanded => isOneHanded;
    public float FireRate => fireRate;
    public GameObject BulletPrefab => bulletPrefab;
    public float BulletSpeed => bulletSpeed;
    [ShowInInspector] private float RPM => 1 / FireRate * 60;

}
