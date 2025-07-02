using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class EnemyManager : MonoBehaviour, IDamageable
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
