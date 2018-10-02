//written by Justin Ortiz

using UnityEngine;

public class ScrollingBackground : MonoBehaviour {

	[SerializeField]
	private bool parallax;
	[SerializeField]
	private Vector2 parallaxMultiplier = new Vector2(0.01f, 0.01f);
	private float rightEdge;
	private float leftEdge;
	private float topEdge;
	private float bottomEdge;
	private Vector3 horizontalDistance;
	private Vector3 verticalDistance;
	private SpriteRenderer sr;

	void Start() {
		//calculate the edges
		sr = GetComponent<SpriteRenderer>();
		rightEdge = transform.position.x + (sr.bounds.extents.x / 3f);
		leftEdge = transform.position.x - (sr.bounds.extents.x / 3f);

		topEdge = transform.position.y + (sr.bounds.extents.y / 3f);
		bottomEdge = transform.position.y - (sr.bounds.extents.y / 3f);

		horizontalDistance = new Vector3(rightEdge - leftEdge, 0f, 0f); //get horizontal distance
		verticalDistance = new Vector3(0f, topEdge - bottomEdge, 0f); //get vert distance
	}

	void Update() {
		if (parallax) {
			AdjustPosition(new Vector3(-Character.player.Velocity.x * parallaxMultiplier.x, -Character.player.Velocity.y * parallaxMultiplier.y));
		}
		
		if (Character.player.transform.position.x > rightEdge) { //player passed right edge of background
			AdjustPosition(horizontalDistance);
		} else if (Character.player.transform.position.x < leftEdge) { //player passed right edge of background
			AdjustPosition(-horizontalDistance);
		}

		if (Character.player.transform.position.y > topEdge) { //player passed top edge of background
			AdjustPosition(verticalDistance);
		} else if (Character.player.transform.position.y < bottomEdge) { //player passed bottom edge of background
			AdjustPosition(-verticalDistance);
		}
	}

	private void AdjustPosition(Vector3 distance) {
		transform.position += distance; //move background so it seems infinite

		//adjust edges so it will continue to move after the first time
		rightEdge += distance.x;
		leftEdge += distance.x;

		topEdge += distance.y;
		bottomEdge += distance.y;
	}
}
