using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour {

    private CinemachineVirtualCamera virtualCamera;

    void Awake() {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        GameManager_SwordSwipe.currCameraManager = this;
    }

    public void Follow(Transform objectToFollow) {
        virtualCamera.Follow = objectToFollow;
    }
}
