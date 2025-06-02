using UnityEngine;

[CreateAssetMenu(fileName = "MedicalData", menuName = "Scriptable Objects/MedicalData")]
public class MedicalData : BaseItemDataSO
{
    [SerializeField] private string itemID;
    [SerializeField] private string itemName;
    
    [SerializeField] private int healAmount;
    
    
    [SerializeField] private Sprite itemSprite;
    [SerializeField] private Vector2 pickUpColliderOffset;
    [SerializeField] private Vector2 pickUpColliderSize;
    [SerializeField] private int itemWidth;
    [SerializeField] private int itemHeight;
    
    public override string ItemID => itemID;
    public override string ItemName => itemName;
    public int HealAmount => healAmount;
    //
    public override Sprite ItemSprite => itemSprite;
    public override Vector2 PickUpColliderOffset => pickUpColliderOffset;
    public override Vector2 PickUpColliderSize => pickUpColliderSize;
    public override int ItemWidth => itemWidth;
    public override int ItemHeight => itemHeight;
    public override GearType GearType => GearType.None;
}
