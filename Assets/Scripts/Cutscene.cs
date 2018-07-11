//Written by Justin Ortiz

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cutscene : MonoBehaviour {

	private static GameObject cutsceneObject; //pointer to object within the main scene that will display the image and text
	private static Image image; //pointer to the image on the cutscene object we will change -- attached to this gameobject
	private static Text narrationText; //pointer to the text on the cutscene we will change -- attached to child of this gameobject

	[SerializeField]
	private Scene[] scenes; // all of the data for the cutscene stored in this script -- edited in the unity inspector

	private int currScene; //current scene to display
	private int currNarration; //current narration
	private float currDisplayTime; //how much time is left for current narration
	private float currDelayTime; //delay between scenes to give player time to process what they have seen
	private bool delayComplete;
	private bool cutsceneComplete; //to let us know when the cutscene is done

	public bool isComplete { get { return cutsceneComplete; } }

	private void ClearNarrationText() {
		narrationText.text = "";
	}

	private void ClearSprite() {
		image.sprite = null;
	}

	void FixedUpdate () {
		if (GameManager.currGameState == GameState.Cutscene) { //cutscene to show, not complete
			if (currDelayTime > 0) { //we are in a delay
				currDelayTime -= Time.fixedDeltaTime;
			} else if (!delayComplete) { //we finished a delay, narration text was cleared
				SetSprite (); //show current picture
				SetNarrationText (); //show current narration
				delayComplete = true;
			} else if (currDisplayTime > 0) { //narration is being displayed
				currDisplayTime -= Time.fixedDeltaTime;
			} else {
				NextNarration ();
			}
		}
	}

	private void NextScene() {
		currScene++; //increment current scene

		if (currScene < scenes.Length) { //another scene to show
			currNarration = 0; //reset narration
			ClearNarrationText (); //clear narration text

			currDelayTime = 2f; //set delay so player can process
			delayComplete = false;
		} else { //no more scenes
			cutsceneObject.SetActive(false);
			GameManager.currGameManager.EndCutscene ();
		}
	}

	private void NextNarration() {
		currNarration++; //increment current narration

		if (currNarration < scenes [currScene].SceneNarration.Length) { //if there is another narration for the current scene
			SetNarrationText();
		} else {
			NextScene (); //go to the next scene
		}
	}

	private void SetNarrationText() {
		narrationText.text = scenes [currScene].SceneNarration [currNarration].Text; //set the text
		currDisplayTime = scenes [currScene].SceneNarration [currNarration].DisplayTime; //set displaytime
	}

	private void SetSprite() {
		image.sprite = scenes [currScene].SceneSprite; //set the sprite
	}

	void Start() {
		cutsceneComplete = false;

		if (cutsceneObject == null) {
			cutsceneObject = GameManager.currGameManager.transform.Find("Canvas (Overlay)").Find ("Cutscene").gameObject; //get the cutscene object so we can show/hide it later on
			image = cutsceneObject.transform.Find("Image").GetComponent<Image> (); //get the image component so we can change the sprite later on
			narrationText = cutsceneObject.transform.Find ("Narration").GetComponent<Text>(); //get the text child so we can change narration text later on
		}

		currScene = 0; //set to the first scene
		currNarration = 0; //set to the first narration text

		SetSprite (); //show the first sprite
		SetNarrationText (); //show the first narration text

		cutsceneObject.SetActive (true);
	}
}
