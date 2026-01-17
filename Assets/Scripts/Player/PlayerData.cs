using System;
using System.Collections;
using System.Collections.Generic;
using Item;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerData : MonoBehaviour, IDamageable
{
    //외부(json 등) -> SO로 수정 필요. 
    [SerializeField] private float moveSpeed = 3f; //이동 속도
    [SerializeField] private float sprintSpeedMultiplier = 1.5f; //달리기 배수
    [SerializeField] private float jumpForce = 4f; //점프 운동량
    public float MoveSpeed => moveSpeed;
    public float SprintSpeedMultiplier => sprintSpeedMultiplier;
    public float JumpForce => jumpForce;
    
    [SerializeField] private float playerHealth = 100f; //플레이어 스탯 수치
    [SerializeField] private float playerStamina = 100f;
    [SerializeField] private float playerEnergy = 100f;
    [SerializeField] private float playerHydration = 100f;
    [ShowInInspector]
    private readonly Dictionary<PlayerStat, (float current, float max)>  _playerStats = new(); //Stat Dictionary
    
    [SerializeField] private float defaultHealthPerSecond = 0f; //초당 스탯 변화(기본)
    [SerializeField] private float defaultStaminaPerSecond = 5f;
    [SerializeField] private float defaultEnergyPerSecond = -0.1f;
    [SerializeField] private float defaultHydrationPerSecond = -0.3f;
    [ShowInInspector]
    private readonly Dictionary<PlayerStat, float> _statsPerSecond = new(); //초당 스탯 변화 Dictionary

    [SerializeField] private float defaultSprintStaminaPerSecond = -5f; //기본 달리기 스태미나 소모량
    
    private float _totalArmor;
    private float _totalWeights;

    public bool CanSprint { get; private set; } //스태미나 없으면 달리기 불가

    public event Action<PlayerStat, (float current, float max)> OnPlayerStatChanged; //플레이어 스탯 변화 이벤트(UI)
    public event Action OnStaminaEmpty; //스태미나 없을때
    
    private void Awake()
    {
        _playerStats[PlayerStat.Health] = (playerHealth, playerHealth); //플레이어 스탯 초기화
        _playerStats[PlayerStat.Stamina] = (playerStamina, playerStamina);
        _playerStats[PlayerStat.Energy] = (playerEnergy, playerEnergy);
        _playerStats[PlayerStat.Hydration] = (playerHydration, playerHydration);
        
        _statsPerSecond[PlayerStat.Health] = defaultHealthPerSecond; //스탯 초당 변화량 초기화
        _statsPerSecond[PlayerStat.Stamina] = defaultStaminaPerSecond;
        _statsPerSecond[PlayerStat.Energy] = defaultEnergyPerSecond;
        _statsPerSecond[PlayerStat.Hydration] = defaultHydrationPerSecond;

        CanSprint = true;
    }

    private void Start()
    {
        foreach (var (stat, amount) in _playerStats) //스탯UI 초기화
        {
            OnPlayerStatChanged?.Invoke(stat, amount);
        }
    }

    private void Update()
    {
        //초당 스탯 변화
        UpdatePlayerStat(PlayerStat.Health, _statsPerSecond[PlayerStat.Health] * Time.deltaTime); 
        UpdatePlayerStat(PlayerStat.Stamina, _statsPerSecond[PlayerStat.Stamina] * Time.deltaTime);
        UpdatePlayerStat(PlayerStat.Energy, _statsPerSecond[PlayerStat.Energy] *  Time.deltaTime);
        UpdatePlayerStat(PlayerStat.Hydration, _statsPerSecond[PlayerStat.Hydration] *  Time.deltaTime);
    }

    public void ItemEffectAdjustStat(StatAdjustAmount statAdjustAmount, float useDuration) 
    {
        //아이템 효과 적용(사용 시간에 나누어 초당 변화)
        var amountPerSecond = statAdjustAmount.amount / useDuration;
        StartCoroutine(AdjustStatPerSecondCoroutine(statAdjustAmount.stat, amountPerSecond, useDuration));
    }

    public void ItemEffectAdjustStatPerSecond(StatEffectPerSecond statEffectPerSecond)
    {
        //효과 시간 동안 초당 스탯 변화량 조절
        StartCoroutine(AdjustStatPerSecondCoroutine(statEffectPerSecond.stat, 
            statEffectPerSecond.amountPerSecond, statEffectPerSecond.effectDuration));
    }

    private IEnumerator AdjustStatPerSecondCoroutine(PlayerStat stat, float amountPerSecond, float duration)
    {
        //스탯 변화 코루틴
        float elapsedTime = 0f;
        _statsPerSecond[stat] += amountPerSecond; //초당 변화량
        while (elapsedTime < duration) //지속 시간 동안
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _statsPerSecond[stat] -= amountPerSecond; //지속 시간이 끝나면 원래대로
    }

    public void SprintStaminaSpend(bool isSprint) //달리기 스태미나 소모
    {
        if (isSprint) //달리기 시 스태미나 소모
        {
            _statsPerSecond[PlayerStat.Stamina] = defaultSprintStaminaPerSecond;
        }
        else //아니라면 스태미나 회복
        {
            _statsPerSecond[PlayerStat.Stamina] = defaultStaminaPerSecond;
        }
    }
    
    private void UpdatePlayerStat(PlayerStat stat , float amount) //스탯 변화량 업데이트
    {
        if(amount == 0) return; //변화량 0이면 return
        
        var playerStat = _playerStats[stat];
        playerStat.current += amount;
        if (playerStat.current > playerStat.max) playerStat.current = playerStat.max; //max 넘지 못하게 제한
        _playerStats[stat] = playerStat;
        OnPlayerStatChanged?.Invoke(stat, playerStat); //UI업데이트 이벤트
        
        switch (stat)
        {
            case PlayerStat.Health when _playerStats[stat].current <= 0:
                Debug.Log("Player Dead: Health"); //사망처리
                break;
            case PlayerStat.Stamina:
            {
                CanSprint = _playerStats[stat].current > 0; 
                if(!CanSprint) //스태미나 고갈 시 달리기 제한
                {
                    OnStaminaEmpty?.Invoke(); //스태미나 고갈 이벤트
                    SprintStaminaSpend(false);//달리기 불가 시 회복으로
                }
                break;
            }
            case PlayerStat.Energy:
                break;
            case PlayerStat.Hydration:
                break;
        }
    }
    
    public void UpdateTotalArmor(float armor) //총 방어도 업데이트
    {
        _totalArmor = armor;
    }
    public void TakeDamage(float damage) //데미지 받을 때
    {
        UpdatePlayerStat(PlayerStat.Health, -damage);
    }
}
