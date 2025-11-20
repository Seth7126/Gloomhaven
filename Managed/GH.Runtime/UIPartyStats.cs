using System;
using Code.State;
using GLOOM;
using MapRuleLibrary.Party;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using Script.GUI.SMNavigation.States.PopupStates;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

public class UIPartyStats : MonoBehaviour
{
	[SerializeField]
	protected UIPerksInventory _perksInventory;

	[Header("Level")]
	[SerializeField]
	protected UITextTooltipTarget levelTooltip;

	[SerializeField]
	protected TextMeshProUGUI levelText;

	[Header("Total")]
	[SerializeField]
	protected UITextTooltipTarget totalTooltip;

	[SerializeField]
	private string _tooltipShowCombo = "L2 + R2";

	private IDisposable _onAnyButtonPressedListener;

	private float _tooltipAvailableToggleTime;

	private readonly float _tooltipToggleCooldown = 0.5f;

	private readonly IStateFilter _showStateFilter = new StateFilterByType(typeof(MapStoryState), typeof(CreateNameStepState), typeof(VisualKeyboardCharacterNameState), typeof(PersonalQuestChoiceState), typeof(AdventurePartyAssemblyRosterState), typeof(CampaignRewardState), typeof(GuildmasterRewardsState));

	protected CMapParty mapParty;

	public UITextTooltipTarget TotalTooltip => totalTooltip;

	public event Action<bool> BeforeTooltipStateChanged;

	public virtual void Setup(CMapParty mapParty)
	{
		InitInputEvents();
		this.mapParty = mapParty;
		RefreshLevel();
		base.gameObject.SetActive(value: true);
	}

	public virtual void RefreshLevel()
	{
		levelText.text = string.Format("{0} {1}", LocalizationManager.GetTranslation("GUI_LEVEL"), mapParty.PartyLevel);
		levelTooltip.SetText(string.Format(LocalizationManager.GetTranslation("GUI_PartyLevel")), refreshTooltip: true);
	}

	public virtual void RefreshTotalInfo()
	{
	}

	public virtual void ClearPartyTooltip()
	{
		if (TotalTooltip.TooltipShown)
		{
			UITooltip.ResetContent();
			TotalTooltip.TooltipShown = false;
		}
	}

	protected virtual void OnDestroy()
	{
		ClearInputEvents();
	}

	private void InitInputEvents()
	{
		Singleton<InputManager>.Instance.ButtonsComboController?.AddCombo(_tooltipShowCombo, ToggleTooltipByCombo);
		Singleton<InputManager>.Instance.ButtonsComboController?.SetEnabledCombo(_tooltipShowCombo, value: true);
	}

	private void ClearInputEvents()
	{
		ClearCloseTooltipEvents();
		if (Singleton<InputManager>.Instance != null)
		{
			Singleton<InputManager>.Instance.ButtonsComboController?.RemoveCombo(_tooltipShowCombo);
		}
	}

	private void InitCloseTooltipEvents()
	{
		InputManager instance = Singleton<InputManager>.Instance;
		instance.PreUnityMoveSelectionEvent = (Action<UINavigationDirection>)Delegate.Combine(instance.PreUnityMoveSelectionEvent, new Action<UINavigationDirection>(CloseTooltipByMoveSelection));
		_onAnyButtonPressedListener = InputSystem.onAnyButtonPress.Call(CloseTooltipByButtonPress);
	}

	private void ClearCloseTooltipEvents()
	{
		_onAnyButtonPressedListener?.Dispose();
		_onAnyButtonPressedListener = null;
		if (Singleton<InputManager>.Instance != null)
		{
			InputManager instance = Singleton<InputManager>.Instance;
			instance.PreUnityMoveSelectionEvent = (Action<UINavigationDirection>)Delegate.Remove(instance.PreUnityMoveSelectionEvent, new Action<UINavigationDirection>(CloseTooltipByMoveSelection));
		}
	}

	private void ToggleTooltipByCombo(Gamepad gamepad)
	{
		ToggleTooltipByInput();
	}

	private void CloseTooltipByMoveSelection(UINavigationDirection navigationDirection)
	{
		ToggleTooltipByInput();
	}

	private void CloseTooltipByButtonPress(InputControl control)
	{
		ToggleTooltipByInput();
	}

	private void ToggleTooltipByInput()
	{
		if (!(Time.time < _tooltipAvailableToggleTime))
		{
			_tooltipAvailableToggleTime = Time.time + _tooltipToggleCooldown;
			if (TotalTooltip.TooltipShown)
			{
				CloseTooltip();
			}
			else
			{
				ShowTooltip();
			}
		}
	}

	public void ShowTooltip()
	{
		if (!TotalTooltip.TooltipShown && NewPartyDisplayUI.PartyDisplay.Initialised && NewPartyDisplayUI.PartyDisplay.IsVisible && !Singleton<UIItemConfirmationBox>.Instance.IsActive && !_showStateFilter.IsCurrentStateValid())
		{
			if (Singleton<UINavigation>.Instance.StateMachine.IsCurrentState<CharacterCreatorClassRosterState>())
			{
				TotalTooltip.SetText(LocalizationManager.GetTranslation("GUI_STARTING_PERK_POINTS_TOOLTIP"));
			}
			else
			{
				RefreshTotalInfo();
			}
			this.BeforeTooltipStateChanged?.Invoke(obj: true);
			TotalTooltip.ToggleTooltip();
			InitCloseTooltipEvents();
		}
	}

	public void CloseTooltip()
	{
		if (TotalTooltip.TooltipShown)
		{
			this.BeforeTooltipStateChanged?.Invoke(obj: false);
			TotalTooltip.HideTooltip();
			ClearCloseTooltipEvents();
		}
	}
}
