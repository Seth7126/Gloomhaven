using MapRuleLibrary.Party;
using Script.GUI;
using UnityEngine;

public class TrainerWindowHotkeysHandler : MonoBehaviour
{
	[SerializeField]
	private PanelHotkeyContainer _panelHotkeyContainer;

	[SerializeField]
	private UITrainerWindow _uiTrainerWindow;

	[SerializeField]
	private UIAchievementInventory _uiAchievementInventory;

	private void Awake()
	{
		_panelHotkeyContainer.ToggleActiveAllHotkeys(value: false);
		_panelHotkeyContainer.SetActiveHotkey("Back", value: true);
		_uiAchievementInventory.OnHoveredAchievement += UiAchievementInventoryOnOnHoveredAchievement;
	}

	private void OnDestroy()
	{
		_uiAchievementInventory.OnHoveredAchievement -= UiAchievementInventoryOnOnHoveredAchievement;
	}

	private void UiAchievementInventoryOnOnHoveredAchievement(CPartyAchievement achievement)
	{
		if (!FFSNetwork.IsOnline || !FFSNetwork.IsClient)
		{
			bool value = achievement.State == EAchievementState.Completed;
			_panelHotkeyContainer.SetActiveHotkey("ClaimReward", value);
		}
	}
}
