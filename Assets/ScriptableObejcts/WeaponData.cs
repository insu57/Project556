using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : BaseItemDataSO
{
    [SerializeField] private string itemID;
    [SerializeField] private string weaponName;
    
    [SerializeField] private WeaponType weaponType;
    [SerializeField] private AmmoCaliber ammoCaliber;
    [SerializeField] private int defaultMagazineSize;
    [SerializeField] private int damage;
    
    [SerializeField] private bool canFullAuto = false;
    [SerializeField] private bool isOneHanded = false;
    
    [SerializeField] private float fireRate;
    [SerializeField] private float accuracy;
    
    [SerializeField] private float bulletSpeed; //Bullet에 따로?
    
    [SerializeField] private Sprite itemSprite;
    [SerializeField] private Vector2 pickUpColliderOffset;
    [SerializeField] private Vector2 pickUpColliderSize;
    [SerializeField] private int itemWidth;
    [SerializeField] private int itemHeight;
    [SerializeField] private Vector3 muzzlePosition;
    
    public override string ItemID => itemID;
    public override string ItemName => weaponName;
    public WeaponType WeaponType => weaponType;
    public AmmoCaliber AmmoCaliber => ammoCaliber;
    public int DefaultMagazineSize => defaultMagazineSize;
    public int Damage => damage;
    public bool CanFullAuto => canFullAuto;
    public bool IsOneHanded => isOneHanded;
    public float FireRate => fireRate;
    public float Accuracy => accuracy;
    public float BulletSpeed => bulletSpeed;
    [ShowInInspector] private float RPM => 1 / FireRate * 60; //Rounds Per Minute (FireRate = 0.1 => 600 RPM)

    public override Sprite ItemSprite => itemSprite;
    public override Vector2 PickUpColliderOffset => pickUpColliderOffset;
    public override Vector2 PickUpColliderSize => pickUpColliderSize;
    public override int ItemWidth => itemWidth;
    public override int ItemHeight => itemHeight;
    public override GearType GearType => GearType.Weapon;
    public Vector3 MuzzlePosition => muzzlePosition;
    public override bool IsStackable => false;
    public override int MaxStackAmount => 1;
}
