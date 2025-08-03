using Player;
using Unity.Cinemachine;
using UnityEngine;

public class SceneCamera : MonoBehaviour
{
    private CinemachineCamera _cinemachineCamera;

    private void Awake()
    {
        _cinemachineCamera = GetComponentInChildren<CinemachineCamera>();
        var playerManager = FindAnyObjectByType<PlayerManager>();
        _cinemachineCamera.Follow = playerManager.transform;
    }
}
