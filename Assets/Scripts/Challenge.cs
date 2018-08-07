//written by Justin Ortiz
using UnityEngine;
using UnityEngine.UI;

//Challenge Format:         [Chapter]_[Mission]_[Requirement Type]_[final parameter]

public class Challenge : MonoBehaviour  {
    private static string[] requirementTypeKeywords = { "Difficulty", "Item", "Time", "Enemy" };

    [SerializeField, Tooltip("Challenge description displayed for the player to see.")]
	private string description;
    [SerializeField, Tooltip("Format: [Chapter]_[Mission]_[Requirement Type]_[final parameter]")]
    private string requirement;
    private int requirementType;
    [SerializeField, Tooltip("Formats: 'Currency_[int amount]' && '[unlock name]'")]
    private string reward;
    private bool complete;
    private Transform challengeCompleteParent;

    public string Name { get { return gameObject.name; } }
    public string Description { get { return description; } }
    public string Reward { get { return reward; } }
    public bool Complete { get { return complete; } }
    
    public bool CheckRequirementMet(string actionPerformed) { // Checks to see if the challenge requirement has been met
        if (requirementType == -1) { //no valid requirement present
            return false;
        }

        int actionType = GetRequirementType(actionPerformed); //get the type of action

        if (actionType == requirementType) { //if the action is the same type as the requirement
            string[] actionInfo = actionPerformed.Split('_');
            string[] requirementInfo = requirement.Split('_'); //split the info -- chapter, mission, req type, etc.

            if (actionInfo.Length != requirementInfo.Length) { //typo in script or inspector, so they don't have same amount of parameters
                return false;
            }

            for (int i = 0; i < actionInfo.Length - 1; i++) { //go through all except last piece of info -- last piece may be higher
                if (!actionInfo[i].Equals(requirementInfo[i])) { //if any of info does not match
                    return false; //return false
                }
            }

            if (requirementType == 0 || requirementType == 1) { //difficulty challenge or item collection challenge
                int actionDifficulty, requirementDifficulty; //declare the difficulties
                if (int.TryParse(actionInfo[actionInfo.Length - 1], out actionDifficulty)) { // try to get the action difficulty
                    if (int.TryParse(requirementInfo[requirementInfo.Length - 1], out requirementDifficulty)) { //try to get the requirement difficulty
                        if (actionDifficulty >= requirementDifficulty) { //see if it is equal or greater
                            return true;
                        }
                    }
                }
            } else {
                if (actionPerformed.Equals(requirement)) { //see if the action is identical to requirement
                    return true;
                }
            }
        }
        return false;
    }

    private int GetRequirementType(string reqString) {
        for (int i = 0; i < requirementTypeKeywords.Length; i++) { //go through challenge keywords
            if (reqString.Contains(requirementTypeKeywords[i])) { //see what type of challenge it is
                return i; //return the type
            }
        }
        return -1; //no type found
    }

    public void MarkComplete() {
        if (challengeCompleteParent != null)
            challengeCompleteParent.gameObject.SetActive(true);

        complete = true;
    }

    void Start() {
        challengeCompleteParent = transform.Find("Challenge Complete");
        transform.Find("Challenge Name").GetComponent<Text>().text = Name;
        transform.Find("Challenge Description").GetComponent<Text>().text = description;

        requirementType = GetRequirementType(requirement);
    }
}