#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using SM.Utils;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;

namespace ScenarioRuleLibrary;

public class MonsterClassManager
{
	private static List<CMonsterClass> s_Classes = new List<CMonsterClass>();

	private static List<CObjectClass> s_ObjectClasses = new List<CObjectClass>();

	private static CMonsterAttackModifierDeck s_EnemyMonsterAttackModifierDeck = new CMonsterAttackModifierDeck();

	private static CMonsterAttackModifierDeck s_AlliedMonsterAttackModifierDeck = new CMonsterAttackModifierDeck();

	private static CMonsterAttackModifierDeck s_Enemy2MonsterAttackModifierDeck = new CMonsterAttackModifierDeck();

	private static CMonsterAttackModifierDeck s_NeutralMonsterAttackModifierDeck = new CMonsterAttackModifierDeck();

	private static CMonsterAttackModifierDeck s_BossMonsterAttackModifierDeck = new CMonsterAttackModifierDeck();

	public static List<CMonsterClass> Classes => s_Classes;

	public static List<CObjectClass> ObjectClasses => s_ObjectClasses;

	public static List<CMonsterClass> MonsterAndObjectClasses => s_Classes.Concat(s_ObjectClasses).ToList();

	public static CMonsterAttackModifierDeck EnemyMonsterAttackModifierDeck => s_EnemyMonsterAttackModifierDeck;

	public static CMonsterAttackModifierDeck AlliedMonsterAttackModifierDeck => s_AlliedMonsterAttackModifierDeck;

	public static CMonsterAttackModifierDeck Enemy2MonsterAttackModifierDeck => s_Enemy2MonsterAttackModifierDeck;

	public static CMonsterAttackModifierDeck NeutralMonsterAttackModifierDeck => s_NeutralMonsterAttackModifierDeck;

	public static CMonsterAttackModifierDeck BossMonsterAttackModifierDeck => s_BossMonsterAttackModifierDeck;

	public static int MonsterBlessCount => EnemyMonsterAttackModifierDeck.AttackModifierCards.Count((AttackModifierYMLData x) => x.IsBless) + AlliedMonsterAttackModifierDeck.AttackModifierCards.Count((AttackModifierYMLData x) => x.IsBless) + Enemy2MonsterAttackModifierDeck.AttackModifierCards.Count((AttackModifierYMLData x) => x.IsBless) + NeutralMonsterAttackModifierDeck.AttackModifierCards.Count((AttackModifierYMLData x) => x.IsBless) + BossMonsterAttackModifierDeck.AttackModifierCards.Count((AttackModifierYMLData x) => x.IsBless);

	public static void Load()
	{
		s_Classes.Clear();
		s_ObjectClasses.Clear();
		EnemyMonsterAttackModifierDeck.LoadCardPool();
		AlliedMonsterAttackModifierDeck.LoadCardPool();
		Enemy2MonsterAttackModifierDeck.LoadCardPool();
		NeutralMonsterAttackModifierDeck.LoadCardPool();
		BossMonsterAttackModifierDeck.LoadCardPool();
		foreach (MonsterYMLData monster in ScenarioRuleClient.SRLYML.Monsters)
		{
			List<CMonsterAbilityCard> list = new List<CMonsterAbilityCard>();
			if (monster.AbilityCards != null)
			{
				foreach (MonsterCardYMLData abilityCard in monster.AbilityCards)
				{
					if (abilityCard != null && abilityCard.CardAction != null)
					{
						list.Add(new CMonsterAbilityCard(abilityCard.ID, abilityCard.Initiative, abilityCard.CardAction, abilityCard.Shuffle, monster.ID, abilityCard));
					}
					else
					{
						DLLDebug.LogError("Invalid monster card in file " + abilityCard.FileName);
					}
				}
			}
			if (monster.MonsterType == EMonsterType.Monster || monster.MonsterType == EMonsterType.Boss)
			{
				s_Classes.Add(new CMonsterClass(monster, list));
			}
			else if (monster.MonsterType == EMonsterType.Object)
			{
				s_ObjectClasses.Add(new CObjectClass(monster, list));
			}
			else
			{
				DLLDebug.LogError("Invalid Monster Type! " + monster.FileName);
			}
		}
	}

	private static MonsterYMLData GetMonsterYml(string id)
	{
		return ScenarioRuleClient.SRLYML.GetMonsterData(id);
	}

	private static CMonsterClass CreateMonsterClass(MonsterYMLData ymlData)
	{
		if (ymlData == null)
		{
			return null;
		}
		CMonsterClass cMonsterClass = null;
		List<CMonsterAbilityCard> list = new List<CMonsterAbilityCard>();
		if (ymlData.AbilityCards != null)
		{
			foreach (MonsterCardYMLData abilityCard in ymlData.AbilityCards)
			{
				if (abilityCard != null && abilityCard.CardAction != null)
				{
					list.Add(new CMonsterAbilityCard(abilityCard.ID, abilityCard.Initiative, abilityCard.CardAction, abilityCard.Shuffle, ymlData.ID, abilityCard));
				}
				else
				{
					DLLDebug.LogError("Invalid monster card in file " + abilityCard.FileName);
				}
			}
		}
		if (ymlData.MonsterType == EMonsterType.Monster || ymlData.MonsterType == EMonsterType.Boss)
		{
			cMonsterClass = new CMonsterClass(ymlData, list);
			s_Classes.Add(cMonsterClass);
			if (string.IsNullOrEmpty(ymlData.NonEliteVariant))
			{
				FindEliteVariantOfClass(cMonsterClass.ID);
			}
		}
		else if (ymlData.MonsterType == EMonsterType.Object)
		{
			CObjectClass cObjectClass = new CObjectClass(ymlData, list);
			cMonsterClass = cObjectClass;
			s_ObjectClasses.Add(cObjectClass);
		}
		else
		{
			DLLDebug.LogError("Invalid Monster Type! " + ymlData.FileName);
		}
		return cMonsterClass;
	}

	public static void ValidateMonsterClassManager()
	{
	}

	public static void SelectRoundAbilityCards()
	{
		foreach (CEnemyActor allAliveMonster in ScenarioManager.Scenario.AllAliveMonsters)
		{
			CMonsterClass monsterClass = allAliveMonster.MonsterClass;
			((monsterClass.NonEliteVariant == null) ? monsterClass : monsterClass.NonEliteVariant).SelectRoundAbilityCard();
		}
		foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
		{
			if (@object.MonsterClass is CObjectClass cObjectClass)
			{
				cObjectClass.SelectRoundAbilityCard();
			}
		}
	}

	public static void DiscardRoundAbilityCards()
	{
		foreach (CEnemyActor allMonster in ScenarioManager.Scenario.AllMonsters)
		{
			CMonsterClass monsterClass = allMonster.MonsterClass;
			((monsterClass.NonEliteVariant == null) ? monsterClass : monsterClass.NonEliteVariant).DiscardRoundAbilityCard();
		}
		foreach (CObjectActor allObject in ScenarioManager.Scenario.AllObjects)
		{
			CMonsterClass monsterClass2 = allObject.MonsterClass;
			((monsterClass2.NonEliteVariant == null) ? monsterClass2 : monsterClass2.NonEliteVariant).DiscardRoundAbilityCard();
		}
	}

	public static List<AttackModifierYMLData> DrawAttackModifierCards(CActor actor, int attackStrength, EAdvantageStatuses advStatus, out List<AttackModifierYMLData> notUsed)
	{
		if (actor is CEnemyActor cEnemyActor)
		{
			if (cEnemyActor.MonsterClass.Boss)
			{
				return BossMonsterAttackModifierDeck.DrawAttackModifierCards(actor, attackStrength, advStatus, out notUsed);
			}
			switch (cEnemyActor.OriginalType)
			{
			case CActor.EType.Ally:
				return AlliedMonsterAttackModifierDeck.DrawAttackModifierCards(actor, attackStrength, advStatus, out notUsed);
			case CActor.EType.Neutral:
				return NeutralMonsterAttackModifierDeck.DrawAttackModifierCards(actor, attackStrength, advStatus, out notUsed);
			case CActor.EType.Enemy:
				return EnemyMonsterAttackModifierDeck.DrawAttackModifierCards(actor, attackStrength, advStatus, out notUsed);
			case CActor.EType.Enemy2:
				return Enemy2MonsterAttackModifierDeck.DrawAttackModifierCards(actor, attackStrength, advStatus, out notUsed);
			}
		}
		notUsed = new List<AttackModifierYMLData>();
		return null;
	}

	public static void CheckAllAttackModifierDecksForShuffle()
	{
		EnemyMonsterAttackModifierDeck.CheckAttackModifierCardShuffle();
		AlliedMonsterAttackModifierDeck.CheckAttackModifierCardShuffle();
		Enemy2MonsterAttackModifierDeck.CheckAttackModifierCardShuffle();
		NeutralMonsterAttackModifierDeck.CheckAttackModifierCardShuffle();
		BossMonsterAttackModifierDeck.CheckAttackModifierCardShuffle();
	}

	public static bool IsDeckEmpty(CEnemyActor checkActor)
	{
		if (checkActor.MonsterClass.Boss)
		{
			return BossMonsterAttackModifierDeck.AttackModifierCards.Count <= 0;
		}
		return checkActor.OriginalType switch
		{
			CActor.EType.Ally => AlliedMonsterAttackModifierDeck.AttackModifierCards.Count <= 0, 
			CActor.EType.Neutral => NeutralMonsterAttackModifierDeck.AttackModifierCards.Count <= 0, 
			CActor.EType.Enemy => EnemyMonsterAttackModifierDeck.AttackModifierCards.Count <= 0, 
			CActor.EType.Enemy2 => Enemy2MonsterAttackModifierDeck.AttackModifierCards.Count <= 0, 
			_ => false, 
		};
	}

	public static void CheckAttackModifierCardShuffle(CEnemyActor forceShuffleForActor)
	{
		if (!GameState.ShuffleAttackModsEnabledForMonsters)
		{
			return;
		}
		if (forceShuffleForActor.MonsterClass.Boss)
		{
			BossMonsterAttackModifierDeck.CheckAttackModifierCardShuffle(force: true);
			return;
		}
		switch (forceShuffleForActor.OriginalType)
		{
		case CActor.EType.Ally:
			AlliedMonsterAttackModifierDeck.CheckAttackModifierCardShuffle(force: true);
			break;
		case CActor.EType.Neutral:
			NeutralMonsterAttackModifierDeck.CheckAttackModifierCardShuffle(force: true);
			break;
		case CActor.EType.Enemy:
			EnemyMonsterAttackModifierDeck.CheckAttackModifierCardShuffle(force: true);
			break;
		case CActor.EType.Enemy2:
			Enemy2MonsterAttackModifierDeck.CheckAttackModifierCardShuffle(force: true);
			break;
		case CActor.EType.HeroSummon:
			break;
		}
	}

	public static CMonsterClass Find(string classID)
	{
		if (classID == null || classID == "Empty")
		{
			return null;
		}
		CMonsterClass cMonsterClass = MonsterAndObjectClasses.FirstOrDefault((CMonsterClass x) => x.ID == classID);
		if (cMonsterClass == null)
		{
			cMonsterClass = CreateMonsterClass(GetMonsterYml(classID));
		}
		if (cMonsterClass == null)
		{
			LogUtils.LogWarning("Created monster is null " + classID);
		}
		return cMonsterClass;
	}

	public static CMonsterClass Find(string classID, Predicate<CMonsterClass> predicate)
	{
		if (classID == null || classID == "Empty")
		{
			return null;
		}
		CMonsterClass cMonsterClass = MonsterAndObjectClasses.FirstOrDefault((CMonsterClass x) => x.ID == classID);
		if (cMonsterClass == null)
		{
			cMonsterClass = CreateMonsterClass(GetMonsterYml(classID));
		}
		if (cMonsterClass == null)
		{
			LogUtils.LogError("Created monster is null " + classID);
		}
		else if (!predicate(cMonsterClass))
		{
			return null;
		}
		return cMonsterClass;
	}

	public static CObjectClass FindObjectClass(string classID)
	{
		if (classID == null || classID == "Empty")
		{
			return null;
		}
		CObjectClass cObjectClass = ObjectClasses.FirstOrDefault((CObjectClass x) => x.ID == classID);
		if (cObjectClass == null)
		{
			cObjectClass = CreateMonsterClass(GetMonsterYml(classID)) as CObjectClass;
		}
		if (cObjectClass == null)
		{
			LogUtils.LogError("Created monster is null " + classID);
		}
		return cObjectClass;
	}

	public static CMonsterClass FindEliteVariantOfClass(string id)
	{
		CMonsterClass cMonsterClass = Classes.FirstOrDefault((CMonsterClass c) => c.NonEliteVariant != null && c.NonEliteVariant.ID == id);
		if (cMonsterClass == null)
		{
			cMonsterClass = Find(id.Substring(0, id.Length - 2) + "EliteID");
			if (cMonsterClass == null)
			{
				LogUtils.Log("Checking for Elite variant for " + id + ". Result: not exist");
			}
		}
		return cMonsterClass;
	}

	public static List<CActiveBonus> FindAllActiveBonuses()
	{
		List<CActiveBonus> list = new List<CActiveBonus>();
		foreach (CEnemyActor allEnemy in ScenarioManager.Scenario.AllEnemies)
		{
			list.AddRange(allEnemy.MonsterClass.FindActiveBonuses(allEnemy));
		}
		return list;
	}

	public static List<CActiveBonus> FindAllActiveBonuses(CActor actor)
	{
		List<CActiveBonus> list = new List<CActiveBonus>();
		if (actor is CEnemyActor cEnemyActor)
		{
			list.AddRange(cEnemyActor.MonsterClass.FindActiveBonuses(actor));
		}
		return list;
	}

	public static List<CActiveBonus> FindAllActiveBonuses(CAbility.EAbilityType type, CActor actor)
	{
		List<CActiveBonus> list = new List<CActiveBonus>();
		foreach (CMonsterClass monsterAndObjectClass in MonsterAndObjectClasses)
		{
			if (monsterAndObjectClass.NonEliteVariant == null && monsterAndObjectClass.FindActiveBonuses(type, actor) != null)
			{
				list.AddRange(monsterAndObjectClass.FindActiveBonuses(type, actor));
			}
		}
		return list;
	}

	public static List<CActiveBonus> FindAllActiveBonuses(CActiveBonus.EActiveBonusDurationType durationType, CActor actor)
	{
		List<CActiveBonus> list = new List<CActiveBonus>();
		if (actor is CEnemyActor cEnemyActor)
		{
			list.AddRange(cEnemyActor.MonsterClass.FindActiveBonuses(durationType, actor, includeIfCasterOfAura: true));
		}
		return list;
	}

	public static void SetAllMonsterStatLevels(int level, List<CStatBasedOnXOverrideDetails> statBasedOnXOverrides = null)
	{
		foreach (CMonsterClass monsterClass in MonsterAndObjectClasses)
		{
			List<AbilityData.StatIsBasedOnXData> additionalStatBasedOnXData = null;
			if (statBasedOnXOverrides != null && statBasedOnXOverrides.Any((CStatBasedOnXOverrideDetails s) => s.AssociatedClassID == monsterClass.ID))
			{
				additionalStatBasedOnXData = (from o in statBasedOnXOverrides
					where o.AssociatedClassID == monsterClass.ID
					select o.OverrideData).ToList();
			}
			monsterClass.SetMonsterStatLevel(level, 0, additionalStatBasedOnXData);
		}
	}

	public static CClass FindActiveBonusClass(CActiveBonus activeBonus)
	{
		new List<CActiveBonus>();
		foreach (CMonsterClass monsterAndObjectClass in MonsterAndObjectClasses)
		{
			if (monsterAndObjectClass.NonEliteVariant == null && monsterAndObjectClass.HasActiveBonus(activeBonus))
			{
				return monsterAndObjectClass;
			}
		}
		return null;
	}

	public static void Reset()
	{
		foreach (CMonsterClass monsterAndObjectClass in MonsterAndObjectClasses)
		{
			monsterAndObjectClass.Reset();
		}
		if (ScenarioManager.CurrentScenarioState != null)
		{
			ResetAttackModifiers();
		}
	}

	public static void ResetAttackModifiers()
	{
		EnemyMonsterAttackModifierDeck.ResetAttackModifiers(ScenarioManager.CurrentScenarioState.EnemyClassManager?.EnemyAttackModifierDeck);
		AlliedMonsterAttackModifierDeck.ResetAttackModifiers(ScenarioManager.CurrentScenarioState.EnemyClassManager?.AlliedMonsterAttackModifierDeck);
		Enemy2MonsterAttackModifierDeck.ResetAttackModifiers(ScenarioManager.CurrentScenarioState.EnemyClassManager?.Enemy2MonsterAttackModifierDeck);
		NeutralMonsterAttackModifierDeck.ResetAttackModifiers(ScenarioManager.CurrentScenarioState.EnemyClassManager?.NeutralMonsterAttackModifierDeck);
		BossMonsterAttackModifierDeck.ResetAttackModifiers(ScenarioManager.CurrentScenarioState.EnemyClassManager?.BossAttackModifierDeck);
	}

	public static void RandomizeAllMonsterAvailableAttackModifierCardsOrder()
	{
		EnemyMonsterAttackModifierDeck.RandomizeAvailableAttackModifierCardsOrder();
		AlliedMonsterAttackModifierDeck.RandomizeAvailableAttackModifierCardsOrder();
		Enemy2MonsterAttackModifierDeck.RandomizeAvailableAttackModifierCardsOrder();
		NeutralMonsterAttackModifierDeck.RandomizeAvailableAttackModifierCardsOrder();
		BossMonsterAttackModifierDeck.RandomizeAvailableAttackModifierCardsOrder();
	}

	public static void AddAdditionalModifierCards(CEnemyActor enemyActor, List<string> cardNames)
	{
		if (enemyActor.MonsterClass.Boss)
		{
			BossMonsterAttackModifierDeck.AddAdditionalModifierCards(cardNames);
			return;
		}
		switch (enemyActor.OriginalType)
		{
		case CActor.EType.Ally:
			AlliedMonsterAttackModifierDeck.AddAdditionalModifierCards(cardNames);
			break;
		case CActor.EType.Neutral:
			NeutralMonsterAttackModifierDeck.AddAdditionalModifierCards(cardNames);
			break;
		case CActor.EType.Enemy:
			EnemyMonsterAttackModifierDeck.AddAdditionalModifierCards(cardNames);
			break;
		case CActor.EType.Enemy2:
			Enemy2MonsterAttackModifierDeck.AddAdditionalModifierCards(cardNames);
			break;
		case CActor.EType.HeroSummon:
			break;
		}
	}

	public static string DebugSetAttackMod(CEnemyActor enemyActor)
	{
		if (enemyActor.MonsterClass.Boss)
		{
			return BossMonsterAttackModifierDeck.DebugSetAttackMod();
		}
		return enemyActor.OriginalType switch
		{
			CActor.EType.Ally => AlliedMonsterAttackModifierDeck.DebugSetAttackMod(), 
			CActor.EType.Neutral => NeutralMonsterAttackModifierDeck.DebugSetAttackMod(), 
			CActor.EType.Enemy => EnemyMonsterAttackModifierDeck.DebugSetAttackMod(), 
			CActor.EType.Enemy2 => Enemy2MonsterAttackModifierDeck.DebugSetAttackMod(), 
			_ => "", 
		};
	}

	public static int GetCurseCardCount(CEnemyActor enemyActor)
	{
		if (enemyActor.MonsterClass.Boss)
		{
			return BossMonsterAttackModifierDeck.AttackModifierCards.Where((AttackModifierYMLData x) => x.IsCurse).Count();
		}
		return enemyActor.OriginalType switch
		{
			CActor.EType.Ally => AlliedMonsterAttackModifierDeck.AttackModifierCards.Where((AttackModifierYMLData x) => x.IsCurse).Count(), 
			CActor.EType.Neutral => NeutralMonsterAttackModifierDeck.AttackModifierCards.Where((AttackModifierYMLData x) => x.IsCurse).Count(), 
			CActor.EType.Enemy => EnemyMonsterAttackModifierDeck.AttackModifierCards.Where((AttackModifierYMLData x) => x.IsCurse).Count(), 
			CActor.EType.Enemy2 => Enemy2MonsterAttackModifierDeck.AttackModifierCards.Where((AttackModifierYMLData x) => x.IsCurse).Count(), 
			_ => 0, 
		};
	}

	public static void AddCurseCard(CEnemyActor enemyActor)
	{
		if (enemyActor.MonsterClass.Boss)
		{
			BossMonsterAttackModifierDeck.AttackModifierCards.Insert(ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(0, BossMonsterAttackModifierDeck.AttackModifierCards.Count), AttackModifiersYML.CreateCurse());
			return;
		}
		switch (enemyActor.OriginalType)
		{
		case CActor.EType.Ally:
			AlliedMonsterAttackModifierDeck.AttackModifierCards.Insert(ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(0, AlliedMonsterAttackModifierDeck.AttackModifierCards.Count), AttackModifiersYML.CreateCurse());
			break;
		case CActor.EType.Neutral:
			NeutralMonsterAttackModifierDeck.AttackModifierCards.Insert(ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(0, NeutralMonsterAttackModifierDeck.AttackModifierCards.Count), AttackModifiersYML.CreateCurse());
			break;
		case CActor.EType.Enemy:
			EnemyMonsterAttackModifierDeck.AttackModifierCards.Insert(ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(0, EnemyMonsterAttackModifierDeck.AttackModifierCards.Count), AttackModifiersYML.CreateCurse());
			break;
		case CActor.EType.Enemy2:
			Enemy2MonsterAttackModifierDeck.AttackModifierCards.Insert(ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(0, Enemy2MonsterAttackModifierDeck.AttackModifierCards.Count), AttackModifiersYML.CreateCurse());
			break;
		case CActor.EType.HeroSummon:
			break;
		}
	}

	public static void AddBlessCard(CEnemyActor enemyActor)
	{
		if (enemyActor.MonsterClass.Boss)
		{
			BossMonsterAttackModifierDeck.AttackModifierCards.Insert(ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(0, BossMonsterAttackModifierDeck.AttackModifierCards.Count), AttackModifiersYML.CreateBless());
			return;
		}
		switch (enemyActor.OriginalType)
		{
		case CActor.EType.Ally:
			AlliedMonsterAttackModifierDeck.AttackModifierCards.Insert(ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(0, AlliedMonsterAttackModifierDeck.AttackModifierCards.Count), AttackModifiersYML.CreateBless());
			break;
		case CActor.EType.Neutral:
			NeutralMonsterAttackModifierDeck.AttackModifierCards.Insert(ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(0, NeutralMonsterAttackModifierDeck.AttackModifierCards.Count), AttackModifiersYML.CreateBless());
			break;
		case CActor.EType.Enemy:
			EnemyMonsterAttackModifierDeck.AttackModifierCards.Insert(ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(0, EnemyMonsterAttackModifierDeck.AttackModifierCards.Count), AttackModifiersYML.CreateBless());
			break;
		case CActor.EType.Enemy2:
			Enemy2MonsterAttackModifierDeck.AttackModifierCards.Insert(ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(0, Enemy2MonsterAttackModifierDeck.AttackModifierCards.Count), AttackModifiersYML.CreateBless());
			break;
		case CActor.EType.HeroSummon:
			break;
		}
	}
}
