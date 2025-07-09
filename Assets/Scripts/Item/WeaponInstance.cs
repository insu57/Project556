using UnityEngine;

public class WeaponInstance : ItemInstance
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public WeaponData WeaponData { get; }
    public int CurrentMagazineCount { get; private set; }
    private int MaxMagazineCount { get; }
    public bool HasChamber {get; private set;}
    public WeaponInstance( WeaponData weaponData) : base(weaponData)
    {
        WeaponData = weaponData;
        
        MaxMagazineCount = weaponData.DefaultMagazineSize;
    }

    public void SetMagazineCount(int magazineCount)
    {
        CurrentMagazineCount = magazineCount;
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

    public bool IsFullyLoaded()
    {
        if (WeaponData.IsOpenBolt) //오픈볼트면 약실에 없음
        {
            return false;
        }

        return CurrentMagazineCount > WeaponData.DefaultMagazineSize; //총알이 없으면 약실에 없음
    }
}
