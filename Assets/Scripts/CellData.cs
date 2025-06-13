using System;
using UnityEngine;

public class CellData
{
    private Vector2 CellOffset => new Vector2(CellRT.sizeDelta.x, -CellRT.sizeDelta.y) / 2;
    public bool IsEmpty { private set; get; }
    public RectTransform CellRT { get; private set; }
    //public Vector2 MinPosition => CellRT.anchoredPosition - CellOffset;
    //public Vector2 MaxPosition => CellRT.anchoredPosition + CellOffset; 
    //public Vector2 ImagePosition => CellRT.position + CellOffset;
    public Guid Id { private set; get; }
    public bool IsGearSlot { private set; get; }
    public GearType GearType { private set; get; }

    public CellData(GearType gearType)
    {
        IsEmpty = true;
        //CellRT = cellRT;
        Id = Guid.Empty;
        IsGearSlot = gearType != GearType.None;//none이면 일반 Cell
        GearType = gearType;
    }

    public void SetCellRT(RectTransform cellRT)
    {
        CellRT = cellRT;
    }

    public void SetEmpty(bool isEmpty, Guid id)
    {
        IsEmpty = isEmpty;
        Id = id;
    }
}
