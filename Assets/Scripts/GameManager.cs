using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameMode { Survival, Story };
public enum GameDifficulty { Easiest, Easy, Normal, Masochist };
public enum GameState { Menu, Cutscene, Active, Loading, Paused };

//To do:
//-Challenges
//	--list of challenges in gamemanager?
//-Weapon Selection:
//	--show attack animation on selection? later on
//		--show shadow for not unlocked weapons
//	--filters:
//		--default: unlocked and usable for the currently selected outfit
//		--available for current oufit
//		--all weapons
//-revisit Spawn class for cleaner register advanced enemy minion solution
//-continue survival game mode
//	--Survival Spawner
//		--bool for boss wave?
//		--attribute to store boss?
//	--AdvancedEnemy
//		--Flying Capability
//			--fly straight horizontally, swoop down using sine or cosine formula to attack player
//			--drop projectiles onto player
//				--projectiles break on collision with environment
//	--Code Boss
//		--wave of enemies prior to boss spawns?
//		--cutscene played when spawned??
//		--inherit from character class?
//		--ability to disable knockback
//		--no flinch time; only delays
//		--ability to spawn enemies
//		--hp bar displayed at the bottom/top? GetHPSliderPos(), GetHPSliderSizeDelta()
//		--Script delegates so behavior can be set in the inspector?
//--Player Mechanics
//	--multiple control schemes
//		--Option 1: Analog stick on screen to move, 1 button on screen for attack, 1 button for jump -- able to scale/reposition each in settings
//		--Option 2: tilt to move, tap to jump, swipe to attack
//		--Option 3: press and hold to move, tap button on screen to jump, swipe to attack
//	--ranged weapons: swipe hold to continue to fire??
//

public class GameManager : MonoBehaviour {

	public static GameMode currGameMode;
	public static GameDifficulty currDifficulty;
	public static GameState currGameState;
	private static GameState prevGameState; //for return buttons
	public static GameManager currGameManager;
	public static SurvivalSpawner currSurvivalSpawner;
	private static MenuScript menu;
	private static Toggle soundToggle;
	private static Slider bgmSlider;
	private static Slider sfxSlider;
	private static Dropdown difficultyDropdown;
	private static Transform bgParent;
	private static Image loadingScreen;
	private static string selectedOutfit;
	private static string selectedWeapon;
	private static int currency;
	private static List<string> unlocks;

	private InputField displayedSurvivalWaveNumber;
	private Text displayedSurvivalWaveInfo;
	private Text displayedSurvivalWaveWarning;
	private int selectedSurvivalWave;
	private int highestSurvivalWave;
	private int currSurvivalStreak;
	private bool difficultyChanged;
	private Transform unlocks_outfitsParent;
	private Transform unlocks_weaponsParent;
	private Text displayedSelectedOutfitInfo;
	private Text displayedSelectedWeaponInfo;
	private string purchase_selectedItemName;
	private string purchase_selectedItemType;
	private Color purchase_selectedItemColor;
	private int purchase_selectedItemCost;
	private Text purchaseConfirmationText;
	private GameObject displayPurchaseConfirmationButton_Outfit;
	private GameObject displayPurchaseConfirmationButton_Weapon;
	private GameObject purchaseUnsuccessfulPanel;

	public static string[] Unlocks { get { return unlocks.ToArray (); } }
	public static string SelectedOutfit { get { return selectedOutfit; }  set { selectedOutfit = value; } }
	public static string SelectedWeapon { get { return selectedWeapon; } set { selectedWeapon = value; } }
	public static bool SoundEnabled { get { return soundToggle.isOn; } set { soundToggle.isOn = value; } }
	public static float BGMVolume { get { return bgmSlider.value; } set { bgmSlider.value = value; } }
	public static float SFXVolume { get { return sfxSlider.value; } set { sfxSlider.value = value; } }
	public int Currency { get { return currency; } }
	public int HighestSurvivalWave { get { return highestSurvivalWave; } }
	public int CurrentSurvivalStreak { get { return currSurvivalStreak; } }

	private void ClearAllCharacters() {
		Transform temp = GameObject.FindGameObjectWithTag ("Character Parent").transform;
		foreach (Transform child in temp) {
			child.GetComponent<Character>().Die();
		}
	}

	public void DecrementSelectedSurvivalWave() {
		if (selectedSurvivalWave > 1) {
			selectedSurvivalWave--;
		} else {
			selectedSurvivalWave = 1;
		}

		UpdateSurvivalDisplayText ();
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

	public void ExitToDesktop () {
		Application.Quit ();
	}

	public void IncrementSelectedSurvivalWave() {
		if (selectedSurvivalWave > highestSurvivalWave) { //if the selected wave is higher than possible
			selectedSurvivalWave = highestSurvivalWave + 1; //set it to the next available wave
		} else if (selectedSurvivalWave < currSurvivalSpawner.NumberOfWaves) { //prevent from incrementing past the amount of waves in survival spawner
			selectedSurvivalWave++;
		}

		UpdateSurvivalDisplayText ();
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

	public void PurchaseSelectedItem() {
		if (currency >= purchase_selectedItemCost) { //if the player has enough money
			UnlockItem (purchase_selectedItemName); //unlock the item
			currency -= purchase_selectedItemCost; //update money
			DataPersistence.Save (); //save the changes

			if (purchase_selectedItemType.Equals ("Outfit")) {
				SelectOutfit (purchase_selectedItemName);
			} else if (purchase_selectedItemType.Equals ("Weapon")) {
				SelectWeapon (purchase_selectedItemName);
			}
		} else {
			purchaseUnsuccessfulPanel.SetActive (true); //unable to purchase; inform the player
		}
	}

	public void ReturnToMainMenu() {
		currGameState = GameState.Menu;
		menu.ChangeState ("Main");
	}

	public void SaveSettings() {
		DataPersistence.SavePlayerPrefs ();
	}

	public void SelectOutfit (string OutfitName) {
		string textToDisplay;

		Player p = (Instantiate (Resources.Load ("Characters/Player/" + OutfitName)) as GameObject).GetComponent<Player> (); //spawn prefab to get the info from script
		textToDisplay = "Name: " + OutfitName + "\nMax hp: " + p.MaxHP + "     Movement Speed: " + p.MovementSpeed + " m/s"; //set outfit info text

		if (unlocks.Contains (OutfitName)) { //outfit is unlocked
			displayPurchaseConfirmationButton_Outfit.SetActive (false); //hide unlock button

			unlocks_outfitsParent.Find(selectedOutfit).Find("Checkmark").gameObject.SetActive(false); //hide checkmark from previously selected outfit
			unlocks_outfitsParent.Find(OutfitName).Find("Checkmark").gameObject.SetActive(true); //show the checkmark for currently selected outfit
			selectedOutfit = OutfitName; //set this as current outfit to spawn as
		} else { //outfit not unlocked
			purchase_selectedItemName = OutfitName; //remember which outfit we just clicked on
			purchase_selectedItemType = "Outfit";
			purchase_selectedItemCost = p.UnlockCost; //remember the cost
			purchase_selectedItemColor = p.SpriteColor;

			if (p.UnlockCost > 0) { //if the outfit is for sale
				textToDisplay += "\nCost: " + purchase_selectedItemCost; //display the cost
				displayPurchaseConfirmationButton_Outfit.SetActive (true); //display the unlock button
			} else { //outfit is only available after completing a challenge
				textToDisplay += "\nChallenge to unlock: " + "[null]"; //display which challenge must be completed
			}
		}
		p.Die (); //destry the player we created temporarily

		displayedSelectedOutfitInfo.text = textToDisplay; //update the displayed text
	}

	public void SelectWeapon(string WeaponName) {
		Weapon w = (Instantiate (Resources.Load ("Weapons/" + WeaponName)) as GameObject).GetComponent<Weapon> (); //spawn prefab to get the info from script
		displayedSelectedWeaponInfo.text = "Damage: " + w.Damage + "\nType: " + w.Specialization; //update info

		if (unlocks.Contains (WeaponName)) { // if the weapon is unlocked
			displayedSelectedWeaponInfo.color = Color.black; //ensure the text is black
			displayPurchaseConfirmationButton_Weapon.SetActive (false); //hide purchase/unlock button

			unlocks_weaponsParent.Find(selectedWeapon).Find("Checkmark").gameObject.SetActive(false); //hide checkmark from previously selected weapon
			unlocks_weaponsParent.Find(WeaponName).Find("Checkmark").gameObject.SetActive(true); //show the checkmark for currently selected weapon
			selectedWeapon = WeaponName; //update the selected weapon
		} else { //weapon not unlocked
			purchase_selectedItemName = WeaponName;
			purchase_selectedItemType = "Weapon";
			purchase_selectedItemCost = w.UnlockCost;
			purchase_selectedItemColor = w.SpriteColor;

			if (w.UnlockCost > 0) {
				displayedSelectedWeaponInfo.color = Color.red; //inform the player visually the item is locked
				displayedSelectedWeaponInfo.text += "\nCost: " + w.UnlockCost;
				displayPurchaseConfirmationButton_Weapon.SetActive (true); //display purchase/unlock button
			} else {
				displayedSelectedWeaponInfo.text += "\nChallenge to unlock:" + "[null]";
			}
		}

		Destroy (w.gameObject);
	}

	public void SetDifficulty(Dropdown difficulty) {
		SetDifficulty (difficulty.value);
	}

	public void SetDifficulty (int value) {
		currDifficulty = (GameDifficulty)value;
		difficultyDropdown.value = value;
		difficultyChanged = true;
	}

	public void SetSelectedSurvivalWave(InputField inputField) {
		selectedSurvivalWave = int.Parse (inputField.text);
		if (selectedSurvivalWave > highestSurvivalWave + 1)
			selectedSurvivalWave = highestSurvivalWave + 1;
		else if (selectedSurvivalWave < 0)
			selectedSurvivalWave = 1;

		inputField.text = selectedSurvivalWave.ToString ();
	}

	private void SpawnPlayer() {
		Instantiate(Resources.Load("Characters/Player/" + selectedOutfit));
		Weapon w = (Instantiate (Resources.Load ("Weapons/" + selectedWeapon)) as GameObject).GetComponent<Weapon> (); //spawn selected weapon
		Character.player.Wield(w);
	}

	private void SpawnSurvivalSpawner() {
		Instantiate(Resources.Load("Spawners/SurvivalSpawner"));
	}

	void Start () {
		if (currGameManager == null) {
			currGameManager = this;
			DontDestroyOnLoad (gameObject);

			menu = transform.GetChild (0).GetComponent<MenuScript> ();
			bgParent = GameObject.FindGameObjectWithTag ("Camera Canvas").transform;
			loadingScreen = bgParent.GetChild (bgParent.childCount - 1).GetComponent<Image> ();

			soundToggle = GameObject.Find ("Sound Toggle").GetComponent<Toggle>();
			bgmSlider = GameObject.Find ("BGM Slider").GetComponent<Slider> ();
			bgmSlider.handleRect.sizeDelta = new Vector2 (Screen.width * 0.07f, 0); //keep the handles somewhat circular
			sfxSlider = GameObject.Find ("SFX Slider").GetComponent<Slider> ();
			sfxSlider.handleRect.sizeDelta = new Vector2 (Screen.width * 0.07f, 0);
			difficultyDropdown = GameObject.Find ("Difficulty Dropdown").GetComponent<Dropdown> ();

			Vector2 contentSize = new Vector2 (0, Screen.height * 0.1f);
			difficultyDropdown.template.sizeDelta = contentSize * difficultyDropdown.options.Count;
			difficultyDropdown.template.GetComponent<ScrollRect> ().content.sizeDelta = contentSize;
			difficultyDropdown.template.GetComponent<ScrollRect> ().content.GetChild (0).GetComponent<RectTransform> ().sizeDelta = contentSize;

			PlayerData loadedData = DataPersistence.Load (); //load player save data
			unlocks = new List<string>();

			if (loadedData != null) {
				currency = loadedData.currency;
				highestSurvivalWave = loadedData.highestSurvivalWave;
				currSurvivalStreak = loadedData.survivalStreak;
				unlocks.AddRange (loadedData.unlocks);
				//challenges
				//missions
				//extra00
				//extra01
				//extra02
			} else {
				highestSurvivalWave = 0;
				currSurvivalStreak = 0;
				currency = 0;
				unlocks.Add ("Stick it to 'em"); //default player
				unlocks.Add ("Iron Longsword"); //default weapon
				//challenges
				//missions
				//extra00
				//extra01
				//extra02
			}

			SetDifficulty ((int)currDifficulty); //set the current difficulty to loaded/preset value
			sfxSlider.value = SFXVolume; //adjust sfx bar to loaded/preset value
			bgmSlider.value = BGMVolume; //adjust bgm bar to loaded/preset value
			soundToggle.isOn = SoundEnabled; //set soundtoggle

			selectedSurvivalWave = 1;

			unlocks_outfitsParent = GameObject.Find("Outfit Layout Group").transform; //get the transform parent
			unlocks_outfitsParent.GetComponent<RectTransform>().sizeDelta = new Vector2 (unlocks_outfitsParent.childCount * (unlocks_outfitsParent.GetChild(0).GetComponent<RectTransform>().sizeDelta.x + 5), 0); //ensure the size of the view area is the right size
			foreach (Transform child in unlocks_outfitsParent) { //check each child
				if (unlocks.Contains (child.name)) { //see if it has been unlocked
					child.Find ("Lock").gameObject.SetActive (false); //hide the lock image

					if (child.name.Equals (selectedOutfit)) { //if the child is the selected outfit (and unlocked)
						child.Find ("Checkmark").gameObject.SetActive (true); //show selected checkmark
					}
				} else { //outfit not unlocked
					child.Find ("Sprite").GetComponent<Image> ().color = Color.black; //show outfit in black
				}
			}

			unlocks_weaponsParent = GameObject.Find("Weapon Layout Group").transform; //get the transform parent
			unlocks_weaponsParent.GetComponent<RectTransform>().sizeDelta = new Vector2 (unlocks_weaponsParent.childCount * (unlocks_weaponsParent.GetChild(0).GetComponent<RectTransform>().sizeDelta.x + 5), 0); //ensure the size of the view area is the right size
			foreach (Transform child in unlocks_weaponsParent) { //check each child
				if (unlocks.Contains (child.name)) { //see if it has been unlocked
					child.Find ("Lock").gameObject.SetActive (false); //hide the lock image

					if (child.name.Equals (selectedWeapon)) { //if the child is the selected weapon (and unlocked)
						child.Find ("Checkmark").gameObject.SetActive (true); //show selected checkmark
					}
				} else { //weapon not unlocked
					child.Find ("Sprite").GetComponent<Image> ().color = Color.black; //show weapon in black
				}
			}

			displayedSelectedWeaponInfo = GameObject.Find ("Selected Weapon Text").GetComponent<Text>(); //get the text object for weapon info

			displayPurchaseConfirmationButton_Weapon = GameObject.Find ("Purchase Weapon Button");
			displayPurchaseConfirmationButton_Weapon.SetActive (false);

			unlocks_weaponsParent.parent.parent.parent.gameObject.SetActive (false);

			displayedSurvivalWaveNumber = GameObject.Find ("Selected Wave Number Input Field").GetComponent<InputField> ();
			displayedSurvivalWaveInfo = GameObject.Find ("Survival Description Text").GetComponent<Text> ();
			displayedSurvivalWaveWarning = GameObject.Find ("Wave Warnings Text").GetComponent<Text> ();
			displayedSelectedOutfitInfo = GameObject.Find ("Selected Outfit Text").GetComponent<Text> ();

			purchaseConfirmationText = GameObject.Find ("Purchase Confirmation Text").GetComponent<Text>();
			purchaseConfirmationText.transform.parent.gameObject.SetActive (false);

			displayPurchaseConfirmationButton_Outfit = GameObject.Find ("Purchase Outfit Button");
			displayPurchaseConfirmationButton_Outfit.SetActive (false);

			purchaseUnsuccessfulPanel = GameObject.Find ("Purchase Unsuccessful");
			purchaseUnsuccessfulPanel.SetActive (false);

			currGameState = GameState.Menu;
		} else {
			Destroy (gameObject);
		}
	}

	public void StartStory() {
		ClearAllCharacters();

		currGameMode = GameMode.Story;
		currGameState = GameState.Menu;
		menu.ChangeState ("Story");
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

	public void StartSurvivalWave() {
		StartCoroutine(LoadLevel());
	}

	public void ToggleSoundEnabled(Toggle UIToggle) {
		SoundEnabled = UIToggle.isOn;
		//enable/disable the sound
	}

	public void UnlockItem(string itemName) {
		Transform temp = unlocks_outfitsParent.Find (itemName);
		if (temp != null) { //it was an outfit
			displayPurchaseConfirmationButton_Outfit.SetActive (false);
			temp.Find ("Sprite").GetComponent<Image> ().color = purchase_selectedItemColor;
			temp.Find ("Lock").gameObject.SetActive (false);
		}
		unlocks.Add (itemName);
	}

	public void UnPauseGame() {
		currGameState = prevGameState;
		Time.timeScale = 1f;
	}

	public void UpdatePurchaseConfirmationText() {
		purchaseConfirmationText.text = "Are you sure you would like to unlock " + purchase_selectedItemName + " for " + purchase_selectedItemCost + "?"
			+ "\n\nYour Currency: " + currency;
	}

	private void UpdateSurvivalDisplayText() {
		displayedSurvivalWaveNumber.text = selectedSurvivalWave.ToString ();
		displayedSurvivalWaveWarning.text = currSurvivalSpawner.GetWaveWarning (selectedSurvivalWave); //update the wave warning text
	}
}