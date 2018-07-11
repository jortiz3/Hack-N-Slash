//Justin Ortiz

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

[Serializable]
public class Scene {
	[SerializeField]
	private Sprite sprite; //the picture to display while narration is happening
	[SerializeField]
	private Narration[] narration; //what the player is hearing/reading while the picture is displayed

	//properties
	public Sprite SceneSprite { get { return sprite; } }
	public Narration[] SceneNarration { get { return narration; } }

	public Scene() { //default constructor
		sprite = null;
		narration = new Narration[1];
	}
}
