using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D)), RequireComponent(typeof(Animator))]
public class Weapon : MonoBehaviour {

	[HideInInspector]
	public Character wielder;
	private SpriteRenderer sr;
	private Animator anim;
	private BoxCollider2D bc2D;
	private int currAnim;
	[Header("Attack Settings"), SerializeField]
	private int damage;
	[Tooltip("How much force is behind each attack?")]
	public float attackForce = 80f;
	[SerializeField, Tooltip("The shortest time between swings (x) and the longest (y).\nOne array element per animation.")]
	private Vector2[] attackDelayRanges;
	private Vector2 currAttackDelay;
	[SerializeField, Tooltip("The range in which the player would be considered to 'perfectly' time the attack.\nOne array element per animation")]
	private Vector2[] critRanges;
	private Vector2 currCritRange;
	[Header("Hitbox Settings")]
	[SerializeField, Tooltip("When to enable the hitbox. \nExample: (0, swingdelaymax) means hitbox enabled for entire swing\nOne array element per animation.")]
	private Vector2[] hitboxEnableRanges;
	private Vector2 currhbEnableRange;
	[SerializeField, Tooltip("Hitbox offset when the player is facing left.")]
	private Vector2 hitboxOffsetLeft;
	[SerializeField, Tooltip("Hitbox offset when the player is facing right.")]
	private Vector2 hitboxOffsetRight;

	public int Damage { get { return damage; } }
	public Vector2 currentAttackDelay { get { return currAttackDelay; } }
	public Vector2 currentHitboxEnableRange { get { return currhbEnableRange; } }
	public Vector2 currentCritRange { get { return currCritRange; } }

	/// <summary>
	/// Returns the name of the trigger to call for the character animation.
	/// </summary>
	public void Attack(string triggerName) {
		if (!bc2D.enabled) { //only flips hitbox when it is not active
			if (sr.flipX) {
				bc2D.offset = hitboxOffsetLeft;
			} else {
				bc2D.offset = hitboxOffsetRight;
			}
		}

		anim.SetTrigger (triggerName); //trigger the animation
		anim.SetBool ("Run", false); //ensure other bools don't get in the way
		anim.SetBool ("Idle", false);

		//get current animation, set currAnim to appropriate index
		if (anim.GetCurrentAnimatorStateInfo (0).IsName ("Attack1")) {
			currAnim = 0;
		} else if (anim.GetCurrentAnimatorStateInfo (0).IsName ("Attack2")) {
			currAnim = 1;
		} else if (anim.GetCurrentAnimatorStateInfo (0).IsName ("Attack_Up")) {
			currAnim = 2;
		}

		if (currAnim < attackDelayRanges.Length)
			currAttackDelay = attackDelayRanges [currAnim];

		if (currAnim < hitboxEnableRanges.Length)
			currhbEnableRange = hitboxEnableRanges [currAnim];

		if (currAnim < critRanges.Length)
			currCritRange = critRanges [currAnim];
	}

	public void Attack_Available() {
		anim.SetBool ("Attack_Expire", false);
	}

	public void Attack_Expire() {
		anim.SetBool ("Attack_Expire", true);
		anim.SetBool ("Run", false);
		anim.SetBool ("Idle", true);
		currAnim = 0;
	}

	public void FaceLeft() {
		sr.flipX = true;
	}

	public void FaceRight() {
		sr.flipX = false;
	}

	public void FaceToggle () {
		sr.flipX = !sr.flipX;
	}

	public void Fall() {
		anim.SetBool ("Jump", false);
		anim.SetBool ("Falling", true);
	}

	public void Hitbox_Enable() {
		if (!bc2D.enabled)
			bc2D.enabled = true;
	}

	public void Hitbox_Disable() {
		if (bc2D.enabled)
			bc2D.enabled = false;
	}

	public void Idle() {
		anim.SetBool ("Run", false);
		anim.SetBool ("Idle", true);
	}

	public void Jump() {
		anim.SetBool ("Jump", true);
		anim.SetBool ("Falling", false);
	}

	public void Land() {
		anim.SetBool ("Jump", false);
		anim.SetBool ("Falling", false);
	}

	public void Move() {
		anim.SetBool ("Run", true);
		anim.SetBool ("Idle", false);
	}

	public void OnTriggerEnter2D(Collider2D otherObj) {
		if (!otherObj.Equals (wielder)) { //if it's not the character wielding the weapon
			Character otherCharacter = otherObj.GetComponent<Character> ();
			if (otherCharacter != null) //if it is actually a character
				otherCharacter.ReceiveDamageFrom (wielder);
		}
	}

	void Start() {
		if (transform.parent != null) {
			wielder = transform.parent.GetComponent<Character>();
		} else {
			Debug.Log ("Weapon '" + gameObject.name + "' does not have a character wielding it (parent).");
		}

		sr = gameObject.GetComponent<SpriteRenderer> ();
		anim = gameObject.GetComponent<Animator> ();
		bc2D = gameObject.GetComponent<BoxCollider2D> ();

		if (!bc2D.isTrigger)
			bc2D.isTrigger = true;
		if (bc2D.enabled)
			bc2D.enabled = false;

		currAnim = 0;

		if (attackDelayRanges.Length >= 1)
			currAttackDelay = attackDelayRanges [currAnim];
		else
			currAttackDelay = new Vector2 (0.1f, 0.7f);

		if (critRanges.Length >= 1)
			currCritRange = critRanges [currAnim];
		else
			currCritRange = new Vector2 (0.3f, 0.5f);

		if (hitboxEnableRanges.Length >= 1)
			currhbEnableRange = hitboxEnableRanges [currAnim];
		else
			currhbEnableRange = new Vector2 (0f, 0.3f);
	}
}