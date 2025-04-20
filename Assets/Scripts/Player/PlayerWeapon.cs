using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    //변경? -> 들고있는건 Sprite만 정보는 다른 클래스로?
    [SerializeField] private Transform muzzleTransform;
    [SerializeField] private WeaponData weaponData;
  
    public WeaponData WeaponData => weaponData;
    
    private int _currentMagazineAmmo;
    
    private bool _canShoot = true;
    //public bool CanShoot => _canShoot;
 
    private UIManager _uiManager;
    private ObjectPoolingManager _objectPoolingManager;
    
    private AmmoCategory _ammoCategory;
    private const float MaxAccuracy = 100f;
    private float _normalizedAccuracy;
    private const float MaxSpreadAngle = 30f;
    private float _maxDeviationAngle;
    //temp
    [SerializeField] private float speed = 5f;

    private void Awake()
    {
        _objectPoolingManager = FindFirstObjectByType<ObjectPoolingManager>();
    }
    
    public void Init(UIManager uiManager)
    {
        _uiManager = uiManager;
        _currentMagazineAmmo = weaponData.DefaultMagazineSize;
        _uiManager.UpdateAmmoText(_currentMagazineAmmo);

        _ammoCategory = EnumManager.GetAmmoCategory(WeaponData.AmmoCaliber);
        _normalizedAccuracy = Mathf.Clamp01(weaponData.Accuracy / MaxAccuracy);
        _maxDeviationAngle = MaxSpreadAngle * (1 - _normalizedAccuracy);
    }
    
    public void Shoot(bool isFlipped, float shootAngle)
    {
        if(!_canShoot) return;
        if(_currentMagazineAmmo <= 0) return; //잔탄이 없으면 return
        
        StartCoroutine(ShootCoroutine()); //fireRate
        
        _currentMagazineAmmo--;
        _uiManager.UpdateAmmoText(_currentMagazineAmmo);
        

        float bulletAngle;
        Vector2 direction;
        
        float offsetAngle = Random.Range(-_maxDeviationAngle, _maxDeviationAngle);
        
        shootAngle += offsetAngle;
        if (isFlipped)
        {
            //Flip이면 x반대방향으로
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
        bullet.Init(weaponData.BulletSpeed);
        bullet.ShootBullet(bulletAngle, direction, muzzleTransform);
        
    }
    
    public void Reload()
    {
        _currentMagazineAmmo = weaponData.DefaultMagazineSize;
        _uiManager.UpdateAmmoText(_currentMagazineAmmo);
    }
    
    private IEnumerator ShootCoroutine()
    {
        _canShoot = false;
        yield return new WaitForSeconds(WeaponData.FireRate);
        _canShoot = true;
    }
    
    
}
