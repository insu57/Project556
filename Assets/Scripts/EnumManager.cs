using UnityEngine;

public enum SlotStatus
{
    None = 0,
    Available,
    Unavailable,
}

public enum InteractType
{
    PickUp = 0,
    Equip,
    Use,
    Open
}

public enum ItemContextType
{
    Info = 0,
    Use,
    Equip,
    Drop
}

public enum WeaponType
{
    Pistol = 0,
    Shotgun,
    SMG,
    AR,
    DMR,
    SR,
    Melee,
    Unarmed,
}

public enum AmmoCaliber
{
    _556x45mm = 0,
    _762x51mm,
    _9x19mm,
    _45Acp,
    _762x39mm,
    _545x39mm,
    _762x54mmR,
    _50BMG,
    _338Lapua,
    _46x30mm, //HK 4.6x30mm MP7 
    _57x28mm, //FN 5.7x28mm P90/FN57
    _12Gauge,
        
}

public enum AmmoCategory
{
    Pistol,
    Rifle,
    Buckshot,
}

public enum GearType
{
    None,
    Backpack,
    BodyArmor,
    ArmoredRig,
    UnarmoredRig,
    HeadWear,
    EyeWear,
    Weapon,
}

public enum EquipWeaponIdx
{
    Unarmed = 0,
    Primary,
    Secondary,
}

public enum QuickSlotIdx
{
    QuickSlot4 = 4,
    QuickSlot5, 
    QuickSlot6,
    QuickSlot7 = 7
}

public enum PlayerStat
{
    Health,
    Stamina,
    Energy,
    Hydration,
}

public enum AudioType
{
    Master, BGM, SFX, //Voice?...
}

public enum SFXType
{
    Footstep, Weapon, UI, //...
}

public enum SFX //개선 -> json 파일로 저장하고 불러오기?
{
    None, FootstepDirt, FootstepRock, 
    PistolShoot, ARShoot, SgShoot ,PistolReload, ARReload, SgReloadNoMag,SgPump ,Selector,
    SRShoot, SMGShoot
}

public enum FootStepSFX
{
    None,FootstepDirt, FootstepRock
}

public enum WeaponSFX
{
    None, PistolShoot, ARShoot, SgShoot ,PistolReload, ARReload, SgReloadNoMag,SgPump ,Selector
}

public enum UiSFX
{
    None,
}

public enum BGM
{
    //BGM
    ForestEnvironment,
}

public enum FireMode
{
    SemiAuto,
    _2Burst,
    _3Burst,
    FullAuto
}

public enum WeaponActionType
{
    Automatic,
    PumpAction,
    BoltAction,
    //...
}

public class EnumManager : MonoBehaviour
{
    public static AmmoCategory GetAmmoCategory(AmmoCaliber caliber)
    {
        switch (caliber)
        {
            case AmmoCaliber._556x45mm:
            case AmmoCaliber._762x51mm:
            case AmmoCaliber._545x39mm:
            case AmmoCaliber._762x39mm:
            case AmmoCaliber._762x54mmR:    
                return AmmoCategory.Rifle;
            case AmmoCaliber._9x19mm:
            case AmmoCaliber._45Acp:
            case AmmoCaliber._46x30mm:
            case AmmoCaliber._57x28mm:
                return AmmoCategory.Pistol;
            case AmmoCaliber._12Gauge:
                return AmmoCategory.Buckshot;
            default:
                return AmmoCategory.Rifle;
        }
    }

    public static string AmmoCaliberToString(AmmoCaliber caliber)
    {
        switch (caliber)
        {
            case AmmoCaliber._556x45mm: return "5.56x45mm";
            case AmmoCaliber._762x51mm: return "7.62x51mm";
            case AmmoCaliber._545x39mm: return "5.45x39mm";
            case AmmoCaliber._762x39mm: return "7.62x39mm";
            case AmmoCaliber._762x54mmR: return "7.62x54mmR";
            case AmmoCaliber._50BMG: return ".50BMG";
            case AmmoCaliber._9x19mm: return "9x19mm";
            case AmmoCaliber._45Acp: return ".45Acp";
            case AmmoCaliber._46x30mm: return "4.6x30mm";
            case AmmoCaliber._57x28mm: return "5.7x28mm";
            case AmmoCaliber._12Gauge: return "12Gauge";
        }

        return "None";
    }
}
