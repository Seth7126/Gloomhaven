using SM.Gamepad;

namespace Script.GUI.SMNavigation.States.ScenarioStates;

public class CardSelectHotkeys
{
	private readonly SelectHotkeys _selectHotkeys = new SelectHotkeys();

	public bool HideOnFullCardFocus { get; set; }

	public bool CheckSelectable { get; set; }

	public bool CheckHovered { get; set; }

	public bool UpdateOnHover { get; set; }

	public bool CheckSelectInScenario { get; set; }

	public CardSelectHotkeys(bool checkSelectable = true, bool updateOnHover = true, bool checkSelectInScenario = true, bool checkHovered = true, bool hideOnFullCardFocus = true)
	{
		HideOnFullCardFocus = hideOnFullCardFocus;
		CheckHovered = checkHovered;
		CheckSelectable = checkSelectable;
		UpdateOnHover = updateOnHover;
		CheckSelectInScenario = checkSelectInScenario;
	}

	public void Enter(IHotkeySession session)
	{
		_selectHotkeys.Enter(session);
		if (UpdateOnHover)
		{
			AbilityCardUI.CardHoveringStateChanged += AbilityCardUIOnCardHoveringStateChanged;
		}
		ShortRest.OnSelectShortRest += UpdateSelectShortRest;
		ShortRest.OnUnselectShortRest += UpdateSelectShortRest;
		if (HideOnFullCardFocus)
		{
			FullAbilityCard.OnEnterForView += OnViewFullCard;
		}
		AbilityCardUI.CardSelectionStateChanged += AbilityCardUIOnCardSelectionStateChanged;
	}

	public void Exit()
	{
		AbilityCardUI.CardHoveringStateChanged -= AbilityCardUIOnCardHoveringStateChanged;
		ShortRest.OnSelectShortRest -= UpdateSelectShortRest;
		ShortRest.OnUnselectShortRest -= UpdateSelectShortRest;
		FullAbilityCard.OnEnterForView -= OnViewFullCard;
		AbilityCardUI.CardSelectionStateChanged -= AbilityCardUIOnCardSelectionStateChanged;
	}

	private void AbilityCardUIOnCardSelectionStateChanged(AbilityCardUI card, bool _)
	{
		SetSelectHotkeys(card);
	}

	private void AbilityCardUIOnCardHoveringStateChanged(AbilityCardUI card, bool _)
	{
		SetSelectHotkeys(card);
	}

	public void SetSelectHotkeys(AbilityCardUI card)
	{
		if (card == null)
		{
			_selectHotkeys.SetShown(canSelect: false, canUnselect: false);
			return;
		}
		bool flag = card.PlayerActor.IsUnderControlOrSingle() && (!CheckSelectable || card.IsSelectable) && (!CheckHovered || card.IsHovered);
		bool flag2 = !CheckSelectInScenario || card.CanSelectInScenario();
		_selectHotkeys.SetShown(flag && !card.IsSelected && flag2, flag && card.IsSelected);
	}

	private void UpdateSelectShortRest(ShortRest shortRest, bool select)
	{
		bool flag = shortRest.PlayerActor.IsUnderControlOrSingle();
		_selectHotkeys.SetShown(select && shortRest.IsInteractable && flag, canUnselect: false);
	}

	private void OnViewFullCard()
	{
		_selectHotkeys.SetShown(canSelect: false, canUnselect: false);
	}
}
