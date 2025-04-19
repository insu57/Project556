using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerAnimation : MonoBehaviour
{
    private static readonly int Idle = Animator.StringToHash("IDLE");
    private static readonly int IsMove = Animator.StringToHash("isMove");
    private static readonly int Reload = Animator.StringToHash("Reload");
    public const string Walk = "Walk";
    public const string Run = "Run";
    public const string Climb = "Climb";
    
    [SerializeField] private Animator upperAnimator;
    [SerializeField] private Animator lowerAnimator;

    [Serializable]
    private struct OverrideController
    {
        public AnimatorOverrideController upperController;
        public AnimatorOverrideController lowerController;
    }

    [SerializeField] private float test;
    [Header("OverrideController")] 
    [SerializeField] private OverrideController unarmedController;
    [SerializeField] private OverrideController pistolAnimator;
    [SerializeField] private OverrideController rifleAnimator;

    private PlayerControl _playerControl;
    
    private WeaponType _currentWeaponType = WeaponType.Pistol; //현재 들고있는 무기 종류
    //하반신 애니메이터 별도로..
    
    private void Awake()
    {
        _playerControl = GetComponent<PlayerControl>();
    }

    private void Start()
    {
        _playerControl.OnPlayerMove += HandlePlayerMove;
        _playerControl.OnPlayerReload += HandlePlayerReload;
        ChangeWeapon(WeaponType.Pistol);//
    }
    
    private void HandlePlayerMove(bool isMove)
    {
        upperAnimator.SetBool(IsMove, isMove);
        lowerAnimator.SetBool(IsMove, isMove);
    }

    private void HandlePlayerReload()
    {
        upperAnimator.SetTrigger(Reload);
    }

    public void ChangeWeapon(WeaponType newWeaponType)
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
