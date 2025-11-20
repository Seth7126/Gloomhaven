using Code.State;
using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.CampaignMapStates.Enhancment;

public class EnhancmentSelectCardOptionState : CampaignMapState
{
	private readonly StateMachine _stateMachine;

	private UiNavigationRoot _latestRoot;

	private IHotkeySession _hotkeySession;

	public override CampaignMapStateTag StateTag => CampaignMapStateTag.EnhancmentSelectCardOption;

	protected override string RootName => "AbilityCardTopAbilityContent";

	protected override bool SelectedFirst => true;

	private UINewEnhancementWindow EnhancementWindow => Singleton<UIGuildmasterHUD>.Instance.EnhancementWindow;

	public EnhancmentSelectCardOptionState(StateMachine stateMachine, UiNavigationManager navigationManager)
		: base(navigationManager)
	{
		_stateMachine = stateMachine;
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		if (!(payload is UiNavigationRoot latestRoot))
		{
			if (payload == null)
			{
				if (_latestRoot != null)
				{
					SwitchStateWithData();
				}
				else
				{
					DefaultNavigation();
				}
			}
			else
			{
				DefaultNavigation();
			}
		}
		else
		{
			_latestRoot = latestRoot;
			SwitchStateWithData();
		}
		NewPartyDisplayUI.PartyDisplay.TabInput.UnRegister();
		NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(value: false);
		_hotkeySession = EnhancementWindow.CardAbilityHotkeys.GetSessionOrEmpty().AddOrReplaceHotkeys("Back", "Select");
		InputManager.RegisterToOnPressed(KeyAction.UI_CANCEL, SwitchToCardSelectState);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		_hotkeySession.Dispose();
		NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(value: true);
		InputManager.UnregisterToOnPressed(KeyAction.UI_CANCEL, SwitchToCardSelectState);
	}

	private void SwitchStateWithData()
	{
		_navigationManager.SetCurrentRoot(_latestRoot, selectFirst: false);
	}

	private void SwitchToCardSelectState()
	{
		CoroutineHelper.RunNextFrame(delegate
		{
			EnhancementWindow.DeselectCurrentCard();
			_stateMachine.Enter(CampaignMapStateTag.EnhancmentCardSelect);
		});
	}
}
