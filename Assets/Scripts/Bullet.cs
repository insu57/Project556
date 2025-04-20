using System;
using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //[SerializeField] private float damage;
    //[SerializeField] private float armorPiercing;

    private AmmoCaliber _ammoCaliber;
    [SerializeField] private AmmoCategory ammoCategory;
    
    private ObjectPoolingManager _objectPoolingManager;
    private Rigidbody2D _rigidbody2D;
    private float _speed;

    //[SerializeField] private float bulletWaitTime = 2f;
    private static readonly WaitForSeconds BulletWait= new WaitForSeconds(2f);
    
    private void Awake()
    {
        _objectPoolingManager = GetComponentInParent<ObjectPoolingManager>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void Init(float speed)
    {
        _speed = speed;
    }
    
    public void ShootBullet(float angle, Vector2 direction, Transform muzzleTransform)
    {
        transform.position = muzzleTransform.position;
        transform.localRotation = Quaternion.Euler(0, 0, angle);
        _rigidbody2D.AddForce(direction * _speed, ForceMode2D.Impulse);
        StartCoroutine(DestroyBullet());
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            Destroy(this.gameObject);
        }
    }

    private IEnumerator DestroyBullet()
    {
        yield return BulletWait;
        _objectPoolingManager.ReleaseBullet(ammoCategory, this);
    }
}
