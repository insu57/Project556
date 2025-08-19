using System.Collections.Generic;
using UnityEngine;

public class WorldUIPresenter : MonoBehaviour
{
    private WorldUI _worldUI;
    private PlayerInteract _playerInteract;

    private void Awake()
    {
        TryGetComponent(out _worldUI);
        _playerInteract = FindAnyObjectByType<PlayerInteract>();
    }

    private void OnEnable()
    {
        _playerInteract.OnShowFieldInteract += HandleOnShowFieldInteract;
        _playerInteract.OnScrollItemPickup += HandleOnScrollFieldInteract;
        _playerInteract.OnHideFieldInteract += HandleOnHideFieldInteract;
    }

    private void OnDisable()
    {
        _playerInteract.OnShowFieldInteract -= HandleOnShowFieldInteract;
        _playerInteract.OnScrollItemPickup -= HandleOnScrollFieldInteract;
        _playerInteract.OnHideFieldInteract -= HandleOnHideFieldInteract;
    }
    private void HandleOnShowFieldInteract(Vector3 pos, List<(bool available, InteractType type)> availableList)
    {
        _worldUI.ShowFieldInteractUI(pos, availableList);//상호작용 리스트(해당 좌표에 표시)
    }

    private void HandleOnScrollFieldInteract(int idx)
    {
        _worldUI.ScrollFieldInteractUI(idx);
    }
    
    private void HandleOnHideFieldInteract()
    {
        _worldUI.HideFieldInteractUI();
    }
    
}
