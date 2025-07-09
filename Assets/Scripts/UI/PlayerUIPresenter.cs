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
        
        _playerManager.OnPlayerHealthChanged += HandleOnUpdateHealthBar;
        _playerManager.OnUpdateMagazineCount += HandleOnUpdateMagazineCount;
    }

    private void OnDisable()
    {
        
    }

    private void HandleOnUpdateHealthBar(float health, float maxHealth)
    {
        _playerUIManager.UpdateHealthBar(health, maxHealth);
    }

    private void HandleOnUpdateMagazineCount(bool isFullyLoaded, int ammo)
    {
        _playerUIManager.UpdateAmmoText(isFullyLoaded, ammo);
    }
    
}
