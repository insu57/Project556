using System;
using UnityEngine;

public class CellData
{
    public bool IsEmpty { private set; get; }
    public Vector2 MinPos { get; } 
    public Guid InstanceID { private set; get; }
    public bool IsGearSlot { private set; get; }
    public GearType GearType { private set; get; }

    public CellData(GearType gearType, Vector2 minPos)
    {
        IsEmpty = true;
        InstanceID = Guid.Empty;
        IsGearSlot = gearType != GearType.None;//none이면 일반 Cell
        GearType = gearType;
        MinPos = minPos;
    }

    public void SetEmpty(bool isEmpty, Guid id)
    {
        IsEmpty = isEmpty;
        InstanceID = id;
    }
}
