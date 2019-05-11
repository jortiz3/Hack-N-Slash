using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleEnemy : Character { //simple enemy that walks towards the player

	private float pauseTime;
	private float currActiveTimer;
	[SerializeField, Tooltip("How long will this enemy stay active before pausing? (Random.Range)")]
	private Vector2 attentionSpanRange = Vector2.one;
	private Vector2 defaultAttentionSpanRange;
	protected Character targetCharacter;
	protected Vector3 targetLocation;
	private bool freezeTargetLocation;
	[SerializeField, Tooltip("How often does this enemy check for the target's position?")]
	private Vector2 targetIdentifyRate = Vector2.one;
	private Vector2 defaultTargetIdentifyRate;
	protected float currTargetIdentifyTimer;
	private float currChaseTimer;
	private float maxChaseTime;
	private float maxChaseDistance;
	private bool hasFallenOver;
	[SerializeField]
	private bool isAbleToJump = true;
	protected Vector3 moveDirection;
	[SerializeField, Tooltip("Defeating this enemy will complete a challenge.")]
	private bool challengeEnemy;
	private GameDifficulty prevGameDifficulty;
	
	public bool HasFallenOver { get { return hasFallenOver; } }
	public bool IsAbleToJump { get { return IsAbleToJump; } }
	protected bool FreezeTargetLocationEnabled { get { return freezeTargetLocation; } }

	public override void DetectBeginOtherCharacter(Character otherCharacter) {
		targetCharacter = otherCharacter;

		if ((int)GameManager_SwordSwipe.currDifficulty < 2) //if the difficulty is below normal, the enemy will pause on detection
			PauseMovement();
	}

	public override void DetectEndOtherCharacter(Character otherCharacter) {
		if (otherCharacter == targetCharacter) {
			targetCharacter = null;
			PauseMovement();
		}
	}

	public override void Die() {
		if (GameManager_SwordSwipe.currGameState == GameState.Active) {
			if (challengeEnemy) {
				if (GameManager_SwordSwipe.currGameMode == GameMode.Campaign) {
					GameManager_SwordSwipe.currGameManager.ChallengeActionComplete(GameManager_SwordSwipe.SelectedCampaignMission + "_Enemy:" + this.ToString());
				} else {
					GameManager_SwordSwipe.currGameManager.ChallengeActionComplete("Survival_" + GameManager_SwordSwipe.currSurvivalSpawner.CurrentWave + "_enemy:" + this.ToString());
				}
			}
		}
		base.Die();
	}

	void FixedUpdate() {
		UpdateAnimations();
	}

	protected void FreezeTargetLocation(bool enabled) {
		if (!enabled){
			currActiveTimer = 0;
		}

		freezeTargetLocation = enabled;
	}

	private float GenerateFloatFromVector2(Vector2 range) {
		return Random.Range(range.x, range.y);
	}

	protected virtual Vector3 IdentifyTargetLocation() {
		if (player != null && GameManager_SwordSwipe.currGameMode == GameMode.Survival) {
			targetCharacter = player;
			return player.transform.position; //target is player
		} else if (targetCharacter != null) {
			return targetCharacter.transform.position;
		} else {
			return new Vector3(Random.Range(-10, 10), Random.Range(0, 3), 0);
		}
	}

	protected override void Initialize() {
		currActiveTimer = GenerateFloatFromVector2(attentionSpanRange);
		defaultAttentionSpanRange = new Vector2(attentionSpanRange.x, attentionSpanRange.y); //capture the initial values
		defaultTargetIdentifyRate = new Vector2(targetIdentifyRate.x, targetIdentifyRate.y); //capture the initial values
		UpdateValuesByDifficulty();

		base.Initialize();
	}

	protected virtual void KipUp() { //fixes z rotation of freely rotating enemies
		if (!isJumping) {
			if (Mathf.Abs(gameObject.transform.rotation.eulerAngles.z) < 10f) {//stopping state; enemy is upright again
				StopRotation(); //halt the spin
				hasFallenOver = false;
			} else if (isAbleToJump) {
				Jump(); //get enemy off the ground
				AddTorque(transform.rotation.z * 10); //add torque once to rigidbody to rotate the enemy
			} else { //unable to jump
				float torque = isFacingLeft ? -2f * Time.deltaTime : 2f * Time.deltaTime;
				AddTorque(torque);
			}
		} else if (isFalling) {//on our way down
			if (Mathf.Abs(transform.rotation.eulerAngles.z) < 10f) { //if close enough to upright
				StopRotation(); //halt the spin
			}
		}
	}

	protected virtual void Move() {
		if (hasFallenOver) { //enemy has fallen over
			KipUp();
			return;
		}

		if (targetLocation == Vector3.zero)
			return;

		if (moveDirection == Vector3.zero)
			moveDirection = targetLocation - transform.position;

		if (moveDirection.x > 0.1f) {//target is to the right
			Run(1);
		} else if (moveDirection.x < -0.1f) {//target is to the left
			Run(-1);
		} else {//target is close enough
			Run(0);
			currTargetIdentifyTimer = 0;
		}

		if (isAbleToJump) {
			if (Mathf.Abs(moveDirection.x) < 2f) {//jump only when horizontally near target
				if (moveDirection.y > 2f) {//jump only when below target
					Jump();
				}
			}
		}

		moveDirection = Vector3.zero;
	}

	protected void PauseMovement() {
		currActiveTimer = 0;
	}

	void Start() {
		Initialize();
	}

	public override string ToString() {
		return gameObject.name.Replace("(Clone)", "");
	}

	void Update() {
		if (GameManager_SwordSwipe.currGameState == GameState.Active) {
			UpdateEnemy();
		} else {
			StopMovement();
		}
	}

	protected virtual void UpdateEnemy() {
		if (prevGameDifficulty != GameManager_SwordSwipe.currDifficulty) { //difficulty changed
			UpdateValuesByDifficulty(); //make sure values are updated
		}

		if (currTargetIdentifyTimer > 0) { //update the timer
			currTargetIdentifyTimer -= Time.deltaTime;
		} else if (!freezeTargetLocation) { //find target if location not frozen
			targetLocation = IdentifyTargetLocation();
			currTargetIdentifyTimer = GenerateFloatFromVector2(targetIdentifyRate); //set the delay
			currChaseTimer = maxChaseTime; //reset chase time
		}

		if ((currActiveTimer > 0 && currChaseTimer > 0 && !isFlinching) || freezeTargetLocation) { //able to do stuff
			if (Mathf.Abs(gameObject.transform.rotation.eulerAngles.z) > 10f) //upright position is z == 0
				hasFallenOver = true;

			Move();

			if ((currChaseTimer > 0 && targetCharacter != null)) //there is time left to chase, and there is a character being targeted
				currChaseTimer -= Time.deltaTime; //update how long this enemy has been chasing target
			currActiveTimer -= Time.deltaTime; //update time left for current movement
		} else if (pauseTime > 0) { //taking a break
			pauseTime -= Time.deltaTime;
		} else { //returning to move towards target
			currActiveTimer = GenerateFloatFromVector2(attentionSpanRange); //reset active time
			pauseTime = 2f; //reset pause time for when activity stops
		}

		if (GameManager_SwordSwipe.currGameMode == GameMode.Campaign) { //enemies only chase for so long and so far away on Campaign
			if (currChaseTimer <= 0 && !freezeTargetLocation) { //only chase target for so long -- stuck or player ran away
				if (Vector3.Distance(targetLocation, transform.position) > maxChaseDistance) { //target is too far away
					currTargetIdentifyTimer = 0; //time to identify new target
					targetCharacter = null;
				}
			}
		}
	}

	private void UpdateValuesByDifficulty() {
		int currDifficultyMod = (int)GameManager_SwordSwipe.currDifficulty + 1; //minimum now becomes 1 instead of 0

		moveSpeed = moveSpeed * (0.334f * currDifficultyMod); //enemies move slower on easier difficulties
		attentionSpanRange = currDifficultyMod > 1 ? defaultAttentionSpanRange * currDifficultyMod * 0.6f : defaultAttentionSpanRange; //enemies stay focused longer on harder difficulties
		targetIdentifyRate = defaultTargetIdentifyRate / currDifficultyMod; //enemies check for target more frequently on harder difficulties
		maxChaseTime = 2f * currDifficultyMod; //enemies chase for longer time on harder difficulties
		maxChaseDistance = 2f * currDifficultyMod; //enemies will go after you for further distances on harder difficulties

		prevGameDifficulty = GameManager_SwordSwipe.currDifficulty;
	}
}
