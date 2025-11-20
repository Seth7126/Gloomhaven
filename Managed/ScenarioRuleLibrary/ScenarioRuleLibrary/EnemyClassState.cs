using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class EnemyClassState : ISerializable
{
	private CMonsterClass m_CachedMonsterClass;

	public string ClassID { get; private set; }

	public AbilityDeckState AbilityDeck { get; private set; }

	public List<int> AvailableIDs { get; private set; }

	public CMonsterClass MonsterClass
	{
		get
		{
			if (m_CachedMonsterClass == null)
			{
				m_CachedMonsterClass = MonsterClassManager.Find(ClassID);
				if (m_CachedMonsterClass == null)
				{
					DLLDebug.LogError("Unable to find MonsterClass in MonsterClassManager MonsterAndObjectClasses for ID: " + ClassID.ToString());
				}
			}
			return m_CachedMonsterClass;
		}
	}

	public EnemyClassState()
	{
	}

	public EnemyClassState(EnemyClassState state, ReferenceDictionary references)
	{
		ClassID = state.ClassID;
		AbilityDeck = references.Get(state.AbilityDeck);
		if (AbilityDeck == null && state.AbilityDeck != null)
		{
			AbilityDeck = new AbilityDeckState(state.AbilityDeck, references);
			references.Add(state.AbilityDeck, AbilityDeck);
		}
		AvailableIDs = references.Get(state.AvailableIDs);
		if (AvailableIDs == null && state.AvailableIDs != null)
		{
			AvailableIDs = new List<int>();
			for (int i = 0; i < state.AvailableIDs.Count; i++)
			{
				int item = state.AvailableIDs[i];
				AvailableIDs.Add(item);
			}
			references.Add(state.AvailableIDs, AvailableIDs);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("ClassID", ClassID);
		info.AddValue("AbilityDeck", AbilityDeck);
		info.AddValue("AvailableIDs", AvailableIDs);
	}

	public EnemyClassState(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "ClassID":
					ClassID = info.GetString("ClassID");
					break;
				case "AbilityDeck":
					AbilityDeck = (AbilityDeckState)info.GetValue("AbilityDeck", typeof(AbilityDeckState));
					break;
				case "AvailableIDs":
					AvailableIDs = (List<int>)info.GetValue("AvailableIDs", typeof(List<int>));
					break;
				case "ClassName":
				{
					string text = info.GetString("ClassName").Replace(" ", string.Empty);
					ClassID = text + "ID";
					break;
				}
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize EnemyClassState entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public EnemyClassState(string classID, AbilityDeckState deck)
	{
		ClassID = classID;
		AbilityDeck = deck;
	}

	public void Save()
	{
		if (MonsterClass.NonEliteVariant != null)
		{
			AbilityDeck.Save(MonsterClass.NonEliteVariant);
		}
		else
		{
			AbilityDeck.Save(MonsterClass);
		}
		AvailableIDs = ((MonsterClass.NonEliteVariant != null) ? MonsterClass.NonEliteVariant.AvailableIDs.ToList() : MonsterClass.AvailableIDs.ToList());
	}

	public void Load()
	{
		MonsterClass.LoadAbilityDeck(AbilityDeck);
		if (MonsterClass.NonEliteVariant != null)
		{
			MonsterClass.NonEliteVariant.LoadAbilityDeck(AbilityDeck);
		}
		MonsterClass.LoadAvailableIDs(AvailableIDs);
		if (MonsterClass.NonEliteVariant != null)
		{
			MonsterClass.NonEliteVariant.LoadAvailableIDs(AvailableIDs);
		}
	}

	public void ResetIDs()
	{
		AvailableIDs = null;
	}

	public static List<Tuple<int, string>> Compare(EnemyClassState state1, EnemyClassState state2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			if (state1.ClassID != state2.ClassID)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 1501, "Enemy Class State ID does not match.", new List<string[]> { new string[3] { "Class ID", state1.ClassID, state2.ClassID } });
			}
			list.AddRange(AbilityDeckState.Compare(state1.AbilityDeck, state2.AbilityDeck, "NA", state1.ClassID, isMPCompare, state1.MonsterClass.ID));
			switch (StateShared.CheckNullsMatch(state1.AvailableIDs, state2.AvailableIDs))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 1503, "Enemy Class State AvailableIDs null state does not match.", new List<string[]>
				{
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"AvailableIDs",
						(state1.AvailableIDs == null) ? "is null" : "is not null",
						(state2.AvailableIDs == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
			{
				if (state1.AvailableIDs.Count != state2.AvailableIDs.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1504, "Enemy Class State total AvailableIDs Count does not match.", new List<string[]>
					{
						new string[3] { "Class ID", state1.ClassID, state2.ClassID },
						new string[3]
						{
							"AvailableIDs Count",
							state1.AvailableIDs.Count.ToString(),
							state2.AvailableIDs.Count.ToString()
						}
					});
					break;
				}
				bool flag = false;
				foreach (int id in state1.AvailableIDs)
				{
					if (state1.AvailableIDs.Where((int w) => w == id).Count() > 1)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1505, "Enemy Class State AvailableIDs in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " contains duplicate ID values.", new List<string[]>
						{
							new string[3] { "Class ID", state1.ClassID, state2.ClassID },
							new string[3]
							{
								"Duplicate AvailableID",
								"ID " + id + " Count: " + state1.AvailableIDs.Where((int w) => w == id).Count(),
								"NA"
							}
						});
					}
					flag = true;
				}
				foreach (int id2 in state2.AvailableIDs)
				{
					if (state2.AvailableIDs.Where((int w) => w == id2).Count() > 1)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1505, "Enemy Class State AvailableIDs in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " contains duplicate ID values.", new List<string[]>
						{
							new string[3] { "Class ID", state1.ClassID, state2.ClassID },
							new string[3]
							{
								"Duplicate AvailableID",
								"NA",
								"ID " + id2 + " Count: " + state2.AvailableIDs.Where((int w) => w == id2).Count()
							}
						});
					}
					flag = true;
				}
				if (flag)
				{
					break;
				}
				foreach (int availableID in state1.AvailableIDs)
				{
					if (!state2.AvailableIDs.Contains(availableID))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1506, "Enemy Class State AvailableIDs in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a value contained in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Class ID", state1.ClassID, state2.ClassID },
							new string[3]
							{
								"AvailableID",
								availableID.ToString(),
								"Missing"
							}
						});
					}
				}
				foreach (int availableID2 in state2.AvailableIDs)
				{
					if (!state1.AvailableIDs.Contains(availableID2))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1506, "Enemy Class State AvailableIDs in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a value contained in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Class ID", state1.ClassID, state2.ClassID },
							new string[3]
							{
								"AvailableID",
								"Missing",
								availableID2.ToString()
							}
						});
					}
				}
				break;
			}
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(1599, "Exception during Enemy Class State compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
