//Written by Justin Ortiz

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PersistentStatus : MonoBehaviour {

	private CanvasGroup canvasGroup;
	private Slider hpbar;
	private Text text;
	[SerializeField]
	private float transparency = 0.3f;

	public bool FullyDisplayed { get { return canvasGroup.alpha > transparency; } }
	public bool Transparent { get { return canvasGroup.alpha == transparency; } }

	public void Display() {
		canvasGroup.alpha = 1f;
	}

	public ref Slider GetHPBar() {
		return ref hpbar;
	}

	public void Hide() {
		canvasGroup.alpha = 0f;
	}

	public void SetStatusText(string statusText) {
		text.text = statusText;
	}

	/// <summary>
	/// Assigns the default transparency and sets the displayed alpha value.
	/// </summary>
	public void SetTransparency(float alpha) {
		transparency = alpha;
		canvasGroup.alpha = transparency;
	}

	/// <summary>
	/// Sets the displayed alpha to the default transparency value.
	/// </summary>
	public void SetTransparent() {
		canvasGroup.alpha = transparency;
	}

	void Start() {
		canvasGroup = GetComponent<CanvasGroup>(); //get all required components
		hpbar = transform.Find("HP Bar").GetComponent<Slider>();
		text = transform.Find("Text").GetComponent<Text>();

		Hide();
	}
}
