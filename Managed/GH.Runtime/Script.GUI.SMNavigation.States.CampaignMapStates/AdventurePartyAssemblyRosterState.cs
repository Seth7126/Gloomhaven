using System;
using System.Collections.Generic;
using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class AdventurePartyAssemblyRosterState : CampaignMapState
{
	private readonly string _buttonName = "New Char Button";

	private readonly UINavigationSelectable _emptySelectable = new UINavigationSelectable();

	public override CampaignMapStateTag StateTag => CampaignMapStateTag.AdventurePartyAssemblyRoster;

	protected override bool SelectedFirst => true;

	protected override string RootName => "AdventurePartyAssemblyRoster";

	public AdventurePartyAssemblyRosterState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		IUiNavigationSelectable uiNavigationSelectable = _navigationManager.RootByName(RootName).Elements[^1] as IUiNavigationSelectable;
		if (uiNavigationSelectable == null || uiNavigationSelectable.NavigationName != _buttonName || !uiNavigationSelectable.ControlledSelectable.interactable)
		{
			uiNavigationSelectable = _emptySelectable;
		}
		_navigationManager.SetCurrentRoot(RootName, selectFirst: false, uiNavigationSelectable);
		CampaignMapStateTag latestState = stateProvider.GetLatestState<CampaignMapStateTag>(CampaignMapStateTag.Merchant, CampaignMapStateTag.Temple, CampaignMapStateTag.WorldMap);
		NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(value: true);
		if (latestState == CampaignMapStateTag.Merchant)
		{
			NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(value: true);
			Hotkeys.Instance.AddOrReplaceHotkeysForObject("AdventurePartyAssemblyRosterState", new Dictionary<string, Action> { 
			{
				"Switch_Right",
				delegate
				{
					NewPartyDisplayUI.PartyDisplay.CharacterSelector.Hide();
					Singleton<UIGuildmasterHUD>.Instance.ShopInputArea.Focus();
				}
			} });
		}
		Singleton<UIReadyToggle>.Instance.BlockVisibility(this);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		base.Exit(stateProvider, payload);
		Hotkeys.Instance.RemoveHotkeysForObject("AdventurePartyAssemblyRosterState");
		NewPartyDisplayUI.PartyDisplay?.CloseTooltip();
		Singleton<UIReadyToggle>.Instance.UnblockVisibility(this);
	}
}
