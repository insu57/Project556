using UnityEngine;

[CreateAssetMenu(fileName = "GearData", menuName = "Scriptable Objects/GearData")]
public class GearData : BaseItemDataSO
{
    [SerializeField] private string itemID;
    [SerializeField]  private string itemName;
    [SerializeField] private GearType gearType;
    
    [SerializeField] private Sprite itemSprite;
    [SerializeField] private Vector2 pickUpColliderOffset;
    [SerializeField] private Vector2 pickUpColliderSize;
    [SerializeField] private int itemWidth;
    [SerializeField] private int itemHeight;
   
    [SerializeField] private int slotWidth;
    [SerializeField] private int slotHeight;
    
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private bool isPartitioned;
    public override string ItemID => itemID;
    public override string ItemName => itemName;
    public GearType GearType => gearType;
    public override Sprite ItemSprite => itemSprite;
    public override Vector2 PickUpColliderOffset => pickUpColliderOffset;
    public override Vector2 PickUpColliderSize => pickUpColliderSize;
    public override int ItemWidth => itemWidth;
    public override int ItemHeight => itemHeight;
    public int SlotWidth => slotWidth;
    public int SlotHeight => slotHeight;
    
    public GameObject SlotPrefab => slotPrefab;
    public bool IsPartitioned => isPartitioned;
    
}
