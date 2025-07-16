using UnityEngine;

[CreateAssetMenu(fileName = "FoodData", menuName = "Scriptable Objects/FoodData")]
public class FoodData : BaseItemDataSO
{
    [SerializeField] private string itemID;
    [SerializeField] private string itemName;
    
    [SerializeField] private float hydrationAmount;
    [SerializeField] private float energyAmount;
    
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
    public override GearType GearType => GearType.None;
    public override float ItemWeight => itemWeight;
    public override bool IsStackable => isStackable;
    public override bool IsConsumable => true;
    public override int MaxStackAmount => maxStackAmount;
    
    public float HydrationAmount => hydrationAmount;
    public float EnergyAmount => energyAmount;
}
