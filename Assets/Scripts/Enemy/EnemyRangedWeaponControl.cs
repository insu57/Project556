using System;
using UnityEngine;

public class EnemyRangedWeaponControl : MonoBehaviour
{
    //Ranged Weapon
    //공격 유형?
    //적 유형별 명중률 보정 필요.
    //적 유형별 사격속도(약한적은 감소 보정)
    private IEnemyRangedContext _enemy; //EnemyBase의 데이터 접근용
    private EnemyData _enemyData;
    private WeaponData _weaponData;
    private Transform _muzzleTransform;
    private float _accuracyMultiplier;
    private float _fireRateMultiplier;
    private float _reloadMultiplier;
    private float _viewRange;
    private FireMode _fireMode;
    private int _currentMagazine;
    private float _shootAngle;
    //Sprite Transform
    [SerializeField] private Transform enemyCenter;
    [SerializeField] private Transform leftArm;
    [SerializeField] private Transform rightArm;
    private Vector3 _baseRArmPosition;
    
    public event Action<float> EnemyShoot;

    public void Init(IEnemyRangedContext enemy, EnemyData enemyData, WeaponData enemyWeaponData)
    {
        _enemy = enemy;
        _enemyData = enemyData;
        _accuracyMultiplier = enemyData.AccuracyMultiplier;
        _fireRateMultiplier = enemyData.FireRateMultiplier;
        _reloadMultiplier = enemyData.ReloadMultiplier;
        _viewRange = enemyData.ViewRange;
        
        _weaponData = enemyWeaponData;

        _fireMode = FireMode.SemiAuto;

        foreach (var fireMode in _weaponData.FireModes) //연사에 가까운 FireMode로
        {
            if (fireMode > _fireMode)
            {
                _fireMode = fireMode;
            }
        }
        
        _muzzleTransform = _weaponData.IsOneHanded ? enemy.OneHandedMuzzle : enemy.TwoHandedMuzzle; //총구 위치 초기화
        _currentMagazine = _weaponData.DefaultMagazineSize;
    }

    private void Start()
    {
        _baseRArmPosition = rightArm.transform.localPosition;
    }

    private void Update()
    {
        Attack();
        //RotateArm();
    }

    private void LateUpdate()
    {
       RotateArm();
    }

    private void Attack()
    {
        if(_currentMagazine <= 0) return; //장탄 체크
        Vector2 dir;
        if (_enemy.Target)
        {
            dir = _enemy.Target.position + new Vector3(0, 1, 0) - enemyCenter.position;
        }
        else dir = Vector2.zero;
        
        
        //RotateArm();
        
        float shootAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Debug.DrawRay(enemyCenter.position, dir, Color.red);
        //shootAngle = Mathf.Clamp(shootAngle, -60f, 60f);
        _shootAngle = shootAngle;
        
        Debug.Log(_shootAngle);
        
        switch (_fireMode)
        {
            case FireMode.SemiAuto:
                break;
            case FireMode._2Burst:
                break;
            case FireMode._3Burst:
                break;
            case FireMode.FullAuto:
            {
                //
                
                
                
                break;
            }
        }
        
        //무기에 따라...연사-(점사?)-단발(가능한 경우 앞부터)
        //발사 시 이벤트 -> EnemyManager(HumanRanged)
        
        //연사 점사 단발 처리??
        //if(_currentMagazine <= 0) return;
        //EnemyShoot?.Invoke();
        
        
    }

    private void RotateArm()
    {
        //팔 회전 추가 필요(기존 코드 이용)
        float angle = _shootAngle;
        
        
        if (_enemy.IsFlipped)
        {
            if (angle > 0)
            {
                angle = 180 - angle;
            }
            else
            {
                angle = -180 -  angle;
            }
            
        }
        _shootAngle = Mathf.Clamp(_shootAngle, -60f, 60f);
        
        
        if (_weaponData.IsOneHanded)
        {
            //angle = Mathf.Clamp(angle, -60f, 60f);
            rightArm.transform.localRotation = Quaternion.Euler(0, 0, -angle);//팔 각도 할당
        }
        else
        {
            float t = Mathf.InverseLerp(-40f, 40f, -angle); //현재 조준각도의 보간치 
            float targetAngle = Mathf.Lerp(-90, 70, t); //조준각도의 보간치에 맞추어 RightArm 회전보간

            float angleThreshold = 37f;  //해당 각도보다 크면(임계치) rightArm Position 또한 변경         
            if (targetAngle > angleThreshold)
            {
                float pt = Mathf.InverseLerp(angleThreshold, 70, targetAngle ); //임계치-최댓값에 현재 각도 보간치
                rightArm.transform.localPosition = Vector3.Lerp(_baseRArmPosition, new Vector3(-0.272f, -0.072f, 0f), pt);
                //보간치에 따라 position 벡터 계산
                //기본 (-0.161, 0.037) -> (-0.2, -0.25) -> (-0.272 -0.072) 개선?
                //무기마다 세부설정 -> Animation Curve? struct로 저장해서 불러오기
            }
            else
            {
                rightArm.transform.localPosition = _baseRArmPosition;
            }
         
            rightArm.transform.localRotation = Quaternion.Euler(0, 0, targetAngle); //팔 각도 회전
            leftArm.transform.localRotation = Quaternion.Euler(0, 0, -angle - 85f);
        }
        
        
    }
}
