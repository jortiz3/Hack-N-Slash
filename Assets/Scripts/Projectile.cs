using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D)), RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour {

	private Weapon owner;
	private Rigidbody2D rb2D;
	[SerializeField]
	private ImpactEffect impactEffect;

	public void Fire(Vector3 startPos, Vector2 force) {
		transform.position = startPos; //place the projectile at the start position
		if (!gameObject.activeSelf) //if projectile isn't visible
			gameObject.SetActive (true); //display it


		rb2D.velocity = Vector2.zero; //ensure the projectile is not currently moving
		rb2D.AddForce(force * rb2D.mass); //launch projectile in the direction
	}

	void OnCollisionEnter2D (Collision2D otherObj) {
		bool triggerImpact = false;

		if (otherObj.gameObject.layer == LayerMask.NameToLayer ("World")) { //if projectile collided with the world
			gameObject.SetActive (false); //hide projectile; do not destroy so we save from having to create another
			triggerImpact = true;
			return;
		}

		Character c = otherObj.gameObject.GetComponent<Character> (); //see if the other object is a character; should not collide with friendlies
		if (c != null) {
			c.ReceiveDamageFrom (owner); //damage the other character
			gameObject.SetActive (false); //hide the projectile
			triggerImpact = true;
		}

		if (triggerImpact) {
			if (impactEffect != null) {
				impactEffect.transform.position = transform.position;
				impactEffect.ResetAnimation();
				impactEffect.gameObject.SetActive(true);
			}
		}
	}

	public void SetOwner(Weapon Owner) {
		owner = Owner;
		gameObject.layer = owner.gameObject.layer; //conforms projectile to owner's physics layer; allows projectiles to be used by different types of characters
		rb2D = gameObject.GetComponent<Rigidbody2D> ();
		gameObject.SetActive (false);
	}
}
