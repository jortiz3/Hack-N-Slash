using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animation))]
public class ImpactEffect : MonoBehaviour {

	private Animation anim;
	//owner

	void Start() {
		anim = gameObject.GetComponent<Animation>();
	}

	void Update() {
		if (!anim.isPlaying) {
			gameObject.SetActive(false);
		}
	}

	void OnTriggerEnter2D(Collider2D collision) {
		//hurt character
	}
}
