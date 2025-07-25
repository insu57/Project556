using Player;
using UnityEngine;

public class MoveAnimationBehaviour : StateMachineBehaviour
{
    private PlayerManager _playerManager;
    private StageManager _stageManager;
    
    private float _footstepLength;
    private float _lastFootstepTime;
    
    public void Init(PlayerManager playerManager , StageManager stageManager)
    {
        _playerManager = playerManager;
        _stageManager = stageManager;
    }
    
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var playerTilePos = _playerManager.transform.position + new Vector3(0, -0.25f, 0);
        var sfx = _stageManager.GetTileFootstepSFX(playerTilePos);
        if(sfx == SFX.None) return;
        
        _footstepLength = AudioManager.Instance.GetSFXClip(SFXType.Footstep, sfx).length;

        if (Time.time - _lastFootstepTime >= _footstepLength)
        {
            AudioManager.Instance.PlaySFX(_playerManager.OneShotSource, SFXType.Footstep, sfx);
            _lastFootstepTime = Time.time;
        }
    }
}
