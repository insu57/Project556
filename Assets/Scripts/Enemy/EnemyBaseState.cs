using UnityEngine;

public abstract class EnemyBaseState
{
   protected EnemyManager _enemyManager;

   public EnemyBaseState(EnemyManager enemyManager)
   {
      _enemyManager = enemyManager;
   }
   
   public virtual void EnterState(){}
   public virtual void ExitState(){}
   public virtual void UpdateState(){}
   //(물리)?
}
