using Player;
using UnityEngine;

public class MoveAnimationBehaviour : StateMachineBehaviour
{
    private PlayerManager _playerManager;
    private StageManager _stageManager;
    
    public void Init(PlayerManager playerManager , StageManager stageManager)
    {
        _playerManager = playerManager;
        _stageManager = stageManager;
    }
    
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var playerPos = animator.transform.position;
        var sfx = _stageManager.GetTileFootstepSFX(playerPos);
        Debug.Log(sfx);
        //
    }
}
