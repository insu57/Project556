using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

public class ObjectPoolingManager : Singleton<ObjectPoolingManager>
{
    [Serializable]
    private struct BulletPoolSetting
    {
        public AmmoCategory category;
        public GameObject prefab;
        public int initialSize;
        public int maxSize;
    }
    [SerializeField] private BulletPoolSetting[] bulletPoolSettings; //추후변경...
    [SerializeField] private Transform bulletParent;
    private readonly Dictionary<AmmoCategory, ObjectPool<Bullet>> _bulletPool = new();
    
    [SerializeField, Space] private ItemDragHandler itemDragHandlerPrefab;
    [SerializeField] private Transform itemDragInitParent;
    [SerializeField] private int itemDragInitialSize = 30;
    [SerializeField] private int itemDragMaxSize = 200;
    private ObjectPool<ItemDragHandler> _itemDragHandlerPool;
    
    [SerializeField, Space] private ItemPickUp itemPickUpPrefab;
    [SerializeField] private Transform itemPickupParent;
    [SerializeField] private int itemPickupInitialSize = 30;
    [SerializeField] private int itemPickupMaxSize = 200;
    private ObjectPool<ItemPickUp> _itemPickUpPool;

    private void Start()
    {
        InitBulletPools();
        InitItemDragHandlerPool();
        InitItemPickupPool(); //중복 개선? - 개수가 많아지면 수정
    }

    private void InitBulletPools()
    {
        foreach (var setting in bulletPoolSettings)
        {
            _bulletPool[setting.category] = new ObjectPool<Bullet>(
                createFunc: () => Instantiate(setting.prefab, bulletParent).GetComponent<Bullet>(),
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

    private void InitItemDragHandlerPool()
    {
        _itemDragHandlerPool = new ObjectPool<ItemDragHandler>(
            createFunc: () => Instantiate(itemDragHandlerPrefab, itemDragInitParent),
            actionOnGet: itemDragHandler => { itemDragHandler.gameObject.SetActive(true); },
            actionOnRelease: itemDragHandler => { itemDragHandler.gameObject.SetActive(false); },
            actionOnDestroy: itemDragHandler => { Destroy(itemDragHandler.gameObject); },
            collectionCheck: false,
            defaultCapacity: itemDragInitialSize,
            maxSize: itemDragMaxSize
        );
        
        var tempList = new List<ItemDragHandler>();
        for (int i = 0; i < itemDragInitialSize; i++)
        {
            var obj = _itemDragHandlerPool.Get();
            tempList.Add(obj);
        }
        foreach (var obj in tempList)
        {
            _itemDragHandlerPool.Release(obj);
        }
    }
    
    public ItemDragHandler GetItemDragHandler() => _itemDragHandlerPool.Get();
    public void ReleaseItemDragHandler(ItemDragHandler itemDragHandler) => _itemDragHandlerPool.Release(itemDragHandler);

    private void InitItemPickupPool()
    {
        _itemPickUpPool = new ObjectPool<ItemPickUp>(
            createFunc: () => Instantiate(itemPickUpPrefab, itemPickupParent),
            actionOnGet: itemPickUp => { itemPickUp.gameObject.SetActive(true); },
            actionOnRelease: itemPickUp => { itemPickUp.gameObject.SetActive(false); },
            actionOnDestroy: itemPickUp => { Destroy(itemPickUp.gameObject); },
            collectionCheck: false,
            defaultCapacity: itemPickupInitialSize,
            maxSize: itemPickupMaxSize
        );
        
        var tempList = new List<ItemPickUp>();
        for (int i = 0; i < itemPickupInitialSize; i++)
        {
            var obj = _itemPickUpPool.Get();
            tempList.Add(obj);
        }

        foreach (var obj in tempList)
        {
            _itemPickUpPool.Release(obj);
        }
    }
    public ItemPickUp GetItemPickUp() => _itemPickUpPool.Get();
    public void ReleaseItemPickUp(ItemPickUp itemPickUp) => _itemPickUpPool.Release(itemPickUp);
}
