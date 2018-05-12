using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//keep chasing target a little longer depending on difficulty

public class AdvancedEnemy : SimpleEnemy {//Capable of a variety of things depending on game difficulty

	[Tooltip("Locations this enemy would go or be attracted to."), SerializeField]
	protected Transform[] pointsOfInterest;
	[SerializeField]
	protected Spawn[] spawns;
	protected List<Character> minions;
	[SerializeField]
	protected MovementType movementType = MovementType.Run;
	protected Behaviour behaviour = Behaviour.Aggressive;

	public enum MovementType { Run, FlySwoop, FlyBomber, Jumper }
	public enum Behaviour { Aggressive, StandGround, Evasive }

	public override void Die () {
		if (minions != null) {
			foreach (Character c in minions) {
				c.Die ();
			}
		}
		base.Die ();
	}

	protected override Vector3 IdentifyTargetLocation () {
		if (GameManager.currGameMode == GameMode.Survival) {
			if (player != null) {
				return player.transform.position;
			}
		} else {
			if (targetCharacter != null) {
				return targetCharacter.transform.position;
			}

			if (pointsOfInterest != null && pointsOfInterest.Length > 0) {
				return pointsOfInterest[Random.Range(0, pointsOfInterest.Length)].position;
			}
		}

		return base.IdentifyTargetLocation ();
	}

	protected override void Move () {
		base.Move ();
	}

	protected override void UpdateEnemy () {
		//if target is acquired
		//	if target is too far away
		//		if too long
		//			get new target -- point of interest || player if survival game mode
		//		end if too long
		//	end if target too far away
		//
		//	move towards target
		//	if close enough to target
		//		attack target
		//	end if close enough
		//else if target not acquired
		//	roam around
		//end target not acquired
		base.UpdateEnemy();
	}
}
