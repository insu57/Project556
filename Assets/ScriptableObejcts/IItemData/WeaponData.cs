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
    
    [SerializeField] private bool canFullAuto = false;
    [SerializeField] private bool isOneHanded = false;
    [SerializeField] private bool isOpenBolt = false;
    
    [SerializeField] private float fireRate;
    [SerializeField] private float accuracy;
    
    [SerializeField] private float bulletSpeed; //Bullet에 따로?
    
    [SerializeField] private Sprite itemSprite;
    [SerializeField] private SFX shootSFX;
    [SerializeField] private SFX reloadSFX;
    [Tooltip("Seconds")]
    [SerializeField] private float reloadTime;
    
    [SerializeField] private int itemWidth;
    [SerializeField] private int itemHeight;
    [SerializeField] private float itemWeight;
    
    [SerializeField] private WeaponSetup weaponSetup;
    
    public override string ItemDataID => itemID;
    public override string ItemName => weaponName;
    public WeaponType WeaponType => weaponType;
    public AmmoCaliber AmmoCaliber => ammoCaliber;
    public int DefaultMagazineSize => defaultMagazineSize;
    public SFX ShootSFX => shootSFX;
    public SFX ReloadSFX => reloadSFX;
    public float ReloadTime => reloadTime;
    
    public bool CanFullAuto => canFullAuto;
    public bool IsOneHanded => isOneHanded;
    public bool IsOpenBolt => isOpenBolt;
    
    public float FireRate => fireRate;
    public float Accuracy => accuracy;
    public float BulletSpeed => bulletSpeed;
    //데미지 : 탄환*무기 배율
    //탄속 : 탄환 탄속 * 무기 보정
    //방어관통 : 탄환만
    //사거리 (고려x)
    //기타: 소음, 무게
    [ShowInInspector] public float RPM => 1 / FireRate * 60; //Rounds Per Minute (FireRate = 0.1 => 600 RPM)

    public override Sprite ItemSprite => itemSprite;
    public override int ItemWidth => itemWidth;
    public override int ItemHeight => itemHeight;
    public override GearType GearType => GearType.Weapon;
    public override float ItemWeight => itemWeight;
    public Vector3 MuzzlePosition => weaponSetup.MuzzleOffset;
    public Vector3 MuzzleFlashOffset => weaponSetup.MuzzleFlashOffset;
    public override bool IsStackable => false;
    public override bool IsConsumable => false;
    public override int MaxStackAmount => 1;
}
