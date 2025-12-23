using Player;
using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    public EnemyIdleState(EnemyBase enemy, HumanAnimation animation) : base(enemy, animation) { }

    //기본 상태
    //추적 -> 기본은..?
    
    public override void EnterState()
    {
        //Animation Change...
        //애니메이션 클래스 분리?
        Debug.Log(Enemy.name);
        
        HumanAnimation.ChangeAnimationMove(false);//정지(Idle)
    }

    public override void ExitState()
    {
        
    }

    public override void UpdateState() //순찰(이동) or 경계(시야 움직임)
    {
        
    }
}
