using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextFade : MonoBehaviour {

	private static float fadeTime = 1f;
	private static float displayTime = 2f;

	private bool fadingIn;
	private bool fadingOut;
	private bool displayingText;
	private float currFadeTime;
	private float currDisplayTime;
	[SerializeField]
	private Text title;
	[SerializeField]
	private Text body;
	private Color[] titleColors;
	private Color[] bodyColors;

	private void Awake() {
		titleColors = new Color[2];
		titleColors[0] = title.color; //soon-to-be transparent
		titleColors[1] = title.color; //color set in unity editor

		titleColors[0].a = 0f; //set transparency

		bodyColors = new Color[2];
		bodyColors[0] = body.color;
		bodyColors[1] = body.color;

		bodyColors[0].a = 0f;

		title.color = titleColors[0];
		body.color = bodyColors[0];
	}

	public void Display(string Title, string Body) {
		fadingIn = true;
		fadingOut = false;
		displayingText = false;
		currFadeTime = fadeTime;
		currDisplayTime = 0f;

		title.text = Title;
		title.color = Color.clear;

		body.text = Body;
		body.color = Color.clear;
	}

	void Update () {
		if (fadingIn) {
			currFadeTime -= Time.deltaTime; //update current time

			float t = Mathf.Clamp(currFadeTime / fadeTime, 0f, 1f); //clamp the time between 0 & 1 for function

			title.color = Color.Lerp(titleColors[1], titleColors[0], t); //transition color from clear to default
			body.color = Color.Lerp(bodyColors[1], bodyColors[0], t);

			if (currFadeTime <= 0) {
				fadingIn = false;
				displayingText = true;
				currDisplayTime = displayTime;
			}
		} else if (displayingText) {
			currDisplayTime -= Time.deltaTime;

			if (currDisplayTime <= 0) {
				displayingText = false;
				fadingOut = true;
				currFadeTime = fadeTime;
			}
		} else if (fadingOut) {
			currFadeTime -= Time.deltaTime; //update current time

			float t = Mathf.Clamp(currFadeTime / fadeTime, 0f, 1f); //clamp the time between 0 & 1 for function

			title.color = Color.Lerp(titleColors[0], titleColors[1], t); //transition color from default to clear
			body.color = Color.Lerp(bodyColors[0], bodyColors[1], t);

			if (currFadeTime <= 0) {
				fadingOut = false;
			}
		}
	}
}
