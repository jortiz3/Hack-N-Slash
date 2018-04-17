using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivalSpawner : MonoBehaviour {
	private int currentWave;
	private int previousWave;

	[SerializeField]
	private Wave[] waveList;

	private int currSpawn;
	private int currSpawnLoc;

	private float spawnTimer;

	private int remainingSpawns;

	public bool Depleted { get { return remainingSpawns <= 0 ? true : false; } }

	public int CurrentWave { get { return currentWave; } }
	public int PreviousWave { get { return previousWave; } }
	public int NumberOfWaves { get { return waveList.Length; } }

	void Awake () {
		if (GameManager.currSurvivalSpawner == null) {
			GameManager.currSurvivalSpawner = this;
			transform.SetParent (GameObject.Find("Spawners").transform);
		} else {
			Destroy (gameObject);
		}
	}

	void Update () {
		if (GameManager.currGameState == GameState.Active) { //only update while user is playing
			if (!Depleted) { //while there are characters to spawn
				if (spawnTimer > 0) { //if currently between spawns
					spawnTimer -= Time.deltaTime; //update spawn timer
				} else { //else we need to spawn something
					InstantiateSpawn ();

					spawnTimer = Random.Range (waveList[currentWave].spawnTimeRange.x, waveList[currentWave].spawnTimeRange.y); //randomize the spawn timer
				}
			} else if (Character.numOfEnemies < 1) { //if the user has slain all the enemies
				GameManager.currGameManager.EndSurvivalWave ("survived");
			}
		}
	}

	private void InstantiateSpawn() {
		do {
				currSpawn = Random.Range (0, waveList[currentWave].spawnList.Length); //pick a random spawn from the current wave
		} while (waveList[currentWave].spawnList [currSpawn].currqty <= 0); //see if it has any left to spawn

		StartCoroutine(waveList [currentWave].spawnList [currSpawn].Instantiate ()); //instantiate it
		remainingSpawns--; //update our quantity
	}

	public void StartWave(int waveNumber) {
		previousWave = currentWave;
		currentWave = waveNumber - 1;
		spawnTimer = 3f; //gives the player some time to get adjusted
		remainingSpawns = 0;
		Character.player.NumberOfRespawnsRemaining = 0;

		for (int i = 0; i < waveList[currentWave].spawnList.Length; i++) {//for all of our spawns
			remainingSpawns += waveList[currentWave].spawnList [i].quantity;//add this quantity to our total remaining spawns
			waveList[currentWave].spawnList[i].currqty = waveList[currentWave].spawnList[i].quantity;//ensure the currqty matches how many should appear this wave
		}
	}

	public string GetWaveWarning (int waveNumber) {
		waveNumber -= 1;
		if (waveNumber < waveList.Length) { //ensure the number is within the bounds of the array
			if (waveList [waveNumber].waveWarningText != null && waveList [waveNumber].waveWarningText.Length > 0) //ensure the text isn't null and has text in it
				return waveList [waveNumber].waveWarningText;
		}
		return "No warnings for this wave. Good luck!";
	}

	[System.Serializable]
	private struct Spawn {
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
				Destroy(tempObj); //destroy spawn animation
			}

			GameObject.Instantiate (character.gameObject, spawnLocations[tempLoc].position, Quaternion.Euler(Vector3.zero)); //instantiate the character
			currqty--; //update quantity
		}
	}

	[System.Serializable]
	private struct Wave {
		public string waveWarningText;
		[SerializeField, Tooltip("Range of time between spawns (Random).")]
		public Vector2 spawnTimeRange;
		public Spawn[] spawnList;
	}
}
