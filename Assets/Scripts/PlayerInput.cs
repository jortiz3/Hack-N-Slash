using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ControlScheme { Default, Buttonless };

public class PlayerInput : MonoBehaviour {

	public static ControlScheme currControlScheme;

	private Transform controlMenuParent;
	private Transform hudMenuParent;
	private RectTransform attackButton;
	private RectTransform jumpButton;
	private SingleJoystick joystick;
	private Vector3 joystickDirection;
	private float joystickAngle;

	private List<TouchInfo> touchInfo;

	private bool joystick_left;
	private bool joystick_right;
	private bool joystick_up;
	private bool joystick_down;

	public void Attack() {
		if (joystickDirection != Vector3.zero) { //holding the joy stick
			if (joystick_up) {
				Character.player.ReceivePlayerInput ("Attack_Up");
			} else if (joystick_down) {
				Character.player.ReceivePlayerInput ("Attack_Up"); //Attack_Down
			}else if (Character.player.isFacingRight) {
				if (joystick_left) {
					Character.player.ReceivePlayerInput ("Attack_Backward");
				} else if (joystick_right) {
					Character.player.ReceivePlayerInput ("Attack_Forward");
				}
			} else if (Character.player.isFacingLeft) {
				if (joystick_left) {
					Character.player.ReceivePlayerInput ("Attack_Forward");
				} else if (joystick_right) {
					Character.player.ReceivePlayerInput ("Attack_Backward");
				}
			}
		} else {
			Character.player.ReceivePlayerInput ("Attack");
		}
	}

	void Awake() {
		controlMenuParent = GameObject.Find ("Controls").transform.Find("Default");
		hudMenuParent = GameObject.Find ("HUD").transform;
		attackButton = GameObject.Find ("Attack Button").GetComponent<RectTransform> (); //get the attack button
		jumpButton = GameObject.Find ("Jump Button").GetComponent<RectTransform> (); //get the jump button
		joystick = GameObject.Find ("Joystick").GetComponent<SingleJoystick> (); //get the joystick

		joystick.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (Screen.width * 0.08f, Screen.height * 0.2f);
		Vector2 joystickSize = new Vector2 (Screen.width * 0.1f, Screen.height * 0.2f);
		joystick.GetComponent<RectTransform> ().sizeDelta = joystickSize; //size of the joystick background
		joystick.transform.GetChild (0).GetComponent<RectTransform> ().sizeDelta = joystickSize * 0.8f; //size of the joystick handle

		attackButton.anchoredPosition = new Vector2 (Screen.width * 0.92f, Screen.height * 0.2f);
		attackButton.sizeDelta = joystickSize * 0.75f; //set the size of the attack button
		jumpButton.anchoredPosition = new Vector2 (Screen.width * 0.82f, Screen.height * 0.1f);
		jumpButton.sizeDelta = attackButton.sizeDelta; //mirror the size of the attack button

		touchInfo = new List<TouchInfo> ();
	}

	void FixedUpdate() {
		if (GameManager.currGameState == GameState.Active) {
			if (PlayerInput.currControlScheme == ControlScheme.Buttonless) {
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
								if (Character.player.isFacingLeft)
									Character.player.ReceivePlayerInput("Attack_Forward");
								else
									Character.player.ReceivePlayerInput ("Attack_Backward");
								break;
							case "swipe_right":
								if (Character.player.isFacingRight)
									Character.player.ReceivePlayerInput ("Attack_Forward");
								else
									Character.player.ReceivePlayerInput ("Attack_Backward");
								break;
							case "swipe_up":
								Character.player.ReceivePlayerInput ("Attack_Up");
								break;
							case "swipe_down":
								Character.player.ReceivePlayerInput ("Attack");
								//Attack ("Attack_Down");
								break;
							case "jump":
								Character.player.ReceivePlayerInput ("Jump");
								break;
							default:
								Character.player.ReceivePlayerInput ("Run0"); //transition to idle
								break;
							}

							touchInfo.RemoveAt (i);
						} else if (touchInfo [i].deltaTime > 0.06f) {
							Vector3 temp = Camera.main.ScreenToWorldPoint (currTouch.position);

							if (temp.x > transform.position.x + 0.1f) { //holding to the right of the character
								Character.player.ReceivePlayerInput ("Run1");
							} else if (temp.x < transform.position.x - 0.1f) { //holding to the left of the character
								Character.player.ReceivePlayerInput ("Run-1");
							} else { //holding directly above or on character
								Character.player.ReceivePlayerInput ("Run0"); //transition to idle
							}
						}//end touch phase if
					} //end for loop
				}//end touch count if
			} else if (currControlScheme == ControlScheme.Default) {
				joystickDirection = joystick.GetInputDirection ();
				if (joystickDirection != Vector3.zero) {
					joystickAngle = Mathf.Acos (Vector3.Dot (Vector3.right, joystickDirection)); //angle between the swipe and directly to the right

					if (joystickAngle < 1.04 || joystickAngle >= 5.23) { //holding right
						Character.player.ReceivePlayerInput ("Run" + joystickDirection.x.ToString ());
						joystick_right = true;
						joystick_left = false;
						joystick_down = false;
						joystick_up = false;
					} else if (joystickAngle < 2.09) { //holding up
						joystick_right = false;
						joystick_left = false;
						joystick_down = false;
						joystick_up = true;
					} else if (joystickAngle < 4.18) { //holding left
						Character.player.ReceivePlayerInput ("Run" + joystickDirection.x.ToString ());
						joystick_right = false;
						joystick_left = true;
						joystick_down = false;
						joystick_up = false;
					} else if (joystickAngle < 5.49) { //holding down
						joystick_right = false;
						joystick_left = false;
						joystick_down = true;
						joystick_up = false;
					}
				} else {
					joystick_right = false;
					joystick_left = false;
					joystick_down = false;
					joystick_up = false;
				}
			}//end control scheme if
		}//end gamestate if
	}

	public void Jump() {
		Character.player.ReceivePlayerInput ("Jump");
	}

	public void PrepareControlMenu() {
		attackButton.transform.SetParent (controlMenuParent);
		jumpButton.transform.SetParent (controlMenuParent);
		joystick.transform.SetParent (controlMenuParent);

		attackButton.gameObject.SetActive (true);
		jumpButton.gameObject.SetActive (true);
		joystick.gameObject.SetActive (true);

		attackButton.GetComponent<Button> ().interactable = false;
		jumpButton.GetComponent<Button> ().interactable = false;

		attackButton.transform.GetChild (1).gameObject.SetActive (true);
		jumpButton.transform.GetChild (1).gameObject.SetActive (true);
		joystick.transform.GetChild (1).gameObject.SetActive (true);
	}

	public void RevertControlMenu() {
		attackButton.transform.SetParent (hudMenuParent);
		jumpButton.transform.SetParent (hudMenuParent);
		joystick.transform.SetParent (hudMenuParent);

		if (currControlScheme == ControlScheme.Buttonless) {
			attackButton.gameObject.SetActive (false);
			jumpButton.gameObject.SetActive (false);
			joystick.gameObject.SetActive (false);
		}

		attackButton.GetComponent<Button> ().interactable = true;
		jumpButton.GetComponent<Button> ().interactable = true;

		attackButton.transform.GetChild (1).gameObject.SetActive (false);
		jumpButton.transform.GetChild (1).gameObject.SetActive (false);
		joystick.transform.GetChild (1).gameObject.SetActive (false);
	}

	public void SetControlScheme(int controlScheme) {
		currControlScheme = (ControlScheme)controlScheme;
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