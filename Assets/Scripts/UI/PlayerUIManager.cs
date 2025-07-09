using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] private Image healthBar;
    [SerializeField] private TMP_Text ammoText;
    
    public void UpdateHealthBar(float health, float maxHealth)
    {
        healthBar.fillAmount = health / maxHealth;
    }

    public void UpdateAmmoText(bool isFullyLoaded, int ammo) 
    {
        ammoText.text = ammo.ToString();
        if (isFullyLoaded)
        {
            ammoText.text = ammo - 1 + "<size=75%>+1</size>";
        }
        else
        {
            ammoText.text = ammo.ToString();
        }
    }
}
