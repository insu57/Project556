using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private GameObject playerUI;
    
    public void UpdateAmmoText(int currentAmmo)
    {
        ammoText.text = currentAmmo.ToString();
    }

    public void OpenPlayerUI(bool isOpen)
    {
        playerUI.SetActive(isOpen);
    }
}
