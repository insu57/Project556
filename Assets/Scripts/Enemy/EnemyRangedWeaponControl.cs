using System;
using UnityEngine;

public class EnemyRangedWeaponControl : MonoBehaviour
{
    //Ranged Weapon
    //공격 유형?
    //적 유형별 명중률 보정 필요.
    //적 유형별 사격속도(약한적은 감소 보정)
    private IEnemyRangedContext _enemy; //EnemyBase의 데이터 접근용
    
    private WeaponData _weaponData;
    private Transform _muzzleTransform;
    private float _accuracyMultiplier;
    private float _fireRateMultiplier;
    private float _reloadMultiplier;
    private FireMode _fireMode;
    private int _currentMagazine;
    
    public event Action EnemyShoot;

    public void Init(IEnemyRangedContext enemy, EnemyData enemyData, WeaponData enemyWeaponData)
    {
        _enemy = enemy;
        
        _accuracyMultiplier = enemyData.AccuracyMultiplier;
        _fireRateMultiplier = enemyData.FireRateMultiplier;
        _reloadMultiplier = enemyData.ReloadMultiplier;

        _weaponData = enemyWeaponData;

        _fireMode = FireMode.SemiAuto;

        foreach (var fireMode in _weaponData.FireModes) //연사에 가까운 FireMode로
        {
            if (fireMode > _fireMode)
            {
                _fireMode = fireMode;
            }
        }
        
        _muzzleTransform = _weaponData.IsOneHanded ? enemy.OneHandedMuzzle : enemy.TwoHandedMuzzle; //총구 위치 초기화
        _currentMagazine = _weaponData.DefaultMagazineSize;
    }

    private void Update()
    {
        Attack();
    }
    
    private void Attack()
    {
        //무기에 따라...연사-(점사?)-단발(가능한 경우 앞부터)
        //발사 시 이벤트 -> EnemyManager(HumanRanged)
        
        //연사 점사 단발 처리??
        if(_currentMagazine <= 0) return;
        //EnemyShoot?.Invoke();
        
        
    }
    
    
}
