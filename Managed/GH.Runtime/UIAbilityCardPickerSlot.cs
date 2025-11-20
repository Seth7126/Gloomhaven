using System;
using FFSNet;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class UIAbilityCardPickerSlot : MonoBehaviour
{
	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private Transform cardHolder;

	private AbilityCardUI abilityCardUI;

	private IAbilityCardOption option;

	private Action<IAbilityCardOption> onSelected;

	private Action<IAbilityCardOption> onDeselected;

	private bool isSelected;

	public IAbilityCardOption AbilityCardOption => option;

	public Selectable Selectable => button;

	private void Awake()
	{
		button.onClick.AddListener(Toggle);
	}

	private void OnDestroy()
	{
		button.onClick.RemoveAllListeners();
	}

	public void SetOption(IAbilityCardOption option, Action<IAbilityCardOption> onSelected, Action<IAbilityCardOption> onDeselected)
	{
		this.option = option;
		this.onSelected = onSelected;
		this.onDeselected = onDeselected;
		isSelected = false;
		Clear();
		abilityCardUI = ObjectPool.SpawnCard(option.AbilityCard.ID, ObjectPool.ECardType.Ability, cardHolder, resetLocalScale: true, resetToMiddle: true).GetComponent<AbilityCardUI>();
		abilityCardUI.Init(option.AbilityCard);
		if (option.ActionTypeDisabled != CBaseCard.ActionType.NA)
		{
			abilityCardUI.fullAbilityCard.ToggleSideInteractivity(active: false, option.ActionTypeDisabled);
		}
	}

	public void Clear()
	{
		if (!(abilityCardUI == null))
		{
			ObjectPool.RecycleCard(abilityCardUI.CardID, ObjectPool.ECardType.Ability, abilityCardUI.gameObject);
			abilityCardUI = null;
		}
	}

	private void Toggle()
	{
		if (isSelected)
		{
			Deselect();
		}
		else
		{
			Select();
		}
	}

	private void OnDisable()
	{
		Clear();
	}

	public void Deselect()
	{
		if (!FFSNetwork.IsOnline || Choreographer.s_Choreographer.m_CurrentActor.IsUnderMyControl)
		{
			Deselect(networkActionIfOnline: true);
		}
	}

	public void Deselect(bool networkActionIfOnline = true)
	{
		isSelected = false;
		if (option.ActionTypeDisabled != CBaseCard.ActionType.NA)
		{
			abilityCardUI.fullAbilityCard.ToggleSelect(active: false, (option.ActionTypeDisabled != CBaseCard.ActionType.BottomAction) ? CBaseCard.ActionType.BottomAction : CBaseCard.ActionType.TopAction);
		}
		onDeselected?.Invoke(option);
		if (FFSNetwork.IsOnline && Choreographer.s_Choreographer.m_CurrentActor.IsUnderMyControl && networkActionIfOnline)
		{
			Synchronizer.SendGameAction(GameActionType.DeselectAbilityCard, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, option.AbilityCard.ID);
		}
	}

	public void Select()
	{
		if (!FFSNetwork.IsOnline || Choreographer.s_Choreographer.m_CurrentActor.IsUnderMyControl)
		{
			Select(networkActionIfOnline: true);
		}
	}

	public void Select(bool networkActionIfOnline = true)
	{
		isSelected = true;
		if (option.ActionTypeDisabled != CBaseCard.ActionType.NA)
		{
			abilityCardUI.fullAbilityCard.ToggleSelect(active: true, (option.ActionTypeDisabled != CBaseCard.ActionType.BottomAction) ? CBaseCard.ActionType.BottomAction : CBaseCard.ActionType.TopAction);
		}
		onSelected?.Invoke(option);
		if (FFSNetwork.IsOnline && Choreographer.s_Choreographer.m_CurrentActor.IsUnderMyControl && networkActionIfOnline)
		{
			Synchronizer.SendGameAction(GameActionType.SelectAbilityCard, ActionProcessor.CurrentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, option.AbilityCard.ID);
		}
	}

	public void SetFocused(bool focused)
	{
		abilityCardUI.SetType(focused ? CardPileType.Hand : CardPileType.Discarded);
	}
}
