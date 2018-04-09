using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameMode { Survival, Story };
public enum GameDifficulty { Easiest, Easy, Normal, Masochist };
public enum GameState { Menu, Cutscene, Active, Rest, Paused };

//To do:
//-continue survival game mode
//	--recognize the end of survival mode
//	--Add custom enemy
//	--Code Boss
//	--background transitions
//-bug: enemies can get suck on their side

public class GameManager : MonoBehaviour {

	public static GameMode currGameMode;
	public static GameDifficulty currDifficulty;
	public static GameState currGameState;
	private static GameState prevGameState; //for return buttons

	private bool difficultyChanged;

	public static GameManager currGameManager;

	public static SurvivalSpawner currSurvivalSpawner;

	private static MenuScript menu;

	private static string selectedCharacter;

	private Text displayedSurvivalWaveNumber;
	private Text displayedSurvivalWaveInfo;
	private Text displayedSurvivalWaveWarning;

	private int selectedSurvivalWave;
	private int highestSurvivalWave;
	private int currSurvivalStreak;

	private int currency;

	void Start () {
		if (currGameManager == null) {
			currGameManager = this;
			DontDestroyOnLoad (gameObject);

			menu = transform.GetChild (0).GetComponent<MenuScript> ();

			//load player save data
			selectedCharacter = "Default Player";
			highestSurvivalWave = 0;
			currency = 0;

			selectedSurvivalWave = 1;

			currSurvivalStreak = 0;

			displayedSurvivalWaveNumber = GameObject.Find ("Selected Wave Number").GetComponent<Text> ();
			displayedSurvivalWaveInfo = GameObject.Find ("Survival Description Text").GetComponent<Text> ();
			displayedSurvivalWaveWarning = GameObject.Find ("Wave Warnings Text").GetComponent<Text> ();

			currGameState = GameState.Menu;
		} else {
			Destroy (gameObject);
		}
	}

	public void UpdateGameDifficulty(int difficulty) {
		currDifficulty = (GameDifficulty)difficulty;
		difficultyChanged = true;
	}

	private void ClearAllCharacters() {
		Transform temp = GameObject.FindGameObjectWithTag ("Character Parent").transform;
		foreach (Transform child in temp) {
			child.GetComponent<Character>().Die();
		}
	}

	private void SpawnPlayer() {
		Instantiate(Resources.Load("Characters/" + selectedCharacter));
	}

	private void SpawnSurvivalSpawner() {
		Instantiate(Resources.Load("Spawners/SurvivalSpawner"));
	}

	public void StartSurvival() {
		if (SceneManager.GetActiveScene ().name != "Main")
			SceneManager.LoadScene ("Main");
		ClearAllCharacters();

		//change displayed info on survival panel
		SpawnSurvivalSpawner();

		difficultyChanged = false;
		currGameMode = GameMode.Survival;
		UpdateSurvivalDisplayText ();
		menu.ChangeState ("Survival");
	}

	public void IncrementSelectedSurvivalWave() {
		if (selectedSurvivalWave > highestSurvivalWave) { //if the selected wave is higher than possible
			selectedSurvivalWave = highestSurvivalWave + 1; //set it to the next available wave
		} else if (selectedSurvivalWave < currSurvivalSpawner.NumberOfWaves) { //prevent from incrementing past the amount of waves in survival spawner
			selectedSurvivalWave++;
		}

		UpdateSurvivalDisplayText ();
	}

	public void DecrementSelectedSurvivalWave() {
		if (selectedSurvivalWave > 1) {
			selectedSurvivalWave--;
		} else {
			selectedSurvivalWave = 1;
		}

		UpdateSurvivalDisplayText ();
	}

	public void StartSurvivalWave() {
		ClearAllCharacters (); //clear all remaining enemies
		SpawnPlayer();

		currSurvivalSpawner.StartWave (selectedSurvivalWave);

		currGameState = GameState.Active;
		menu.ChangeState ("");
		Time.timeScale = 1f;
	}

	public void ShowSurvivalRest(string waveInfo) {
		if (waveInfo.Equals ("survived")) {
			int currencyEarned = 0;
			if (highestSurvivalWave == currSurvivalSpawner.CurrentWave) { //player completed the next available wave
				currencyEarned++;

				if (currSurvivalSpawner.CurrentWave % 25 == 0) //first clear of boss wave
					currencyEarned += 25;
				if (currSurvivalSpawner.CurrentWave % 100 == 0) //first clear of megaboss wave
					currencyEarned += 75;

				if (!difficultyChanged) {
					switch (currDifficulty) {
					case GameDifficulty.Normal:
						currencyEarned += 2;
						break;
					case GameDifficulty.Masochist:
						currencyEarned += 5;
						break;
					}
				}

				highestSurvivalWave = currSurvivalSpawner.CurrentWave + 1;

				if (selectedSurvivalWave < currSurvivalSpawner.NumberOfWaves)
					selectedSurvivalWave++;
			}

			if (currSurvivalSpawner.CurrentWave < 25)
				currencyEarned++;
			else if (currSurvivalSpawner.CurrentWave < 50)
				currencyEarned += 2;
			else if (currSurvivalSpawner.CurrentWave < 75)
				currencyEarned += 3;
			else if (currSurvivalSpawner.CurrentWave < 100)
				currencyEarned += 4;
			else
				currencyEarned += 5;

			currSurvivalStreak++;

			if (currSurvivalStreak % 5 == 0)
				currencyEarned += 5;
			if (currSurvivalStreak % 20 == 0)
				currencyEarned += 20;
			if (currSurvivalStreak % 100 == 0)
				currencyEarned += 300;

			currency += currencyEarned;
			UpdateSurvivalDisplayText ();
			displayedSurvivalWaveInfo.text = "Wave " + currSurvivalSpawner.CurrentWave + " complete!\n\n+" + currencyEarned + " currency! You now have: " + currency + "\n+1 survival streak (" + currSurvivalStreak + ")";
		} else if (waveInfo.Equals ("died")) {
			displayedSurvivalWaveInfo.text = "Wave " + currSurvivalSpawner.CurrentWave + " lost!\n\n-No currency gained\n-Win streak reset to 0";
			currSurvivalStreak = 0;
		}

		menu.ChangeState ("Survival");
		currGameState = GameState.Rest;
		Time.timeScale = 0f;
	}

	private void UpdateSurvivalDisplayText() {
		displayedSurvivalWaveNumber.text = selectedSurvivalWave.ToString ();
		displayedSurvivalWaveWarning.text = currSurvivalSpawner.GetWaveWarning (selectedSurvivalWave); //update the wave warning text
	}

	public void StartStory() {
		//load story scene?
		currGameMode = GameMode.Story;
		currGameState = GameState.Active;

		Time.timeScale = 1f;
	}

	public void SetDifficulty(Dropdown difficulty) {
		currDifficulty = (GameDifficulty)difficulty.value;
	}

	public void PauseGame() {
		prevGameState = currGameState;
		currGameState = GameState.Paused;
		Time.timeScale = 0f;
	}

	public void UnPauseGame() {
		currGameState = prevGameState;
		Time.timeScale = 1f;
	}

	public void ReturnToMainMenu() {
		currGameState = GameState.Menu;
		menu.ChangeState ("Main");
	}

	public void ExitToDesktop () {
		Application.Quit ();
	}

	public void ToggleSoundEnabled(Toggle UIToggle) {
		//get the toggle value
		//enable/disable the sound
	}
}