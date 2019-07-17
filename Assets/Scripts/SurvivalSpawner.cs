//Written by Justin Ortiz

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
	private int numWavesCompleted;
	private bool continuousWavesEnabled;

	public bool Depleted { get { return remainingSpawns <= 0 ? true : false; } }
	public bool ContinuousWavesEnabled { get { return continuousWavesEnabled; } set { continuousWavesEnabled = value; } }
	public int CurrentWave { get { return currentWave + 1; } }
	public int PreviousWave { get { return previousWave + 1; } }
	public int NumberOfWaves { get { return waveList.Length; } }
	public int NumberOfWavesCompleted { get { return numWavesCompleted; } }

	void Awake () {
		if (GameManager_SwordSwipe.currSurvivalSpawner == null) {
			GameManager_SwordSwipe.currSurvivalSpawner = this;
			transform.SetParent (GameObject.Find("Spawners").transform);
		} else {
			Destroy (gameObject);
		}
	}

	public void Initialize() {
		numWavesCompleted = 0;
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

		int waveToSpawn = currentWave;

		if (currentWave >= waveList.Length) { //if the current wave is higher than what is implemented
			waveToSpawn = waveList.Length - 1; //spawn the last implemented wave once again
		}

		for (int i = 0; i < waveList[waveToSpawn].spawnList.Length; i++) {//for all of our spawns
			remainingSpawns += waveList[waveToSpawn].spawnList [i].quantity;//add this quantity to our total remaining spawns
			waveList[waveToSpawn].spawnList[i].currqty = waveList[waveToSpawn].spawnList[i].quantity;//ensure the currqty matches how many should appear this wave
		}
	}

	void Update () {
		if (GameManager_SwordSwipe.currGameState == GameState.Active) { //only update while user is playing
			if (!Depleted) { //while there are characters to spawn
				if (spawnTimer > 0) { //if currently between spawns
					spawnTimer -= Time.deltaTime; //update spawn timer
				} else { //else we need to spawn something
					InstantiateSpawn ();

					spawnTimer = Random.Range (waveList[currentWave].spawnTimeRange.x, waveList[currentWave].spawnTimeRange.y); //randomize the spawn timer
				}
			} else if (Character.numOfEnemies < 1) { //if the user has slain all the enemies
				if (continuousWavesEnabled) {
					StartWave(currentWave + 2); //+2 because the number is decremented in startwave()
					numWavesCompleted++;
				} else {
					GameManager_SwordSwipe.currGameManager.EndSurvivalWave("survived");
				}
			}
		}
	}

	[System.Serializable]
	private struct Wave {
		[SerializeField, Tooltip("Range of time between spawns (Random).")]
		public Vector2 spawnTimeRange;
		public Spawn[] spawnList;
	}
}
