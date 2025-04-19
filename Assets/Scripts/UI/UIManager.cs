using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text ammoText;


    public void UpdateAmmoText(int currentAmmo)
    {
        ammoText.text = currentAmmo.ToString();
    }
}
