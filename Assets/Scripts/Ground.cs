using UnityEngine;
using System.Collections;

public class Ground : MonoBehaviour {
	void OnTriggerEnter(Collider other) {
		Character c = other.GetComponent<Character> ();
		if (c != null) {
			c.OnGround = true;
			Debug.Log (other.name + " is on the ground.");
		}
	}
}
