using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private int playerHealth = 100; //추후 SO에서 받아오게 수정 예정

    [SerializeField] private PlayerWeapon currentWeapon;

    public PlayerWeapon CurrentWeapon => currentWeapon;

    private void Awake()
    {
        
    }

}
