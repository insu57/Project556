using UnityEngine;

[CreateAssetMenu(fileName = "GearData", menuName = "Scriptable Objects/GearData")]
public class GearData : BaseItemDataSO
{
    [SerializeField] private string itemID;
    [SerializeField]  private string itemName;
    [SerializeField] private GearType gearType;
    
    [SerializeField] private Sprite itemSprite;
    [SerializeField] private Vector2 pickUpColliderSize;
    [SerializeField] private int itemWidth = 1;
    [SerializeField] private int itemHeight = 1;
   
    [SerializeField] private int slotWidth;
    [SerializeField] private int slotHeight;
    
    [SerializeField] private GameObject slotPrefab;

    
    public override string ItemDataID => itemID;
    public override string ItemName => itemName;
    public override GearType GearType => gearType;
    public override Sprite ItemSprite => itemSprite;

    public override int ItemWidth => itemWidth;
    public override int ItemHeight => itemHeight;

    public GameObject SlotPrefab => slotPrefab;
    public override bool IsStackable => false;
    public override int MaxStackAmount => 1;
}
