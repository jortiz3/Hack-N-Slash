using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleEnemy : Character { //simple enemy that always moves towards the player in survival, just walks around in story

	private float pauseTime;
	private float currActiveTimer;
	[SerializeField, Tooltip("How long will this enemy stay active before pausing? (Random.Range)")]
	private Vector2 attentionSpanRange = Vector2.one;

	protected Vector3 targetLocation;
	[SerializeField, Tooltip("How often does this enemy check for the target's position?")]
	private Vector2 targetIdentifyRate = Vector2.one;
	protected float currTargetIdentifyTimer;

	private bool hasFallenOver;

	[SerializeField]
	private bool isAbleToJump = true;

	void FixedUpdate() {
		UpdateAnimations ();
	}

	private float GenerateFloatFromVector2(Vector2 range) {
		return Random.Range (range.x, range.y);
	}

	protected virtual Vector3 IdentifyTarget() {
		if (player != null && GameManager.currGameMode == GameMode.Survival)
			return player.transform.position; //target is player
		else
			return new Vector3 (Random.Range (-10, 10), Random.Range (0, 3), 0);
	}

	protected virtual void KipUp() {
		if (!isJumping) {
			if (Mathf.Abs (gameObject.transform.rotation.eulerAngles.z) < 10f) {//stopping state; enemy is upright again
				hasFallenOver = false;
			} else {
				Jump ();
				AddTorque (transform.rotation.z * 10); //add torque to rigidbody to rotate the enemy
			}
		} else if (isFalling) {//on our way down
			if (Mathf.Abs (transform.rotation.eulerAngles.z) < 10f) { //if close enough to upright
				transform.rotation = Quaternion.Euler (Vector3.zero); //snap to correct orientation
				StopRotation (); //halt the spin
			}
		}
	}

	protected virtual void Move() {
		Vector3 direction = targetLocation - transform.position;

		if (direction.x > 0.4f) {//target is to the right
			Move (1);
		} else if (direction.x < -0.4f) {//target is to the left
			Move (-1);
		} else {//target is close enough
			Move (0);
			currTargetIdentifyTimer = 0;
		}

		if (isAbleToJump) {
			if (Mathf.Abs (direction.x) < 2f) {//jump only when horizontally near target
				if (direction.y > 2f) {//jump only when below target
					Jump ();
				}
			}
		}
	}

	protected void PauseMovement() {
		currActiveTimer = 0;
	}

	void Start () {
		Initialize ();

		currActiveTimer = GenerateFloatFromVector2 (attentionSpanRange);
	}

	void Update() {
		UpdateEnemy ();
	}

	protected virtual void UpdateEnemy () {
		if (currTargetIdentifyTimer > 0) { //update the
			currTargetIdentifyTimer -= Time.deltaTime;
		} else { //find target
			targetLocation = IdentifyTarget();
			currTargetIdentifyTimer = GenerateFloatFromVector2(targetIdentifyRate); //set the delay
		}

		if (currActiveTimer > 0 && !isFlinching) { //able to do stuff
			if (Mathf.Abs (gameObject.transform.rotation.eulerAngles.z) > 10f) //upright position is z == 0
				hasFallenOver = true;

			if (!hasFallenOver) {
				Move ();
			} else { //enemy has fallen over
				if (isAbleToJump) {
					KipUp ();
				}
			}

			currActiveTimer -= Time.deltaTime;
		} else if (pauseTime > 0) { //taking a break
			pauseTime -= Time.deltaTime;
		} else { //returning to move towards target
			currActiveTimer = GenerateFloatFromVector2 (attentionSpanRange);
			pauseTime = 2f;
		}
	}
}
