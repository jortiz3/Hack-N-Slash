using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;

public class DataPersistence {
	private static string saveLocation = Application.persistentDataPath + "/meta.dat";

	public static PlayerData Load() {
		GameManager.currGameManager.SetDifficulty (PlayerPrefs.GetInt("Difficulty", 2)); //set the game difficulty using the stored playerpref
		GameManager.SoundEnabled = PlayerPrefs.GetInt ("Sound Enabled") == 1 ? true : false; //set the sound toggle using the stored playerpref

		GameManager.BGMVolume = PlayerPrefs.GetFloat ("BGM Volume", 0.5f); //set the background music volume using the stored playerpref
		GameManager.SFXVolume = PlayerPrefs.GetFloat ("SFX Volume", 0.5f); //set the sound effects volume using the stored playerpref

		GameManager.SelectedOutfit = PlayerPrefs.GetString ("SelectedCharacter", "Stick it to 'em");
		GameManager.SelectedWeapon = PlayerPrefs.GetString ("SelectedWeapon", "Iron Longsword");

		FileStream file;
		if (File.Exists (saveLocation)) {
			file = File.OpenRead (saveLocation);
		} else {
			return null; //no player data to load if there is no file
		}

		BinaryFormatter formatter = new BinaryFormatter ();
		return (PlayerData)formatter.Deserialize (file); //get the playerdata from the file, send the data to the gameManager who called the method
	}
	
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
		playerData.currency = GameManager.currGameManager.Currency;
		playerData.highestSurvivalWave = GameManager.currGameManager.HighestSurvivalWave;
		playerData.survivalStreak = GameManager.currGameManager.CurrentSurvivalStreak;
		playerData.unlocks = GameManager.Unlocks;

		BinaryFormatter formatter = new BinaryFormatter ();
		formatter.Serialize (file, playerData);
		file.Close ();
	}

	public static void SavePlayerPrefs() {
		PlayerPrefs.SetInt("Difficulty", (int)GameManager.currDifficulty); //set difficulty to playerprefs
		PlayerPrefs.SetInt ("Sound Enabled", GameManager.SoundEnabled ? 1 : 0); //set bool as an integer to playerprefs

		PlayerPrefs.SetFloat("BGM Volume", GameManager.BGMVolume); //set background music volume to playerprefs
		PlayerPrefs.SetFloat ("SFX Volume", GameManager.SFXVolume); //set sound effects volume to playerprefs

		PlayerPrefs.SetString ("SelectedCharacter", GameManager.SelectedOutfit); //set the currently selected character
		PlayerPrefs.SetString ("SelectedWeapon", GameManager.SelectedWeapon); //set the currently selected

		PlayerPrefs.Save (); //save the playerpref changes
	}
}