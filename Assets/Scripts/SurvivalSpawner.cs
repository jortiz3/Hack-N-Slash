using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivalSpawner : MonoBehaviour {
	private int currentWave;

	[SerializeField]
	private Wave[] waveList;

	private int currSpawn;
	private int currSpawnLoc;

	private float spawnTimer;

	private int remainingSpawns;

	public bool Depleted { get { return remainingSpawns <= 0 ? true : false; } }

	public int CurrentWave { get { return currentWave; } }
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
		if (GameManager.currGameState == GameState.Active) {
			if (!Depleted) {
				if (spawnTimer > 0) {
					spawnTimer -= Time.deltaTime;
				} else {
					InstantiateSpawn ();

					spawnTimer = Random.Range (waveList[currentWave].spawnTimeRange.x, waveList[currentWave].spawnTimeRange.y);
				}
			} else if (Character.numOfEnemies  < 1) {
				GameManager.currGameManager.ShowSurvivalRest ("survived");
			}
		}
	}

	private void InstantiateSpawn() {
		do {
				currSpawn = Random.Range (0, waveList[currentWave].spawnList.Length);
		} while (waveList[currentWave].spawnList [currSpawn].currqty <= 0);

		if (waveList [currentWave].spawnList [currSpawn].spawnLocations.Length > 0) {
			currSpawnLoc = Random.Range (0, 10000) % waveList [currentWave].spawnList [currSpawn].spawnLocations.Length;
		}

		GameObject.Instantiate (waveList[currentWave].spawnList [currSpawn].character.gameObject, waveList[currentWave].spawnList [currSpawn].spawnLocations[currSpawnLoc].position, Quaternion.Euler(Vector3.zero));

		remainingSpawns--;
		waveList[currentWave].spawnList [currSpawn].currqty--;
	}

	public void StartWave(int waveNumber) {
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
		if (waveNumber < waveList.Length) {
			if (waveList [waveNumber].waveWarningText != null || waveList [waveNumber].waveWarningText.Length > 0)
				return waveList [waveNumber].waveWarningText;
		}
		return "No warnings for this wave. Good luck!";
	}

	[System.Serializable]
	private struct Spawn {
		public Character character;
		public int quantity; //how many to spawn per wave
		[HideInInspector]
		public int currqty; //how many left to spawn on this wave
		public Transform[] spawnLocations;
	}

	[System.Serializable]
	private struct Wave {
		public string waveWarningText;
		[SerializeField, Tooltip("Range of time between spawns (Random).")]
		public Vector2 spawnTimeRange;
		public Spawn[] spawnList;
	}
}
