using UnityEngine;

public class WeaponInstance : ItemInstance
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public WeaponData WeaponData { get; }
    public int CurrentMagazineCount { get; private set; }
    private int MaxMagazineCount { get; }
    public bool HasChamber {get; private set;}
    public WeaponSelector WeaponSelector  {get; private set;}
    public WeaponInstance( WeaponData weaponData) : base(weaponData)
    {
        WeaponData = weaponData;
        
        MaxMagazineCount = weaponData.DefaultMagazineSize;

        WeaponSelector = WeaponSelector.Single;
    }

    public void SetMagazineCount(int magazineCount)
    {
        CurrentMagazineCount = magazineCount;
    }
    
    public void UseAmmo() 
    {
        CurrentMagazineCount--;
    }

    public void ReloadAmmo(int reloadAmmo) //주머니 탄 소모...
    {
        CurrentMagazineCount = reloadAmmo;
    }

    public bool IsFullyLoaded() //클로즈드 볼트일 때 탄약최대+약실 
    {
        if (WeaponData.IsOpenBolt) //오픈볼트면 약실에 없음
        {
            return false;
        }
    
        return CurrentMagazineCount > MaxMagazineCount; //총알이 없으면 약실에 없음
    }

    public void ChangeSelector(WeaponSelector selector)
    {
        WeaponSelector = selector; //수정?
    }
}
