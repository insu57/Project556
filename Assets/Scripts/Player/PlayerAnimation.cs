using System;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerAnimation : MonoBehaviour
{
    private static readonly int Idle = Animator.StringToHash("IDLE");
    private static readonly int IsMove = Animator.StringToHash("isMove");
    private static readonly int IsReload = Animator.StringToHash("isReload");
    public const string Walk = "Walk";
    public const string Run = "Run";
    public const string Climb = "Climb";
    
    [SerializeField] private Animator upperAnimator;
    [SerializeField] private Animator lowerAnimator;
    public Animator UpperAnimator => upperAnimator;
    
    [Serializable]
    private struct OverrideController
    {
        public AnimatorOverrideController upperController;
        public AnimatorOverrideController lowerController;
    }
    
    [Header("OverrideController")] 
    [SerializeField] private OverrideController unarmedController;
    [SerializeField] private OverrideController pistolAnimator;
    [SerializeField] private OverrideController rifleAnimator;

    //private PlayerControl _playerControl;//플레이어 조작관련
    
    private WeaponType _currentWeaponType; //현재 들고있는 무기 종류 - Manager에서 관리하니 제거?
    //하반신 애니메이터 별도로..
    
    public void ChangeAnimationMove(bool isMove)
    {
        upperAnimator.SetBool(IsMove, isMove);
        lowerAnimator.SetBool(IsMove, isMove);
    }

    public void ChangeAnimationReload()
    {
        upperAnimator.SetBool(IsReload, true);
    }

    public void ChangeWeapon(WeaponType newWeaponType) //애니메이션 전환
    {
        _currentWeaponType = newWeaponType;
        switch (newWeaponType)
        {
            case WeaponType.Pistol:
                upperAnimator.runtimeAnimatorController = pistolAnimator.upperController;
                lowerAnimator.runtimeAnimatorController = pistolAnimator.lowerController;
                break;
            case WeaponType.Melee:
            case WeaponType.Unarmed:
                upperAnimator.runtimeAnimatorController = unarmedController.upperController;
                lowerAnimator.runtimeAnimatorController = unarmedController.lowerController;
                break;
            default:
                upperAnimator.runtimeAnimatorController = rifleAnimator.upperController;
                lowerAnimator.runtimeAnimatorController = rifleAnimator.lowerController;
                break;
        }
    }
  
    private void ChangeAnimation(string animName)
    {
        
    }
    
}
