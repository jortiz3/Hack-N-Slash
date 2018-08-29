//Written by Justin Ortiz

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using UnityEngine.Purchasing;

public class AdvertisementManager : MonoBehaviour {
    private static bool optIn;
    private static bool premium_NoAds;
    private static int optInCurrencyAmount = 15;
    private static int defaultCurrencyAmount = 5;

    private Text IAP_FeedbackText;

    public static bool Premium_NoAds { get { return premium_NoAds; } }

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

                GameManager_SwordSwipe.currGameManager.CurrencyEarned(currencyReward);
				break;
            default: //failed or skipped
                //do nothing at this point in time -- maybe display a message pop-up?
                break;
		}
	}

    /*public void OnInitializeFailed(InitializationFailureReason error) {
        string outputText = "Unable to initialize in-app purchases due to the following reason: ";
        switch (error) {
            case InitializationFailureReason.AppNotKnown:
                outputText += "The store reported the app as unknown.";
                break;
            case InitializationFailureReason.NoProductsAvailable:
                outputText += "No products available for purchase.";
                break;
            case InitializationFailureReason.PurchasingUnavailable:
                outputText += "In-App purchases disabled in device settings.";
                break;
        }

        UpdateFeedbackText(outputText);
    }*/

    public void OnPurchaseFailed(Product item, PurchaseFailureReason r) { //in case the purchase fails -- called by IAP Button
        string outputText = "The transaction has failed due to the following reason: ";
        if (item.definition.id.Equals("premium_noads")) { //if it is the right product
            switch (r) {
                case PurchaseFailureReason.PaymentDeclined:
                    outputText += "'Payment Declined'.\n\nPlease try again.";
                    break;
                case PurchaseFailureReason.PurchasingUnavailable:
                    outputText += "'Purchasing Unavailable'.\n\nPlease try again later.";
                    break;
                case PurchaseFailureReason.SignatureInvalid:
                    outputText += "'Signature Invalid'.\n\nPlease try again.";
                    break;
                case PurchaseFailureReason.UserCancelled:
                    outputText += "'User Cancelled'.";
                    break;
                case PurchaseFailureReason.ExistingPurchasePending:
                    outputText += "'Existing Purchase Pending'.";
                    break;
                case PurchaseFailureReason.ProductUnavailable:
                    outputText += "'Product Unavailable'.\n\nPlease try again later.";
                    break;
                case PurchaseFailureReason.Unknown:
                    outputText += "'Purchase Error 4094'.";
                    break;
                default:
                    outputText += "'Purchase Error 0'.";
                    break;
            }
        } else {
            outputText += item.definition.id;
        }

        UpdateFeedbackText(outputText);
    }

    /*public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e) {
        if (e.purchasedProduct.definition.id.Equals("premium_NoAds")) { //if it is the right product
            premium_NoAds = true; //remove ads
        }
        return PurchaseProcessingResult.Complete; // complete
    } */

    public void PurchaseProduct(Product product) { //for codeless IAP
        switch (product.definition.id) {
            case "premium_noads":
                premium_NoAds = true;
                break;
        }
    }

    void Start() {
        IAP_FeedbackText = GameObject.Find("IAP Feedback Text").GetComponent<Text>(); //get the text object to display error output
        IAP_FeedbackText.transform.parent.gameObject.SetActive(false);
        /*Product product_NoAds = controller.products.WithID("premium_NoAds"); //get the product
        if (product_NoAds != null && product_NoAds.hasReceipt) { //if it was purchased already
            premium_NoAds = true; //mark it as true
        } else { //otherwise
            premium_NoAds = false; //mark it as false
        }*/
    }

    private void UpdateFeedbackText(string textToDisplay) {
        IAP_FeedbackText.text = textToDisplay; //set the text
        IAP_FeedbackText.transform.parent.gameObject.SetActive(true); //show the object -- may need to modify later
    }
}
