//Written by Justin Ortiz

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameMode { Survival, Campaign };
public enum GameDifficulty { Easiest, Easy, Normal, Masochist };
public enum GameState { Menu, Cutscene, Active, Loading, Paused };

//To do:
//-different ways to complete mission -- cutscenes begin & end every mission -- complete
//	--reach location -- complete (via checkpoint & door)
//	--defeat enemy -- add cutscene to play upon enemy death -- complete (via AdvancedEnemy)
//	--collect items?
//	--puzzle? -- complete via doors, checkpoint & enemies
//-Challenges
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
//		--Option 1: joystick + buttons -- complete
//		--Option 2: tilt to move, tap to jump, swipe to attack
//		--Option 3: buttonless -- complete
//			-recode buttonless controls to be neater/simpler
//				--touch.x far enough to side to move
//				--swipe from character x to attack
//				--tap near character to jump
//	--ranged weapons: swipe hold to continue to fire??

public class GameManager : MonoBehaviour {

	public static GameMode currGameMode;
	public static GameDifficulty currDifficulty;
	public static GameState currGameState;
	private static GameState prevGameState; //for return buttons
	public static GameManager currGameManager;
	public static SurvivalSpawner currSurvivalSpawner;
	public static Vector3 currPlayerSpawnLocation;
	public static Transform cutsceneParent;

	private static MenuScript menu;
	private static Toggle soundToggle;
	private static Slider bgmSlider;
	private static Slider sfxSlider;
	private static Dropdown difficultyDropdown;
	private static Transform bgParent;
	private static Image loadingScreen;
	private static string selectedOutfit;
	private static string selectedWeapon;
	private static string selectedWeaponSpecialization;
	private static string selectedCampaignMission;
	private static int currency; //currency player currently has
	private static int currencyEarned; //currency player has recently earned
	private static List<string> unlocks; //outfits & weapons player has unlocked
	private static List<string> missions; //missions player has already completed
	private static List<string> items; //which objects player has already obtained -- i.e. "Chapter 1_Prologue_Currency01"
	private static List<string> checkpointItems; //which objects the player has obtained up until the current checkpoint
	private static Cutscene currCutscene;


	private Transform campaignMissionsParent;
	private Transform campaignTabsParent;
	private Transform missionReportParent;
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
	private List<string> selectedOutfit_weaponSpecializations;
	private bool getDefaultPlayerSpawnLocation;

	public static string[] Unlocks { get { return unlocks.ToArray (); } }
	public static string[] Missions { get { return missions.ToArray (); } }
	public static string[] Items { get { return items.ToArray (); } }
	public static string SelectedOutfit { get { return selectedOutfit; }  set { selectedOutfit = value; } }
	public static string SelectedWeapon { get { return selectedWeapon; } set { selectedWeapon = value; } }
	public static string SelectedWeaponSpecialization { get { return selectedWeaponSpecialization; } set { selectedWeaponSpecialization = value; } }
	public static string SelectedCampaignMission { get { return selectedCampaignMission; } }
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

	public void CompleteCurrentCampaignMission() {
		int missionCompleteBonus = 200; //add completion bonus for currency
		if (missions.Contains (selectedCampaignMission)) { //mission previously completed
			missionCompleteBonus = (int)(missionCompleteBonus * 0.05f); //reduce completion bonus
		} else { //not previously completed
			CompleteCampaignMission (selectedCampaignMission, true); //record mission as complete
		}
		currencyEarned += missionCompleteBonus;
		RecordItemsObtainedByPlayer (ref items, true, true); //adds items to remembered items and increases currency earned
		currency += currencyEarned; //add the currency the player earned
		StartCampaign(); //display campaign screen
		DisplayMissionReportScreen (true); //show the mission report
		DataPersistence.Save();
		Time.timeScale = 0f;
	}

	private void CompleteCampaignMission (string missionName, bool saveCompletionToArray) { //used in start method and at end of missions
		string[] missionInfo = missionName.Split ('_'); //split the name of the mission -- example: "Chapter 1_Mission 1" -> {"Chapter 1", "Mission 1"}

		Transform currChapterTransform = campaignMissionsParent.Find(missionInfo[0]); //find chapter transform using first piece of mission info -- "Chapter 1"
		Transform currMissionTransform = currChapterTransform.Find(missionName);

		currMissionTransform.Find ("Lock").gameObject.SetActive (false); //hide the lock
		currMissionTransform.Find ("Complete").gameObject.SetActive (true); //show mission as complete
		if (saveCompletionToArray) {
			missions.Add (selectedCampaignMission); //record current mission as complete
		}

		int currMissionIndex = currMissionTransform.GetSiblingIndex (); //get sibling index -- 0

		if ((currMissionIndex + 1) < currChapterTransform.childCount) { //if there is another mission within this chapter
			currChapterTransform.GetChild (currMissionIndex + 1).Find ("Lock").gameObject.SetActive (false); //remove lock from next mission
		} else { //there is no other mission in the current chapter, check for next chapter
			int chapter = int.Parse ((missionInfo [0].Split (' ')) [1]); //split chapter portion further and put into int -- example: {"Chapter 1", "Mission 1"} -> {"Chapter", "1"} -> 1
			
			if (chapter < campaignTabsParent.childCount) { //if there is a next chapter {
				Toggle tab = campaignTabsParent.GetChild(chapter).GetComponent<Toggle>(); //get tab for next chapter (desired chapter - 1)
				tab.interactable = true; //make tab usable
				tab.transform.Find ("Lock").gameObject.SetActive (false); //hide the lock
				campaignMissionsParent.GetChild (chapter).GetChild (0).Find ("Lock").gameObject.SetActive (false); //unlock the first mission in the chapter
			}
		}
	}

	public void CurrencyEarned (int amount) { //to be used by objects within missions -- once player obtains the object for the first time
		currencyEarned += amount; //add the amount the object is worth
	}

	public void DecrementSelectedSurvivalWave() {
		if (selectedSurvivalWave > 1) {
			selectedSurvivalWave--;
		} else {
			selectedSurvivalWave = 1;
		}

		UpdateSurvivalDisplayText ();
	}

	private void DisplayMissionReportScreen(bool missionSuccessful) {
		Text result = missionReportParent.Find ("Result").GetComponent<Text> ();
		Text info = missionReportParent.Find ("Info").GetComponent<Text> ();
		missionReportParent.Find ("Mission Report Retry Button").gameObject.SetActive (!missionSuccessful);

		if (missionSuccessful) {
			result.text = "Mission Complete!";
			result.color = Color.green;

			info.text = "Mission Rating: A+\nCurrency Earned: " + currencyEarned + "\nTotal currency: " + currency;
		} else {
			result.text = "Mission Failure";
			result.color = Color.black;

			info.text = "Mission Rating: F\nCurrency Earned: " + currencyEarned + "\nTotal currency: " + currency;
		}

		missionReportParent.gameObject.SetActive (true);
	}

	public void EndSurvivalWave(string waveInfo) {
		if (waveInfo.Equals ("survived")) {
			if (selectedSurvivalWave < currSurvivalSpawner.NumberOfWaves) //if the selected wave is less than we have developed/created for the players
				selectedSurvivalWave++; //encourage them to play the next one

			currencyEarned = 0; //track how much currency we earned
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

	public void FailCurrentCampaignMission() {
		currencyEarned = 0;
		StartCampaign(); //display campaign screen
		DisplayMissionReportScreen (false); //show the mission report
		Time.timeScale = 0f;
	}

	public void FilterVisibleWeaponUnlocks() {
		foreach (Transform WeaponSpecialization in unlocks_weaponsParent) {
			if (selectedOutfit_weaponSpecializations.Contains (WeaponSpecialization.name)) {
				WeaponSpecialization.gameObject.SetActive (true);
			} else {
				WeaponSpecialization.gameObject.SetActive (false);
			}
		}
		ResizeHorizontalLayoutGroup (unlocks_weaponsParent.GetComponent<RectTransform> ());
	}

	public void IncrementSelectedSurvivalWave() {
		if (selectedSurvivalWave > highestSurvivalWave) { //if the selected wave is higher than possible
			selectedSurvivalWave = highestSurvivalWave + 1; //set it to the next available wave
		} else if (selectedSurvivalWave < currSurvivalSpawner.NumberOfWaves) { //prevent from incrementing past the amount of waves in survival spawner
			selectedSurvivalWave++;
		}

		UpdateSurvivalDisplayText ();
	}

	private IEnumerator LoadLevel () {
		menu.ChangeState ("");
		loadingScreen.color = Color.white; //display load screen
		loadingScreen.raycastTarget = true;
		currGameState = GameState.Loading;

		difficultyChanged = false; //show the player has not changed the difficulty yet during this mission
		currencyEarned = 0; //show the player has not earned any currency yet for this mission

		ClearAllCharacters (); //clear all remaining enemies

		bool backgroundIsMissing = bgParent.childCount > 0 ? !bgParent.GetChild (0).tag.Equals ("Level") : true;
		bool bgNeedsToBeInstantiated = false;
		string bgFilePath;
		GameState nextGameState;

		if (currGameMode == GameMode.Survival) {
			bgFilePath = "Survival_" + ((selectedSurvivalWave - 1) / 25);
				
			if (backgroundIsMissing || !bgParent.GetChild (0).name.Contains (bgFilePath) || //background is not what it needs to be
				(selectedSurvivalWave - 1) / 25 !=  currSurvivalSpawner.PreviousWave / 25) {//player just started next set of 25 waves
				bgNeedsToBeInstantiated = true;
			}

			bgFilePath = "Levels/" + bgFilePath;

			currSurvivalSpawner.StartWave (selectedSurvivalWave);
			nextGameState = GameState.Active;
		} else {
			bgFilePath = "Levels/" + selectedCampaignMission;

			bgNeedsToBeInstantiated = true;
			nextGameState = GameState.Cutscene;
		}

		if (bgNeedsToBeInstantiated) {
			if (!backgroundIsMissing) {
				Destroy (bgParent.GetChild (0).gameObject); //destroy current background
			}

			GameObject.Instantiate (Resources.Load (bgFilePath), bgParent);//instantiate background
			bgParent.GetChild (bgParent.childCount - 1).SetAsFirstSibling (); //set the background as the first child
		}

		bool loadItems = false;
		if (getDefaultPlayerSpawnLocation) { //player is starting a new level or is starting a level over -- we need to get the original start position for level
			currPlayerSpawnLocation = bgParent.GetChild (0).Find ("Player_Spawn_Loc").position; //get the location from the level
			checkpointItems.Clear (); //reset the items the player has collected since the last checkpoint
			getDefaultPlayerSpawnLocation = false; //reset the bool in case we need to use checkpoint location next time level is loaded
		} else {
			loadItems = true;
		}

		if (nextGameState == GameState.Cutscene) { //if a mission if being loaded
			PlayCutscene (bgParent.GetChild(0).Find("Cutscenes").Find("Start").GetComponent<Cutscene>()); //play opening cutscene
		}

		SpawnPlayer(); //spawn player after we get the location and after the cutscene
		Camera.main.transform.position = new Vector3(Character.player.transform.position.x, Character.player.transform.position.y, -10);


		if (loadItems) { //player reached a checkpoint and is retrying the level
			if (checkpointItems.Count > 0) { //see if player previously attained any items
				Transform itemsParent = bgParent.GetChild (0).Find ("Items"); //get items parent
				Transform currItem;
				foreach (string itemName in checkpointItems) { //go through items collected prior to checkpoint
					currItem = itemsParent.Find((itemName.Split('_'))[2]); //search for transform -- "Chapter [n]_Mission [n]_Item Name"
					if (currItem != null) { //verify item is in the level after it was loaded
						currItem.GetComponent<Item>().PickedUpBy (Character.player, false); //add item to current player inventory
					}
				}
			}
		}

		Time.timeScale = 1f; //return time to normal -- needs to be normal to wait for seconds

		yield return new WaitForSeconds (1); //timescale needs to be >0 to work

		loadingScreen.transform.SetAsLastSibling (); //ensure load screen is still the last child (covers everything)
		loadingScreen.color = Color.clear; //hide load screen
		loadingScreen.raycastTarget = false;

		currGameState = nextGameState;
	}

	public void PauseGame() {
		prevGameState = currGameState;
		currGameState = GameState.Paused;
		Time.timeScale = 0f;
	}

	public void PlayCutscene(Cutscene c) {
		cutsceneParent.gameObject.SetActive(true); //show cutscene parent -- image, subtitles, etc.
		currCutscene = c;
		currCutscene.gameObject.SetActive (true); //ensure the cutscene object can update
		currGameState = GameState.Cutscene; //change gamestate
	}

	public void PurchaseSelectedItem() {
		if (currency >= purchase_selectedItemCost) { //if the player has enough money
			UnlockItem (purchase_selectedItemName); //unlock the item
			currency -= purchase_selectedItemCost; //update money
			DataPersistence.Save (); //save the changes

			if (purchase_selectedItemType.Equals ("Outfit")) {
				SelectOutfit (purchase_selectedItemName);
			} else { //item type will equal the weapon specialization
				SelectWeapon (purchase_selectedItemName);
			}
		} else {
			purchaseUnsuccessfulPanel.SetActive (true); //unable to purchase; inform the player
		}
	}

	public void RecordItemsObtainedByPlayerAtCheckpoint() {
		RecordItemsObtainedByPlayer (ref checkpointItems, false, false);
	}

	private void RecordItemsObtainedByPlayer(ref List<string> list, bool onlySingleAcquirance, bool rewardCurrency) {
		string itemName;
		foreach (Item i in Character.player.Inventory) {
			if (!onlySingleAcquirance || onlySingleAcquirance && i.SingleAcquirance) {
				itemName = i.ToString();
				if (!list.Contains (itemName)) {
					list.Add (itemName);

					if (rewardCurrency && itemName.Contains("Currency")) {
						currencyEarned += 50;
					}
				}
			}
		}
	}

	public void ReselectBothOutfitAndWeapon() {
		SelectOutfit (selectedOutfit);
		SelectWeapon (selectedWeapon);
	}

	public IEnumerator ResizeAllUIElements() {
		yield return new WaitForEndOfFrame ();

		//outfits
		foreach (RectTransform outfit in unlocks_outfitsParent) { //check each child
			if (unlocks.Contains (outfit.name)) { //see if it has been unlocked
				outfit.Find ("Lock").gameObject.SetActive (false); //hide the lock image

				if (outfit.name.Equals (selectedOutfit)) { //if the child is the selected outfit (and unlocked)
					outfit.Find ("Checkmark").gameObject.SetActive (true); //show selected checkmark

					//get weapon specialization
				}
			} else { //outfit not unlocked
				outfit.Find ("Sprite").GetComponent<Image> ().color = Color.black; //show outfit in black
			}
			outfit.sizeDelta = new Vector2 (outfit.rect.height, outfit.rect.height);
		}
		ResizeHorizontalLayoutGroup (unlocks_outfitsParent.GetComponent<RectTransform>()); //ensure outfit area has enough space to scroll

		//weapons
		foreach (RectTransform weaponSpecialization in unlocks_weaponsParent) {
			foreach (RectTransform weapon in weaponSpecialization) {
				if (unlocks.Contains (weapon.name)) { //see if it has been unlocked
					weapon.Find ("Lock").gameObject.SetActive (false); //hide the lock image

					if (weapon.name.Equals (selectedWeapon)) { //if the child is the selected weapon (and unlocked)
						weapon.Find ("Checkmark").gameObject.SetActive (true); //show selected checkmark
					}
				} else { //weapon not unlocked
					weapon.Find ("Sprite").GetComponent<Image> ().color = Color.black; //show weapon in black
				}
				//weapon.sizeDelta = new Vector2 (weapon.rect.height, weapon.rect.height);
			}
			ResizeHorizontalLayoutGroup (weaponSpecialization); //ensure each weaponspec has enough space
		}

		//campaign missions
		foreach (Transform chapter in campaignMissionsParent) {
			ResizeHorizontalLayoutGroup (chapter.GetComponent<RectTransform>()); //make sure each chapter has enough scroll space
		}
		ResizeHorizontalLayoutGroup (campaignMissionsParent.GetComponent<RectTransform>()); //make sure the viewport has enough scroll space
	}

	public void ResizeHorizontalLayoutGroup (RectTransform parent) {
		Vector2 newSizeDelta = Vector2.zero; //start with value of 0
		float spacing = parent.GetComponent<HorizontalLayoutGroup>().spacing;
		foreach (RectTransform child in parent) { //go through all children of rect transform
			if (child.gameObject.activeSelf) { //if the child is active/shown
				newSizeDelta.x += child.sizeDelta.x + spacing; //add to the width
			}
		}

		parent.sizeDelta = newSizeDelta;
	}

	public void RetryCampaignMission() { //player just failed a mission
		getDefaultPlayerSpawnLocation = false; //if a checkpoint was triggered, then we will start from there
		StartCoroutine (LoadLevel ());
	}

	public void ReturnToMainMenu() {
		currGameState = GameState.Menu;
		menu.ChangeState ("Main");
	}

	public void SaveSettings() {
		DataPersistence.SavePlayerPrefs ();
	}

	public void SelectCampaignMission(Transform buttonWithLockAsChild) {
		Transform lockChild = buttonWithLockAsChild.Find("Lock"); //get the lock

		if (lockChild.gameObject.activeSelf) { //if the lock is active
			//player is unable to play the level -- inform them they cannot play?
			return;
		}
		getDefaultPlayerSpawnLocation = true; //ensure we get the default spawn location for the level
		selectedCampaignMission = buttonWithLockAsChild.name; //set this as the selected mission
		StartCoroutine (LoadLevel ()); //load the level
	}

	public void SelectOutfit (string OutfitName) {
		string textToDisplay;

		Player p = (Instantiate (Resources.Load ("Characters/Player/" + OutfitName)) as GameObject).GetComponent<Player> (); //spawn prefab to get the info from script
		textToDisplay = OutfitName + "\nMax hp: " + p.MaxHP + "     Movement Speed: " + p.MovementSpeed + " m/s"; //set outfit info text

		if (unlocks.Contains (OutfitName)) { //outfit is unlocked
			displayPurchaseConfirmationButton_Outfit.SetActive (false); //hide unlock button

			selectedOutfit_weaponSpecializations = p.weaponSpecialization;

			unlocks_outfitsParent.Find(selectedOutfit).Find("Checkmark").gameObject.SetActive(false); //hide checkmark from previously selected outfit
			unlocks_outfitsParent.Find(OutfitName).Find("Checkmark").gameObject.SetActive(true); //show the checkmark for currently selected outfit
			selectedOutfit = OutfitName; //set this as current outfit to spawn as

			SelectWeapon (p.DefaultWeapon); //ensures the player always has the correct type of weapon equipped
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
		p.Die (); //destroy the player we created temporarily

		displayedSelectedOutfitInfo.text = textToDisplay; //update the displayed text
	}

	public void SelectWeapon(string WeaponName) {
		Weapon w = (Instantiate (Resources.Load ("Weapons/" + WeaponName)) as GameObject).GetComponent<Weapon> (); //spawn prefab to get the info from script
		displayedSelectedWeaponInfo.text = WeaponName + "\nDamage: " + w.Damage + "\nType: " + w.Specialization; //update info

		if (unlocks.Contains (WeaponName)) { // if the weapon is unlocked
			if (selectedOutfit_weaponSpecializations.Contains (w.Specialization)) { //if the weapon can be used with the current outfit
				displayedSelectedWeaponInfo.color = Color.black; //ensure the text is black
				displayPurchaseConfirmationButton_Weapon.SetActive (false); //hide purchase/unlock button

				unlocks_weaponsParent.Find (selectedWeaponSpecialization).Find (selectedWeapon).Find ("Checkmark").gameObject.SetActive (false); //hide checkmark from previously selected weapon
				unlocks_weaponsParent.Find (w.Specialization).Find (WeaponName).Find ("Checkmark").gameObject.SetActive (true); //show the checkmark for currently selected weapon
				selectedWeapon = WeaponName; //update the selected weapon
				selectedWeaponSpecialization = w.Specialization;
			} else {
				displayedSelectedWeaponInfo.text += "\nWeapon cannot be used with the current outfit.";
			}
		} else { //weapon not unlocked
			purchase_selectedItemName = WeaponName;
			purchase_selectedItemType = w.Specialization;
			purchase_selectedItemCost = w.UnlockCost;
			purchase_selectedItemColor = w.SpriteColor;

			if (w.UnlockCost > 0) {
				//displayedSelectedWeaponInfo.color = Color.red; //inform the player visually the item is locked
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

	public void ShowAllWeaponUnlocks() {
		foreach (Transform WeaponSpecialization in unlocks_weaponsParent) {
			if (!WeaponSpecialization.gameObject.activeSelf) {
				WeaponSpecialization.gameObject.SetActive (true);
			}
		}
		ResizeHorizontalLayoutGroup (unlocks_weaponsParent.GetComponent<RectTransform> ());
	}

	public void SkipCurrentCutscene() {
		if (missions.Contains(selectedCampaignMission)) { //if the player has already beaten the current mission
			currCutscene.EndCutscene (); //allow them to skip the cutscene
		}
	}

	private void SpawnPlayer() {
		Instantiate(Resources.Load("Characters/Player/" + selectedOutfit), currPlayerSpawnLocation, Quaternion.Euler(Vector3.zero));
		Weapon w = (Instantiate (Resources.Load ("Weapons/" + selectedWeapon)) as GameObject).GetComponent<Weapon> (); //spawn selected weapon
		Character.player.Wield(w);
	}

	private void SpawnSurvivalSpawner() {
		if (currSurvivalSpawner != null) {
			currSurvivalSpawner.gameObject.SetActive (true);
		} else {
			Instantiate (Resources.Load ("Spawners/SurvivalSpawner"));
		}
	}

	void Start () {
		if (currGameManager == null) {
			currGameManager = this;
			DontDestroyOnLoad (gameObject);

			menu = transform.Find("Canvas (Overlay)").GetComponent<MenuScript> ();
			bgParent = GameObject.Find ("Level").transform;
			loadingScreen = menu.transform.GetChild (menu.transform.childCount - 1).GetComponent<Image> ();
			cutsceneParent = menu.transform.Find ("Cutscene");

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
			missions = new List<string> ();
			items = new List<string> ();
			checkpointItems = new List<string> ();

			if (loadedData != null) {
				currency = loadedData.currency;
				highestSurvivalWave = loadedData.highestSurvivalWave;
				currSurvivalStreak = loadedData.survivalStreak;
				unlocks.AddRange (loadedData.unlocks);
				//challenges
				missions.AddRange (loadedData.missions);
				items.AddRange (loadedData.items);
				//extra01
				//extra02
			} else {
				highestSurvivalWave = 0;
				currSurvivalStreak = 0;
				currency = 0;
				unlocks.Add ("Stick it to 'em"); //default player
				unlocks.Add ("Iron Longsword"); //default weapon
				unlocks.Add ("Test Dagger");
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

			currPlayerSpawnLocation = Vector3.zero;

			unlocks_outfitsParent = GameObject.Find("Outfit Layout Group").transform; //get the transform parent
			unlocks_weaponsParent = GameObject.Find("Weapon Layout Group").transform; //get the transform parent

			displayedSelectedWeaponInfo = GameObject.Find ("Selected Weapon Text").GetComponent<Text>(); //get the text object for weapon info

			displayPurchaseConfirmationButton_Weapon = GameObject.Find ("Purchase Weapon Button");
			displayPurchaseConfirmationButton_Weapon.SetActive (false);

			unlocks_weaponsParent.parent.parent.parent.gameObject.SetActive (false);

			campaignMissionsParent = GameObject.Find ("Missions Layout Group").transform;
			campaignTabsParent = GameObject.Find ("Campaign Tab Container").transform;
			missionReportParent = GameObject.Find ("Mission Report").transform;
			missionReportParent.gameObject.SetActive (false);

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

			ReselectBothOutfitAndWeapon ();
			StartCoroutine (ResizeAllUIElements ());

			foreach (string mission in missions) { //for each loaded mission
				CompleteCampaignMission (mission, false); //make sure each mission is shown as complete without adding extra strings in the save file
			}
		} else {
			Destroy (gameObject);
		}
	}

	public void StartCampaign() {
		if (currSurvivalSpawner != null) {
			currSurvivalSpawner.gameObject.SetActive(false);
		}

		currGameMode = GameMode.Campaign;
		currGameState = GameState.Menu;
		menu.ChangeState ("Campaign");
	}

	public void StartSurvival() {
		if (SceneManager.GetActiveScene ().name != "Main")
			SceneManager.LoadScene ("Main");

		SpawnSurvivalSpawner();

		currGameMode = GameMode.Survival;
		currGameState = GameState.Menu;
		UpdateSurvivalDisplayText ();
		menu.ChangeState ("Survival");
	}

	public void StartSurvivalWave() {
		getDefaultPlayerSpawnLocation = true;
		StartCoroutine(LoadLevel());
	}

	public void StopCutscene(Cutscene c) {
		cutsceneParent.gameObject.SetActive (false); //hide pictures, subtitles, etc.
		c.gameObject.SetActive (false); //prevent cutscene from updating further
		currGameState = GameState.Active; //change gamestate
	}

	public void StopCutscene (Cutscene c, bool completeMission) {
		StopCutscene (c);

		if (completeMission) {
			CompleteCurrentCampaignMission (); //mark mission as complete
		}
	}

	public void ToggleSoundEnabled(Toggle UIToggle) {
		SoundEnabled = UIToggle.isOn;
		//enable/disable the sound
	}

	public void UnlockItem(string itemName) {
		Transform temp = unlocks_outfitsParent.Find (itemName);
		if (temp != null) { //it was an outfit
			temp.Find ("Sprite").GetComponent<Image> ().color = purchase_selectedItemColor;
			temp.Find ("Lock").gameObject.SetActive (false);
			displayPurchaseConfirmationButton_Outfit.SetActive (false);
		} else { //it was a weapon
			temp = unlocks_weaponsParent.Find(purchase_selectedItemType).Find(purchase_selectedItemName);
			temp.Find ("Sprite").GetComponent<Image> ().color = purchase_selectedItemColor;
			temp.Find ("Lock").gameObject.SetActive (false);
			displayPurchaseConfirmationButton_Weapon.SetActive(false);
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