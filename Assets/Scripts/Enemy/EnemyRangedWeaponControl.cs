using System;
using UnityEngine;

public class EnemyRangedWeaponControl : MonoBehaviour
{
    //Ranged Weapon
    //공격 유형?
    //적 유형별 명중률 보정 필요.
    //적 유형별 사격속도(약한적은 감소 보정)
    
    private float _accuracyMultiplier;
    private float _fireRateMultiplier;
    private float _reloadMultiplier;

    public event Action EnemyShoot;

    public void Init(EnemyData enemyData)
    {
        _accuracyMultiplier = enemyData.AccuracyMultiplier;
        _fireRateMultiplier = enemyData.FireRateMultiplier;
        _reloadMultiplier = enemyData.ReloadMultiplier;
    }

    private void Update()
    {
        Attack();
    }
    
    private void Attack()
    {
        //무기에 따라...연사-점사-단발(가능한 경우 앞부터)
    }
    
    
}
