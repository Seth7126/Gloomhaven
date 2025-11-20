using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;

namespace ScenarioRuleLibrary;

public class CMonsterClass : CClass
{
	public static List<EMonsterBaseStats> RefreshRequiringStatIsXTypes = new List<EMonsterBaseStats>
	{
		EMonsterBaseStats.Shield,
		EMonsterBaseStats.Retaliate
	};

	protected List<CMonsterAbilityCard> m_AbilityCardsPool = new List<CMonsterAbilityCard>();

	protected List<CMonsterAbilityCard> m_AbilityCards = new List<CMonsterAbilityCard>();

	protected List<CMonsterAbilityCard> m_DiscardedAbilityCards = new List<CMonsterAbilityCard>();

	protected CMonsterAbilityCard m_RoundAbilityCard;

	protected CAction[] m_BaseActions = new CAction[8];

	protected BaseStats m_CurrentMonsterStat;

	protected int m_MonsterStatLevel;

	private bool m_Boss;

	private bool m_PredominantlyMelee;

	private MonsterYMLData m_MonstersYML;

	private CMonsterClass m_NonEliteVariant;

	private BaseStats m_OriginalMonsterStats;

	protected List<int> m_AvailableIDs;

	public List<CMonsterAbilityCard> AbilityCardsPool => m_AbilityCardsPool;

	public List<CMonsterAbilityCard> AbilityCards
	{
		get
		{
			if (m_NonEliteVariant != null)
			{
				return m_NonEliteVariant.AbilityCards;
			}
			return m_AbilityCards;
		}
		private set
		{
			if (m_NonEliteVariant != null)
			{
				m_NonEliteVariant.AbilityCards = value;
			}
			else
			{
				m_AbilityCards = value;
			}
		}
	}

	public List<CMonsterAbilityCard> DiscardedAbilityCards
	{
		get
		{
			if (m_NonEliteVariant != null)
			{
				return m_NonEliteVariant.DiscardedAbilityCards;
			}
			return m_DiscardedAbilityCards;
		}
		private set
		{
			if (m_NonEliteVariant != null)
			{
				m_NonEliteVariant.DiscardedAbilityCards = value;
			}
			else
			{
				m_DiscardedAbilityCards = value;
			}
		}
	}

	public CMonsterAbilityCard RoundAbilityCard
	{
		get
		{
			if (m_NonEliteVariant != null)
			{
				return m_NonEliteVariant.RoundAbilityCard;
			}
			return m_RoundAbilityCard;
		}
		private set
		{
			if (m_NonEliteVariant != null)
			{
				m_NonEliteVariant.RoundAbilityCard = value;
			}
			else
			{
				m_RoundAbilityCard = value;
			}
		}
	}

	public List<CBaseCard> ActivatedCards { get; private set; }

	public CAction BaseAction => m_BaseActions[m_MonsterStatLevel];

	public int Shield => m_CurrentMonsterStat.Shield;

	public int Retaliate => m_CurrentMonsterStat.Retaliate;

	public int RetaliateRange => m_CurrentMonsterStat.RetaliateRange;

	public int Target => m_CurrentMonsterStat.Target;

	public int Pierce => m_CurrentMonsterStat.Pierce;

	public bool Advantage => m_CurrentMonsterStat.Advantage;

	public bool AttackersGainDisadv => m_CurrentMonsterStat.AttackersGainDisadvantage;

	public bool Boss => m_Boss;

	public bool PredominantlyMelee => m_PredominantlyMelee;

	public int Range => m_CurrentMonsterStat.Range;

	public int Attack => m_CurrentMonsterStat.Attack;

	public int Move => m_CurrentMonsterStat.Move;

	public List<CCondition.ENegativeCondition> Conditions => m_CurrentMonsterStat.Conditions.ToList();

	public List<CAbility.EAbilityType> Immunities => m_CurrentMonsterStat.Immunities.ToList();

	public bool Flying => m_CurrentMonsterStat.Flying;

	public bool Invulnerable => m_CurrentMonsterStat.Invulnerable;

	public bool PierceInvulnerability => m_CurrentMonsterStat.PierceInvulnerablility;

	public bool Untargetable => m_CurrentMonsterStat.Untargetable;

	public int StatLevel => m_MonsterStatLevel;

	public List<int> AvailableIDs => m_AvailableIDs;

	public BaseStats CurrentMonsterStat => m_CurrentMonsterStat;

	public BaseStats OriginalBaseStats => m_OriginalMonsterStats;

	public bool InitialState { get; set; }

	public CMonsterClass NonEliteVariant => m_NonEliteVariant;

	public List<AbilityData.StatIsBasedOnXData> StatIsBasedOnXEntries => m_CurrentMonsterStat.StatIsBasedOnXEntries;

	public string MonsterClassIDToActImmediatelyBefore => m_MonstersYML.MonsterClassIDToActImmediatelyBefore;

	public MonsterYMLData MonsterYML => m_MonstersYML;

	public List<AbilityData.StatIsBasedOnXData> AttackStatIsBasedOnXEntries => StatIsBasedOnXEntries?.WhereCustom((AbilityData.StatIsBasedOnXData x) => x.BaseStatType == EMonsterBaseStats.Attack || x.BaseStatType == EMonsterBaseStats.Range || x.BaseStatType == EMonsterBaseStats.Target).ToList();

	public List<AbilityData.StatIsBasedOnXData> MoveStatIsBasedOnXEntries => StatIsBasedOnXEntries?.WhereCustom((AbilityData.StatIsBasedOnXData x) => x.BaseStatType == EMonsterBaseStats.Move).ToList();

	public List<AbilityData.StatIsBasedOnXData> ShieldStatIsBasedOnXEntries => StatIsBasedOnXEntries?.WhereCustom((AbilityData.StatIsBasedOnXData x) => x.BaseStatType == EMonsterBaseStats.Shield).ToList();

	public EMonsterType MonsterType => m_MonstersYML.MonsterType;

	public override int Health()
	{
		return GetHealth();
	}

	public override string Avatar()
	{
		return ScenarioRuleClient.SRLYML.MonsterConfigs.SingleOrDefault((MonsterConfigYMLData s) => MonsterYML.CustomConfig == s.ID)?.Avatar ?? base.Avatar();
	}

	public CMonsterClass(MonsterYMLData monster, List<CMonsterAbilityCard> abilityCards)
		: base(monster.ID, monster.Models, monster.LocKey)
	{
		m_MonstersYML = monster;
		ActivatedCards = new List<CBaseCard>();
		for (int i = 0; i < 8; i++)
		{
			SetMonsterStatLevel(i);
			CAction cAction = new CAction(null, null, 0);
			try
			{
				cAction.AddAbility(CAbilityAttack.CreateDefaultAttack(m_CurrentMonsterStat.Attack, m_CurrentMonsterStat.Range, m_CurrentMonsterStat.Target, isMonster: true));
				cAction.AddAbility(CAbilityMove.CreateDefaultMove(m_CurrentMonsterStat.Move, isMonster: true));
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Unable to create base stats ability for monster " + monster.ID + "\n" + ex.Message + "\n" + ex.StackTrace);
			}
			m_BaseActions[i] = cAction;
		}
		SetMonsterStatLevel(1);
		m_AbilityCardsPool = abilityCards;
		m_NonEliteVariant = MonsterClassManager.Find(monster.NonEliteVariant);
		m_Boss = monster.MonsterType == EMonsterType.Boss;
		m_PredominantlyMelee = monster.PredominantlyMelee;
		Reset();
	}

	private int GetHealth()
	{
		return m_CurrentMonsterStat.Health;
	}

	public void SetBaseStatIsBasedOnXValueUsingCurrent(bool onlyRefreshRequiredTypes, List<AbilityData.StatIsBasedOnXData> additionalStatBasedOnXData = null)
	{
		SetBaseStatIsBasedOnXValue(ref m_CurrentMonsterStat, onlyRefreshRequiredTypes, additionalStatBasedOnXData);
	}

	public void SetBaseStatIsBasedOnXValue(ref BaseStats baseStats, bool onlyRefreshRequiredTypes, List<AbilityData.StatIsBasedOnXData> additionalStatBasedOnXData = null)
	{
		if ((StatIsBasedOnXEntries == null && additionalStatBasedOnXData == null) || ScenarioManager.CurrentScenarioState == null)
		{
			return;
		}
		List<AbilityData.StatIsBasedOnXData> list = new List<AbilityData.StatIsBasedOnXData>();
		if (StatIsBasedOnXEntries != null)
		{
			list.AddRange(StatIsBasedOnXEntries);
		}
		if (additionalStatBasedOnXData != null)
		{
			list.AddRange(additionalStatBasedOnXData);
		}
		if (onlyRefreshRequiredTypes)
		{
			list = list.WhereCustom((AbilityData.StatIsBasedOnXData x) => RefreshRequiringStatIsXTypes.Contains(x.BaseStatType)).ToList();
		}
		foreach (AbilityData.StatIsBasedOnXData item in list)
		{
			int baseStatIsBasedOnXValue = GetBaseStatIsBasedOnXValue(item);
			switch (item.BaseStatType)
			{
			case EMonsterBaseStats.Health:
				baseStats.Health = Math.Max(0, (item.AddTo ? m_OriginalMonsterStats.Health : 0) + baseStatIsBasedOnXValue);
				break;
			case EMonsterBaseStats.Shield:
				baseStats.Shield = Math.Max(0, (item.AddTo ? m_OriginalMonsterStats.Shield : 0) + baseStatIsBasedOnXValue);
				break;
			case EMonsterBaseStats.Retaliate:
				baseStats.Retaliate = Math.Max(0, (item.AddTo ? m_OriginalMonsterStats.Retaliate : 0) + baseStatIsBasedOnXValue);
				break;
			case EMonsterBaseStats.RetaliateRange:
				baseStats.RetaliateRange = Math.Max(0, (item.AddTo ? m_OriginalMonsterStats.RetaliateRange : 0) + baseStatIsBasedOnXValue);
				break;
			case EMonsterBaseStats.Pierce:
				baseStats.Pierce = Math.Max(0, (item.AddTo ? m_OriginalMonsterStats.Pierce : 0) + baseStatIsBasedOnXValue);
				break;
			}
			if (item.AddTo)
			{
				item.AddedStat = baseStatIsBasedOnXValue;
			}
		}
	}

	public static int GetBaseStatIsBasedOnXValue(AbilityData.StatIsBasedOnXData statIsBasedOnXData, CAbility.EStatIsBasedOnXType basedOnOverride = CAbility.EStatIsBasedOnXType.None, float xVariableOverride = 1f, float yVariableOverride = 1f)
	{
		int num = 0;
		float num2 = 0f;
		CAbility.EStatIsBasedOnXType eStatIsBasedOnXType = ((basedOnOverride == CAbility.EStatIsBasedOnXType.None && statIsBasedOnXData != null) ? statIsBasedOnXData.BasedOn : basedOnOverride);
		float num3 = ((xVariableOverride == 1f && statIsBasedOnXData != null) ? statIsBasedOnXData.Multiplier : xVariableOverride);
		float num4 = ((yVariableOverride == 1f && statIsBasedOnXData != null) ? statIsBasedOnXData.SecondVariable : yVariableOverride);
		CAbilityFilterContainer cAbilityFilterContainer = statIsBasedOnXData?.Filter;
		switch (eStatIsBasedOnXType)
		{
		case CAbility.EStatIsBasedOnXType.InitialPlayerCharacterCount:
			num2 = (float)Math.Max(ScenarioManager.CurrentScenarioState.Players.Count, 1) * num3;
			break;
		case CAbility.EStatIsBasedOnXType.XAddedToInitialPlayerCharacterCount:
			num2 = (float)Math.Max(ScenarioManager.CurrentScenarioState.Players.Count, 1) + num3;
			break;
		case CAbility.EStatIsBasedOnXType.XAddedToCharactersTimesLevel:
			num2 = num3 + (float)(ScenarioManager.CurrentScenarioState.Players.Count * ScenarioManager.CurrentScenarioState.Level);
			break;
		case CAbility.EStatIsBasedOnXType.CharactersTimesLevelPlusX:
			num2 = (num3 + (float)ScenarioManager.CurrentScenarioState.Level) * (float)ScenarioManager.CurrentScenarioState.Players.Count;
			break;
		case CAbility.EStatIsBasedOnXType.XAddedToYTimesLevel:
			num2 = num3 + (float)(int)Math.Round(num4 * (float)ScenarioManager.CurrentScenarioState.Level);
			break;
		case CAbility.EStatIsBasedOnXType.XPlusCharactersPlusLevel:
			num2 = num3 + (float)ScenarioManager.CurrentScenarioState.Players.Count + (float)ScenarioManager.CurrentScenarioState.Level;
			break;
		case CAbility.EStatIsBasedOnXType.XPlusLevel:
			num2 = num3 + (float)ScenarioManager.CurrentScenarioState.Level;
			break;
		case CAbility.EStatIsBasedOnXType.LevelAddedToXTimesCharacters:
			num2 = (float)ScenarioManager.CurrentScenarioState.Level + num3 * (float)ScenarioManager.CurrentScenarioState.Players.Count;
			break;
		case CAbility.EStatIsBasedOnXType.XAddedToLevelTimesCharactersOverY:
			num2 = num3 + (float)(int)Math.Ceiling((float)(ScenarioManager.CurrentScenarioState.Level * ScenarioManager.CurrentScenarioState.Players.Count) / num4);
			break;
		case CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusY:
			num2 = num3 * (float)ScenarioManager.CurrentScenarioState.Players.Count + (float)ScenarioManager.CurrentScenarioState.Level - num4;
			break;
		case CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountPlusLevelMinusYTimesRound:
			num2 = (num3 * (float)ScenarioManager.CurrentScenarioState.Players.Count + (float)ScenarioManager.CurrentScenarioState.Level - num4) * (float)ScenarioManager.CurrentScenarioState.RoundNumber;
			break;
		case CAbility.EStatIsBasedOnXType.XAddedToLevelThenTimesCharactersOverY:
			num2 = (num3 + (float)ScenarioManager.CurrentScenarioState.Level) * ((float)ScenarioManager.CurrentScenarioState.Players.Count / num4);
			break;
		case CAbility.EStatIsBasedOnXType.LevelTimesXPlusY:
			num2 = (float)ScenarioManager.CurrentScenarioState.Level * num3 + num4;
			break;
		case CAbility.EStatIsBasedOnXType.CharactersPlusLevelOverX:
			num2 = (float)ScenarioManager.CurrentScenarioState.Level / num3 + (float)ScenarioManager.CurrentScenarioState.Players.Count;
			break;
		case CAbility.EStatIsBasedOnXType.XTimesInitialPlayerCharacterCountMinusY:
			num2 = num3 * (float)ScenarioManager.CurrentScenarioState.Players.Count - num4;
			break;
		case CAbility.EStatIsBasedOnXType.XPlusCharactersPlusLevelTimesY:
			num2 = num3 + (float)ScenarioManager.CurrentScenarioState.Players.Count + (float)ScenarioManager.CurrentScenarioState.Level * num4;
			break;
		case CAbility.EStatIsBasedOnXType.ActorCount:
		{
			int num6 = 0;
			foreach (ActorState actorState2 in ScenarioManager.CurrentScenarioState.ActorStates)
			{
				if (actorState2.IsRevealed)
				{
					CActor cActor2 = null;
					cActor2 = ScenarioManager.Scenario.PlayerActors.FirstOrDefault((CPlayerActor a) => a.ActorGuid == actorState2.ActorGuid);
					if (cActor2 == null)
					{
						cActor2 = ScenarioManager.Scenario.HeroSummons.FirstOrDefault((CHeroSummonActor a) => a.ActorGuid == actorState2.ActorGuid);
					}
					if (cActor2 == null)
					{
						cActor2 = ScenarioManager.Scenario.AllAliveMonsters.FirstOrDefault((CEnemyActor a) => a.ActorGuid == actorState2.ActorGuid);
					}
					if (cActor2 == null)
					{
						cActor2 = ScenarioManager.Scenario.Objects.FirstOrDefault((CObjectActor a) => a.ActorGuid == actorState2.ActorGuid);
					}
					if (cActor2 != null && cAbilityFilterContainer != null && cAbilityFilterContainer.IsValidTarget(cActor2, cActor2, isTargetedAbility: false, useTargetOriginalType: false, true))
					{
						num6++;
					}
				}
				else if (cAbilityFilterContainer != null && statIsBasedOnXData.IncludeUnrevealed && (!(actorState2 is EnemyState enemyState2) || enemyState2.GetConfigForPartySize(ScenarioManager.CurrentScenarioState?.Players.Count ?? 2) != ScenarioManager.EPerPartySizeConfig.Hidden) && actorState2 != null && cAbilityFilterContainer.IsValidTarget_ActorState(actorState2, actorState2, isTargetedAbility: false, useTargetOriginalType: false, true))
				{
					num6++;
				}
			}
			if (statIsBasedOnXData.IncludeUnrevealed)
			{
				foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
				{
					if (prop.GetConfigForPartySize(ScenarioManager.CurrentScenarioState?.Players.Count ?? 2) != ScenarioManager.EPerPartySizeConfig.Hidden && prop.PropHealthDetails != null && prop.PropHealthDetails.HasHealth && !prop.PropActorHasBeenAssigned)
					{
						CTile propTile = ScenarioManager.Tiles[prop.ArrayIndex.X, prop.ArrayIndex.Y];
						int propStartingHealth = prop.PropHealthDetails.GetPropStartingHealth();
						ObjectState objectState = prop.PropHealthDetails.CreateStateForPropWithHealth(propTile, propStartingHealth, prop.PropHealthDetails.ActorType);
						if (cAbilityFilterContainer.IsValidTarget_ActorState(objectState, objectState, isTargetedAbility: false, useTargetOriginalType: false, true))
						{
							num6++;
						}
						objectState = null;
					}
				}
			}
			num2 = (float)num6 * num3;
			break;
		}
		case CAbility.EStatIsBasedOnXType.DeadActorCount:
		{
			int num5 = 0;
			foreach (ActorState actorState in ScenarioManager.CurrentScenarioState.ActorStates)
			{
				if (actorState is EnemyState enemyState && enemyState.GetConfigForPartySize(ScenarioManager.CurrentScenarioState?.Players.Count ?? 2) == ScenarioManager.EPerPartySizeConfig.Hidden)
				{
					continue;
				}
				CActor cActor = null;
				cActor = ScenarioManager.Scenario.ExhaustedPlayers.FirstOrDefault((CPlayerActor a) => a.ActorGuid == actorState.ActorGuid);
				if (cActor == null)
				{
					cActor = ScenarioManager.Scenario.DeadHeroSummons.FirstOrDefault((CHeroSummonActor a) => a.ActorGuid == actorState.ActorGuid);
				}
				if (cActor == null)
				{
					cActor = ScenarioManager.Scenario.DeadEnemies.FirstOrDefault((CEnemyActor a) => a.ActorGuid == actorState.ActorGuid);
				}
				if (cActor == null)
				{
					cActor = ScenarioManager.Scenario.DeadAllyMonsters.FirstOrDefault((CEnemyActor a) => a.ActorGuid == actorState.ActorGuid);
				}
				if (cActor == null)
				{
					cActor = ScenarioManager.Scenario.DeadNeutralMonsters.FirstOrDefault((CEnemyActor a) => a.ActorGuid == actorState.ActorGuid);
				}
				if (cActor == null)
				{
					cActor = ScenarioManager.Scenario.Objects.FirstOrDefault((CObjectActor a) => a.ActorGuid == actorState.ActorGuid);
				}
				if (cActor != null && cAbilityFilterContainer != null && cAbilityFilterContainer.IsValidTarget(cActor, cActor, isTargetedAbility: false, useTargetOriginalType: false, true))
				{
					num5++;
				}
			}
			num2 = (float)num5 * num3;
			break;
		}
		case CAbility.EStatIsBasedOnXType.InitialPlayerCharacterCountMinusYTimesX:
			num2 = num3 * ((float)ScenarioManager.CurrentScenarioState.Players.Count - num4);
			break;
		}
		switch (statIsBasedOnXData?.RoundingType ?? CAbility.EStatIsBasedOnXRoundingType.RoundOff)
		{
		case CAbility.EStatIsBasedOnXRoundingType.RoundOff:
			num = (int)Math.Round(num2, MidpointRounding.AwayFromZero);
			break;
		case CAbility.EStatIsBasedOnXRoundingType.ToFloor:
			num = (int)Math.Floor(num2);
			break;
		case CAbility.EStatIsBasedOnXRoundingType.ToCeil:
			num = (int)Math.Ceiling(num2);
			break;
		}
		if (statIsBasedOnXData != null)
		{
			num = Math.Max(statIsBasedOnXData.MinValue, num);
			num = Math.Min(statIsBasedOnXData.MaxValue, num);
		}
		return num;
	}

	private static void SetAddNumberOfPlayersTo(MonsterYMLData yml, ref BaseStats baseStats, int playercount = 0)
	{
		if (yml.AddNumberOfPlayersTo == null || yml.AddNumberOfPlayersTo.Count <= 0 || ScenarioManager.Scenario == null)
		{
			return;
		}
		int num = playercount;
		if (num == 0)
		{
			num = ScenarioManager.Scenario.AllPlayers.Count();
		}
		using List<EMonsterBaseStats>.Enumerator enumerator = yml.AddNumberOfPlayersTo.GetEnumerator();
		while (enumerator.MoveNext())
		{
			switch (enumerator.Current)
			{
			case EMonsterBaseStats.Attack:
				baseStats.Attack += num;
				break;
			case EMonsterBaseStats.Health:
				baseStats.Health += num;
				break;
			case EMonsterBaseStats.Move:
				baseStats.Move += num;
				break;
			case EMonsterBaseStats.Range:
				baseStats.Range += num;
				break;
			case EMonsterBaseStats.Target:
				baseStats.Target += num;
				break;
			default:
				DLLDebug.LogError("Invalid property in AddNumberOfPlayersTo for monster " + yml.FileName);
				break;
			}
		}
	}

	public void SetMonsterStatLevel(int level, int playercount = 0, List<AbilityData.StatIsBasedOnXData> additionalStatBasedOnXData = null)
	{
		m_MonsterStatLevel = level;
		m_CurrentMonsterStat = m_MonstersYML.MonsterBaseStats.Single((BaseStats x) => x.Level == level).Copy();
		m_OriginalMonsterStats = m_CurrentMonsterStat.Copy();
		SetBaseStatIsBasedOnXValue(ref m_CurrentMonsterStat, onlyRefreshRequiredTypes: false, additionalStatBasedOnXData);
	}

	public BaseStats GetMonsterStatsData(int level)
	{
		BaseStats baseStats = m_MonstersYML.MonsterBaseStats.Single((BaseStats x) => x.Level == level).Copy();
		SetBaseStatIsBasedOnXValue(ref baseStats, onlyRefreshRequiredTypes: false);
		return baseStats;
	}

	public override CBaseCard FindCardWithAbility(CAbility ability, CActor actor)
	{
		if (RoundAbilityCard != null)
		{
			if (RoundAbilityCard.Action.Abilities.Any((CAbility x) => x.HasID(ability.ID)))
			{
				return RoundAbilityCard;
			}
			if (RoundAbilityCard.Action.Augmentations != null && RoundAbilityCard.Action.Augmentations.Any((CActionAugmentation x) => x.AugmentationOps.Any((CActionAugmentationOp y) => y.Ability != null && y.Ability.HasID(ability.ID))))
			{
				return RoundAbilityCard;
			}
		}
		foreach (CBaseCard activatedCard in ActivatedCards)
		{
			if (activatedCard.ActiveBonuses.Any((CActiveBonus a) => a.Ability != null && a.Ability.HasID(ability.ID)))
			{
				return activatedCard;
			}
		}
		foreach (CAbilityCard temporaryCard in base.TemporaryCards)
		{
			if (temporaryCard.HasAbility(ability))
			{
				return temporaryCard;
			}
		}
		foreach (CScenarioModifier scenarioModifier in ScenarioManager.CurrentScenarioState.ScenarioModifiers)
		{
			foreach (CAbility item in scenarioModifier.AllListedTriggerAbilities())
			{
				if (item != null && item.HasID(ability.ID))
				{
					return scenarioModifier;
				}
			}
		}
		if (CurrentMonsterStat.OnDeathAbilities != null && CurrentMonsterStat.OnDeathAbilities.Any((CAbility a) => a.HasID(ability.ID)))
		{
			return RoundAbilityCard;
		}
		return null;
	}

	public override CBaseCard FindCard(int id, string name)
	{
		CMonsterAbilityCard roundAbilityCard = RoundAbilityCard;
		if (roundAbilityCard != null && roundAbilityCard.ID == id && roundAbilityCard?.Name == name)
		{
			return roundAbilityCard;
		}
		foreach (CBaseCard activatedCard in ActivatedCards)
		{
			if (activatedCard != null && activatedCard.ID == id && activatedCard?.Name == name)
			{
				return activatedCard;
			}
		}
		return null;
	}

	public void SelectRoundAbilityCard()
	{
		if (m_NonEliteVariant != null)
		{
			m_NonEliteVariant.SelectRoundAbilityCard();
			return;
		}
		if (m_AbilityCards.Count == 0)
		{
			ShuffleAbilityDeck();
		}
		if (m_AbilityCards.Count > 0 && m_RoundAbilityCard == null)
		{
			m_RoundAbilityCard = m_AbilityCards[0];
			m_RoundAbilityCard.ActionHasHappened = false;
			m_AbilityCards.Remove(m_RoundAbilityCard);
			if (m_AbilityCards.Exists((CMonsterAbilityCard item) => item.GetMonsterCardYML.ID == m_RoundAbilityCard.GetMonsterCardYML.ID))
			{
				DLLDebug.LogWarning("Duplicate ability " + m_RoundAbilityCard.GetMonsterCardYML.ID + " in ability deck for monster: " + base.ID);
			}
			m_AbilityCards.RemoveAll((CMonsterAbilityCard item) => item.GetMonsterCardYML.ID == m_RoundAbilityCard.GetMonsterCardYML.ID);
		}
	}

	public void DiscardRoundAbilityCard()
	{
		if (m_NonEliteVariant != null)
		{
			m_NonEliteVariant.DiscardRoundAbilityCard();
		}
		else if (m_RoundAbilityCard != null)
		{
			m_DiscardedAbilityCards.Add(m_RoundAbilityCard);
			if (m_RoundAbilityCard.Shuffle)
			{
				ShuffleAbilityDeck();
			}
			m_RoundAbilityCard = null;
		}
	}

	protected void ShuffleAbilityDeck()
	{
		if (!GameState.ShuffleAbilityDecksEnabledForMonsters)
		{
			return;
		}
		if (m_NonEliteVariant != null)
		{
			m_NonEliteVariant.ShuffleAbilityDeck();
			return;
		}
		bool flag = false;
		while (m_DiscardedAbilityCards.Count > 0)
		{
			flag = true;
			CMonsterAbilityCard monsterAbilityCard = m_DiscardedAbilityCards[0];
			m_DiscardedAbilityCards.Remove(monsterAbilityCard);
			monsterAbilityCard.ActiveBonuses.Clear();
			if (m_AbilityCards.Exists((CMonsterAbilityCard item) => item.GetMonsterCardYML.ID == monsterAbilityCard.GetMonsterCardYML.ID))
			{
				DLLDebug.LogWarning("Duplicate ability " + monsterAbilityCard.GetMonsterCardYML.ID + " in ability deck for monster: " + base.ID);
			}
			m_AbilityCards.Remove(monsterAbilityCard);
			m_AbilityCards.Insert(ScenarioManager.CurrentScenarioState.EnemyAbilityCardRNG.Next(0, m_AbilityCards.Count), monsterAbilityCard);
		}
		if (flag)
		{
			m_AbilityCards.Shuffle();
		}
		SimpleLog.AddToSimpleLog("ENEMY CARD:" + base.ID + " ability deck shuffled:\nAbility Cards:\n" + string.Join("\n", m_AbilityCards.Select((CMonsterAbilityCard s) => s.Name)));
	}

	public int GetNextID(int thisID = int.MaxValue)
	{
		if (m_NonEliteVariant != null)
		{
			return m_NonEliteVariant.GetNextID(thisID);
		}
		int num = -1;
		int num2 = 0;
		if (thisID != int.MaxValue)
		{
			num = m_AvailableIDs.FindIndex((int x) => x == thisID);
			num2 = thisID;
			if (num == -1 && m_AvailableIDs.Count > 0)
			{
				num = ScenarioManager.CurrentScenarioState.EnemyIDRNG.Next(m_AvailableIDs.Count);
				num2 = m_AvailableIDs[num];
			}
		}
		else
		{
			if (m_AvailableIDs.Count <= 0)
			{
				return -1;
			}
			num = ScenarioManager.CurrentScenarioState.EnemyIDRNG.Next(m_AvailableIDs.Count);
			num2 = m_AvailableIDs[num];
		}
		if (num != -1)
		{
			m_AvailableIDs.RemoveAt(num);
		}
		DLLDebug.LogFromSimpleLog("Generating EnemyID: " + num2 + " for Class " + base.ID + ".\nAvailable IDs: + " + string.Join(", ", m_AvailableIDs) + "\nIndex: " + num + "\nNext EnemyIDRNG: " + ScenarioManager.CurrentScenarioState.EnemyIDRNG.Save().Restore().Next());
		return num2;
	}

	public void RemoveAvailableID(int id)
	{
		m_AvailableIDs.Remove(id);
		DLLDebug.LogFromSimpleLog("Removed EnemyID: " + id + " for Class " + base.ID + ".\nAvailable IDs: + " + string.Join(", ", m_AvailableIDs) + "\nNext EnemyIDRNG: " + ScenarioManager.CurrentScenarioState.EnemyIDRNG.Save().Restore().Next());
	}

	public void RecycleID(int id)
	{
		if (m_NonEliteVariant != null)
		{
			m_NonEliteVariant.RecycleID(id);
			return;
		}
		if (!m_AvailableIDs.Contains(id) && id <= m_MonstersYML.StandeeLimit)
		{
			m_AvailableIDs.Add(id);
		}
		DLLDebug.LogFromSimpleLog("Recycled EnemyID: " + id + " for Class " + base.ID + ".\nAvailable IDs: + " + string.Join(", ", m_AvailableIDs) + "\nNext EnemyIDRNG: " + ScenarioManager.CurrentScenarioState.EnemyIDRNG.Save().Restore().Next());
	}

	public List<CActiveBonus> FindAllActiveBonuses()
	{
		List<CActiveBonus> activeBonuses = new List<CActiveBonus>();
		if (RoundAbilityCard != null)
		{
			activeBonuses.AddRange(RoundAbilityCard.ActiveBonuses);
		}
		activeBonuses.AddRange(ActivatedCards.SelectManyWhere((CBaseCard s) => s.ActiveBonuses, (CActiveBonus w) => !activeBonuses.Contains(w)));
		return activeBonuses;
	}

	public List<CActiveBonus> FindActiveBonuses(CActor actor)
	{
		List<CActiveBonus> activeBonuses = new List<CActiveBonus>();
		if (RoundAbilityCard != null)
		{
			activeBonuses.AddRange(RoundAbilityCard.ActiveBonuses.FindAll((CActiveBonus x) => x.Actor?.ActorGuid == actor?.ActorGuid && !x.IsAura));
		}
		activeBonuses.AddRange(ActivatedCards.SelectManyWhere((CBaseCard s) => s.ActiveBonuses, (CActiveBonus w) => w.Actor == actor && !activeBonuses.Contains(w) && !w.IsAura));
		foreach (CActiveBonus item in ActivatedCards.SelectManyWhere((CBaseCard s) => s.ActiveBonuses, (CActiveBonus bonus) => bonus.IsAura))
		{
			if (!activeBonuses.Contains(item) && item.ValidActorsInRangeOfAura.Contains(actor))
			{
				activeBonuses.Add(item);
			}
		}
		return activeBonuses;
	}

	public List<CActiveBonus> FindActiveBonuses(CActiveBonus.EActiveBonusDurationType durationType, CActor actor, bool includeIfCasterOfAura = false)
	{
		List<CActiveBonus> activeBonuses = new List<CActiveBonus>();
		if (RoundAbilityCard != null)
		{
			activeBonuses.AddRange(RoundAbilityCard.ActiveBonuses.FindAll((CActiveBonus x) => x.Duration == durationType && x.Actor == actor && !x.IsAura));
		}
		activeBonuses.AddRange(ActivatedCards.SelectManyWhere((CBaseCard s) => s.ActiveBonuses, (CActiveBonus w) => w.Duration == durationType && w.Actor == actor && !activeBonuses.Contains(w) && !w.IsAura));
		foreach (CActiveBonus item in ActivatedCards.SelectManyWhere((CBaseCard s) => s.ActiveBonuses, (CActiveBonus bonus) => bonus.IsAura))
		{
			if (item.Duration == durationType && !activeBonuses.Contains(item) && (item.ValidActorsInRangeOfAura.Contains(actor) || (includeIfCasterOfAura && item.Caster == actor)))
			{
				activeBonuses.Add(item);
			}
		}
		return activeBonuses;
	}

	public List<CActiveBonus> FindActiveBonuses(CAbility.EAbilityType type, CActor actor)
	{
		List<CActiveBonus> activeBonuses = new List<CActiveBonus>();
		if (RoundAbilityCard != null)
		{
			activeBonuses.AddRange(RoundAbilityCard.ActiveBonuses.FindAll((CActiveBonus x) => x.Type() == type && x.Actor == actor && !x.IsAura));
		}
		activeBonuses.AddRange(ActivatedCards.SelectManyWhere((CBaseCard s) => s.ActiveBonuses, (CActiveBonus w) => w.Type() == type && w.Actor == actor && !activeBonuses.Contains(w) && !w.IsAura));
		foreach (CActiveBonus item in ActivatedCards.SelectManyWhere((CBaseCard s) => s.ActiveBonuses, (CActiveBonus bonus) => bonus.IsAura))
		{
			if (item.Type() == type && !activeBonuses.Contains(item) && item.ValidActorsInRangeOfAura.Contains(actor))
			{
				activeBonuses.Add(item);
			}
		}
		return activeBonuses;
	}

	public List<CActiveBonus> FindActiveBonuses(CActiveBonus.EActiveBonusBehaviourType behaviour, CActor actor)
	{
		List<CActiveBonus> activeBonuses = new List<CActiveBonus>();
		if (RoundAbilityCard != null)
		{
			activeBonuses.AddRange(RoundAbilityCard.ActiveBonuses.FindAll((CActiveBonus x) => x.Ability.ActiveBonusData.Behaviour == behaviour && x.Actor == actor && !x.IsAura));
		}
		activeBonuses.AddRange(ActivatedCards.SelectManyWhere((CBaseCard s) => s.ActiveBonuses, (CActiveBonus w) => w.Ability.ActiveBonusData.Behaviour == behaviour && w.Actor == actor && !activeBonuses.Contains(w) && !w.IsAura));
		foreach (CActiveBonus item in ActivatedCards.SelectManyWhere((CBaseCard s) => s.ActiveBonuses, (CActiveBonus bonus) => bonus.IsAura))
		{
			if (item.Ability.ActiveBonusData.Behaviour == behaviour && !activeBonuses.Contains(item) && item.ValidActorsInRangeOfAura.Contains(actor))
			{
				activeBonuses.Add(item);
			}
		}
		return activeBonuses;
	}

	public List<CActiveBonus> FindActiveBonusAuras(CActor actor)
	{
		try
		{
			List<CActiveBonus> list = new List<CActiveBonus>();
			foreach (CBaseCard activatedCard in ActivatedCards)
			{
				foreach (CActiveBonus activeBonuse in activatedCard.ActiveBonuses)
				{
					if (!activeBonuse.IsAura)
					{
						continue;
					}
					if (activeBonuse.Duration == CActiveBonus.EActiveBonusDurationType.Summon)
					{
						if ((actor as CHeroSummonActor)?.Summoner == activeBonuse.Actor && !list.Contains(activeBonuse))
						{
							list.Add(activeBonuse);
						}
					}
					else if (actor == activeBonuse.Actor && !list.Contains(activeBonuse))
					{
						list.Add(activeBonuse);
					}
				}
			}
			return list;
		}
		catch (InvalidOperationException ex)
		{
			if (ex.Message.Contains("Collection was modified"))
			{
				return FindActiveBonusAuras(actor);
			}
			throw ex;
		}
	}

	public bool HasActiveBonus(CActiveBonus activeBonus)
	{
		if (RoundAbilityCard != null && RoundAbilityCard.ActiveBonuses.Contains(activeBonus))
		{
			return true;
		}
		if (ActivatedCards.Any((CBaseCard a) => a.ActiveBonuses.Contains(activeBonus)))
		{
			return true;
		}
		return false;
	}

	public void ResetAbilityDeck()
	{
		if (m_NonEliteVariant != null)
		{
			m_NonEliteVariant.ResetAbilityDeck();
			return;
		}
		m_AbilityCards = m_AbilityCardsPool.ToList();
		m_DiscardedAbilityCards.Clear();
		m_RoundAbilityCard = null;
	}

	public void Reset()
	{
		InitialState = true;
		foreach (CMonsterAbilityCard item in m_AbilityCardsPool)
		{
			item.Reset();
		}
		AbilityCards.Clear();
		DiscardedAbilityCards.Clear();
		ActivatedCards.Clear();
		base.TemporaryCards.Clear();
		RoundAbilityCard = null;
		AbilityCards = m_AbilityCardsPool.ToList();
		ResetEnemyStandeeIDs();
	}

	public void ResetAbilityCards()
	{
		if (m_NonEliteVariant != null)
		{
			m_NonEliteVariant.ResetAbilityCards();
			return;
		}
		m_AbilityCards.Clear();
		if (ScenarioManager.CurrentScenarioState?.EnemyAbilityCardRNG == null)
		{
			return;
		}
		foreach (CMonsterAbilityCard item in m_AbilityCardsPool)
		{
			m_AbilityCards.Insert(ScenarioManager.CurrentScenarioState.EnemyAbilityCardRNG.Next(0, m_AbilityCards.Count), item);
		}
		InitialState = false;
	}

	public void ResetEnemyStandeeIDs()
	{
		if (m_AvailableIDs == null)
		{
			m_AvailableIDs = new List<int>();
		}
		else
		{
			m_AvailableIDs.Clear();
		}
		for (int i = 1; i <= m_MonstersYML.StandeeLimit; i++)
		{
			m_AvailableIDs.Add(i);
		}
		DLLDebug.LogFromSimpleLog("Reset EnemyIDs for Class " + base.ID + ".\nAvailable IDs: + " + string.Join(", ", m_AvailableIDs));
	}

	public override void AddCurseCard(CActor actor, bool canGoOverLimit = false)
	{
		if (actor is CEnemyActor enemyActor && (MonsterClassManager.GetCurseCardCount(enemyActor) < 10 || canGoOverLimit))
		{
			DLLDebug.LogInfo(base.ID + " has added a curse card to the Monsters attack modifier deck.");
			MonsterClassManager.AddCurseCard(enemyActor);
		}
		else
		{
			DLLDebug.LogInfo("Curse card was not added as there are already 10 curse cards in the attack modifier deck for type: " + actor.OriginalType);
		}
	}

	public override void AddBlessCard(CActor actor, bool canGoOverLimit = false)
	{
		int num = ScenarioManager.Scenario.AllPlayers.Select((CPlayerActor s) => (s.Class as CCharacterClass).AttackModifierCards.WhereCustom((AttackModifierYMLData x) => x.IsBless).Count()).Sum();
		num += MonsterClassManager.MonsterBlessCount;
		if (actor is CEnemyActor enemyActor && (num < 10 || canGoOverLimit))
		{
			DLLDebug.LogInfo(base.ID + " has added a bless card to the Monsters attack modifier deck.");
			MonsterClassManager.AddBlessCard(enemyActor);
		}
		else
		{
			DLLDebug.LogInfo("Bless card was not added as there are already 10 bless cards in the attack modifier deck");
		}
	}

	public void LoadAvailableIDs(List<int> availableIDs)
	{
		if (availableIDs != null)
		{
			m_AvailableIDs = availableIDs.ToList();
			DLLDebug.LogFromSimpleLog("Loaded EnemyIDs for Class " + base.ID + ".\nAvailable IDs: + " + string.Join(", ", m_AvailableIDs) + "\nNext EnemyIDRNG: " + ScenarioManager.CurrentScenarioState.EnemyIDRNG.Save().Restore().Next());
		}
	}

	public void LoadAbilityDeck(AbilityDeckState deck)
	{
		InitialState = deck.InitialState;
		RoundAbilityCard = null;
		if (deck.HandAbilityCardIDsAndInstanceIDs != null)
		{
			try
			{
				AbilityCards = deck.HandAbilityCardIDsAndInstanceIDs.Select((Tuple<int, int> s) => AbilityCardsPool.Single((CMonsterAbilityCard c) => c.ID == s.Item1)).ToList();
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Failed to load in monster ability cards from save data for MonsterID: " + base.ID + "\n" + ex.Message + "\n" + ex.StackTrace);
				ResetAbilityCards();
			}
		}
		else
		{
			ResetAbilityCards();
		}
		if (deck.RoundAbilityCardIDsAndInstanceID != null && deck.RoundAbilityCardIDsAndInstanceID.Count > 0)
		{
			try
			{
				RoundAbilityCard = AbilityCardsPool.Single((CMonsterAbilityCard s) => s.ID == deck.RoundAbilityCardIDsAndInstanceID[0].Item1);
			}
			catch
			{
				throw new Exception("Failed to load in round cards from save data");
			}
		}
		if (deck.DiscardedAbilityCardIDs != null)
		{
			try
			{
				DiscardedAbilityCards = deck.DiscardedAbilityCardIDs.Select((int s) => AbilityCardsPool.Single((CMonsterAbilityCard c) => c.ID == s)).ToList();
			}
			catch (Exception ex2)
			{
				DLLDebug.LogError("Failed to load in discarded cards from save data\n" + ex2.Message + "\n" + ex2.StackTrace);
				DiscardedAbilityCards = new List<CMonsterAbilityCard>();
			}
		}
		if (deck.ActivatedAbilityCardIDs != null)
		{
			try
			{
				List<int> cardIDs = (from s in deck.ActivatedAbilityCardIDs.WhereCustom((Tuple<int, CBaseCard.ECardType> w2) => w2.Item2 == CBaseCard.ECardType.MonsterAbility)
					select s.Item1).Distinct().ToList();
				ActivatedCards = ((IEnumerable<CMonsterAbilityCard>)AbilityCardsPool.WhereCustom((CMonsterAbilityCard w) => cardIDs.Contains(w.ID))).Select((Func<CMonsterAbilityCard, CBaseCard>)((CMonsterAbilityCard s) => s)).ToList();
				if (ActivatedCards.Count != cardIDs.Count)
				{
					foreach (int id in cardIDs.WhereCustom((int w) => !ActivatedCards.Select((CBaseCard s) => s.ID).Contains(w)))
					{
						ActivatedCards.Add(MonsterClassManager.Classes.SelectManyCustom((CMonsterClass sm) => sm.AbilityCardsPool).First((CMonsterAbilityCard f) => f.ID == id));
					}
				}
				ActivatedCards.AddRange((from s in ScenarioRuleClient.SRLYML.ItemCards
					select s.GetItem into w
					where (from s in deck.ActivatedAbilityCardIDs.WhereCustom((Tuple<int, CBaseCard.ECardType> tuple) => tuple.Item2 == CBaseCard.ECardType.Item)
						select s.Item1).ToList().Contains(w.ID)
					select w).Select((Func<CItem, CBaseCard>)((CItem s) => s)).ToList());
				ActivatedCards.AddRange(((IEnumerable<AttackModifierYMLData>)ScenarioRuleClient.SRLYML.AttackModifiers.WhereCustom((AttackModifierYMLData w) => (from s in deck.ActivatedAbilityCardIDs.WhereCustom((Tuple<int, CBaseCard.ECardType> tuple) => tuple.Item2 == CBaseCard.ECardType.AttackModifier)
					select s.Item1).ToList().Contains(w.ID))).Select((Func<AttackModifierYMLData, CBaseCard>)((AttackModifierYMLData s) => s.Card)).ToList());
				if (ActivatedCards.Count < cardIDs.Count)
				{
					throw new Exception("Failed to load all Activated ability cards for enemy");
				}
				foreach (CBaseCard activatedCard in ActivatedCards)
				{
					activatedCard.ActionHasHappened = true;
				}
			}
			catch (Exception ex3)
			{
				DLLDebug.LogError("Failed to load in activated cards from save data\n" + ex3.Message + "\n" + ex3.StackTrace);
			}
		}
		if (deck.ActiveBonuses == null || deck.ActiveBonuses.Count <= 0 || RoundAbilityCard == null)
		{
			return;
		}
		foreach (ActiveBonusState activeBonusState in deck.ActiveBonuses)
		{
			CActor cActor = ScenarioManager.FindActor(activeBonusState.ActorGuid);
			CActor caster = ScenarioManager.FindActor(activeBonusState.CasterGuid);
			if (cActor == null)
			{
				DLLDebug.LogError("Unable to find actor attached to active bonus");
				continue;
			}
			int? num = null;
			if (activeBonusState.BespokeBehaviourStrength != 0)
			{
				num = activeBonusState.BespokeBehaviourStrength;
			}
			if (RoundAbilityCard.ID == activeBonusState.CardID && (!activeBonusState.NameIsValid() || activeBonusState.CardName == RoundAbilityCard.Name))
			{
				CAbility cAbility = RoundAbilityCard.FindAbilityOnCard(activeBonusState.AbilityName);
				if (cAbility == null)
				{
					DLLDebug.LogError("Unable to find ability " + activeBonusState.AbilityName + " for active bonus state");
					continue;
				}
				CMonsterAbilityCard roundAbilityCard = RoundAbilityCard;
				CAbility ability = CAbility.CopyAbility(cAbility, generateNewID: false);
				int? iD = activeBonusState.ID;
				int? bespokeStrength = num;
				roundAbilityCard.AddActiveBonus(ability, cActor, caster, iD, null, isAugment: false, isSong: false, loadingItemBonus: false, isDoom: false, bespokeStrength);
				continue;
			}
			CItem getItem = ScenarioRuleClient.SRLYML.ItemCards.SingleOrDefault((ItemCardYMLData s2) => s2.ID == activeBonusState.CardID && activeBonusState.CardName == s2.Name).GetItem;
			if (getItem != null)
			{
				CAbility cAbility2 = getItem.YMLData.Data.Abilities.SingleOrDefault((CAbility s) => s.Name == activeBonusState.AbilityName);
				if (cAbility2 != null)
				{
					CAbility ability2 = CAbility.CopyAbility(cAbility2, generateNewID: false);
					int? iD2 = activeBonusState.ID;
					int? bespokeStrength = num;
					getItem.AddActiveBonus(ability2, cActor, caster, iD2, null, isAugment: false, isSong: false, loadingItemBonus: false, isDoom: false, bespokeStrength);
				}
				else
				{
					DLLDebug.LogError("Unable to find ability " + activeBonusState.AbilityName + " for active bonus state");
				}
				continue;
			}
			AttackModifierYMLData attackModifierYMLData = ScenarioRuleClient.SRLYML.AttackModifiers.SingleOrDefault((AttackModifierYMLData s) => s.Name == activeBonusState.CardName);
			if (attackModifierYMLData == null)
			{
				continue;
			}
			foreach (CAbility item in attackModifierYMLData.Abilities.WhereCustom((CAbility w) => w.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA))
			{
				AttackModifierCard card = attackModifierYMLData.Card;
				int? iD3 = activeBonusState.ID;
				int? bespokeStrength = num;
				card.AddActiveBonus(item, cActor, caster, iD3, null, isAugment: false, isSong: false, loadingItemBonus: false, isDoom: false, bespokeStrength);
			}
		}
	}

	public void ClearRoundAbilityCardForEditor()
	{
		if (RoundAbilityCard != null)
		{
			if (m_AbilityCards.Exists((CMonsterAbilityCard item) => item.GetMonsterCardYML.ID == RoundAbilityCard.GetMonsterCardYML.ID))
			{
				DLLDebug.LogWarning("Duplicate ability " + RoundAbilityCard.GetMonsterCardYML.ID + " in ability deck for monster: " + base.ID);
			}
			AbilityCards.Remove(RoundAbilityCard);
			AbilityCards.Insert(0, RoundAbilityCard);
			RoundAbilityCard = null;
		}
	}
}
