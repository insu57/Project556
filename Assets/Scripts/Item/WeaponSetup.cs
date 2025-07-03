using UnityEngine;
using UnityEngine.Serialization;

public class WeaponSetup : MonoBehaviour
{
    [SerializeField] private Transform muzzleOffset;
    [SerializeField] private Transform muzzleFlashOffset;
    public Vector3 MuzzleOffset => muzzleOffset.localPosition;
    public Vector3 MuzzleFlashOffset => muzzleFlashOffset.localPosition;
}
