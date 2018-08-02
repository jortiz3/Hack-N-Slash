using UnityEngine;

public class ScreenBoundary : MonoBehaviour {
	private bool moveCamera;
    [SerializeField]
    private MovementAxis axis;
    private Vector3 cameraVelocity;

    public enum MovementAxis { Horizontal, Vertical };

	void FixedUpdate() {
		if (moveCamera) {
            cameraVelocity = Vector3.zero;
            if (axis == MovementAxis.Horizontal) {
                cameraVelocity.x = Character.player.Velocity.x * Time.fixedDeltaTime;
            }
            else {
                cameraVelocity.y = Character.player.Velocity.y * Time.fixedDeltaTime;
            }
            Camera.main.transform.position += cameraVelocity;
		}
	}

	void OnTriggerEnter2D(Collider2D other) { //first frame of collision
		if (other.gameObject.Equals(Character.player.gameObject)) { //if player
			moveCamera = true;
		}
	}

	void OnTriggerExit2D(Collider2D other) { //first frame exit collision
		if (other.gameObject.Equals(Character.player.gameObject)) { //if player
			moveCamera = false;
		}
	}

	void OnTriggerStay2D(Collider2D other) { //every frame of collision
		if (other.tag.Equals("World Boundary")) { //if world boundary
			moveCamera = false;
		}
	}

	void Start() {
		moveCamera = false;
	}
}
