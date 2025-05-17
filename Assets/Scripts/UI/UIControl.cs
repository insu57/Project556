using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIControl : MonoBehaviour
{
    private PlayerControl _playerControl;
    private PlayerInput _playerInput;
    private UIManager _uiManager;
    
    private InputAction _closeAction;
    private InputAction _closeUIAction;
    private void Awake()
    {
        _uiManager = GetComponent<UIManager>();
    }
    
    public void Init(PlayerControl playerControl)
    {
        _playerControl = playerControl;
        _playerInput = _playerControl.GetComponent<PlayerInput>();
        
        var map = _playerInput.actions.FindActionMap("UI");
        _closeAction = map.FindAction("Close");
        _closeUIAction = map.FindAction("CloseUI");
        
        _closeAction.performed += OnClose;
        _closeUIAction.performed += OnCloseUI;
        _closeAction.Enable();
        _closeUIAction.Enable();
    }

    private void OnEnable()
    {
        if (_playerControl)
        {
            _closeAction.performed += OnClose;
            _closeUIAction.performed += OnCloseUI;
            _closeAction.Enable();
            _closeUIAction.Enable();
        }
    }

    private void OnDisable()
    {
        _closeAction.performed -= OnClose;
        _closeUIAction.performed -= OnCloseUI;
        _closeAction.Disable();
        _closeUIAction.Disable();
    }

    public void OnOpenUI()
    {
        _uiManager.OpenPlayerUI(true);
        Debug.Log("OnOpenUI - UIControl");
        Debug.Log(_playerInput.currentActionMap);
    }
    
    private void OnClose(InputAction.CallbackContext context)
    {
        _uiManager.OpenPlayerUI(false);
        _playerControl.BlockControl(false);
        Debug.Log("OnCloseUI");
    }

    private void OnCloseUI(InputAction.CallbackContext context) //Tab key
    {
        //
    }
    
}
