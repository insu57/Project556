using System;
using System.Collections.Generic;
using Player;
using UnityEngine;

namespace UI
{
    public class PlayerUIPresenter : MonoBehaviour
    {
        private PlayerUIManager _playerUIManager;
        private PlayerManager _playerManager;
        private PlayerData _playerData;
        
        private void Awake()
        {
            TryGetComponent(out _playerManager);
            _playerUIManager = FindFirstObjectByType<PlayerUIManager>();
            TryGetComponent(out _playerData);
        }

        private void OnEnable()
        {
            //_playerManager.OnPlayerHealthChanged += HandleOnUpdateHealthBar;
            _playerData.OnPlayerHealthChanged += HandleOnUpdateHealthBar;
            _playerManager.OnUpdateMagazineCountUI += HandleOnUpdateMagazineCountUI;
            _playerManager.OnShowItemPickup += HandleOnShowItemPickup;
            _playerManager.OnScrollItemPickup += HandleOnScrollItemPickup;
            _playerManager.OnHideItemPickup += HandleOnHideItemPickup;
            _playerManager.OnReloadNoAmmo += HandleOnReloadNoAmmo;
        }

        private void OnDisable()
        {
            //_playerManager.OnPlayerHealthChanged -= HandleOnUpdateHealthBar;
            _playerData.OnPlayerHealthChanged -= HandleOnUpdateHealthBar;
            _playerManager.OnUpdateMagazineCountUI -= HandleOnUpdateMagazineCountUI;
            _playerManager.OnShowItemPickup -= HandleOnShowItemPickup;
            _playerManager.OnScrollItemPickup -= HandleOnScrollItemPickup;
            _playerManager.OnHideItemPickup -= HandleOnHideItemPickup;
            _playerManager.OnReloadNoAmmo -= HandleOnReloadNoAmmo;
        }

        private void HandleOnUpdateHealthBar(float health, float maxHealth)
        {
            _playerUIManager.UpdateHealthBar(health, maxHealth);
        }

        private void HandleOnUpdateMagazineCountUI(bool isFullyLoaded, int ammo)
        {
            _playerUIManager.UpdateAmmoText(isFullyLoaded, ammo);
        }

        private void HandleOnShowItemPickup(Vector2 pos, List<(bool available, ItemInteractType type)> availableList)
        {
            _playerUIManager.ShowItemPickup(pos, availableList);
        }

        private void HandleOnScrollItemPickup(int idx)
        {
            _playerUIManager.ScrollItemPickup(idx);
        }
    
        private void HandleOnHideItemPickup()
        {
            _playerUIManager.HideItemPickup();
        }

        private void HandleOnReloadNoAmmo()
        {
            _playerUIManager.ShowNoAmmoWarning();
        }
    }
}
