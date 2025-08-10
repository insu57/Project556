using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class EnemyManager : MonoBehaviour, IDamageable //적 관리 매니저. 구현예정
{
    [SerializeField] private EnemyData enemyData; 
    [ShowInInspector] private float _currentHealth;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        TryGetComponent(out _spriteRenderer);
        _currentHealth = enemyData.HealthAmount;
    }
    
    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
    }
}
