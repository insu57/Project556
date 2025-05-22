using System;
using UnityEngine;

public struct SlotData
{
    public bool IsEmpty;
    public readonly Vector2 MinPosition;
    public readonly Vector2 MaxPosition;
    public readonly Vector2 ImagePosition;
    public Guid Id;
    public bool IsGearSlot;
    public GearType GearType;
    
    public SlotData( Vector2 minPosition, Vector2 maxPosition, Vector2 imagePosition )
    {
        IsEmpty = true;
        MinPosition = minPosition;
        MaxPosition = maxPosition;
        ImagePosition = imagePosition;
        Id = Guid.Empty;
        IsGearSlot = false;
        GearType = GearType.None;
    }
}
