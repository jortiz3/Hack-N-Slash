using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDetection : MonoBehaviour {

	private Character character;

	void Start () {
		character = transform.parent.GetComponent<Character> ();
	}

	void OnTriggerEnter2D(Collider2D otherObj) {
		Character c = otherObj.GetComponent<Character> ();
		if (c != null) {
			character.DetectBeginOtherCharacter (c);
		}
	}

	void OnTriggerExit2D(Collider2D otherObj) {
		Character c = otherObj.GetComponent<Character> ();
		if (c != null) {
			character.DetectEndOtherCharacter (c);
		}
	}
}
