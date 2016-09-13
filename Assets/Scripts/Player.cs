using UnityEngine;
using System.Collections;

public class Player : Character {
	private int level;
	private int exp;
	private int reqExp;

	private int money;

	protected override void InitializeCharacter () {
		base.InitializeCharacter ();
	}

	protected override void UpdateCharacter () {
		if (Input.GetAxis ("Horizontal") != 0) {
			Move (Vector2.right * Input.GetAxis ("Horizontal"));
		}
		if (Input.GetAxis ("Vertical") > 0) {
			if (onGround) {
				Move (Vector2.up * 99999999f);
				onGround = false;
			}
		}
		base.UpdateCharacter ();
	}
}
