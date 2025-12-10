using System;
using Player;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyManager : MonoBehaviour, IDamageable //적 관리 매니저. 구현예정
{
    [SerializeField] private EnemyData enemyData;
    [ShowInInspector] private float _currentHealth;
    [SerializeField] private Material stencilHideMaterial;

    [SerializeField] private float detectRadius = 2f;
    [SerializeField] private float viewDistance = 4f;
    [SerializeField] private float viewAngle = 90f;
    
    [SerializeField] private LayerMask playerLayerMask;
    [SerializeField] private LayerMask obstacleLayerMask;
    
    private bool _isFlipped = false;
    private bool _playerDetected = false;
    private bool _playerInSight = false;
    private Transform _target;

    private HumanAnimation _enemyAnimation; //Animation 클래스 변경 예정(BaseAnimation를 상속)
    
    //private EnemyState _currentState;
    //적 무장(EnemyWeapon) - 기본 적으로 탄 소지는 무한.(장탄은 무기따라). 탄의 종류(무기 탄종에서)는 적의 등급에 따라.
    //아이템 드랍은 무작위로?(소지 무기, 탄 + 장비 + 기타 아이템)
    //공격 - 적 유형에 따라...
    private EnemyWeapon _enemyWeapon;
    [SerializeField] private SpriteRenderer oneHandWeaponSprite;
    [SerializeField] private SpriteRenderer twoHandWeaponSprite;
    [SerializeField] private Transform oneHandMuzzleTransform;
    [SerializeField] private Transform twoHandMuzzleTransform;
    
    [SerializeField] private WeaponData _testWeapon;
    [SerializeField] private AmmoData _testAmmo;
    //아이템 장착(플레이어 처럼)
    
    private EnemyBaseState _currentState;
    private EnemyIdleState _idleState;
    private EnemyChaseState _chaseState;
    
    //적 유형 별 State 구현 필요(공격적인 적 유형 등)
    
    private enum EnemyState
    {
        Idle,
        Tracking,
        Attacking,
        Dead,
    }

    private void Awake()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer render in renderers)
        {
            render.material = stencilHideMaterial; //StencilHide Material로 교체(FOV 안에서만 Render)
        }

        _idleState = new EnemyIdleState(this, _enemyAnimation);
        _chaseState = new EnemyChaseState(this, _enemyAnimation);

        TryGetComponent(out _enemyAnimation);
        TryGetComponent(out _enemyWeapon);
    }

    private void Start()
    {
        _currentHealth = enemyData.HealthAmount;

        ChangeState(_idleState);
    }

    private void Update()
    {
        _currentState.UpdateState();   
    }

    private void FixedUpdate()
    {
        TargetFind();
    }
    
    //적 캐릭터 구현
    //1. FSM기반 AI (State, 그에 따른 애니메이션, 공격(근접, 총기))
    //2. 적 장비(무장) 설정
    //3. 맵 배치, 적 시체 아이템 루팅
    
    //적-플레이어 충돌 시...? 개선필요

    private void ChangeState(EnemyBaseState newState)
    {
        _currentState?.ExitState();
        
        _currentState = newState;
        
        _currentState?.EnterState();
    }
    
    
    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    private void TargetFind() //코루틴 수정? 반응 속도는 어떤 방식으로? 코루틴 딜레이?
    {
        //
        _playerDetected = false;
        _playerInSight = false;
        _target = null;
        
        Collider2D targetInRadius = Physics2D.OverlapCircle(transform.position, viewDistance, playerLayerMask);
        //시야 범위 만큼
        
        if (targetInRadius) //감지 시
        {
            Transform target = targetInRadius.transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized; //방향
            float distToTarget = Vector3.Distance(transform.position, target.position); //타겟과의 거라
            //
            if (distToTarget < detectRadius) //감지 범위 이내
            {
                //감지 범위 내부 -> 소리 감지로 수정 예정
                //State 변경
                _playerDetected = true;
                _target = target;
                
                //temp
                ChangeState(_chaseState);
            }

            Vector2 facingDir = transform.right; //Flip에 따라 변경 필요

            if (Vector2.Angle(facingDir, dirToTarget) < viewAngle / 2) //각도 이내
            {
                if (!Physics2D.Raycast(transform.position, dirToTarget, 
                        distToTarget, obstacleLayerMask))
                {
                    _playerInSight = true;
                    _target = target;
                    
                    //temp
                    ChangeState(_chaseState);
                }
            }

            //감지 시야 -> Flip여부 확인 필요...
        }
        
    }

    private void OnDrawGizmos() //감지범위, 시야 표시
    {
        //감지 거리 -> 소리감지??(플레이어 걷기, 달리기, 사격 등 소음 발생, 소음마다 거리가 다름. 그 소리가 감지 범위 내라면 경계? 위치로 이동)
        //시야 감지 -> 사격
        
        //상태
        //기본
        //경계(감지)
        //공격(발견)
        
        //현재 감지 범위 ( 추후 소리 감지 범위로)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
        
        //시야 범위
        Gizmos.color = Color.yellow;
        Vector3 viewAngleVectorMin = AngleToDirection(-viewAngle / 2);
        Vector3 viewAngleVectorMax = AngleToDirection(viewAngle / 2);
        Vector3 viewAngleVectorMid = (viewAngleVectorMin + viewAngleVectorMax).normalized;
        
        Gizmos.DrawLine(transform.position, transform.position + viewAngleVectorMax * viewDistance);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleVectorMin * viewDistance);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleVectorMid * viewDistance);

        if (_playerDetected || _playerInSight)
        {
            Gizmos.color = Color.red;
            if (_target)
            {
                Gizmos.DrawLine(transform.position, _target.position);
            }
        }
    }

    private Vector3 AngleToDirection(float angle)
    {
        //
        return new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle *  Mathf.Deg2Rad));
    }
}
