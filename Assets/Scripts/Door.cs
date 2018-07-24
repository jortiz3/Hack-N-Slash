using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {

	public static bool TutorialDisplayed;

	//private Key keyToUnlockDoor;
	[SerializeField]
	private Cutscene cutsceneToPlay;
	[SerializeField]
	private Transform locationToMoveTo;
	[SerializeField]
	private float timeDelay;

	private List<GameObjectToMove> charactersToMove;

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

	void OnTriggerEnter2D (Collider2D other) {
		if (other.gameObject == Character.player.gameObject) {
			if (!TutorialDisplayed) {
				TutorialDisplayed = true;
				PlayerPrefs.SetInt ("Door Tutorial", 1);

				//display tutorial
				//stop time
			}
		}

		//if the character has the key or there is no key {
		if (locationToMoveTo != null) {
			charactersToMove.Add (new GameObjectToMove(other.gameObject, timeDelay));
			other.gameObject.SetActive (false);
		} else if (cutsceneToPlay != null){
			GameManager.currGameManager.PlayCutscene (cutsceneToPlay);
		}
		//}

	}

	void Start() {
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
