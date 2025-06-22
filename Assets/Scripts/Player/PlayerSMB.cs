using Player;
using UnityEngine;

public class PlayerSMB : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerControl playerControl = animator.GetComponentInParent<PlayerControl>();
        if (playerControl)
        {
            playerControl.OnReloadEnd();
        }
    }
}
