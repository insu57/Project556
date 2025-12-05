using System;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolingManager : Singleton<ObjectPoolingManager>
{
    [Serializable]
    private class BulletPoolSetting
    {
        public AmmoCategory category;
        public PoolSetting poolSetting;
        public GameObject prefab;
    }

    [Serializable]
    private class PoolSetting
    {
        public Transform parent;
        public int initialSize = 30;
        public int maxSize = 200;
    }
    
    [SerializeField] private BulletPoolSetting[] bulletPoolSettings; //추후변경...
    private readonly Dictionary<AmmoCategory, ObjectPool<Bullet>> _bulletPool = new();
    
    [SerializeField, Space] private ItemDragHandler itemDragHandlerPrefab;
    [SerializeField] private PoolSetting itemDragSetting;
    private ObjectPool<ItemDragHandler> _itemDragHandlerPool;
    
    [SerializeField, Space] private ItemPickUp itemPickUpPrefab;
    [SerializeField] private PoolSetting itemPickUpSetting;
    private ObjectPool<ItemPickUp> _itemPickUpPool;
    
    [SerializeField, Space] private AudioSource audioSourcePrefab;
    [SerializeField] private PoolSetting audioSourceSetting;
    private ObjectPool<AudioSource> _audioSourcePool;

    protected override void Awake()
    {
        base.Awake();
        
        InitBulletPools();
        _itemDragHandlerPool = InitPool(itemDragHandlerPrefab, itemDragSetting);
        _itemPickUpPool = InitPool(itemPickUpPrefab, itemPickUpSetting);
        _audioSourcePool = InitPool(audioSourcePrefab, audioSourceSetting);
    }

    private void InitBulletPools()
    {
        foreach (var setting in bulletPoolSettings)
        {
            _bulletPool[setting.category] = InitPool(setting.prefab.GetComponent<Bullet>(), setting.poolSetting);
        }
    }
    
    public Bullet GetBullet(AmmoCategory category) => _bulletPool[category].Get();

    public void ReleaseBullet(AmmoCategory ammoCategory, Bullet bullet) => _bulletPool[ammoCategory].Release(bullet);
    
    public ItemDragHandler GetItemDragHandler() => _itemDragHandlerPool.Get();//? Get불가
    public void ReleaseItemDragHandler(ItemDragHandler itemDragHandler) => _itemDragHandlerPool.Release(itemDragHandler);

    public ItemPickUp GetItemPickUp() => _itemPickUpPool.Get();
    public void ReleaseItemPickUp(ItemPickUp itemPickUp) => _itemPickUpPool.Release(itemPickUp);
    
    public AudioSource GetAudioSource() => _audioSourcePool.Get();
    public void ReleaseAudioSource(AudioSource audioSource) => _audioSourcePool.Release(audioSource);

    private ObjectPool<T> InitPool<T>(T prefab, PoolSetting poolSetting) where T : Component
    {
        var pool = new ObjectPool<T>(
            createFunc: () => Instantiate(prefab, poolSetting.parent),
            actionOnGet: component => component.gameObject.SetActive(true),
            actionOnRelease: component => component.gameObject.SetActive(false),
            actionOnDestroy: component => Destroy(component.gameObject),
            collectionCheck: false,
            defaultCapacity: poolSetting.initialSize,
            maxSize: poolSetting.maxSize
        );
        
        var tempList = new List<T>();
        for (var i = 0; i < poolSetting.initialSize; i++)
        {
            var obj = pool.Get();
            tempList.Add(obj);
        }

        foreach (var obj in tempList)
        {
            pool.Release(obj);
        }

        return pool;
    }
}
