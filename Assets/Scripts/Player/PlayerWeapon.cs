using System;
using System.Collections;
using TMPro;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerWeapon : MonoBehaviour
{
    private WeaponData _currentWeaponData;
    private Transform _muzzleTransform;
    private AmmoData _currentAmmoData;
    private const float MaxAccuracy = 100f; //최대 정획도.(탄퍼짐 0)
    private float _normalizedAccuracy; //정규화 정확도
    private const float MaxSpreadAngle = 30f; //최대 탄퍼짐 각도
    private float _maxDeviationAngle; //최대 각도 편차(탄퍼짐 편차)
    private float _lastShotTime;
    
    public event Action OnShowMuzzleFlash;
    public event Action<WeaponActionType> OnEndWeaponAction;
    
    public void ChangeWeaponData(WeaponData weaponData, AmmoData ammoData)
    {
        _currentWeaponData = weaponData;
        _currentAmmoData = ammoData;
        
        _normalizedAccuracy = Mathf.Clamp01(weaponData.Accuracy / MaxAccuracy); //정확도 정규화
        _maxDeviationAngle = MaxSpreadAngle * (1 - _normalizedAccuracy); //탄퍼짐 각도 편차
    }

    public void SetAmmoData(AmmoData ammoData)
    {
        _currentAmmoData = ammoData;
    }
    
    public bool Shoot(bool isFlipped, float shootAngle)
    {
        if(Time.time - _lastShotTime < _currentWeaponData.FireRate) return false; //FireRate제한
        _lastShotTime = Time.time;
        
        OnShowMuzzleFlash?.Invoke(); //show flash

        var palletCount = _currentAmmoData.IsBuckshot ? _currentAmmoData.PelletCount : 1; //벅샷이라면 해당 탄의 펠릿 수 만큼 발사
        
        for (var i = 0; i < palletCount; i++)
        {
            float bulletAngle;
            Vector2 direction;
        
            float offsetAngle = Random.Range(-_maxDeviationAngle, _maxDeviationAngle); //랜덤 탄퍼짐 각도
        
            shootAngle += offsetAngle;
            if (isFlipped)
            {
                //Flip이면 x반대방향으로
                //발사 각도 연산
                direction = 
                    new Vector2(-Mathf.Cos(shootAngle*Mathf.Deg2Rad), Mathf.Sin(shootAngle*Mathf.Deg2Rad));
                bulletAngle = 180 - shootAngle;
            }
            else
            {
                direction = 
                    new Vector2(Mathf.Cos(shootAngle*Mathf.Deg2Rad), Mathf.Sin(shootAngle*Mathf.Deg2Rad));
                bulletAngle = shootAngle;
            }
            Bullet bullet = ObjectPoolingManager.Instance.GetBullet(_currentAmmoData.AmmoCategory);//Pool에서 Get(탄종에 따라)
            var finalDamage = _currentAmmoData.AmmoDamage * _currentWeaponData.DamageMultiplier; //탄환 데미지 * 무기 피해량 배수
            var finalSpeed = _currentAmmoData.ProjectileSpeed * _currentWeaponData.MuzzleVelocityMultiplier; //탄환 속도 * 무기 총구 속도 배수
            bullet.Init(finalSpeed, finalDamage, _currentAmmoData.AmmoPiercing);
            bullet.ShootBullet(bulletAngle, direction, _muzzleTransform);
        }

        if (_currentWeaponData.WeaponActionType == WeaponActionType.PumpAction)
        {
            OnEndWeaponAction?.Invoke(WeaponActionType.PumpAction);
        }
        
        return true;
    }
    
    public void SetMuzzleTransform(Transform muzzleTransform) //총구 위치 설정
    {
        _muzzleTransform = muzzleTransform;
    }
    
}
