using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    //변경? -> 들고있는건 Sprite만 정보는 다른 클래스로?
    //private Transform muzzleTransform;
    private WeaponData _weaponData;
    private Transform _muzzleTransform;
    //public WeaponData WeaponData => weaponData;
    
    private int _currentMagazineAmmo;
    
    private bool _canShoot = true;
    //public bool CanShoot => _canShoot;
 
    private UIManager _uiManager;
    private ObjectPoolingManager _objectPoolingManager;
    
    private AmmoCategory _ammoCategory;
    private const float MaxAccuracy = 100f; //최대 정획도.(탄퍼짐 0)
    private float _normalizedAccuracy; //정규화 정확도
    private const float MaxSpreadAngle = 30f; //최대 탄퍼짐 각도
    private float _maxDeviationAngle; //최대 각도 편차(탄퍼짐 편차)
 
    //[SerializeField] private float speed = 5f;

    private void Awake()
    {
        _objectPoolingManager = FindFirstObjectByType<ObjectPoolingManager>();
    }
    
    public void Init(UIManager uiManager, WeaponData weaponData)
    {
        _uiManager = uiManager;
        _weaponData = weaponData;
        _currentMagazineAmmo = weaponData.DefaultMagazineSize;
        _uiManager.UpdateAmmoText(_currentMagazineAmmo);

        _ammoCategory = EnumManager.GetAmmoCategory(weaponData.AmmoCaliber);
        _normalizedAccuracy = Mathf.Clamp01(weaponData.Accuracy / MaxAccuracy); //정확도 정규화
        _maxDeviationAngle = MaxSpreadAngle * (1 - _normalizedAccuracy); //탄퍼짐 각도 편차
    }
    
    //장전시 사격 제한 추가 필요!!
    public void Shoot(bool isFlipped, float shootAngle)
    {
        if(!_canShoot) return;
        if(_currentMagazineAmmo <= 0) return; //잔탄이 없으면 return
        
        StartCoroutine(ShootCoroutine()); //fireRate
        
        _currentMagazineAmmo--;
        _uiManager.UpdateAmmoText(_currentMagazineAmmo);
        

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
        Bullet bullet = _objectPoolingManager.GetBullet(_ammoCategory);
        bullet.Init(_weaponData.BulletSpeed);
        bullet.ShootBullet(bulletAngle, direction, _muzzleTransform); //Muzzle위치 수정!!
        
    }

    public void SetMuzzleTransform(Transform muzzleTransform)
    {
        _muzzleTransform = muzzleTransform;
    }
    
    public void Reload()
    {
        //장전 시 사격 제한...
        _currentMagazineAmmo = _weaponData.DefaultMagazineSize;
        _uiManager.UpdateAmmoText(_currentMagazineAmmo);
    }
    
    private IEnumerator ShootCoroutine()
    {
        _canShoot = false;
        yield return new WaitForSeconds(_weaponData.FireRate);
        _canShoot = true;
    }
    
    
}
