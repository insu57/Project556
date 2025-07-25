using System;
using UnityEngine;

public class PlayerData : MonoBehaviour, IDamageable
{
    //PlayerManager에서 이동...
    [SerializeField] private float playerHealth = 100f;
    [SerializeField] private float playerStamina = 100f;
    [SerializeField] private float playerEnergy = 100f;
    [SerializeField] private float playerHydration = 100f;
    private float _currentHealth;
    private float _currentStamina;
    private float _currentEnergy;
    private float _currentHydration;

    private float _totalArmor;
    private float _totalWeights;

    public event Action<float, float> OnPlayerHealthChanged;
    
    private void Awake()
    {
        _currentHealth = playerHealth;
        _currentStamina = playerStamina;
        
        OnPlayerHealthChanged?.Invoke(_currentHealth, playerHealth);
    }
    
    public void UpdateTotalArmor(float armor)
    {
        _totalArmor = armor;
    }
    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        OnPlayerHealthChanged?.Invoke(_currentHealth, playerHealth);
        if (_currentHealth <= 0)
        {
            Debug.Log("Player Dead: Health");
        }
    }
}
