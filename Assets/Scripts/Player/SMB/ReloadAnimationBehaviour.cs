using Player;
using UnityEngine;

public class ReloadAnimationBehaviour : StateMachineBehaviour //장전 애니메이션 SMB
{
    private PlayerControl _playerControl;
    private PlayerManager _playerManager;
    private const float DefaultAnimationSpeed = 1f; //기본 장전 애니메이션 속도
    
    public void Init(PlayerControl playerControl, PlayerManager playerManager ) //초기화
    {
        _playerControl = playerControl;
        _playerManager = playerManager;
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //장전진입
        var reloadTime = _playerManager.PlayReloadSFX(); //장전SFX 재생 및 장전시간 조회
        var animationLength = stateInfo.length;
        var currentAnimationSpeed = animationLength / reloadTime; //장전시간에 따라 조절
        animator.speed = currentAnimationSpeed;
    }
    
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //장전 끝날 때
        if (_playerManager.CheckWeaponHasNotDetachMag())//내장탄창(관형탄창 등) 
        {
            _playerControl.OnReloadOneRoundEnd(); //한발씩 장전
        }
        else
        {
            _playerControl.OnReloadEnd(); //장전 완료 처리
        }
        animator.speed = DefaultAnimationSpeed; //속도 원래대로
    }
}
