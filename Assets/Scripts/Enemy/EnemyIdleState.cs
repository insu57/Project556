using Player;
using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    public EnemyIdleState(EnemyManager enemyManager, HumanAnimation animation) : base(enemyManager, animation) { }

    //기본 상태
    //추적 -> 기본은..?
    
    public override void EnterState()
    {
        //Animation Change...
        //애니메이션 클래스 분리? 
    }

    public override void ExitState()
    {
        
    }

    public override void UpdateState()
    {
        
    }
}
