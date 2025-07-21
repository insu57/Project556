using Player;
using UnityEngine;

public class PlayerSMB : StateMachineBehaviour
{
    private PlayerControl PlayerControl { get; set; }

    public void SetPlayerControl(PlayerControl playerControl)
    {
        PlayerControl = playerControl;
    }
    
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerControl.OnReloadEnd();
    }
}
