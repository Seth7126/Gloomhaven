using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;

namespace ScenarioRuleLibrary;

public class CInventory
{
	public CItem HeadSlot { get; private set; }

	public CItem BodySlot { get; private set; }

	public CItem[] OneHandSlots { get; private set; }

	public CItem TwoHandSlot { get; private set; }

	public CItem LegSlot { get; private set; }

	public CItem[] SmallItemSlots { get; private set; }

	public CItem[] QuestItemSlots { get; private set; }

	public int SmallItemMax => GetSmallItemMax();

	public int SmallItemOverride { get; private set; }

	public int QuestItemMax => 2;

	public int Level { get; private set; }

	public CActor InventoryActor { get; set; }

	public List<CItem> AllItems
	{
		get
		{
			List<CItem> list = new List<CItem>();
			list.Add(HeadSlot);
			list.Add(BodySlot);
			list.Add(LegSlot);
			if (TwoHandSlot != null)
			{
				list.Add(TwoHandSlot);
			}
			if (OneHandSlots != null && OneHandSlots.Length == 2)
			{
				list.AddRange(OneHandSlots);
			}
			list.AddRange(SmallItemSlots);
			list.AddRange(QuestItemSlots);
			return list.Where((CItem item) => item != null).ToList();
		}
	}

	public List<CItem> SelectedItems => AllItems.Where((CItem w) => w.SlotState == CItem.EItemSlotState.Selected || w.SlotState == CItem.EItemSlotState.Locked).ToList();

	public event EventHandler ItemAdded;

	public event EventHandler ItemSpent;

	public event EventHandler ItemConsumed;

	public event EventHandler ItemUnrestrictedUsed;

	public event EventHandler ItemRefreshed;

	public event EventHandler ItemSelected;

	public event EventHandler ItemDeSelected;

	public event EventHandler ItemUsable;

	public event EventHandler ItemNoLongerUsable;

	public event EventHandler ItemActive;

	private static void SendClientItemMessage(EventHandler callback, CItem item, ESESubTypeItem callbackType, CActor actor = null)
	{
		COnItemCallback_MessageData message = new COnItemCallback_MessageData(actor)
		{
			m_Callback = callback,
			m_Item = item,
			m_CallbackType = callbackType
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	protected virtual void OnItemAdded(CItem item)
	{
		if (this.ItemAdded != null)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventItem(ESESubTypeItem.ItemAdded, item.YMLData.ItemType, item.YMLData.Slot, item.YMLData.StringID));
			SendClientItemMessage(this.ItemAdded, item, ESESubTypeItem.ItemAdded);
		}
	}

	protected virtual void OnItemSpent(CItem item)
	{
		if (this.ItemSpent != null)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventItem(ESESubTypeItem.ItemSpent, item.YMLData.ItemType, item.YMLData.Slot, item.YMLData.StringID));
			SendClientItemMessage(this.ItemSpent, item, ESESubTypeItem.ItemSpent);
		}
	}

	protected virtual void OnItemConsumed(CItem item)
	{
		if (this.ItemConsumed != null)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventItem(ESESubTypeItem.ItemConsumed, item.YMLData.ItemType, item.YMLData.Slot, item.YMLData.StringID));
			SendClientItemMessage(this.ItemConsumed, item, ESESubTypeItem.ItemConsumed);
		}
	}

	protected virtual void OnItemUnrestrictedUsed(CItem item)
	{
		if (this.ItemUnrestrictedUsed != null)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventItem(ESESubTypeItem.ItemUnrestrictedUsed, item.YMLData.ItemType, item.YMLData.Slot, item.YMLData.StringID));
			SendClientItemMessage(this.ItemUnrestrictedUsed, item, ESESubTypeItem.ItemUnrestrictedUsed);
		}
	}

	protected virtual void OnItemRefreshed(CItem item, CActor actor)
	{
		if (this.ItemRefreshed != null)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventItem(ESESubTypeItem.ItemRefreshed, item.YMLData.ItemType, item.YMLData.Slot, item.YMLData.StringID));
			SendClientItemMessage(this.ItemRefreshed, item, ESESubTypeItem.ItemRefreshed, actor);
		}
	}

	protected virtual void OnItemSelected(CItem item)
	{
		if (this.ItemSelected != null)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventItem(ESESubTypeItem.ItemSelected, item.YMLData.ItemType, item.YMLData.Slot, item.YMLData.StringID));
			SendClientItemMessage(this.ItemSelected, item, ESESubTypeItem.ItemSelected);
		}
	}

	protected virtual void OnItemDeSelected(CItem item)
	{
		if (this.ItemDeSelected != null)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventItem(ESESubTypeItem.ItemDeSelected, item.YMLData.ItemType, item.YMLData.Slot, item.YMLData.StringID));
			SendClientItemMessage(this.ItemDeSelected, item, ESESubTypeItem.ItemDeSelected);
		}
	}

	protected virtual void OnItemUsable(CItem item)
	{
		if (this.ItemUsable != null)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventItem(ESESubTypeItem.ItemUsable, item.YMLData.ItemType, item.YMLData.Slot, item.YMLData.StringID));
			SendClientItemMessage(this.ItemUsable, item, ESESubTypeItem.ItemUsable);
		}
	}

	protected virtual void OnItemNoLongerUsable(CItem item)
	{
		if (this.ItemNoLongerUsable != null)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventItem(ESESubTypeItem.ItemNoLongerUsable, item.YMLData.ItemType, item.YMLData.Slot, item.YMLData.StringID));
			SendClientItemMessage(this.ItemNoLongerUsable, item, ESESubTypeItem.ItemNoLongerUsable);
		}
	}

	protected virtual void OnItemActive(CItem item)
	{
		if (this.ItemActive != null)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventItem(ESESubTypeItem.ItemActive, item.YMLData.ItemType, item.YMLData.Slot, item.YMLData.StringID));
			SendClientItemMessage(this.ItemActive, item, ESESubTypeItem.ItemActive);
		}
	}

	public int GetSmallItemMax()
	{
		int num = (int)Math.Ceiling((float)Level / 2f) + SmallItemOverride;
		if (SmallItemSlots != null && SmallItemSlots.Length != 0)
		{
			CItem[] array = new CItem[num];
			for (int i = 0; i < SmallItemSlots.Length; i++)
			{
				array[i] = SmallItemSlots[i];
			}
			SmallItemSlots = new CItem[num];
			for (int j = 0; j < num; j++)
			{
				SmallItemSlots[j] = array[j];
			}
		}
		else
		{
			SmallItemSlots = new CItem[num];
		}
		return num;
	}

	public void CheckSmallItemOverride(CItem item, bool add = false)
	{
		if (item.YMLData.Data.SmallSlots != 0)
		{
			if (add)
			{
				SmallItemOverride = Math.Min(2, SmallItemOverride + item.YMLData.Data.SmallSlots);
			}
			else
			{
				SmallItemOverride = Math.Max(0, SmallItemOverride - item.YMLData.Data.SmallSlots);
			}
		}
		GetSmallItemMax();
	}

	public CInventory(int characterLevel, CActor inventoryActor)
	{
		InventoryActor = inventoryActor;
		HeadSlot = null;
		BodySlot = null;
		OneHandSlots = new CItem[2];
		TwoHandSlot = null;
		LegSlot = null;
		Level = characterLevel;
		GetSmallItemMax();
		SmallItemSlots = new CItem[SmallItemMax];
		QuestItemSlots = new CItem[QuestItemMax];
	}

	public bool AddItem(CItem item, bool overrideExisting = false)
	{
		item.SlotState = CItem.EItemSlotState.Equipped;
		CheckSmallItemOverride(item, add: true);
		switch (item.YMLData.Slot)
		{
		case CItem.EItemSlot.Head:
			if (HeadSlot == null || overrideExisting)
			{
				RemoveItem(HeadSlot);
				HeadSlot = item;
				OnItemAdded(item);
				return true;
			}
			break;
		case CItem.EItemSlot.Body:
			if (BodySlot == null || overrideExisting)
			{
				RemoveItem(BodySlot);
				BodySlot = item;
				OnItemAdded(item);
				return true;
			}
			break;
		case CItem.EItemSlot.Legs:
			if (LegSlot == null || overrideExisting)
			{
				RemoveItem(LegSlot);
				LegSlot = item;
				OnItemAdded(item);
				return true;
			}
			break;
		case CItem.EItemSlot.TwoHand:
			if ((TwoHandSlot == null && OneHandSlots[0] == null && OneHandSlots[1] == null) || overrideExisting)
			{
				RemoveItem(TwoHandSlot);
				RemoveItem(OneHandSlots[0]);
				RemoveItem(OneHandSlots[1]);
				TwoHandSlot = item;
				OnItemAdded(item);
				return true;
			}
			break;
		case CItem.EItemSlot.OneHand:
			if (overrideExisting || (TwoHandSlot == null && ((!item.IsSlotIndexSet && (OneHandSlots[0] == null || OneHandSlots[1] == null)) || (item.IsSlotIndexSet && OneHandSlots[item.SlotIndex] == null))))
			{
				int num2 = ((item.SlotIndex != int.MaxValue) ? item.SlotIndex : ((OneHandSlots[0] != null) ? 1 : 0));
				if (OneHandSlots[num2] != null)
				{
					num2 ^= 1;
					DLLDebug.Log("Add One Hand item " + item.Name + ". Slot " + item.SlotIndex + " full, switching to slot " + num2);
					item.SlotIndex = num2;
				}
				RemoveItem(TwoHandSlot);
				RemoveItem(OneHandSlots[num2]);
				OneHandSlots[num2] = item;
				OnItemAdded(item);
				return true;
			}
			break;
		case CItem.EItemSlot.SmallItem:
			if (!item.IsSlotIndexSet)
			{
				int num3 = 0;
				for (int j = 0; j < SmallItemMax; j++)
				{
					if (SmallItemSlots[j] == null)
					{
						num3 = j;
						break;
					}
				}
				if (SmallItemSlots[num3] == null || overrideExisting)
				{
					RemoveItem(SmallItemSlots[num3]);
					SmallItemSlots[num3] = item;
					OnItemAdded(item);
					return true;
				}
				break;
			}
			if (item.SlotIndex >= SmallItemSlots.Length)
			{
				DLLDebug.LogWarning("Invalid Small Item Slot " + item.SlotIndex + " for item " + item.Name + ".  Slot Index must be " + (SmallItemSlots.Length - 1) + " or less.  Character " + InventoryActor.Class.ID + " is level " + InventoryActor.Level + " and has " + SmallItemMax + " Small Item slots.");
				return false;
			}
			if (overrideExisting || SmallItemSlots[item.SlotIndex] == null)
			{
				RemoveItem(SmallItemSlots[item.SlotIndex]);
				SmallItemSlots[item.SlotIndex] = item;
				OnItemAdded(item);
				return true;
			}
			break;
		case CItem.EItemSlot.QuestItem:
			if (!item.IsSlotIndexSet)
			{
				int num = 0;
				for (int i = 0; i < QuestItemMax; i++)
				{
					if (QuestItemSlots[i] == null)
					{
						num = i;
						break;
					}
				}
				if (QuestItemSlots[num] == null || overrideExisting)
				{
					RemoveItem(QuestItemSlots[num]);
					QuestItemSlots[num] = item;
					OnItemAdded(item);
					return true;
				}
				break;
			}
			if (item.SlotIndex >= QuestItemSlots.Length)
			{
				DLLDebug.LogWarning("Invalid Quest Item Slot " + item.SlotIndex + " for item " + item.Name + ".  Slot Index must be " + (QuestItemSlots.Length - 1) + " or less. Character  has " + QuestItemMax + " Quest Item slots.");
				return false;
			}
			if (overrideExisting || QuestItemSlots[item.SlotIndex] == null)
			{
				RemoveItem(QuestItemSlots[item.SlotIndex]);
				QuestItemSlots[item.SlotIndex] = item;
				OnItemAdded(item);
				return true;
			}
			break;
		}
		return false;
	}

	public void ToggleItem(CItem item)
	{
		if (item.SlotState != CItem.EItemSlotState.Selected && item.YMLData.ItemType == CItem.EItemType.Ability)
		{
			CItem cItem = SelectedItems.SingleOrDefault((CItem a) => a.YMLData.ItemType == CItem.EItemType.Ability);
			if (cItem != null)
			{
				CheckForItemAbilityOverridesAndSubAbilitiesToDeselect();
				DeselectItemAbility(cItem);
			}
		}
		if (item.SlotState != CItem.EItemSlotState.Selected)
		{
			if (item.YMLData.ItemType == CItem.EItemType.Ability)
			{
				if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
				{
					cPhaseAction.StackItemAbilities(item);
					return;
				}
				CPhase currentPhase = PhaseManager.CurrentPhase;
				if (!(currentPhase is CPhaseStartTurn))
				{
					if (!(currentPhase is CPhaseActionSelection))
					{
						if (currentPhase is CPhaseEndRound || currentPhase is CPhaseEndTurn)
						{
							if (item.YMLData.Trigger == CItem.EItemTrigger.DuringOwnTurn)
							{
								ToggleItemAbility(item);
							}
							else
							{
								DLLDebug.LogError("Item triggered at invalid phase");
							}
						}
					}
					else if (item.YMLData.Trigger == CItem.EItemTrigger.DuringOwnTurn || (item.YMLData.Trigger == CItem.EItemTrigger.AtEndOfTurn && (GameState.CurrentActionSelectionSequence == GameState.ActionSelectionSequenceType.Complete || InventoryActor.Class is CCharacterClass { HasLongRested: not false })))
					{
						ToggleItemAbility(item);
					}
					else
					{
						DLLDebug.LogError("Item triggered at invalid phase");
					}
				}
				else if (item.YMLData.Trigger == CItem.EItemTrigger.AtStartOfRound || item.YMLData.Trigger == CItem.EItemTrigger.DuringOwnTurn)
				{
					ToggleItemAbility(item);
				}
				else
				{
					DLLDebug.LogError("Item triggered at invalid phase");
				}
			}
			else if (item.YMLData.ItemType == CItem.EItemType.Override)
			{
				SelectItem(item);
				bool flag = false;
				if (item.YMLData.Consumes.Count == 0 || (item.YMLData.Consumes.Count > 0 && (item.YMLData.Consumes[0] != ElementInfusionBoardManager.EElement.Any || (item.YMLData.Consumes[0] == ElementInfusionBoardManager.EElement.Any && item.ChosenElement != null && item.ChosenElement.Count > 0))))
				{
					if (PhaseManager.CurrentPhase is CPhaseAction)
					{
						CPhaseAction cPhaseAction2 = PhaseManager.CurrentPhase as CPhaseAction;
						if (cPhaseAction2.CurrentPhaseAbility?.m_Ability != null)
						{
							if (item.YMLData.Trigger == CItem.EItemTrigger.EntireAction)
							{
								foreach (CAbilityOverride @override in item.YMLData.Data.Overrides)
								{
									if ((item.YMLData.Data.CompareAbility == null || item.YMLData.Data.CompareAbility.CompareAbility(cPhaseAction2.CurrentPhaseAbility.m_Ability)) && (item.YMLData.Data.ItemRequirements == null || item.YMLData.Data.ItemRequirements.MeetsAbilityRequirements(cPhaseAction2.CurrentPhaseAbility.m_Ability.TargetingActor, cPhaseAction2.CurrentPhaseAbility.m_Ability)))
									{
										flag = true;
										cPhaseAction2.CurrentPhaseAbility.m_Ability.OverrideAbilityValues(@override, perform: false, item);
									}
									foreach (CPhaseAction.CPhaseAbility item2 in cPhaseAction2.RemainingPhaseAbilities.Where((CPhaseAction.CPhaseAbility w) => !w.ItemID.HasValue))
									{
										if ((item.YMLData.Data.CompareAbility == null || item.YMLData.Data.CompareAbility.CompareAbility(item2.m_Ability)) && (item.YMLData.Data.ItemRequirements == null || item.YMLData.Data.ItemRequirements.MeetsAbilityRequirements(item2.m_Ability.TargetingActor, item2.m_Ability)))
										{
											flag = true;
											item2.m_Ability.OverrideAbilityValues(@override, perform: false, item);
										}
									}
									if (@override.SubAbilities == null)
									{
										continue;
									}
									foreach (CAbility subAbility in @override.SubAbilities)
									{
										flag = true;
										CAbility cAbility = CAbility.CopyAbility(subAbility, generateNewID: false);
										cAbility.ParentAbility = cPhaseAction2.CurrentPhaseAbility.m_Ability;
										if (!cAbility.IsInlineSubAbility)
										{
											cPhaseAction2.RemainingPhaseAbilities.Add(new CPhaseAction.CPhaseAbility(cAbility, cPhaseAction2.CurrentPhaseAbility.TargetingActor, item, null, item.ID));
										}
									}
								}
								if (flag)
								{
									cPhaseAction2.CurrentPhaseAbility.m_Ability.Perform();
								}
							}
							else if (item.YMLData.Trigger == CItem.EItemTrigger.SingleAbility)
							{
								foreach (CAbilityOverride override2 in item.YMLData.Data.Overrides)
								{
									cPhaseAction2.CurrentPhaseAbility.m_Ability.OverrideAbilityValues(override2, perform: false, item);
									if (override2.SubAbilities == null)
									{
										continue;
									}
									foreach (CAbility subAbility2 in override2.SubAbilities)
									{
										CAbility cAbility2 = CAbility.CopyAbility(subAbility2, generateNewID: false);
										cAbility2.ParentAbility = cPhaseAction2.CurrentPhaseAbility.m_Ability;
										cPhaseAction2.RemainingPhaseAbilities.Add(new CPhaseAction.CPhaseAbility(cAbility2, cPhaseAction2.CurrentPhaseAbility.TargetingActor, item, null, item.ID));
									}
								}
								cPhaseAction2.CurrentPhaseAbility.m_Ability.Perform();
							}
							else if (item.YMLData.Trigger == CItem.EItemTrigger.SingleTarget)
							{
								cPhaseAction2.CurrentPhaseAbility.m_Ability.ToggleSingleTargetItem(item);
							}
							else
							{
								DLLDebug.LogError("Invalid Item Trigger");
							}
						}
						else
						{
							DLLDebug.LogError("Invalid phase for item");
						}
					}
					else
					{
						DLLDebug.LogError("Invalid phase for item");
					}
				}
				else
				{
					DLLDebug.Log("Cannot activate " + item.YMLData.StringID + ". No Element selected.");
				}
			}
			else
			{
				DLLDebug.LogError("Invalid Item Type");
			}
		}
		else if (item.YMLData.ItemType == CItem.EItemType.Ability)
		{
			CheckForItemAbilityOverridesAndSubAbilitiesToDeselect();
			DeselectItemAbility(item);
		}
		else if (item.YMLData.ItemType == CItem.EItemType.Override)
		{
			if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction3)
			{
				if (cPhaseAction3.CurrentPhaseAbility?.m_Ability != null)
				{
					UndoOverridesAndSubAbilities(item, cPhaseAction3);
					DeselectItem(item);
					if (item.YMLData.Trigger == CItem.EItemTrigger.SingleAbility || (item.YMLData.Trigger == CItem.EItemTrigger.EntireAction && (item.YMLData.Data.CompareAbility == null || item.YMLData.Data.CompareAbility.CompareAbility(cPhaseAction3.CurrentPhaseAbility.m_Ability)) && (item.YMLData.Data.ItemRequirements == null || item.YMLData.Data.ItemRequirements.MeetsAbilityRequirements(cPhaseAction3.CurrentPhaseAbility.m_Ability.TargetingActor, cPhaseAction3.CurrentPhaseAbility.m_Ability))))
					{
						cPhaseAction3.CurrentPhaseAbility.m_Ability.Perform();
					}
					else if (item.YMLData.Trigger == CItem.EItemTrigger.SingleTarget)
					{
						cPhaseAction3.CurrentPhaseAbility.m_Ability.ToggleSingleTargetItem(item);
						cPhaseAction3.CurrentPhaseAbility.m_Ability.TargetingActor.Inventory.HighlightUsableItems(cPhaseAction3.CurrentPhaseAbility.m_Ability, CItem.EItemTrigger.DuringOwnTurn, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility, CItem.EItemTrigger.SingleTarget);
					}
				}
				else
				{
					DLLDebug.LogError("Unable to undo item override.  No valid ability in phase.");
				}
			}
			else
			{
				DLLDebug.LogError("Invalid phase for item");
			}
		}
		else
		{
			DLLDebug.LogError("Invalid Item Type");
		}
	}

	private void ToggleItemAbility(CItem item)
	{
		if (item.YMLData.ItemType == CItem.EItemType.Ability)
		{
			if (item.SlotState == CItem.EItemSlotState.Selected)
			{
				DeselectItem(item);
				PhaseManager.EndItemAbilities();
			}
			else if (item.SlotState != CItem.EItemSlotState.Locked)
			{
				SelectItem(item);
				PhaseManager.StartItemAbilities(item);
			}
		}
	}

	private void DeselectItemAbility(CItem item)
	{
		if (PhaseManager.PreviousPhase == null)
		{
			DeselectItem(item);
			if (PhaseManager.CurrentPhase is CPhaseAction)
			{
				(PhaseManager.CurrentPhase as CPhaseAction).UnstackItemAbilities(item);
			}
			else
			{
				DLLDebug.LogError("Invalid phase for item");
			}
		}
		else
		{
			ToggleItemAbility(item);
		}
	}

	private void CheckForItemAbilityOverridesAndSubAbilitiesToDeselect()
	{
		List<CItem> list = SelectedItems.FindAll((CItem a) => a.YMLData.ItemType == CItem.EItemType.Override);
		if (list.Count <= 0)
		{
			return;
		}
		CPhaseAction cPhaseAction = PhaseManager.CurrentPhase as CPhaseAction;
		if (cPhaseAction.CurrentPhaseAbility?.m_Ability == null)
		{
			return;
		}
		foreach (CItem item in list)
		{
			if (UndoOverridesAndSubAbilities(item, cPhaseAction))
			{
				DeselectItem(item);
			}
		}
	}

	private bool UndoOverridesAndSubAbilities(CItem item, CPhaseAction phaseAction)
	{
		bool result = false;
		if (item.YMLData.Trigger == CItem.EItemTrigger.EntireAction)
		{
			foreach (CAbilityOverride @override in item.YMLData.Data.Overrides)
			{
				if (item.YMLData.Data.CompareAbility == null || item.YMLData.Data.CompareAbility.CompareAbility(phaseAction.CurrentPhaseAbility.m_Ability))
				{
					result = true;
					phaseAction.CurrentPhaseAbility.m_Ability.UndoOverride(@override, perform: false, item);
				}
				foreach (CPhaseAction.CPhaseAbility item2 in phaseAction.RemainingPhaseAbilities.Where((CPhaseAction.CPhaseAbility w) => !w.ItemID.HasValue))
				{
					if (item.YMLData.Data.CompareAbility == null || item.YMLData.Data.CompareAbility.CompareAbility(item2.m_Ability))
					{
						result = true;
						item2.m_Ability.UndoOverride(@override, perform: false, item);
					}
				}
				if (@override.SubAbilities != null)
				{
					result = true;
					phaseAction.RemainingPhaseAbilities.RemoveAll((CPhaseAction.CPhaseAbility r) => r.ItemID == item.ID);
				}
			}
		}
		else if (item.YMLData.Trigger == CItem.EItemTrigger.SingleAbility)
		{
			foreach (CAbilityOverride override2 in item.YMLData.Data.Overrides)
			{
				result = true;
				phaseAction.CurrentPhaseAbility.m_Ability.UndoOverride(override2, perform: false, item);
				if (override2.SubAbilities != null)
				{
					phaseAction.RemainingPhaseAbilities.RemoveAll((CPhaseAction.CPhaseAbility r) => r.ItemID == item.ID);
				}
			}
		}
		return result;
	}

	public bool SelectItem(CItem item)
	{
		if (item.SlotState == CItem.EItemSlotState.Selected || item.SlotState == CItem.EItemSlotState.Locked)
		{
			return false;
		}
		item.SlotState = CItem.EItemSlotState.Selected;
		OnItemSelected(item);
		return true;
	}

	public bool DeselectItem(CItem item)
	{
		if (item.SlotState != CItem.EItemSlotState.Selected)
		{
			return false;
		}
		item.SlotState = CItem.EItemSlotState.Useable;
		OnItemDeSelected(item);
		if (item.YMLData.ItemType == CItem.EItemType.Ability)
		{
			CClearWaypointsAndTargets_MessageData message = new CClearWaypointsAndTargets_MessageData();
			ScenarioRuleClient.MessageHandler(message);
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
		}
		return true;
	}

	public void RefreshItems(CItem.EItemSlotState slotStateToRefresh, CItem.EItemSlot itemSlot = CItem.EItemSlot.None, CItem.EUsageType usage = CItem.EUsageType.None)
	{
		foreach (CItem item in AllItems.Where((CItem w) => w.SlotState == slotStateToRefresh && (itemSlot == CItem.EItemSlot.None || w.YMLData.Slot == itemSlot) && (usage == CItem.EUsageType.None || w.YMLData.Usage == usage)))
		{
			item.SlotState = CItem.EItemSlotState.Equipped;
			OnItemRefreshed(item, InventoryActor);
			CancelActiveBonusesOnRefreshingItems(item);
		}
	}

	public void RemoveItem(CItem item)
	{
		if (item == null)
		{
			return;
		}
		DeselectItem(item);
		ReactivateItem(item);
		CheckSmallItemOverride(item);
		switch (item.YMLData.Slot)
		{
		case CItem.EItemSlot.Head:
			HeadSlot = null;
			break;
		case CItem.EItemSlot.Body:
			BodySlot = null;
			break;
		case CItem.EItemSlot.OneHand:
			if (OneHandSlots[0] == item)
			{
				OneHandSlots[0] = null;
			}
			else if (OneHandSlots[1] == item)
			{
				OneHandSlots[1] = null;
			}
			break;
		case CItem.EItemSlot.TwoHand:
			TwoHandSlot = null;
			break;
		case CItem.EItemSlot.Legs:
			LegSlot = null;
			break;
		case CItem.EItemSlot.SmallItem:
		{
			for (int j = 0; j < SmallItemMax; j++)
			{
				if (SmallItemSlots[j] == item)
				{
					SmallItemSlots[j] = null;
					break;
				}
			}
			break;
		}
		case CItem.EItemSlot.QuestItem:
		{
			for (int i = 0; i < QuestItemMax; i++)
			{
				if (QuestItemSlots[i] == item)
				{
					QuestItemSlots[i] = null;
					break;
				}
			}
			break;
		}
		}
	}

	public List<CItem> GetItemsInSlot(CItem.EItemSlot slot)
	{
		return AllItems.Where((CItem x) => x.YMLData.Slot == slot).ToList();
	}

	public bool IsItemUsed(CItem item)
	{
		if (item.SlotState != CItem.EItemSlotState.Consumed)
		{
			return item.SlotState == CItem.EItemSlotState.Spent;
		}
		return true;
	}

	public bool IsItemUsedOrActive(CItem item)
	{
		if (item.SlotState != CItem.EItemSlotState.Consumed && item.SlotState != CItem.EItemSlotState.Spent)
		{
			return item.SlotState == CItem.EItemSlotState.Active;
		}
		return true;
	}

	public void ReactivateItem(CItem item, CActor actor = null)
	{
		if (IsItemUsed(item))
		{
			item.SlotState = CItem.EItemSlotState.Equipped;
			OnItemRefreshed(item, InventoryActor);
			CancelActiveBonusesOnRefreshingItems(item);
			actor?.ActivatePassiveItems(firstLoad: true, null, item);
		}
	}

	private void CancelActiveBonusesOnRefreshingItems(CItem item)
	{
		if (!(InventoryActor.Class is CCharacterClass cCharacterClass))
		{
			return;
		}
		for (int num = cCharacterClass.ActivatedCards.Count - 1; num >= 0; num--)
		{
			CBaseCard cBaseCard = cCharacterClass.ActivatedCards[num];
			if (cBaseCard.CardType == CBaseCard.ECardType.Item && cBaseCard.ID.Equals(item.ID) && cBaseCard.ActiveBonuses.Count > 0)
			{
				for (int num2 = cBaseCard.ActiveBonuses.Count - 1; num2 >= 0; num2--)
				{
					CClass.CancelActiveBonus(cBaseCard.ActiveBonuses[num2]);
				}
			}
		}
	}

	public void ClearSelectedItems()
	{
		foreach (CItem item in AllItems.Where((CItem w) => w.SlotState == CItem.EItemSlotState.Selected))
		{
			DeselectItem(item);
		}
	}

	public int GetItemShieldValue()
	{
		return GameState.RecievedDamagedReducedByShieldItems;
	}

	public void HandleUsedItems()
	{
		foreach (CItem item in AllItems.Where((CItem w) => w.SlotState == CItem.EItemSlotState.Selected || w.SlotState == CItem.EItemSlotState.Locked))
		{
			bool flag = false;
			if (InventoryActor is CPlayerActor cPlayerActor && cPlayerActor.CharacterClass.ActivatedCards.FindAll((CBaseCard x) => x is CItem).SingleOrDefault((CBaseCard s) => s.ID.Equals(item.ID)) is CItem cItem)
			{
				List<CActiveBonus> activeBonuses = cItem.ActiveBonuses;
				if (activeBonuses != null && activeBonuses.Count > 0)
				{
					flag = true;
				}
			}
			if (flag)
			{
				HandleActiveItemTriggered(item);
			}
			else
			{
				HandleUsedItem(item);
			}
		}
	}

	public void HandleUsedItem(int ItemID, CActiveBonus activeBonus = null)
	{
		CItem cItem = AllItems.SingleOrDefault((CItem s) => s.ID == ItemID);
		if (cItem != null)
		{
			HandleUsedItem(cItem, activeBonus);
		}
	}

	public void HandleUsedItems(params CItem.EItemTrigger[] triggers)
	{
		foreach (CItem allItem in AllItems)
		{
			foreach (CItem.EItemTrigger eItemTrigger in triggers)
			{
				if (allItem.YMLData.Trigger.HasFlag(eItemTrigger))
				{
					HandleUsedItem(allItem);
					break;
				}
			}
		}
	}

	public void HandleActiveItemTriggered(CItem item, bool ignoreConsume = false)
	{
		if (item.SlotState != CItem.EItemSlotState.Selected && item.SlotState != CItem.EItemSlotState.Locked && item.SlotState != CItem.EItemSlotState.None)
		{
			return;
		}
		if (item.SlotState != CItem.EItemSlotState.None && !ignoreConsume)
		{
			int num = 0;
			foreach (ElementInfusionBoardManager.EElement consume in item.YMLData.Consumes)
			{
				ElementInfusionBoardManager.EElement eElement = consume;
				if (eElement == ElementInfusionBoardManager.EElement.Any && item.ChosenElement.Count > num)
				{
					eElement = item.ChosenElement[num];
					num++;
				}
				ElementInfusionBoardManager.Consume(eElement, InventoryActor);
			}
		}
		item.SlotState = CItem.EItemSlotState.Active;
		OnItemActive(item);
	}

	public void HandleUsedItem(CItem item, CActiveBonus activeBonus = null)
	{
		if (item.SlotState != CItem.EItemSlotState.Selected && item.SlotState != CItem.EItemSlotState.Locked && item.SlotState != CItem.EItemSlotState.Active && item.SlotState != CItem.EItemSlotState.Spent)
		{
			return;
		}
		if (item.SlotState != CItem.EItemSlotState.Active)
		{
			int num = 0;
			foreach (ElementInfusionBoardManager.EElement consume in item.YMLData.Consumes)
			{
				ElementInfusionBoardManager.EElement eElement = consume;
				if (eElement == ElementInfusionBoardManager.EElement.Any && item.ChosenElement.Count > num)
				{
					eElement = item.ChosenElement[num];
					num++;
				}
				ElementInfusionBoardManager.Consume(eElement, InventoryActor);
			}
			if (activeBonus != null)
			{
				CAbility ability = activeBonus.Ability;
				if (ability != null && ability.ActiveBonusData?.Consuming.Count > 0)
				{
					foreach (ElementInfusionBoardManager.EElement item2 in activeBonus.Ability.ActiveBonusData.Consuming)
					{
						ElementInfusionBoardManager.Consume((item2 == ElementInfusionBoardManager.EElement.Any) ? (activeBonus.ToggledElement.HasValue ? activeBonus.ToggledElement.Value : item2) : item2, InventoryActor);
					}
				}
			}
		}
		bool flag = ((activeBonus != null) ? (!activeBonus.HasTracker) : (item.YMLData.Data.Abilities?.SingleOrDefault(delegate(CAbility x)
		{
			AbilityData.ActiveBonusData activeBonusData = x.ActiveBonusData;
			return activeBonusData != null && activeBonusData.Tracker?.Count > 0;
		}) == null));
		if (item.YMLData.Usage == CItem.EUsageType.Spent && item.SlotState != CItem.EItemSlotState.Spent)
		{
			item.SlotState = CItem.EItemSlotState.Spent;
			item.RoundUsed = ScenarioManager.CurrentScenarioState.RoundNumber;
			OnItemSpent(item);
			if (flag)
			{
				InventoryActor.UsedItem(item, firstTimeUse: true);
			}
		}
		else if (item.YMLData.Usage == CItem.EUsageType.Consumed && item.SlotState != CItem.EItemSlotState.Consumed)
		{
			item.SlotState = CItem.EItemSlotState.Consumed;
			item.RoundUsed = ScenarioManager.CurrentScenarioState.RoundNumber;
			OnItemConsumed(item);
			if (flag)
			{
				InventoryActor.UsedItem(item, firstTimeUse: true);
			}
		}
		else if (item.YMLData.Usage == CItem.EUsageType.Unrestricted)
		{
			item.SlotState = CItem.EItemSlotState.Equipped;
			item.RoundUsed = ScenarioManager.CurrentScenarioState.RoundNumber;
			OnItemUnrestrictedUsed(item);
			if (flag)
			{
				InventoryActor.UsedItem(item, firstTimeUse: true);
			}
		}
	}

	public void LockInSelectedItemsAndResetUnselected()
	{
		foreach (CItem item in AllItems.Where((CItem w) => w.SlotState == CItem.EItemSlotState.Selected))
		{
			item.SlotState = CItem.EItemSlotState.Locked;
		}
		foreach (CItem item2 in AllItems.Where((CItem w) => w.SlotState == CItem.EItemSlotState.Useable))
		{
			item2.SlotState = CItem.EItemSlotState.Equipped;
			OnItemNoLongerUsable(item2);
		}
	}

	public void OnAbilityEnd()
	{
		foreach (CItem item in AllItems.Where((CItem w) => w.SlotState == CItem.EItemSlotState.Locked))
		{
			item.SlotState = CItem.EItemSlotState.Selected;
		}
	}

	public void HighlightUsableItems(CAbility ability, params CItem.EItemTrigger[] acceptedTriggers)
	{
		HighlightUsableItems(ability, (List<CActor>)null, acceptedTriggers);
	}

	public void HighlightUsableItems(CAbility ability, CActor targetActor, params CItem.EItemTrigger[] acceptedTriggers)
	{
		HighlightUsableItems(ability, new List<CActor> { targetActor }, acceptedTriggers);
	}

	public void HighlightUsableItems(CAbility ability, List<CActor> targetActors, params CItem.EItemTrigger[] acceptedTriggers)
	{
		foreach (CItem item in AllItems.Where((CItem w) => w.SlotState != CItem.EItemSlotState.Selected && w.SlotState != CItem.EItemSlotState.Locked && !IsItemUsedOrActive(w)))
		{
			if (!InventoryActor.ItemLocked && acceptedTriggers.Contains(item.YMLData.Trigger) && CheckItemConsumes(item) && CheckConditions(GameState.InternalCurrentActor, item))
			{
				if (targetActors != null && targetActors.Count > 0)
				{
					foreach (CActor targetActor in targetActors)
					{
						if ((ability == null || item.YMLData.Data.CompareAbility == null || item.YMLData.Data.CompareAbility.CompareAbility(ability, targetActor)) && (item.YMLData.Data.ItemRequirements == null || item.YMLData.Data.ItemRequirements.MeetsAbilityRequirements(targetActor, ability)))
						{
							item.SlotState = CItem.EItemSlotState.Useable;
							OnItemUsable(item);
							break;
						}
						if (item.SlotState == CItem.EItemSlotState.Useable)
						{
							item.SlotState = CItem.EItemSlotState.Equipped;
							OnItemNoLongerUsable(item);
						}
					}
				}
				else if ((ability == null || item.YMLData.Data.CompareAbility == null || item.YMLData.Data.CompareAbility.CompareAbility(ability)) && (item.YMLData.Data.ItemRequirements == null || item.YMLData.Data.ItemRequirements.MeetsAbilityRequirements(InventoryActor, ability)))
				{
					item.SlotState = CItem.EItemSlotState.Useable;
					OnItemUsable(item);
				}
				else if (item.SlotState == CItem.EItemSlotState.Useable)
				{
					item.SlotState = CItem.EItemSlotState.Equipped;
					OnItemNoLongerUsable(item);
				}
			}
			else if (item.SlotState == CItem.EItemSlotState.Useable)
			{
				item.SlotState = CItem.EItemSlotState.Equipped;
				OnItemNoLongerUsable(item);
			}
		}
	}

	public void Undo()
	{
		foreach (CItem item in AllItems.Where((CItem w) => w.SlotState == CItem.EItemSlotState.Selected))
		{
			item.SlotState = CItem.EItemSlotState.Useable;
			OnItemDeSelected(item);
		}
	}

	private bool CheckItemConsumes(CItem item)
	{
		List<ElementInfusionBoardManager.EElement> availableElements = ElementInfusionBoardManager.GetAvailableElements();
		if (item.YMLData.Consumes.Contains(ElementInfusionBoardManager.EElement.Any) && availableElements.Count > 0)
		{
			return true;
		}
		foreach (ElementInfusionBoardManager.EElement consume in item.YMLData.Consumes)
		{
			if (!availableElements.Contains(consume))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckConditions(CActor actor, CItem item)
	{
		if (item.YMLData.Data.CompareConditions == null || item.YMLData.Data.CompareConditions.Count == 0)
		{
			return true;
		}
		foreach (string compareCondition in item.YMLData.Data.CompareConditions)
		{
			if (compareCondition == "AnyNegative")
			{
				if (actor.Tokens.GetAllNegativeConditions().Count > 0)
				{
					return true;
				}
				return false;
			}
			if (compareCondition == "AnyPositive")
			{
				if (actor.Tokens.GetAllPositiveConditions().Count > 0)
				{
					return true;
				}
				return false;
			}
			if (Enum.TryParse<CCondition.ENegativeCondition>(compareCondition, out var result))
			{
				if (actor.Tokens.HasKey(result))
				{
					return true;
				}
				return false;
			}
			if (Enum.TryParse<CCondition.EPositiveCondition>(compareCondition, out var result2))
			{
				if (actor.Tokens.HasKey(result2))
				{
					return true;
				}
				return false;
			}
		}
		return true;
	}

	public CItem GetItem(CItem.EItemSlot slot, int index = 0)
	{
		return slot switch
		{
			CItem.EItemSlot.Head => HeadSlot, 
			CItem.EItemSlot.Body => BodySlot, 
			CItem.EItemSlot.OneHand => OneHandSlots[index], 
			CItem.EItemSlot.TwoHand => TwoHandSlot, 
			CItem.EItemSlot.Legs => LegSlot, 
			CItem.EItemSlot.SmallItem => SmallItemSlots[index], 
			CItem.EItemSlot.QuestItem => QuestItemSlots[index], 
			_ => null, 
		};
	}

	public void UseItem(CItem item)
	{
		if (item.YMLData.Usage == CItem.EUsageType.Spent && item.SlotState != CItem.EItemSlotState.Spent)
		{
			item.SlotState = CItem.EItemSlotState.Spent;
			OnItemSpent(item);
		}
		else if (item.YMLData.Usage == CItem.EUsageType.Consumed && item.SlotState != CItem.EItemSlotState.Consumed)
		{
			item.SlotState = CItem.EItemSlotState.Consumed;
			OnItemConsumed(item);
		}
	}
}
