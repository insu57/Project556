using System;
using Player;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class UIControl : MonoBehaviour
{
    private PlayerControl _playerControl;
    private PlayerInput _playerInput;
    private ItemUIManager _itemUIManager;
    private SettingUIManager _settingUIManager;
    
    private InputAction _closeAction;
    private InputAction _closeUIAction;

    public InputAction ItemRotateAction {private set; get;}
    public InputAction QuickAddItemAction {private set; get;}
    public InputAction QuickDropItemAction {private set; get;}
    public InputAction SetQuickSlotAction {private set; get;}
    
    private void Awake()
    {
        TryGetComponent(out _itemUIManager);
        TryGetComponent(out _settingUIManager);
    }
    
    public void Init(PlayerControl playerControl) //개선?
    {
        _playerControl = playerControl;
        _playerInput = _playerControl.GetComponent<PlayerInput>();
        var map = _playerInput.actions.FindActionMap("UI");
        
        _closeAction = map.FindAction("Close");
        _closeUIAction = map.FindAction("CloseUI");
        ItemRotateAction = map.FindAction("Rotate");
        QuickAddItemAction = map.FindAction("QuickAddItem");
        QuickDropItemAction = map.FindAction("QuickDropItem");
        SetQuickSlotAction = map.FindAction("SetQuickSlot");
        
        _closeAction.performed += OnClose;
        _closeUIAction.performed += OnCloseUI;
    }
    
    private void OnEnable()
    {
        if (_playerControl)
        {
            _closeAction.performed += OnClose;
            _closeUIAction.performed += OnCloseUI;
        }
    }

    private void OnDisable()
    {
        _closeAction.performed -= OnClose;
        _closeUIAction.performed -= OnCloseUI;
    }   
    
    //개선?
    public void OnOpenPlayerUI()
    {
        _itemUIManager.OpenPlayerUI(true);
    }

    public void OnOpenSettingsUI()
    {
        _settingUIManager.OpenSettingUI(true);
    }
    
    private void OnClose(InputAction.CallbackContext context) //esc
    {
        //_itemUIManager.OpenPlayerUI(false);
        _settingUIManager.OpenSettingUI(false);
        _playerControl.BlockControl(false);
        
    }

    private void OnCloseUI(InputAction.CallbackContext context) //Tab key
    {
        _itemUIManager.OpenPlayerUI(false);
        _playerControl.BlockControl(false);
    }

    
}
