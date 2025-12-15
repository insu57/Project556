using Player;
using UnityEngine;

public class EnemyChaseState : EnemyBaseState
{
    public EnemyChaseState(EnemyBase enemy, HumanAnimation animation) : base(enemy, animation) { }

    //타겟으로 이동...
    
    public override void EnterState()
    {
        Debug.Log("Entering ChaseState");
    }

    public override void ExitState()
    {
        
    }

    public override void UpdateState()
    {
        
    }
}
