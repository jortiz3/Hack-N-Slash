using System.Collections;
using UnityEngine;

//restart music when advertisement ends

public class AudioManager : MonoBehaviour {
	
	private AudioSource audioSource_BGM;
	private AudioClip nextClip;
	private bool fadeInProgress;

	void Awake() {
		audioSource_BGM = transform.Find("AudioSource_Music").GetComponent<AudioSource>(); //get the component asap
	}

	private IEnumerator FadeBGM(bool fadeIn) {
		while (fadeInProgress) { //if a fade has already been called
			yield return null; //wait here
		}
		fadeInProgress = true; //let other coroutines know a fade has begun

		float fadeVelocity = 0.03f; //the speed at which the audio fades

		if (fadeIn) {
			audioSource_BGM.Stop(); //stop playing previous clip
			audioSource_BGM.volume = 0; //ensure volume is at 0 and not below
			if (nextClip != null) {
				audioSource_BGM.clip = nextClip; //set the new clip
				nextClip = null; //clear the next clip
			}
			yield return new WaitForEndOfFrame();
			audioSource_BGM.Play();
		} else { //fading out
			fadeVelocity *= -1f;
		}

		do {
			audioSource_BGM.volume += fadeVelocity; //decrease volume
			yield return null;
		} while (audioSource_BGM.volume > 0 && audioSource_BGM.volume < GameManager_SwordSwipe.BGMVolume); //loop until volume reaches the desired

		if (fadeIn) {
			audioSource_BGM.volume = GameManager_SwordSwipe.BGMVolume;
		} else { //fading out
			audioSource_BGM.Stop();
			audioSource_BGM.volume = 0; //ensure volume is at 0 and not below
		}
		fadeInProgress = false; //let other coroutines this one has finished
	}

	public static void PlaySoundEffect(AudioClip effect, Vector3 sourcePosition, float volumeModifier) {
		sourcePosition.z = Camera.main.transform.position.z; //ensure sound effect and camera are both on the same plane (2D)
		float distance = Vector3.Distance(Camera.main.transform.position, sourcePosition); //current distance between camera and sound effect

		if (distance < 12) {
			distance = Mathf.Clamp(distance, 1f, float.MaxValue); //keeps the value from being too small
			AudioSource.PlayClipAtPoint(effect, sourcePosition, (GameManager_SwordSwipe.SFXVolume / distance) * volumeModifier); //play sound effect at location at correct volume
		}
	}

	public void SetNextClip (AudioClip clip) {
		nextClip = clip;
	}

	private void Start() {
		audioSource_BGM.volume = GameManager_SwordSwipe.BGMVolume; //ensure the volume is correct at the start
		fadeInProgress = false;
	}

	public void StartBackgroundMusic() {
		StartCoroutine(FadeBGM(true));
	}

	public void StopBackgroundMusic() {
		StartCoroutine(FadeBGM(false));
	}

	public void TransitionBackgroundMusic(AudioClip clip) {
		StartCoroutine(FadeBGM(false));
		SetNextClip(clip);
		StartCoroutine(FadeBGM(true));
	}

	public void UpdateVolume() {
		audioSource_BGM.volume = GameManager_SwordSwipe.BGMVolume;
		audioSource_BGM.enabled = GameManager_SwordSwipe.SoundEnabled;
	}
}
