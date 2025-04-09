using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    //private static readonly int Idle = Animator.StringToHash("IDLE");
    private static readonly int IsMove = Animator.StringToHash("isMove");
    public const string Walk = "Walk";
    public const string Run = "Run";
    public const string Climb = "Climb";
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private AnimatorOverrideController unarmedAnimator;
    [SerializeField] private AnimatorOverrideController pistolAnimator;
    //[SerializeField] private List<String> idleList = new List<String>();
    //[SerializeField] private List<String> walkList = new List<String>();

    private PlayerControl _playerControl;
    
    private GunType _currentWeapon = GunType.Pistol; //현재 들고있는 무기 종류
    
    private void Awake()
    {
        _playerControl = GetComponent<PlayerControl>();
    }

    private void Start()
    {
        _playerControl.OnPlayerMove += HandlePlayerMove;
        ChangeWeapon(GunType.Pistol);//
    }

    private void Update()
    {
        
    }

    private void HandlePlayerMove(bool isMove)
    {
        playerAnimator.SetBool(IsMove, isMove);
    }

    private void ChangeWeapon(GunType newGunType)
    {
        _currentWeapon = newGunType;
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
