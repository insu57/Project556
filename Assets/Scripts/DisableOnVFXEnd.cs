using UnityEngine;

public class DisableOnVFXEnd : MonoBehaviour
{
    public void OnVFXEnd()
    {
        gameObject.SetActive(false);
    }
}
