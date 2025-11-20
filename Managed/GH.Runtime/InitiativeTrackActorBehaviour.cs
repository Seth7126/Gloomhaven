using System;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public abstract class InitiativeTrackActorBehaviour : MonoBehaviour, IComparable
{
	[SerializeField]
	protected InitiativeTrackActorAvatar m_Avatar;

	[SerializeField]
	private LayoutElement layoutElement;

	public ExtendedButton avatarButton;

	public InteractabilityIsolatedUIControl IsolatedUIControl;

	[SerializeField]
	private GUIAnimator extraTurnAnimation;

	protected CActor actor;

	protected bool isSelected;

	private bool isInitialized;

	private bool isDeadState;

	public bool IsDeadState => isDeadState;

	public InitiativeTrackActorAvatar Avatar => m_Avatar;

	public CActor Actor
	{
		get
		{
			return actor;
		}
		protected set
		{
			actor = value;
			m_Avatar.Init(OnAvatarHighlight, actor, CardsHandManager.Instance.EnableCancelActiveAbilities);
		}
	}

	public abstract CActor.EType ActorType();

	protected abstract void OnAvatarHighlight(bool active);

	public void SetDeadState(bool value)
	{
		isDeadState = value;
		m_Avatar.SetActiveHeader(!value);
		m_Avatar.SetActiveDeadImage(value);
	}

	public virtual void Deselect()
	{
		isSelected = false;
		m_Avatar.Deselect();
	}

	public virtual void Select()
	{
		isSelected = true;
		m_Avatar.Select();
	}

	public void RefreshInitiative()
	{
		m_Avatar.RefreshInitiative();
	}

	public void RefreshAbilities()
	{
		m_Avatar.RefreshAbilities();
	}

	public bool IsSelected()
	{
		return m_Avatar.IsSelected();
	}

	public virtual void SetAttributes(CActor actor, bool activePlayerButton, bool changeHilight, bool generateCard = true)
	{
		Actor = actor;
		m_Avatar.SetAttributes(actor, activePlayerButton, changeHilight);
		if (IsolatedUIControl != null)
		{
			IsolatedUIControl.ControlIdentifier = actor.GetPrefabName();
		}
		ShowExtraTurn(actor.IsTakingExtraTurn);
	}

	public virtual void SetAttributesDirect(CActor actor, bool activeIDBackplate, bool activeID, string textID, bool activeInitiative, int initiative, int currentHealth, int maxHealth, bool isActive, bool activeHilight, bool activeButton)
	{
		Actor = actor;
		m_Avatar.SetAttributesDirect(actor, activeIDBackplate, activeID, textID, activeInitiative, initiative, currentHealth, maxHealth, isActive, activeHilight, activeButton);
		ShowExtraTurn(actor.IsTakingExtraTurn);
	}

	public void ShowDetails(bool active)
	{
		m_Avatar.ShowDetails(active);
	}

	public int GetOrderPriority()
	{
		if (actor == null)
		{
			return -3;
		}
		if (actor.Initiative() == 0)
		{
			if (actor.IsPlayerByDefault())
			{
				return -1;
			}
			return -2;
		}
		return 100 - actor.Initiative();
	}

	public int CompareTo(object obj)
	{
		if (obj == null)
		{
			return 1;
		}
		InitiativeTrackActorBehaviour initiativeTrackActorBehaviour = obj as InitiativeTrackActorBehaviour;
		if (initiativeTrackActorBehaviour != null)
		{
			if (actor.IsDeadPlayer)
			{
				if (initiativeTrackActorBehaviour.Actor.IsDeadPlayer)
				{
					if (ScenarioManager.Scenario != null)
					{
						int num = ScenarioManager.Scenario.ExhaustedPlayers.IndexOf((CPlayerActor)actor);
						int value = ScenarioManager.Scenario.ExhaustedPlayers.IndexOf((CPlayerActor)initiativeTrackActorBehaviour.Actor);
						return num.CompareTo(value);
					}
					return 0;
				}
				return -1;
			}
			if (initiativeTrackActorBehaviour.Actor.IsDeadPlayer)
			{
				if (actor.IsDeadPlayer)
				{
					if (ScenarioManager.Scenario != null)
					{
						int value2 = ScenarioManager.Scenario.ExhaustedPlayers.IndexOf((CPlayerActor)actor);
						return ScenarioManager.Scenario.ExhaustedPlayers.IndexOf((CPlayerActor)initiativeTrackActorBehaviour.Actor).CompareTo(value2);
					}
					return 0;
				}
				return 1;
			}
			if (FFSNetwork.IsOnline && PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest && actor.IsPlayerByDefault() && initiativeTrackActorBehaviour.Actor.IsPlayerByDefault() && (!actor.IsUnderMyControl || !initiativeTrackActorBehaviour.Actor.IsUnderMyControl))
			{
				if (actor.IsUnderMyControl != initiativeTrackActorBehaviour.Actor.IsUnderMyControl)
				{
					if (!actor.IsUnderMyControl)
					{
						return -1;
					}
					return 1;
				}
				return 0;
			}
			int orderPriority = initiativeTrackActorBehaviour.GetOrderPriority();
			int orderPriority2 = GetOrderPriority();
			if (orderPriority != orderPriority2 || orderPriority < 0 || orderPriority2 < 0)
			{
				return orderPriority2.CompareTo(orderPriority);
			}
			return (100 - actor.SubInitiative()).CompareTo(100 - initiativeTrackActorBehaviour.Actor.SubInitiative());
		}
		Debug.LogError("Comparing non InitiativeTrackActorBehaviour class");
		return 1;
	}

	public void OnActiveBonusTriggered(CActiveBonus activeBonus)
	{
		m_Avatar.OnActiveBonusTriggered(activeBonus);
	}

	public void ShowExtraTurn(bool show)
	{
		if (show)
		{
			extraTurnAnimation.Play();
		}
		else
		{
			extraTurnAnimation.Stop();
		}
	}

	protected virtual void OnDisable()
	{
		ShowExtraTurn(show: false);
	}

	public void EnableNavigation(Selectable left, Selectable right)
	{
		m_Avatar.EnableNavigation(avatarButton, right);
		avatarButton.SetNavigation(new NavigationCalculator
		{
			left = () => left,
			right = () => right,
			up = m_Avatar.GetFirstBonus,
			down = m_Avatar.GetFirstBonus
		});
	}

	public void DisableNavigation()
	{
		avatarButton.DisableNavigation();
		m_Avatar.DisableNavigation();
	}
}
