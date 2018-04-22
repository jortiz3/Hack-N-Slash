using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameMode { Survival, Story };
public enum GameDifficulty { Easiest, Easy, Normal, Masochist };
public enum GameState { Menu, Cutscene, Active, Loading, Paused };

//To do:
//-continue survival game mode
//	--Code Boss
//		--wave of enemies prior to boss spawns?
//		--cutscene played when spawned??
//		--inherit from character class?
//		--ability to disable knockback
//		--shorter flinch time
//		--ability to spawn enemies
//		--hp bar displayed at the bottom?
//		--Script delegates so behavior can be set in the inspector?
//--Character Class
//	--recognize if damage from weapon or touching enemy
//--Player Mechanics
//	--multiple control schemes
//		--Option 1: tilt to move, tap to jump, swipe to attack
//		--Option 2: press and hold to move, tap button on screen to jump, swipe to attack
//

public class GameManager : MonoBehaviour {

	public static GameMode currGameMode;
	public static GameDifficulty currDifficulty;
	public static GameState currGameState;
	private static GameState prevGameState; //for return buttons

	private bool difficultyChanged;

	public static GameManager currGameManager;
	public static SurvivalSpawner currSurvivalSpawner;

	private static MenuScript menu;

	private static Toggle soundToggle;
	private static Slider bgmSlider;
	private static Slider sfxSlider;
	private static Dropdown difficultyDropdown;

	private static Transform bgParent;
	private static Image loadingScreen;

	private static string selectedCharacter;

	private Text displayedSurvivalWaveNumber;
	private Text displayedSurvivalWaveInfo;
	private Text displayedSurvivalWaveWarning;

	private int selectedSurvivalWave;
	private int highestSurvivalWave;
	private int currSurvivalStreak;

	private static int currency;

	public static string SelectedCharacter { get { return selectedCharacter; } }

	public static bool SoundEnabled { get { return soundToggle.isOn; } set { soundToggle.isOn = value; } }
	public static float BGMVolume { get { return bgmSlider.value; } set { bgmSlider.value = value; } }
	public static float SFXVolume { get { return sfxSlider.value; } set { sfxSlider.value = value; } }

	public int Currency { get { return currency; } }
	public int HighestSurvivalWave { get { return highestSurvivalWave; } }
	public int CurrentSurvivalStreak { get { return currSurvivalStreak; } }

	void Start () {
		if (currGameManager == null) {
			currGameManager = this;
			DontDestroyOnLoad (gameObject);

			menu = transform.GetChild (0).GetComponent<MenuScript> ();
			bgParent = GameObject.FindGameObjectWithTag ("Camera Canvas").transform;
			loadingScreen = bgParent.GetChild (bgParent.childCount - 1).GetComponent<Image> ();

			soundToggle = GameObject.Find ("Sound Toggle").GetComponent<Toggle>();
			bgmSlider = GameObject.Find ("BGM Slider").GetComponent<Slider> ();
			bgmSlider.handleRect.sizeDelta = new Vector2 (Screen.width * 0.08f, 0);
			sfxSlider = GameObject.Find ("SFX Slider").GetComponent<Slider> ();
			sfxSlider.handleRect.sizeDelta = new Vector2 (Screen.width * 0.08f, 0);
			difficultyDropdown = GameObject.Find ("Difficulty Dropdown").GetComponent<Dropdown> ();

			Vector2 contentSize = new Vector2 (0, Screen.height * 0.1f);
			difficultyDropdown.template.sizeDelta = contentSize * difficultyDropdown.options.Count;
			difficultyDropdown.template.GetComponent<ScrollRect> ().content.sizeDelta = contentSize;
			difficultyDropdown.template.GetComponent<ScrollRect> ().content.GetChild (0).GetComponent<RectTransform> ().sizeDelta = contentSize;

			PlayerData loadedData = DataPersistence.Load (); //load player save data

			if (loadedData != null) {
				selectedCharacter = loadedData.selectedCharacter;
				currency = loadedData.currency;
				highestSurvivalWave = loadedData.highestSurvivalWave;
				currSurvivalStreak = loadedData.survivalStreak;
			} else {
				selectedCharacter = "Default Player";
				highestSurvivalWave = 0;
				currSurvivalStreak = 0;
				currency = 0;

				SetDifficulty (2);
				SoundEnabled = true;
				BGMVolume = 1f;
				SFXVolume = 1f;
			}


			selectedSurvivalWave = 1;

			displayedSurvivalWaveNumber = GameObject.Find ("Selected Wave Number").GetComponent<Text> ();
			displayedSurvivalWaveInfo = GameObject.Find ("Survival Description Text").GetComponent<Text> ();
			displayedSurvivalWaveWarning = GameObject.Find ("Wave Warnings Text").GetComponent<Text> ();

			currGameState = GameState.Menu;
		} else {
			Destroy (gameObject);
		}
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

		SpawnSurvivalSpawner();

		currGameMode = GameMode.Survival;
		currGameState = GameState.Menu;
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
		StartCoroutine(LoadLevel());
	}

	public void EndSurvivalWave(string waveInfo) {
		if (waveInfo.Equals ("survived")) {
			if (selectedSurvivalWave < currSurvivalSpawner.NumberOfWaves) //if the selected wave is less than we have developed/created for the players
				selectedSurvivalWave++; //encourage them to play the next one
			
			int currencyEarned = 0; //track how much currency we earned
			if (highestSurvivalWave == currSurvivalSpawner.CurrentWave) { //player completed the next available wave
				currencyEarned++; //1 currency for completing for the first time

				if (currSurvivalSpawner.CurrentWave != 0) {
					if (currSurvivalSpawner.CurrentWave % 25 == 0) //first clear of boss wave
						currencyEarned += 25;
					if (currSurvivalSpawner.CurrentWave % 100 == 0) //first clear of megaboss wave
						currencyEarned += 75;
				}

				if (!difficultyChanged) { //if the player didn't alter the difficulty
					switch (currDifficulty) {
					case GameDifficulty.Normal://remained on normal entire wave
						currencyEarned += 2;
						break;
					case GameDifficulty.Masochist://remained on masochist entire wave
						currencyEarned += 5;
						break;
					}
				}

				highestSurvivalWave = currSurvivalSpawner.CurrentWave + 1; //update highest survival wave completed
			}

			if (currSurvivalSpawner.CurrentWave < 25) //easier waves only give +1
				currencyEarned++;
			else if (currSurvivalSpawner.CurrentWave < 50) //harder +2
				currencyEarned += 2;
			else if (currSurvivalSpawner.CurrentWave < 75) //even harder +3
				currencyEarned += 3;
			else if (currSurvivalSpawner.CurrentWave < 100) //just mean +4
				currencyEarned += 4;
			else //ludicrous +5
				currencyEarned += 5;

			currSurvivalStreak++; //increase their survival streak

			if (currSurvivalStreak % 5 == 0) //bonuses for surviving multiple waves in a row
				currencyEarned += 5;
			if (currSurvivalStreak % 20 == 0)
				currencyEarned += 20;
			if (currSurvivalStreak % 100 == 0)
				currencyEarned += 300;

			currency += currencyEarned; //add the currency earned
			UpdateSurvivalDisplayText (); //inform the player how much they earned
			displayedSurvivalWaveInfo.text = "Wave " + currSurvivalSpawner.CurrentWave + " complete!\n\n+" + currencyEarned + " currency! You now have: " + currency + "\n+1 survival streak (" + currSurvivalStreak + ")";
		} else if (waveInfo.Equals ("died")) {
			displayedSurvivalWaveInfo.text = "Wave " + currSurvivalSpawner.CurrentWave + " lost!\n\n-No currency gained\n-Win streak reset to 0";
			currSurvivalStreak = 0; //reset survival streak
		}

		menu.ChangeState ("Survival");
		currGameState = GameState.Menu;
		Time.timeScale = 0f; //freeze game

		DataPersistence.Save (); //save the game no matter what
	}

	private void UpdateSurvivalDisplayText() {
		displayedSurvivalWaveNumber.text = selectedSurvivalWave.ToString ();
		displayedSurvivalWaveWarning.text = currSurvivalSpawner.GetWaveWarning (selectedSurvivalWave); //update the wave warning text
	}

	public void StartStory() {
		ClearAllCharacters();

		currGameMode = GameMode.Story;
		currGameState = GameState.Menu;
		menu.ChangeState ("Story");
	}

	public void SetDifficulty(Dropdown difficulty) {
		SetDifficulty (difficulty.value);
	}

	public void SetDifficulty (int value) {
		currDifficulty = (GameDifficulty)value;
		difficultyDropdown.value = value;
		difficultyChanged = true;
	}

	private IEnumerator LoadLevel() {
		loadingScreen.color = Color.white; //display load screen

		difficultyChanged = false;

		ClearAllCharacters (); //clear all remaining enemies
		SpawnPlayer();

		bool backgroundIsMissing = !bgParent.GetChild (0).tag.Equals ("Background");

		if (currGameMode == GameMode.Survival) {
			if ((selectedSurvivalWave - 1 != currSurvivalSpawner.PreviousWave && selectedSurvivalWave % 25 == 1)//player just started a new wave, and background needs to change
				|| backgroundIsMissing) { //or if background is missing at the moment

				if (!backgroundIsMissing) { //if we have a background
					Destroy (bgParent.GetChild (0).gameObject); //destroy current background
				}

				GameObject.Instantiate (Resources.Load ("Backgrounds/Survival_" + (selectedSurvivalWave - 1)), bgParent);//instantiate background
				bgParent.GetChild (bgParent.childCount - 1).SetAsFirstSibling (); //set the background as the first child

				yield return new WaitForSeconds(1);
			}

			currSurvivalSpawner.StartWave (selectedSurvivalWave);
		} else {
			//load story level
		}

		loadingScreen.transform.SetAsLastSibling (); //ensure load screen is still the last child (covers everything)
		loadingScreen.color = Color.clear; //hide load screen

		currGameState = GameState.Active;
		menu.ChangeState ("");
		Time.timeScale = 1f;
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

	public void SaveSettings() {
		DataPersistence.SavePlayerPrefs ();
	}

	public void ExitToDesktop () {
		Application.Quit ();
	}

	public void ToggleSoundEnabled(Toggle UIToggle) {
		//get the toggle value
		//enable/disable the sound
	}
}