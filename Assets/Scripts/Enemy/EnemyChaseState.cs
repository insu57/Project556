using Player;
using UnityEngine;

public class EnemyChaseState : EnemyBaseState
{
    public EnemyChaseState(EnemyManager enemyManager, HumanAnimation animation) : base(enemyManager, animation) { }

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
