using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D)), RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(Animator)), DisallowMultipleComponent, System.Serializable]
public abstract class Character : MonoBehaviour {

	public static Player player;
	private static Transform characterParent;
	protected static Transform cameraCanvas;
	private static float defaultFlinchTime = 1.5f;
	[SerializeField]
	private float moveSpeed;
	private BoxCollider2D bc2D;
	private Rigidbody2D rb2D;
	private Animator anim;
	private SpriteRenderer sr;
	private Color defaultSRColor;
	private float groundDetectDist;
	private float groundLandingDelay;
	private Weapon weapon;
	private int hp;
	[SerializeField]
	protected int maxhp;
	private Slider hpSlider;
	protected bool hpSliderAlwaysActive;
	private float attackTimer; //used to delay attacks and as the time until the attack expires
	private float flinchTimer; //'air-time' after being hit
	private float invulnTimer; //timespan of invulnerability
	[SerializeField]
	private float baseAttackDamage = 3;
	[SerializeField, Tooltip("How much will this enemy move/slide on their own when they attack?")]
	protected float baseAttackForce = 25f;
	protected Slider attackTimerSlider; //used to visually display the timer to the player
	[SerializeField]
	private Color sliderColor_default = Color.white;
	[SerializeField]
	private Color sliderColor_critical = Color.white;
	private bool critAvailable;
	private Text statText;

	public static int numOfEnemies { get { return characterParent.childCount - 1; } }
	public int MaxHP { get { return maxhp; } }
	public float MovementSpeed { get { return moveSpeed; } }
	public Vector2 Velocity { get { return rb2D.velocity; } }
	public bool isFacingRight { get { return !sr.flipX; } }
	public bool isFacingLeft { get { return !isFacingRight; } }
	public bool isJumping { get { return anim.GetBool ("Jump"); } }
	public bool isFalling { get { return anim.GetBool ("Falling"); } }
	public bool isOnGround { get { return !isJumping && !isFalling ? true : false; } }
	public bool isAttacking { get { return anim.GetCurrentAnimatorStateInfo (0).IsTag ("Attack"); } }
	public bool isFlinching { get { return flinchTimer > 0 ? true : false; } }
	public bool isInvulnerable { get{ return invulnTimer > 0 ? true : false; } }

	protected void AddTorque (float torque) {
		rb2D.AddTorque (torque);
	}

	protected void Attack(string triggerName) {
		if (!isFlinching || gameObject.tag.Equals ("Player")) {
			if (weapon != null) {
				if (attackTimer < (weapon.currentAttackDelay.y - weapon.currentAttackDelay.x) && !anim.GetBool ("Attack_Expire")) { //attackTimer starts at max value, so we need to make sure the min delay is upheld

					weapon.Attack (triggerName);
					anim.SetTrigger (triggerName);

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

					if (attackTimer < weapon.currentCritRange.y && weapon.currentCritRange.x < attackTimer) { //if the player timed the attack correctly
						critAvailable = true; //they get a critical hit
					} else {
						critAvailable = false;
					}

					attackTimer = weapon.currentAttackDelay.y;

					if (attackTimerSlider != null)
						attackTimerSlider.maxValue = attackTimer;
				}
			} else {
				anim.SetTrigger ("Attack");
				anim.SetBool ("Run", false);
				anim.SetBool ("Idle", false);

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
			attackTimerSlider.gameObject.SetActive (false);
		}

		attackTimer = 0;
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
		Destroy (gameObject);
	}

	protected void Fly (Vector2 direction) {
		if (rb2D.gravityScale > 0)
			rb2D.gravityScale = 0;

		if (!isAttacking) {
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
		return transform.position + (Vector3.up * groundDetectDist);
	}

	protected virtual Vector2 GetHPSliderSizeDelta() {
		return new Vector2 (Screen.width / 15f, Screen.height / 15f);
	}

	protected void Initialize () {
		if (cameraCanvas == null)
			cameraCanvas = GameObject.FindGameObjectWithTag ("Camera Canvas").transform;
		if (characterParent == null)
			characterParent = GameObject.FindGameObjectWithTag ("Character Parent").transform;

		transform.SetParent (characterParent);

		bc2D = gameObject.GetComponent<BoxCollider2D> ();
		rb2D = gameObject.GetComponent<Rigidbody2D> ();
		anim = gameObject.GetComponent<Animator> ();
		sr = gameObject.GetComponent<SpriteRenderer> ();

		defaultSRColor = sr.color;

		groundDetectDist = (sr.sprite.bounds.extents.y * (gameObject.GetComponent<BoxCollider2D>().size.y)) + 0.05f;
		groundLandingDelay = 0f;

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
		}
	}

	protected void Jump() {
		if (!isAttacking && !isJumping) {
			anim.SetBool ("Jump", true);

			if (weapon != null)
				weapon.Jump ();

			rb2D.AddForce (Vector2.up * rb2D.mass * 300);
		}
	}

	protected void LandOnGround() {
		anim.SetBool ("Jump", false);
		anim.SetBool ("Falling", false);

		if (weapon != null)
			weapon.Land ();

		groundLandingDelay = 0;
	}

	void OnCollisionEnter2D(Collision2D otherObj) {
		if (otherObj.transform.position.y < transform.position.y) { //if other object is below
			if (transform.position.x > otherObj.transform.position.x && transform.position.x < otherObj.transform.position.x + otherObj.collider.bounds.size.x) { //other object is centered below
				LandOnGround();
			}
		}

		if (otherObj.gameObject.tag.Equals ("Player")) { //if the other object is the player
			if (!isFlinching) { //and this isn't flinching
				otherObj.gameObject.GetComponent<Character> ().ReceiveDamageFrom (this); //damage the player
			}
		}
	}

	private void ReceiveDamage(float srcDmgVal, bool criticalHit) {
		if (!isInvulnerable) { //enemies typically never invulnerable
			if (!isFlinching) {
				if (gameObject.tag.Equals ("Player")) {
					flinchTimer = defaultFlinchTime / 2f;
				} else {
					flinchTimer = defaultFlinchTime;
				}
			}

			float damage = 0;
			float difficultyDamageModifier = 1f;

			if (gameObject.tag.Equals ("Player")) { //player takes more damage based on difficulty
				switch (GameManager.currDifficulty) {
				case GameDifficulty.Easiest:
					difficultyDamageModifier = 0.5f;
					break;
				case GameDifficulty.Easy:
					difficultyDamageModifier = 0.8f;
					break;
				case GameDifficulty.Normal:
					difficultyDamageModifier = 1f;
					break;
				case GameDifficulty.Masochist:
					damage = maxhp;
					break;
				}

				invulnTimer = 1f; //player receives temporary invulnerability when damaged
			}

			if (damage == 0) {
				damage += srcDmgVal;
			}

			damage *= difficultyDamageModifier;

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

			if (!gameObject.tag.Equals ("Player"))
				AttackExpire ();
		}
	}

	public void ReceiveDamageFrom(Character c) {
		ReceiveKnockback (c);
		ReceiveDamage (c.baseAttackDamage, false); //handle damage
	}

	public void ReceiveDamageFrom(Weapon w) {
		ReceiveKnockback (w.wielder);
		ReceiveDamage (w.Damage, w.wielder.critAvailable); //handle damage + possibility of critical hit
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

	protected void Run(int xDir) {
		if (rb2D.gravityScale < 1)
			rb2D.gravityScale = 1;

		if (!isAttacking) {
			xDir = Mathf.Clamp(xDir, -1, 1);

			if (xDir < 0) {//if moving left
				sr.flipX = true;

				if (weapon != null) {
					weapon.FaceLeft ();
				}
			} else if (xDir > 0) {
				sr.flipX = false;

				if (weapon != null) {
					weapon.FaceRight ();
				}
			}

			if (!isFlinching) { //if character hasn't been hit recently
				rb2D.velocity = new Vector2 (xDir * moveSpeed, rb2D.velocity.y); //move normally; set velocity
				if (xDir != 0) { //if character is moving
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
				if ((rb2D.velocity.x < moveSpeed / 2f && xDir > 0) || (rb2D.velocity.x > -(moveSpeed / 2f) && xDir < 0))
					rb2D.AddForce (new Vector2 (xDir * moveSpeed, 0)); //only able to make slight adjustments mid-air;
			}
		}
	}

	protected void StopRotation() {
		rb2D.angularVelocity = 0;
	}

	protected void UpdateAnimations() {
		
		if (rb2D.velocity.y < -0.0001f) {
			anim.SetBool ("Falling", true);
			anim.SetBool ("Jump", true);
			if (weapon != null)
				weapon.Fall ();
		} else if (rb2D.velocity.y > 0.0001f) {
			anim.SetBool ("Falling", false);
			anim.SetBool ("Jump", true);
		} else if (!isOnGround && isFalling) { //velocity is ~0 && we haven't moved vertically for a bit
			groundLandingDelay += Time.fixedDeltaTime;

			if (groundLandingDelay >= 0.15f)
				LandOnGround ();
		}

		if (anim.GetBool("Attack_Expire") && !isAttacking) {
			anim.SetBool ("Attack_Expire", false);
			anim.SetBool ("Run", false);
			anim.SetBool ("Idle", true);

			if (weapon != null)
				weapon.Attack_Available ();
		}

		if (attackTimer > 0) {
			attackTimer -= Time.fixedDeltaTime;


			if (attackTimer < weapon.currentAttackDelay.y - weapon.currentHitboxEnableRange.x
				&& attackTimer > weapon.currentAttackDelay.y - weapon.currentHitboxEnableRange.y)
				weapon.Hitbox_Enable ();
			else
				weapon.Hitbox_Disable();

			if (attackTimerSlider != null) {
				attackTimerSlider.value = attackTimer;

				if (attackTimer < weapon.currentAttackDelay.y - weapon.currentCritRange.x
					&& attackTimer > weapon.currentAttackDelay.y - weapon.currentCritRange.y) {
					if (!critAvailable) {
						attackTimerSlider.fillRect.GetComponent<Image> ().color = sliderColor_critical;
						critAvailable = true;
					}
				} else {
					if (critAvailable) {
						attackTimerSlider.fillRect.GetComponent<Image> ().color = sliderColor_default;
						critAvailable = false;
					}
				}

				attackTimerSlider.transform.position = transform.position + (Vector3.down * groundDetectDist);

				if (!attackTimerSlider.gameObject.activeSelf)
					attackTimerSlider.gameObject.SetActive (true);
			}
		} else if (isAttacking) {
			AttackExpire ();
		}
		
		if (isFlinching) {
			if (!sr.color.Equals (Color.red))
				sr.color = Color.red;

			hpSlider.transform.position = GetHPSliderPos();
			statText.transform.position = transform.position + (Vector3.down * (groundDetectDist + 0.12f));

			if (!hpSlider.gameObject.activeSelf)
				hpSlider.gameObject.SetActive (true);

			if (!statText.gameObject.activeSelf)
				statText.gameObject.SetActive (true);

			if (hp <= 0 && bc2D.enabled) {
				bc2D.enabled = false;
				rb2D.gravityScale /= 4f;
			}

			flinchTimer -= Time.fixedDeltaTime;
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
}