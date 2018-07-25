using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer)), RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(AudioSource))] //ensure there is always a renderer & audio source on gameobject
public class Item : MonoBehaviour {
	
	[SerializeField, Tooltip("The player can only receive the reward for obtaining this item the first time they acquire it.")]
	private bool singleAcquirance;
	private SpriteRenderer sr;
	[SerializeField, Tooltip("Color this object should be if the player has already collected this item.")]
	private Color collectedColor;
	private AudioSource aSource;
	private bool obtained;

	public bool SingleAcquirance { get { return singleAcquirance; } }

	void FixedUpdate() {
		if (GameManager.currGameState == GameState.Active) {
			if (obtained) { //item was obtained
				if (!aSource.isPlaying) { //sound effect finished playing
					gameObject.SetActive (false); //hide object
				}
			}
		}
	}

	void OnTriggerEnter2D (Collider2D other) {
		Character c = other.GetComponent<Character>();
		if (c != null) {
			PickUp (c);
		}
	}

	protected virtual void PickUp(Character c) {
		c.ReceiveItem (this); //give the item to the character
		PlaySoundEffect();
		obtained = true;
		//in future, play pickup animation
	}

	private void PlaySoundEffect() {
		if (GameManager.SoundEnabled && aSource.clip != null) {
			aSource.volume = GameManager.SFXVolume;
			aSource.Play ();
		}
	}

	void Start() {
		aSource = GetComponent<AudioSource> ();
		obtained = false;
	}

	public override string ToString () {
		return GameManager.SelectedCampaignMission + "_" + gameObject.name;
	}
}
