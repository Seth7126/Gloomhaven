using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;

public class TutorialService : MonoBehaviour, ITutorialService
{
	[Serializable]
	private class Tutorial : ITutorial
	{
		public string tutorialID;

		public string tutorialFileName;

		public string title;

		public string description;

		public Sprite image;

		public string TutorialID => tutorialID;

		public string TutorialFileName => tutorialFileName;

		public string TitleLocText => title;

		public string DescriptionLocText => description;

		public Sprite Image => image;
	}

	[SerializeField]
	private List<Tutorial> tutorials;

	public List<ITutorial> GetTutorials()
	{
		return tutorials.Cast<ITutorial>().ToList();
	}

	public void StartTutorial(ITutorial tutorial)
	{
		CCustomLevelData customLevel = ScenarioRuleClient.SRLYML.GetCustomLevel(tutorial.TutorialFileName);
		if (customLevel.PartySpawnType == ELevelPartyChoiceType.PresetSpawnAtEntrance || customLevel.PartySpawnType == ELevelPartyChoiceType.PresetSpawnSpecificLocations)
		{
			SaveData.Instance.LoadCustomLevelFromData(customLevel, LevelEditorController.ELevelEditorState.PreviewingFixedPartyLevel);
			SceneController.Instance.LoadCustomLevel(SaveData.Instance.Global.CurrentCustomLevelData, isFrontEndTutorial: true);
			SaveData.Instance.Global.CurrentFrontEndTutorialID = tutorial.TutorialID;
			SaveData.Instance.Global.CurrentFrontEndTutorialFilename = tutorial.TutorialFileName;
		}
		else
		{
			Debug.LogError("Attempted to load a tutorial level data without preset characters spawned in the level - this is unsupported");
		}
	}

	public bool IsTutorialComplete(ITutorial tutorial)
	{
		return SaveData.Instance.Global.CompletedTutorialIDs.Contains(tutorial.TutorialID);
	}
}
