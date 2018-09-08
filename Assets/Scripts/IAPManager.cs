//Written by Justin Ortiz

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour, IStoreListener {
    private static bool bool_iap_purchased_rewards;
    private const string id_product_rewards = "premium_rewards";

    private IStoreController storeController;
    private Text text_iap_feedback;
    private GameObject button_iap_rewards;
    private GameObject panel_iap_purchased_rewards;
    private GameObject panel_iap_feedback;

    public static bool IAP_Rewards_Purchased { get { return bool_iap_purchased_rewards; } }

    public void OnInitialized(IStoreController controller, IExtensionProvider provider) {
        Product product_NoAds = controller.products.WithID(id_product_rewards); //get the noads product
        if (product_NoAds.hasReceipt) { //if it was purchased already
            PurchaseProduct(product_NoAds); //mark the product as purchased
        } else { //otherwise
            bool_iap_purchased_rewards = false; //mark it as false
            panel_iap_purchased_rewards.SetActive(false); //hide purchased panel
        }

        storeController = controller;
    }

    public void OnInitializeFailed(InitializationFailureReason error) {
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
            default:
                outputText += "Unable to connect to the play store. Please restart the application.";
                break;
        }

        UpdateFeedbackText(outputText);
    }

    public void OnPurchaseFailed(Product item, PurchaseFailureReason r) { //in case the purchase fails -- called by IAP Button
        string outputText = "The transaction has failed due to the following reason: ";
        if (item.definition.id.Equals(id_product_rewards)) { //if it is the right product
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
                case PurchaseFailureReason.DuplicateTransaction:
                    PurchaseProduct(item); //mark the product as purchased
                    return; //return without displaying error
                default:
                    outputText += "'Purchase Error 0'.";
                    break;
            }
        } else {
            outputText += item.definition.id;
        }

        UpdateFeedbackText(outputText);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e) {
        PurchaseProduct(e.purchasedProduct);
        return PurchaseProcessingResult.Complete; // complete
    }

    public void PurchaseProduct(Product product) { //for codeless IAP
        switch (product.definition.id) {
            case id_product_rewards:
                bool_iap_purchased_rewards = true; //ensure gameManager knows it was purchased
                button_iap_rewards.SetActive(false); //hide the button so they are unable to purchase again
                panel_iap_purchased_rewards.SetActive(true); //show the purchased panel

                if (panel_iap_feedback.activeSelf) //if feedback panel displayed
                    panel_iap_feedback.SetActive(false); //hide feedback panel if it is displayed
                break;
        }
    }

    public void PurchaseProduct(string productID) {
        Product product = storeController.products.WithID(productID); //attempt to get product
        if (product != null) { //product was found
            if (product.availableToPurchase) { //if they can buy it
                storeController.InitiatePurchase(product); //initiate purchase
            } else if (product.hasReceipt) { //if they have already purchased it
                PurchaseProduct(product); //mark it as purchased
            }
        } else {
            //Debug.Log(productID + " not found in product catalog.");
        }
    }

    void Start() {
        text_iap_feedback = GameObject.Find("IAP Feedback Text").GetComponent<Text>(); //get the text object to display error output
        button_iap_rewards = GameObject.Find("Premium Purchase Button"); //button to initiate purchase for noads
        panel_iap_purchased_rewards = GameObject.Find("Premium Purchased Panel"); //panel to display the player already purchased noads
        panel_iap_feedback = GameObject.Find("IAP Feedback Panel"); //panel for displaying iap error feedback

        panel_iap_feedback.SetActive(false); //hide feedback panel

        ConfigurationBuilder cb = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance()); //used to define products
        cb.AddProduct(id_product_rewards, ProductType.NonConsumable); //add the noads product

        UnityPurchasing.Initialize(this, cb); //initialize istorelistener
    }

    private void UpdateFeedbackText(string textToDisplay) {
        text_iap_feedback.text = textToDisplay; //set the text
        panel_iap_feedback.SetActive(true); //display feedback panel
    }
}
