using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//keep chasing target a little longer depending on difficulty
//create editor script to show/hide flyingHeight based on movementType

public class AdvancedEnemy : SimpleEnemy {//Capable of a variety of things depending on game difficulty

	[Tooltip("Locations this enemy would go or be attracted to."), SerializeField]
	protected Transform[] pointsOfInterest;
	[SerializeField]
	protected Spawn[] spawns;
	protected List<Character> minions;
	[SerializeField]
	protected MovementType movementType = MovementType.Run;
	protected Behaviour behaviour = Behaviour.Aggressive;

	private int remainingSpawns;
	private float spawnTimer;
	[SerializeField]
	private float attackDistance;
	[SerializeField]
	private float attackDelay;
	private float currAttackTimer;

	[SerializeField, Tooltip("What is the cruising altitude for this flying enemy?")]
	protected float flyingHeight; //somehow make this the height above current platform

	public enum MovementType { Run, FlySwoop, FlyBomber, Jumper }
	public enum Behaviour { Aggressive, StandGround, Evasive }

	public override void Die () {
		if (minions != null) {
			foreach (Character c in minions) {
				if (c != null) {
					c.Die ();
				}
			}
		}
		base.Die ();
	}

	protected override Vector3 IdentifyTargetLocation () {
		if (GameManager.currGameMode == GameMode.Survival) {
			if (player != null) {
				return player.transform.position;
			}
		} else {
			if (targetCharacter != null) {
				return targetCharacter.transform.position;
			}

			if (pointsOfInterest != null && pointsOfInterest.Length > 0) {
				return pointsOfInterest[Random.Range(0, pointsOfInterest.Length)].position;
			}
		}

		return base.IdentifyTargetLocation ();
	}

	protected override void Move () {
		if (behaviour == Behaviour.StandGround) //this enemy will not move
			return;

		if (movementType == MovementType.Run || movementType == MovementType.Jumper) { //if a runner or jumper
			if (movementType == MovementType.Jumper && isOnGround) { //if the enemy is a jumper and they are on the ground
				Jump (); //jump before moving
			}

			if (behaviour == Behaviour.Aggressive) { //if the enemy is aggressive
				base.Move (); //move towards the player just as a simple enemy
			} else if (behaviour == Behaviour.Evasive) { //if the enemy is evasive
				if (targetCharacter != null) { //if the target is nearby/detected
					moveDirection = targetLocation - transform.position; //get the direction of the targetCharacter
					moveDirection *= -1; //get the direction opposite the targetCharacter
					base.Move (); //move away from the targetCharacter
				} else {
					base.Move (); //walk towards points of interest normally
				}
			}
		} else if (movementType == MovementType.FlySwoop || movementType == MovementType.FlyBomber) {
			moveDirection = targetLocation - transform.position;//direction of target
			moveDirection.y = flyingHeight; //stay at cruise altitude
			Fly (moveDirection); //fly towards target at cruise altitude
		} else {
			base.Move ();
		}
			
		if (Vector3.Distance (transform.position, targetLocation) < attackDistance) { //if close enough
			if (targetCharacter != null) { //player is in range
				if (currAttackTimer <= 0) { //attack delay is over
					Attack ("Attack_Forward"); //attack
					PauseMovement (); //stop moving
					currAttackTimer = attackDelay; //reset delay
				} else {
					currAttackTimer -= Time.deltaTime; //decrease delay
				}
			} else { //no target to attack, enemy is just moving to point of interest
				PauseMovement();
			}
		}
	}

	public void RegisterMinion(Character c) {
		minions.Add (c);
	}

	void Start() {
		Initialize ();

		remainingSpawns = 0;
		for (int i = 0; i < spawns.Length; i++) {
			spawns [i].currqty += spawns [i].quantity;
			remainingSpawns += spawns [i].quantity;
		}

		spawnTimer = Random.Range (1, 2);
		currAttackTimer = attackDelay;

		minions = new List<Character> ();
	}

	protected override void UpdateEnemy () {
		if (remainingSpawns > 0) { //spawn minions
			if (!isFlinching && !isAttacking && isOnGround) {
				if (spawnTimer <= 0) {
					int currSpawn;
					do {
						currSpawn = Random.Range (0, spawns.Length); //pick a random spawn from the current wave
					} while (spawns [currSpawn].currqty <= 0);

					StartCoroutine (spawns [currSpawn].Instantiate (this));
					spawnTimer = Random.Range (1, 2);
					remainingSpawns--;
				} else {
					spawnTimer -= Time.deltaTime;
				}
			}
		}

		//	if target is too far away
		//		if too long
		//			get new target -- point of interest || player if survival game mode
		//		end if too long
		//	end if target too far away
		base.UpdateEnemy();
	}
}
