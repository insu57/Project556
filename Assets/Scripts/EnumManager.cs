using UnityEngine;

public enum SlotStatus
{
    None = 0,
    Available,
    Unavailable,
}

public enum ItemInteractType
{
    PickUp = 0,
    Equip,
    Use,
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

public enum CurrentWeaponIdx
{
    Unarmed = 0,
    Primary,
    Secondary,
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
}
