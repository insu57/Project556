using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [SerializeField] private Transform muzzleTransform;
    
    public Transform MuzzleTransform => muzzleTransform;
    
}
