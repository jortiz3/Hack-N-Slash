using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour {

    private CinemachineVirtualCamera virtualCamera;

    void Awake() {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        GameManager_SwordSwipe.instance_CameraManager = this;
    }

    public void Follow(Transform objectToFollow) {
        virtualCamera.Follow = objectToFollow;
    }
}
