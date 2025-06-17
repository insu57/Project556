using System.Collections.Generic;
using UnityEngine;

public abstract class BaseItemDataSO : ScriptableObject, IItemData
{
    public abstract string ItemID { get; }
    public abstract string ItemName { get; }
    public abstract Sprite ItemSprite { get; }
    public abstract Vector2 PickUpColliderOffset { get; }
    public abstract Vector2 PickUpColliderSize { get; }
    public abstract int ItemWidth { get; }
    public abstract int ItemHeight { get; }
    public abstract GearType GearType { get; }
    public abstract bool IsStackable { get; }
    public abstract int MaxStackAmount { get; }
    
    //public abstract Vector2 ItemSize { get; }
    //public abstract GameObject ItemPrefab { get; }
}
