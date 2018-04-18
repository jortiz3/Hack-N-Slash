using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;

public class DataPersistence {

	private static string saveLocation = Application.persistentDataPath + "/meta.dat";
	
	public static void Save() {
		SavePlayerPrefs ();

		FileStream file;
		if (File.Exists (saveLocation)) { //check to see if the game has been saved before
			file = File.OpenWrite (saveLocation); //open the file if so
		} else {
			file = File.Create (saveLocation); //if not, create the file
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

	public static void SavePlayerPrefs() {
		PlayerPrefs.SetInt("Difficulty", (int)GameManager.currDifficulty); //set difficulty to playerprefs
		PlayerPrefs.SetInt ("Sound Enabled", GameManager.SoundEnabled ? 1 : 0); //set bool as an integer to playerprefs

		PlayerPrefs.SetFloat("BGM Volume", GameManager.BGMVolume); //set background music volume to playerprefs
		PlayerPrefs.SetFloat ("SFX Volume", GameManager.SFXVolume); //set sound effects volume to playerprefs

		PlayerPrefs.Save (); //save the playerpref changes
	}

	public static PlayerData Load() {
		FileStream file;
		if (File.Exists (saveLocation)) {
			file = File.OpenRead (saveLocation);
		} else {
			return null; //nothing to load if there is no file; skip loading playerprefs
		}

		GameManager.currGameManager.SetDifficulty (PlayerPrefs.GetInt("Difficulty")); //set the game difficulty using the stored playerpref
		GameManager.SoundEnabled = PlayerPrefs.GetInt ("Sound Enabled") == 1 ? true : false; //set the sound toggle using the stored playerpref

		GameManager.BGMVolume = PlayerPrefs.GetFloat ("BGM Volume"); //set the background music volume using the stored playerpref
		GameManager.SFXVolume = PlayerPrefs.GetFloat ("SFX Volume"); //set the sound effects volume using the stored playerpref

		BinaryFormatter formatter = new BinaryFormatter ();
		return (PlayerData)formatter.Deserialize (file); //get the playerdata from the file, send the data to the gameManager who called the method
	}
}

[Serializable]
public class PlayerData { //all of the player data that will be stored in a file
	public string fileVersion;

	public string selectedCharacter;

	public int currency;
	public int highestSurvivalWave;
	public int survivalStreak;
}