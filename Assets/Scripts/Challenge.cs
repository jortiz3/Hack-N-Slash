//written by Justin Ortiz
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//Challenge Format:         [chapter]_[mission]_[requirement type]:[parameter]_[requirement type]:[parameter]_...

public class Challenge : MonoBehaviour {
    private static string[] requirementTypeKeywords = { "difficulty", "item", "time", "enemy", "outfit", "weapon", "winstreak" }; //Time is in seconds

    [SerializeField, Tooltip("Challenge description displayed for the player to see.")]
    private string description;
    [SerializeField, Tooltip("Format: [chapter]_[mission]_[requirement type]:[parameter]_[requirement type]:[parameter]_...")]
    private string requirement;
    private string[] requirementInfo;
    private int[] requirementTypes;
    [SerializeField, Tooltip("Formats: 'currency_[int amount]' && '[unlock name]'")]
    private string reward;
    private bool complete;
    private Transform challengeCompleteParent;
    [SerializeField]
    private Sprite notificationSprite;
    [SerializeField]
    private Color notificationColor;

    public string Name { get { return gameObject.name; } }
    public string Description { get { return description; } }
    public string Reward { get { return reward; } }
    public bool Complete { get { return complete; } }
    public Sprite NotificationSprite { get { return notificationSprite; } }
    public Color NotificationColor { get { return notificationColor; } }

    public bool CheckRequirementMet(string actionPerformed) { // Checks to see if the challenge requirement has been met
        if (requirementTypes == null) { //no valid requirement present
            return false;
        }

        string[] actionInfo;
        int[] actionTypes = GetRequirementTypes(actionPerformed, out actionInfo); //get the requirement types within action

        for (int i = 0; i < 2; i++) { //check the chapter and mission to verify they are the same
            if (!actionInfo[i].Equals(requirementInfo[i])) { //if any of info does not match
                return false; //return false
            }
        }

        bool requirementMet;
        for (int currRequirement = 0; currRequirement < requirementTypes.Length; currRequirement++) { //go through each requirement
            requirementMet = false; //initialize as false
            for (int currAction = 0; currAction < actionTypes.Length; currAction++) { //go through all actions
                if (requirementTypes[currRequirement] == -1) { //if one of the requirements aren't valid
                    return false; //fail the check
                }

                if (actionTypes[currAction] == requirementTypes[currRequirement]) { //if the action is the same type as the requirement
                    if (requirementTypes[currRequirement] == 0 || requirementTypes[currRequirement] == 1 ||
                        requirementTypes[currRequirement] == 3 || requirementTypes[currRequirement] == 6) { //difficulty challenge or item collection challenge or enemy challenge or winstreak
                        int actionParameter, requirementParameter; //declare the parameters
                        if (int.TryParse(actionInfo[currAction + 2].Split(':')[1], out actionParameter)) { // try to get the action difficulty/item qty/enemy qty
                            if (int.TryParse(requirementInfo[currRequirement + 2].Split(':')[1], out requirementParameter)) { //try to get the requirement parameter
                                if (actionParameter >= requirementParameter) { //see if it is equal or greater
                                    requirementMet = true;
                                    break;
                                }
                            }
                        } else if (actionInfo[currAction + 2].Equals(requirementInfo[currRequirement + 2])) { //see if the action is identical to requirement -- i.e. ..._Enemy_[Name]
                            requirementMet = true;
                            break;
                        }
                    } else if (requirementTypes[currRequirement] == 2) { //time challenge
                        float actionTime, requirementTime; //declare the times
                        if (float.TryParse(actionInfo[currAction + 2].Split(':')[1], out actionTime)) { //try to get the time spent on level
                            if (float.TryParse(requirementInfo[currRequirement + 2].Split(':')[1], out requirementTime)) { //try to get the time required to complete challenge
                                if (actionTime <= requirementTime) { //see if it is less than or equal to
                                    requirementMet = true;
                                    break;
                                }
                            }
                        }
                    } else { //undefined keyword
                        if (actionInfo[currAction + 2].Equals(requirementInfo[currRequirement + 2])) { //see if the action is identical to requirement
                            requirementMet = true;
                            break;
                        }
                    }
                }
            }

            if (!requirementMet) { //if any of the requirements aren't met, the challenge isn't complete
                return false;
            }
        }
        return true; //if we make it through all the requirements as met, then the challenge is complete
    }

    private int[] GetRequirementTypes(string reqString, out string[] reqSplitInfo) {
        reqSplitInfo = reqString.Split('_');
        List<int> reqTypes = new List<int>();
        bool keywordFound = false;

        for (int currReqInfo = 2; currReqInfo < reqSplitInfo.Length; currReqInfo++) { //start on the 3rd piece of info -- skip chapter and mission to save time
            for (int reqTypeKeyword = 0; reqTypeKeyword < requirementTypeKeywords.Length; reqTypeKeyword++) {
                if (reqSplitInfo[currReqInfo].Contains(requirementTypeKeywords[reqTypeKeyword])) {
                    reqTypes.Add(reqTypeKeyword);
                    keywordFound = true;
                    break;
                }
            }
            if (!keywordFound) {
                reqTypes.Add(-1);
            }
        }

        return reqTypes.ToArray();
    }

    public void MarkComplete() {
        if (challengeCompleteParent != null)
            challengeCompleteParent.gameObject.SetActive(true);

        complete = true;
    }

    void Start() {
        challengeCompleteParent = transform.Find("Challenge Complete");
        transform.Find("Challenge Name").GetComponent<Text>().text = Name;
        transform.Find("Challenge Description").GetComponent<Text>().text = System.Text.RegularExpressions.Regex.Unescape(description); //unescape turns \n into the utf-8 control character so it will actually work

        requirementTypes = GetRequirementTypes(requirement, out requirementInfo);
    }
}