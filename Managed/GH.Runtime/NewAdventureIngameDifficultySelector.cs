using MapRuleLibrary.Adventure;
using UnityEngine;

public class NewAdventureIngameDifficultySelector : MonoBehaviour
{
	[SerializeField]
	private UIDifficultySelector m_DifficultySelector;

	private void Awake()
	{
		m_DifficultySelector.DifficultySelected += OnDifficultySelected;
	}

	private void OnDestroy()
	{
		m_DifficultySelector.DifficultySelected -= OnDifficultySelected;
	}

	private void OnDifficultySelected(object sender, UIDifficultySelector.DifficultySelectedEventArgs eventArgs)
	{
		AdventureState.MapState.ChangeDifficulty(eventArgs.SelectedDifficulty);
		Singleton<MapChoreographer>.Instance.RegenerateAllMapScenarios(rerollQuestRewards: true);
		Singleton<UIQuestPopupManager>.Instance.ResetQuestPopups();
		SaveData.Instance.Global.CurrentAdventureData.Save();
	}

	private void OnDisable()
	{
		m_DifficultySelector.ClearSelection();
	}
}
