using Player;
using UnityEngine;

public class ReloadAnimationBehaviour : StateMachineBehaviour
{
    //private static readonly int IsReload = Animator.StringToHash("isReload");
    private PlayerControl _playerControl;
    private PlayerManager _playerManager;
    private const float DefaultAnimationSpeed = 1f;
    
    public void Init(PlayerControl playerControl, PlayerManager playerManager )
    {
        _playerControl = playerControl;
        _playerManager = playerManager;
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //장전진입
        var reloadTime = _playerManager.PlayReloadSFX();
        var animationLength = stateInfo.length;
        var currentAnimationSpeed = animationLength / reloadTime;
        animator.speed = currentAnimationSpeed;
    }
    
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _playerControl.OnReloadEnd();
        animator.speed = DefaultAnimationSpeed;
        animator.SetBool(PlayerAnimatorHash.IsReload, false);
    }
}
