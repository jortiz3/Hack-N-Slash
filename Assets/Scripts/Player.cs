using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//change from keyboard to touch input
//--tap>>jump
//--directional swipe>>directional attack

public class Player : Character {

	[SerializeField]
	private bool infiniteRespawn;
	[SerializeField]
	private int numOfRespawnsRemaining = 3;

	public int NumberOfRespawnsRemaining { get { return numOfRespawnsRemaining; } set { numOfRespawnsRemaining = value; } }

	void Awake() {
		if (player != null) //if there is a player already
			player.Die (); //tell that player to die, there is a new player now
		player = this; //lets all of the character objects know that this is the player
	}

	void Start() {
		if (cameraCanvas == null)
			cameraCanvas = GameObject.FindGameObjectWithTag ("Camera Canvas").transform;

		attackTimerSlider = (GameObject.Instantiate (Resources.Load ("UI/attackTimerSlider"), cameraCanvas) as GameObject).GetComponent<Slider>();
		attackTimerSlider.gameObject.name = gameObject.name + "'s attack timer slider";
		
		Initialize ();
	}

	void Update () {
		if (GameManager.currGameState == GameState.Active) {

			if (Input.touchCount > 0) {
				Touch currTouch;
				for (int i = 0; i < Input.touchCount; i++) {
					currTouch = Input.GetTouch (i);

					switch (currTouch.phase) {
					case TouchPhase.Stationary:
						//if deltatime > 0.15f
						//if touchpos.x > player.x
						//move right
						//if touchpos.x < player.x
						//move left
						break;
					case TouchPhase.Moved:
						if (currTouch.deltaTime < 0.3f && currTouch.deltaPosition.magnitude > 50) {
							//get attack direction
						}
						break;
					case TouchPhase.Ended:
						if (currTouch.tapCount >= 2)
							Jump ();
						break;
					case TouchPhase.Canceled:
						break;
					}
				}
			}

			#region PCControls
			if (Input.GetAxisRaw ("Horizontal") != 0) //horizontal button(s) held down; can be multiple frames
				Move ((int)Input.GetAxisRaw ("Horizontal"));
			else if (Input.GetButtonUp ("Horizontal") == true) //first frame horizontal buttons released
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

			if (Input.GetAxis ("Vertical") > 0) //button is held down; can be multiple frames
				Jump ();
			#endregion
		}
	}

	void FixedUpdate() {
		UpdateAnimations ();
	}

	public override void Die () {
		if (infiniteRespawn) {
			Respawn (Vector3.zero);
		} else if (numOfRespawnsRemaining > 0) {
			Respawn (Vector3.zero);
			numOfRespawnsRemaining--;
		} else {
			GameManager.currGameManager.ShowSurvivalLose ();
			base.Die ();
		}
	}
}