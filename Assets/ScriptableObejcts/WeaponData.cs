using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    [SerializeField] private GunType gunType;
    [SerializeField] private AmmoType ammoType;
    [SerializeField] private int defaultMagazineSize;
    [SerializeField] private int damage;
    [SerializeField] private bool isAutomatic = false;
    [SerializeField] private float fireRate;
    
    public GunType GunType => gunType;
    public AmmoType AmmoType => ammoType;
    public int DefaultMagazineSize => defaultMagazineSize;
    public int Damage => damage;
    public bool IsAutomatic => isAutomatic;
    public float FireRate => fireRate;
}
