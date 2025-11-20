using AStar;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CObjectActor : CEnemyActor
{
	private CObjectProp m_PropAttachedTo;

	public CObjectProp AttachedProp => m_PropAttachedTo;

	public bool IsAttachedToProp => m_PropAttachedTo != null;

	public CObjectActor()
	{
	}

	public CObjectActor(CObjectActor state, ReferenceDictionary references)
		: base(state, references)
	{
		m_PropAttachedTo = references.Get(state.m_PropAttachedTo);
		if (m_PropAttachedTo == null && state.m_PropAttachedTo != null)
		{
			m_PropAttachedTo = new CObjectProp(state.m_PropAttachedTo, references);
			references.Add(state.m_PropAttachedTo, m_PropAttachedTo);
		}
	}

	public CObjectActor(Point startPosition, CObjectClass objectClass, EType type, string actorGuid, int currentHealth, int maxHealth, int level, int standeeID, bool initial, int chosenModelIndex)
		: base(startPosition, objectClass, type, isSummon: false, string.Empty, actorGuid, currentHealth, maxHealth, level, standeeID, initial, chosenModelIndex)
	{
	}

	public override int Initiative()
	{
		if (base.MonsterClass.RoundAbilityCard != null)
		{
			return base.MonsterClass.RoundAbilityCard.Initiative;
		}
		return 99;
	}

	public override string ActorLocKey()
	{
		if (IsAttachedToProp)
		{
			if (string.IsNullOrEmpty(m_PropAttachedTo.PropHealthDetails?.CustomLocKey))
			{
				return m_PropAttachedTo.PrefabName;
			}
			return m_PropAttachedTo.PropHealthDetails.CustomLocKey;
		}
		return base.ActorLocKey();
	}

	public override bool OnDeath(CActor targetingActor, ECauseOfDeath causeOfDeath, out bool startedOnDeathAbility, bool fromDeathAbilityComplete = false, CAbility causeOfDeathAbility = null, CAttackSummary.TargetSummary attackSummary = null)
	{
		if (IsAttachedToProp)
		{
			switch (m_PropAttachedTo.PropHealthDetails.DeathAction)
			{
			case PropHealthDetails.EDeathAction.Destruct:
				if (m_PropAttachedTo is CObjectObstacle)
				{
					m_PropAttachedTo.DestroyProp(causeOfDeathAbility?.SpawnDelay ?? 0f);
				}
				break;
			case PropHealthDetails.EDeathAction.Activate:
				if (m_PropAttachedTo is CObjectDoor cObjectDoor)
				{
					cObjectDoor.SetExtraLockState(lockedStateToSet: false, openDoorIfUnlocked: true, targetingActor);
				}
				else
				{
					m_PropAttachedTo.Activate(targetingActor);
				}
				break;
			}
		}
		return base.OnDeath(targetingActor, causeOfDeath, out startedOnDeathAbility, fromDeathAbilityComplete, causeOfDeathAbility, (CAttackSummary.TargetSummary)null);
	}

	protected override void OnHealthChanged()
	{
		base.OnHealthChanged();
		if (IsAttachedToProp)
		{
			m_PropAttachedTo.PropHealthDetails.CurrentHealth = base.Health;
		}
	}

	public new CObjectActor Clone()
	{
		return (CObjectActor)MemberwiseClone();
	}

	public void SetAttachedToProp(CObjectProp propAttachedTo)
	{
		if (propAttachedTo.PropHealthDetails == null || !propAttachedTo.PropHealthDetails.HasHealth)
		{
			DLLDebug.LogWarning($"Unable to create dummy ObjectActor for {propAttachedTo.ObjectType.ToString()} prop with GUID {propAttachedTo.PropGuid}, The prop is not configured for health Correctly");
			return;
		}
		m_PropAttachedTo = propAttachedTo;
		base.Type = propAttachedTo.PropHealthDetails.ActorType;
	}
}
