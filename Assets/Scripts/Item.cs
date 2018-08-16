using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer)), RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(AudioSource))] //ensure there is always a renderer & audio source on gameobject
public class Item : MonoBehaviour {

    [SerializeField, Tooltip("The player can only receive the reward for obtaining this item the first time they acquire it.")]
    private bool singleAcquirance;
    private Rigidbody2D rb2d;
    private SpriteRenderer sr;
    [SerializeField, Tooltip("Color this object should be if the player has already collected this item.")]
    private Color collectedColor;
    private AudioSource aSource;
    private bool obtained;
    [SerializeField]
    private Cutscene cutsceneToPlayOnPickup;
    [SerializeField]
    private int quantityRequiredForCutscene = 1;
    [SerializeField]
    private bool challengeItem;

    public bool SingleAcquirance { get { return singleAcquirance; } }
    public bool ChallengeItem { get { return challengeItem; } }

    void Awake() {
        aSource = GetComponent<AudioSource>();
        rb2d = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        if (quantityRequiredForCutscene <= 0) {
            quantityRequiredForCutscene = 1;
        }
    }

    public bool Equals(Item other) {
        if (cutsceneToPlayOnPickup == other.cutsceneToPlayOnPickup && singleAcquirance == other.singleAcquirance) {
            return true;
        }
        return false;
    }

    void FixedUpdate() {
        if (GameManager.currGameState == GameState.Active) {
            if (obtained) { //item was obtained
                if (!aSource.isPlaying) { //sound effect finished playing
                    gameObject.SetActive(false); //hide object
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (!obtained) {
            Character c = other.GetComponent<Character>();
            if (c != null) {
                PickedUpBy(c, true);
            }
        }
    }

    public virtual void PickedUpBy(Character c, bool playSoundEffect) {
        if (playSoundEffect)
            PlaySoundEffect();

        sr.color = collectedColor; //change object color
        GetComponent<Collider2D>().enabled = false; //ensure this object won't collide with anything
        rb2d.gravityScale = 0; //turn off gravity
        rb2d.velocity = Vector2.up * 0.15f; // make object float up

        obtained = true; //allow the object to disappear once sound effect ends

        if (c.ReceiveItem(this) >= quantityRequiredForCutscene) { //give the item to the character && see if enough of item obtained
            if (cutsceneToPlayOnPickup != null) { //if there is a cutscene to play
                GameManager.currGameManager.PlayCutscene(cutsceneToPlayOnPickup); //play the cutscene
            }
        }
    }

    private void PlaySoundEffect() {
        if (GameManager.SoundEnabled && aSource.clip != null) {
            aSource.volume = GameManager.SFXVolume;
            aSource.Play();
        }
    }

    public override string ToString() {
        return GameManager.SelectedCampaignMission + "_" + gameObject.name;
    }
}
