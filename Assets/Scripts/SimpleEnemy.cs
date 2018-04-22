using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleEnemy : Character { //simple enemy that always moves towards the player

	private float pauseTime;
	private float currPauseDelay;
	[SerializeField, Tooltip("Time between pauses -- How long will this enemy stay active before pausing (Random.Range)")]
	private Vector2 pauseDelayRange = Vector2.zero;

	protected Vector3 targetLocation;
	[SerializeField, Tooltip("Target delay determines how often this enemy searches for the target position.")]
	private float targetIdentifyDelay = 2f;
	private float currTargetIdentifyDelay;

	private bool hasFallenOver;

	[SerializeField]
	private bool thisEnemyIsAbleToJump = true;

	void Start () {
		Initialize ();

		GeneratePauseDelay ();
	}

	void Update() {
		UpdateEnemy ();
	}

	void FixedUpdate() {
		UpdateAnimations ();
	}

	protected virtual void UpdateEnemy () {
		if (currTargetIdentifyDelay > 0) { //delay finding target
			currTargetIdentifyDelay -= Time.deltaTime;
		} else { //find target
			if (player != null)
				targetLocation = player.transform.position; //target is player
			else
				targetLocation = new Vector3 (Random.Range (-10, 10), Random.Range (0, 3), 0);
			
			currTargetIdentifyDelay = targetIdentifyDelay; //set the delay
		}

		if (currPauseDelay > 0 && !isFlinching) { //able to do stuff

			if (Mathf.Abs (gameObject.transform.rotation.eulerAngles.z) > 10f) //upright position is z == 0
				hasFallenOver = true;

			if (!hasFallenOver) {
				Move ();
			} else { //enemy has fallen over
				if (thisEnemyIsAbleToJump) {
					KipUp ();
				}
			}

			currPauseDelay -= Time.deltaTime;
		} else if (pauseTime > 0) { //taking a break
			pauseTime -= Time.deltaTime;
		} else { //returning to move towards target
			currPauseDelay = GeneratePauseDelay ();
			pauseTime = 2f;
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
		}

		if (thisEnemyIsAbleToJump) {
			if (Mathf.Abs (direction.x) < 2f) {//jump only when horizontally near target
				if (direction.y > 2f) {//jump only when below target
					Jump ();
				}
			}
		}
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

	private float GeneratePauseDelay() {
		return Random.Range (pauseDelayRange.x, pauseDelayRange.y);
	}

	protected void PauseMovement() {
		currPauseDelay = 0;
	}
}
