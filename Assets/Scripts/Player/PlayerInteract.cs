using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    private InventoryManager _inventoryManager; //개선 방안?
    
    private int _interactableLayer;
 
    private List<(bool isAvailable, InteractType type)> _currentFieldInteractList = new();
    private int _currentFieldInteractIdx;
    public (bool IsAvailable, InteractType type) CurrentFieldInteract => _currentFieldInteractList[_currentFieldInteractIdx];
    public bool CanInteract{get; private set;}
    public ItemInstance CurrentFieldItem {get; private set;}
    public Inventory CurrentLootInventory {get; private set;}
    private readonly Dictionary<GameObject, IFieldInteractable> _fieldInteractableDict = new();
    public IFieldInteractable CurrentFieldInteractable {get; private set;}
   
    public event Action<Vector3, List<(bool available, InteractType type)>> OnShowFieldInteract; //현재 FieldInteract
    public event Action OnHideFieldInteract;
    public event Action<int> OnScrollItemPickup; //아이템 픽업 UI 스크롤
    
    private void Awake()
    {
        _interactableLayer = LayerMask.NameToLayer("Interactable"); //Layer 캐싱
        _inventoryManager = FindAnyObjectByType<InventoryManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != _interactableLayer) return;
        
        CanInteract = true;
        if (!_fieldInteractableDict.ContainsKey(other.gameObject))
        {
            var fieldInteractable = other.GetComponent<IFieldInteractable>();
            _fieldInteractableDict[other.gameObject] = fieldInteractable;
        }
        GetNearestFieldInteractable(); //가장 가까운 상호작용 오브젝트 
        CurrentFieldInteractable.PlayerGetFieldInteractInfo(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Item") || other.CompareTag("Crate"))
        {
            RemoveFieldInteractable(other.gameObject);
        }
    }
    
    private void GetNearestFieldInteractable()
    {
        float nearestDist = float.MaxValue;

        foreach (var (go, fieldInteractable) in _fieldInteractableDict)
        {
            float dist = Vector3.Distance(gameObject.transform.position, go.transform.position);
            if (nearestDist <= dist) continue;
            //가장 가까운 상호작용으로 
            nearestDist = dist; 
            CurrentFieldInteractable = fieldInteractable;
        }
    }
    public void RemoveFieldInteractable(GameObject interactableObject)
    {
        _fieldInteractableDict.Remove(interactableObject);
        if (_fieldInteractableDict.Count == 0) //비었으면 가리기
        {
            CanInteract = false;
            OnHideFieldInteract?.Invoke();
        }
        else
        {
            GetNearestFieldInteractable();
            CurrentFieldInteractable.PlayerGetFieldInteractInfo(this);
        }
    }
    
    public void ScrollItemPickup(float scrollDeltaY)
    {
        if(!CanInteract) return;
            
        _currentFieldInteractIdx += (int)scrollDeltaY;
        if (_currentFieldInteractIdx < 0) _currentFieldInteractIdx = _currentFieldInteractList.Count - 1;
        else if (_currentFieldInteractIdx >= _currentFieldInteractList.Count) _currentFieldInteractIdx = 0;
        //범위 넘기면 처리
      
        OnScrollItemPickup?.Invoke(_currentFieldInteractIdx);
    }
    
    public void GetFieldItemData(ItemInstance itemInstance, Vector3 pos)
    { 
        _currentFieldInteractList = _inventoryManager.CheckFieldItemPickup(itemInstance);
        _currentFieldInteractIdx = 0;
        CurrentFieldItem = itemInstance;
        CurrentLootInventory = null;
        
        OnShowFieldInteract?.Invoke(pos, _currentFieldInteractList);
    }

    public void GetLootCrateData(Inventory inventory, Vector3 pos)
    {
        _currentFieldInteractList.Clear();
        _currentFieldInteractIdx = 0;
        _currentFieldInteractList.Add((true, InteractType.Open));
        CurrentFieldItem = null;
        CurrentLootInventory = inventory;
        
        OnShowFieldInteract?.Invoke(pos, _currentFieldInteractList);
    }
}
