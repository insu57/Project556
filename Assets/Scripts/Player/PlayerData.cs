using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerData : MonoBehaviour, IDamageable
{
    //SO에서 기본 수치 관리...
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float sprintSpeedMultiplier = 1.5f;
    [SerializeField] private float jumpSpeed = 4f;
    public float MoveSpeed => moveSpeed;
    public float SprintSpeedMultiplier => sprintSpeedMultiplier;
    public float JumpSpeed => jumpSpeed;
    
    [SerializeField] private float playerHealth = 100f;
    [SerializeField] private float playerStamina = 100f;
    [SerializeField] private float playerEnergy = 100f;
    [SerializeField] private float playerHydration = 100f;
    [ShowInInspector]
    private readonly Dictionary<PlayerStat, (float current, float max)>  _playerStats = new();
    
    [SerializeField] private float defaultHealthPerSecond = 0f;
    [SerializeField] private float defaultStaminaPerSecond = 5f;
    [SerializeField] private float defaultEnergyPerSecond = -0.1f;
    [SerializeField] private float defaultHydrationPerSecond = -0.3f;
    [ShowInInspector]
    private readonly Dictionary<PlayerStat, float> _statsPerSecond = new();

    [SerializeField] private float defaultSprintStaminaPerSecond = -5f;
    
    private float _totalArmor;
    private float _totalWeights;

    public bool CanSprint { private set; get; }
    
    public event Action<PlayerStat, (float current, float max)> OnPlayerStatChanged;
    public event Action OnStaminaEmpty;
    
    private void Awake()
    {
        _playerStats[PlayerStat.Health] = (playerHealth, playerHealth);
        _playerStats[PlayerStat.Stamina] = (playerStamina, playerStamina);
        _playerStats[PlayerStat.Energy] = (playerEnergy, playerEnergy);
        _playerStats[PlayerStat.Hydration] = (playerHydration, playerHydration);
        
        _statsPerSecond[PlayerStat.Health] = defaultHealthPerSecond;
        _statsPerSecond[PlayerStat.Stamina] = defaultStaminaPerSecond;
        _statsPerSecond[PlayerStat.Energy] = defaultEnergyPerSecond;
        _statsPerSecond[PlayerStat.Hydration] = defaultHydrationPerSecond;

        CanSprint = true;
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
        UpdatePlayerStat(PlayerStat.Health, _statsPerSecond[PlayerStat.Health] * Time.deltaTime); //개선?
        UpdatePlayerStat(PlayerStat.Stamina, _statsPerSecond[PlayerStat.Stamina] * Time.deltaTime);
        UpdatePlayerStat(PlayerStat.Energy, _statsPerSecond[PlayerStat.Energy] *  Time.deltaTime);
        UpdatePlayerStat(PlayerStat.Hydration, _statsPerSecond[PlayerStat.Hydration] *  Time.deltaTime);
    }

    public void ItemEffectAdjustStat(StatAdjustAmount statAdjustAmount, float useDuration)
    {
        var amountPerSecond = statAdjustAmount.amount / useDuration;
        StartCoroutine(AdjustStatPerSecondCoroutine(statAdjustAmount.stat, amountPerSecond, useDuration));
    }

    public void ItemEffectAdjustStatPerSecond(StatEffectPerSecond statEffectPerSecond)
    {
        StartCoroutine(AdjustStatPerSecondCoroutine(statEffectPerSecond.stat, 
            statEffectPerSecond.amountPerSecond, statEffectPerSecond.effectDuration));
    }

    private IEnumerator AdjustStatPerSecondCoroutine(PlayerStat stat, float amountPerSecond, float duration)
    {
        float elapsedTime = 0f;
        _statsPerSecond[stat] += amountPerSecond;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _statsPerSecond[stat] -= amountPerSecond;
    }

    public void SprintStaminaSpend(bool isSprint)
    {
        if (isSprint)
        {
            _statsPerSecond[PlayerStat.Stamina] = defaultSprintStaminaPerSecond;
        }
        else
        {
            _statsPerSecond[PlayerStat.Stamina] = defaultStaminaPerSecond;
        }
        if(!CanSprint) _statsPerSecond[PlayerStat.Stamina] = defaultStaminaPerSecond;
    }
    
    private void UpdatePlayerStat(PlayerStat stat , float amount)
    {
        if(amount == 0) return;
        var playerStat = _playerStats[stat];
        playerStat.current += amount;
        if (playerStat.current > playerStat.max) playerStat.current = playerStat.max; //max보다 크다면 
        _playerStats[stat] = playerStat;
        OnPlayerStatChanged?.Invoke(stat, playerStat);
        switch (stat)
        {
            case PlayerStat.Health when _playerStats[stat].current <= 0:
                Debug.Log("Player Dead: Health");
                break;
            case PlayerStat.Stamina:
            {
                CanSprint = !(_playerStats[stat].current <= 0);
                if(!CanSprint) OnStaminaEmpty?.Invoke();
                break;
            }
            case PlayerStat.Energy:
                break;
            case PlayerStat.Hydration:
                break;
        }
    }
    
    public void UpdateTotalArmor(float armor)
    {
        _totalArmor = armor;
    }
    public void TakeDamage(float damage) //개선?
    {
        UpdatePlayerStat(PlayerStat.Health, -damage);
    }
}
