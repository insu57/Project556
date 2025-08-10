using System;
using UnityEngine;

namespace Item
{
    public interface IConsumableItem //소비아이템 인터페이스
    {
        public StatAdjustAmount[] AdjustAmount { get; } //상태이상은?
        public StatEffectPerSecond[] EffectPerSecond { get; }
        [Tooltip("Seconds")] public float UseDuration { get; }
        //ItemEffect...(출혈회복 등(이상상태 회복), 이상상태(취함 등),
        //체력, 스태미나 등 회복량(스태미나, 에너지, 수분 -> 증감량에 더하기, 체력 등 -> 추가로
        //효과 중복 방지 추가 필요.
    }

    [Serializable]
    public struct StatAdjustAmount //아이템 효과(증감량, 아이템 사용시간, 초당 증감량/사용시간)
    {
        public PlayerStat stat;
        public float amount;
    }

    [Serializable]
    public struct StatEffectPerSecond //아이템 효과(초당 효과, 지속 시간)
    {
        public PlayerStat stat;
        public float amountPerSecond;
        public float effectDuration;
    }
}