using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private static readonly int Idle = Animator.StringToHash("IDLE");
    private static readonly int IsMove = Animator.StringToHash("isMove");
    private static readonly int Reload = Animator.StringToHash("Reload");
    public const string Walk = "Walk";
    public const string Run = "Run";
    public const string Climb = "Climb";
    
    [SerializeField] private Animator playerAnimator;
    [Header("OverrideController")]
    [SerializeField] private AnimatorOverrideController unarmedAnimator;
    [SerializeField] private AnimatorOverrideController pistolAnimator;

    private PlayerControl _playerControl;
    
    private GunType _currentWeaponType = GunType.Pistol; //현재 들고있는 무기 종류
    
    private void Awake()
    {
        _playerControl = GetComponent<PlayerControl>();
    }

    private void Start()
    {
        _playerControl.OnPlayerMove += HandlePlayerMove;
        _playerControl.OnPlayerReload += HandlePlayerReload;
        ChangeWeapon(GunType.Pistol);//
    }
    
    private void HandlePlayerMove(bool isMove)
    {
        playerAnimator.SetBool(IsMove, isMove);
    }

    private void HandlePlayerReload()
    {
        playerAnimator.SetTrigger(Reload);
    }

    private void ChangeWeapon(GunType newGunType)
    {
        _currentWeaponType = newGunType;
        switch (newGunType)
        {
            case GunType.Pistol:
                playerAnimator.runtimeAnimatorController = pistolAnimator;
                break;
            default:
                playerAnimator.runtimeAnimatorController  = unarmedAnimator;
                break;
        }
    }
    
    private void ChangeAnimation(string animName)
    {
        
    }
    
}
