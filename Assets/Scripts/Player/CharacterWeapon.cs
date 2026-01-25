using System;
using System.Collections;
using TMPro;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

public class CharacterWeapon : MonoBehaviour //무기 사격(장탄 관련은 처리하지 않음)
{
    private WeaponData _currentWeaponData;
    private Transform _muzzleTransform;
    private AmmoData _currentAmmoData;
    private float _timeBetweenShot;
    private const float MaxAccuracy = 100f; //최대 정획도.(탄퍼짐 0)
    private float _normalizedAccuracy; //정규화 정확도
    private const float MaxSpreadAngle = 30f; //최대 탄퍼짐 각도 -> 무기(종류)별로 설정?
    private float _maxDeviationAngle; //최대 각도 편차(탄퍼짐 편차)
    private float _lastShotTime;
    
    public event Action OnShowMuzzleFlash;
    public event Action<WeaponActionType> OnEndWeaponLoadAction;
    
    public void ChangeWeaponData(WeaponData weaponData, AmmoData ammoData)
    {
        _currentWeaponData = weaponData;
        _currentAmmoData = ammoData;
        
        _normalizedAccuracy = Mathf.Clamp01(weaponData.Accuracy / MaxAccuracy); //정확도 정규화
        _maxDeviationAngle = MaxSpreadAngle * (1 - _normalizedAccuracy); //탄퍼짐 각도 편차

        _timeBetweenShot = _currentWeaponData.TimeBetweenShot;
    }

    public void SetAmmoData(AmmoData ammoData)
    {
        _currentAmmoData = ammoData;
    }

    public void SetCharacterMultiplier(float accuracyMultiplier, float fireRateMultiplier)
    {
        //적 사격 보정치
        _normalizedAccuracy *= accuracyMultiplier; //명중 보정치(1에 가까울수록 높은 명중률)
        _maxDeviationAngle = MaxSpreadAngle * (1 - _normalizedAccuracy);

        fireRateMultiplier = Mathf.Max(fireRateMultiplier, 0.01f); //최소치 0.01
        _timeBetweenShot /= fireRateMultiplier; //발사속도 계수(0에 가까울 수 록 느리게) 만큼 발사속도를 낮춤(발사 간 시간을 늘림)
    }
    
    public bool Shoot(bool isFlipped, float shootAngle)
    {
        if(Time.time - _lastShotTime < _timeBetweenShot) return false; //FireRate제한
        _lastShotTime = Time.time;
        
        OnShowMuzzleFlash?.Invoke(); //show flash

        var palletCount = _currentAmmoData.IsBuckshot ? _currentAmmoData.PelletCount : 1; //벅샷이라면 해당 탄의 펠릿 수 만큼 발사
        //사격 버그? 원인 미확인
        
        //명중률 관련 버그 해결
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
            OnEndWeaponLoadAction?.Invoke(WeaponActionType.PumpAction);
        }
        
        return true;
    }
    
    public void SetMuzzleTransform(Transform muzzleTransform) //총구 위치 설정
    {
        _muzzleTransform = muzzleTransform;
    }
    
}
