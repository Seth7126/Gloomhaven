using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using UnityEngine;

public class InitiativeTrackPlayerAvatar : InitiativeTrackActorAvatar
{
	[SerializeField]
	private GameObject _hotkeyContainer;

	[SerializeField]
	private Hotkey[] _hotkeys;

	private bool isSelectableByClick;

	public override CActor.EType ActorType()
	{
		return CActor.EType.Player;
	}

	public override void Select()
	{
		base.Select();
		if (m_Actor != null && (Choreographer.s_Choreographer.LastMessage == null || (Choreographer.s_Choreographer.LastMessage.m_Type != CMessageData.MessageType.StartTurn && Choreographer.s_Choreographer.LastMessage.m_Type != CMessageData.MessageType.ActionSelectionPhaseStart && Choreographer.s_Choreographer.LastMessage.m_Type != CMessageData.MessageType.CheckForInitiativeAdjustments)) && (CardsHandManager.Instance.IsShown || CardsHandManager.Instance.CurrentHand?.PlayerActor != m_Actor) && !Singleton<AbilityEffectManager>.Instance.IsControlAbilityAffectingActor(m_Actor))
		{
			CardsHandManager.Instance.SwitchHand((CPlayerActor)m_Actor);
		}
	}

	public override void OnClick(InitiativeTrackActorBehaviour actorUI)
	{
		if (!InputManager.GamePadInUse)
		{
			bool num = PhaseManager.CurrentPhase.Type != CPhase.PhaseType.SelectAbilityCardsOrLongRest || isSelected;
			if (isSelectableByClick)
			{
				base.OnClick(actorUI);
			}
			if (num)
			{
				CardsHandManager.Instance.ToggleViewAllCards(actorUI.Actor as CPlayerActor);
			}
		}
	}

	private void OnEnable()
	{
		_hotkeyContainer?.SetActive(InputManager.GamePadInUse);
		for (int i = 0; i < _hotkeys.Length; i++)
		{
			_hotkeys[i].Initialize(Singleton<UINavigation>.Instance.Input);
		}
	}

	private void OnDisable()
	{
		for (int i = 0; i < _hotkeys.Length; i++)
		{
			_hotkeys[i].Deinitialize();
		}
	}

	public override void Deselect()
	{
		base.Deselect();
	}

	public override void ShowDetails(bool active)
	{
		base.ShowDetails(active);
	}

	public void SwapInitiative()
	{
		if (PhaseManager.PhaseType != CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			return;
		}
		CPlayerActor cPlayerActor = (CPlayerActor)m_Actor;
		if (cPlayerActor.CharacterClass.RoundAbilityCards.Count == 2)
		{
			if (cPlayerActor.CharacterClass.InitiativeAbilityCard == cPlayerActor.CharacterClass.RoundAbilityCards[0])
			{
				cPlayerActor.CharacterClass.SetInitiativeAbilityCard(cPlayerActor.CharacterClass.RoundAbilityCards[1]);
				cPlayerActor.CharacterClass.SetSubInitiativeAbilityCard(cPlayerActor.CharacterClass.RoundAbilityCards[0]);
			}
			else
			{
				cPlayerActor.CharacterClass.SetInitiativeAbilityCard(cPlayerActor.CharacterClass.RoundAbilityCards[0]);
				cPlayerActor.CharacterClass.SetSubInitiativeAbilityCard(cPlayerActor.CharacterClass.RoundAbilityCards[1]);
			}
			cPlayerActor.CharacterClass.RoundAbilityCards.Reverse();
			InitiativeTrack.Instance.UpdateActors();
			CardsHandManager.Instance.GetHand(cPlayerActor).NetworkSelectedRoundCards();
		}
	}

	public override void SetAttributesDirect(CActor actor, bool activeIDBackplate, bool activeID, string textID, bool activeInitiative, int initiative, int currentHealth, int maxHealth, bool isActive, bool activeHilight, bool activeButton)
	{
		isSelectableByClick = activeButton;
		base.SetAttributesDirect(actor, activeIDBackplate, activeID, textID, activeInitiative, initiative, currentHealth, maxHealth, isActive, activeHilight, activeButton: true);
		RefreshAbilities();
	}

	protected override string CalculateInitiative(CActor actor)
	{
		if (FFSNetwork.IsOnline && PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest && !actor.IsUnderMyControl)
		{
			return "?";
		}
		return base.CalculateInitiative(actor);
	}

	public void OnControllerFocused()
	{
		CardsHandManager.Instance.EnableAllCardsCombo(value: true);
	}

	public void OnControllerUnfocused()
	{
		CardsHandManager.Instance.EnableAllCardsCombo(value: false);
	}
}
