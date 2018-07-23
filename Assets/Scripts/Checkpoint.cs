//Written by Justin Ortiz

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Checkpoint : MonoBehaviour {

	private bool triggered;
	private AudioSource aSource;
	[SerializeField]
	private AudioClip soundEffect;
	[SerializeField]
	private Cutscene cutsceneToTrigger;

	void OnTriggerEnter2D (Collider2D other) {
		if (!triggered) {
			GameManager.currPlayerSpawnLocation = transform.position;

			if (soundEffect != null && GameManager.SoundEnabled) {
				aSource.volume = GameManager.SFXVolume;
				aSource.Play ();
			}

			if (cutsceneToTrigger != null) {
				//play cutscene
			}

			triggered = true;
		}
	}

	void Start() {
		aSource = GetComponent<AudioSource> ();
		aSource.clip = soundEffect;
	}
}
