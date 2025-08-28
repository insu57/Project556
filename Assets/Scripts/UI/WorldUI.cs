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
    [SerializeField] private Vector3 fieldInteractOffset = new(0f, .5f, 0f);
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
        fieldInteractUI.gameObject.SetActive(true);
        currentInteract.anchoredPosition = Vector2.zero;

        fieldInteractUI.position = fieldPos + fieldInteractOffset; //offset만큼 이동
        
        _interactAvailableList = availableList;

        foreach (var text in _fieldInteractTextList)
        {
            text.gameObject.SetActive(false);
        }
        
        foreach (var (_, type) in _interactAvailableList) //type에 따른 표시(활성화)
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
        LayoutRebuilder.ForceRebuildLayoutImmediate(fieldInteractUI); //UI정렬
        
        interactHighlight.color = _interactAvailableList[0].isAvailable
            ? interactHighlightAvailableColor : interactHighlightUnavailableColor;
    }

    public void HideFieldInteractUI()
    {
        fieldInteractUI.gameObject.SetActive(false);
    }
    
    public void ScrollFieldInteractUI(int idx) //상호작용 스크롤
    {
        var uiPos = currentInteract.anchoredPosition; 

        uiPos.y = -idx * fieldInteractTextSize; //인덱스에 따라 
        var (isAvailable, _) = _interactAvailableList[idx];
        interactHighlight.color = isAvailable ? interactHighlightAvailableColor : interactHighlightUnavailableColor;
        //상호작용 가능여부 표시
        currentInteract.anchoredPosition =  uiPos;
    }
}
