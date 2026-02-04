using System;
using Player;
using UnityEngine;

public abstract class EnemyManagerBase : MonoBehaviour, IDamageable, IEnemyContext
{
    [SerializeField] protected EnemyData enemyData;
    [SerializeField] protected float currentHealth;
    [SerializeField] protected GameObject enemySprite;
    
    //FOV Hide
    [SerializeField] protected Material stencilHideMat;
    //Detect
    [SerializeField] protected LayerMask playerLayerMask;
    [SerializeField] protected LayerMask obstacleLayerMask;
    [SerializeField] protected LayerMask groundLayerMask;

    public float DetectRadius { get; private set; }
    public float ViewDistance { get; private set; }
    public float ViewAngle { get; private set; }
    public float ChaseRange { get; private set; }
    
    //[SerializeField] private float _detectionDelay = 0.5f;
    private float _detectionDelay;
    private float _detectionTimer = 0f;

    //감지거리, 시야거리, 사정거리 -> 구분 필요
    //이동과 사격은 별도...
    
    protected bool _targetDetected = false;
    protected bool _targetInSight = false;
    protected Transform _target;

    public bool TargetDetected => _targetDetected;
    public bool TargetInSight => _targetInSight;
    public Transform Target => _target;
    public float TargetDist { private set; get; }

    protected EnemyMoveControl EnemyMoveControl;
    //Enemy Move?
    
    protected bool _isFlipped; //
    public bool IsFlipped => _isFlipped;
    
    protected HumanAnimation EnemyAnimation; //Animation 클래스 변경 예정(BaseAnimation를 상속) -> 공용으로 수정 필요.
    
    //State
    protected EnemyBaseState CurrentState;
    protected EnemyIdleState IdleState;
    protected EnemyChaseState ChaseState;
   
    //타겟을 놓치거나(시야 밖으로 이동, 방해물 가려지는 등 시야에서 사라질 시), 방해 받으면?(수류탄 등으로) 
    
    //감지 or 시야범위 이내 -> 추적(추적거리 이내까지 이동)이동하며 공격?(탄 전부 쓰면 장전) 이동/공격은 별도로! 
    
    
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
        //AttackState = new EnemyAttackState(this, EnemyAnimation);

        TryGetComponent(out EnemyMoveControl);
    }
    
    protected virtual void Start()
    {
        currentHealth = enemyData.HealthAmount;
        
        ChangeState(IdleState);

        DetectRadius = enemyData.DetectRange;
        ViewDistance = enemyData.ViewRange;
        ViewAngle = enemyData.ViewAngle;
        ChaseRange = enemyData.ChaseRange;
        _detectionDelay = enemyData.DetectionDelay;

        EnemyMoveControl.Init(enemyData.MoveSpeed, this, groundLayerMask);
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
        Debug.Log("TakeDamage");
        
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            //gameObject.SetActive(false); //임시
        }
    }
    
    private void TargetFind()
    {
        //감지 방식 개선 필요.

        Collider2D targetInRadius = Physics2D.OverlapCircle(transform.position, ViewDistance, playerLayerMask);
        
        //if(!_targetInSight) ResetDetection();

        if (!targetInRadius)
        {
            ResetDetection();
            return;
        }
        
        Transform target = targetInRadius.transform;
        Vector3 dirToTarget = (target.position - transform.position).normalized; //방향
        float distToTarget = Vector3.Distance(transform.position, target.position); //타겟과의 거라
        
        if (!_targetInSight)
        {
            if (!(distToTarget < DetectRadius)) return; 
            //감지 범위 이내
            //감지 범위 내부 -> 소리 감지로 수정 예정
            //_playerDetected = true;
            _target = target;
                
            _isFlipped = target.position.x < transform.position.x;//기본 방향(오른쪽)이 아니라 왼쪽이라면 Flip
            float xScale = enemySprite.transform.localScale.x;
            if (_isFlipped) xScale = Mathf.Abs(xScale) * -1;
            else xScale =  Mathf.Abs(xScale) * 1;
            enemySprite.transform.localScale = new Vector3(xScale, 
                enemySprite.transform.localScale.y, enemySprite.transform.localScale.z);
        }
        
        Vector2 facingDir = transform.right; //Flip에 따라 변경 필요
        if(_isFlipped) facingDir = -facingDir;

        if (Vector2.Angle(facingDir, dirToTarget) < ViewAngle / 2) //각도 이내
        {
            if (!Physics2D.Raycast(transform.position, dirToTarget, 
                    distToTarget, obstacleLayerMask))
            {
                _targetInSight = true;
                _target = target;
                
                _detectionTimer += Time.fixedDeltaTime;
                ChangeState(ChaseState);
                
                if(_detectionTimer < _detectionDelay) return;

                _targetDetected = true;
                TargetDist = Vector3.Distance(transform.position, target.position);
                
                
                //ChangeState(ChaseState);
            }
        }
        
        
        //수정 필요...
        //시야거리 >= 감지범위 제한 
        //감지 먼저 -> 시야 회전(반대이므로)
        //시야 감지 -> 감지시간시작...
        //1.시야 방향에 바로 감지시간으로
        //2.반대 -> 회전 후 감지시간
    }

    private void ResetDetection()
    {
        // _detectionTimer = 0f;
        _detectionTimer -= Time.fixedDeltaTime;
        if (_detectionTimer <= 0f) _detectionTimer = 0f;
        _targetDetected = false;
        _targetInSight = false;
            
        _target = null;
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
        Gizmos.DrawWireSphere(transform.position, DetectRadius);
        
        //시야 범위
        Gizmos.color = Color.yellow;
        float angle = ViewAngle;
        if(_isFlipped) angle = 180 - ViewAngle;
        Vector3 viewAngleVectorMin = AngleToDirection(-angle / 2);
        Vector3 viewAngleVectorMax = AngleToDirection(angle / 2);
        Vector3 viewAngleVectorMid = (viewAngleVectorMin + viewAngleVectorMax).normalized;
        float dist = ViewDistance;
        if(_isFlipped) dist = -ViewDistance;
        Gizmos.DrawLine(transform.position, transform.position + viewAngleVectorMax * dist);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleVectorMin * dist);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleVectorMid * dist);

        if (_targetDetected || _targetInSight)
        {
            Gizmos.color = Color.red;
            if (_target)
            {
                Gizmos.DrawLine(transform.position, _target.position);
            }
        }
    }

    public void StartChase(bool inChase) //추적 - 추적알고리즘 구현 필요
    {
        if(!_target) return;
        
        EnemyMoveControl.StartChase(inChase);
    }
    
    private Vector3 AngleToDirection(float angle)
    {
        return new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle *  Mathf.Deg2Rad));
    }
}
