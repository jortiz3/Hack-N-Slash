using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloudScript : MonoBehaviour {

	private Vector3 speed;
	private float delay;

	private Image image;

	void Awake () {
		image = gameObject.GetComponent<Image> ();
		image.rectTransform.sizeDelta = new Vector2 (Screen.width / 5f, Screen.height / 5f);
		image.enabled = false;
	}

	void Start () {
		Prepare ();
	}

	void FixedUpdate () {
		if (GameManager.currGameState == GameState.Active) {
			if (image.enabled) {
				transform.position += speed;

				if (transform.position.x < Camera.main.transform.position.x - 11 || transform.position.x > Camera.main.transform.position.x + 11) {
					image.enabled = false;
					Prepare ();
				}
			} else if (delay > 0) {
				delay -= Time.deltaTime;
			} else {
				image.enabled = true;
			}
		}
	}

	private void Prepare() {
		if (Random.Range (0f, 100f) < 50f) {
			image.rectTransform.anchoredPosition = new Vector2 (0 - image.rectTransform.rect.width, Random.Range (20f, 150f));
			speed = new Vector3(Random.Range (0.001f, 0.005f), 0);
		} else {
			image.rectTransform.anchoredPosition = new Vector2(Screen.width, Random.Range(20f, 150f));
			speed = new Vector3(Random.Range (-0.005f, -0.001f), 0);
		}

		delay = Random.Range (1f, 60f);

	}
}
