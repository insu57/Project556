using Player;
using UnityEngine;
using UnityEngine.Animations;

public class LoadAmmoAnimationBehaviour : StateMachineBehaviour
{
    private const float DefaultSpeed = 1f;
    private PlayerManager _playerManager;
    
    public void Init(PlayerManager playerManager)
    {
        _playerManager = playerManager;
    }
    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var fireRate = _playerManager.PlayLoadAmmoSFX();
        var animationLength = stateInfo.length;
        var speed = animationLength/fireRate;
        animator.speed = speed;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.speed = DefaultSpeed;
    }
}
