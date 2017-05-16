using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//implement directional attacks after first swing -- first swing stops player and enemy movement
//	-down swings from top to bottom; knocks enemy downwards
//	-up swings from bottom to top; knocks enemy upwards
//	-opposite current face direction keeps enemy in place? or maybe grabs enemy?
//	-towards current face direction does power attack?
//	-standard attack keeps enemy and player in place until end of attack chain

public class Player : Character {

	[SerializeField]
	private bool infiniteRespawn;
	[SerializeField]
	private int numOfRespawnsRemaining = 3;

	void Start() {
		if (cameraCanvas == null)
			cameraCanvas = GameObject.FindGameObjectWithTag ("Camera Canvas").transform;

		attackTimerSlider = (GameObject.Instantiate (Resources.Load ("attackTimerSlider"), cameraCanvas) as GameObject).GetComponent<Slider>();
		attackTimerSlider.gameObject.name = gameObject.name + "'s attack timer slider";
		
		Initialize ();

		player = this; //lets all of the character objects know that this is the player
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetAxisRaw("Horizontal") != 0) //horizontal button(s) held down; can be multiple frames
			Move ((int)Input.GetAxisRaw("Horizontal"));
		else if (Input.GetButtonUp("Horizontal") == true) //first frame horizontal buttons released
			Move (0);

		if (Input.GetButtonDown ("Attack") == true) {//first frame button pressed
			if (Input.GetAxis ("Vertical") > 0)//holding up
				Attack ("Attack_Up");
			else if (Input.GetAxis ("Vertical") < 0)//holding down
				Attack ("Attack_Down");
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

		if (Input.GetButton("Vertical") == true) //button is held down; can be multiple frames
			Jump ();
	}

	void FixedUpdate() {
		UpdateAnimations ();
	}

	protected override void Die () {
		if (infiniteRespawn) {
			Respawn (Vector3.zero);
		} else if (numOfRespawnsRemaining > 0) {
			Respawn (Vector3.zero);
			numOfRespawnsRemaining--;
		} else {
			base.Die ();
		}
	}
}
