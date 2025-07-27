using UnityEngine;

[CreateAssetMenu(fileName = "MedicalData", menuName = "Scriptable Objects/MedicalData")]
public class MedicalData : BaseItemDataSO, IConsumableItem
{
    [SerializeField] private string itemID;
    [SerializeField] private string itemName;
    
    [SerializeField] private int healAmount;
    //추가 - 상태이상(출혈, 독 등) 치료
    //틱 당 회복, 출혈 치료, 독 치료... 등
    //여러 종류...
    [SerializeField] private StatAdjustAmount[] adjustAmount;
    [SerializeField] private StatEffectPerSecond[] effectPerSecond;
    [SerializeField] private float useDuration = 1;
    
    [SerializeField] private Sprite itemSprite;
   
    [SerializeField] private int itemWidth;
    [SerializeField] private int itemHeight;
    [SerializeField] private float itemWeight;
    [SerializeField] private bool isStackable = false;
    [SerializeField] private int maxStackAmount = 1;
    
    public override string ItemDataID => itemID;
    public override string ItemName => itemName;
    public int HealAmount => healAmount;
    public override Sprite ItemSprite => itemSprite;
    public override int ItemWidth => itemWidth;
    public override int ItemHeight => itemHeight;
    public override GearType GearType => GearType.None;
    public override float ItemWeight => itemWeight;
    public override bool IsStackable => isStackable;
    public override bool IsConsumable => true;
    public override int MaxStackAmount => maxStackAmount;
    public StatAdjustAmount[] AdjustAmount => adjustAmount;
    public StatEffectPerSecond[] EffectPerSecond => effectPerSecond;
    public float UseDuration => useDuration;
}
