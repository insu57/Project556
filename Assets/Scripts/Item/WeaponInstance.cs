using UnityEngine;

public class WeaponInstance : ItemInstance
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public WeaponData WeaponData { get; }
    public int CurrentMagazineCount { get; private set; }
    private int MaxMagazineCount { get; }
    public WeaponInstance( WeaponData weaponData) : base(weaponData)
    {
        WeaponData = weaponData;
        CurrentMagazineCount = weaponData.DefaultMagazineSize;
        MaxMagazineCount = weaponData.DefaultMagazineSize;
    }
    public void UseAmmo() 
    {
        CurrentMagazineCount--;
    }

    public void ReloadAmmo() //주머니 탄 소모...
    {
        if (WeaponData.IsOpenBolt)
            CurrentMagazineCount = MaxMagazineCount;
        else CurrentMagazineCount = MaxMagazineCount + 1;
    }
}
