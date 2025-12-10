using Player;
using UnityEngine;

public abstract class EnemyBaseState
{
   protected EnemyManager _enemyManager;
   protected HumanAnimation _humanAnimation;//임시 -> Base로 수정예정

   public EnemyBaseState(EnemyManager enemyManager, HumanAnimation animation)
   {
      _enemyManager = enemyManager;
      _humanAnimation = animation;
   }
   
   public virtual void EnterState(){}
   public virtual void ExitState(){}
   public virtual void UpdateState(){}
   //(물리)?
}
