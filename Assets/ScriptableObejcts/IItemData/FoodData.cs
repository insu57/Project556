using Item;
using UnityEngine;

[CreateAssetMenu(fileName = "FoodData", menuName = "Scriptable Objects/FoodData")]
public class FoodData : BaseItemDataSO, IConsumableItem
{
    [SerializeField] private string itemID;
    [SerializeField] private string itemName;
    
    [SerializeField] private StatAdjustAmount[] adjustAmount; //아이템 효과(증감량, 아이템 사용시간, 초당 증감량/사용시간)
    [SerializeField] private StatEffectPerSecond[] effectPerSecond;//아이템 효과(초당 효과, 지속 시간)
    [SerializeField] private float useDuration = 1; //사용시간
    
    [SerializeField] private Sprite itemSprite;
   
    [SerializeField] private int itemWidth;
    [SerializeField] private int itemHeight;
    [SerializeField] private float itemWeight;
    [SerializeField] private bool isStackable = false;
    [SerializeField] private int maxStackAmount = 1;
    
    public override string ItemDataID => itemID;
    public override string ItemName => itemName;
    public override Sprite ItemSprite => itemSprite;
    public override int ItemWidth => itemWidth;
    public override int ItemHeight => itemHeight;
    public override Vector2Int ItemCellCount => new (itemWidth, itemHeight);
    public override GearType GearType => GearType.None;
    public override float ItemWeight => itemWeight;
    public override bool IsStackable => isStackable;
    public override int MaxStackAmount => maxStackAmount;
    
    public StatAdjustAmount[] AdjustAmount =>  adjustAmount;
    public StatEffectPerSecond[] EffectPerSecond => effectPerSecond;
    public float UseDuration => useDuration;
}
