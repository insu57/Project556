using UnityEngine;

public class WeaponInstance : ItemInstance
{
    public WeaponData WeaponData { get; }
    public int CurrentMagazineCount { get; private set; }
    public int MaxMagazineCount { get; private set; }
    //public bool IsFullyLoaded
    public bool HasChamber {get; private set;}
    public FireMode CurrentFireMode { get; private set; }
    private int _fireModeIdx;
    public AmmoData CurrentAmmoData { get; private set; }//탄종 시스템 진행중
    public AmmoCategory AmmoCategory => EnumManager.GetAmmoCategory(WeaponData.AmmoCaliber);
    public WeaponInstance( WeaponData weaponData) : base(weaponData)
    {
        WeaponData = weaponData;
        var ammoIdx = Random.Range(0,weaponData.DefaultAmmoData.Length);
        CurrentAmmoData = weaponData.DefaultAmmoData[ammoIdx];
        MaxMagazineCount = weaponData.DefaultMagazineSize;

        CurrentFireMode = weaponData.FireModes[0]; 
        _fireModeIdx = 0;//기본 FireMode
    }

    public void SetMagazineCount(int magazineCount)
    {
        CurrentMagazineCount = magazineCount;
    }
    
    public void UseAmmo() 
    {
        CurrentMagazineCount--;
    }

    public void ReloadAmmo(AmmoData ammoData, int reloadAmmo) //주머니 탄 소모...
    {
        CurrentAmmoData = ammoData;
        CurrentMagazineCount = reloadAmmo;
    }

    public bool IsFullyLoaded() //클로즈드 볼트일 때 탄약최대+약실 
    {
        if (WeaponData.IsOpenBolt) //오픈볼트면 약실에 없음
        {
            return CurrentMagazineCount >= MaxMagazineCount;
        }
    
        return CurrentMagazineCount > MaxMagazineCount; //총알이 없으면 약실에 없음
    }

    public void ToggleFireMode()
    {
        _fireModeIdx++;
        if(_fireModeIdx >= WeaponData.FireModes.Length) _fireModeIdx = 0;
        CurrentFireMode = WeaponData.FireModes[_fireModeIdx];
    }
}
