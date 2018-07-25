using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {

	void OnTriggerEnter2D (Collider2D other) {
		if (other.gameObject == Character.player.gameObject) {
			PickUp ();
		}
	}

	protected virtual void PickUp() {
		gameObject.SetActive (false);
	}
}
