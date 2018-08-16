using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChallengeNotificationManager : MonoBehaviour {
	private static float moveSpeed = 2f;

	private RectTransform rectTransform;
	private Image image;
    private Sprite defaultSprite;
	private Text text;
	[SerializeField]
	private float displayTime;
	private float currDisplayTime;
	private List<Notification> notifications;

    public void ClearAllNotifications() { //to be called when the level/mission is complete -- prevent old notifications from continuing to appear
        gameObject.SetActive(false); //hide panel
        notifications.Clear(); //clear list
        currDisplayTime = displayTime; //reset display time
        rectTransform.anchoredPosition = new Vector2(0, 0); //reset position
    }

	public void DisplayNotification(string text, Sprite sprite, Color spriteColor) {
        Notification newNotification = new Notification(text, sprite, spriteColor);
        if (!notifications.Contains(newNotification)) { //notifications doesn't already have this notification
            notifications.Add(newNotification);

            if (notifications.Count == 1) { //if we just added first notification
                RefreshNotificationPanel(); //refresh panel
                gameObject.SetActive(true); //show panel
            }
        }
	}

	void Update() {
		if (notifications.Count > 0) { //we have a notification to display
			if (!notifications[0].HasBeenDisplayed) { //if the notification has not already been displayed
				if (rectTransform.anchoredPosition.y < rectTransform.sizeDelta.y) { //if the panel isn't at the right spot yet
					rectTransform.anchoredPosition += new Vector2(0, moveSpeed);

					if (rectTransform.anchoredPosition.y > rectTransform.sizeDelta.y) { //if the panel went too far
						rectTransform.anchoredPosition = new Vector2(0, rectTransform.sizeDelta.y); //set it at the right spot
					}
				} else if (currDisplayTime > 0) { //The notification is not moving, currently being shown to the player
					currDisplayTime -= Time.fixedDeltaTime; //decrement display time
				} else { //notification displayed long enough
					notifications[0].HasBeenDisplayed = true;
				}
			} else { // notification has already been displayed, move notification downwards out of view
				if (rectTransform.anchoredPosition.y > 0) { //if the panel isn't at the right spot yet
					rectTransform.anchoredPosition -= new Vector2(0, moveSpeed); //update position

					if (rectTransform.anchoredPosition.y < 0) { //if the panel went too far
						rectTransform.anchoredPosition = new Vector2(0, 0); //set it at the right spot
					}
				} else { //panel is at the right spot
					notifications.RemoveAt(0); //remove the current notification
                    RefreshNotificationPanel(); //refresh panel
				}
			}
        }
	}

	private void RefreshNotificationPanel() {
		if (notifications.Count > 0) { //if there is a notification to display
            if (notifications[0].Sprite != null) { //if there is a sprite to change to
                image.sprite = notifications[0].Sprite; //set the sprite
            } else { //no sprite provided
                image.sprite = defaultSprite; //set default sprite
            }

            image.color = notifications[0].Color; //set the provided color
			text.text = notifications[0].Text; //set the provided text

			currDisplayTime = displayTime; //reset display time
		} else { //no notification to display
            gameObject.SetActive(false); //hide the game object
        }
	}

	void Start () {
		rectTransform = GetComponent<RectTransform>(); //get reference to rect transform
		rectTransform.sizeDelta = new Vector2(0, Screen.height * 0.15f); //set the height
		rectTransform.anchoredPosition = new Vector2(0, 0); //reset the position

		image = transform.Find("Challenge Notification Image").GetComponent<Image>(); //get image child
        defaultSprite = image.sprite; //get default sprite incase the notification doesn't provide one

		text = transform.Find("Challenge Notification Info").GetComponent<Text>(); //get the text child

		currDisplayTime = displayTime; //initialize the display time
		notifications = new List<Notification>(); //initialize the list

        gameObject.SetActive(false); //hide the panel
	}
}
