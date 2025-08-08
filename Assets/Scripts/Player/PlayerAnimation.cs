using System;
using UnityEngine;

namespace Player
{
    public static class PlayerAnimatorHash
    {
        public static readonly int Idle = Animator.StringToHash("IDLE");
        public static readonly int IsMove = Animator.StringToHash("isMove");
        public static readonly int IsSprint = Animator.StringToHash("isSprint");
        public static readonly int IsReload = Animator.StringToHash("isReload");
        public static readonly int Pump = Animator.StringToHash("Pump");
        public static readonly int Bolt = Animator.StringToHash("Bolt");
        public static readonly int Reload  = Animator.StringToHash("Reload");
    }

    public class PlayerAnimation : MonoBehaviour
    {
        public const string Walk = "Walk";
        public const string Run = "Run";
        public const string Climb = "Climb";
    
        [SerializeField] private Animator upperAnimator;
        [SerializeField] private Animator lowerAnimator;
        public Animator UpperAnimator => upperAnimator;
        public Animator LowerAnimator => lowerAnimator;
    
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
    
        private WeaponType _currentWeaponType; //현재 들고있는 무기 종류 - Manager에서 관리하니 제거?
        //하반신 애니메이터 별도로..
    
        public void ChangeAnimationMove(bool isMove)
        {
            upperAnimator.SetBool(PlayerAnimatorHash.IsMove, isMove);
            lowerAnimator.SetBool(PlayerAnimatorHash.IsMove, isMove);
        }

        public void ChangeAnimationSprint(bool isSprint)
        {
            upperAnimator.SetBool(PlayerAnimatorHash.IsSprint, isSprint);
            lowerAnimator.SetBool(PlayerAnimatorHash.IsSprint, isSprint);
        }

        public void ChangeAnimationReload()
        {
            //upperAnimator.SetBool(PlayerAnimatorHash.IsReload, true);
            upperAnimator.SetTrigger(PlayerAnimatorHash.Reload);
        }

        public void ChangeAnimationLoadAmmo(WeaponActionType type)
        {
            int id;
            if(type is WeaponActionType.PumpAction) id = PlayerAnimatorHash.Pump;
            else if (type is WeaponActionType.BoltAction) id = PlayerAnimatorHash.Bolt;
            else return;
            upperAnimator.SetTrigger(id);
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
}