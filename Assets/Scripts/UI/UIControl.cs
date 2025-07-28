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
    private UIManager _uiManager;
    private SettingUI _settingUI;
    
    private InputAction _settingAction;
    private InputAction _closeUIAction;

    public InputAction ItemRotateAction {private set; get;}
    public InputAction QuickAddItemAction {private set; get;}
    public InputAction QuickDropItemAction {private set; get;}
    public InputAction SetQuickSlotAction {private set; get;}
    
    private void Awake()
    {
        TryGetComponent(out _uiManager);
        TryGetComponent(out _settingUI);
    }
    
    public void Init(PlayerControl playerControl) //개선?
    {
        _playerControl = playerControl;
        _playerInput = _playerControl.GetComponent<PlayerInput>();
        var map = _playerInput.actions.FindActionMap("UI");
        
        _settingAction = map.FindAction("Setting");
        _closeUIAction = map.FindAction("CloseUI");
        ItemRotateAction = map.FindAction("Rotate");
        QuickAddItemAction = map.FindAction("QuickAddItem");
        QuickDropItemAction = map.FindAction("QuickDropItem");
        SetQuickSlotAction = map.FindAction("SetQuickSlot");
        
        _settingAction.performed += OnSettingUI;
        _closeUIAction.performed += OnClosePlayerUI;
    }
    
    private void OnEnable()
    {
        if (_playerControl)
        {
            _settingAction.performed += OnSettingUI;
            _closeUIAction.performed += OnClosePlayerUI;
        }
    }

    private void OnDisable()
    {
        _settingAction.performed -= OnSettingUI;
        _closeUIAction.performed -= OnClosePlayerUI;
    }   
    
    //개선?
    public void OnOpenPlayerUI()
    {
        if(_settingUI.SettingUIOpen) return;//SettingUI가 켜졌다면 return
        _uiManager.OpenPlayerUI(true);
    }

    public void OnOpenSettingsUI()
    {
        _settingUI.OpenSettingUI(true);
    }
    
    private void OnSettingUI(InputAction.CallbackContext context) //esc
    {
        _settingUI.OpenSettingUI(!_settingUI.SettingUIOpen);
        if(_uiManager.PlayerMenuOpen) return; //PlayerMenu가 켜졌다면 
        _playerControl.BlockControl(false);
        
    }

    private void OnClosePlayerUI(InputAction.CallbackContext context) //Tab key
    {
        if(!_uiManager.PlayerMenuOpen) return;
        _uiManager.OpenPlayerUI(false);
        _playerControl.BlockControl(false);
    }

    
}
