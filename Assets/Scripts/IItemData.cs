using UnityEngine;

public interface IItemData
{
    //SO
    //public 
    public Sprite ItemSprite { get; }
    public Vector2 PickUpColliderOffset { get; }
    public Vector2 PickUpColliderSize { get; }
    public int ItemWidth { get; }
    public int ItemHeight { get; }
    
}
 