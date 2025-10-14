using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class EnemyManager : MonoBehaviour, IDamageable //적 관리 매니저. 구현예정
{
    [SerializeField] private EnemyData enemyData; 
    [ShowInInspector] private float _currentHealth;
    private SpriteRenderer _spriteRenderer;
    
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
        TryGetComponent(out _spriteRenderer);
        _currentHealth = enemyData.HealthAmount;

        _currentState = EnemyState.Idle;
    }
    
    //적 캐릭터 구현
    //1. FSM기반 AI (State, 그에 따른 애니메이션, 공격(근접, 총기))
    //2. 적 장비(무장) 설정
    //3. 맵 배치, 적 시체 아이템 루팅
    
    
    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            gameObject.SetActive(false);
        }
    }
    
    
}
