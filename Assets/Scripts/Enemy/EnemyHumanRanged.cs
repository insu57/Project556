using System;
using Player;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyHumanRanged : EnemyBase
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
    
    [Header("WeaponSprite")] [Space]
    [SerializeField] private SpriteRenderer oneHandWeaponSprite;
    [SerializeField] private SpriteRenderer twoHandWeaponSprite;
    [SerializeField] private Transform oneHandMuzzleTransform;
    [SerializeField] private Transform twoHandMuzzleTransform;
    
    [SerializeField] private GameObject muzzleFlashVFX; //총구화염VFX -> 무기(+파츠)마다 다르게?
    
    [SerializeField] private WeaponData _testWeapon;
    [SerializeField] private AmmoData _testAmmo;
    //아이템 장착(플레이어 처럼)

    protected override void Awake()
    {
        base.Awake();

        TryGetComponent(out _enemyWeapon);
    }

    protected override void Start()
    {
        base.Start();
        //적 기본무기(EnemyData에서 가져오도록 수정 예정)
        SetWeapon();
    }

    private void SetWeapon()
    {
        _enemyWeapon.ChangeWeaponData(_testWeapon, _testAmmo);
        
        var weaponData = _testWeapon;
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

    
    
    //적 캐릭터 구현
    //1. FSM기반 AI (State, 그에 따른 애니메이션, 공격(근접, 총기))
    //2. 적 장비(무장) 설정
    //3. 맵 배치, 적 시체 아이템 루팅
    
    //적-플레이어 충돌 시...? 개선필요
}
