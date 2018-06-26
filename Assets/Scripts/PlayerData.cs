using System;

[Serializable]
public class PlayerData { //all of the player data that will be stored in a file
	public string fileVersion;

	public int currency;
	public int highestSurvivalWave;
	public int survivalStreak;

	public string[] unlocks; //stores what the player has unlocked -- not ordered/sorted
	public string[] challenges; //stores completed challenges -- not ordered/sorted
	public string[] missions; //stores completed mission data -- 5 stars? 98%? bronze, silver, gold? only has a row if completed
	public string[] extra00;
	public string[] extra01;
	public string[] extra02;
}