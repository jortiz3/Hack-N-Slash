//Written by Justin Ortiz

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))] //ensure there is always audio source on gameobject
public class Checkpoint : MonoBehaviour {

	private AudioSource aSource;
	[SerializeField]
	private Cutscene cutsceneToTrigger;

	void OnTriggerEnter2D (Collider2D other) {
		if (!GameManager.currPlayerSpawnLocation.Equals(transform.position)) { //if the current spawn location is not this checkpoint
			GameManager.currPlayerSpawnLocation = transform.position; //set this checkpoint as spawn location

			if (GameManager.SoundEnabled && aSource.clip != null) { //if there is a sound effect & sound is enabled
				aSource.volume = GameManager.SFXVolume; //set the volume
				aSource.Play (); //play sound effect
			}

			if (cutsceneToTrigger != null) { //if there is a cutscene
				GameManager.currGameManager.PlayCutscene (cutsceneToTrigger); //play cutscene
			}
		}
	}

	void Start() {
		aSource = GetComponent<AudioSource> (); //get the audio source
	}
}
