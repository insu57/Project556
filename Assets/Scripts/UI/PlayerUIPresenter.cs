using System;
using System.Collections.Generic;
using Player;
using UnityEngine;

namespace UI
{
    public class PlayerUIPresenter : MonoBehaviour
    {
        private PlayerUI _playerUI;
        private PlayerManager _playerManager;
        private PlayerData _playerData;
        private PlayerInteract _playerInteract;
        
        private void Awake()
        {
            TryGetComponent(out _playerManager);
            _playerUI = FindFirstObjectByType<PlayerUI>();
            TryGetComponent(out _playerData);
        }

        private void OnEnable()
        {
            _playerData.OnPlayerStatChanged += HandleOnUpdatePlayerStat;
            
            _playerManager.OnUpdateMagazineCountUI += HandleOnUpdateMagazineCountUI;
            _playerManager.OnReloadNoAmmo += HandleOnReloadNoAmmo;
            _playerManager.OnToggleFireMode += HandleOnToggleFireMode;
            _playerManager.OnShowAmmoIndicator += HandleOnShowAmmoIndicator;
        }

        private void OnDisable()
        {
            _playerData.OnPlayerStatChanged -= HandleOnUpdatePlayerStat;
         
            _playerManager.OnUpdateMagazineCountUI -= HandleOnUpdateMagazineCountUI;
            _playerManager.OnReloadNoAmmo -= HandleOnReloadNoAmmo;
            _playerManager.OnToggleFireMode -= HandleOnToggleFireMode;
            _playerManager.OnShowAmmoIndicator -= HandleOnShowAmmoIndicator;
        }
        
        private void HandleOnUpdatePlayerStat(PlayerStat stat, (float current, float max) amount)
        {
            switch (stat)
            {
                case  PlayerStat.Health:
                    _playerUI.UpdateHealthUI(amount.current, amount.max);
                    break;
                case PlayerStat.Stamina:
                    _playerUI.UpdateStaminaUI(amount.current, amount.max);
                    break;
                case PlayerStat.Energy:
                    _playerUI.UpdateEnergyUI(amount.current, amount.max);
                    break;
                case PlayerStat.Hydration:
                    _playerUI.UpdateHydrationUI(amount.current, amount.max);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stat), stat, null);
            }
        }

        private void HandleOnUpdateMagazineCountUI(bool isFullyLoaded, int ammo)
        {
            _playerUI.UpdateAmmoText(isFullyLoaded, ammo);
        }
        
        private void HandleOnReloadNoAmmo()
        {
            _playerUI.ShowNoAmmoWarning();
        }

        private void HandleOnToggleFireMode(AmmoCategory ammoCategory, FireMode fireMode)
        {
            _playerUI.ToggleFireModeImage(ammoCategory, fireMode);
        }

        private void HandleOnShowAmmoIndicator(bool isShow)
        {
            _playerUI.ShowAmmoIndicator(isShow);
        }
    }
}
