using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "AmmoData", menuName = "Scriptable Objects/AmmoData")]
public class AmmoData : BaseItemDataSO
{
    [SerializeField] private AmmoCaliber ammoCaliber; //탄 구경
    [SerializeField] private AmmoCategory ammoCategory; //탄 분류
    
    [SerializeField] private string itemID; 
    [SerializeField] private string itemName;
    [SerializeField] private Sprite itemSprite;

    [SerializeField] private int itemWidth = 1; //아이템 크기 
    [SerializeField] private int itemHeight = 1;
    [SerializeField] private float itemWeight; //아이템 무게(구현 필요)
    [SerializeField] private int maxStackAmount = 60; 
    
    [SerializeField] private float ammoDamage; //탄 피해량
    [SerializeField] private float ammoPiercing; //방어 관통력
    [SerializeField] private float velocityModify; //탄속 보정치
    [SerializeField] private bool isBuckshot; //벅샷(여러 펠릿 발사)
    [SerializeField] private int pelletCount = 1; //펠릿 숫자
    
    public AmmoCaliber AmmoCaliber => ammoCaliber;
    public AmmoCategory AmmoCategory => ammoCategory;

    
    public override string ItemDataID => itemID;
    public override string ItemName => itemName;
    public override Sprite ItemSprite => itemSprite;
    public override int ItemWidth => itemWidth;
    public override int ItemHeight => itemHeight;
    public override GearType GearType => GearType.None;
    public override float ItemWeight => itemWeight;
    public override bool IsStackable => true;
    public override int MaxStackAmount => maxStackAmount;
    
    public float AmmoDamage => ammoDamage;
    public float AmmoPiercing => ammoPiercing;
    public float VelocityModify => velocityModify;
    public bool IsBuckshot => isBuckshot;
    public int PelletCount => pelletCount;
    
}
