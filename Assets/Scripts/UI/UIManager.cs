using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private GameObject playerUI;
    [SerializeField] private RectTransform pickupUI;
    [SerializeField] private TMP_Text pickupText;
    [SerializeField] private TMP_Text equipText;
    [SerializeField] private RectTransform itemInteractUI;
    public void UpdateAmmoText(int currentAmmo)
    {
        ammoText.text = currentAmmo.ToString();
    }

    public void OpenPlayerUI(bool isOpen) //PlayerUI
    {
        //
        playerUI.SetActive(isOpen);
    }
    
    public void ShowItemPickup(bool show, Vector2 position)
    {
        pickupUI.gameObject.SetActive(show);
        if (show)
        {
            pickupUI.position = position;
        }
        //아이템에 따라 달라질필요있음.(장착 상태, 종류에 따라)
    }

    public void ScrollItemPickup(float y)
    {
        var uiPos = itemInteractUI.anchoredPosition;
        uiPos.y += y * 50; //50 -> 칸의 크기 
        if (uiPos.y > 0)
        {
            uiPos.y = -50;//변경될 예정(스크롤 목록의 마지막 y위치)
        }
        else if (uiPos.y < -50)
        {
            uiPos.y = 0;
        }

        itemInteractUI.anchoredPosition =  uiPos;
    }
}
