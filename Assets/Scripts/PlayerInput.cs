//Written by Justin Ortiz

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

	private bool editModeEnabled;
	private RectTransform editObject;
	private float scaleSpeed;
	private float prevTouchDifference;
	private float currTouchDifference;
	private Vector2 firstTouchPrevPos;
	private Vector2 secondTouchPrevPos;

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

	public void ExitEditMode() {
		//save positions of all 3 buttons in playerprefs
		RectTransform temp = joystick.GetComponent<RectTransform> ();
		PlayerPrefs.SetFloat("Joystick.x", temp.anchoredPosition.x);
		PlayerPrefs.SetFloat("Joystick.y", temp.anchoredPosition.y);
		PlayerPrefs.SetFloat("Joystick.scale.x", temp.localScale.x);
		PlayerPrefs.SetFloat("Joystick.scale.y", temp.localScale.y);

		PlayerPrefs.SetFloat("JumpButton.x", jumpButton.anchoredPosition.x);
		PlayerPrefs.SetFloat("JumpButton.y", jumpButton.anchoredPosition.y);
		PlayerPrefs.SetFloat("JumpButton.scale.x", jumpButton.localScale.x);
		PlayerPrefs.SetFloat("JumpButton.scale.y", jumpButton.localScale.y);

		PlayerPrefs.SetFloat("AttackButton.x", attackButton.anchoredPosition.x);
		PlayerPrefs.SetFloat("AttackButton.y", attackButton.anchoredPosition.y);
		PlayerPrefs.SetFloat("AttackButton.scale.x", attackButton.localScale.x);
		PlayerPrefs.SetFloat("AttackButton.scale.y", attackButton.localScale.y);

		PlayerPrefs.Save (); //save the prefs

		joystick.GetComponent<SingleJoystick> ().SetCornerPivot(); //ensure the joystick works properly again

		editObject = null; //remove pointer to current edit object
		editModeEnabled = false; //exit edit mode
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
						} else if (touchInfo [i].deltaTime > 0.02f) {
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
		}// end gamestate if
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

		for (int i = 1; i < 2; i++) { //reposition and scale buttons
			attackButton.transform.GetChild (i).gameObject.SetActive (true);
			jumpButton.transform.GetChild (i).gameObject.SetActive (true);
			joystick.transform.GetChild (i).gameObject.SetActive (true);
		}
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

		for (int i = 1; i < 2; i++) { //reposition and scale buttons
			attackButton.transform.GetChild (i).gameObject.SetActive (false);
			jumpButton.transform.GetChild (i).gameObject.SetActive (false);
			joystick.transform.GetChild (i).gameObject.SetActive (false);
		}
	}

	public void SetControlScheme(int controlScheme) {
		currControlScheme = (ControlScheme)controlScheme;
	}

	public void SetDefaultButtonPosition() {
		joystick.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (Screen.width * 0.08f, Screen.height * 0.2f);
		Vector2 joystickSize = new Vector2 (Screen.width * 0.1f, Screen.height * 0.2f);
		joystick.GetComponent<RectTransform> ().sizeDelta = joystickSize; //size of the joystick background
		joystick.transform.GetChild (0).GetComponent<RectTransform> ().sizeDelta = joystickSize * 0.8f; //size of the joystick handle
		joystick.transform.localScale = Vector3.one;

		attackButton.anchoredPosition = new Vector2 (Screen.width * 0.92f, Screen.height * 0.2f);
		attackButton.sizeDelta = joystickSize * 0.75f; //set the size of the attack button
		attackButton.transform.localScale = Vector3.one;
		jumpButton.anchoredPosition = new Vector2 (Screen.width * 0.82f, Screen.height * 0.1f);
		jumpButton.sizeDelta = attackButton.sizeDelta; //mirror the size of the attack button
		jumpButton.transform.localScale = Vector3.one;
	}

	void Start() {
		controlMenuParent = GameObject.Find ("Controls").transform.Find("Default");
		hudMenuParent = GameObject.Find ("HUD").transform;
		attackButton = GameObject.Find ("Attack Button").GetComponent<RectTransform> (); //get the attack button
		jumpButton = GameObject.Find ("Jump Button").GetComponent<RectTransform> (); //get the jump button
		joystick = GameObject.Find ("Joystick").GetComponent<SingleJoystick> (); //get the joystick

		if (PlayerPrefs.GetFloat ("Joystick.x", -9999.999f) == -9999.999f) {
			SetDefaultButtonPosition ();
		} else {
			joystick.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (PlayerPrefs.GetFloat("Joystick.x"), PlayerPrefs.GetFloat("Joystick.y"));
			joystick.transform.localScale = new Vector3 (PlayerPrefs.GetFloat("Joystick.scale.x"), PlayerPrefs.GetFloat("Joystick.scale.y"));

			attackButton.anchoredPosition = new Vector2 (PlayerPrefs.GetFloat("AttackButton.x"), PlayerPrefs.GetFloat("AttackButton.y"));
			attackButton.transform.localScale = new Vector3 (PlayerPrefs.GetFloat("AttackButton.scale.x"), PlayerPrefs.GetFloat("AttackButton.scale.y"));

			jumpButton.anchoredPosition = new Vector2 (PlayerPrefs.GetFloat("JumpButton.x"), PlayerPrefs.GetFloat("JumpButton.y"));
			jumpButton.transform.localScale = new Vector3 (PlayerPrefs.GetFloat("JumpButton.scale.x"), PlayerPrefs.GetFloat("JumpButton.scale.y"));
		}

		if (currControlScheme == ControlScheme.Buttonless) {
			GameObject.Find ("Control Scheme Toggle Group").transform.Find ("Buttonless").GetComponent<Toggle> ().isOn = true;
			RevertControlMenu ();
		}

		touchInfo = new List<TouchInfo> ();

		scaleSpeed = 0.005f;
	}

	public void StartEditMode (RectTransform ObjectToEdit) {
		joystick.GetComponent<SingleJoystick> ().SetCenterPivot();

		editObject = ObjectToEdit; //only edit one object at a time to decrease confusion and frustration of overlapping
		editModeEnabled = true; //lets the code know we are editing buttons now
	}

	void Update() {
		if (editModeEnabled) {
			if (Input.touchCount == 1) { //reposition
				Vector2 currTouchPos = Input.GetTouch(0).position; //get the current touch
				if ((currTouchPos - (Vector2)editObject.position).magnitude < editObject.sizeDelta.magnitude / 2) { //if the touch is close enough to the current object
					float marginWidth = Screen.width * 0.05f; //get the current margin the left and right sides of the screen
					if (currTouchPos.x > marginWidth && currTouchPos.x < Screen.width - marginWidth) { //ensure the player is dragging between the margins
						float marginHeight = Screen.height * 0.08f; //get the current margin between the top and bottom sides of the screen
						if (currTouchPos.y > marginHeight && currTouchPos.y < Screen.height - marginHeight) { //ensure the player is draggin between the margins
							editObject.position = currTouchPos; //move the object with the touch
						}
					}
				}
			} else if (Input.touchCount == 2) { //scale if 2 fingers being used
				Touch firstTouch = Input.GetTouch(0);
				Touch secondTouch = Input.GetTouch (1); //get both fingers

				firstTouchPrevPos = firstTouch.position - firstTouch.deltaPosition;
				secondTouchPrevPos = secondTouch.position - secondTouch.deltaPosition; //get the previous positions for both fingers

				prevTouchDifference = (firstTouchPrevPos - secondTouchPrevPos).magnitude;
				currTouchDifference = (firstTouch.position - secondTouch.position).magnitude; //get the difference in positions

				float ScaleModifier = (firstTouch.deltaPosition - secondTouch.deltaPosition).magnitude * scaleSpeed; //how much will we be changing the scale this frame
				Vector3 newScale = editObject.localScale; //get the current scale
				if (prevTouchDifference > currTouchDifference) { //previous distance greater than current -- fingers pinching -- we want the object to get smaller
					newScale = new Vector3(Mathf.Clamp(newScale.x - ScaleModifier,0.5f,2f),Mathf.Clamp(newScale.y - ScaleModifier,0.5f,2f),Mathf.Clamp(newScale.z - ScaleModifier,0.5f,2f)); //decrease scale, but clamp
				} else if (prevTouchDifference < currTouchDifference) { //previous distance less than current -- fingers spreading -- we want the object to get bigger
					newScale = new Vector3(Mathf.Clamp(newScale.x + ScaleModifier,0.5f,2f),Mathf.Clamp(newScale.y + ScaleModifier,0.5f,2f),Mathf.Clamp(newScale.z + ScaleModifier,0.5f,2f)); //increase scale, but clamp
				}
				editObject.localScale = newScale; //apply the new scale
			}
		} //end edit enabled
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

				if (angle < 1.04 || angle >= 5.23) {
					return "swipe_right";
				} else if (angle < 2.09) {
					return "swipe_up";
				} else if (angle < 4.18) {
					return "swipe_left";
				} else if (angle < 5.49) {
					return "swipe_down";
				}
			} else if (deltaTime < 0.2f) { //briefly tapped the screen
				return "jump";
			}

			return "";
		}
	}
}