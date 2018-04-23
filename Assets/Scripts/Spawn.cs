using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Spawn {
	public Character character;
	[TooltipAttribute("Animation to play prior to spawning the character. (can be left null)")]
	public Animator animator;
	public int quantity; //how many to spawn per wave
	[HideInInspector]
	public int currqty; //how many left to spawn on this wave
	public Transform[] spawnLocations;

	public IEnumerator Instantiate() {
		if (spawnLocations.Length < 1 || currqty < 1) { //if we don't have any spawn locations or enough spawns
			yield break; //exit
		}

		int tempLoc = Random.Range (0, spawnLocations.Length);//get current spawn location

		if (animator != null) {
			GameObject tempObj = GameObject.Instantiate (animator.gameObject, spawnLocations [tempLoc].position, Quaternion.Euler (Vector3.zero)); //instantiate the spawn animation
			yield return new WaitForSeconds (animator.runtimeAnimatorController.animationClips[0].length); //wait for the animation to play
			GameObject.Destroy(tempObj); //destroy spawn animation
		}

		GameObject.Instantiate (character.gameObject, spawnLocations[tempLoc].position, Quaternion.Euler(Vector3.zero)); //instantiate the character
		currqty--; //update quantity
	}
}
