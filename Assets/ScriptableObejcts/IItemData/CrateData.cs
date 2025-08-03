using UnityEngine;

[CreateAssetMenu(fileName = "CrateData", menuName = "Scriptable Objects/CrateData")]
public class CrateData : BaseCrateDataSO
{
    [SerializeField] private string crateDataID;
    [SerializeField] private string crateName;
    [SerializeField] private Inventory inventoryPrefab;
    [SerializeField] private Sprite crateSprite;
    //Type?
    public override string CrateDataID => crateDataID;
    public override string CrateName => crateName;
    public override Inventory InventoryPrefab => inventoryPrefab;
    public override Sprite CrateSprite => crateSprite;
}
