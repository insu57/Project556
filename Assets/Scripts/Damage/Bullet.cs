using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private AmmoCategory ammoCategory;
    
    private Rigidbody2D _rigidbody2D;
    private float _speed;
    private float _damage;
    private float _armorPiercing;

    private const float BulletLifeTime = 2f;
    private float _timer;

    private readonly List<IDamageable> _targets = new();
    
    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        _timer = 0f;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= BulletLifeTime)
        {
            ObjectPoolingManager.Instance.ReleaseBullet(ammoCategory, this);
        }
    }

    public void Init(float speed, float damage, float armorPiercing)
    {
        _speed = speed;
        _damage = damage;
        _armorPiercing = armorPiercing;
        
        _targets.Clear();
    }
    
    public void ShootBullet(float angle, Vector2 direction, Transform muzzleTransform)
    {
        transform.position = muzzleTransform.position;
        transform.localRotation = Quaternion.Euler(0, 0, angle);
        _rigidbody2D.AddForce(direction * _speed, ForceMode2D.Impulse);
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            ObjectPoolingManager.Instance.ReleaseBullet(ammoCategory, this);
        }
        else if (collision.TryGetComponent(out IDamageable damageable))
        {
            if(_targets.Contains(damageable)) return; //리스트에 있으면 처리안함. 캐릭터 당 한번만 처리해야함.
            
            _targets.Add(damageable);

            damageable.TakeDamage(_damage);
            
            ObjectPoolingManager.Instance.ReleaseBullet(ammoCategory, this);//적중 시 비활성화(풀링), 관통력 관련 추가 필요.
        }
    }
}

