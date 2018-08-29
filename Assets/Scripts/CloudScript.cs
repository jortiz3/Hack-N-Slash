using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CloudScript : MonoBehaviour {

	private static Vector3 tripleScreenDimensionsInWorldUnits;
	private static Vector3 doubleScreenDimensionsInWorldUnits;

	private Vector3 speed;
	private float delay;
	private SpriteRenderer sr;

	void FixedUpdate () {
		if (GameManager_SwordSwipe.currGameState == GameState.Active) { //if the game is active
			if (sr.enabled) { //if the cloud is visible
				transform.position += speed; //move the cloud

				if ((transform.position.x < Camera.main.transform.position.x - 11 && speed.x < 0) || //cloud is left of screen and cloud is moving left
					(transform.position.x > Camera.main.transform.position.x + 11 && speed.x > 0)) { //or cloud is right of screen and cloud is moving right
					sr.enabled = false; //hide cloud
					Prepare (); //prepare cloud for next movement
				}
			} else if (delay > 0) { //if cloud is still in delay
				delay -= Time.deltaTime; //decrement delay
			} else { //delay is over
				sr.enabled = true; //show cloud
			}
		}
	}

	private void Prepare() {
		Vector3 deltaPosition = Camera.main.transform.position + (Vector3.forward * 10); //start with camera position -- negate the z axis

		if (Random.Range (0f, 100f) < 50f) { //randomly select this option
			deltaPosition += new Vector3 (Random.Range(-tripleScreenDimensionsInWorldUnits.x, -doubleScreenDimensionsInWorldUnits.x),
				Random.Range (doubleScreenDimensionsInWorldUnits.y, tripleScreenDimensionsInWorldUnits.y), 0); //place cloud on left side with variation
			speed = new Vector3(Random.Range (0.001f, 0.005f), 0);
		} else { //else
			deltaPosition += new Vector3 (Random.Range(doubleScreenDimensionsInWorldUnits.x, tripleScreenDimensionsInWorldUnits.x),
				Random.Range (doubleScreenDimensionsInWorldUnits.y, tripleScreenDimensionsInWorldUnits.y), 0);
			speed = new Vector3(Random.Range (-0.005f, -0.001f), 0);
		}

		transform.position += deltaPosition;

		delay = Random.Range (1f, 60f);

	}

	void Start () {
		if (tripleScreenDimensionsInWorldUnits.Equals(Vector3.zero)) {
			Vector3 screensize = Camera.main.ScreenToWorldPoint (new Vector3 (Screen.width, Screen.height, 0)); //get the screen size
			tripleScreenDimensionsInWorldUnits = new Vector3(screensize.x * 3f, screensize.y * 1.5f, 0); //triple screen size (sort of don't want the clouds going too high)
			doubleScreenDimensionsInWorldUnits = new Vector3(screensize.x * 2f, screensize.y * 1.2f, 0); //double screen size
		}

		sr = GetComponent<SpriteRenderer> ();
		sr.enabled = false;

		Prepare ();
	}
}
