using GLOOM;
using ScenarioRuleLibrary;
using UnityEngine.UI;

public class ControllerInputAbilityCardsArea : ControllerInputEscapableArea
{
	public override void Focus()
	{
		UIWindowManager.RegisterEscapable(this);
		if (PhaseManager.CurrentPhase != null)
		{
			if (PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				InitiativeTrack.Instance.CheckRoundAbilityCardsOrLongRestSelected();
			}
			else if (PhaseManager.CurrentPhase.Type == CPhase.PhaseType.ActionSelection)
			{
				if (GameState.InternalCurrentActor != null && GameState.InternalCurrentActor.Class is CCharacterClass cCharacterClass && (cCharacterClass.LongRest || cCharacterClass.HasLongRested))
				{
					InitiativeTrack.Instance.helpBox.ClearOverrideController();
				}
				else if (GameState.CurrentActionSelectionSequence == GameState.ActionSelectionSequenceType.FirstAction)
				{
					InitiativeTrack.Instance.helpBox.OverrideControllerOrKeyboardTip(() => string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_GAMEPAD_SELECT_FIRST_ACTION"), Singleton<InputManager>.Instance.GetGamepadActionIcon(KeyAction.UI_SUBMIT)));
				}
				else if (GameState.CurrentActionSelectionSequence == GameState.ActionSelectionSequenceType.SecondAction)
				{
					InitiativeTrack.Instance.helpBox.OverrideControllerOrKeyboardTip(() => string.Format(LocalizationManager.GetTranslation("GUI_TOOLTIP_GAMEPAD_SELECT_SECOND_ACTION"), Singleton<InputManager>.Instance.GetGamepadActionIcon(KeyAction.UI_SUBMIT)));
				}
				else
				{
					InitiativeTrack.Instance.helpBox.ClearOverrideController();
				}
			}
		}
		base.Focus();
	}

	public override void Unfocus()
	{
		if (PhaseManager.CurrentPhase != null && (PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest || PhaseManager.CurrentPhase.Type == CPhase.PhaseType.ActionSelection))
		{
			InitiativeTrack.Instance.helpBox.ClearOverrideController();
		}
		base.Unfocus();
	}

	public override void DisableGroup()
	{
		UIWindowManager.UnregisterEscapable(this);
		base.DisableGroup();
		if (ControllerInputAreaManager.IsFocusedArea(base.Id))
		{
			ControllerInputAreaManager.Instance.ResetFocusedArea();
		}
	}
}
