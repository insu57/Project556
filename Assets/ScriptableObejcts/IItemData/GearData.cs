using UnityEngine;

[CreateAssetMenu(fileName = "GearData", menuName = "Scriptable Objects/GearData")]
public class GearData : BaseItemDataSO
{
    [SerializeField] private string itemID;
    [SerializeField]  private string itemName;
    [SerializeField] private GearType gearType; //Gear종류(None이면 장비가 아닌 아이템)
    [SerializeField] private float armorAmount; //방어도
    
    [SerializeField] private Sprite itemSprite;
    [SerializeField] private int itemWidth = 1;
    [SerializeField] private int itemHeight = 1;
    [SerializeField] private float itemWeight;
    [SerializeField] private GameObject slotPrefab; //리그, 백팩 인벤토리 슬롯

    
    public override string ItemDataID => itemID;
    public override string ItemName => itemName;
    public override GearType GearType => gearType;
    public float ArmorAmount => armorAmount;
    public override Sprite ItemSprite => itemSprite;

    public override int ItemWidth => itemWidth;
    public override int ItemHeight => itemHeight;
    public override Vector2Int ItemCellCount => new (itemWidth, itemHeight);
    public override float ItemWeight => itemWeight;
    public GameObject SlotPrefab => slotPrefab;
    public override bool IsStackable => false;
    public override int MaxStackAmount => 1;
}
