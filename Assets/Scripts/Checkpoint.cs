//Written by Justin Ortiz

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))] //ensure there is always audio source on gameobject
public class Checkpoint : MonoBehaviour {

	private static float previousCheckpointTime;

	private AudioSource aSource;
	[SerializeField]
	private Cutscene cutsceneToTrigger;

	void OnTriggerEnter2D (Collider2D other) {
		if (Time.time - previousCheckpointTime > 5f || GameManager_SwordSwipe.currPlayerSpawnLocation == Vector3.zero) { //if it has been a reasonable amount of time since the last checkpoint was used, we can use this checkpoint again
			GameManager_SwordSwipe.currPlayerSpawnLocation = transform.position; //set this checkpoint as spawn location
			GameManager_SwordSwipe.instance.CheckpointReached();

			if (GameManager_SwordSwipe.SoundEnabled && aSource.clip != null) { //if there is a sound effect & sound is enabled
				aSource.volume = GameManager_SwordSwipe.SFXVolume; //set the volume
				aSource.Play (); //play sound effect
			}

			if (cutsceneToTrigger != null) { //if there is a cutscene
				GameManager_SwordSwipe.instance.PlayCutscene (cutsceneToTrigger); //play cutscene
			}
		}
	}

	void Start() {
		aSource = GetComponent<AudioSource> (); //get the audio source
		previousCheckpointTime = Time.time;
	}
}
