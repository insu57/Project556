using System;
using Player;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyManagerHumanRanged : EnemyManagerBase, IHumanType, IEnemyRangedContext
{
    //적 - 원거리(기본 : 인간형 사격, 무한잔탄) / 근접(인간형, 괴물형)
    //인간형, 적 -> 근거리, 원거리로?
    //공통 : FOV Hide 처리, 체력
    //분리필요 : 무기관련(사격)
    
    //private EnemyState _currentState;
    //적 무장(EnemyWeapon) - 기본 적으로 탄 소지는 무한.(장탄은 무기따라). 탄의 종류(무기 탄종에서)는 적의 등급에 따라.
    //아이템 드랍은 무작위로?(소지 무기, 탄 + 장비 + 기타 아이템)
    //공격 - 적 유형에 따라...
    private CharacterWeapon _enemyWeapon;
    private EnemyRangedWeaponControl _rangedWeaponControl;
    
    [Header("WeaponSprite")] [Space]
    [SerializeField] private SpriteRenderer oneHandWeaponSprite;
    [SerializeField] private SpriteRenderer twoHandWeaponSprite;
    [SerializeField] private Transform oneHandMuzzleTransform;
    [SerializeField] private Transform twoHandMuzzleTransform;
    public Transform OneHandedMuzzle => oneHandMuzzleTransform;
    public Transform TwoHandedMuzzle => twoHandMuzzleTransform;
    
    [SerializeField] private GameObject muzzleFlashVFX; //총구화염VFX -> 무기(+파츠)마다 다르게?
    //[SerializeField] private AudioSource oneShotSource;
    public AudioSource OneShotSource => oneShotSource;
    public float LastFootstepTime { set; get;}

    private WeaponData _currentWeaponData;
    
    //임시
    [SerializeField] private WeaponData _testWeapon;
    [SerializeField] private AmmoData _testAmmo;

    protected override void Awake()
    {
        base.Awake();

        TryGetComponent(out _enemyWeapon);
        TryGetComponent(out _rangedWeaponControl);
    }

    protected override void Start()
    {
        base.Start();
        //적 기본무기(EnemyData에서 가져오도록 수정 예정)
        SetWeapon();
        
        //HumanAnimation
        var stageManager = FindAnyObjectByType<StageManager>(); //개선필요
        var reloadAnimationBehaviour = EnemyAnimation.UpperAnimator.GetBehaviour<ReloadAnimationBehaviour>();
        reloadAnimationBehaviour.Init(this);
        var sprintAnimationBehaviours =  
            EnemyAnimation.LowerAnimator.GetBehaviours<MoveAnimationBehaviour>();
        foreach (var behaviour in sprintAnimationBehaviours)
        {
            behaviour.Init(this, stageManager);
        }
        var loadAmmoAnimationBehaviours = EnemyAnimation.UpperAnimator.GetBehaviours<LoadAmmoAnimationBehaviour>();
        foreach (var behaviour in loadAmmoAnimationBehaviours)
        {
            behaviour.Init(this);
        }
    }

    private void OnEnable()
    {
        _rangedWeaponControl.EnemyShoot += Shoot;
    }

    private void OnDisable()
    {
        _rangedWeaponControl.EnemyShoot -= Shoot;
    }

    private void SetWeapon()
    {
        //수정 필요(EnemyData 무기 리스트에서 무작위로 뽑기)
        _enemyWeapon.ChangeWeaponData(_testWeapon, _testAmmo);
        _enemyWeapon.SetCharacterMultiplier(enemyData.AccuracyMultiplier, enemyData.FireRateMultiplier);
        _rangedWeaponControl.Init(this , enemyData, _testWeapon);
        
        var weaponData = _testWeapon;
        _currentWeaponData = _testWeapon;
        var weaponType = _testWeapon.WeaponType;
        
        EnemyAnimation.ChangeWeapon(weaponType);

        if (weaponType is WeaponType.Pistol) //한손
        {
            oneHandWeaponSprite.sprite = weaponData.ItemSprite; //스프라이트 위치
            oneHandWeaponSprite.enabled = true;
            twoHandWeaponSprite.enabled = false;
            oneHandMuzzleTransform.localPosition = weaponData.MuzzlePosition;
            _enemyWeapon.SetMuzzleTransform(oneHandMuzzleTransform); //총구위치 설정
            
            muzzleFlashVFX.transform.SetParent(oneHandMuzzleTransform);
        }
        else //양손무기
        {
            twoHandWeaponSprite.sprite = weaponData.ItemSprite;
            twoHandWeaponSprite.enabled = true;
            oneHandWeaponSprite.enabled = false;
            twoHandMuzzleTransform.localPosition = weaponData.MuzzlePosition;
            _enemyWeapon.SetMuzzleTransform(twoHandMuzzleTransform);
            
            muzzleFlashVFX.transform.SetParent(twoHandMuzzleTransform);
        }
        muzzleFlashVFX.transform.localRotation = Quaternion.identity;
        muzzleFlashVFX.transform.localPosition = weaponData.MuzzleFlashOffset;
    }

    private void Shoot(float angle)
    {
        //CharacterWeapon
        
        _enemyWeapon.Shoot(_isFlipped, angle);
    }
    
    public float PlayReloadSFX()
    {
        AudioManager.Instance.PlaySFX(oneShotSource,SFXType.Weapon, _currentWeaponData.ReloadSFX);
        return _currentWeaponData.ReloadTime;
    }

    public bool CheckWeaponHasNotDetachMag()
    {
        return !_currentWeaponData.HasDetachableMagazine;
    }

    public void OnReloadOneRoundEnd() //관형탄창 장전(한발 씩 장전)
    {
        //적 장전, 공격(무기)제어 추가 필요
    }

    public void OnReloadEnd()
    {
        //
    }

    public float GetSprintSpeedMultiplier()
    {
        return 1.2f; //임시 EnemyData에서
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public float PlayLoadAmmoSFX()
    {
        AudioManager.Instance.PlaySFX(oneShotSource, SFXType.Weapon, _currentWeaponData.LoadAmmoSFX);
        return _currentWeaponData.TimeBetweenShot;
    }

    //적 캐릭터 구현
    //1. FSM기반 AI (State, 그에 따른 애니메이션, 공격(근접, 총기))
    //2. 적 장비(무장) 설정
    //3. 맵 배치, 적 시체 아이템 루팅
    
    //적-플레이어 충돌 시...? 개선필요
}
