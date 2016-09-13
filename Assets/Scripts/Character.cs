using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour {

	public enum Direction { Left, Right }

	[Range(0.1f, 20f)]
	public float moveSpeed = 0.5f;

	private int health;
	public int maxHealth = 25;

	public int baseDamage;

	private float hitTimer;
	private float knockbackTimer;

	protected bool onGround;

	private Direction facing;

	private Rigidbody2D rb;

	public int Health { get { return health; } }
	public bool OnGround { get { return onGround; } set { onGround = value; } }
	public Direction Facing { get { return facing; } }

	// Initialization
	void Start () {
		health = maxHealth;

		rb = gameObject.GetComponent<Rigidbody2D> ();

		if (rb == null)
			Debug.Log (gameObject.name + " does not have component 'Rigidbody2D' when it needs one!");

		InitializeCharacter ();
	}

	void Update () {
		UpdateCharacter ();

		if (hitTimer > 0)
			hitTimer -= Time.deltaTime;
		if (knockbackTimer > 0)
			knockbackTimer -= Time.deltaTime;
	}
		
	protected virtual void InitializeCharacter() {/*intentionally left blank*/}

	protected virtual void UpdateCharacter() {/*intentionally left blank*/}

	/// <summary>
	/// Move the specified direction. Provided direction must be normalized.
	/// </summary>
	/// <param name="direction">Direction.</param>
	protected void Move(Vector2 direction) {
		if (direction.x >= 0) { //positive x is to the right
			if (knockbackTimer <= 0) {//if we are not being knocked back
				if (facing == Direction.Left) //if we are changing directions, stop movement to keep the character from 'slipping and sliding'
					rb.velocity = new Vector2 (0, rb.velocity.y);
			}

			facing = Direction.Right; //set the direction we are moving towards
		} else {
			if (knockbackTimer <= 0) {
				if (facing == Direction.Right)
					rb.velocity = new Vector2 (0, rb.velocity.y);
			}
			
			facing = Direction.Left;
		}

		rb.AddForce (direction * moveSpeed * 1000.0f); //add the movement force to the object
	}

	public void AddKnockback(Vector2 force) {
		rb.AddForce (force);
		knockbackTimer = force.magnitude;
	}

	//called when weapon hitbox triggered
	public void ReceiveHit(int damage) {
		if (hitTimer > 0)
			return;

		health -= damage;

		if (health <= 0) {
			Destroy (gameObject); //later play animation and delay object destroy
			return;
		}

		hitTimer = 1f;
	}
}
