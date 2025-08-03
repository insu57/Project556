using Player;
using UnityEngine;

public class MoveAnimationBehaviour : StateMachineBehaviour
{
    private PlayerManager _playerManager;
    private PlayerData _playerData;
    private StageManager _stageManager;
    
    private float _sprintSpeedMultiplier;
    private float _footstepLength;
    //private float _lastFootstepTime;
    
    public void Init(PlayerManager playerManager, PlayerData playerData , StageManager stageManager)
    {
        _playerManager = playerManager;
        _playerData = playerData;
        _stageManager = stageManager;
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(animator.GetBool(PlayerAnimatorHash.IsSprint)) 
            _sprintSpeedMultiplier = _playerData.SprintSpeedMultiplier;
        else _sprintSpeedMultiplier = 1;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var playerTilePos = _playerManager.transform.position + new Vector3(0, -0.25f, 0);
        var sfx = _stageManager.GetTileFootstepSFX(playerTilePos);
        if(sfx == SFX.None) return;
        
        _footstepLength = AudioManager.Instance.GetSFXClip(SFXType.Footstep, sfx).length;
        _footstepLength /= _sprintSpeedMultiplier;
        
        if (Time.time - _playerManager.LastFootstepTime >= _footstepLength)
        {
            AudioManager.Instance.PlaySFX(_playerManager.OneShotSource, SFXType.Footstep, sfx);
            _playerManager.LastFootstepTime = Time.time;
        }
    }
}
