//Written by Justin Ortiz

using UnityEngine;
using UnityEngine.Advertisements;

public class AdvertisementManager : MonoBehaviour {
	private static bool postMissionAd;
	private const int optInCurrencyAmount_finished = 15;
	private const int optInCurrencyAmount_skipped = 5;

	private GameObject button_campaign_ad;
	private GameObject button_survival_ad;

	/// <summary>
	/// Displays a full-screen advertisement to the player.
	/// </summary>
	/// <param name="PostMissionAd">Is this advertisement a post-mission ad(true) or an opt-in ad from the menu(false)?</param>
	public void DisplayAd(bool PostMissionAd) {
		if (Advertisement.IsReady()) {
			ShowOptions options = new ShowOptions { resultCallback = HandleAdvertisementCallback };
			Advertisement.Show(options);
			postMissionAd = PostMissionAd;
		}
	}

	private void HandleAdvertisementCallback(ShowResult result) {
		if (!postMissionAd) {
			if (result == ShowResult.Finished) { //the ad was completely watched
				GameManager_SwordSwipe.currGameManager.CurrencyEarned(optInCurrencyAmount_finished);
			} else if (result == ShowResult.Skipped && IAPManager.IAP_Rewards_Purchased) { //the ad was skipped and the user has premium
				GameManager_SwordSwipe.currGameManager.CurrencyEarned(optInCurrencyAmount_skipped);
			}
		} else {
			if (result == ShowResult.Finished) {
				GameManager_SwordSwipe.currGameManager.PostMissionAdComplete(true); //let the game manager know the ad is complete and reward the player
			}
			
		}

		button_campaign_ad.SetActive(false);
		button_survival_ad.SetActive(false);
	}

	public void ShowButton(bool CampaignButton) {
		if (CampaignButton) {
			button_campaign_ad.SetActive(true);
		} else {
			button_survival_ad.SetActive(true);
		}
	}

	void Start() {
		button_campaign_ad = GameObject.Find("Post-Mission Ad Button");
		button_survival_ad = GameObject.Find("Post-Survival Ad Button");

		button_campaign_ad.SetActive(false);
		button_survival_ad.SetActive(false);
	}
}
