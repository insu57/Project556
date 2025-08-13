using System;
using UnityEngine;

public class CellData
{
    public bool IsEmpty { private set; get; }
    public Vector2 CellLocalPos { get; } 
    public Guid InstanceID { private set; get; }
    public GearType GearType { private set; get; }

    public CellData(GearType gearType, Vector2 cellLocalPos)
    {
        IsEmpty = true;
        InstanceID = Guid.Empty;
        GearType = gearType;
        CellLocalPos = cellLocalPos; 
    }

    public void SetEmpty(bool isEmpty, Guid id)
    {
        IsEmpty = isEmpty;
        InstanceID = id;
    }
}
