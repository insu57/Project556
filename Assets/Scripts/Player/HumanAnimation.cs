using System;
using UnityEngine;

namespace Player
{
    public static class HumanAnimatorHash //애니메이션 Hash
    {
        public static readonly int Idle = Animator.StringToHash("IDLE");
        public static readonly int IsMove = Animator.StringToHash("isMove");
        public static readonly int IsSprint = Animator.StringToHash("isSprint");
        public static readonly int IsReload = Animator.StringToHash("isReload");
        public static readonly int Pump = Animator.StringToHash("Pump");
        public static readonly int Bolt = Animator.StringToHash("Bolt");
        public static readonly int Reload  = Animator.StringToHash("Reload");
    }

    public class HumanAnimation : MonoBehaviour
    {
        [SerializeField] private Animator upperAnimator;
        [SerializeField] private Animator lowerAnimator; 
        //상,하체 애니메이터
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
        [SerializeField] private OverrideController rifleAnimator; //각 무기별 OverrideController(기존 애니메이션 대체)
        
        public void ChangeAnimationMove(bool isMove) //move animation transition
        {
            upperAnimator.SetBool(HumanAnimatorHash.IsMove, isMove);
            lowerAnimator.SetBool(HumanAnimatorHash.IsMove, isMove);
        }

        public void ChangeAnimationSprint(bool isSprint) //sprint animation transition 개선?
        {
            upperAnimator.SetBool(HumanAnimatorHash.IsSprint, isSprint);
            lowerAnimator.SetBool(HumanAnimatorHash.IsSprint, isSprint);
        }

        public void ChangeAnimationReload() //Reload Trigger
        {
            upperAnimator.SetTrigger(HumanAnimatorHash.Reload);
        }

        public void ChangeAnimationLoadAmmo(WeaponActionType type) //LoadAmmo Trigger(차탄 공급)
        {
            int id;
            if(type is WeaponActionType.PumpAction) id = HumanAnimatorHash.Pump; //액션타입에 따라 다른 애니메이션
            else if (type is WeaponActionType.BoltAction) id = HumanAnimatorHash.Bolt;
            else return;
            upperAnimator.SetTrigger(id);
        }
        
        public void ChangeWeapon(WeaponType newWeaponType) //애니메이션 전환
        {
            switch (newWeaponType)
            {
                case WeaponType.Pistol: //한손무기
                    upperAnimator.runtimeAnimatorController = pistolAnimator.upperController;
                    lowerAnimator.runtimeAnimatorController = pistolAnimator.lowerController;
                    break;
                case WeaponType.Melee:
                case WeaponType.Unarmed: //비무장 (근접무기 미구현)
                    upperAnimator.runtimeAnimatorController = unarmedController.upperController;
                    lowerAnimator.runtimeAnimatorController = unarmedController.lowerController;
                    break;
                default: //두손무기
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