using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "AmmoData", menuName = "Scriptable Objects/AmmoData")]
public class AmmoData : BaseItemDataSO
{
    [SerializeField] private AmmoCaliber ammoCaliber;
    [SerializeField] private AmmoCategory ammoCategory;

    [SerializeField] private string itemID;
    [SerializeField] private string itemName;
    [SerializeField] private Sprite itemSprite;

    [SerializeField] private int itemWidth;
    [SerializeField] private int itemHeight;
    [SerializeField] private int maxStackAmount;
    
    [SerializeField] private float ammoDamage;
    [SerializeField] private float ammoPiercing;
    [SerializeField] private float velocityModify; //탄속 보정치

    public AmmoCaliber AmmoCaliber => ammoCaliber;
    public AmmoCategory AmmoCategory => ammoCategory;

    
    public override string ItemDataID => itemID;
    public override string ItemName => itemName;
    public override Sprite ItemSprite => itemSprite;
    public override int ItemWidth => itemWidth;
    public override int ItemHeight => itemHeight;
    public override GearType GearType => GearType.None;
    public override bool IsStackable => true;
    public override int MaxStackAmount => maxStackAmount;
    
    public float AmmoDamage => ammoDamage;
    public float AmmoPiercing => ammoPiercing;
    public float VelocityModify => velocityModify;
}
