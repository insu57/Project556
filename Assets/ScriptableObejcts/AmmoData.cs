using UnityEngine;

[CreateAssetMenu(fileName = "AmmoData", menuName = "Scriptable Objects/AmmoData")]
public class AmmoData : ScriptableObject
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private AmmoCaliber ammoCaliber;
    [SerializeField] private AmmoCategory ammoCategory;
    //ammoPiercing~
    public AmmoCaliber AmmoCaliber => ammoCaliber;
    public AmmoCategory AmmoCategory => ammoCategory;
    public GameObject BulletPrefab => bulletPrefab;
}
