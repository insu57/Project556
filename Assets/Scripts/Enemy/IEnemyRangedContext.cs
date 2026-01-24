using UnityEngine;

public interface IEnemyRangedContext
{
    public Transform Target { get; }
    public float TargetDist { get; }
    public bool IsFlipped { get; }
    //public Transform OneHandedMuzzle { get; }
    //public Transform TwoHandedMuzzle { get; }
}
