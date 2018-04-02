using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
			if (Input.touchCount > 0) { //if the player is touching the screen
				Touch currTouch;
				for (int i = 0; i < Input.touchCount; i++) {
					currTouch = Input.GetTouch (i);

					switch (currTouch.phase) {
					case TouchPhase.Stationary:
						if (currTouch.deltaTime > 0.12f) { //holding for 0.12 seconds
							if (currTouch.position.x > transform.position.x + 5) { //holding to the right of the character
								Move (1);
							} else if (currTouch.position.x < transform.position.x - 5) { //holding to the left of the character
								Move (-1);
							} else {
								Move (0);
							}
						}
						break;
					case TouchPhase.Moved:
						if (currTouch.deltaTime < 0.3f && currTouch.deltaPosition.magnitude > 30) { //if the touch moved quickly and at least 30 pixels

							float angle = Mathf.Acos(Vector2.Dot(Vector2.right, currTouch.deltaPosition)); //dot product gets us the cosine of the angle between vectors; so we get the arccosine to get the angle (in radians)

							if (angle < 0.78 || angle >= 5.49) { //swipe right
								if (isFacingRight)
									Attack ("Attack_Forward");
								else
									Attack ("Attack_Backward");
							} else if (angle < 2.35) { //swipe up
								Attack ("Attack_Up");
							} else if (angle < 3.92) { //swipe left
								if (isFacingLeft)
									Attack ("Attack_Forward");
								else
									Attack ("Attack_Backward");
							} else if (angle < 5.49) { //swipe down
								Attack ("Attack_Down");
							}
						}
						break;
					case TouchPhase.Ended:
						if (currTouch.tapCount >= 1)
							Jump ();
						/*else if (currTouch.deltaPosition.magnitude < 30) //not sure if will need to end movement
							Move (0);*/
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