using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//move spawn locations to each Spawn struct

public class CharacterSpawner : MonoBehaviour {
	
	[SerializeField]
	private Transform[] spawnLocations;
	[SerializeField]
	private bool sequentialSpawnLocations = true;
	private int currLocation;

	[System.Serializable]
	private struct Spawn {
		public Character character;
		public int quantity;
	}

	[SerializeField]
	private Spawn[] spawnList;
	[SerializeField]
	private bool sequentialSpawns = true;
	private int currSpawn;

	[SerializeField]
	private bool randomizeAll = true;

	[SerializeField]
	private Vector2 spawnTimeRange;
	private float spawnTimer;

	private int remainingSpawns;

	public bool Depleted { get { return remainingSpawns <= 0 ? true : false; } }

	// Use this for initialization
	void Start () {
		currLocation = 0;
		currSpawn = 0;

		remainingSpawns = 0;
		for (int i = 0; i < spawnList.Length; i++)
			remainingSpawns += spawnList [i].quantity;
	}

	void Update () {
		if (GameManager_SwordSwipe.currGameState == GameState.Active) {
			if (!Depleted) {
				if (spawnTimer > 0) {
					spawnTimer -= Time.deltaTime;
				} else {
					InstantiateSpawn ();

					if (!randomizeAll)
						spawnTimer = Random.Range (spawnTimeRange.x, spawnTimeRange.y);
					else
						spawnTimer = Random.Range (1f, 8f);
				}
			}
		}
	}

	private void InstantiateSpawn() {
		if (!sequentialSpawnLocations || randomizeAll)
			currLocation = Random.Range (0, spawnLocations.Length - 1);
		
		if (!sequentialSpawns || randomizeAll) {
			do {
				currSpawn = Random.Range (0, spawnList.Length - 1);
			} while (spawnList [currSpawn].quantity <= 0);
		}

		GameObject.Instantiate (spawnList [currSpawn].character.gameObject, spawnLocations [currLocation].position, Quaternion.Euler(Vector3.zero));

		remainingSpawns--;
		spawnList [currSpawn].quantity--;

		if (sequentialSpawnLocations) {
			currLocation++;

			if (currLocation >= spawnLocations.Length)
				currLocation = 0;
		}

		if (sequentialSpawns) {
			if (spawnList [currSpawn].quantity <= 0)
				currSpawn++;
		}
	}

	private Vector3 GetSpawnLocation() {
		if (spawnLocations.Length > 0) {
			return spawnLocations[Random.Range(0,spawnLocations.Length - 1)].position;
		} else {
			return Vector3.zero;
		}
	}
}
