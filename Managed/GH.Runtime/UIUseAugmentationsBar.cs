using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using Photon.Bolt;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Events;

public class UIUseAugmentationsBar : Singleton<UIUseAugmentationsBar>
{
	[SerializeField]
	private RectTransform container;

	[SerializeField]
	private UIUseAugmentation slotPrefab;

	[SerializeField]
	private UnityEvent OnChangedAugmentsAvailable;

	private Dictionary<ConsumeButton, UIUseAugmentation> augmentSlots = new Dictionary<ConsumeButton, UIUseAugmentation>();

	private Dictionary<string, UIUseAugmentation> augmentGroupSlots = new Dictionary<string, UIUseAugmentation>();

	private Stack<UIUseAugmentation> pool = new Stack<UIUseAugmentation>();

	private CActor actor;

	private List<ConsumeButton> normalConsumes;

	private Transform lastCreatedSlot;

	private void Create(ConsumeButton button)
	{
		button.Enable();
		if (!CanConsume(button.abilityConsume.ConsumeData))
		{
			button.SetAvailable(isAvailable: false);
			return;
		}
		button.SetAvailable(isAvailable: true);
		if (!augmentSlots.ContainsKey(button))
		{
			augmentSlots[button] = GetSlotFromPool();
			augmentSlots[button].Init(actor, button, delegate(IAugmentation augment)
			{
				OnActivatedConsumeButton(button, augment);
			}, delegate(IAugmentation augment)
			{
				OnDisactivatedConsumeButton(button, augment);
			});
			augmentSlots[button].Show();
		}
		OnChangedAugmentsAvailable.Invoke();
	}

	private void Create(string consumeGroup, List<CActionAugmentation> augmentations)
	{
		if (!augmentGroupSlots.ContainsKey(consumeGroup))
		{
			augmentGroupSlots[consumeGroup] = GetSlotFromPool();
			augmentGroupSlots[consumeGroup].Init(actor, consumeGroup, augmentations, OnActivatedAugmentGroup, OnDeactivatedAugmentGroup);
			augmentGroupSlots[consumeGroup].Show();
		}
		OnChangedAugmentsAvailable.Invoke();
	}

	public void SetInteractionAvailableSlots(bool interactable, bool onlyChangeUnselected = false)
	{
		foreach (KeyValuePair<string, UIUseAugmentation> augmentGroupSlot in augmentGroupSlots)
		{
			if (!onlyChangeUnselected || !augmentGroupSlot.Value.IsSelected())
			{
				augmentGroupSlot.Value.SetInteractable(interactable);
			}
		}
		foreach (KeyValuePair<ConsumeButton, UIUseAugmentation> augmentSlot in augmentSlots)
		{
			if (!onlyChangeUnselected || !augmentSlot.Value.IsSelected())
			{
				augmentSlot.Value.SetInteractable(interactable);
			}
		}
	}

	private void OnDisactivatedConsumeButton(ConsumeButton consume, IAugmentation augmentation)
	{
		UIEventManager.LogUIEvent(new UIEvent(consume.IsAbilityCardTopButton ? UIEvent.EUIEventType.ConsumeElementCardTop : UIEvent.EUIEventType.ConsumeElementCardBottom, null, consume.ID, consume.ConsumeButtonIndex));
		consume.ClearSelection();
		consume.TryNetworkAbilityAugmentToggle(toggleOn: false);
	}

	private void OnActivatedConsumeButton(ConsumeButton consume, IAugmentation augmentation)
	{
		UIEventManager.LogUIEvent(new UIEvent(consume.IsAbilityCardTopButton ? UIEvent.EUIEventType.ConsumeElementCardTop : UIEvent.EUIEventType.ConsumeElementCardBottom, null, consume.ID, consume.ConsumeButtonIndex));
		consume.TryNetworkAbilityAugmentToggle(toggleOn: true);
	}

	private void OnDeactivatedAugmentGroup(CActionAugmentation augment, IAugmentation augmentationGroup)
	{
		TryNetworkAbilityAugmentationGroupToggle(augment, augmentationGroup, toggleOn: false);
	}

	private void OnActivatedAugmentGroup(CActionAugmentation augment, IAugmentation augmentationGroup)
	{
		TryNetworkAbilityAugmentationGroupToggle(augment, augmentationGroup, toggleOn: true);
	}

	private bool CanConsume(CActionAugmentation augment)
	{
		List<ElementInfusionBoardManager.EElement> availableElements = InfusionBoardUI.Instance.GetAvailableElements();
		if (availableElements.Count < augment.Elements.Count)
		{
			return false;
		}
		foreach (ElementInfusionBoardManager.EElement element in augment.Elements)
		{
			if (element != ElementInfusionBoardManager.EElement.Any && !availableElements.Contains(element))
			{
				return false;
			}
		}
		return true;
	}

	private void RefreshAvailableElements()
	{
		List<ElementInfusionBoardManager.EElement> availableElements = augmentSlots.Keys.SelectMany((ConsumeButton it) => it.abilityConsume.ConsumeData.Elements).Distinct().ToList();
		InfusionBoardUI.Instance.SetAvailableElements(availableElements);
	}

	private void OnUnreservedElement(List<ElementInfusionBoardManager.EElement> elements)
	{
		for (int i = 0; i < normalConsumes.Count; i++)
		{
			if (!augmentSlots.ContainsKey(normalConsumes[i]) && CanConsume(normalConsumes[i].abilityConsume.ConsumeData))
			{
				Create(normalConsumes[i]);
			}
		}
		RefreshAvailableElements();
	}

	private void OnReservedElement(List<ElementInfusionBoardManager.EElement> elements)
	{
		foreach (KeyValuePair<ConsumeButton, UIUseAugmentation> item in augmentSlots.ToList())
		{
			if (!item.Value.IsSelected() && !CanConsume(item.Key.abilityConsume.ConsumeData))
			{
				Remove(item.Key, undoSelection: true);
			}
		}
		RefreshAvailableElements();
	}

	private void Remove(ConsumeButton button, bool undoSelection = false)
	{
		button.Disable();
		if (augmentSlots.ContainsKey(button))
		{
			button.ResetState(undoSelection);
			if (undoSelection)
			{
				augmentSlots[button].ClearSelection();
			}
			augmentSlots[button].Hide();
			pool.Push(augmentSlots[button]);
			augmentSlots.Remove(button);
			OnChangedAugmentsAvailable.Invoke();
		}
	}

	private void Remove(string groupId, bool undoSelection = false)
	{
		if (augmentGroupSlots.ContainsKey(groupId))
		{
			UIUseAugmentation uIUseAugmentation = augmentGroupSlots[groupId];
			if (undoSelection)
			{
				uIUseAugmentation.ClearSelection();
			}
			uIUseAugmentation.Hide();
			pool.Push(uIUseAugmentation);
			augmentGroupSlots.Remove(groupId);
			OnChangedAugmentsAvailable.Invoke();
		}
	}

	private UIUseAugmentation GetSlotFromPool()
	{
		UIUseAugmentation uIUseAugmentation;
		if (pool.Count > 0)
		{
			uIUseAugmentation = pool.Pop();
			uIUseAugmentation.gameObject.SetActive(value: true);
			return uIUseAugmentation;
		}
		uIUseAugmentation = UnityEngine.Object.Instantiate(slotPrefab, container);
		if (lastCreatedSlot != null)
		{
			uIUseAugmentation.transform.SetSiblingIndex(lastCreatedSlot.GetSiblingIndex() + 1);
		}
		lastCreatedSlot = uIUseAugmentation.transform;
		return uIUseAugmentation;
	}

	public void Hide(bool clearSelection = false)
	{
		InfusionBoardUI.Instance.OnReservedElement.RemoveListener(OnReservedElement);
		InfusionBoardUI.Instance.OnUnreserveElement.RemoveListener(OnUnreservedElement);
		foreach (KeyValuePair<ConsumeButton, UIUseAugmentation> augmentSlot in augmentSlots)
		{
			if (clearSelection)
			{
				augmentSlot.Value.ClearSelection();
			}
			augmentSlot.Value.Hide();
			pool.Push(augmentSlot.Value);
		}
		foreach (KeyValuePair<string, UIUseAugmentation> augmentGroupSlot in augmentGroupSlots)
		{
			if (clearSelection)
			{
				augmentGroupSlot.Value.ClearSelection();
			}
			augmentGroupSlot.Value.Hide();
			pool.Push(augmentGroupSlot.Value);
		}
		if (!augmentSlots.IsNullOrEmpty() || !augmentGroupSlots.IsNullOrEmpty())
		{
			OnChangedAugmentsAvailable.Invoke();
		}
		augmentSlots.Clear();
		augmentGroupSlots.Clear();
		RefreshAvailableElements();
	}

	public void UndoSelection()
	{
		foreach (KeyValuePair<ConsumeButton, UIUseAugmentation> augmentSlot in augmentSlots)
		{
			augmentSlot.Value.ClearSelection();
		}
		foreach (KeyValuePair<string, UIUseAugmentation> augmentGroupSlot in augmentGroupSlots)
		{
			augmentGroupSlot.Value.ClearSelection();
		}
	}

	public void Show(CActor actor, List<ConsumeButton> consumes, List<Tuple<string, List<CActionAugmentation>>> groupConsumes)
	{
		this.actor = actor;
		InfusionBoardUI.Instance.OnReservedElement.RemoveListener(OnReservedElement);
		InfusionBoardUI.Instance.OnUnreserveElement.RemoveListener(OnUnreservedElement);
		InfusionBoardUI.Instance.OnUnreserveElement.AddListener(OnUnreservedElement);
		InfusionBoardUI.Instance.OnReservedElement.AddListener(OnReservedElement);
		normalConsumes = consumes;
		foreach (ConsumeButton item in augmentSlots.Keys.Where((ConsumeButton x) => !normalConsumes.Contains(x)).ToList())
		{
			Remove(item);
		}
		foreach (ConsumeButton normalConsume in normalConsumes)
		{
			Create(normalConsume);
		}
		foreach (string item2 in augmentGroupSlots.Keys.ToList())
		{
			Remove(item2);
		}
		foreach (Tuple<string, List<CActionAugmentation>> groupConsume in groupConsumes)
		{
			Create(groupConsume.Item1, groupConsume.Item2);
		}
		RefreshAvailableElements();
	}

	public void TryNetworkAbilityAugmentationGroupToggle(CActionAugmentation augment, IAugmentation augmentationGroup, bool toggleOn)
	{
		if (FFSNetwork.IsOnline && Choreographer.s_Choreographer.ThisPlayerHasTurnControl)
		{
			int actionType = (toggleOn ? 36 : 37);
			ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
			IProtocolToken supplementaryDataToken = new AbilityAugmentGroupToken(augment.Name, augmentationGroup.ID);
			Synchronizer.SendGameAction((GameActionType)actionType, currentPhase, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
		}
	}

	public void ProxyToggleAugmentationGroup(GameAction action)
	{
		AbilityAugmentGroupToken abilityAugmentGroupToken = action.SupplementaryDataToken as AbilityAugmentGroupToken;
		if (augmentGroupSlots.ContainsKey(abilityAugmentGroupToken.AugmentGroupID))
		{
			if (action.ActionTypeID == 36)
			{
				augmentGroupSlots[abilityAugmentGroupToken.AugmentGroupID].SetOption(abilityAugmentGroupToken.AugmentName);
				augmentGroupSlots[abilityAugmentGroupToken.AugmentGroupID].Select();
			}
			else
			{
				augmentGroupSlots[abilityAugmentGroupToken.AugmentGroupID].Unselect();
			}
			return;
		}
		throw new Exception("Error toggling the augment. No such augmentation group exists on the bar (GroupID: " + abilityAugmentGroupToken.AugmentGroupID + ").");
	}

	public void ProxyToggleAugment(ConsumeButton consumeButton, bool toggleOn, AbilityAugmentToken dataToken)
	{
		if (augmentSlots.ContainsKey(consumeButton))
		{
			if (toggleOn)
			{
				List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement> { (ElementInfusionBoardManager.EElement)dataToken.SecondElementTypeID };
				if (dataToken.UsesBothElements)
				{
					list.Add((ElementInfusionBoardManager.EElement)dataToken.FirstElementTypeID);
				}
				augmentSlots[consumeButton].SetConsumes(list);
				augmentSlots[consumeButton].Select();
			}
			else
			{
				augmentSlots[consumeButton].Unselect();
			}
			return;
		}
		throw new Exception("Error toggling the augment. No such augment exists on the bar (Ability: " + consumeButton.ID + ", Augment: " + consumeButton.abilityConsume.Name + ").");
	}
}
