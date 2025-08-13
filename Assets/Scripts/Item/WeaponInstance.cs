using UnityEngine;

public class WeaponInstance : ItemInstance //무기 아이템 인스턴스
{
    public WeaponData WeaponData { get; }
    public int CurrentMagazineCount { get; private set; }
    public int MaxMagazineCount { get; private set; }
    public bool HasChamber {get; private set;} //챔버(약실) 관련 미구현.(장전세분화)
    public FireMode CurrentFireMode { get; private set; } //현재 발사모드(조정간)
    private int _fireModeIdx; //무기 조정간 인덱스
    public AmmoData CurrentAmmoData { get; private set; }//탄종 시스템 진행중(구경별 탄종)
    public AmmoCategory AmmoCategory => EnumManager.GetAmmoCategory(WeaponData.AmmoCaliber); //탄 구경
    public WeaponInstance( WeaponData weaponData) : base(weaponData)
    {
        WeaponData = weaponData;
        var ammoIdx = Random.Range(0,weaponData.DefaultAmmoData.Length); //초기 총알 데이터
        CurrentAmmoData = weaponData.DefaultAmmoData[ammoIdx];
        MaxMagazineCount = weaponData.DefaultMagazineSize; //탄창크기

        CurrentFireMode = weaponData.FireModes[0]; //초기 조정간
        _fireModeIdx = 0;//기본 FireMode
    }

    public void SetMagazineCount(int magazineCount)
    {
        CurrentMagazineCount = magazineCount;
    }
    
    public void UseAmmo() //탄 사용
    {
        CurrentMagazineCount--;
    }

    public void ReloadAmmo(AmmoData ammoData, int reloadAmmo) //장전. //탄 교체
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
    
        return CurrentMagazineCount > MaxMagazineCount; //(탄창크기+1 -> 최대)
    }

    public int GetAmmoToRefill() //장전에 사용될 탄약량
    {
        if (!WeaponData.HasDetachableMagazine) //내부 탄창
        {
            return 1; //내부 탄창(관형탄창 등)이라면 1발씩
        }
        return NeedAmmo();
    }

    public int NeedAmmo() //최대치까지 부족한 탄약량
    {
        if (WeaponData.IsOpenBolt)
        {
            return MaxMagazineCount -  CurrentMagazineCount;
        }
        
        if (CurrentMagazineCount == 0) return MaxMagazineCount;
        return MaxMagazineCount - CurrentMagazineCount + 1;//약실 한 발
    }
    
    public void ToggleFireMode() //발사모드 변경.
    {
        _fireModeIdx++; //인덱스 증가
        if(_fireModeIdx >= WeaponData.FireModes.Length) _fireModeIdx = 0; //넘으면 초기화
        CurrentFireMode = WeaponData.FireModes[_fireModeIdx]; //현재 발사모드
    }
}
