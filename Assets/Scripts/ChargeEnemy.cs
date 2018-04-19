using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeEnemy : SimpleEnemy {
	[SerializeField]
	private float chargeDistance;

	protected override void UpdateEnemy () {
		if (!isAttacking || Velocity.magnitude < 1)
			base.UpdateEnemy ();
	}

	protected override void Move () {
		if (player != null && targetLocation != player.transform.position) {
			base.Move ();
		} else {
			Vector3 direction = targetLocation - transform.position;

			if (direction.x > chargeDistance) {//target is to the right
				Move (1);
			} else if (direction.x < -chargeDistance) {//target is to the left
				Move (-1);
			} else {//target is close enough
				Attack("Attack_Forward");
				PauseMovement();
			}
		}
	}
}
