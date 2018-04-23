using System;

[Serializable]
public class PlayerData { //all of the player data that will be stored in a file
	public string fileVersion;

	public string selectedCharacter;

	public int currency;
	public int highestSurvivalWave;
	public int survivalStreak;
}