using Player;
using UnityEngine;

public class EnemyChaseState : EnemyBaseState
{
    public EnemyChaseState(EnemyBase enemy, HumanAnimation animation) : base(enemy, animation) { }

    //타겟으로 이동...
    
    public override void EnterState()
    {
        Debug.Log("Entering ChaseState");

        if (Enemy.TargetInSight) Enemy.StartTargetAttack();
    }

    public override void ExitState()
    {
        
    }

    public override void UpdateState() //사정거리(혹은 공격 범위 이내)까지 이동 후 사격, 근접이라면 근접 거리까지
    {
        //ViewDistance 이내 까지 이동...
        if (Enemy.TargetInSight) return;
        HumanAnimation.ChangeAnimationMove(true);
    }
}
