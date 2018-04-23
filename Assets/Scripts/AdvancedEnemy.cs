using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedEnemy : SimpleEnemy {//Capable of a variety of things depending on game difficulty

	[Tooltip("Locations this enemy would go or be attracted to.")]
	protected Transform[] pointsOfInterest;
	protected Spawn[] spawns;
	protected List<Character> minions;
	[SerializeField]
	protected MovementType movementType = MovementType.Run;

	public enum MovementType { Run, FlyOnly, JumpOnly, Scripted }
	public enum FlightType { none, sinX, sinY }

	public override void Die () {
		if (minions != null) {
			foreach (Character c in minions) {
				c.Die ();
			}
		}
		base.Die ();
	}

	protected override void Move () {
		base.Move ();
	}

	protected override void UpdateEnemy () {
		//if target is acquired
		//	if target is too far away
		//		if target has been too far away for too long
		//			no longer target
		//	end if target too far away
		//
		//	move towards target
		//	if close enough to target
		//		attack target
		//	end if close enough
		//else if target not acquired
		//	roam around
		//
		//	ray cast in front
		//	ray collide with character.tag != "Enemy" -> new target
		//end target not acquired
	}
}
