using System.Collections.Generic;
using Player;
using UnityEngine;

namespace UI
{
    public class PlayerUIPresenter : MonoBehaviour
    {
        private PlayerUIManager _playerUIManager;
        private PlayerManager _playerManager;

        private void Awake()
        {
            TryGetComponent(out _playerManager);
            _playerUIManager = FindFirstObjectByType<PlayerUIManager>();
        
            _playerManager.OnPlayerHealthChanged += HandleOnUpdateHealthBar;
            _playerManager.OnUpdateMagazineCountUI += HandleOnUpdateMagazineCountUI;
            _playerManager.OnShowItemPickup += HandleOnShowItemPickup;
            _playerManager.OnScrollItemPickup += HandleOnScrollItemPickup;
            _playerManager.OnHideItemPickup += HandleOnHideItemPickup;
        }

        private void OnDisable()
        {
            _playerManager.OnPlayerHealthChanged -= HandleOnUpdateHealthBar;
            _playerManager.OnUpdateMagazineCountUI -= HandleOnUpdateMagazineCountUI;
            _playerManager.OnShowItemPickup -= HandleOnShowItemPickup;
            _playerManager.OnScrollItemPickup -= HandleOnScrollItemPickup;
            _playerManager.OnHideItemPickup -= HandleOnHideItemPickup;
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
    }
}
