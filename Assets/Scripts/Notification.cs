using UnityEngine;

public class Notification {

    private string text; //attributes
    private Sprite sprite;
    private Color color;
    private bool displayed;

    public string Text { get { return text; } } //properties
    public Sprite Sprite { get { return sprite; } }
    public Color Color { get { return color; } }
    public bool HasBeenDisplayed { get { return displayed; } set { displayed = value; } }

    public override bool Equals(object obj) {
        if (obj.GetType() != typeof(Notification)) { //if the other object is not a notification
            return false;
        } else { //if other object is a notification
            Notification otherNotification = (Notification)obj; //cast object to notification object
            if (!text.Equals(otherNotification.text)) { //if text doesn't match
                return false;
            }

            if (sprite != null) { //if there is a sprite for this notification
                if (otherNotification.sprite != null) { //if there is a sprite for other notification
                    if (!sprite.Equals(otherNotification.sprite)) { //if the sprites don't match
                        return false;
                    }
                } else { //one had a sprite and the other did not
                    return false;
                }
            }

            if (!color.Equals(otherNotification.color)) { //if color doesn't match
                return false;
            }
        }
        return true; //everything matched
    }

    public override int GetHashCode() {
        return base.GetHashCode();
    }

    public Notification(string Text, Sprite Sprite, Color SpriteColor) { //constructor
        text = Text; //initialize attributes
        sprite = Sprite;
        color = SpriteColor;
        displayed = false;
    }

    public override string ToString() {
        return text;
    }
}
