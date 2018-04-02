using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScript : MonoBehaviour {

	public string startState = "Main";
	[HideInInspector]
	public string currentState = "";

	private string previousState = "";

	[Tooltip ("Only hidden when a focus menu is displayed (i.e. HUD).")]
	public string[] overlays;

	[Tooltip ("No overlays may be displayed when one of these menus are displayed.")]
	public string[] focusMenus;

	private CanvasGroup[] groups;

	void Start () {
		groups = new CanvasGroup[transform.childCount];
		for (int i = 0; i < groups.Length; i++) {
			if (transform.GetChild (i).GetComponent<CanvasGroup> () != null)
				groups [i] = transform.GetChild (i).GetComponent<CanvasGroup> ();
		}
		ChangeState (startState);
	}

	public void ChangeState (string state) { //changes which menu is displayed (i.e. Main -> Settings, Main -> About)

		previousState = currentState;

		if (currentState.Equals (state)) //if we are already in this state, then we will exit the state
			currentState = "";
		else
			currentState = state;

		bool currStateIsFocus = false;
		for (int f = 0; f < focusMenus.Length; f++) { //loop through focus menus to see if the state we desire needs to be the only thing displayed
			if (currentState.Equals (focusMenus [f])) {
				currStateIsFocus = true;
				break;
			}
		}

		string currGroupName = "";
		bool currGroupIsOverlay;
		for (int i = 0; i < groups.Length; i++) {
			if (groups [i] != null) {
				currGroupName = groups [i].transform.name;

				currGroupIsOverlay = false;
				for (int o = 0; o < overlays.Length; o++) { //determine whether the current canvasgroup is an overlay
					if (currGroupName.Equals (overlays [o])) {
						currGroupIsOverlay = true;
						break;
					}
				}

				if (currGroupName.Equals (currentState) || (!currStateIsFocus && currGroupIsOverlay)) { //desired state? desired state a focus? overlay?
					groups [i].alpha = 1; //show group and make it interactable
					groups [i].interactable = true;
					groups [i].blocksRaycasts = true;

					if (groups [i].GetComponent<MenuScript> () != null) //if there is a sub-menu, refresh it
						groups [i].GetComponent<MenuScript> ().RefreshState ();
				} else { //if it doesn't need to be shown
					groups [i].alpha = 0; //hide group and make sure we cannot interact
					groups [i].interactable = false;
					groups [i].blocksRaycasts = false;
				}
			}
		}
	}

	public void ReturnToPreviousState() {
		ChangeState (previousState);
	}

	public void RefreshState () {
		string temp = currentState;
		currentState = "";
		ChangeState (temp);
	}
}
