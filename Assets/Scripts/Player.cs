using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Player : Character {
	struct TouchInfo {
		float startTime;
		Vector2 startPosition;

		public TouchInfo(float StartTime, Vector2 StartPosition) {
			startTime = StartTime;
			startPosition = StartPosition;
		}

		public string GetEndPhase (float EndTime, Vector2 EndPosition) {
			float deltaTime = EndTime - startTime;
			Vector2 deltaPosition = EndPosition - startPosition;

			if (deltaPosition.magnitude > 100) { //if the swipe is long enough
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
			} else if (deltaTime < 0.12f) { //briefly tapped the screen
				return "jump";
			}

			return "";
		}
	}

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
	}

	void Start() {
		if (cameraCanvas == null)
			cameraCanvas = GameObject.FindGameObjectWithTag ("Camera Canvas").transform;

		attackTimerSlider = (GameObject.Instantiate (Resources.Load ("UI/attackTimerSlider"), cameraCanvas) as GameObject).GetComponent<Slider>();
		attackTimerSlider.gameObject.name = gameObject.name + "'s attack timer slider";

		touchInfo = new List<TouchInfo> ();

		Initialize ();
	}

	void Update () {
		if (GameManager.currGameState == GameState.Active) {
			if (Input.touchCount > 0) { //if the player is touching the screen
				Touch currTouch;
				for (int i = 0; i < Input.touchCount; i++) {
					currTouch = Input.GetTouch (i);

					switch (currTouch.phase) {
					case TouchPhase.Began:
						touchInfo.Insert (i, new TouchInfo (Time.time, currTouch.position));
						break;
					case TouchPhase.Stationary:
						Vector3 temp = Camera.main.ScreenToWorldPoint (currTouch.position);

						if (temp.x > transform.position.x + 0.1f) { //holding to the right of the character
							Move (1);
						} else if (temp.x < transform.position.x - 0.1f) { //holding to the left of the character
							Move (-1);
						} else {
							Move (0);
						}
						break;
					case TouchPhase.Ended:
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
							Attack ("Attack_Down");
							break;
						case "jump":
							Jump ();
							break;
						default:
							Move (0);
							break;
						}

						touchInfo.RemoveAt (i);
						break;
					}
				}
			}

			#if UNITY_EDITOR
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
			#endif
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