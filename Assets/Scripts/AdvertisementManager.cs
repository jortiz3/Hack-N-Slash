//Written by Justin Ortiz

using UnityEngine;
using UnityEngine.Advertisements;

public class AdvertisementManager : MonoBehaviour {
    private static bool optIn;
    private static int optInCurrencyAmount = 15;
    private static int defaultCurrencyAmount = 5;

	public void DisplayAd(bool OptIn) {
		if (Advertisement.IsReady()) {
			ShowOptions options = new ShowOptions { resultCallback = HandleAdvertisementCallback };
			Advertisement.Show(options);
            optIn = OptIn;
		}
		
	}

	private void HandleAdvertisementCallback(ShowResult result) {
		switch (result) {
			case ShowResult.Finished: //player watched entire ad
                int currencyReward = defaultCurrencyAmount;
				if (optIn) {
                    currencyReward = optInCurrencyAmount;
                }

                GameManager.currGameManager.CurrencyEarned(currencyReward);
				break;
            default: //failed or skipped
                //do nothing at this point in time -- maybe display a message pop-up?
                break;
		}
	}
}
