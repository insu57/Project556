using System;
using UnityEngine;

public class CellData
{
    private Vector2 CellOffset => new Vector2(CellRT.sizeDelta.x, -CellRT.sizeDelta.y) / 2;
    public bool IsEmpty { private set; get; }
    public RectTransform CellRT { private set; get; }
    public Vector2 MinPosition => CellRT.anchoredPosition - CellOffset;
    public Vector2 MaxPosition => CellRT.anchoredPosition + CellOffset;
    public Vector2 ImagePosition => CellRT.anchoredPosition ;
    public Guid Id { private set; get; }
    public bool IsGearSlot { private set; get; }
    public GearType GearType { private set; get; }

    public CellData( RectTransform cellRT )
    {
        IsEmpty = true;
        CellRT = cellRT;
        Id = Guid.Empty;
        IsGearSlot = false;
        GearType = GearType.None;
    }

    public void SetGearSlot(GearType gearType)
    {
        IsGearSlot = true;
        GearType = gearType;
    }

    public void SetEmpty(bool isEmpty, Guid id)
    {
        IsEmpty = isEmpty;
        Id = id;
    }
}
