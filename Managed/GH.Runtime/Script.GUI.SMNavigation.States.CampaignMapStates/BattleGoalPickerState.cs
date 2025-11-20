using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class BattleGoalPickerState : CampaignMapState
{
	public override CampaignMapStateTag StateTag => CampaignMapStateTag.BattleGoalPicker;

	protected override bool SelectedFirst => false;

	protected override string RootName => "BattleGoalPicker";

	protected CampaignMapStateTag[] TagsFilter { get; }

	public BattleGoalPickerState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
		TagsFilter = new CampaignMapStateTag[3]
		{
			CampaignMapStateTag.Merchant,
			CampaignMapStateTag.Temple,
			CampaignMapStateTag.WorldMap
		};
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		FocusSlot(payload as UIBattleGoalPickerSlot);
		Singleton<UIQuestPopupManager>.Instance.SetSwitchNavigation(canSwitch: true);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		base.Exit(stateProvider, payload);
		Hotkeys.Instance.RemoveHotkeysForObject("BattleGoalPickerState");
		Singleton<UIQuestPopupManager>.Instance.SetSwitchNavigation(canSwitch: false);
	}

	public void FocusSlot(UIBattleGoalPickerSlot slot)
	{
		DefaultNavigation();
		if (slot != null)
		{
			_navigationManager.TrySelect(slot.GetComponent<IUiNavigationSelectable>());
		}
	}

	private CampaignMapStateTag GetPreviousStateTag(IStateProvider stateProvider, CampaignMapStateTag[] tagsFilter)
	{
		return stateProvider.GetLatestState(tagsFilter);
	}
}
