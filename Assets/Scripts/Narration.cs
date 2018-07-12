//Written by Justin Ortiz

using System;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class Narration {
	[SerializeField]
	private string text; //text for the player to read
	[SerializeField]
	private float displayTime; //how long the text will be diplayed
	[SerializeField]
	private AudioClip narration;
	[SerializeField]
	private AudioClip soundEffect;

	public string Text { get { return text; } }
	public float DisplayTime { get { return displayTime; } }
	public AudioClip NarrationAudioClip { get { return narration; } }
	public AudioClip SoundEffect { get { return soundEffect; } }

	public Narration() {
		text = "";
		displayTime = 0f;
	}
}
