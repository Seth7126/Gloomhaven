using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class EnemyClassManagerState : ISerializable
{
	public AttackModifierDeckState EnemyAttackModifierDeck { get; private set; }

	public AttackModifierDeckState AlliedMonsterAttackModifierDeck { get; private set; }

	public AttackModifierDeckState Enemy2MonsterAttackModifierDeck { get; private set; }

	public AttackModifierDeckState NeutralMonsterAttackModifierDeck { get; private set; }

	public AttackModifierDeckState BossAttackModifierDeck { get; private set; }

	public List<EnemyClassState> EnemyClasses { get; private set; }

	public EnemyClassManagerState(EnemyClassManagerState state, ReferenceDictionary references)
	{
		EnemyAttackModifierDeck = references.Get(state.EnemyAttackModifierDeck);
		if (EnemyAttackModifierDeck == null && state.EnemyAttackModifierDeck != null)
		{
			EnemyAttackModifierDeck = new AttackModifierDeckState(state.EnemyAttackModifierDeck, references);
			references.Add(state.EnemyAttackModifierDeck, EnemyAttackModifierDeck);
		}
		AlliedMonsterAttackModifierDeck = references.Get(state.AlliedMonsterAttackModifierDeck);
		if (AlliedMonsterAttackModifierDeck == null && state.AlliedMonsterAttackModifierDeck != null)
		{
			AlliedMonsterAttackModifierDeck = new AttackModifierDeckState(state.AlliedMonsterAttackModifierDeck, references);
			references.Add(state.AlliedMonsterAttackModifierDeck, AlliedMonsterAttackModifierDeck);
		}
		Enemy2MonsterAttackModifierDeck = references.Get(state.Enemy2MonsterAttackModifierDeck);
		if (Enemy2MonsterAttackModifierDeck == null && state.Enemy2MonsterAttackModifierDeck != null)
		{
			Enemy2MonsterAttackModifierDeck = new AttackModifierDeckState(state.Enemy2MonsterAttackModifierDeck, references);
			references.Add(state.Enemy2MonsterAttackModifierDeck, Enemy2MonsterAttackModifierDeck);
		}
		NeutralMonsterAttackModifierDeck = references.Get(state.NeutralMonsterAttackModifierDeck);
		if (NeutralMonsterAttackModifierDeck == null && state.NeutralMonsterAttackModifierDeck != null)
		{
			NeutralMonsterAttackModifierDeck = new AttackModifierDeckState(state.NeutralMonsterAttackModifierDeck, references);
			references.Add(state.NeutralMonsterAttackModifierDeck, NeutralMonsterAttackModifierDeck);
		}
		BossAttackModifierDeck = references.Get(state.BossAttackModifierDeck);
		if (BossAttackModifierDeck == null && state.BossAttackModifierDeck != null)
		{
			BossAttackModifierDeck = new AttackModifierDeckState(state.BossAttackModifierDeck, references);
			references.Add(state.BossAttackModifierDeck, BossAttackModifierDeck);
		}
		EnemyClasses = references.Get(state.EnemyClasses);
		if (EnemyClasses != null || state.EnemyClasses == null)
		{
			return;
		}
		EnemyClasses = new List<EnemyClassState>();
		for (int i = 0; i < state.EnemyClasses.Count; i++)
		{
			EnemyClassState enemyClassState = state.EnemyClasses[i];
			EnemyClassState enemyClassState2 = references.Get(enemyClassState);
			if (enemyClassState2 == null && enemyClassState != null)
			{
				enemyClassState2 = new EnemyClassState(enemyClassState, references);
				references.Add(enemyClassState, enemyClassState2);
			}
			EnemyClasses.Add(enemyClassState2);
		}
		references.Add(state.EnemyClasses, EnemyClasses);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("EnemyAttackModifierDeck", EnemyAttackModifierDeck);
		info.AddValue("AlliedMonsterAttackModifierDeck", AlliedMonsterAttackModifierDeck);
		info.AddValue("Enemy2MonsterAttackModifierDeck", Enemy2MonsterAttackModifierDeck);
		info.AddValue("NeutralMonsterAttackModifierDeck", NeutralMonsterAttackModifierDeck);
		info.AddValue("BossAttackModifierDeck", BossAttackModifierDeck);
		info.AddValue("EnemyClasses", EnemyClasses);
	}

	public EnemyClassManagerState(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "EnemyAttackModifierDeck":
					EnemyAttackModifierDeck = (AttackModifierDeckState)info.GetValue("EnemyAttackModifierDeck", typeof(AttackModifierDeckState));
					break;
				case "AlliedMonsterAttackModifierDeck":
					AlliedMonsterAttackModifierDeck = (AttackModifierDeckState)info.GetValue("AlliedMonsterAttackModifierDeck", typeof(AttackModifierDeckState));
					break;
				case "Enemy2MonsterAttackModifierDeck":
					Enemy2MonsterAttackModifierDeck = (AttackModifierDeckState)info.GetValue("Enemy2MonsterAttackModifierDeck", typeof(AttackModifierDeckState));
					break;
				case "NeutralMonsterAttackModifierDeck":
					NeutralMonsterAttackModifierDeck = (AttackModifierDeckState)info.GetValue("NeutralMonsterAttackModifierDeck", typeof(AttackModifierDeckState));
					break;
				case "BossAttackModifierDeck":
					BossAttackModifierDeck = (AttackModifierDeckState)info.GetValue("BossAttackModifierDeck", typeof(AttackModifierDeckState));
					break;
				case "EnemyClasses":
					EnemyClasses = (List<EnemyClassState>)info.GetValue("EnemyClasses", typeof(List<EnemyClassState>));
					break;
				case "AttackModifierDeck":
					EnemyAttackModifierDeck = (AttackModifierDeckState)info.GetValue("AttackModifierDeck", typeof(AttackModifierDeckState));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize EnemyClassManagerState entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		if (AlliedMonsterAttackModifierDeck == null)
		{
			AlliedMonsterAttackModifierDeck = new AttackModifierDeckState();
		}
		if (Enemy2MonsterAttackModifierDeck == null)
		{
			Enemy2MonsterAttackModifierDeck = new AttackModifierDeckState();
		}
		if (NeutralMonsterAttackModifierDeck == null)
		{
			NeutralMonsterAttackModifierDeck = new AttackModifierDeckState();
		}
		if (BossAttackModifierDeck == null)
		{
			BossAttackModifierDeck = new AttackModifierDeckState();
		}
	}

	public EnemyClassManagerState()
	{
		EnemyAttackModifierDeck = new AttackModifierDeckState();
		AlliedMonsterAttackModifierDeck = new AttackModifierDeckState();
		Enemy2MonsterAttackModifierDeck = new AttackModifierDeckState();
		NeutralMonsterAttackModifierDeck = new AttackModifierDeckState();
		BossAttackModifierDeck = new AttackModifierDeckState();
		EnemyClasses = new List<EnemyClassState>();
	}

	public EnemyClassManagerState Copy()
	{
		return new EnemyClassManagerState
		{
			EnemyAttackModifierDeck = EnemyAttackModifierDeck.Copy(),
			AlliedMonsterAttackModifierDeck = AlliedMonsterAttackModifierDeck.Copy(),
			Enemy2MonsterAttackModifierDeck = Enemy2MonsterAttackModifierDeck.Copy(),
			NeutralMonsterAttackModifierDeck = NeutralMonsterAttackModifierDeck.Copy(),
			BossAttackModifierDeck = BossAttackModifierDeck.Copy(),
			EnemyClasses = EnemyClasses.ToList()
		};
	}

	public void Save()
	{
		EnemyAttackModifierDeck.SaveMonster(MonsterClassManager.EnemyMonsterAttackModifierDeck);
		AlliedMonsterAttackModifierDeck.SaveMonster(MonsterClassManager.AlliedMonsterAttackModifierDeck);
		Enemy2MonsterAttackModifierDeck.SaveMonster(MonsterClassManager.Enemy2MonsterAttackModifierDeck);
		NeutralMonsterAttackModifierDeck.SaveMonster(MonsterClassManager.NeutralMonsterAttackModifierDeck);
		BossAttackModifierDeck.SaveMonster(MonsterClassManager.BossMonsterAttackModifierDeck);
		RefreshEnemyClasses();
		foreach (EnemyClassState enemyClass in EnemyClasses)
		{
			enemyClass.Save();
		}
	}

	public void Load()
	{
		MonsterClassManager.EnemyMonsterAttackModifierDeck.LoadAttackModifierDeck(EnemyAttackModifierDeck);
		MonsterClassManager.AlliedMonsterAttackModifierDeck.LoadAttackModifierDeck(AlliedMonsterAttackModifierDeck);
		MonsterClassManager.Enemy2MonsterAttackModifierDeck.LoadAttackModifierDeck(Enemy2MonsterAttackModifierDeck);
		MonsterClassManager.NeutralMonsterAttackModifierDeck.LoadAttackModifierDeck(NeutralMonsterAttackModifierDeck);
		MonsterClassManager.BossMonsterAttackModifierDeck.LoadAttackModifierDeck(BossAttackModifierDeck);
		foreach (EnemyClassState enemyClass in EnemyClasses)
		{
			enemyClass.Load();
		}
	}

	private void RefreshEnemyClasses()
	{
		if (ScenarioManager.CurrentScenarioState == null)
		{
			return;
		}
		foreach (EnemyState allEnemyState in ScenarioManager.CurrentScenarioState.AllEnemyStates)
		{
			CMonsterClass monsterClassToUse = MonsterClassManager.Find(allEnemyState.ClassID);
			monsterClassToUse = ((monsterClassToUse.NonEliteVariant != null) ? monsterClassToUse.NonEliteVariant : monsterClassToUse);
			if (!EnemyClasses.Any((EnemyClassState a) => a.ClassID == monsterClassToUse.ID))
			{
				EnemyClasses.Add(new EnemyClassState(monsterClassToUse.ID, new AbilityDeckState(monsterClassToUse.AbilityCardsPool.Select((CMonsterAbilityCard c) => new Tuple<int, int>(c.ID, c.ID)).ToList())));
			}
		}
	}

	public static List<Tuple<int, string>> Compare(EnemyClassManagerState state1, EnemyClassManagerState state2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			list.AddRange(AttackModifierDeckState.Compare(state1.EnemyAttackModifierDeck, state2.EnemyAttackModifierDeck, "NA", "Enemy Modifier Deck", "NA", isMPCompare));
			list.AddRange(AttackModifierDeckState.Compare(state1.AlliedMonsterAttackModifierDeck, state2.AlliedMonsterAttackModifierDeck, "NA", "Ally Modifier Deck", "NA", isMPCompare));
			list.AddRange(AttackModifierDeckState.Compare(state1.Enemy2MonsterAttackModifierDeck, state2.Enemy2MonsterAttackModifierDeck, "NA", "Enemy 2 Modifier Deck", "NA", isMPCompare));
			list.AddRange(AttackModifierDeckState.Compare(state1.NeutralMonsterAttackModifierDeck, state2.NeutralMonsterAttackModifierDeck, "NA", "Neutral Modifier Deck", "NA", isMPCompare));
			list.AddRange(AttackModifierDeckState.Compare(state1.BossAttackModifierDeck, state2.BossAttackModifierDeck, "NA", "Boss Modifier Deck", "NA", isMPCompare));
			switch (StateShared.CheckNullsMatch(state1.EnemyClasses, state2.EnemyClasses))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 1401, "Enemy Class Manager State EnemyClasses null state does not match.", new List<string[]> { new string[3]
				{
					"EnemyClasses",
					(state1.EnemyClasses == null) ? "is null" : "is not null",
					(state2.EnemyClasses == null) ? "is null" : "is not null"
				} });
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.EnemyClasses.Count != state2.EnemyClasses.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1402, "Enemy Class Manager State total EnemyClasses Count does not match.", new List<string[]> { new string[3]
					{
						"EnemyClasses Count",
						state1.EnemyClasses.Count.ToString(),
						state2.EnemyClasses.Count.ToString()
					} });
					break;
				}
				bool flag = false;
				foreach (EnemyClassState enemyClass in state1.EnemyClasses)
				{
					if (state1.EnemyClasses.Where((EnemyClassState w) => w.ClassID == enemyClass.ClassID).Count() > 1)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1403, "Enemy Class Manager State EnemyClasses in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " contains duplicate ID values.", new List<string[]> { new string[3]
						{
							"Duplicate EnemyClasses",
							"ID " + enemyClass.ClassID + " Count: " + state1.EnemyClasses.Where((EnemyClassState w) => w.ClassID == enemyClass.ClassID).Count(),
							"NA"
						} });
					}
					flag = true;
				}
				foreach (EnemyClassState enemyClass2 in state2.EnemyClasses)
				{
					if (state2.EnemyClasses.Where((EnemyClassState w) => w.ClassID == enemyClass2.ClassID).Count() > 1)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1403, "Enemy Class Manager State EnemyClasses in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " contains duplicate ID values.", new List<string[]> { new string[3]
						{
							"Duplicate EnemyClasses",
							"NA",
							"ID " + enemyClass2.ClassID + " Count: " + state2.EnemyClasses.Where((EnemyClassState w) => w.ClassID == enemyClass2.ClassID).Count()
						} });
					}
					flag = true;
				}
				if (flag)
				{
					break;
				}
				bool flag2 = false;
				foreach (EnemyClassState enemyClass3 in state1.EnemyClasses)
				{
					if (!state2.EnemyClasses.Exists((EnemyClassState e) => e.ClassID == enemyClass3.ClassID))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1404, "Enemy Class Manager State EnemyClasses in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a value contained in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]> { new string[3] { "EnemyClass", enemyClass3.ClassID, "Missing" } });
					}
					flag2 = true;
				}
				foreach (EnemyClassState enemyClass4 in state2.EnemyClasses)
				{
					if (!state1.EnemyClasses.Exists((EnemyClassState e) => e.ClassID == enemyClass4.ClassID))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1404, "Enemy Class Manager State EnemyClasses in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a value contained in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]> { new string[3] { "EnemyClass", "Missing", enemyClass4.ClassID } });
					}
					flag2 = true;
				}
				if (flag2)
				{
					break;
				}
				foreach (EnemyClassState enemyClass5 in state1.EnemyClasses)
				{
					try
					{
						EnemyClassState state3 = state2.EnemyClasses.Single((EnemyClassState s) => s.ClassID == enemyClass5.ClassID);
						list.AddRange(EnemyClassState.Compare(enemyClass5, state3, isMPCompare));
					}
					catch (Exception ex)
					{
						list.Add(new Tuple<int, string>(1405, "Exception during EnemyClassState compare.\n" + ex.Message + "\n" + ex.StackTrace));
					}
				}
				break;
			}
			}
		}
		catch (Exception ex2)
		{
			list.Add(new Tuple<int, string>(1499, "Exception during Enemy Class Manager State compare.\n" + ex2.Message + "\n" + ex2.StackTrace));
		}
		return list;
	}
}
