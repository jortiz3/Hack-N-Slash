using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

//animation bugs:
//	--attack forwards/backwards from run/idle
//	--getting damaged while attacking

public class Player : Character {
	[SerializeField]
	private bool infiniteRespawn;
	[SerializeField]
	private int numOfRespawnsRemaining = 3;

	private List<TouchInfo> touchInfo;

	public int NumberOfRespawnsRemaining { get { return numOfRespawnsRemaining; } set { numOfRespawnsRemaining = value; } }

	void Awake() {
		if (player != null) //if there is a player already
			player.Die (); //tell that player to die, there is a new player now
		player = this; //lets all of the character objects know that this is the player

		Initialize ();
	}

	void Start() {
		if (cameraCanvas == null)
			cameraCanvas = GameObject.FindGameObjectWithTag ("Camera Canvas").transform;

		attackTimerSlider = (GameObject.Instantiate (Resources.Load ("UI/attackTimerSlider"), cameraCanvas) as GameObject).GetComponent<Slider>();
		attackTimerSlider.gameObject.name = gameObject.name + "'s attack timer slider";
		attackTimerSlider.GetComponent<RectTransform> ().sizeDelta = new Vector2 (Screen.width / 15f, Screen.height / 15f);
		attackTimerSlider.gameObject.SetActive (false);

		touchInfo = new List<TouchInfo> ();
	}

	#if UNITY_EDITOR
	void Update () {
		if (GameManager.currGameState == GameState.Active) {
			if (Input.GetAxisRaw ("Horizontal") != 0) //horizontal button(s) held down; can be multiple frames
				Move ((int)Input.GetAxisRaw ("Horizontal"));
			else if (Input.GetButtonUp ("Horizontal") == true) //first frame horizontal buttons released
				Move (0);

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

	void FixedUpdate() {
		if (GameManager.currGameState == GameState.Active) {
			if (Input.touchCount > 0) { //if the player is touching the screen
				Touch currTouch;
				for (int i = 0; i < Input.touchCount; i++) {
					currTouch = Input.GetTouch (i);

					if (currTouch.phase == TouchPhase.Began) {
						touchInfo.Insert (i, new TouchInfo (Time.time, currTouch.position));
					} else if (currTouch.phase == TouchPhase.Ended) {
						string endPhase = touchInfo [i].GetEndPhase (Time.time, currTouch.position);

						switch (endPhase) {
						case "swipe_left":
							if (isFacingLeft)
								Attack ("Attack_Forward");
							else
								Attack ("Attack_Backward");
							break;
						case "swipe_right":
							if (isFacingRight)
								Attack ("Attack_Forward");
							else
								Attack ("Attack_Backward");
							break;
						case "swipe_up":
							Attack ("Attack_Up");
							break;
						case "swipe_down":
							Attack ("Attack");
							//Attack ("Attack_Down");
							break;
						case "jump":
							Jump ();
							break;
						default:
							Move (0); //transition to idle
							break;
						}

						touchInfo.RemoveAt (i);
					} else if (touchInfo[i].deltaTime > 0.06f) {
						Vector3 temp = Camera.main.ScreenToWorldPoint (currTouch.position);

						if (temp.x > transform.position.x + 0.1f) { //holding to the right of the character
							Move (1);
						} else if (temp.x < transform.position.x - 0.1f) { //holding to the left of the character
							Move (-1);
						} else { //holding directly above or on character
							Move (0); //transition to idle
						}
					}//end touch phase if
				} //end for loop
			}//end touch count if
		}//end gamestate if

		UpdateAnimations ();
	}

	public override void Die () {
		if (infiniteRespawn) {
			Respawn (Vector3.zero);
		} else if (numOfRespawnsRemaining > 0) {
			Respawn (Vector3.zero);
			numOfRespawnsRemaining--;
		} else {
			if (GameManager.currGameState == GameState.Active) {
				switch (GameManager.currGameMode) {
				case GameMode.Story:
					break;
				case GameMode.Survival:
					GameManager.currGameManager.EndSurvivalWave ("died");
					break;
				}
			}
			base.Die ();
		}
	}


	struct TouchInfo {
		float startTime;
		Vector2 startPosition;

		public float deltaTime { get { return Time.time - startTime; } }

		public TouchInfo(float StartTime, Vector2 StartPosition) {
			startTime = StartTime;
			startPosition = StartPosition;
		}

		public string GetEndPhase (float EndTime, Vector2 EndPosition) {
			float deltaTime = EndTime - startTime;
			Vector2 deltaPosition = EndPosition - startPosition;

			if (deltaPosition.magnitude > 50) { //if the swipe is long enough
				//dot product gives us the cosine of the angle between 2 vectors; so, we need to get the arccosine
				float angle = Mathf.Acos (Vector2.Dot (Vector2.right, deltaPosition.normalized)); //angle between the swipe and directly to the right

				if (angle < 0.78 || angle >= 5.49) {
					return "swipe_right";
				} else if (angle < 2.35) {
					return "swipe_up";
				} else if (angle < 3.92) {
					return "swipe_left";
				} else if (angle < 5.49) {
					return "swipe_down";
				}
			} else if (deltaTime < 0.15f) { //briefly tapped the screen
				return "jump";
			}

			return "";
		}
	}
}