using System.Collections.Generic;
using System.Linq;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAbilityConsumeItemCards : CAbilityTargeting
{
	public static int STRENGTH_ALL = -1;

	public List<CItem.EItemSlot> SlotsToConsume;

	public List<CItem.EItemSlotState> SlotStatesToConsume;

	public CAbilityConsumeItemCards(List<CItem.EItemSlot> slotsToConsume, List<CItem.EItemSlotState> slotStatesToConsume)
		: base(EAbilityType.ConsumeItemCards)
	{
		SlotsToConsume = slotsToConsume;
		SlotStatesToConsume = slotStatesToConsume;
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
			if (actor.Type == CActor.EType.Player && actor.Inventory.AllItems.Where((CItem it) => (SlotStatesToConsume == null || SlotStatesToConsume.Count == 0 || SlotStatesToConsume.Contains(it.SlotState)) && (SlotsToConsume == null || SlotsToConsume.Count == 0 || SlotsToConsume.Contains(it.YMLData.Slot))).ToList().Count > 0)
			{
				CSelectRefreshOrConsumeItems_MessageData cSelectRefreshOrConsumeItems_MessageData = new CSelectRefreshOrConsumeItems_MessageData(base.TargetingActor, actor);
				cSelectRefreshOrConsumeItems_MessageData.m_Ability = this;
				cSelectRefreshOrConsumeItems_MessageData.m_SlotsToChooseFrom = SlotsToConsume;
				cSelectRefreshOrConsumeItems_MessageData.m_SlotStatesToChooseFrom = SlotStatesToConsume;
				ScenarioRuleClient.MessageHandler(cSelectRefreshOrConsumeItems_MessageData);
				if (base.Strength == CAbilityRefreshItemCards.STRENGTH_ALL)
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
		return false;
	}

	public override void Reset()
	{
		base.Reset();
		STRENGTH_ALL = -1;
	}

	public CAbilityConsumeItemCards()
	{
	}

	public CAbilityConsumeItemCards(CAbilityConsumeItemCards state, ReferenceDictionary references)
		: base(state, references)
	{
		SlotsToConsume = references.Get(state.SlotsToConsume);
		if (SlotsToConsume == null && state.SlotsToConsume != null)
		{
			SlotsToConsume = new List<CItem.EItemSlot>();
			for (int i = 0; i < state.SlotsToConsume.Count; i++)
			{
				CItem.EItemSlot item = state.SlotsToConsume[i];
				SlotsToConsume.Add(item);
			}
			references.Add(state.SlotsToConsume, SlotsToConsume);
		}
		SlotStatesToConsume = references.Get(state.SlotStatesToConsume);
		if (SlotStatesToConsume == null && state.SlotStatesToConsume != null)
		{
			SlotStatesToConsume = new List<CItem.EItemSlotState>();
			for (int j = 0; j < state.SlotStatesToConsume.Count; j++)
			{
				CItem.EItemSlotState item2 = state.SlotStatesToConsume[j];
				SlotStatesToConsume.Add(item2);
			}
			references.Add(state.SlotStatesToConsume, SlotStatesToConsume);
		}
	}
}
