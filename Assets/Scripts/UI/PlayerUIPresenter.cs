using System;
using UnityEngine;

public class PlayerUIPresenter : MonoBehaviour
{
    private PlayerUIManager _playerUIManager;
    private PlayerManager _playerManager;

    private void Awake()
    {
        TryGetComponent(out _playerManager);
        _playerUIManager = FindFirstObjectByType<PlayerUIManager>();
        
        
    }
    
}
