using System;
using System.Collections;
using TMPro;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerWeapon : MonoBehaviour
{
    private WeaponData _weaponData;
    private Transform _muzzleTransform;
    
    private bool _canShoot = true;
    
    private AmmoCategory _ammoCategory;
    private const float MaxAccuracy = 100f; //최대 정획도.(탄퍼짐 0)
    private float _normalizedAccuracy; //정규화 정확도
    private const float MaxSpreadAngle = 30f; //최대 탄퍼짐 각도
    private float _maxDeviationAngle; //최대 각도 편차(탄퍼짐 편차)

    public event Action OnShowMuzzleFlash;
    
    public void ChangeWeaponData(WeaponData weaponData)
    {
        _weaponData = weaponData;

        _ammoCategory = EnumManager.GetAmmoCategory(weaponData.AmmoCaliber);
        _normalizedAccuracy = Mathf.Clamp01(weaponData.Accuracy / MaxAccuracy); //정확도 정규화
        _maxDeviationAngle = MaxSpreadAngle * (1 - _normalizedAccuracy); //탄퍼짐 각도 편차
    }
    
    //개선...? Player매니저 shoot에서 장탄검사?
    public bool Shoot(bool isFlipped, float shootAngle)
    {
        if(!_canShoot) return false;
        
        OnShowMuzzleFlash?.Invoke(); //show flash
        
        StartCoroutine(ShootCoroutine()); //fireRate

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
        Bullet bullet = ObjectPoolingManager.Instance.GetBullet(_ammoCategory);
        bullet.Init(_weaponData.BulletSpeed, 20f, 0.1f); //data에서
        bullet.ShootBullet(bulletAngle, direction, _muzzleTransform); //Muzzle위치 수정!!
        return true;
    }

    public void SetMuzzleTransform(Transform muzzleTransform)
    {
        _muzzleTransform = muzzleTransform;
    }
    
    private IEnumerator ShootCoroutine()
    {
        _canShoot = false;
        yield return new WaitForSeconds(_weaponData.FireRate);
        _canShoot = true;
    }
    
    
}
