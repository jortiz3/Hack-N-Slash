using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {

	public int damage;
	public Character wielder;

	private float swingIncrement;

	// Use this for initialization
	void Start () {
		if (wielder == null) {
			wielder = transform.parent.GetComponent<Character> ();

			if (wielder == null)
				Debug.Log (gameObject.name + " is unable to find Character component in parent transform. Please assign a Character or add the missing component to the parent transform.");
		}
	}

	void Update() {
		//rotate on the z axis based on wielder direction
	}

	void OnTriggerEnter(Collider other) {
		Character c = other.GetComponent<Character> ();
		if (c != null) {
			int dmg;
			Vector2 dir;
			if (other.tag.Equals ("") && gameObject.tag.Equals ("")) {
				dmg = GetDamage ();

				dir = new Vector2 (other.transform.position.x - transform.position.x, other.transform.position.y - transform.position.y);
				dir.Normalize ();

				c.ReceiveHit (dmg);
				c.AddKnockback (dir * (dmg / c.maxHealth));
			}
		}
	}

	public void Swing() {
		if (wielder.Facing == Character.Direction.Left)
			swingIncrement = -0.3f;
		else
			swingIncrement = 0.3f;
	}

	public void Reset() {
		swingIncrement = 0f;
		transform.rotation = new Quaternion (1f, 1f, 1f, 1f);
	}

	public int GetDamage() {
		return damage + wielder.baseDamage;
	}
}
