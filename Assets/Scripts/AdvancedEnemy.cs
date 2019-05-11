using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//create editor script to show/hide flyingHeight based on movementType
//make fly swoop more of a curve
//-make targetlocation other side of player?
//-target location and transform.position not same coordinates??

public class AdvancedEnemy : SimpleEnemy {//Capable of a variety of things depending on game difficulty

	[Tooltip("Locations this enemy would go or be attracted to."), SerializeField]
	protected Transform[] pointsOfInterest;
	[SerializeField]
	protected Spawn[] spawns;
	protected List<Character> minions;
	[SerializeField]
	protected MovementType movementType = MovementType.Run;
	[SerializeField]
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
	private bool flyingAttackLocationSet;
	private Vector3 initialAttackDistance;
	[SerializeField, Tooltip("Plays when this enemy dies. (Can be left null)")]
	private Cutscene cutsceneToPlayOnDeath; //in case this enemy is the objective of the level, the final cutscene will play

	public enum MovementType { Run, FlySwoop, FlyBomber, Jumper }
	public enum Behaviour { Aggressive, StandGround, Evasive, Neutral }

	public override void Die () {
		if (minions != null) {
			foreach (Character c in minions) {
				if (c != null) {
					c.ReceiveDamage (999, true);
				}
			}
		}
		if (cutsceneToPlayOnDeath != null) {
			GameManager_SwordSwipe.currGameManager.PlayCutscene (cutsceneToPlayOnDeath);
		}
		base.Die ();
	}

	protected override Vector3 IdentifyTargetLocation () {
		if (GameManager_SwordSwipe.currGameMode == GameMode.Survival && player != null) {
				targetCharacter = player;
				return player.transform.position;
		} else {
			if (behaviour != Behaviour.Neutral && targetCharacter != null) {
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

			if (HasFallenOver) { //enemy has fallen over
				KipUp(); //attempt to reorientate z axis
				return; //reorientation is all it can do while upside down
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
			if (HasFallenOver) { //if enemy fallen over
				KipUp(); //reorientate z axis
			} //keep flying
			
			if (isAttacking) { //character attacked
				if (!flyingAttackLocationSet && targetCharacter != null) { //if the attack location isn't set
					initialAttackDistance = targetCharacter.transform.position - transform.position;
					targetLocation.x = targetCharacter.transform.position.x + initialAttackDistance.x;//set attack location to point opposite target
					targetLocation.y = transform.position.y; //keep the current height for the end of the flight path
					FreezeTargetLocation(true); //ensure the target location doesn't change for now
					flyingAttackLocationSet = true; //show this process has been performed
				}
			} else if (FreezeTargetLocationEnabled) { //no longer attacking
				FreezeTargetLocation(false); //return to normal
				flyingAttackLocationSet = false;
			}

			moveDirection = targetLocation - transform.position;//direction of target
			if (movementType == MovementType.FlyBomber || !isAttacking) { //bombers always stay at flying height, swoopers only stay there when too far away to attack
				moveDirection.y = flyingHeight - transform.position.y; //go towards cruise altitude
			} else {
				//swoop motion -- (amplitude; scale vertical) * cosine((scale horizontal) * (x + (shift horizontal))) + (shift vertical),
				moveDirection.y = Mathf.Abs(initialAttackDistance.y) * (Mathf.Cos(moveDirection.x / initialAttackDistance.x)) + initialAttackDistance.y;
				Debug.Log("currTargetLoc: " + targetLocation + "  currDistance: " + moveDirection.ToString() + "  initialDistance: " + initialAttackDistance.ToString());
			}
			Fly (moveDirection); //fly towards target
		} else {
			base.Move ();
		}

		if (targetCharacter != null) { //chasing a character
			if (Vector3.Distance(transform.position, targetCharacter.transform.position) < attackDistance) { //if close enough to attack
				if (currAttackTimer <= 0) { //attack delay is over
					Attack("Attack_Forward"); //attack
					if (movementType != MovementType.FlyBomber && movementType != MovementType.FlySwoop) //if this character doesn't fly
						PauseMovement(); //stop moving
					currAttackTimer = attackDelay; //reset delay
				}
			}
		} else {//moving to point of interest
			if (Vector3.Distance(transform.position, targetLocation) < 0.8f) { //close enough
				PauseMovement(); //stop moving
			}
		}

		if (currAttackTimer > 0) { //always update attack delay
			currAttackTimer -= Time.deltaTime; //decrease delay
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

		if (behaviour == Behaviour.Neutral) { //if neutral
			if (CurrentHP < MaxHP) { //enemy was damaged
				behaviour = Behaviour.Aggressive; //become aggressive
				PauseMovement();
			}
		}

		base.UpdateEnemy();
	}
}
