using UnityEngine;

public interface IItemData
{
    //SO
    public string ItemDataID { get; }
    public string ItemName { get; }
    public Sprite ItemSprite { get; }
    public int ItemWidth { get; }
    public int ItemHeight { get; }
    public GearType GearType { get; }
    public bool IsStackable { get; }
    public bool IsConsumable { get; }
    public int MaxStackAmount { get; }
}
 