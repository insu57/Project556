using System;
using UnityEngine;

public class PlayerUIPresenter : MonoBehaviour
{
    private PlayerUIManager _playerUIManager;
    private PlayerManager _playerManager;
    private PlayerWeapon _playerWeapon;

    private void Awake()
    {
        TryGetComponent(out _playerManager);
        _playerUIManager = FindFirstObjectByType<PlayerUIManager>();
        TryGetComponent(out _playerWeapon);
        
        _playerManager.OnPlayerHealthChanged += HandleOnUpdateHealthBar;
        _playerManager.OnUpdateMagazineCount += HandleOnUpdateMagazineCount;
    }

    private void HandleOnUpdateHealthBar(float health, float maxHealth)
    {
        _playerUIManager.UpdateHealthBar(health, maxHealth);
    }

    private void HandleOnUpdateMagazineCount(int ammo)
    {
        _playerUIManager.UpdateAmmoText(ammo);
    }
    
}
