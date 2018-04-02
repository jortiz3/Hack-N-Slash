using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;

public class DataPersistence {
	
	public static void Save() {
		//playerprefs for volumes & difficulty
		//playerdata
	}

	public static void Load() {
		//volumes
		//playerdata
	}
}

[Serializable]
class PlayerData { //all of the player data that will be stored in a file
	string fileVersion;
	bool[] unlockables;
	float[] experience;

	int currency;

	int highestSurvivalWave;
	bool[] storyMissionsCompleted;
}