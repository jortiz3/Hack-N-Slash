using UnityEngine;

public class ChargeEnemy : SimpleEnemy {
	[SerializeField]
	private float chargeDistance;

	protected override void UpdateEnemy () {
		if (!isAttacking)
			base.UpdateEnemy ();
	}

	protected override void Move () {
		if (player != null && targetLocation != player.transform.position) {
			base.Move ();
		} else {
			Vector3 direction = targetLocation - transform.position;

			if (direction.x > chargeDistance) {//target is to the right
				Run (1);
			} else if (direction.x < -chargeDistance) {//target is to the left
				Run (-1);
			} else {//target is close enough
				Attack("Attack_Forward");
				PauseMovement();
			}
		}
	}
}
