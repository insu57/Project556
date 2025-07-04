using UnityEngine;

[CreateAssetMenu(fileName = "MedicalData", menuName = "Scriptable Objects/MedicalData")]
public class MedicalData : BaseItemDataSO
{
    [SerializeField] private string itemID;
    [SerializeField] private string itemName;
    
    [SerializeField] private int healAmount;
    
    [SerializeField] private Sprite itemSprite;
   
    [SerializeField] private int itemWidth;
    [SerializeField] private int itemHeight;

    [SerializeField] private bool isStackable = false;
    [SerializeField] private int maxStackAmount = 1;
    
    public override string ItemDataID => itemID;
    public override string ItemName => itemName;
    public int HealAmount => healAmount;
    public override Sprite ItemSprite => itemSprite;
    public override int ItemWidth => itemWidth;
    public override int ItemHeight => itemHeight;
    public override GearType GearType => GearType.None;
    public override bool IsStackable => isStackable;
    public override int MaxStackAmount => maxStackAmount;
}
