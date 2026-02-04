using UnityEngine;

public interface IEnemyRangedContext
{
    public Transform Target { get; }
    public float TargetDist { get; }
    public bool IsFlipped { get; }
    public bool TargetInSight { get; }
    public bool TargetDetected { get; }
}
