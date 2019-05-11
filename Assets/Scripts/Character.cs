//Written by Justin Ortiz

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(Animator)), DisallowMultipleComponent, System.Serializable]
public abstract class Character : MonoBehaviour {

	public static Player player;
	private static Transform characterParent;
	protected static Transform cameraCanvas;
	private static float defaultFlinchTime = 1.5f;

	[SerializeField]
	private Collider2D collider_upperbody;
	[SerializeField]
	private Collider2D collider_lowerbody;
	[SerializeField]
	protected float moveSpeed;
	private Rigidbody2D rb2D;
	private Animator anim;
	private SpriteRenderer sr;
	private Color defaultSRColor;
	private Weapon weapon;
	private int hp;
	[SerializeField]
	protected int maxhp;
	private Slider hpSlider;
	protected bool hpSliderAlwaysActive;
	private float sliderOffset;
	private float attackTimer; //used to delay attacks and as the time until the attack expires
	protected float flinchTimer; //'air-time' after being hit
	private float invulnTimer; //timespan of invulnerability
	[SerializeField]
	private float baseAttackDamage = 3;
	[SerializeField, Tooltip("How much will this enemy move/slide on their own when they attack?")]
	protected float baseAttackForce = 25f;
	protected Slider attackTimerSlider; //used to visually display the timer to the player
	private RectTransform criticalHitBox;
	[SerializeField]
	private Color sliderColor_default = Color.white;
	[SerializeField]
	private Color sliderColor_critical = Color.white;
	private bool criticalHitDisplayed;
	private bool criticalHitActivated;
	private Text statText;
	private List<Item> inventory;
	private Door doorInRange;
	private DropthroughPlatform platform;
	[SerializeField]
	protected float soundEffectDelay_movement = 1f; //time between each 'footstep' noise
	private float soundEffectTimer_movement; //how long it has been since the last one
	[SerializeField, Range(0.1f, 1.5f)]
	private float soundEffectVolumeMod_movement = 0.8f;
	[SerializeField]
	private AudioClip[] soundEffect_movement; //list of footstep sound effects to randomize
	[SerializeField]
	private AudioClip soundEffect_jump;
	[SerializeField]
	private AudioClip soundEffect_impact;


	public static int numOfEnemies { get { return characterParent.childCount - 1; } }
	public int CurrentHP { get { return hp; } }
	public int MaxHP { get { return maxhp; } }
	public float MovementSpeed { get { return moveSpeed; } }
	public Vector2 Velocity { get { return rb2D.velocity; } }
	public bool isFacingRight { get { return !sr.flipX; } }
	public bool isFacingLeft { get { return !isFacingRight; } }
	public bool isJumping { get { return anim.GetBool ("Jump"); } }
	public bool isFalling { get { return anim.GetBool ("Falling"); } }
	public bool isCrouching { get { return anim.GetBool("Crouch"); } }
	public bool isOnGround { get { return !isJumping && !isFalling ? true : false; } }
	public bool isAttacking { get { return anim.GetCurrentAnimatorStateInfo (0).IsTag ("Attack"); } }
	public bool isRunning { get { return isOnGround && Velocity.x != 0; } }
	public bool isFlinching { get { return anim.GetBool("Flinching"); } }
	public bool isInvulnerable { get { return invulnTimer > 0 ? true : false; } }
	public bool isOnAPlatform { get { return platform != null ? true : false; } }
	public Door DoorInRange { get { return doorInRange; } set { doorInRange = value; } }
	public Color SpriteColor { get { return sr.color; } }
	public Item[] Inventory { get { return inventory.ToArray(); } }

	protected void AddTorque (float torque) {
		rb2D.AddTorque (torque);
	}

	protected void Attack(string triggerName) { //the character wants to attack
		if (!isFlinching || //if the character isn't flinching
			(gameObject.tag.Equals ("Player") && GameManager_SwordSwipe.currDifficulty < GameDifficulty.Normal)) { //or the character is the player and has the difficulty set below normal
			if (weapon != null) {
				if (attackTimer < (weapon.currentAttackDelay.y - weapon.currentAttackDelay.x) && !anim.GetBool ("Attack_Expire")) { //attackTimer starts at max value, so we need to make sure the min delay is upheld

					weapon.Attack (triggerName);
					anim.SetTrigger (triggerName);
					if (isFlinching) { //if the character is flinching
						anim.SetBool("Flinching", false); //reset bool so we can see the attack animation

						if (weapon != null) { //if they have a weapon
							weapon.Flinch(false); //reset bool so we can see the attack animation
						}
					}

					if (triggerName.Equals ("Attack_Forward")) { //if swinging forward (aka power attack)
						if (!gameObject.tag.Equals ("Player") || Mathf.Abs (rb2D.velocity.x) < moveSpeed / 2f) { //if not going too fast -- don't want to be skating across world with power attacks
							if (isFacingRight) {
								rb2D.AddForce (new Vector2 (weapon.attackForce, 0f));
							} else {
								rb2D.AddForce (new Vector2 (-weapon.attackForce, 0f));
							}
						}
					} else if (triggerName.Equals ("Attack_Backward")) { //if swinging backward
						sr.flipX = !sr.flipX; //flip the sprite
						weapon.FaceToggle (); //flip the weapon sprite
					}

					if (criticalHitDisplayed) { //if the player timed the attack correctly
						criticalHitActivated = true; //they get a critical hit
					} else {
						criticalHitActivated = false;
					}

					attackTimer = weapon.currentAttackDelay.y;

					if (attackTimerSlider != null) {
						attackTimerSlider.maxValue = attackTimer;
						criticalHitBox.anchorMin = new Vector2(weapon.currentCritRange.x, 0.1f);
						criticalHitBox.anchorMax = new Vector2(weapon.currentCritRange.y, 0.9f);
					}
				}
			} else {
				anim.SetTrigger ("Attack");
				anim.SetBool ("Run", false); //this and next line may not be necessary
				anim.SetBool ("Idle", false);

				if (isFlinching) {
					anim.SetBool("Flinching", false);

					if (weapon != null) {
						weapon.Flinch(false);
					}
				}

				if (Mathf.Abs (rb2D.velocity.x) < 2f) { //if character isn't moving too fast
					if (!sr.flipX) //facing right
						rb2D.AddForce (new Vector2 (baseAttackForce, 0)); //add some 'umph' to the attack
					else
						rb2D.AddForce (new Vector2 (-baseAttackForce, 0));
				}
			}
		}
	}

	private void AttackExpire() {
		anim.SetBool ("Attack_Expire", true);

		if (weapon != null)
			weapon.Attack_Expire ();

		if (attackTimerSlider != null) {
			attackTimerSlider.fillRect.GetComponent<Image>().color = sliderColor_default;
			criticalHitDisplayed = false;
			attackTimerSlider.gameObject.SetActive (false);
		}

		attackTimer = 0;
	}

	protected void Crouch() {
		if (!isCrouching) {
			anim.SetBool("Crouch", true);
			if (collider_upperbody != null) {
				collider_upperbody.enabled = false;
			}
		}
	}

	public virtual void DetectBeginOtherCharacter (Character otherCharacter) {
		// empty void for other classes to fill out if necessary
	}

	public virtual void DetectEndOtherCharacter(Character otherCharacter) {
		//empty void for other classes to fill out if necessary
	}

	public virtual void Die() {
		Destroy (hpSlider.gameObject);
		Destroy (statText.gameObject);
		if (attackTimerSlider != null)
			Destroy (attackTimerSlider.gameObject);
		inventory.Clear ();
		Destroy (gameObject);
	}

	protected void DropThroughPlatform() {
		if (isOnAPlatform) {
			platform.DropThrough(gameObject);
		}
	}

	protected void Fly (Vector2 direction) {
		if (rb2D.gravityScale > 0)
			rb2D.gravityScale = 0;

		if (!isAttacking) {
			if (direction.magnitude != 1)
				direction.Normalize ();

			if (direction.x < 0) {//if moving left
				sr.flipX = true;

				if (weapon != null) {
					weapon.FaceLeft ();
				}
			} else if (direction.x > 0) {
				sr.flipX = false;

				if (weapon != null) {
					weapon.FaceRight ();
				}
			}

			if (!isFlinching) { //if character hasn't been hit recently
				rb2D.velocity = direction * moveSpeed; //move normally; set velocity
				if (!direction.Equals(Vector2.zero)) { //if character is moving
					anim.SetBool ("Run", true);
					anim.SetBool ("Idle", false);

					if (weapon != null)
						weapon.Move ();
				} else { //else character is idle
					anim.SetBool ("Run", false);
					anim.SetBool ("Idle", true);

					if (weapon != null)
						weapon.Idle ();
				}
			} else { //character has been hit recently
				if ((rb2D.velocity.x < moveSpeed / 2f && direction.x > 0) || (rb2D.velocity.x > -(moveSpeed / 2f) && direction.x < 0))
					rb2D.AddForce (new Vector2 (direction.x * moveSpeed, 0)); //only able to make slight adjustments mid-air;
			}
		}
	}

	protected virtual Vector3 GetHPSliderPos() {
		return transform.position + (Vector3.up * sliderOffset);
	}

	protected virtual Vector2 GetHPSliderSizeDelta() {
		return new Vector2 (Screen.width / 15f, Screen.height / 15f);
	}

	public bool HasItem (Item i) { //has key?
		if (inventory.Contains (i)) { //if character has key
			return true; //return true
		}
		return false; //else return false
	}

	public bool HasItem (string itemName, int quantity) { //character collected enough parts for weapon?
		int total = 0; //total number of the item that this character has
		for (int i = 0; i < inventory.Count; i++) { //go through inventory
			if (inventory [i].ToString().Equals (itemName)) { //if item name matches
				total++; //add to the total
				if (total >= quantity) //if the character has enough
					return true; //return true
			}
		}
		return false; //went through whole list, and didn't have enough
	}

	protected virtual void Initialize () {
		if (cameraCanvas == null)
			cameraCanvas = GameObject.FindGameObjectWithTag ("Camera Canvas").transform;
		if (characterParent == null)
			characterParent = GameObject.FindGameObjectWithTag ("Character Parent").transform;

		transform.SetParent (characterParent);
		
		rb2D = gameObject.GetComponent<Rigidbody2D> ();
		anim = gameObject.GetComponent<Animator> ();
		sr = gameObject.GetComponent<SpriteRenderer> ();

		defaultSRColor = sr.color;

		sliderOffset = ((sr.sprite.bounds.extents.y * 2f) * (gameObject.GetComponent<Collider2D>().bounds.size.y));

		hp = maxhp;
		hpSlider = (GameObject.Instantiate (Resources.Load ("UI/hpSlider"), cameraCanvas) as GameObject).GetComponent<Slider> ();
		hpSlider.gameObject.name = gameObject.name + "'s hp slider";
		hpSlider.maxValue = maxhp;
		hpSlider.GetComponent<RectTransform> ().sizeDelta = GetHPSliderSizeDelta ();

		if (!hpSliderAlwaysActive)
			hpSlider.gameObject.SetActive (false);

		statText = (GameObject.Instantiate(Resources.Load("UI/statusText"), cameraCanvas) as GameObject).GetComponent<Text>();
		statText.gameObject.name = gameObject.name + "'s status text";
		statText.gameObject.SetActive (false);

		weapon = gameObject.GetComponentInChildren<Weapon> ();

		if (attackTimerSlider != null) {
			if (weapon != null)
				attackTimerSlider.maxValue = weapon.currentAttackDelay.y;
			attackTimerSlider.gameObject.SetActive (false);
			criticalHitBox = attackTimerSlider.transform.Find("Fill Area").Find("Critical Hit Box").GetComponent<RectTransform>();
		}

		inventory = new List<Item> ();
	}

	protected void Jump() {
		if (!isAttacking && !isJumping) { //if character is able to jump
			anim.SetBool("Jump", true); //set bool for jumping

			if (isCrouching) { //if character is crouching
				UnCrouch(); //force them to not crouch
			}

			if (weapon != null)
				weapon.Jump (); //tell weapon to jump

			if (soundEffect_jump != null && GameManager_SwordSwipe.SoundEnabled) //if there is a jump sound effect
				AudioManager.PlaySoundEffect(soundEffect_jump, transform.position, soundEffectVolumeMod_movement); //play the sound effect

			rb2D.AddForce (Vector2.up * rb2D.mass * 300); //add up force
		}
	}

	private IEnumerator LandOnGround () {
		anim.SetBool ("Jump", false);
		anim.SetBool ("Falling", false);

		if (weapon != null)
			weapon.Land ();

		if (GameManager_SwordSwipe.SoundEnabled) {
			if (soundEffect_movement.Length > 0) {
				int index_movementEffect = Random.Range(0, soundEffect_movement.Length); //get current footstep noise

				AudioManager.PlaySoundEffect(soundEffect_movement[index_movementEffect], transform.position, soundEffectVolumeMod_movement);
				yield return new WaitForEndOfFrame();
				AudioManager.PlaySoundEffect(soundEffect_movement[index_movementEffect], transform.position, soundEffectVolumeMod_movement);
			}
		}
	}

	protected void LookAt(Transform other) {
		bool flipNeeded = false;
		if (other.position.x < transform.position.x) { //object is left of character
			if (isFacingRight) { //character is looking right
				flipNeeded = true; //we need to flip
			}
		} else { //object is to the right
			if (isFacingLeft) { // character is facing left
				flipNeeded = true; //flip needed
			}
		}

		if (flipNeeded) {
			sr.flipX = !sr.flipX; //flip the sprite

			if (weapon != null)
				weapon.FaceToggle(); //flip the weapon sprite
		}
	}

	void OnCollisionEnter2D(Collision2D otherObj) {
		if (otherObj.gameObject.tag.Equals ("Player")) { //if the other object is the player
			if (!isFlinching) { //and this isn't flinching
				otherObj.gameObject.GetComponent<Character> ().ReceiveDamageFrom (this); //damage the player
			}
		} else {
			DropthroughPlatform dropthrough = otherObj.gameObject.GetComponent<DropthroughPlatform>();
			if (dropthrough != null) {
				platform = dropthrough;
			}
		}
	}

	void OnCollisionExit2D(Collision2D collision) {
		if (isOnAPlatform) { //character on a platform
			if (collision.gameObject.Equals(platform.gameObject)) { //the object we are no longer colliding with is the platform
				platform = null; //remove reference to platform
			}
		}
	}

	protected internal void ReceiveDamage(float srcDmgVal, bool criticalHit) {
		if (GameManager_SwordSwipe.currGameState != GameState.Active)
			return;

		if (!isInvulnerable) { //enemies typically never invulnerable
			if (!isFlinching) {
				if (gameObject.tag.Equals ("Player")) {
					flinchTimer = defaultFlinchTime / 2f;
				} else {
					flinchTimer = defaultFlinchTime / (int)GameManager_SwordSwipe.currDifficulty;
				}

				anim.SetBool("Flinching", true);

				if (weapon != null) {
					weapon.Flinch(true);
				}
			}

			float damage = 0;

			if (gameObject.tag.Equals ("Player")) { //player takes more damage based on difficulty
				if (GameManager_SwordSwipe.currDifficulty == GameDifficulty.Masochist) {
					damage = maxhp; //instakill player
				} else {
					damage = srcDmgVal * 0.5f * ((int)GameManager_SwordSwipe.currDifficulty + 1); //do more damage based on difficulty -- 0.5x, 1x, 1.5x damage
				}

				invulnTimer = 1f; //player receives temporary invulnerability when damaged
			} else {
				damage = srcDmgVal;
			}

			if (criticalHit) {
				damage *= 1.25f;

				statText.text = "-" + (int)damage + "\nCritical Hit!";
				statText.color = sliderColor_critical;
			} else {
				statText.text = "-" + (int)damage;
				statText.color = sliderColor_default;
			}

			hp -= (int)(damage); //take away the appropriate health

			hpSlider.value = hp;
			
			AttackExpire ();

			if (soundEffect_impact != null && GameManager_SwordSwipe.SoundEnabled)
				AudioManager.PlaySoundEffect(soundEffect_impact, transform.position, 1f);
		}
	}

	public void ReceiveDamageFrom(Character c) {
		ReceiveKnockback (c);
		ReceiveDamage (c.baseAttackDamage, false); //handle damage
		LookAt(c.transform);
	}

	public void ReceiveDamageFrom(Weapon w) {
		ReceiveKnockback (w.Wielder);
		ReceiveDamage (w.Damage, w.Wielder.criticalHitActivated); //handle damage + possibility of critical hit
		LookAt(w.Wielder.transform);
	}

	public int ReceiveItem(Item i) {
		inventory.Add (i); //add item to the inventory
		int total = 0; //initialize the total
		foreach (Item obtained in inventory) { //go through the inventory
			if (obtained.Equals(i)) { //see how many of this item are in the inventory
				total++; //increment total
			}
		}
		if (i.ChallengeItem) {
			GameManager_SwordSwipe.currGameManager.ChallengeActionComplete(GameManager_SwordSwipe.SelectedCampaignMission + "_item:" + i.gameObject.name + "_item:" + total);
		}
		return total; //return total
	}

	private void ReceiveKnockback(Character c) {
		float forceMagnitude = (rb2D.mass * 150) + c.rb2D.velocity.magnitude; //normal knockback + how fast the enemy was moving
		Vector2 knockbackDir = rb2D.position - c.rb2D.position;

		if (!c.anim.GetCurrentAnimatorStateInfo (0).IsName ("Attack_Up")) { //not attacking upwards
			knockbackDir.y = 0f; //don't move vertically
		} else {
			knockbackDir.y = knockbackDir.x * 2f;
		}

		rb2D.velocity = Vector2.zero; //stop current movement
		rb2D.AddForce (knockbackDir.normalized * forceMagnitude); //knockback
	}

	protected void Respawn(Vector3 location) {
		hp = maxhp;
		hpSlider.value = hp;

		if (gameObject.tag.Equals ("Player")) //player receives temporary invulnerability when respawning
			invulnTimer = 1f;

		AttackExpire ();

		transform.position = location;
	}

	protected void Run(float xVel) {
		if (rb2D.gravityScale < 1)
			rb2D.gravityScale = 1;

		if (isCrouching) {
			UnCrouch();
		}

		if (!isAttacking) {
			xVel = Mathf.Clamp(xVel, -1, 1);

			if (xVel < 0) {//if moving left
				sr.flipX = true;

				if (weapon != null) {
					weapon.FaceLeft ();
				}
			} else if (xVel > 0) {
				sr.flipX = false;

				if (weapon != null) {
					weapon.FaceRight ();
				}
			}

			if (!isFlinching) { //if character hasn't been hit recently
				rb2D.velocity = new Vector2 (xVel * moveSpeed, rb2D.velocity.y); //move normally; set velocity
				if (xVel != 0) { //if character is moving
					anim.SetBool ("Run", true);
					anim.SetBool ("Idle", false);

					if (weapon != null)
						weapon.Move ();

					if (GameManager_SwordSwipe.SoundEnabled) {
						if (soundEffectTimer_movement >= soundEffectDelay_movement) {
							if (soundEffect_movement.Length > 0) {
								AudioManager.PlaySoundEffect(soundEffect_movement[Random.Range(0, soundEffect_movement.Length)], transform.position, soundEffectVolumeMod_movement);
							}
							soundEffectTimer_movement = 0;
						}
						soundEffectTimer_movement += Time.deltaTime;
					}
				} else { //else character is idle
					anim.SetBool ("Run", false);
					anim.SetBool ("Idle", true);

					if (weapon != null)
						weapon.Idle ();
				}
			} else { //character has been hit recently
				if ((rb2D.velocity.x < moveSpeed / 2f && xVel > 0) || (rb2D.velocity.x > -(moveSpeed / 2f) && xVel < 0))
					rb2D.AddForce (new Vector2 (xVel * moveSpeed, 0)); //only able to make slight adjustments mid-air;
			}
		}
	}

	protected void StopMovement() {
		rb2D.velocity = Vector2.zero;
	}

	protected void StopRotation() {
		rb2D.angularVelocity = 0;
	}

	protected void UnCrouch() {
		anim.SetBool("Crouch", false);
		if (collider_upperbody != null) {
			collider_upperbody.enabled = true;
		}
	}

	protected void UpdateAnimations() {
		if (rb2D.velocity.y < -0.5f) { //if the character is moving downward
			anim.SetBool("Falling", true); //character is falling
			if (weapon != null) //if character has weapon
				weapon.Fall(); //tell weapon to play fall anim
		} else if (!isOnGround) { //if the character is not marked as not on the ground
			if (isFalling) { //the character has been marked as falling
				if (rb2D.velocity.y < 0.5f) { //make sure not launching upwards
					RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, sr.sprite.bounds.extents.y + 0.01f);

					if (hit) { //if there is a hit
						StartCoroutine(LandOnGround());
					}
				}
			}
		}

		if (anim.GetBool("Attack_Expire") && !isAttacking) { //if the current attack needs to expire and the player is not attacking atm
			anim.SetBool ("Attack_Expire", false);
			anim.SetBool ("Run", false);
			anim.SetBool ("Idle", true);

			if (weapon != null)
				weapon.Attack_Available ();
		}

		if (attackTimer > 0 && !isFlinching) {
			if (weapon.currentHitboxEnableRange.x * weapon.currentAttackDelay.y < attackTimer &&
				attackTimer < weapon.currentHitboxEnableRange.y * weapon.currentAttackDelay.y) //if attack time is at right % of delay for hitbox
				weapon.Hitbox_Enable ();
			else
				weapon.Hitbox_Disable();

			if (attackTimerSlider != null) {
				attackTimerSlider.value = attackTimer; //update slider value

				if (weapon.currentCritRange.x * weapon.currentAttackDelay.y < attackTimer &&
					attackTimer < weapon.currentCritRange.y * weapon.currentAttackDelay.y) { //if attack time is at right % of delay for crit
					attackTimerSlider.fillRect.GetComponent<Image>().color = sliderColor_critical;//change slider color to show when player should attack again
					criticalHitDisplayed = true; //ensure character gets crit if they attack at right time
				} else {
					attackTimerSlider.fillRect.GetComponent<Image>().color = sliderColor_default;//reset slider color
					criticalHitDisplayed = false; //reset crit availability
				}

				attackTimerSlider.transform.position = transform.position + (Vector3.down * sliderOffset); //keep the slider below the character

				if (!attackTimerSlider.gameObject.activeSelf) //if not already enabled
					attackTimerSlider.gameObject.SetActive (true); //enable slider
			}

			attackTimer -= Time.fixedDeltaTime; //update attack timer
		} else if (isAttacking) { //last frame of attacking
			AttackExpire (); //stop attacking
		}
		
		if (isFlinching) {
			if (!sr.color.Equals (Color.red))
				sr.color = Color.red;

			hpSlider.transform.position = GetHPSliderPos();
			statText.transform.position = transform.position + (Vector3.down * (sliderOffset + 0.12f));

			if (!hpSlider.gameObject.activeSelf)
				hpSlider.gameObject.SetActive (true);

			if (!statText.gameObject.activeSelf)
				statText.gameObject.SetActive (true);

			if (hp <= 0) {
				if (collider_lowerbody != null) {
					if (collider_lowerbody.enabled) {
						collider_lowerbody.enabled = false;
					}
				}
				UnCrouch();
				if (collider_upperbody != null) {
					collider_upperbody.enabled = false;
				}
				rb2D.gravityScale /= 4f;
				rb2D.constraints = RigidbodyConstraints2D.None;
			}

			
			if (flinchTimer > 0) {
				flinchTimer -= Time.fixedDeltaTime;
			} else if (isFlinching){
				anim.SetBool("Flinching", false);

				if (weapon != null) {
					weapon.Flinch(false);
				}
			}
		} else if (hpSlider.gameObject.activeSelf) { //flinching has just ended
			if (hp > 0) {
				sr.color = defaultSRColor;
				statText.gameObject.SetActive (false);

				if (!hpSliderAlwaysActive)
					hpSlider.gameObject.SetActive (false);
			} else {
				Die ();
			}
		}
			
		if (isInvulnerable) {
			invulnTimer -= Time.fixedDeltaTime;
		}
	}

	public void Wield (Weapon w) {
		if (weapon != null) {
			Destroy(weapon.gameObject);
		}
		weapon = w;
		w.AssignTo (this);
	}
}