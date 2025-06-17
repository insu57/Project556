using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "AmmoData", menuName = "Scriptable Objects/AmmoData")]
public class AmmoData : BaseItemDataSO
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private AmmoCaliber ammoCaliber;
    [SerializeField] private AmmoCategory ammoCategory;

    [SerializeField] private string itemID;
    [SerializeField] private string itemName;
    [SerializeField] private Sprite itemSprite;
    [SerializeField] private Vector2 pickUpColliderOffset;
    [SerializeField] private Vector2 pickUpColliderSize;
    [SerializeField] private int itemWidth;
    [SerializeField] private int itemHeight;
    [FormerlySerializedAs("maxStack")] [SerializeField] private int maxStackAmount;
    
    //ammoPiercing~
    public AmmoCaliber AmmoCaliber => ammoCaliber;
    public AmmoCategory AmmoCategory => ammoCategory;
    public GameObject BulletPrefab => bulletPrefab;
    
    public override string ItemID => itemID;
    public override string ItemName => itemName;
    public override Sprite ItemSprite => itemSprite;
    public override Vector2 PickUpColliderOffset => pickUpColliderOffset;
    public override Vector2 PickUpColliderSize => pickUpColliderSize;
    public override int ItemWidth => itemWidth;
    public override int ItemHeight => itemHeight;
    public override GearType GearType => GearType.None;
    public override bool IsStackable => true;
    public override int MaxStackAmount => maxStackAmount;
}
