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
//-filter challenges
//  --new method Challenge >> bool MeetsFilter(string filter) { return requirement.Contains(filter); }
//  --new method GameManager
//      ---set challenge gameobject true/false based on method
//      ---resize challenge list
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
//-create unity account for common silk studios so apk can be >> com.CommonSilkStudios.SwordSwipe instead of containing my name

public class GameManager_SwordSwipe : MonoBehaviour {

	public static GameMode currGameMode;
	public static GameDifficulty currDifficulty;
	public static GameState currGameState;
	private static GameState prevGameState; //for return buttons
	public static GameManager_SwordSwipe currGameManager;
	public static CameraManager currCameraManager;
	public static SurvivalSpawner currSurvivalSpawner;
	public static Vector3 currPlayerSpawnLocation;
	public static Transform cutsceneParent;
	private static ChallengeNotificationManager challengeManager;
	private static AudioManager audioManager;

	private static MenuScript menu;
	private static AdvertisementManager adManager; //Script to display ads and track whether the ad was completed or not
	private static Toggle soundToggle;
	private static Toggle continuousWaveToggle;
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
	private static List<string> checkpointItems; //items stored after reaching checkpoint
	private static List<string> challenges; //challenges the player has completed
	private static List<string> checkpointChallenges; //challenges stored after reaching checkpoint
	private static List<string> temporaryChallenges; //challenges obtained prior to checkpoint -- can be lost upon death
	private static List<string> extra01;
	private static List<string> extra02;
	private static Cutscene currCutscene;


	private Transform campaignMissionsParent;
	private Transform campaignTabsParent;
	private Transform missionReportParent;
	private Transform challengesParent;
	private InputField displayedSurvivalWaveNumber;
	private Text displayedSurvivalWaveInfo;
	private int selectedSurvivalWave;
	private int highestSurvivalWave;
	private int currSurvivalStreak;
	private bool difficultyChanged;
	private GameDifficulty lowestDifficulty;
	private Transform unlocks_outfitsParent;
	private Transform unlocks_weaponsParent;
	private Text displayedSelectedOutfitInfo;
	private Text displayedSelectedWeaponInfo;
	private string purchase_selectedItemName;
	private string purchase_selectedItemType;
	private Color purchase_selectedItemColor;
	private int purchase_selectedItemCost;
	private string purchase_selectedItemChallenge;
	private Text purchaseConfirmationText;
	private Button displayPurchaseConfirmationButton_Outfit;
	private Button displayPurchaseConfirmationButton_Weapon;
	private List<string> selectedOutfit_weaponSpecializations;
	private Text currencyText_Survival;
	private Text currencyText_Unlocks;
	private string selectedCampaignMissionObjective;
	private TextFade objectiveText;
	private bool getDefaultPlayerSpawnLocation;
	private float playTime;
	private float numOfRoundsSinceLastAd; //tracks how many missions/survival waves a player has played since the last 'forced' ad
	private bool ad_rewards_given;
	private bool iap_rewards_given;

	public static string[] Unlocks { get { return unlocks.ToArray (); } }
	public static string[] Missions { get { return missions.ToArray (); } }
	public static string[] Items { get { return items.ToArray (); } }
	public static string[] Challenges { get { return challenges.ToArray(); } }
	public static string[] Extra01 { get { return extra01.ToArray(); } }
	public static string[] Extra02 { get { return extra02.ToArray(); } }
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

	public void ChallengeActionComplete(string actionPerformed) { //will be called by other classes to notify GameManager when a challenge might be complete
		Challenge challenge;
		for (int i = 0; i < challengesParent.childCount; i++) { //go through all challenges
			challenge = challengesParent.GetChild(i).GetComponent<Challenge>(); //get challenge component
			if (!challenge.Complete) { //if the challenge isn't already complete
				if (challenge.CheckRequirementMet(actionPerformed)) { //check if challenge requirement met
					if (!temporaryChallenges.Contains(challenge.Name) && !checkpointChallenges.Contains(challenge.Name)) { //make sure challenge not temporarily completed
						if (currGameState == GameState.Active) {
							temporaryChallenges.Add(challenge.Name); //temporarily store challenge as complete
							challengeManager.DisplayNotification(challenge.Name + "\n(Temporary)", challenge.NotificationSprite, Color.gray); //show a temporary notification in gray
						} else {
							CompleteChallenge(challenge);
						}
						break;
					} else { //challenge already complete
						break;
					}
				}

			}
		}
	}

	public void CheckpointReached() {
		checkpointChallenges.AddRange(temporaryChallenges); //store the temporary challenges
		temporaryChallenges.Clear(); //clear temporary challenges so we do not duplicate them
		RecordItemsObtainedByPlayer(ref checkpointItems, false, false); //record the items in the player's inventory
		getDefaultPlayerSpawnLocation = false;
	}

	private void ClearAllCharacters() {
		Transform temp = GameObject.FindGameObjectWithTag ("Character Parent").transform;
		foreach (Transform child in temp) {
			child.GetComponent<Character>().Die();
		}
	}

	private void CompleteCampaignMission(string missionName, bool saveCompletionToArray) { //used in start method and at end of missions
		string[] missionInfo = missionName.Split('_'); //split the name of the mission -- example: "Chapter 1_Mission 1" -> {"Chapter 1", "Mission 1"}

		Transform currChapterTransform = campaignMissionsParent.Find(missionInfo[0]); //find chapter transform using first piece of mission info -- "Chapter 1"
		Transform currMissionTransform = currChapterTransform.Find(missionName);

		currMissionTransform.Find("Lock").gameObject.SetActive(false); //hide the lock
		currMissionTransform.Find("Complete").gameObject.SetActive(true); //show mission as complete
		if (saveCompletionToArray) {
			missions.Add(selectedCampaignMission); //record current mission as complete
		}

		int currMissionIndex = currMissionTransform.GetSiblingIndex(); //get sibling index -- 0

		if ((currMissionIndex + 1) < currChapterTransform.childCount) { //if there is another mission within this chapter
			currChapterTransform.GetChild(currMissionIndex + 1).Find("Lock").gameObject.SetActive(false); //remove lock from next mission
		} else { //there is no other mission in the current chapter, check for next chapter
			int chapter = int.Parse((missionInfo[0].Split(' '))[1]); //split chapter portion further and put into int -- example: {"Chapter 1", "Mission 1"} -> {"Chapter", "1"} -> 1

			if (chapter < campaignTabsParent.childCount) { //if there is a next chapter {
				Toggle tab = campaignTabsParent.GetChild(chapter).GetComponent<Toggle>(); //get tab for next chapter (desired chapter - 1)
				tab.interactable = true; //make tab usable
				tab.transform.Find("Lock").gameObject.SetActive(false); //hide the lock
				campaignMissionsParent.GetChild(chapter).GetChild(0).Find("Lock").gameObject.SetActive(false); //unlock the first mission in the chapter
			}
		}
	}

	private void CompleteAllTemporaryChallenges() { //goes through temporary and checkpoint lists to complete the challenges -- intended to be used at the end of a survival wave or campaign mission
		checkpointChallenges.AddRange(temporaryChallenges); //store the temporary challenges
		temporaryChallenges.Clear(); //clear the list because they are now stored elsewhere
		Transform currChallenge; //variable to store the current challenge we are looking at
		foreach (string challengeName in checkpointChallenges) { //go through the list of challenges
			currChallenge = challengesParent.Find(challengeName); //get the transform for the challenge in the menu
			if (currChallenge != null) { //if we found it
				CompleteChallenge(currChallenge.GetComponent<Challenge>()); //call our method to complete it
			}
		}
	}

	private void CompleteChallenge (Challenge challenge) {
		challenges.Add(challenge.Name); //store challenge as complete
		challenge.MarkComplete(); //mark challenge complete
		challengeManager.DisplayNotification(challenge.Name, challenge.NotificationSprite, challenge.NotificationColor);

		for (int i = 0; i < challenge.Rewards.Length; i++) {
			if (challenge.Rewards[i].Contains("currency")) { //if the reward is currency
				string[] rewardInfo = challenge.Rewards[i].Split('_'); //split the info
				int currencyReward; //declare variable
				if (int.TryParse(rewardInfo[1], out currencyReward)) { //try to parse
					currencyEarned += currencyReward; //add to currency earned
				}
			} else { //reward is an unlock
				UnlockItem(challenge.Rewards[i], true);
			}
		}
	}

	public void CompleteCurrentCampaignMission() {
		challengeManager.ClearAllNotifications();

		currGameState = GameState.Menu; //ensure the challenges are completed correctly without going back to the main menu just yet

		if (!difficultyChanged)
			ChallengeActionComplete(selectedCampaignMission + "_difficulty:" + (int)currDifficulty + "_time:" + playTime.ToString() + "_outfit:" + SelectedOutfit + "_weapon:" + SelectedWeapon); //submit action with difficulty
		else
			ChallengeActionComplete(selectedCampaignMission + "_time:" + playTime.ToString() + "_outfit:" + SelectedOutfit + "_weapon:" + SelectedWeapon); //submit without difficulty

		CompleteAllTemporaryChallenges();

		int missionCompleteBonus = 200; //add completion bonus for currency
		if (missions.Contains (selectedCampaignMission)) { //mission previously completed
			missionCompleteBonus = (int)(missionCompleteBonus * 0.05f); //reduce completion bonus
		} else { //not previously completed
			CompleteCampaignMission (selectedCampaignMission, true); //record mission as complete
		}
		currencyEarned += missionCompleteBonus; //add mission complete bonus

		RecordItemsObtainedByPlayer(ref items, true, true); //adds items to remembered items and increases currency earned
		FinalizeCurrencyEarned();

		StartCampaign(); //display campaign screen
		IncrementAdRoundCounter(); //display ad button when necessary
		DisplayMissionReportScreen (true); //show the mission report
		DataPersistence.Save(); //save the game
		Time.timeScale = 0f; //pause the game
	}

	public void CurrencyEarned (int amount) { //to be used by objects within missions -- once player obtains the object for the first time
		currencyEarned += amount; //add the amount the object is worth

		if (currGameState != GameState.Active) {
			FinalizeCurrencyEarned(); //update currency & text that displays currency
		}
	}

	private string CurrencyEarnedText() {
		string currencyEarnedText = currencyEarned.ToString() + " currency";
		if (IAPManager.IAP_Rewards_Purchased) { //if the player purchased premium
			if (!iap_rewards_given) {//if the reward has not already been applied
				currencyEarned *= 2; //double currency earned
				iap_rewards_given = true;
			}
			currencyEarnedText += " (x2 premium bonus!)"; //display bonus was applied
		}

		if (ad_rewards_given) { //if the player watched the post-mission ad
			currencyEarnedText += " (+25% ad bonus!)";
		}
		return currencyEarnedText;
	}

	public void DecrementSelectedSurvivalWave() {
		if (selectedSurvivalWave > 1) { //if we are able to decrement
			selectedSurvivalWave--; //decrement
		} else { //if we are past point of decrement
			selectedSurvivalWave = 1; //reset
		}

		UpdateSurvivalWaveText (); //update display text
	}

	private void DisplayMissionReportScreen(bool missionSuccessful) {
		Text result = missionReportParent.Find ("Result").GetComponent<Text> ();
		Text info = missionReportParent.Find ("Info").GetComponent<Text> ();
		missionReportParent.Find ("Mission Report Retry Button").gameObject.SetActive (!missionSuccessful);

		if (missionSuccessful) {
			result.text = "Mission Complete!";
			result.color = Color.green;

			info.text = "Mission Rating: A+\nCurrency Earned: " + CurrencyEarnedText() + "\nTotal currency: " + currency;

			if (numOfRoundsSinceLastAd == 0 && !ad_rewards_given) {
				info.text += "\n\nWatch an ad using the button below to earn +25% more rewards!";
			}
		} else {
			result.text = "Mission Failure";
			result.color = Color.black;

			info.text = "Mission Rating: F\nCurrency Earned: " + CurrencyEarnedText() + "\nTotal currency: " + currency;
		}

		missionReportParent.gameObject.SetActive (true);
	}

	public void DisplayObjective (string Title, string Body) {
		objectiveText.Display(Title, Body);
	}

	public void EndSurvivalWave(string endReason) {
		currGameState = GameState.Menu; //change the state

		int currentWave = currSurvivalSpawner.CurrentWave; //if player survived -- intermittent mode
		if (endReason.Equals("Player Death")) { //player died, so previous wave was last completed wave
			currentWave = currSurvivalSpawner.PreviousWave;
		}

		if (currSurvivalSpawner.NumberOfWavesCompleted > 0) {
			if (selectedSurvivalWave + currSurvivalSpawner.NumberOfWavesCompleted < currSurvivalSpawner.NumberOfWaves) //if the selected wave is less than we have developed/created for the players
				selectedSurvivalWave += currSurvivalSpawner.NumberOfWavesCompleted; //encourage them to play the next wave
			else //already surpassed max wave
				selectedSurvivalWave = currSurvivalSpawner.NumberOfWaves - 1; //set to last wave

			currencyEarned = 0; //track how much currency we earned

			if (highestSurvivalWave <= currentWave) { //player completed the next available wave
				if (currentWave != 0) {
					if (currentWave % 25 == 0) //first clear of boss wave
						currencyEarned += 25;
					if (currentWave % 100 == 0) //first clear of megaboss wave
						currencyEarned += 75;
				}

				if (!difficultyChanged) { //if the player didn't alter the difficulty
					switch (currDifficulty) {
					case GameDifficulty.Normal://remained on normal entire wave
						currencyEarned += 5;
						break;
					case GameDifficulty.Masochist://remained on masochist entire wave
						currencyEarned += 10;
						break;
					}
				}

				highestSurvivalWave = currentWave + 1; //update highest survival wave completed
			}
			
			int mod_completedWaves = currSurvivalSpawner.NumberOfWavesCompleted; //stores how many waves completed in current bracket -- always 1 if intermittent mode

			if (currSurvivalSpawner.NumberOfWavesCompleted > 25) { //if more than 25 waves were completed
				currencyEarned += 50; //give max num of rewards for 0-25
				mod_completedWaves -= 25; //reduce the base -- a.k.a. how many in next bracket (50)
			}
			if (currSurvivalSpawner.NumberOfWavesCompleted > 50) {
				currencyEarned += 100; //give max num of rewards for 25-50
				mod_completedWaves -= 25;
			}
			if (currSurvivalSpawner.NumberOfWavesCompleted > 75) {
				currencyEarned += 200; //give max num of rewards for 50-75
				mod_completedWaves -= 25;
			}
			if (currSurvivalSpawner.NumberOfWavesCompleted > 100) {
				currencyEarned += 400; //give max num of rewards for 75-100
				mod_completedWaves -= 25;
			}

			if (currentWave < 25) { //give standard rewards based on waves completed
				currencyEarned += 2 * mod_completedWaves;
			} else if (currentWave < 50) {
				currencyEarned += 4 * mod_completedWaves;
			} else if (currentWave < 75) {
				currencyEarned += 8 * mod_completedWaves;
			} else if (currentWave < 100) {
				currencyEarned += 16 * mod_completedWaves;
			} else {
				currencyEarned += 32 * mod_completedWaves;
			}

			//give bonuses if the player survived multiple waves in a row
			if (!currSurvivalSpawner.ContinuousWavesEnabled) { //intermittent mode
				currSurvivalStreak++; //increase their survival streak

				if (currSurvivalStreak > 5) //bonuses for surviving multiple waves in a row -- given 1 at a time
					currencyEarned += 20;
				else if (currSurvivalStreak > 10)
					currencyEarned += 40;
				else if (currSurvivalStreak > 25)
					currencyEarned += 80;
				else if (currSurvivalStreak > 50)
					currencyEarned += 160;
				else if (currSurvivalStreak > 75)
					currencyEarned += 240;
				else if (currSurvivalStreak > 100)
					currencyEarned += 320;
			} else { //continuous mode
				int streak_completedwaves = currSurvivalSpawner.NumberOfWavesCompleted;

				if (streak_completedwaves > 100) { //bonuses given all at once
					currencyEarned += 320 * (streak_completedwaves - 100);
					streak_completedwaves -= 25;
				}
				if (streak_completedwaves > 75) {
					currencyEarned += 240 * (streak_completedwaves - 75);
					streak_completedwaves -= 25;
				}
				if (streak_completedwaves > 50) {
					currencyEarned += 160 * (streak_completedwaves - 50);
					streak_completedwaves -= 25;
				}
				if (streak_completedwaves > 25) {
					currencyEarned += 80 * (streak_completedwaves - 25);
					streak_completedwaves -= 15;
				}
				if (streak_completedwaves > 10) {
					currencyEarned += 40 * (streak_completedwaves - 10);
					streak_completedwaves -= 5;
				}
			}


			//challenge actions for surviving
			challengeManager.ClearAllNotifications();

			if (!difficultyChanged)
				ChallengeActionComplete("Survival_" + currentWave + "_difficulty:" + (int)currDifficulty + "_time:" + playTime.ToString() + "_outfit:" + SelectedOutfit + "_weapon:" + SelectedWeapon + "_winstreak:" + currSurvivalStreak); //submit action with difficulty
			else
				ChallengeActionComplete("Survival_" + currentWave + "_time:" + playTime.ToString() + "_outfit:" + SelectedOutfit + "_weapon:" + SelectedWeapon + "_winstreak:" + currSurvivalStreak); //submit without difficulty

			CompleteAllTemporaryChallenges();

			UpdateSurvivalWaveText (); //update current wave and warning
			UpdateSurvivalCompleteText(); //show player info from last
			
			FinalizeCurrencyEarned();
		} else { //player did not complete a single wave
			displayedSurvivalWaveInfo.text = "Wave " + currSurvivalSpawner.CurrentWave + " lost!\n\n-No currency gained\n-Survival streak reset to 0";
			currSurvivalStreak = 0; //reset survival streak
		}

		menu.ChangeState ("Survival");
		Time.timeScale = 0f; //freeze game
		DataPersistence.Save (); //save the game no matter what
		IncrementAdRoundCounter(); //display ad when necessary
	}

	public void FailCurrentCampaignMission() {
		temporaryChallenges.Clear(); //clear challenges completed prior to reaching a checkpoint
		currencyEarned = 0;
		StartCampaign(); //display campaign screen
		DisplayMissionReportScreen (false); //show the mission report
		Time.timeScale = 0f;
		IncrementAdRoundCounter(); //display an ad when necessary
	}

	private void FinalizeCurrencyEarned() { //adds currency earned to total & updates all text that displays current amount of currency
		currency += currencyEarned;

		//update all text that displays currency
		string textToDisplay = currency.ToString();
		currencyText_Survival.text = textToDisplay;
		currencyText_Unlocks.text = textToDisplay;
	}

	void FixedUpdate() {
		if (currGameState == GameState.Active) {
			playTime += Time.fixedDeltaTime;
		}
	}

	public void HideNonRelevantWeaponUnlocks() {
		foreach (Transform unlocks_weaponSpec in unlocks_weaponsParent) {
			foreach (string weaponSpec in selectedOutfit_weaponSpecializations) {
				if (weaponSpec.Contains(unlocks_weaponSpec.name)) {
					unlocks_weaponSpec.gameObject.SetActive(true);
				}
				else {
					unlocks_weaponSpec.gameObject.SetActive(false);
				}
			}
		}
		ResizeHorizontalLayoutGroup(unlocks_weaponsParent.GetComponent<RectTransform>());
	}

	private void IncrementAdRoundCounter() {
		if (!IAPManager.IAP_Rewards_Purchased) {
			if (currGameMode == GameMode.Campaign) { //campaign missions will take the player longer to play through
				numOfRoundsSinceLastAd += 1f; //add a higher weight
			} else { //survival
				numOfRoundsSinceLastAd += 0.6f; //play 5 rounds before an add is displayed
			}

			if (numOfRoundsSinceLastAd >= 3) { //if player played enough rounds
				//show opt-in button for ads/extra rewards
				if (currGameMode == GameMode.Campaign) {
					adManager.ShowButton(true);
				} else {
					adManager.ShowButton(false);
				}
				numOfRoundsSinceLastAd = 0; //reset counter
			}
		}
	}

	public void IncrementSelectedSurvivalWave() {
		if (selectedSurvivalWave > highestSurvivalWave) { //if the selected wave is higher than possible
			selectedSurvivalWave = highestSurvivalWave + 1; //set it to the next available wave
		} else if (selectedSurvivalWave < currSurvivalSpawner.NumberOfWaves) { //prevent from incrementing past the amount of waves in survival spawner
			selectedSurvivalWave++;
		}

		UpdateSurvivalWaveText ();
	}

	private IEnumerator LoadLevel () {
		menu.ChangeState ("");
		loadingScreen.color = Color.white; //display load screen
		loadingScreen.raycastTarget = true;
		currGameState = GameState.Loading;

		difficultyChanged = false; //show the player has not changed the difficulty yet during this mission
		currencyEarned = 0; //show the player has not earned any currency yet for this mission
		iap_rewards_given = false;
		ad_rewards_given = false;

		ClearAllCharacters (); //clear all remaining enemies

		bool backgroundIsMissing = bgParent.childCount > 0 ? !bgParent.GetChild (0).tag.Equals ("Level") : true;
		bool bgNeedsToBeInstantiated = false;
		string bgFilePath;
		string bgmFilePath;
		GameState nextGameState;

		if (currGameMode == GameMode.Survival) {
			bgFilePath = "Survival_" + ((selectedSurvivalWave - 1) / 25);
				
			if (backgroundIsMissing || !bgParent.GetChild (0).name.Contains (bgFilePath) || //background is not what it needs to be
				(selectedSurvivalWave - 1) / 25 !=  currSurvivalSpawner.PreviousWave / 25) {//player just started next set of 25 waves
				bgNeedsToBeInstantiated = true;
			}

			if (!audioManager.CurrentSongName.Equals(bgFilePath)) { //if the desired survival song doesn't match
				bgmFilePath = "Audio/Music/" + bgFilePath; //get the filepath for the next song
				audioManager.TransitionBackgroundMusic(Resources.Load<AudioClip>(bgmFilePath));
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
			checkpointChallenges.Clear();
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

		playTime = 0f; //reset playtime;
		Time.timeScale = 1f; //return time to normal -- needs to be normal to wait for seconds

		yield return new WaitForSeconds (1); //timescale needs to be >0 to work

		loadingScreen.transform.SetAsLastSibling (); //ensure load screen is still the last child (covers everything)
		loadingScreen.color = Color.clear; //hide load screen
		loadingScreen.raycastTarget = false;

		currGameState = nextGameState;

		if (currGameMode == GameMode.Survival) {
			DisplayObjective("Survival Wave " + currSurvivalSpawner.CurrentWave.ToString(), "Defeat all enemies to advance!");
		}
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
		audioManager.StopBackgroundMusic(); //stop bgm
	}

	public void PostMissionAdComplete(bool finished) {
		if (finished) {
			if (!ad_rewards_given) {
				currencyEarned = (int)(currencyEarned * 1.25f);
				ad_rewards_given = true;
			}
			if (currGameMode == GameMode.Campaign) {
				DisplayMissionReportScreen(true); //Ads can only be displayed when the mission is successful
			} else {
				UpdateSurvivalCompleteText();
			}
		}
	}

	public void PurchaseSelectedItem() { //change to return string feedback at some point
		UnlockItem (purchase_selectedItemName, false); //unlock the item
		if (purchase_selectedItemCost > 0)
			currency -= purchase_selectedItemCost; //update money
		DataPersistence.Save (); //save the changes

		if (purchase_selectedItemType.Equals ("Outfit")) {
			SelectOutfit (purchase_selectedItemName);
		} else { //item type will equal the weapon specialization
			SelectWeapon (purchase_selectedItemName);
		}
	}

	public void QuitApplication() {
		Application.Quit();
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
				weapon.sizeDelta = new Vector2 (weapon.rect.height, weapon.rect.height);
			}
			ResizeHorizontalLayoutGroup (weaponSpecialization); //ensure each weaponspec has enough space
		}
		HideNonRelevantWeaponUnlocks(); //only show the relevant weapons

		//challenges
		Challenge challenge;
		foreach (RectTransform challengeRectTransform in challengesParent) { //each child object in challengeParent should have a Challenge script
			challenge = challengeRectTransform.GetComponent<Challenge>();
			if (challenges.Contains(challenge.Name)) { //see if the challenge has been completed already
				challenge.MarkComplete(); //mark it as complete
			}
			challengeRectTransform.sizeDelta = new Vector2(challengeRectTransform.rect.width, Screen.height * 0.4f); //keep same width, adjust height to scale with screen dimensions
		}
		ResizeVerticalLayoutGroup(challengesParent.GetComponent<RectTransform>());

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

	public void ResizeVerticalLayoutGroup (RectTransform parent) {
		Vector2 newSizeDelta = Vector2.zero; //start with value of 0
		float spacing = parent.GetComponent<VerticalLayoutGroup>().spacing;
		foreach (RectTransform child in parent) { //go through all children of rect transform
			if (child.gameObject.activeSelf) { //if the child is active/shown
				newSizeDelta.y += child.sizeDelta.y + spacing; //add to the width
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

	public void SelectCampaignMission(Transform CampaignMissionButton) {
		Transform lockChild = CampaignMissionButton.Find("Lock"); //get the lock

		if (lockChild.gameObject.activeSelf) { //if the lock is active
			//player is unable to play the level -- inform them they cannot play?
			return;
		}
		getDefaultPlayerSpawnLocation = true; //ensure we get the default spawn location for the level
		selectedCampaignMission = CampaignMissionButton.name; //set this as the selected mission
		selectedCampaignMissionObjective = CampaignMissionButton.Find("Campaign Mission Objective").GetComponent<Text>().text;
		StartCoroutine (LoadLevel ()); //load the level
	}

	public void SelectOutfit (string OutfitName) {
		string textToDisplay;

		Player p = (Instantiate (Resources.Load ("Characters/Player/" + OutfitName)) as GameObject).GetComponent<Player> (); //spawn prefab to get the info from script
		textToDisplay = OutfitName + "\nHP: " + p.MaxHP + "     Move Speed: " + p.MovementSpeed + " m/s"; //set outfit info text

		if (unlocks.Contains (OutfitName)) { //outfit is unlocked
			displayPurchaseConfirmationButton_Outfit.gameObject.SetActive (false); //hide unlock button

			if (selectedOutfit_weaponSpecializations == null ||
				!selectedOutfit_weaponSpecializations.Equals(p.weaponSpecialization)) { //if a different weapon spec
				selectedOutfit_weaponSpecializations = p.weaponSpecialization; //update current weapon spec
				HideNonRelevantWeaponUnlocks(); //ensures the right weapons are displayed
				SelectWeapon(p.DefaultWeapon); //ensures the player always has the correct type of weapon equipped
			}

			unlocks_outfitsParent.Find(selectedOutfit).Find("Checkmark").gameObject.SetActive(false); //hide checkmark from previously selected outfit
			unlocks_outfitsParent.Find(OutfitName).Find("Checkmark").gameObject.SetActive(true); //show the checkmark for currently selected outfit
			selectedOutfit = OutfitName; //set this as current outfit to spawn as
		} else { //outfit not unlocked
			purchase_selectedItemName = OutfitName; //remember which outfit we just clicked on
			purchase_selectedItemType = "Outfit";
			purchase_selectedItemCost = p.Unlock_Cost; //remember the cost
			purchase_selectedItemChallenge = p.Unlock_Challenge;
			purchase_selectedItemColor = p.SpriteColor;

			bool interactable = true;

			if (purchase_selectedItemChallenge.Length > 0) {//outfit is only available after completing a challenge
				textToDisplay += "\nChallenge: " + purchase_selectedItemChallenge; //display which challenge must be completed

				if (!challenges.Contains(purchase_selectedItemChallenge)) {
					interactable = false;
				}
			}
			if (purchase_selectedItemCost > -1) { //if the outfit is for sale
				textToDisplay += "\nCost: " + purchase_selectedItemCost; //display the cost
				
				if (currency < purchase_selectedItemCost) {
					interactable = false;
				}
			}
			
			displayPurchaseConfirmationButton_Outfit.gameObject.SetActive(true); //display the unlock button
			displayPurchaseConfirmationButton_Outfit.interactable = interactable; //set whether it is interactable or not
		}
		p.Die (); //destroy the player we created temporarily

		displayedSelectedOutfitInfo.text = textToDisplay; //update the displayed text
	}

	public void SelectWeapon(string WeaponName) {
		Weapon w = (Instantiate (Resources.Load ("Weapons/" + WeaponName)) as GameObject).GetComponent<Weapon> (); //spawn prefab to get the info from script
		displayedSelectedWeaponInfo.text = WeaponName + "\nDamage: " + w.Damage + "\nType: " + w.Specialization; //update info

		if (unlocks.Contains (WeaponName)) { // if the weapon is unlocked
			for (int i = 0; i < selectedOutfit_weaponSpecializations.Count; i++) { //go thru all weapon specs
				if (selectedOutfit_weaponSpecializations[i].Contains(w.Specialization)) { //if the weapon can be used with the current outfit
					displayedSelectedWeaponInfo.color = Color.black; //ensure the text is black
					displayPurchaseConfirmationButton_Weapon.gameObject.SetActive(false); //hide purchase/unlock button

					unlocks_weaponsParent.Find(selectedWeaponSpecialization).Find(selectedWeapon).Find("Checkmark").gameObject.SetActive(false); //hide checkmark from previously selected weapon
					unlocks_weaponsParent.Find(w.Specialization).Find(WeaponName).Find("Checkmark").gameObject.SetActive(true); //show the checkmark for currently selected weapon
					selectedWeapon = WeaponName; //update the selected weapon
					selectedWeaponSpecialization = w.Specialization;
				}
				else {
					displayedSelectedWeaponInfo.text += "\nThis weapon cannot be used with the current outfit.";
				}
			}
		} else { //weapon not unlocked
			purchase_selectedItemName = WeaponName;
			purchase_selectedItemType = w.Specialization;
			purchase_selectedItemCost = w.Unlock_Cost;
			purchase_selectedItemChallenge = w.Unlock_Challenge;
			purchase_selectedItemColor = w.SpriteColor;


			if (purchase_selectedItemChallenge.Length > 0) {
				displayedSelectedWeaponInfo.text += "\nChallenge needed:" + purchase_selectedItemChallenge;
			}
			if (purchase_selectedItemCost > -1) {
				//displayedSelectedWeaponInfo.color = Color.red; //inform the player visually the item is locked
				displayedSelectedWeaponInfo.text += "\nCost: " + purchase_selectedItemCost;
				displayPurchaseConfirmationButton_Weapon.gameObject.SetActive(true); //display purchase/unlock button

				if (currency < purchase_selectedItemCost) {
					displayPurchaseConfirmationButton_Weapon.interactable = false;
				} else {
					displayPurchaseConfirmationButton_Weapon.interactable = true;
				}
			}
			
		}

		Destroy (w.gameObject);
	}

	public void SetBGM(AudioClip music) {
		audioManager.TransitionBackgroundMusic(music);
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
		int temp;
		if (int.TryParse(inputField.text, out temp)) { //see if input is valid
			selectedSurvivalWave = temp; //if valid, set the wave
		} else {
			selectedSurvivalWave = 1; //if not valid, set to wave 1
		}
		
		if (selectedSurvivalWave > highestSurvivalWave + 1)
			selectedSurvivalWave = highestSurvivalWave + 1;
		else if (selectedSurvivalWave <= 0)
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
		if (missions.Contains(selectedCampaignMission) || //if the player has already beaten the current mission
			(currCutscene.gameObject.name.ToLower().Contains("start") && !getDefaultPlayerSpawnLocation) || //or the player is continuing from a checkpoint and the current cutscene is the start cutscene
			currCutscene.AlwaysSkippable) { //or the cutscene is always skippable
			currCutscene.EndCutscene (); //allow them to skip the cutscene
		}
	}

	private void SpawnPlayer() {
		Instantiate(Resources.Load("Characters/Player/" + selectedOutfit), currPlayerSpawnLocation, Quaternion.Euler(Vector3.zero));
		Weapon w = (Instantiate (Resources.Load ("Weapons/" + selectedWeapon)) as GameObject).GetComponent<Weapon> (); //spawn selected weapon
		Character.player.Wield(w);
		currCameraManager.Follow(Character.player.transform);
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
			continuousWaveToggle = GameObject.Find("Continuous Wave Toggle").GetComponent<Toggle>();
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
			challenges = new List<string>();
			checkpointChallenges = new List<string>();
			temporaryChallenges = new List<string>();
			missions = new List<string> ();
			items = new List<string> ();
			checkpointItems = new List<string> ();
			extra01 = new List<string>();
			extra02 = new List<string>();

			if (loadedData != null) {
				currency = loadedData.currency;
				highestSurvivalWave = loadedData.highestSurvivalWave;
				currSurvivalStreak = loadedData.survivalStreak;
				unlocks.AddRange (loadedData.unlocks);
				challenges.AddRange(loadedData.challenges);//challenges
				missions.AddRange (loadedData.missions);
				items.AddRange (loadedData.items);
				extra01.AddRange(loadedData.extra01);
				extra02.AddRange(loadedData.extra02);
			} else {
				highestSurvivalWave = 0;
				currSurvivalStreak = 0;
				currency = 0;
				unlocks.Add ("Stick it to 'em"); //default player
				unlocks.Add ("Iron Longsword"); //default weapon
			}

			SetDifficulty ((int)currDifficulty); //set the current difficulty to loaded/preset value
			sfxSlider.value = SFXVolume; //adjust sfx bar to loaded/preset value
			bgmSlider.value = BGMVolume; //adjust bgm bar to loaded/preset value
			soundToggle.isOn = SoundEnabled; //set soundtoggle

			selectedSurvivalWave = 1;

			currPlayerSpawnLocation = Vector3.zero;

			unlocks_outfitsParent = GameObject.Find("Outfit Layout Group").transform; //get the transform parent
			unlocks_weaponsParent = GameObject.Find("Weapon Layout Group").transform; //get the transform parent

			challengesParent = GameObject.Find("Challenges Layout Group").transform; //get the transform parent
			challengesParent.parent.parent.parent.gameObject.SetActive(false);

			displayedSelectedWeaponInfo = GameObject.Find ("Selected Weapon Text").GetComponent<Text>(); //get the text object for weapon info

			displayPurchaseConfirmationButton_Weapon = GameObject.Find ("Purchase Weapon Button").GetComponent<Button>();
			displayPurchaseConfirmationButton_Weapon.gameObject.SetActive (false);

			unlocks_weaponsParent.parent.parent.parent.gameObject.SetActive (false);

			campaignMissionsParent = GameObject.Find ("Missions Layout Group").transform;
			campaignTabsParent = GameObject.Find ("Campaign Tab Container").transform;
			missionReportParent = GameObject.Find ("Mission Report").transform;
			missionReportParent.gameObject.SetActive (false);

			displayedSurvivalWaveNumber = GameObject.Find ("Selected Wave Number Input Field").GetComponent<InputField> ();
			displayedSurvivalWaveInfo = GameObject.Find ("Survival Description Text").GetComponent<Text> ();
			displayedSelectedOutfitInfo = GameObject.Find ("Selected Outfit Text").GetComponent<Text> ();

			purchaseConfirmationText = GameObject.Find ("Purchase Confirmation Text").GetComponent<Text>();
			purchaseConfirmationText.transform.parent.gameObject.SetActive (false);

			displayPurchaseConfirmationButton_Outfit = GameObject.Find ("Purchase Outfit Button").GetComponent<Button>();
			displayPurchaseConfirmationButton_Outfit.gameObject.SetActive (false);

			challengeManager = GameObject.Find("Challenge Notification Panel").GetComponent<ChallengeNotificationManager>();
			adManager = GetComponent<AdvertisementManager>();

			audioManager = GetComponent<AudioManager>();

			currencyText_Survival = GameObject.Find("Survival Currency Text").GetComponent<Text>();
			currencyText_Unlocks = GameObject.Find("Unlocks Currency Text").GetComponent<Text>();

			objectiveText = GameObject.Find("Objective Text").GetComponent<TextFade>();

			FinalizeCurrencyEarned();

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

		currSurvivalSpawner.ContinuousWavesEnabled = continuousWaveToggle.isOn;
		currGameMode = GameMode.Survival;
		currGameState = GameState.Menu;
		UpdateSurvivalWaveText ();
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
		audioManager.StartBackgroundMusic(); // play bgm
	}

	public void StopCutscene (Cutscene c, bool completeMission) {
		StopCutscene (c);

		if (completeMission) {
			CompleteCurrentCampaignMission (); //mark mission as complete
		} else if (c.gameObject.name.ToLower().Contains("start")) {
			DisplayObjective(selectedCampaignMission.Replace("_", ": "), selectedCampaignMissionObjective);
		}
	}

	public void ToggleSoundEnabled(Toggle UIToggle) {
		SoundEnabled = UIToggle.isOn; //enable/disable the sound using the toggle on the screen
	}

	public void ToggleSurvivalContinuousMode(Toggle UIToggle) {
		currSurvivalSpawner.ContinuousWavesEnabled = UIToggle.isOn; //enable/disable continuous wave mode using the toggle on the screen
	}

	public void UnlockItem(string itemName, bool colorGrabNeeded) {
		Transform temp = unlocks_outfitsParent.Find (itemName); //find the outfit
		bool unlockedAnOutfit = true;
		if (temp == null) { //if it wasn't found, it was a weapon
			for (int i = 0; i < unlocks_weaponsParent.childCount; i++) { //go through all weapon specs
				temp = unlocks_weaponsParent.GetChild(i).Find(itemName); //attempt to find the weapon

				if (temp != null) { //weapon was found
					break; //no need to continue
				}
			}

			if (temp == null) { //weapon was not found
				Debug.Log("Weapon not found: " + itemName);
				return;
			}
			unlockedAnOutfit = false;
		}

		Color tempColor = purchase_selectedItemColor; //by default, player will be making purchases more often
		if (unlockedAnOutfit) { //for outfits
			Player p = (Instantiate(Resources.Load("Characters/Player/" + itemName)) as GameObject).GetComponent<Player>(); //load the player

			if (colorGrabNeeded)
				tempColor = p.SpriteColor; //get the color from player

			if (!unlocks.Contains(p.DefaultWeapon.ToString())) { //check to see if the default weapon is unlocked already
				UnlockItem(p.DefaultWeapon.ToString(), true); //automatically unlock the default weapon if it isn't already
			}
			
			Destroy(p.gameObject); //destroy the clone
		} else if (colorGrabNeeded) { //for weapons if unlocked via challenge -- color not automatically found
			Weapon w = (Instantiate(Resources.Load("Weapons/" + itemName)) as GameObject).GetComponent<Weapon>(); //load the weapon
			tempColor = w.SpriteColor; //get the color from weapon
			Destroy(w.gameObject); //destroy the clone
		}

		temp.Find("Sprite").GetComponent<Image>().color = tempColor;
		temp.Find("Lock").gameObject.SetActive(false);
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

	private void UpdateSurvivalWaveText() {
		displayedSurvivalWaveNumber.text = selectedSurvivalWave.ToString ();
	}
	
	private void UpdateSurvivalCompleteText() {
		if (currSurvivalSpawner.ContinuousWavesEnabled)
			displayedSurvivalWaveInfo.text = currSurvivalSpawner.NumberOfWavesCompleted + " waves completed!\n\n+" + CurrencyEarnedText();
		else
			displayedSurvivalWaveInfo.text = "Wave " + currSurvivalSpawner.CurrentWave + " complete!\n\n+" + CurrencyEarnedText() + "\n+1 survival streak (" + currSurvivalStreak + ")"; //inform the player how much they earned

		if (numOfRoundsSinceLastAd == 0 && !ad_rewards_given) {
			displayedSurvivalWaveInfo.text += "\n\nWatch an ad using the button below to earn +25% more rewards!";
		}
	}
}