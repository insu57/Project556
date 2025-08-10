using Player;
using UnityEngine;
using UnityEngine.Animations;

public class LoadAmmoAnimationBehaviour : StateMachineBehaviour //차탄 공급 애니메이션 State(펌프액션, 볼트액션...)
{
    private const float DefaultSpeed = 1f; //기본속도 1.0
    private PlayerManager _playerManager;
    
    public void Init(PlayerManager playerManager) //초기화
    {
        _playerManager = playerManager;
    }
    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var fireRate = _playerManager.PlayLoadAmmoSFX(); //SFX 재생 및 발사속도 조회
        var animationLength = stateInfo.length; //애니메이션 길이
        var speed = animationLength/fireRate; //발사속도에 맞추어 재생속도 조절
        animator.speed = speed;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.speed = DefaultSpeed; //State를 벗어나면 기본속도로
    }
}
