//Written by Justin Ortiz

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cutscene : MonoBehaviour {

	private static Image image; //pointer to the image on the cutscene object we will change -- attached to the cutscene object
	private static AudioSource narrationAudioSource; //pointer to the component that will emit the narration -- attached to child of the cutscene object
	private static AudioSource soundEffectAudioSource; //pointer to the component that will emit the narration -- attached to child of the cutscene object
	private static Text subtitleText; //pointer to the text on the cutscene we will change -- attached to child of the cutscene object

	[SerializeField]
	private Scene[] scenes; // all of the data for the cutscene stored in this script -- edited in the unity inspector

	private int currScene; //current scene to display
	private int currNarration; //current narration
	private float currDisplayTime; //how much time is left for current narration
	private float currDelayTime; //delay between scenes to give player time to process what they have seen
	private bool delayComplete;

	private void ClearNarrationText() {
		subtitleText.text = "";
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
				NextNarration(); //go to first narration -- 0
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
			currNarration = -1; //reset narration to -1
			ClearNarrationText (); //clear narration text

			currDelayTime = 2f; //set delay so player can process
			delayComplete = false;
		} else { //no more scenes
			StopAllAudio(); //may not be necessary
			GameManager.currGameManager.StopCutscene (this); //inform game manager cutscene is complete
		}
	}

	private void NextNarration() {
		currNarration++; //increment current narration

		if (currNarration < scenes [currScene].SceneNarration.Length) { //if there is another narration for the current scene
			SetNarrationText();
			PlayAudio (narrationAudioSource, scenes [currScene].SceneNarration [currNarration].NarrationAudioClip, GameManager.BGMVolume);
			PlayAudio (soundEffectAudioSource, scenes [currScene].SceneNarration [currNarration].SoundEffect, GameManager.SFXVolume);
		} else {
			NextScene (); //go to the next scene
		}
	}

	private void PlayAudio (AudioSource aSource, AudioClip aClip, float volume) {
		if (GameManager.SoundEnabled) {
			if (aClip != null) { //ensure there is a clip
				if (aSource.isPlaying) //is the narrator currently speaking?
					aSource.Stop (); //stop the narrator
				aSource.volume = volume; //ensure the volume is correct
				aSource.clip = aClip; //change it to the correct clip
				aSource.Play (); //play the clip
			}
		}
	}

	private void SetNarrationText() {
		subtitleText.text = scenes [currScene].SceneNarration [currNarration].Text; //set the text
		currDisplayTime = scenes [currScene].SceneNarration [currNarration].DisplayTime; //set displaytime
	}

	private void SetSprite() {
		image.sprite = scenes [currScene].SceneSprite; //set the sprite
	}

	void Start() {
		if (narrationAudioSource == null) {
			narrationAudioSource = GameManager.cutsceneParent.Find("Narration").GetComponent<AudioSource>(); //get the narration audio source
			soundEffectAudioSource = GameManager.cutsceneParent.Find("Sound Effects").GetComponent<AudioSource>(); //get the narration audio source
			image = GameManager.cutsceneParent.Find("Image").GetComponent<Image> (); //get the image component so we can change the sprite later on
			subtitleText = GameManager.cutsceneParent.Find ("Subtitle").GetComponent<Text>(); //get the text child so we can change narration text later on
		}

		delayComplete = true;

		currScene = 0; //set to the first scene
		currNarration = -1; //set position just behind first narration text

		SetSprite (); //show the first sprite
		NextNarration (); //increment to first narration
	}

	private void StopAllAudio() {
		narrationAudioSource.Stop ();
		soundEffectAudioSource.Stop ();
	}
}
