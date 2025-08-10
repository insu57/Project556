using UnityEngine;

[CreateAssetMenu(fileName = "CrateData", menuName = "Scriptable Objects/CrateData")]
public class CrateData : ScriptableObject
{
    [SerializeField] private string crateDataID; //상자별 ID
    [SerializeField] private string crateName;
    [SerializeField] private Inventory inventoryPrefab; //Crate Inventory
    [SerializeField] private Sprite crateSprite; //필드 스프라이트
    //Type 구현 필요(무기상자 등)
    public string CrateDataID => crateDataID;
    public string CrateName => crateName;
    public Inventory InventoryPrefab => inventoryPrefab;
    public Sprite CrateSprite => crateSprite;
}
