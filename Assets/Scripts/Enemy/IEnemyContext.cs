using UnityEngine;

public interface IEnemyContext 
{
    //EnemyBase의 하위 컴포넌트가 EnemyBase(상속 된 자식(EnemyHUmanRanged등) 포함)의 데이터에 접근이 필요 할 때 사용(커플링 최소화)
    public Transform Target { get; }
}
