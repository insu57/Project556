using UnityEngine;

public abstract class BaseCrateDataSO : ScriptableObject
{
    public abstract string CrateDataID { get; }
    public abstract string CrateName { get; }
    public abstract Inventory InventoryPrefab { get; }
    public abstract Sprite CrateSprite { get; }
    
}
