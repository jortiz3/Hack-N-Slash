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

	[SerializeField, Tooltip("If true, will complete the mission once the cutscene is complete.")]
	private bool endMissionOnFinish = false;
	[SerializeField, Tooltip("If left null, will not spawn anything.")]
	private Boss bossToSpawn;
	[SerializeField, Tooltip("Location to spawn the boss. If left null, will spawn on cutscene object location.")]
	private Transform bossSpawnLocation;
	[SerializeField, Tooltip ("Can be used to lock the player into a specific location/area (Set parent gameobject active once they are inside).")]
	private GameObject gameObjectToEnableOnCutsceneEnd;

	[SerializeField]
	private Scene[] scenes; // all of the data for the cutscene stored in this script -- edited in the unity inspector

	private int currScene; //current scene to display
	private int currNarration; //current narration
	private float currDisplayTime; //how much time is left for current narration
	private bool ended;

	private void ClearNarrationText() {
		subtitleText.text = "";
	}

	private void ClearSprite() {
		image.sprite = null;
	}

	public void EndCutscene() {
		if (ended)
			return;

		if (gameObjectToEnableOnCutsceneEnd != null) { //if there is a gameobject we need to show
			gameObjectToEnableOnCutsceneEnd.SetActive (true); //show the gameobject
		}

		if (bossToSpawn != null) { //if there is a boss to spawn
			Vector3 SpawnLoc = transform.position; //set initial spawn location to this object position
			if (bossSpawnLocation != null) { //if there is a different position to spawn the boss
				SpawnLoc = bossSpawnLocation.position; //change location
			}
			Instantiate (bossToSpawn, SpawnLoc, Quaternion.Euler (Vector3.zero)); //spawn the boss
		}

		StopAllAudio(); //may not be necessary
		GameManager_SwordSwipe.currGameManager.StopCutscene (this, endMissionOnFinish); //inform game manager cutscene is complete
		ended = true; //ensure we don't spawn multiple bosses
	}

	void FixedUpdate () {
		if (GameManager_SwordSwipe.currGameState == GameState.Cutscene) { //cutscene to show, not complete
			if (currDisplayTime > 0) { //narration is being displayed
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
			SetSprite();
			NextNarration();
		} else { //no more scenes
			EndCutscene();
		}
	}

	private void NextNarration() {
		currNarration++; //increment current narration

		if (currNarration < scenes [currScene].SceneNarration.Length) { //if there is another narration for the current scene
			SetNarrationText();
			PlayAudio (narrationAudioSource, scenes [currScene].SceneNarration [currNarration].NarrationAudioClip, GameManager_SwordSwipe.BGMVolume);
			PlayAudio (soundEffectAudioSource, scenes [currScene].SceneNarration [currNarration].SoundEffect, GameManager_SwordSwipe.SFXVolume);
		} else {
			NextScene (); //go to the next scene
		}
	}

	private void PlayAudio (AudioSource aSource, AudioClip aClip, float volume) {
		if (GameManager_SwordSwipe.SoundEnabled) {
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
			narrationAudioSource = GameManager_SwordSwipe.cutsceneParent.Find("Narration").GetComponent<AudioSource>(); //get the narration audio source
			soundEffectAudioSource = GameManager_SwordSwipe.cutsceneParent.Find("Sound Effects").GetComponent<AudioSource>(); //get the narration audio source
			image = GameManager_SwordSwipe.cutsceneParent.Find("Image").GetComponent<Image> (); //get the image component so we can change the sprite later on
			subtitleText = GameManager_SwordSwipe.cutsceneParent.Find ("Subtitle").GetComponent<Text>(); //get the text child so we can change narration text later on
		}
		
		ended = false;

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
