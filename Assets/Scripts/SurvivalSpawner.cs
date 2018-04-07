using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivalSpawner : MonoBehaviour {

	private int currentWave;

	[System.Serializable]
	private struct Spawn {
		public Character character;
		[TooltipAttribute("What waves will this character begin to and stop appearing on?")]
		public Vector2 waveAppearance;
		public int quantity; //how many to spawn per wave
		[HideInInspector]
		public int currqty; //how many left to spawn on this wave
		public Transform[] spawnLocations;
	}

	[SerializeField]
	private Spawn[] spawnList;
	private int currSpawn;
	private int currSpawnLoc;

	[SerializeField, Tooltip("Range of time between spawns (Random).")]
	private Vector2 spawnTimeRange;
	private float spawnTimer;

	private int remainingSpawns;

	public bool Depleted { get { return remainingSpawns <= 0 ? true : false; } }

	public int CurrentWave { get { return currentWave; } }

	void Start () {
		if (GameManager.currSurvivalSpawner == null)
			GameManager.currSurvivalSpawner = this;
		else
			Destroy (gameObject);
	}

	void Update () {
		if (GameManager.currGameState == GameState.Active) {
			if (!Depleted) {
				if (spawnTimer > 0) {
					spawnTimer -= Time.deltaTime;
				} else {
					InstantiateSpawn ();

					spawnTimer = Random.Range (spawnTimeRange.x, spawnTimeRange.y);
				}
			} else if (Character.numOfEnemies  < 1){
				GameManager.currGameManager.ShowSurvivalRest ("survived");
			}
		}
	}

	private void InstantiateSpawn() {
		do {
				currSpawn = Random.Range (0, spawnList.Length - 1);
		} while (spawnList [currSpawn].currqty <= 0);

		currSpawnLoc = Random.Range(0, 10000) % spawnList[currSpawn].spawnLocations.Length;

		GameObject.Instantiate (spawnList [currSpawn].character.gameObject, spawnList [currSpawn].spawnLocations[currSpawnLoc].position, Quaternion.Euler(Vector3.zero));

		remainingSpawns--;
		spawnList [currSpawn].currqty--;
	}

	public void StartWave(int waveNumber) {
		currentWave = waveNumber;
		spawnTimer = 3f; //gives the player some time to get adjusted
		remainingSpawns = 0;
		Character.player.NumberOfRespawnsRemaining = 0;

		for (int i = 0; i < spawnList.Length; i++) {//for all of our spawns
			if (spawnList [i].waveAppearance.x <= currentWave && currentWave <= spawnList [i].waveAppearance.y) { //if this should spawn on this wave
				remainingSpawns += spawnList [i].quantity;//add this quantity to our total remaining spawns
				spawnList[i].currqty = spawnList[i].quantity;//ensure the currqty matches how many should appear this wave
			}
		}
		if (remainingSpawns == 0) { //if there are no spawns this wave
			//end of survival; AKA won the game
		}
	}
}
