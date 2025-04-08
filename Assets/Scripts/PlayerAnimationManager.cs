using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private List<String> idleList = new List<String>();
    [SerializeField] private List<String> walkList = new List<String>();

    private GunType _currentWeapon = GunType.Pistol;
    
    public bool IsWalk { get; set; }

    private void Awake()
    {
        
    }

    private void Update()
    {
        playerAnimator.SetBool("isMove", IsWalk);
    }

    private void ChangeAnimation(string animName)
    {
        
    }
    
}
