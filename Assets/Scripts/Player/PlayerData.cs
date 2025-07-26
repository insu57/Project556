using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour, IDamageable
{
    //SO에서 기본 수치 관리...
    [SerializeField] private float playerHealth = 100f;
    [SerializeField] private float playerStamina = 100f;
    [SerializeField] private float playerEnergy = 100f;
    [SerializeField] private float playerHydration = 100f;

    private readonly Dictionary<PlayerStat, (float current, float max)>  _playerStats = new();
    
    [SerializeField] private float energyPerSecond = 0.1f;
    [SerializeField] private float hydrationPerSecond = 0.3f;
    

    private float _totalArmor;
    private float _totalWeights;
    
    public event Action<PlayerStat, (float current, float max)> OnPlayerStatChanged;
    
    private void Awake()
    {
        _playerStats[PlayerStat.Health] = (playerHealth, playerHealth);
        _playerStats[PlayerStat.Stamina] = (playerStamina, playerStamina);
        _playerStats[PlayerStat.Energy] = (playerEnergy, playerEnergy);
        _playerStats[PlayerStat.Hydration] = (playerHydration, playerHydration);
    }

    private void Start()
    {
        foreach (var (stat, amount) in _playerStats)
        {
            OnPlayerStatChanged?.Invoke(stat, amount);
        }
    }

    private void Update()
    {
        UpdatePlayerStat(PlayerStat.Energy, -energyPerSecond *  Time.deltaTime);
        UpdatePlayerStat(PlayerStat.Hydration, -hydrationPerSecond *  Time.deltaTime);
    }

    private void UpdatePlayerStat(PlayerStat stat , float amount)
    {
        var playerStat = _playerStats[stat];
        playerStat.current += amount;
        _playerStats[stat] = playerStat;
        OnPlayerStatChanged?.Invoke(stat, playerStat);
    }
    
    public void UpdateTotalArmor(float armor)
    {
        _totalArmor = armor;
    }
    public void TakeDamage(float damage) //개선?
    {
        UpdatePlayerStat(PlayerStat.Health, -damage);
        var currentHealth = _playerStats[PlayerStat.Health].current;
        if (currentHealth <= 0)
        {
            Debug.Log("Player Dead: Health");
        }
    }
}
