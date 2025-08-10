using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : BaseItemDataSO
{
    [SerializeField] private string itemID;
    [SerializeField] private string weaponName;
    
    [SerializeField] private WeaponType weaponType; //무기 종류
    [SerializeField] private AmmoCaliber ammoCaliber;//탄 구경
    [SerializeField] private int defaultMagazineSize;//기본 탄창 용량
    [SerializeField] private AmmoData[] defaultAmmoData; //기본 탄 정보
    
    [SerializeField,Space] private bool isOneHanded; //한손무기인지
    [SerializeField] private bool isOpenBolt; //오픈볼트인지(장전시 약실에 탄이 없음)
    [SerializeField] private float fireRate; //발사속도(차탄 발사까지 걸리는 시간)
    [SerializeField] private float accuracy; //정확도
    [SerializeField] private float bulletSpeed; //Bullet에 따로?(탄속)
    [SerializeField] private FireMode[] fireModes = { //발사모드 단발/점사/연발, 기본 단발.
        FireMode.SemiAuto
    };
    [SerializeField] private WeaponActionType weaponActionType;//액션타입. 기본(자동)/펌프액션/볼트액션
    [SerializeField] private bool hasDetachableMagazine = true; //탈착가능한 탄창(기본), 내장탄창(관형탄창 등)
    
    [SerializeField] private Sprite itemSprite;
    [SerializeField] private SFX shootSFX; //발사SFX
    [SerializeField] private SFX reloadSFX; //장전SFX
    [SerializeField] private SFX loadAmmoSFX; //차탄공급 SFX(ex펌프액션)
    [Tooltip("Seconds")]
    [SerializeField] private float reloadTime; //장전시간
    
    [SerializeField] private int itemWidth;
    [SerializeField] private int itemHeight;
    [SerializeField] private float itemWeight;
    
    [SerializeField] private WeaponSetup weaponSetup; //무기 설정(총구 위치 프리팹)
    
    public override string ItemDataID => itemID;
    public override string ItemName => weaponName;
    public WeaponType WeaponType => weaponType;
    public AmmoCaliber AmmoCaliber => ammoCaliber;
    public int DefaultMagazineSize => defaultMagazineSize;
    public AmmoData[] DefaultAmmoData => defaultAmmoData;
    public SFX ShootSFX => shootSFX;
    public SFX ReloadSFX => reloadSFX;
    public SFX LoadAmmoSFX => loadAmmoSFX;
    public float ReloadTime => reloadTime;
    public FireMode[] FireModes => fireModes;
    public WeaponActionType WeaponActionType => weaponActionType;
    public bool HasDetachableMagazine => hasDetachableMagazine;
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
    public override int MaxStackAmount => 1;
}
