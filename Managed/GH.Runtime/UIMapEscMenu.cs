using System.Collections;
using System.Linq;
using Code.State;
using GLOOM;
using MapRuleLibrary.Adventure;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates.Enhancment;
using Script.GUI.SMNavigation.States.MainMenuStates;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIMapEscMenu : ESCMenu
{
	[SerializeField]
	protected UIMainMenuOption turnOffTutorialButton;

	[SerializeField]
	protected GameObject _divisor;

	private Coroutine multiplayerButtonCoroutine;

	private IState _prevState;

	private IStateFilter _previousStateFilter = new StateFilterByType(typeof(GamepadDisconnectionBoxState), typeof(EnhancmentSelectCardOptionState), typeof(EnhancmentSelectOptionUpgradeState)).InverseFilter();

	private IStateFilter _previousStateFilterWithConfirmation = new StateFilterByType(typeof(GamepadDisconnectionBoxState), typeof(EnhancmentSelectCardOptionState), typeof(EnhancmentSelectOptionUpgradeState), typeof(EnhancmentConfirmationState)).InverseFilter();

	private bool _wasPartyTabInputRegistered;

	protected override void Awake()
	{
		base.Awake();
		turnOffTutorialButton.Init(TurnOffTutorial, delegate
		{
			if (controllerArea.IsFocused)
			{
				EventSystem.current.SetSelectedGameObject(turnOffTutorialButton.gameObject);
			}
		});
	}

	protected void OnEnable()
	{
		controllerArea.OnFocused.AddListener(SwitchToEscMenuState);
	}

	protected void OnDisable()
	{
		controllerArea.OnFocused.RemoveListener(SwitchToEscMenuState);
	}

	protected override void SetFocused(bool isFocused)
	{
		base.SetFocused(isFocused);
		turnOffTutorialButton.SetFocused(isFocused);
	}

	protected override void OnShow()
	{
		Singleton<UINavigation>.Instance.StateMachine.SetFilter(new StateFilterByTagType<MainStateTag>());
		StopMultiplayerButtonCorutine();
		base.OnShow();
		if (Singleton<UILoadoutManager>.Instance != null)
		{
			if (multiplayerButton.IsInteractable && !Singleton<MapChoreographer>.Instance.PartyAtHQ)
			{
				DisableMultiplayerOptionInLoadout();
			}
			else
			{
				StartTrackMultiplayerButtonState();
			}
			Singleton<UILoadoutManager>.Instance.OnOpen.RemoveListener(DisableMultiplayerOptionInLoadout);
			Singleton<UILoadoutManager>.Instance.OnOpen.AddListener(DisableMultiplayerOptionInLoadout);
		}
		if (MapFTUEManager.IsPlaying)
		{
			turnOffTutorialButton.gameObject.SetActive(value: true);
			_divisor.SetActive(value: true);
			Singleton<MapFTUEManager>.Instance.OnFinished.AddListener(OnFinishedTutorial);
		}
		else
		{
			turnOffTutorialButton.gameObject.SetActive(value: false);
			_divisor.SetActive(value: false);
		}
		_wasPartyTabInputRegistered = NewPartyDisplayUI.PartyDisplay.TabInput.IsRegistered;
		if (_wasPartyTabInputRegistered)
		{
			NewPartyDisplayUI.PartyDisplay.TabInput.UnRegister();
		}
	}

	private void SwitchToEscMenuState()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.MapEscMenu);
	}

	private void OnFinishedTutorial()
	{
		turnOffTutorialButton.gameObject.SetActive(value: false);
		_divisor.SetActive(value: false);
		Singleton<MapFTUEManager>.Instance.OnFinished.RemoveListener(OnFinishedTutorial);
	}

	protected override void OnHide()
	{
		Singleton<UINavigation>.Instance.StateMachine.RemoveFilter();
		if (Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<MapEscMenuState>())
		{
			IStateFilter filter = ((Singleton<UINavigation>.Instance.StateMachine.PreviousState is EnhancmentConfirmationState) ? _previousStateFilter : _previousStateFilterWithConfirmation);
			Singleton<UINavigation>.Instance.StateMachine.ToNonMenuPreviousState(filter, useSimplifiedFilter: false);
		}
		base.OnHide();
		StopMultiplayerButtonCorutine();
		if (Singleton<UILoadoutManager>.Instance != null)
		{
			Singleton<UILoadoutManager>.Instance.OnOpen.RemoveListener(DisableMultiplayerOptionInLoadout);
		}
		if (MapFTUEManager.IsPlaying)
		{
			Singleton<MapFTUEManager>.Instance.OnFinished.RemoveListener(OnFinishedTutorial);
		}
		NewPartyDisplayUI partyDisplay = NewPartyDisplayUI.PartyDisplay;
		if (_wasPartyTabInputRegistered && partyDisplay != null)
		{
			partyDisplay.TabInput.Register();
		}
	}

	private void DisableMultiplayerOptionInLoadout()
	{
		multiplayerButton.IsInteractable = false;
	}

	private void StartTrackMultiplayerButtonState()
	{
		if (multiplayerButtonCoroutine == null)
		{
			multiplayerButtonCoroutine = StartCoroutine(CheckMultiplayerButtonState());
		}
	}

	private void StopMultiplayerButtonCorutine()
	{
		if (multiplayerButtonCoroutine != null)
		{
			StopCoroutine(multiplayerButtonCoroutine);
		}
		multiplayerButtonCoroutine = null;
	}

	private IEnumerator CheckMultiplayerButtonState()
	{
		do
		{
			RefreshMultiplayerButton();
			yield return null;
		}
		while (Singleton<MapChoreographer>.Instance.PartyAtHQ);
		multiplayerButtonCoroutine = null;
	}

	protected override bool CheckMultiplayerButton(out string tooltip)
	{
		if (!base.CheckMultiplayerButton(out tooltip))
		{
			return false;
		}
		if (!Singleton<MapChoreographer>.Instance.PartyAtHQ)
		{
			tooltip = LocalizationManager.GetTranslation("GUI_MULTIPLAYER_LOCKED_PARTY_NOT_HQ_TOOLTIP");
			return false;
		}
		tooltip = null;
		if (FFSNetwork.IsOnline && Singleton<UIMapMultiplayerController>.Instance != null && (Singleton<UIMapMultiplayerController>.Instance.IsReadyRewards || Singleton<UIMapMultiplayerController>.Instance.IsReadyTownRecords))
		{
			return false;
		}
		if (!FFSNetwork.IsOnline && Singleton<UIMapMultiplayerController>.Instance != null && Singleton<UILevelUpWindow>.Instance.IsLevelingUp())
		{
			return false;
		}
		if (!FFSNetwork.IsOnline && Singleton<UIEventPanel>.Instance != null && Singleton<UIEventPanel>.Instance.IsOpen)
		{
			return false;
		}
		if (!FFSNetwork.IsOnline && Singleton<UIDistributeRewardManager>.Instance != null && Singleton<UIDistributeRewardManager>.Instance.IsDistributing)
		{
			return false;
		}
		if (AdventureState.MapState.MapParty.ExistsCharacterToRetire() || Singleton<MapChoreographer>.Instance.QueuedRetirements.Any())
		{
			return false;
		}
		return true;
	}

	private void TurnOffTutorial()
	{
		UIConfirmationBoxManager.MainMenuInstance.ShowGenericWarningConfirmation(LocalizationManager.GetTranslation("GUI_TURN_OFF_TUTORIAL"), LocalizationManager.GetTranslation("GUI_CONFIRMATION_TURN_OFF_TUTORIAL"), delegate
		{
			Singleton<MapFTUEManager>.Instance.Finish();
			Hide();
		}, turnOffTutorialButton.Deselect);
	}
}
