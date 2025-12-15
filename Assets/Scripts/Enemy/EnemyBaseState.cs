using Player;
using UnityEngine;

public abstract class EnemyBaseState
{
   protected EnemyBase Enemy;
   protected HumanAnimation HumanAnimation;//임시 -> Base로 수정예정

   protected EnemyBaseState(EnemyBase enemy, HumanAnimation animation)
   {
      Enemy = enemy;
      HumanAnimation = animation;
   }
   
   public virtual void EnterState(){}
   public virtual void ExitState(){}
   public virtual void UpdateState(){}
   //(물리)?
}
