using System;
using UnityEngine;

public interface IConsumableItem
{
    public StatAdjustAmount[] AdjustAmount { get; } //상태이상은?
    public StatEffectPerSecond[] EffectPerSecond { get; }
    [Tooltip("Seconds")] public float UseDuration { get; }
    //ItemEffect...(출혈회복 등(이상상태 회복), 이상상태(취함 등), 체력, 스태미나 등 회복량(스태미나, 에너지, 수분 -> 증감량에 더하기, 체력 -> 추가로 
}

[Serializable]
public struct StatAdjustAmount
{
    public PlayerStat stat;
    public float amount;
}

[Serializable]
public struct StatEffectPerSecond
{
    public PlayerStat stat;
    public float amountPerSecond;
    public float effectDuration;
}