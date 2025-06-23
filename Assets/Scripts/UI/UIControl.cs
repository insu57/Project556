using System;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class UIControl : MonoBehaviour
{
    private PlayerControl _playerControl;
    private PlayerInput _playerInput;
    private UIManager _uiManager;
    
    private InputAction _closeAction;
    private InputAction _closeUIAction;

    public InputAction ItemRotateAction {private set; get;}
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
        ItemRotateAction = map.FindAction("Rotate");
        var testAction = map.FindAction("Ctrl Test");
        testAction.performed += OnTest;
        
        _closeAction.performed += OnClose;
        _closeUIAction.performed += OnCloseUI;
        
        //_playerInput.inputIsActive
    }

    private void OnTest(InputAction.CallbackContext context)
    {
        Debug.Log("L Ctrl");
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
    public void OnOpenUI()
    {
        _uiManager.OpenPlayerUI(true);
    }
    
    private void OnClose(InputAction.CallbackContext context) //esc
    {
        _uiManager.OpenPlayerUI(false);
        _playerControl.BlockControl(false);
        
    }

    private void OnCloseUI(InputAction.CallbackContext context) //Tab key
    {
        _uiManager.OpenPlayerUI(false);
        _playerControl.BlockControl(false);
    }

    
}
