using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour {

	private SingleJoystick joystick;
	private Vector3 joystickDirection;
	private float joystickAngle;


	public void Attack() {
		if (Character.player.isRunning) {
			Character.player.ReceivePlayerInput ("Attack_Forward");
		} else {
			Character.player.ReceivePlayerInput ("Attack");
		}
	}

	void Awake() {
		joystick = GameObject.Find ("Joystick").GetComponent<SingleJoystick> (); //get the joystick
	}

	void FixedUpdate() {
		joystickDirection = joystick.GetInputDirection ();
		if (joystickDirection != Vector3.zero) {
			joystickAngle = Mathf.Acos (Vector3.Dot (Vector3.right, joystickDirection)); //angle between the swipe and directly to the right

			if (joystickAngle < 1.04 || joystickAngle >= 5.23) {
				Character.player.ReceivePlayerInput ("Run" + joystickDirection.x.ToString());
			} else if (joystickAngle < 2.09) {
				//holding up "swipe_up";
			} else if (joystickAngle < 4.18) {
				Character.player.ReceivePlayerInput ("Run" + joystickDirection.x.ToString());
			} else if (joystickAngle < 5.49) {
				//holding down "swipe_down";
			}
		}
	}

	public void Jump() {
		Character.player.ReceivePlayerInput ("Jump");
	}
}