using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private RectTransform playerMenu;
        public bool PlayerMenuOpen => playerMenu.gameObject.activeSelf;
        [SerializeField] private RectTransform inventoryTab;
        [SerializeField] private RectTransform infoTab;
        [SerializeField] private RectTransform questTab;
        private readonly Dictionary<PlayerMenuState, RectTransform> _playerTabs = new();
        
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button playerInfoBtn;
        [SerializeField] private Button questBtn;
        
        private enum PlayerMenuState
        {
            Inventory,
            PlayerInfo,
            Quest
        }
        
        private void OnEnable()
        {
            inventoryButton.onClick.AddListener(() => OpenTab(PlayerMenuState.Inventory));
            playerInfoBtn.onClick.AddListener(() => OpenTab(PlayerMenuState.PlayerInfo));
            questBtn.onClick.AddListener(() => OpenTab(PlayerMenuState.Quest));
        }

        private void OnDisable()
        {
            inventoryButton.onClick.RemoveAllListeners();
            playerInfoBtn.onClick.RemoveAllListeners();
            questBtn.onClick.RemoveAllListeners();
        }

        private void Awake()
        {
            _playerTabs.Add(PlayerMenuState.Inventory, inventoryTab);
            _playerTabs.Add(PlayerMenuState.PlayerInfo, infoTab);
            _playerTabs.Add(PlayerMenuState.Quest, questTab);
        }
        
        public void OpenPlayerUI(bool isOpen) //PlayerUI
        {
            playerMenu.gameObject.SetActive(isOpen);
            OpenTab(PlayerMenuState.Inventory);
        }

        private void OpenTab(PlayerMenuState state)
        {
            foreach (var (key,tab) in _playerTabs)
            {
                tab.gameObject.SetActive(state == key);
            }
        }
    }
}
