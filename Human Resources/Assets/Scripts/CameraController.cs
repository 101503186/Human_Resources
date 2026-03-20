using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera aimVirtualCamera;
    [SerializeField] private CinemachineCamera regularVirtualCamera;
    [SerializeField] private RawImage crosshairImage;
    public PlayerMovement player;

    private void Update()
    {
        if (player.isAiming)
        {
            aimVirtualCamera.Priority = 20;
            regularVirtualCamera.Priority = 10;
            crosshairImage.gameObject.SetActive(true);
        }
        else
        {
            aimVirtualCamera.Priority = 10;
            regularVirtualCamera.Priority = 20;
            crosshairImage.gameObject.SetActive(false);
        }
    }
}
