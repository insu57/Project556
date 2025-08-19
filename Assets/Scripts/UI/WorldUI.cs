using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldUI : MonoBehaviour
{
    [SerializeField, Space] private RectTransform fieldInteractUI;
    private readonly List<TMP_Text> _fieldInteractTextList = new();
    [SerializeField] private TMP_Text equipText;
    [SerializeField] private TMP_Text pickupText;
    [SerializeField] private TMP_Text useText;
    [SerializeField] private TMP_Text openText;
    [SerializeField] private float fieldInteractTextSize = 50f;
    [SerializeField] private Vector3 fieldInteractOffset = new(0f, 1f, 0f);
    [SerializeField] private RectTransform currentInteract;
    [SerializeField] private Image interactHighlight;
    [SerializeField] private Color interactHighlightAvailableColor;
    [SerializeField] private Color interactHighlightUnavailableColor;
    private List<(bool isAvailable, InteractType type)> _interactAvailableList;
    
    private void Awake()
    {
        _fieldInteractTextList.Add(equipText);
        _fieldInteractTextList.Add(pickupText);
        _fieldInteractTextList.Add(useText);
        _fieldInteractTextList.Add(openText);
    }

    public void ShowFieldInteractUI (Vector3 fieldPos, List<(bool, InteractType)> availableList)
    {
        //설정...아이템 따라
        fieldInteractUI.gameObject.SetActive(true);
        currentInteract.anchoredPosition = Vector2.zero;

        fieldInteractUI.position = fieldPos + fieldInteractOffset;
        
        _interactAvailableList = availableList;

        foreach (var text in _fieldInteractTextList)
        {
            text.gameObject.SetActive(false);
        }
        
        foreach (var (_, type) in _interactAvailableList) //여기가 문제... 색만 변경
        {
            switch (type)
            {
                case InteractType.Equip:
                    equipText.gameObject.SetActive(true);
                    break;
                case InteractType.PickUp:
                    pickupText.gameObject.SetActive(true);
                    break;
                case InteractType.Use:
                    useText.gameObject.SetActive(true);
                    break;
                case InteractType.Open:
                    openText.gameObject.SetActive(true);
                    break;
                default:
                    Debug.LogWarning("ItemInteractType Error: None.");
                    break;
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(fieldInteractUI);
        
        interactHighlight.color = _interactAvailableList[0].isAvailable
            ? interactHighlightAvailableColor : interactHighlightUnavailableColor;
    }

    public void HideFieldInteractUI()
    {
        fieldInteractUI.gameObject.SetActive(false);
    }
    
    public void ScrollFieldInteractUI(int idx)
    {
        var uiPos = currentInteract.anchoredPosition;

        uiPos.y = -idx * fieldInteractTextSize;
        var isAvailable = _interactAvailableList[idx];
        interactHighlight.color = isAvailable.isAvailable ? interactHighlightAvailableColor : interactHighlightUnavailableColor;
        currentInteract.anchoredPosition =  uiPos;
    }
}
