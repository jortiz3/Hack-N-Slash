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
//	--wave rest UI
//		--current wave
//		--enemies slain
//		--currency earned; +1 for clearing; +1 for first time clear; +5 bonus for not dying in 5 rounds; +10 bonus for not dying in 10 rounds; +20, etc.
//	--wave lose UI
//		--current wave
//		--reset wave button needs to delete all enemies and respawn player
//		--state the winstreak is reset
//	--background transitions
//		--bosses??
//-make the character health bars bigger/scale

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

	void Start () {
		if (currGameManager == null) {
			currGameManager = this;
			DontDestroyOnLoad (gameObject);

			menu = transform.GetChild (0).GetComponent<MenuScript> ();

			//load player save data
			selectedCharacter = "Default Player";

			currGameState = GameState.Menu;
		} else {
			Destroy (gameObject);
		}
	}

	public void UpdateGameDifficulty(int difficulty) {
		prevDifficulty = currDifficulty;
		currDifficulty = (GameDifficulty)difficulty;
	}

	public void StartSurvival() {
		if (SceneManager.GetActiveScene ().name != "Main")
			SceneManager.LoadScene ("Main");
		//instantiate the survival spawn

		//destroy all characters
		Transform temp = GameObject.FindGameObjectWithTag ("Character Parent").transform;
		foreach (Transform child in temp) {
			child.GetComponent<Character>().Die();
		}

		//spawn a new player?
		Instantiate(Resources.Load("Characters/" + selectedCharacter));

		currGameMode = GameMode.Survival;
		currGameState = GameState.Active;
		currSurvivalSpawner.Initialize (0);
		StartNextSurvivalWave ();

		Time.timeScale = 1f;
	}

	public void StartNextSurvivalWave() {
		currSurvivalSpawner.StartWave (true);
		menu.ChangeState ("");
		currGameState = GameState.Active;
		Time.timeScale = 1f;
	}

	public void RestartSurvivalWave() {
		currSurvivalSpawner.StartWave (false);
		menu.ChangeState ("");
		currGameState = GameState.Active;
		Time.timeScale = 1f;

		//spawn player
	}

	public void ShowSurvivalRest() {
		menu.ChangeState ("SurvivalRest");
		currGameState = GameState.Rest;
		Time.timeScale = 0f;
	}

	public void ShowSurvivalLose() {
		menu.ChangeState ("SurvivalLose");
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