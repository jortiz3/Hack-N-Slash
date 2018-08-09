using UnityEngine;

public class Notification {

    private string text;
    private Sprite sprite;
    private Color color;
    private bool displayed;

    public string Text { get { return text; } }
    public Sprite Sprite { get { return sprite; } }
    public Color Color { get { return color; } }
    public bool HasBeenDisplayed { get { return displayed; } set { displayed = value; } }

    public Notification(string Text, Sprite Sprite, Color SpriteColor) {
        text = Text;
        sprite = Sprite;
        color = SpriteColor;
        displayed = false;
    }
}
