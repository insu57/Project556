using Player;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    //PlayerManager와 공통부문 추출?(CharacterBase?)
    [SerializeField] protected EnemyData enemyData;
    [SerializeField] protected float currentHealth;
    
    //FOV Hide
    [SerializeField] protected Material stencilHideMat;
    //Detect
    [SerializeField] protected LayerMask playerLayerMask;
    [SerializeField] protected LayerMask obstacleLayerMask;
    [SerializeField] protected float detectRadius = 2f;
    [SerializeField] protected float viewDistance = 4f;
    [SerializeField] protected float viewAngle = 90f; //추후 Data에서

    public float ViewDistance => viewDistance; //사정거리(시야)

    //감지거리, 시야거리, 사정거리 -> 구분 필요
    //이동과 사격은 별도...
    
    protected bool _playerDetected = false;
    protected bool _playerInSight = false;
    protected Transform _target;

    public bool TargetDetected => _playerDetected;
    public bool TargetInSight => _playerInSight;
    public Transform Target => _target;
    //Enemy Move?
    protected bool _isFlipped = false; //
    
    protected HumanAnimation EnemyAnimation; //Animation 클래스 변경 예정(BaseAnimation를 상속)
    
    //State
    protected EnemyBaseState CurrentState;
    protected EnemyIdleState IdleState;
    protected EnemyChaseState ChaseState;
    protected EnemyAttackState AttackState;
    //적 유형에 따라 다른 타겟 감지 시 반응
    //(근접 -> 타겟 추적, 원거리 -> 사정거리 안 까지 이동 후 타겟사격)
    //타겟을 놓치거나 방해 받으면?(수류탄 등으로) 
    
    //Audio
    [SerializeField] protected AudioSource oneShotSource;
    [SerializeField] protected AudioSource loopSource;

    protected virtual void Awake()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer render in renderers)
        {
            render.material = stencilHideMat; //StencilHide Material로 교체(FOV 안에서만 Render)
        }
        
        TryGetComponent(out EnemyAnimation);
        IdleState = new EnemyIdleState(this, EnemyAnimation);
        ChaseState = new EnemyChaseState(this, EnemyAnimation);
        AttackState = new EnemyAttackState(this, EnemyAnimation);
        
    }
    
    protected virtual void Start()
    {
        currentHealth = enemyData.HealthAmount;
        
        ChangeState(IdleState);
    }

    protected virtual void Update()
    {
        CurrentState.UpdateState();
    }
    
    protected virtual void FixedUpdate()
    {
        TargetFind();
    }
    
    private void ChangeState(EnemyBaseState newState)
    {
        if(CurrentState == newState) return;
        
        CurrentState?.ExitState();
        
        CurrentState = newState;
        
        CurrentState?.EnterState();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            gameObject.SetActive(false); //임시
        }
    }
    
    private void TargetFind() //코루틴 수정? 반응 속도는 어떤 방식으로? 코루틴 딜레이? 개선점?
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
                ChangeState(ChaseState);
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
                    ChangeState(ChaseState);
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

    public void StartTargetAttack()
    {
        ChangeState(AttackState);
    }
    
    private Vector3 AngleToDirection(float angle)
    {
        //
        return new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle *  Mathf.Deg2Rad));
    }
}
