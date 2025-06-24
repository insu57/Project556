using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

public class ObjectPoolingManager : MonoBehaviour
{
    [Serializable]
    struct BulletPoolSetting
    {
        public AmmoCategory category;
        public GameObject prefab;
        public int initialSize;
        public int maxSize;
    }
    [SerializeField] private BulletPoolSetting[] bulletPoolSettings; //추후변경...
    private readonly Dictionary<AmmoCategory, ObjectPool<Bullet>> _bulletPool = new();
    //ItemDragHandler -> 오브젝트 풀링 관리...
    private void Start()
    {
        InitBulletPools();
    }

    private void InitBulletPools()
    {
        foreach (var setting in bulletPoolSettings)
        {
            _bulletPool[setting.category] = new ObjectPool<Bullet>(
                createFunc: () => Instantiate(setting.prefab, transform).GetComponent<Bullet>(),
                actionOnGet: bullet => { bullet.gameObject.SetActive(true); },
                actionOnRelease: bullet => { bullet.gameObject.SetActive(false); },
                actionOnDestroy: bullet => { Destroy(bullet.gameObject); },
                collectionCheck: false,
                defaultCapacity: setting.initialSize,
                maxSize: setting.maxSize
            );

            var tempList = new List<Bullet>();
            for (int i = 0; i < setting.initialSize; i++)
            {
                var obj = _bulletPool[setting.category].Get();
                tempList.Add(obj);
            }

            foreach (var obj in tempList)
            {
                _bulletPool[setting.category].Release(obj);
            }
        }
    }
    
    public Bullet GetBullet(AmmoCategory category) => _bulletPool[category].Get();

    public void ReleaseBullet(AmmoCategory ammoCategory, Bullet bullet) => _bulletPool[ammoCategory].Release(bullet);
    
}
