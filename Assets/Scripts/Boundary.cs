using UnityEngine;
using System.Collections;

public class Boundary : MonoBehaviour {

	public enum BoundaryType {Screen, World};
	public enum BoundaryLocation {Left, Right, Top, Bottom};

	public BoundaryType boundaryType;
	public BoundaryLocation boundaryLocation;


	private BoxCollider2D bc2D;

	// Use this for initialization
	void Start () {
		bc2D = gameObject.GetComponent<BoxCollider2D> ();
		if (bc2D == null)
			bc2D = gameObject.AddComponent<BoxCollider2D> ();

		float y = Screen.height;

		if (boundaryLocation == BoundaryLocation.Left || boundaryLocation == BoundaryLocation.Right) {
			//set box collider size based on screen dimensions
			float height = (Camera.main.ScreenToWorldPoint (new Vector3 (0, Screen.height)) - Camera.main.ScreenToWorldPoint (Vector3.zero)).y;;

			if (boundaryType == BoundaryType.World)
				height *= 2;
			
			bc2D.size = new Vector2 (0.2f, height);

			float x = Screen.width;

			if (boundaryLocation == BoundaryLocation.Left) {
				if (boundaryType == BoundaryType.Screen) {
					x = 0;
					y /= 2f;
				} else {
					x *= -0.5f;
				}
				//set position to left of screen
				transform.position = Camera.main.ScreenToWorldPoint(new Vector3(x, y));
			} else {
				if (boundaryType == BoundaryType.Screen) {
					y /= 2f;
				} else {
					x *= 1.5f;
				}
				//set position to right of screen
				transform.position = Camera.main.ScreenToWorldPoint(new Vector3(x, y));
			}
		} else {
			//set box collider size based on screen dimensions
			float width = (Camera.main.ScreenToWorldPoint (new Vector3(Screen.width, 0)) - Camera.main.ScreenToWorldPoint (Vector3.zero)).x;

			if (boundaryType == BoundaryType.World)
				width *= 2;
			
			bc2D.size = new Vector2 (width, 0.7f);

			if (boundaryType == BoundaryType.World)
				y *= 2f;

			if (boundaryLocation == BoundaryLocation.Bottom) {
				//set position to top of screen
				transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, 0f));
			} else {
				//set position to bottom of screen
				transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, y));
			}
		}
	}
}
