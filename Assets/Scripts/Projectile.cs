using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

	private Weapon owner;
	private Rigidbody2D rb2D;

	public void Fire(Vector3 startPos, Vector2 force) {
		transform.position = startPos;
		rb2D.AddForce(force);

		if (!gameObject.activeSelf)
			gameObject.SetActive (true);
	}

	void OnCollisionEnter2D (Collision2D otherObj) {
		if (otherObj.gameObject.layer == LayerMask.NameToLayer ("World")) {
			gameObject.SetActive (false);
			return;
		}

		Character c = otherObj.gameObject.GetComponent<Character> ();
		if (c != null) {
			c.ReceiveDamageFrom (owner);
			gameObject.SetActive (false);
		}
	}

	public void SetOwner(Weapon Owner) {
		owner = Owner;
		gameObject.layer = owner.gameObject.layer; //conformes projectile to owner's physics layer; allows projectiles to be used by different types of characters
	}
}
