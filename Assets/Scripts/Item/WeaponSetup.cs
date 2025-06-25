using UnityEngine;
using UnityEngine.Serialization;

public class WeaponSetup : MonoBehaviour
{
    [SerializeField] private Transform muzzleOffset;
    public Vector3 MuzzleOffset => muzzleOffset.position;
}
