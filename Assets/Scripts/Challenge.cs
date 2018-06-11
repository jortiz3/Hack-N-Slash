using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Challenge : MonoBehaviour {

	private string name;
	private string description;
	private bool complete;

	public bool isComplete { get { return complete; } }

	public Challenge(string Name, string Description) {
		name = Name;
		description = Description;
		complete = false; //load?
	}

	public bool CompleteChallenge() {
		if (!complete) {
			complete = true;
			return true;
		}
		return false;
	}
}