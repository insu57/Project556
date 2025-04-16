using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    //변경? -> 들고있는건 Sprite만 정보는 다른 클래스로?
    [SerializeField] private Transform muzzleTransform;
    [SerializeField] private WeaponData weaponData;
    
    //[SerializeField] private TMP_Text currentMagazineAmmoText;
    public Transform MuzzleTransform => muzzleTransform;
    public WeaponData WeaponData => weaponData;
    
    private int _currentMagazineAmmo;
    
    private bool _canShoot = true;
    public bool CanShoot => _canShoot;
    private Collider2D _collider2D;
    private UIManager _uiManager;

    private void Awake()
    {
        _uiManager = FindFirstObjectByType<UIManager>();
    }
    
    private void Start()
    {
        _collider2D = GetComponent<Collider2D>();
    }
    
    public void Init()
    {
        _currentMagazineAmmo = weaponData.DefaultMagazineSize;
        //currentMagazineAmmoText.text = _currentMagazineAmmo.ToString();
        _uiManager.UpdateAmmoText(_currentMagazineAmmo);
        _collider2D.enabled = false;
    }
    
    public void Shoot(bool isFlipped, float shootAngle)
    {
        if(!_canShoot) return;
      
        StartCoroutine(ShootCoroutine()); //fireRate
        
        if(_currentMagazineAmmo <= 0) return; //잔탄이 없으면 return
       
        _currentMagazineAmmo--;
        //currentMagazineAmmoText.text = _currentMagazineAmmo.ToString();
        _uiManager.UpdateAmmoText(_currentMagazineAmmo);
        
        GameObject bulletPrefab = Instantiate(weaponData.BulletPrefab, muzzleTransform.transform.position, Quaternion.identity);
        Destroy(bulletPrefab, 2f); //임시 -> ObjectPooling으로
        Rigidbody2D bulletRB = bulletPrefab.GetComponent<Rigidbody2D>();
        
        Vector2 direction;
        if (isFlipped)
        {
            //Flip이면 x반대방향으로
            direction = 
                new Vector2(-Mathf.Cos(shootAngle*Mathf.Deg2Rad), Mathf.Sin(shootAngle*Mathf.Deg2Rad));
            bulletPrefab.transform.localRotation = Quaternion.Euler(0, 0, 180 - shootAngle);
        }
        else
        {
            direction = 
                new Vector2(Mathf.Cos(shootAngle*Mathf.Deg2Rad), Mathf.Sin(shootAngle*Mathf.Deg2Rad));
            bulletPrefab.transform.localRotation = Quaternion.Euler(0, 0, shootAngle);
        }
        bulletRB.AddForce(direction * weaponData.BulletSpeed, ForceMode2D.Impulse);
    }
    
    public void Reload()
    {
        _currentMagazineAmmo = weaponData.DefaultMagazineSize;
        //currentMagazineAmmoText.text = _currentMagazineAmmo.ToString();
        _uiManager.UpdateAmmoText(_currentMagazineAmmo);
    }
    
    private IEnumerator ShootCoroutine()
    {
        _canShoot = false;
        yield return new WaitForSeconds(WeaponData.FireRate);
        _canShoot = true;
    }
}
