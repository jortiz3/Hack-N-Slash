using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animation))]
public class ImpactEffect : MonoBehaviour {

	private Animation anim;
	private Character owner;
	[SerializeField]
	private int damage;
	[SerializeField, Tooltip("If enabled, will damage friendly units.")]
	private bool friendlyFire;

	void Update() {
		if (!anim.isPlaying) {
			gameObject.SetActive(false);
		}
	}

	void OnTriggerEnter2D(Collider2D otherObj) {
		Character c = otherObj.GetComponent<Character>();
		if (c != null) {
			c.ReceiveDamage(damage, false);
		}
	}

	public void SetOwner(Character Owner) {
		owner = Owner;
		if (!friendlyFire)
			gameObject.layer = owner.gameObject.layer;
	}

	void Start() {
		anim = gameObject.GetComponent<Animation>();
	}

	public void ResetAnimation() {
		//anim reset to frame 1
	}
}
