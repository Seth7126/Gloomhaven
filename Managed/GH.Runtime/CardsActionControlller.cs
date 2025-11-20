#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation.Utils;
using SharedLibrary.SimpleLog;
using UnityEngine;
using UnityEngine.UI;

public class CardsActionControlller : MonoBehaviour
{
	public enum Phase
	{
		None,
		Select1stCard,
		Pick1stTarget,
		Select2ndCard,
		Pick2ndTarget
	}

	[SerializeField]
	private GridLayoutGroup _layoutGroup;

	public static CardsActionControlller s_Instance;

	private static Phase CurrentPhase;

	private FullAbilityCard topCard;

	private FullAbilityCard bottomCard;

	private FullAbilityCard cachedTopCard;

	private FullAbilityCard cachedBottomCard;

	private FullAbilityCard firstPlayedCard;

	private CBaseCard.ActionType firstPlayedActionType;

	private FullAbilityCard secondPlayedCard;

	private CBaseCard.ActionType secondPlayedActionType;

	private static Phase CachedPhase;

	private FullAbilityCard cachedFirstPlayedCard;

	private CBaseCard.ActionType cachedFirstPlayedActionType;

	private FullAbilityCard cachedSecondPlayedCard;

	private CBaseCard.ActionType cachedSecondPlayedActionType;

	private CAbilityExtraTurn.EExtraTurnType extraTurnType;

	private bool isCardHighlighted;

	public bool IsActionAvailable
	{
		get
		{
			if (CurrentPhase != Phase.Select1stCard)
			{
				return CurrentPhase == Phase.Select2ndCard;
			}
			return true;
		}
	}

	private void Awake()
	{
		if (s_Instance == null)
		{
			s_Instance = this;
			CurrentPhase = Phase.None;
		}
		FullAbilityCard.FullCardHoveringStateChanged += OnFullCardHoveringStateChanged;
	}

	private void OnDestroy()
	{
		if (s_Instance == this)
		{
			s_Instance = null;
		}
		FullAbilityCard.FullCardHoveringStateChanged -= OnFullCardHoveringStateChanged;
	}

	public void Init(FullAbilityCard topCard, FullAbilityCard bottomCard, bool resetPhase = true, CAbilityExtraTurn.EExtraTurnType extraTurnType = CAbilityExtraTurn.EExtraTurnType.None)
	{
		Debug.Log("[CardsActionController.cs] Called Init(topCard: " + ((topCard != null) ? topCard.Title : "null") + ", bottomCard:" + ((bottomCard != null) ? bottomCard.Title : "null") + ", resetPhase: " + resetPhase + ", extraTurnType: " + extraTurnType);
		if (!resetPhase)
		{
			ReloadState();
		}
		else if (CurrentPhase == Phase.None || CurrentPhase == Phase.Pick2ndTarget)
		{
			this.topCard = topCard;
			this.bottomCard = bottomCard;
			this.extraTurnType = extraTurnType;
			if (topCard != null)
			{
				SimpleLog.AddToSimpleLog("Init Select1stCard phase");
				SetPhase(Phase.Select1stCard);
			}
			else
			{
				Debug.LogError("[CardsActionController.cs] Trying to Init() and about to setPhase to Phase.Select1stCard, but topCard is null!!");
				SetPhase(Phase.Select1stCard);
			}
		}
	}

	private void OnFullCardHoveringStateChanged(bool anyFullCardHovered)
	{
		if (InputManager.GamePadInUse)
		{
			Vector2 vector = (anyFullCardHovered ? UISettings.CardsHighlightLayoutGroupSettings.AnyCardHoveredSpacing : UISettings.CardsHighlightLayoutGroupSettings.NoneCardHoveredSpacing);
			Vector2 spacing = _layoutGroup.spacing;
			if (vector != spacing)
			{
				_layoutGroup.spacing = vector;
			}
		}
	}

	private void ReloadState()
	{
		if (CurrentPhase == Phase.None)
		{
			return;
		}
		Phase currentPhase = CurrentPhase;
		SimpleLog.AddToSimpleLog("ReloadState");
		SetPhase(Phase.Select1stCard);
		if (currentPhase == Phase.Select1stCard)
		{
			return;
		}
		OnCardClicked(firstPlayedCard, firstPlayedActionType);
		if (currentPhase != Phase.Pick1stTarget)
		{
			OnActionFinished();
			if (currentPhase != Phase.Select2ndCard)
			{
				OnCardClicked(secondPlayedCard, secondPlayedActionType);
				_ = 4;
			}
		}
	}

	public void OnCardHighlight(bool isActive, FullAbilityCard abilityCard, bool isTopSide, bool isDefaultAbility)
	{
		if (CurrentPhase == Phase.Select1stCard)
		{
			if (isActive != isCardHighlighted)
			{
				isCardHighlighted = isActive;
				if (topCard == abilityCard)
				{
					topCard.ToggleHighlightHover(isActive, isTopSide, isDefaultAbility);
					bottomCard?.ToggleHover(isActive, !isTopSide);
				}
				else
				{
					bottomCard?.ToggleHighlightHover(isActive, isTopSide, isDefaultAbility);
					topCard.ToggleHover(isActive, !isTopSide);
				}
			}
		}
		else if (CurrentPhase == Phase.Select2ndCard)
		{
			if (topCard == abilityCard)
			{
				topCard.ToggleHighlightHover(isActive, isTopSide, isDefaultAbility);
			}
			else
			{
				bottomCard.ToggleHighlightHover(isActive, isTopSide, isDefaultAbility);
			}
		}
	}

	private void ResetHighlight()
	{
		if (isCardHighlighted)
		{
			isCardHighlighted = false;
			bottomCard?.ToggleHover(active: false, isTopSide: true);
			bottomCard?.ToggleHover(active: false, isTopSide: false);
			topCard?.ToggleHover(active: false, isTopSide: false);
			topCard?.ToggleHover(active: false, isTopSide: true);
			bottomCard?.UntoggleHighlightHover(isTopSide: false);
			bottomCard?.UntoggleHighlightHover(isTopSide: true);
			topCard?.UntoggleHighlightHover(isTopSide: false);
			topCard?.UntoggleHighlightHover(isTopSide: true);
		}
	}

	public Phase GetPhase()
	{
		return CurrentPhase;
	}

	public CBaseCard.ActionType? GetAction(CAbilityCard card)
	{
		if (secondPlayedCard != null && secondPlayedCard.AbilityCard == card)
		{
			return secondPlayedActionType;
		}
		if (card != firstPlayedCard.AbilityCard)
		{
			return null;
		}
		return firstPlayedActionType;
	}

	private void SetPhase(Phase newPhase)
	{
		switch (newPhase)
		{
		case Phase.Select1stCard:
		{
			bool isValid = !FFSNetwork.IsOnline || CardsHandManager.Instance.CurrentHand.PlayerActor.IsUnderMyControl;
			SimpleLog.AddToSimpleLog("Set card interactable  on Select1stCard " + topCard?.AbilityCard?.Name + " & " + bottomCard?.AbilityCard?.Name);
			topCard?.SetInteractable(active: true);
			bottomCard?.SetInteractable(active: true);
			topCard?.SetValid(isValid, UIMultiplayerNotifications.ShowSelectedOtherPlayerCard);
			bottomCard?.SetValid(isValid, UIMultiplayerNotifications.ShowSelectedOtherPlayerCard);
			if (extraTurnType == CAbilityExtraTurn.EExtraTurnType.BottomAction || GameState.HasPlayedTopAction)
			{
				topCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.TopAction);
				bottomCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.TopAction);
				SimpleLog.AddToSimpleLog("ToggleSideInteractivity to false to TopAction on Select1stCard " + topCard?.AbilityCard?.Name + " & " + bottomCard?.AbilityCard?.Name);
			}
			if (extraTurnType == CAbilityExtraTurn.EExtraTurnType.TopAction || GameState.HasPlayedBottomAction)
			{
				topCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.BottomAction);
				bottomCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.BottomAction);
				SimpleLog.AddToSimpleLog("ToggleSideInteractivity to false to BottomAction on Select1stCard " + topCard?.AbilityCard?.Name + " & " + bottomCard?.AbilityCard?.Name);
			}
			foreach (CDisableCardActionActiveBonus cachedDisableCardActionActiveBonuse in CardsHandManager.Instance.CurrentHand.PlayerActor.CachedDisableCardActionActiveBonuses)
			{
				if (cachedDisableCardActionActiveBonuse.DisableCardActionData.CardName == topCard?.Title)
				{
					SimpleLog.AddToSimpleLog("ToggleActionInteractivity for cached card: " + topCard?.AbilityCard?.Name + " on Select1stCard");
					switch (cachedDisableCardActionActiveBonuse.DisableCardActionData.DisableActionType)
					{
					case CBaseCard.ActionType.TopAction:
						topCard?.ToggleActionInteractivity(active: false, CBaseCard.ActionType.TopAction);
						break;
					case CBaseCard.ActionType.BottomAction:
						topCard?.ToggleActionInteractivity(active: false, CBaseCard.ActionType.BottomAction);
						break;
					}
				}
				if (bottomCard != null && cachedDisableCardActionActiveBonuse.DisableCardActionData.CardName == bottomCard.Title)
				{
					SimpleLog.AddToSimpleLog("ToggleActionInteractivity for cached card: " + bottomCard?.AbilityCard?.Name + " on Select1stCard");
					switch (cachedDisableCardActionActiveBonuse.DisableCardActionData.DisableActionType)
					{
					case CBaseCard.ActionType.TopAction:
						bottomCard.ToggleActionInteractivity(active: false, CBaseCard.ActionType.TopAction);
						break;
					case CBaseCard.ActionType.BottomAction:
						bottomCard.ToggleActionInteractivity(active: false, CBaseCard.ActionType.BottomAction);
						break;
					}
				}
			}
			topCard?.Deselect();
			bottomCard?.Deselect();
			topCard?.ResetConsumes();
			bottomCard?.ResetConsumes();
			topCard?.ResetHighlightInfusions();
			bottomCard?.ResetHighlightInfusions();
			ResetHighlight();
			topCard?.HighlightAvailableConsumes(InfusionBoardUI.Instance.GetAvailableElements());
			bottomCard?.HighlightAvailableConsumes(InfusionBoardUI.Instance.GetAvailableElements());
			Singleton<UIUseAugmentationsBar>.Instance.Hide();
			break;
		}
		case Phase.Pick1stTarget:
			secondPlayedCard = ((topCard != firstPlayedCard) ? topCard : bottomCard);
			ResetHighlight();
			SimpleLog.AddToSimpleLog("ToggleSideInteractivity " + firstPlayedActionType.ToString() + " to false on Pick1stTarget " + topCard.AbilityCard?.Name + " & " + bottomCard?.AbilityCard?.Name);
			secondPlayedCard?.ToggleSideInteractivity(active: false, firstPlayedActionType);
			firstPlayedCard.ToggleSideInteractivity(active: false, (firstPlayedActionType != CBaseCard.ActionType.BottomAction && firstPlayedActionType != CBaseCard.ActionType.DefaultMoveAction) ? CBaseCard.ActionType.BottomAction : CBaseCard.ActionType.TopAction);
			firstPlayedCard.ToggleSelect(active: true, firstPlayedActionType);
			firstPlayedCard.HighlightInfusions(highlight: true, firstPlayedActionType);
			Choreographer.s_Choreographer.ShowAugmentationBar = true;
			break;
		case Phase.Select2ndCard:
			if (CurrentPhase != Phase.Pick2ndTarget)
			{
				firstPlayedCard.TryPlayBurnAnimation(firstPlayedActionType);
			}
			SimpleLog.AddToSimpleLog("Disable card interaction on Select2ndCard " + firstPlayedCard.AbilityCard?.Name);
			firstPlayedCard.HighlightInfusions(highlight: false, firstPlayedActionType);
			firstPlayedCard.SetInteractable(active: false);
			secondPlayedCard.Deselect();
			firstPlayedCard.ResetConsumes(clearselection: false);
			secondPlayedCard.HighlightAvailableConsumes(InfusionBoardUI.Instance.GetAvailableElements());
			SimpleLog.AddToSimpleLog("ToggleSideInteractivity on Select2ndCard " + secondPlayedCard.AbilityCard?.Name + " action " + ((firstPlayedActionType != CBaseCard.ActionType.BottomAction && firstPlayedActionType != CBaseCard.ActionType.DefaultMoveAction) ? CBaseCard.ActionType.BottomAction : CBaseCard.ActionType.TopAction));
			secondPlayedCard.ToggleSideInteractivity(active: false, (firstPlayedActionType == CBaseCard.ActionType.BottomAction || firstPlayedActionType == CBaseCard.ActionType.DefaultMoveAction) ? CBaseCard.ActionType.BottomAction : CBaseCard.ActionType.TopAction);
			if (firstPlayedActionType == CBaseCard.ActionType.BottomAction || firstPlayedActionType == CBaseCard.ActionType.DefaultMoveAction)
			{
				secondPlayedCard.ToggleSideInteractivity(extraTurnType != CAbilityExtraTurn.EExtraTurnType.BottomAction && !GameState.HasPlayedTopAction, CBaseCard.ActionType.TopAction);
			}
			else
			{
				secondPlayedCard.ToggleSideInteractivity(extraTurnType != CAbilityExtraTurn.EExtraTurnType.TopAction && !GameState.HasPlayedBottomAction, CBaseCard.ActionType.BottomAction);
			}
			foreach (CDisableCardActionActiveBonus cachedDisableCardActionActiveBonuse2 in CardsHandManager.Instance.CurrentHand.PlayerActor.CachedDisableCardActionActiveBonuses)
			{
				if (cachedDisableCardActionActiveBonuse2.DisableCardActionData.CardName == topCard.Title)
				{
					SimpleLog.AddToSimpleLog("ToggleActionInteractivity for cached card: " + topCard.AbilityCard?.Name + " on Select2ndCard");
					switch (cachedDisableCardActionActiveBonuse2.DisableCardActionData.DisableActionType)
					{
					case CBaseCard.ActionType.TopAction:
						topCard.ToggleActionInteractivity(active: false, CBaseCard.ActionType.TopAction);
						break;
					case CBaseCard.ActionType.BottomAction:
						topCard.ToggleActionInteractivity(active: false, CBaseCard.ActionType.BottomAction);
						break;
					}
				}
				if (bottomCard != null && cachedDisableCardActionActiveBonuse2.DisableCardActionData.CardName == bottomCard.Title)
				{
					SimpleLog.AddToSimpleLog("ToggleActionInteractivity for cached card: " + bottomCard?.AbilityCard?.Name + " on Select2ndCard");
					switch (cachedDisableCardActionActiveBonuse2.DisableCardActionData.DisableActionType)
					{
					case CBaseCard.ActionType.TopAction:
						bottomCard.ToggleActionInteractivity(active: false, CBaseCard.ActionType.TopAction);
						break;
					case CBaseCard.ActionType.BottomAction:
						bottomCard.ToggleActionInteractivity(active: false, CBaseCard.ActionType.BottomAction);
						break;
					}
				}
			}
			Singleton<UIUseAugmentationsBar>.Instance.Hide();
			break;
		case Phase.Pick2ndTarget:
			secondPlayedCard.HighlightInfusions(highlight: true, secondPlayedActionType);
			secondPlayedCard.ToggleSelect(active: true, secondPlayedActionType);
			secondPlayedCard.ResetConsumes(clearselection: false);
			Choreographer.s_Choreographer.ShowAugmentationBar = true;
			break;
		default:
			Singleton<UIUseAugmentationsBar>.Instance.Hide();
			break;
		}
		CurrentPhase = newPhase;
	}

	private static void RefreshConsumeBar(CBaseCard.ActionType actionType, FullAbilityCard card, bool skipAugments = false)
	{
		Choreographer.s_Choreographer.ShowAugmentationBar = false;
		if (actionType == CBaseCard.ActionType.BottomAction || actionType == CBaseCard.ActionType.TopAction)
		{
			List<ConsumeButton> list = ((actionType == CBaseCard.ActionType.BottomAction) ? card.bottomActionButton.consumeButtons.OrderByDescending((ConsumeButton it) => card.AbilityCard.BottomAction.Augmentations.FindIndex((CActionAugmentation i) => i.Name == it.ConsumeName)).ToList() : card.topActionButton.consumeButtons.OrderByDescending((ConsumeButton it) => card.AbilityCard.TopAction.Augmentations.FindIndex((CActionAugmentation i) => i.Name == it.ConsumeName)).ToList());
			if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
			{
				List<CActionAugmentation> actionAugmentationsAvailable = cPhaseAction.ActionAugmentationsAvailableForCurrentAbility();
				list = list.FindAll((ConsumeButton x) => actionAugmentationsAvailable.Any((CActionAugmentation y) => x.ConsumeName == y.Name));
			}
			List<Tuple<string, List<CActionAugmentation>>> list2 = new List<Tuple<string, List<CActionAugmentation>>>();
			if (!skipAugments)
			{
				list2 = GetAugmentationGroups((actionType == CBaseCard.ActionType.BottomAction) ? card.AbilityCard.BottomAction.Augmentations : card.AbilityCard.TopAction.Augmentations);
			}
			if (list.Count > 0 || list2.Count > 0)
			{
				Singleton<UIUseAugmentationsBar>.Instance.Show(Choreographer.s_Choreographer.m_CurrentActor, list, list2);
				return;
			}
		}
		Singleton<UIUseAugmentationsBar>.Instance.Hide();
	}

	private static List<Tuple<string, List<CActionAugmentation>>> GetAugmentationGroups(List<CActionAugmentation> augmentations)
	{
		IEnumerable<IGrouping<string, CActionAugmentation>> source = from it in augmentations
			where it.ConsumeGroup.IsNOTNullOrEmpty()
			group it by it.ConsumeGroup;
		CPhase currentPhase = PhaseManager.CurrentPhase;
		CPhaseAction phaseAction = currentPhase as CPhaseAction;
		if (phaseAction != null)
		{
			source = source.Where((IGrouping<string, CActionAugmentation> it) => !it.Any(phaseAction.HasConsumedActionAugmentation));
		}
		return source.Select((IGrouping<string, CActionAugmentation> it) => new Tuple<string, List<CActionAugmentation>>(it.Key, it.ToList())).ToList();
	}

	public void RefreshConsumeBar(bool skipAugments = false)
	{
		if (CurrentPhase == Phase.Pick1stTarget)
		{
			RefreshConsumeBar(firstPlayedActionType, firstPlayedCard, skipAugments);
		}
		else if (CurrentPhase == Phase.Pick2ndTarget)
		{
			RefreshConsumeBar(secondPlayedActionType, secondPlayedCard, skipAugments);
		}
		else
		{
			Singleton<UIUseAugmentationsBar>.Instance.Hide();
		}
	}

	private void Finish()
	{
		secondPlayedCard?.TryPlayBurnAnimation(secondPlayedActionType);
		secondPlayedCard?.HighlightInfusions(highlight: false, secondPlayedActionType);
		topCard.Deselect();
		bottomCard?.Deselect();
		SimpleLog.AddToSimpleLog("Disable card interaction on Finish " + topCard.AbilityCard?.Name + " & " + bottomCard?.AbilityCard?.Name);
		topCard.SetInteractable(active: false);
		bottomCard?.SetInteractable(active: false);
		topCard.ResetConsumes(clearselection: false);
		bottomCard?.ResetConsumes(clearselection: false);
		topCard.HighlightAvailableConsumes(null);
		bottomCard?.HighlightAvailableConsumes(null);
		SetPhase(Phase.None);
	}

	private void AfterItemUseAtEndOfTurn()
	{
		topCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.TopAction);
		topCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.BottomAction);
		bottomCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.TopAction);
		bottomCard?.ToggleSideInteractivity(active: false, CBaseCard.ActionType.BottomAction);
		firstPlayedCard?.TryPlayBurnAnimation(firstPlayedActionType);
		firstPlayedCard?.HighlightInfusions(highlight: false, firstPlayedActionType);
		secondPlayedCard?.TryPlayBurnAnimation(secondPlayedActionType);
		secondPlayedCard?.HighlightInfusions(highlight: false, secondPlayedActionType);
		SimpleLog.AddToSimpleLog("Disable card interaction on AfterItemUseAtEndOfTurn " + topCard?.AbilityCard?.Name + " & " + bottomCard?.AbilityCard?.Name);
		topCard?.Deselect();
		bottomCard?.Deselect();
		topCard?.SetInteractable(active: false);
		bottomCard?.SetInteractable(active: false);
		topCard?.ResetConsumes(clearselection: false);
		bottomCard?.ResetConsumes(clearselection: false);
		topCard?.HighlightAvailableConsumes(null);
		bottomCard?.HighlightAvailableConsumes(null);
		SetPhase(Phase.None);
	}

	public void OnCardClicked(FullAbilityCard card, CBaseCard.ActionType actionType)
	{
		Singleton<UIUseItemsBar>.Instance.Hide();
		if (CurrentPhase == Phase.Select1stCard)
		{
			firstPlayedActionType = actionType;
			firstPlayedCard = card;
			SetPhase(Phase.Pick1stTarget);
		}
		else if (CurrentPhase == Phase.Select2ndCard)
		{
			secondPlayedActionType = actionType;
			secondPlayedCard = card;
			SetPhase(Phase.Pick2ndTarget);
		}
		else
		{
			Debug.LogError("Unexpected phase - " + CurrentPhase);
		}
	}

	public void OnActionFinished()
	{
		if (CurrentPhase == Phase.Pick1stTarget)
		{
			if (secondPlayedCard != null)
			{
				SetPhase(Phase.Select2ndCard);
			}
			else
			{
				Finish();
			}
		}
		else if (CurrentPhase == Phase.Pick2ndTarget)
		{
			Finish();
		}
		else if (CurrentPhase == Phase.None)
		{
			AfterItemUseAtEndOfTurn();
		}
		else if (CurrentPhase != Phase.Select1stCard && CurrentPhase != Phase.Select2ndCard)
		{
			Debug.LogError("Unexpected phase - " + CurrentPhase);
		}
	}

	public void Reset()
	{
		SetPhase(Phase.None);
	}

	public void CachePhase()
	{
		CachedPhase = CurrentPhase;
		cachedTopCard = topCard;
		cachedBottomCard = bottomCard;
		cachedFirstPlayedCard = firstPlayedCard;
		cachedFirstPlayedActionType = firstPlayedActionType;
		cachedSecondPlayedCard = secondPlayedCard;
		cachedSecondPlayedActionType = secondPlayedActionType;
		SetPhase(Phase.None);
	}

	public void RestorePhase()
	{
		if (CachedPhase != Phase.None)
		{
			topCard = cachedTopCard;
			bottomCard = cachedBottomCard;
			firstPlayedCard = cachedFirstPlayedCard;
			firstPlayedActionType = cachedFirstPlayedActionType;
			secondPlayedCard = cachedSecondPlayedCard;
			secondPlayedActionType = cachedSecondPlayedActionType;
			SetPhase(CachedPhase);
			CachedPhase = Phase.None;
			cachedTopCard = null;
			cachedBottomCard = null;
			cachedFirstPlayedCard = null;
			cachedFirstPlayedActionType = CBaseCard.ActionType.NA;
			cachedSecondPlayedCard = null;
			cachedSecondPlayedActionType = CBaseCard.ActionType.NA;
		}
	}

	public void RefreshPhase()
	{
		SetPhase(CurrentPhase);
	}

	public void OnActionUndo()
	{
		InfusionBoardUI.Instance.UnreserveElements();
		if (CurrentPhase == Phase.Pick1stTarget)
		{
			SimpleLog.AddToSimpleLog("OnActionUndo  set Select1stCard phase");
			Choreographer.s_Choreographer.onCharacterAbilityComplete = null;
			SetPhase(Phase.Select1stCard);
		}
		else if (CurrentPhase == Phase.Pick2ndTarget)
		{
			Choreographer.s_Choreographer.onCharacterAbilityComplete = null;
			SimpleLog.AddToSimpleLog("ToggleSideInteractivity to true on OnActionUndo " + secondPlayedCard.AbilityCard?.Name + " action " + secondPlayedActionType);
			secondPlayedCard.ToggleSideInteractivity(active: true, secondPlayedActionType);
			secondPlayedCard.ToggleSelect(active: false, secondPlayedActionType);
			SetPhase(Phase.Select2ndCard);
		}
		else
		{
			Debug.LogError("Unexpected phase - " + CurrentPhase);
		}
	}

	public void PickAnyInfusionElements(CActor actorPicking, Action<bool> onPicked = null)
	{
		if (CurrentPhase == Phase.Pick1stTarget)
		{
			PickUnselectedInfusions(firstPlayedCard, firstPlayedActionType, onPicked, actorPicking);
		}
		else if (CurrentPhase == Phase.Pick2ndTarget)
		{
			PickUnselectedInfusions(secondPlayedCard, secondPlayedActionType, onPicked, actorPicking);
		}
		else
		{
			onPicked?.Invoke(obj: false);
		}
	}

	private void PickUnselectedInfusions(FullAbilityCard card, CBaseCard.ActionType abilityType, Action<bool> onFinish, CActor actorPicking)
	{
		if (abilityType == CBaseCard.ActionType.DefaultAttackAction || abilityType == CBaseCard.ActionType.DefaultMoveAction)
		{
			onFinish?.Invoke(obj: false);
			return;
		}
		List<InfuseElement> infusions = card.UnselectedInfusions(abilityType);
		if (infusions.Count == 0)
		{
			onFinish?.Invoke(obj: false);
			return;
		}
		if (FFSNetwork.IsOnline)
		{
			if (!actorPicking.IsUnderMyControl)
			{
				ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.ElementPicking, savePreviousState: true);
			}
			else
			{
				ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.ElementPicking, savePreviousState: true);
			}
		}
		Singleton<UIUseAbilitiesBar>.Instance.ShowInfusionsAction(actorPicking as CPlayerActor, card, (abilityType == CBaseCard.ActionType.BottomAction) ? card.AbilityCard.BottomAction : card.AbilityCard.TopAction, infusions, delegate
		{
			Singleton<UIUseAbilitiesBar>.Instance.Hide();
			if (PhaseManager.PhaseType == CPhase.PhaseType.Action)
			{
				foreach (InfuseElement item in infusions)
				{
					_ = item;
					((CPhaseAction)PhaseManager.CurrentPhase).ElementsToInfuse.Remove(ElementInfusionBoardManager.EElement.Any);
				}
			}
			onFinish?.Invoke(obj: true);
		});
	}

	public bool AreThereInfusionsToPick()
	{
		if (CurrentPhase != Phase.Pick1stTarget)
		{
			if (CurrentPhase != Phase.Pick1stTarget)
			{
				return true;
			}
			return secondPlayedCard.AreThereInfusionsToPick(secondPlayedActionType);
		}
		return firstPlayedCard.AreThereInfusionsToPick(firstPlayedActionType);
	}

	public bool HasPlayedCards()
	{
		if (!(firstPlayedCard != null))
		{
			return secondPlayedCard != null;
		}
		return true;
	}
}
