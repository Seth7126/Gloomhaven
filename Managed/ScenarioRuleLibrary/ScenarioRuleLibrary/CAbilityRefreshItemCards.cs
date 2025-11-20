using System.Collections.Generic;
using System.Linq;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityRefreshItemCards : CAbilityTargeting
{
	public static int STRENGTH_ALL = -1;

	public List<CItem.EItemSlot> SlotsToRefresh;

	public List<CItem.EItemSlotState> SlotStatesToRefresh;

	public CAbilityRefreshItemCards(List<CItem.EItemSlot> slotsToRefresh, List<CItem.EItemSlotState> slotStatesToRefresh)
		: base(EAbilityType.RefreshItemCards)
	{
		SlotsToRefresh = slotsToRefresh;
		SlotStatesToRefresh = slotStatesToRefresh;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		if (base.Strength != STRENGTH_ALL)
		{
			m_OneTargetAtATime = true;
		}
		base.Start(targetActor, filterActor, controllingActor);
	}

	public override bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		base.ActorIsApplying(actorApplying, actorsAppliedTo);
		CActorIsSelectingItemCards_MessageData message = new CActorIsSelectingItemCards_MessageData(base.AnimOverload, actorApplying)
		{
			m_ActorsRefreshed = actorsAppliedTo,
			m_ItemSelectionAbility = this
		};
		ScenarioRuleClient.MessageHandler(message);
		return false;
	}

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			base.AbilityHasHappened = true;
			if (actor.Type == CActor.EType.Player && actor.Inventory.AllItems.Where((CItem it) => it.YMLData.PermanentlyConsumed != true && (SlotStatesToRefresh == null || SlotStatesToRefresh.Count == 0 || SlotStatesToRefresh.Contains(it.SlotState)) && (SlotsToRefresh == null || SlotsToRefresh.Count == 0 || SlotsToRefresh.Contains(it.YMLData.Slot))).ToList().Count > 0)
			{
				CSelectRefreshOrConsumeItems_MessageData cSelectRefreshOrConsumeItems_MessageData = new CSelectRefreshOrConsumeItems_MessageData(base.TargetingActor, actor);
				cSelectRefreshOrConsumeItems_MessageData.m_Ability = this;
				cSelectRefreshOrConsumeItems_MessageData.m_SlotsToChooseFrom = SlotsToRefresh;
				cSelectRefreshOrConsumeItems_MessageData.m_SlotStatesToChooseFrom = SlotStatesToRefresh;
				ScenarioRuleClient.MessageHandler(cSelectRefreshOrConsumeItems_MessageData);
				if (base.Strength == STRENGTH_ALL)
				{
					actor.Inventory.HighlightUsableItems(null, CItem.EItemTrigger.DuringOwnTurn);
				}
			}
			if (m_PositiveConditions.Count > 0)
			{
				ProcessPositiveStatusEffects(actor);
			}
		}
		return true;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override void Reset()
	{
		base.Reset();
		STRENGTH_ALL = -1;
	}

	public CAbilityRefreshItemCards()
	{
	}

	public CAbilityRefreshItemCards(CAbilityRefreshItemCards state, ReferenceDictionary references)
		: base(state, references)
	{
		SlotsToRefresh = references.Get(state.SlotsToRefresh);
		if (SlotsToRefresh == null && state.SlotsToRefresh != null)
		{
			SlotsToRefresh = new List<CItem.EItemSlot>();
			for (int i = 0; i < state.SlotsToRefresh.Count; i++)
			{
				CItem.EItemSlot item = state.SlotsToRefresh[i];
				SlotsToRefresh.Add(item);
			}
			references.Add(state.SlotsToRefresh, SlotsToRefresh);
		}
		SlotStatesToRefresh = references.Get(state.SlotStatesToRefresh);
		if (SlotStatesToRefresh == null && state.SlotStatesToRefresh != null)
		{
			SlotStatesToRefresh = new List<CItem.EItemSlotState>();
			for (int j = 0; j < state.SlotStatesToRefresh.Count; j++)
			{
				CItem.EItemSlotState item2 = state.SlotStatesToRefresh[j];
				SlotStatesToRefresh.Add(item2);
			}
			references.Add(state.SlotStatesToRefresh, SlotStatesToRefresh);
		}
	}
}
