using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;

public class DataPersistence {

	private static string saveLocation = Application.persistentDataPath + "/meta.dat";
	
	public static void Save() {
		PlayerPrefs.SetInt("Difficulty", (int)GameManager.currDifficulty);
		PlayerPrefs.SetInt ("Sound Enabled", GameManager.SoundEnabled ? 1 : 0);

		PlayerPrefs.SetFloat("BGM Volume", GameManager.BGMVolume);
		PlayerPrefs.SetFloat ("SFX Volume", GameManager.SFXVolume);

		PlayerPrefs.Save ();

		FileStream file;
		if (File.Exists (saveLocation)) {
			file = File.OpenWrite (saveLocation);
		} else {
			file = File.Create (saveLocation);
		}

		PlayerData playerData = new PlayerData ();
		playerData.fileVersion = "v0.1";
		playerData.selectedCharacter = GameManager.SelectedCharacter;
		playerData.currency = GameManager.currGameManager.Currency;
		playerData.highestSurvivalWave = GameManager.currGameManager.HighestSurvivalWave;
		playerData.survivalStreak = GameManager.currGameManager.CurrentSurvivalStreak;

		BinaryFormatter formatter = new BinaryFormatter ();
		formatter.Serialize (file, playerData);
		file.Close ();
	}

	public static PlayerData Load() {
		GameManager.currGameManager.SetDifficulty (PlayerPrefs.GetInt("Difficulty"));
		GameManager.SoundEnabled = PlayerPrefs.GetInt ("Sound Enabled") == 1 ? true : false;

		GameManager.BGMVolume = PlayerPrefs.GetFloat ("BGM Volume");
		GameManager.SFXVolume = PlayerPrefs.GetFloat ("SFX Volume");

		FileStream file;
		if (File.Exists (saveLocation)) {
			file = File.OpenRead (saveLocation);
		} else {
			return null;
		}

		BinaryFormatter formatter = new BinaryFormatter ();
		return (PlayerData)formatter.Deserialize (file);
	}
}

[Serializable]
public class PlayerData { //all of the player data that will be stored in a file
	public string fileVersion; //will be used to verify whether file was tampered with

	public string selectedCharacter;

	public int currency;
	public int highestSurvivalWave;
	public int survivalStreak;
}