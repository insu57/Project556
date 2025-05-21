using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    private InventoryItem _headGear;
    private InventoryItem _bodyArmor;
    private InventoryItem _chestRig;
    private InventoryItem _primaryWeapon;
    private InventoryItem _secondaryWeapon;
    private InventoryItem _backpack;
    
    private UIManager _uiManager;


    public void Init(UIManager uiManager)
    {
        _uiManager = uiManager;
    }
    
    private void Awake()
    {
        
    }
}
