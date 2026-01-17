using Player;
using UnityEngine;

public class MoveAnimationBehaviour : StateMachineBehaviour //이동 애니메이션(move, sprint) state SMB
{
    private IHumanType _humanType;
    private StageManager _stageManager;
    
    private float _sprintSpeedMultiplier; //달리기 속도 배수
    private float _footstepLength; //발소리SFX 길이
    
    public void Init(IHumanType humanType, StageManager stageManager) //초기화
    {
        _humanType = humanType;
        _stageManager = stageManager;
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(animator.GetBool(HumanAnimatorHash.IsSprint))  //달리기 배수 할당
            _sprintSpeedMultiplier = _humanType.GetSprintSpeedMultiplier();
        else _sprintSpeedMultiplier = 1;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var playerTilePos = _humanType.GetPosition() + new Vector3(0, -0.5f, 0);
        var sfx = _stageManager.GetTileFootstepSFX(playerTilePos); //위치(타일)에 따른 발소리 SFX
        if(sfx == SFX.None) return; //공중 등 발소리가 없으면 return
        
        _footstepLength = AudioManager.Instance.GetSFXClip(SFXType.Footstep, sfx).length; //발소리 길이
        _footstepLength /= _sprintSpeedMultiplier; //달리기 배수 만큼 빠르게
        
        if (Time.time - _humanType.LastFootstepTime >= _footstepLength) //발소리 재생 시간을 넘기면 다시 재생
        {
            AudioManager.Instance.PlaySFX(_humanType.OneShotSource, SFXType.Footstep, sfx);
            _humanType.LastFootstepTime = Time.time; //마지막 발소리
        }
    }
}
