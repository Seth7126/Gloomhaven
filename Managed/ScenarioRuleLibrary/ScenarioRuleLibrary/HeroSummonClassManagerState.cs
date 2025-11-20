using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class HeroSummonClassManagerState : ISerializable
{
	public List<HeroSummonClassState> HeroSummonClasses { get; private set; }

	public HeroSummonClassManagerState(HeroSummonClassManagerState state, ReferenceDictionary references)
	{
		HeroSummonClasses = references.Get(state.HeroSummonClasses);
		if (HeroSummonClasses != null || state.HeroSummonClasses == null)
		{
			return;
		}
		HeroSummonClasses = new List<HeroSummonClassState>();
		for (int i = 0; i < state.HeroSummonClasses.Count; i++)
		{
			HeroSummonClassState heroSummonClassState = state.HeroSummonClasses[i];
			HeroSummonClassState heroSummonClassState2 = references.Get(heroSummonClassState);
			if (heroSummonClassState2 == null && heroSummonClassState != null)
			{
				heroSummonClassState2 = new HeroSummonClassState(heroSummonClassState, references);
				references.Add(heroSummonClassState, heroSummonClassState2);
			}
			HeroSummonClasses.Add(heroSummonClassState2);
		}
		references.Add(state.HeroSummonClasses, HeroSummonClasses);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("HeroSummonClasses", HeroSummonClasses);
	}

	public HeroSummonClassManagerState(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				if (current.Name == "HeroSummonClasses")
				{
					HeroSummonClasses = (List<HeroSummonClassState>)info.GetValue("HeroSummonClasses", typeof(List<HeroSummonClassState>));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize HeroSummonClassManagerState entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public HeroSummonClassManagerState()
	{
		HeroSummonClasses = new List<HeroSummonClassState>();
		RefreshHeroSummonClasses();
		foreach (HeroSummonClassState heroSummonClass in HeroSummonClasses)
		{
			heroSummonClass.Save();
		}
	}

	public HeroSummonClassManagerState Copy()
	{
		return new HeroSummonClassManagerState
		{
			HeroSummonClasses = HeroSummonClasses.ToList()
		};
	}

	public void Save()
	{
		RefreshHeroSummonClasses();
		foreach (HeroSummonClassState heroSummonClass in HeroSummonClasses)
		{
			heroSummonClass.Save();
		}
	}

	public void Load()
	{
		foreach (HeroSummonClassState heroSummonClass in HeroSummonClasses)
		{
			heroSummonClass.Load();
		}
	}

	private void RefreshHeroSummonClasses()
	{
		if (ScenarioManager.Scenario == null)
		{
			return;
		}
		foreach (CHeroSummonActor heroSummonActor in ScenarioManager.Scenario.AllHeroSummons)
		{
			if (!HeroSummonClasses.Any((HeroSummonClassState a) => a.ClassID == heroSummonActor.Class.ID))
			{
				HeroSummonClasses.Add(new HeroSummonClassState(heroSummonActor.Class.ID));
			}
		}
	}

	public static List<Tuple<int, string>> Compare(HeroSummonClassManagerState state1, HeroSummonClassManagerState state2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			switch (StateShared.CheckNullsMatch(state1.HeroSummonClasses, state2.HeroSummonClasses))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 3101, "Hero Summon Class Manager State HeroSummonClasses null state does not match.", new List<string[]> { new string[3]
				{
					"HeroSummonClasses",
					(state1.HeroSummonClasses == null) ? "is null" : "is not null",
					(state2.HeroSummonClasses == null) ? "is null" : "is not null"
				} });
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.HeroSummonClasses.Count != state2.HeroSummonClasses.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 3102, "Hero Summon Class Manager State total EnemyClasses Count does not match.", new List<string[]> { new string[3]
					{
						"HeroSummonClasses Count",
						state1.HeroSummonClasses.Count.ToString(),
						state2.HeroSummonClasses.Count.ToString()
					} });
					break;
				}
				bool flag = false;
				foreach (HeroSummonClassState heroSummonClass in state1.HeroSummonClasses)
				{
					if (state1.HeroSummonClasses.Where((HeroSummonClassState w) => w.ClassID == heroSummonClass.ClassID).Count() > 1)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 3103, "Hero Summon Class Manager State HeroSummonClasses in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " contains duplicate ID values.", new List<string[]> { new string[3]
						{
							"Duplicate HeroSummonClasses",
							"ID " + heroSummonClass.ClassID + " Count: " + state1.HeroSummonClasses.Where((HeroSummonClassState w) => w.ClassID == heroSummonClass.ClassID).Count(),
							"NA"
						} });
					}
					flag = true;
				}
				foreach (HeroSummonClassState heroSummonClass2 in state2.HeroSummonClasses)
				{
					if (state2.HeroSummonClasses.Where((HeroSummonClassState w) => w.ClassID == heroSummonClass2.ClassID).Count() > 1)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 3103, "Hero Summon Class Manager State HeroSummonClasses in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " contains duplicate ID values.", new List<string[]> { new string[3]
						{
							"Duplicate HeroSummonClasses",
							"NA",
							"ID " + heroSummonClass2.ClassID + " Count: " + state2.HeroSummonClasses.Where((HeroSummonClassState w) => w.ClassID == heroSummonClass2.ClassID).Count()
						} });
					}
					flag = true;
				}
				if (flag)
				{
					break;
				}
				bool flag2 = false;
				foreach (HeroSummonClassState heroSummonClass3 in state1.HeroSummonClasses)
				{
					if (!state2.HeroSummonClasses.Exists((HeroSummonClassState e) => e.ClassID == heroSummonClass3.ClassID))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 3104, "Hero Summon Class Manager State HeroSummonClasses in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a value contained in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]> { new string[3] { "HeroSummonClass", heroSummonClass3.ClassID, "Missing" } });
					}
					flag2 = true;
				}
				foreach (HeroSummonClassState heroSummonClass4 in state2.HeroSummonClasses)
				{
					if (!state1.HeroSummonClasses.Exists((HeroSummonClassState e) => e.ClassID == heroSummonClass4.ClassID))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 3104, "Hero Summon Class Manager State HeroSummonClasses in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a value contained in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]> { new string[3] { "HeroSummonClass", heroSummonClass4.ClassID, "Missing" } });
					}
					flag2 = true;
				}
				if (flag2)
				{
					break;
				}
				foreach (HeroSummonClassState heroSummonClass5 in state1.HeroSummonClasses)
				{
					try
					{
						HeroSummonClassState state3 = state2.HeroSummonClasses.Single((HeroSummonClassState s) => s.ClassID == heroSummonClass5.ClassID);
						list.AddRange(HeroSummonClassState.Compare(heroSummonClass5, state3, isMPCompare));
					}
					catch (Exception ex)
					{
						list.Add(new Tuple<int, string>(3105, "Exception during HeroSummonClassState compare.\n" + ex.Message + "\n" + ex.StackTrace));
					}
				}
				break;
			}
			}
		}
		catch (Exception ex2)
		{
			list.Add(new Tuple<int, string>(3199, "Exception during Hero Summon Class Manager State compare.\n" + ex2.Message + "\n" + ex2.StackTrace));
		}
		return list;
	}
}
