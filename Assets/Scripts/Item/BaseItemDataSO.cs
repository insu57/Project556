using System.Collections.Generic;
using Item;
using UnityEngine;

public abstract class BaseItemDataSO : ScriptableObject, IItemData //IItemData 직렬화 클래스
{
    public abstract string ItemDataID { get; }
    public abstract string ItemName { get; }
    public abstract Sprite ItemSprite { get; }
    public abstract int ItemWidth { get; }
    public abstract int ItemHeight { get; }
    public abstract GearType GearType { get; }
    public abstract float ItemWeight { get; }
    public abstract bool IsStackable { get; }
    public abstract int MaxStackAmount { get; }
    
}
