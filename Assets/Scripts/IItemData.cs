using UnityEngine;

public interface IItemData
{
    //SO
    public string ItemID { get; }
    public string ItemName { get; }
    public Sprite ItemSprite { get; }
    public Vector2 PickUpColliderOffset { get; }
    public Vector2 PickUpColliderSize { get; }
    public int ItemWidth { get; }
    public int ItemHeight { get; }
    public GearType GearType { get; }
    //public Vector2 ItemSize { get; }
}
 