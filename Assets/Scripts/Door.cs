using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Door : MonoBehaviour {

	public static bool TutorialDisplayed;

	[SerializeField]
	private Item keyToUnlockDoor;
	[SerializeField]
	private AudioClip lockedSoundEffect;
	[SerializeField]
	private AudioClip openedSoundEffect;
	private AudioSource aSource;
	[SerializeField]
	private Cutscene cutsceneToPlay;
	[SerializeField]
	private Transform locationToMoveTo;
	[SerializeField]
	private float moveTimeDelay;

	private List<GameObjectToMove> charactersToMove;

	public void EnterDoor(Character characterAttemptingToEnter) {
		if (keyToUnlockDoor == null || characterAttemptingToEnter.HasItem (keyToUnlockDoor)) {
			if (locationToMoveTo != null) { //if there is a location to move too
				MoveGameObject (characterAttemptingToEnter.gameObject); //add the player to the list

				if (GameManager.SoundEnabled && openedSoundEffect != null) { //sound enabled && sound effect available
					aSource.volume = GameManager.SFXVolume;
					aSource.clip = openedSoundEffect;
					aSource.Play ();
				}
			}
			if (cutsceneToPlay != null) { //if there is a cutscene
				GameManager.currGameManager.PlayCutscene (cutsceneToPlay); //play the cutscene
			}
		} else {
			if (GameManager.SoundEnabled && lockedSoundEffect != null) { //sound enabled && sound effect available
				aSource.volume = GameManager.SFXVolume;
				aSource.clip = lockedSoundEffect;
				aSource.Play ();
			}
		}
		characterAttemptingToEnter.DoorInRange = null; //only allow one frame to attempt to enter, they will need to exit collision and reenter
	}

	void FixedUpdate() {
		for (int i = charactersToMove.Count - 1; i >= 0; i--) { //go through each character in list
			charactersToMove[i].CurrentTimeDelay -= Time.fixedDeltaTime; //update delay
			if (charactersToMove[i].CurrentTimeDelay < 0) { //if delay is complete
				charactersToMove [i].GameObject.transform.position = locationToMoveTo.position; //update position
				charactersToMove [i].GameObject.SetActive (true); //show character
				charactersToMove.RemoveAt (i); //remove from list
			}
		}
	}

	private void MoveGameObject(GameObject go) {
		charactersToMove.Add (new GameObjectToMove(go, moveTimeDelay));
		go.SetActive (false);
	}

	void OnTriggerEnter2D (Collider2D other) {
		Character c; //pointer
		if (other.gameObject == Character.player.gameObject) { //if the other object is the player
			c = Character.player; //set value of pointer
			if (!TutorialDisplayed) { //if we havent displayed tutorial for doors (how to use them)
				//display tutorial
				//stop time
				TutorialDisplayed = true;
				PlayerPrefs.SetInt ("Door Tutorial", 1);
			}
		} else { //other object is a random character
			c = other.gameObject.GetComponent<Character> (); //set value of pointer
		}

		if (c != null)
			c.DoorInRange = this; //set value of door pointer
	}

	void OnTriggerExit2D (Collider2D other) {
		Character c = other.gameObject.GetComponent<Character> (); //set pointer

		if (c != null) //ensure object is a character
			c.DoorInRange = null; //remove pointer to this door
	}

	void Start() {
		aSource = GetComponent<AudioSource> ();
		charactersToMove = new List<GameObjectToMove> ();
	}

	private class GameObjectToMove {
		private GameObject gameObject;
		private float currentTimeDelay;

		public GameObject GameObject { get { return gameObject; } set { gameObject = value; } }
		public float CurrentTimeDelay { get { return currentTimeDelay; } set { currentTimeDelay = value; } }

		public GameObjectToMove(GameObject GameObj, float CurrTimeDelay) {
			gameObject = GameObj;
			currentTimeDelay = CurrTimeDelay;
		}
	}
}
