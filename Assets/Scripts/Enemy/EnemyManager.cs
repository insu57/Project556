using System;
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
    
    private EnemyState _currentState;

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
    }

    private void Start()
    {
        _currentHealth = enemyData.HealthAmount;

        _currentState = EnemyState.Idle;
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
    private void EnterState(EnemyState newState)
    {
        ExitState();
        _currentState = newState;
        //현재 State 변경
    }
    
    //현재 State 처리..?

    private void ExitState()
    {
        //현재 State를 벗어 날 때
    }
    
    
    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    private void TargetFind()
    {
        //
        
        Collider2D targetInRadius = Physics2D.OverlapCircle(transform.position, detectRadius, playerLayerMask);
        
        //Target(Player)
        if (targetInRadius)
        {
            Transform target = targetInRadius.transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, target.position); //타겟과의 거라
            //
            if (distanceToTarget < detectRadius)
            {
                //감지 범위 내부 -> 소리 감지로 수정 예정
            }
            
            //감지 시야 -> Flip여부 확인 필요...
        }
        
    }

    private void OnDrawGizmos()
    {
        //감지 거리 -> 소리감지??(플레이어 걷기, 달리기, 사격 등 소음 발생, 소음마다 거리가 다름. 그 소리가 감지 범위 내라면 경계? 위치로 이동)
        //시야 감지 -> 사격
        
        //상태
        //기본
        //경계(감지)
        //공격(발견)
        
        //현재 감지 범위 ( 추후 소리 감지 범위로)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}
