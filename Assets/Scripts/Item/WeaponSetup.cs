using UnityEngine;
using UnityEngine.Serialization;

public class WeaponSetup : MonoBehaviour //무기 총구 위치 설정
{
    [SerializeField] private Transform muzzleOffset;
    [SerializeField] private Transform muzzleFlashOffset;
    public Vector3 MuzzleOffset => muzzleOffset.localPosition; //총구 위치
    public Vector3 MuzzleFlashOffset => muzzleFlashOffset.localPosition; //총구 화염 위치
}
