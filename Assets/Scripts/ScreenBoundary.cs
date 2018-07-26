using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenBoundary : MonoBehaviour {
	private bool moveCamera;

	void FixedUpdate() {
		if (moveCamera) {
			Camera.main.transform.position += (Vector3)Character.player.Velocity * Time.fixedDeltaTime;
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
