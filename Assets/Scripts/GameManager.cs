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
//		--currency earned; +1 for clearing; +1 for first time clear; +5 bonus for not dying in 5 rounds; +10 bonus for not dying in 10 rounds; +20, etc.
//		--new enemy warning
//		--boss wave warning
//		--state the winstreak is reset
//		--button for character select
//	--background transitions
//		--bosses??
//-make the character health bars bigger/scale
//-remove attack timer slider?
//-bug: enemies can get suck on their side

public class GameManager : MonoBehaviour {

	public static GameMode currGameMode;
	public static GameDifficulty currDifficulty;
	private static GameDifficulty prevDifficulty; //to ensure player doesn't change difficulty to masochist right at the end of level
	public static GameState currGameState;
	private static GameState prevGameState; //for return buttons

	public static GameManager currGameManager;

	public static SurvivalSpawner currSurvivalSpawner;

	private static MenuScript menu;

	private static string selectedCharacter;

	private Text displayedSurvivalWaveNumber;

	private int selectedSurvivalWave;

	private int highestSurvivalWave;

	void Start () {
		if (currGameManager == null) {
			currGameManager = this;
			DontDestroyOnLoad (gameObject);

			menu = transform.GetChild (0).GetComponent<MenuScript> ();

			//load player save data
			selectedCharacter = "Default Player";
			highestSurvivalWave = 0;

			selectedSurvivalWave = 1;

			displayedSurvivalWaveNumber = GameObject.Find ("Selected Wave Number").GetComponent<Text> ();

			currGameState = GameState.Menu;
		} else {
			Destroy (gameObject);
		}
	}

	public void UpdateGameDifficulty(int difficulty) {
		prevDifficulty = currDifficulty;
		currDifficulty = (GameDifficulty)difficulty;
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

	public void StartSurvival() {
		if (SceneManager.GetActiveScene ().name != "Main")
			SceneManager.LoadScene ("Main");
		ClearAllCharacters();

		//change displayed info on survival panel

		currGameMode = GameMode.Survival;
		menu.ChangeState ("Survival");
	}

	public void IncrementSelectedSurvivalWave() {
		if (selectedSurvivalWave > highestSurvivalWave) {
			selectedSurvivalWave = highestSurvivalWave + 1;
		} else {
			selectedSurvivalWave++;
		}

		displayedSurvivalWaveNumber.text = selectedSurvivalWave.ToString (); //update the displayed text
	}

	public void DecrementSelectedSurvivalWave() {
		if (selectedSurvivalWave > 1) {
			selectedSurvivalWave--;
		}

		displayedSurvivalWaveNumber.text = selectedSurvivalWave.ToString (); //update the displayed text
	}

	public void StartSurvivalWave() {
		ClearAllCharacters (); //clear all remaining enemies
		SpawnPlayer();

		currSurvivalSpawner.StartWave (selectedSurvivalWave);

		currGameState = GameState.Active;
		menu.ChangeState ("");
		Time.timeScale = 1f;
	}

	public void ShowSurvivalRest(string waveInfo) { //add parameter to determine whether player survived or lost
		//update displayed info
		if (waveInfo.Equals ("survived")) {
			highestSurvivalWave = currSurvivalSpawner.CurrentWave;
			selectedSurvivalWave++;
			displayedSurvivalWaveNumber.text = selectedSurvivalWave.ToString ();
		} else if (waveInfo.Equals ("died")) {

		}

		menu.ChangeState ("Survival");
		currGameState = GameState.Rest;
		Time.timeScale = 0f;
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