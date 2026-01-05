using Player;
using UnityEngine;

public abstract class EnemyBaseState
{
   protected EnemyManagerBase EnemyManager;
   protected HumanAnimation HumanAnimation;//임시 -> Base로 수정예정

   protected EnemyBaseState(EnemyManagerBase enemyManager, HumanAnimation animation)
   {
      EnemyManager = enemyManager;
      HumanAnimation = animation;
   }
   
   public virtual void EnterState(){}
   public virtual void ExitState(){}
   public virtual void UpdateState(){}
   //(물리)?
}
