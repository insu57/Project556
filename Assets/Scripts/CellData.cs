using System;
using UnityEngine;

public class CellData
{
    public bool IsEmpty;
    public RectTransform CellRT;
    public readonly Vector2 MinPosition;
    public readonly Vector2 MaxPosition;
    public readonly Vector2 ImagePosition;
    public Guid Id;
    public bool IsGearSlot;
    public GearType GearType;
    
    public CellData( Vector2 minPosition, Vector2 maxPosition, Vector2 imagePosition )
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
