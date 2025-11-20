using System;
using MapRuleLibrary.Party;
using MapRuleLibrary.YML.PersonalQuests;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterPersonalQuest : MonoBehaviour
{
	[SerializeField]
	protected UIPersonalQuestProgress personalQuest;

	[SerializeField]
	protected Image characterShield;

	[SerializeField]
	protected UIPersonalQuestProgressiveObjective progressiveObjective;

	public virtual void SetPersonalQuest(CMapCharacter character, Action onFinishedProgress = null)
	{
		personalQuest.SetPersonalQuest(character.PersonalQuest, character.PersonalQuest.LastProgressShown, onFinishedProgress);
		characterShield.sprite = UIInfoTools.Instance.GetCharacterMarker(character.CharacterYMLData);
		progressiveObjective.SetPersonalQuest(character.PersonalQuest);
	}

	public virtual void SetPersonalQuest(CMapCharacter character, PersonalQuestYMLData personalQuestData, int currentProgress, int totalProgress, int currentStep, Action onFinishedProgress = null)
	{
		personalQuest.SetPersonalQuest(personalQuestData, character.PersonalQuest.LastProgressShown, currentProgress, totalProgress, onFinishedProgress);
		characterShield.sprite = UIInfoTools.Instance.GetCharacterMarker(character.CharacterYMLData);
		progressiveObjective.SetPersonalQuestStep(character.PersonalQuest, currentStep);
	}
}
