//Written by Justin Ortiz

using System;
using UnityEngine;

[Serializable]
public class Narration {
	[SerializeField]
	private string text; //text for the player to read
	[SerializeField]
	private float displayTime; //how long the text will be diplayed
	//audio clip?

	public string Text { get { return text; } }
	public float DisplayTime { get { return displayTime; } }

	public Narration() {
		text = "";
		displayTime = 0f;
	}
}
