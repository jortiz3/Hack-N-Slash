//Written by Justin Ortiz

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlatformEffector2D))]
public class DropthroughPlatform : MonoBehaviour {

	public void DropThrough(GameObject objectDroppingThrough) {
		StartCoroutine(DropThroughCoroutine(objectDroppingThrough));
	}

	private IEnumerator DropThroughCoroutine(GameObject objectDroppingThrough) {
		Collider2D[] temp = objectDroppingThrough.GetComponents<Collider2D>();
		foreach (Collider2D c2D in temp) {
			c2D.enabled = false;
		}
		yield return new WaitForSeconds(1);
		foreach (Collider2D c2D in temp) {
			c2D.enabled = true;
		}
	}
}
