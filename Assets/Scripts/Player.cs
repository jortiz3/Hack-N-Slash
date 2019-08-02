using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Player : Character {

	[SerializeField]
	private string defaultWeapon;
	public List<string> weaponSpecialization;
	[SerializeField]
	private bool infiniteRespawn;
	[SerializeField]
	private int numOfRespawnsRemaining = 3;
	[SerializeField]
	private int unlock_cost;
	[SerializeField]
	private string unlock_challenge;

	public int NumberOfRespawnsRemaining { get { return numOfRespawnsRemaining; } set { numOfRespawnsRemaining = value; } }
	public int Unlock_Cost { get { return unlock_cost; } }
	public string Unlock_Challenge { get { return unlock_challenge; } }
	public string DefaultWeapon { get { return defaultWeapon; } }

	void Awake() {
		if (player != null) //if there is a player already
			player.Die (); //tell that player to die, there is a new player now
		player = this; //lets all of the character objects know that this is the player

		Initialize ();
	}

	public override void Die () {
		if (infiniteRespawn) {
			Respawn (GameManager_SwordSwipe.currPlayerSpawnLocation);
		} else if (numOfRespawnsRemaining > 0) {
			Respawn (GameManager_SwordSwipe.currPlayerSpawnLocation);
			numOfRespawnsRemaining--;
		} else {
			if (GameManager_SwordSwipe.currGameState == GameState.Active) {
				switch (GameManager_SwordSwipe.currGameMode) {
				case GameMode.Campaign:
					GameManager_SwordSwipe.instance.FailCurrentCampaignMission (); //fail mission
					break;
				case GameMode.Survival:
					GameManager_SwordSwipe.instance.EndSurvivalWave ("Player Death");
					break;
				}
			}
			base.Die ();
		}
	}

	void FixedUpdate() {
		UpdateAnimations ();
	}

	public void ReceivePlayerInput (string input) {
		if (input.Contains("Run")) {
			input = input.Replace("Run", "");
			Run(float.Parse(input));
		} else if (input.Equals("Jump")) {
			Jump();
		} else if (input.Equals("Crouch/DropThrough")) {
			if (isOnAPlatform) {
				DropThroughPlatform();
			} else {
				Crouch();
			}
		} else if (input.Equals("UnCrouch")) {
			UnCrouch();
		} else {
			Attack(input);
		}
	}

	void Start() {
		if (cameraCanvas == null)
			cameraCanvas = GameObject.FindGameObjectWithTag ("Camera Canvas").transform;

		attackTimerSlider = (GameObject.Instantiate (Resources.Load ("UI/attackTimerSlider"), cameraCanvas) as GameObject).GetComponent<Slider>();
		attackTimerSlider.gameObject.name = gameObject.name + "'s attack timer slider";
		attackTimerSlider.GetComponent<RectTransform> ().sizeDelta = new Vector2 (Screen.width / 15f, Screen.height / 15f);
		attackTimerSlider.gameObject.SetActive (false);

		Initialize();
	}

	#if UNITY_EDITOR
	void Update () {
		if (GameManager_SwordSwipe.currGameState == GameState.Active) {
			if (Input.GetAxisRaw ("Horizontal") != 0) //horizontal button(s) held down; can be multiple frames
				Run ((int)Input.GetAxisRaw ("Horizontal"));
			else if (Input.GetButtonUp ("Horizontal") == true) //first frame horizontal buttons released
				Run (0);

			if (Input.GetButtonDown ("Attack") == true) {//first frame button pressed
				if (Input.GetAxis ("Vertical") > 0)//holding up
					Attack ("Attack_Up");
				//else if (Input.GetAxis ("Vertical") < 0)//holding down
					//Attack ("Attack_Down");
				else if (Input.GetAxis ("Horizontal") > 0) {//holding right
					if (isFacingRight)
						Attack ("Attack_Forward");
					else
						Attack ("Attack_Backward");
				} else if (Input.GetAxis ("Horizontal") < 0) {//holding left
					if (isFacingLeft)
						Attack ("Attack_Forward");
					else
						Attack ("Attack_Backward");
				} else {
					Attack ("Attack");
				}
			}

			if (Input.GetAxis ("Vertical") > 0) //button is held down; can be multiple frames
				Jump ();	
		}
	}
	#endif
}