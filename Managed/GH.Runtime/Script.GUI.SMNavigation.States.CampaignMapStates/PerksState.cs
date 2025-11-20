using System;
using System.Collections.Generic;
using Code.State;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class PerksState : CampaignMapState
{
	public override CampaignMapStateTag StateTag => CampaignMapStateTag.Perks;

	protected override bool SelectedFirst => true;

	protected override string RootName => "Perks";

	protected CampaignMapStateTag[] TagsFilter { get; }

	public PerksState(UiNavigationManager navigationManager)
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
		FirstOrCurrentNavigation();
		NewPartyDisplayUI.PartyDisplay.TabInput.Register();
		NewPartyDisplayUI.PartyDisplay.ClearNewEquippedItemsWithModifiers();
		CampaignMapStateTag previousStateTag = GetPreviousStateTag(stateProvider, TagsFilter);
		SetHotKeys(previousStateTag);
		NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(value: true);
		NewPartyDisplayUI.PartyDisplay.PerkManager.PerkDisplay.EnableNavigation();
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		base.Exit(stateProvider, payload);
		Hotkeys.Instance.RemoveHotkeysForObject("PerksState");
		NewPartyDisplayUI.PartyDisplay?.CloseTooltip();
		NewPartyDisplayUI.PartyDisplay?.PerkManager.PerkDisplay.DisableNavigation();
	}

	private void SetHotKeys(CampaignMapStateTag previousStateTag)
	{
		Dictionary<string, Action> hotKeys = GetHotKeys(previousStateTag);
		Hotkeys.Instance.AddOrReplaceHotkeysForObject("PerksState", hotKeys);
	}

	private CampaignMapStateTag GetPreviousStateTag(IStateProvider stateProvider, CampaignMapStateTag[] tagsFilter)
	{
		return stateProvider.GetLatestState(tagsFilter);
	}

	private Dictionary<string, Action> GetHotKeys(CampaignMapStateTag previousStateTag)
	{
		_ = 24;
		return new Dictionary<string, Action>();
	}
}
