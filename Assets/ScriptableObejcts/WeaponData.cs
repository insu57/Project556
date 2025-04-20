using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    [SerializeField] private string weaponName;
    [SerializeField] private WeaponType weaponType;
    [SerializeField] private AmmoCaliber ammoCaliber;
    [SerializeField] private int defaultMagazineSize;
    [SerializeField] private int damage;
    [SerializeField] private bool isAutomatic = false;
    [SerializeField] private bool isOneHanded = false;
    [SerializeField] private float fireRate;
    [SerializeField] private float accuracy;
    [SerializeField] private float bulletSpeed; //Bullet에 따로?
    
    
    public string WeaponName => weaponName;
    public WeaponType WeaponType => weaponType;
    public AmmoCaliber AmmoCaliber => ammoCaliber;
    public int DefaultMagazineSize => defaultMagazineSize;
    public int Damage => damage;
    public bool IsAutomatic => isAutomatic;
    public bool IsOneHanded => isOneHanded;
    public float FireRate => fireRate;
    public float Accuracy => accuracy;
    public float BulletSpeed => bulletSpeed;
    [ShowInInspector] private float RPM => 1 / FireRate * 60; //Rounds Per Minute (FireRate = 0.1 => 600 RPM)

}
