//Written by Justin Ortiz

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Boss : AdvancedEnemy {

	public override void Die() {
		GameManager_SwordSwipe.bossStatus.Hide();
		base.Die();
	}

	private void DisplayStatus() {
		GameManager_SwordSwipe.bossStatus.Display();
	}

	private void UpdateStatusText() {
		GameManager_SwordSwipe.bossStatus.SetStatusText(gameObject.name + "      " + CurrentHP + "/" + MaxHP);
	}

	void Start() {
		unwavering = true;
		hpSliderIsPersistent = true;
		hpSliderAlwaysActive = true;
		hpSlider = GameManager_SwordSwipe.bossStatus.GetHPBar();
		base.Initialize();

		UpdateStatusText();
		DisplayStatus();
	}

	protected override void UpdateEnemy() {
		if (SpriteColor.Equals(Color.red)) {
			UpdateStatusText();
		}

		//if above the camera
		//if !status.transparent
		//make status transparent

		base.UpdateEnemy();
	}
}
