using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class PropHealthDetails : ISerializable
{
	public enum EDeathAction
	{
		None,
		Destruct,
		Activate
	}

	public static EDeathAction[] DeathActions = (EDeathAction[])Enum.GetValues(typeof(EDeathAction));

	public bool HasHealth;

	public List<int> MaxHealthPerPartySize;

	public int CurrentHealth;

	public string CustomLocKey;

	public CActor.EType ActorType;

	public EDeathAction DeathAction;

	public string ActorSpriteName;

	public CAbility.EStatIsBasedOnXType HealthBasedOnXType;

	public float HealthBasedOnXVariable;

	public float HealthBasedOnYVariable;

	public bool IgnoredByAIFocus;

	public PropHealthDetails(PropHealthDetails state, ReferenceDictionary references)
	{
		HasHealth = state.HasHealth;
		MaxHealthPerPartySize = references.Get(state.MaxHealthPerPartySize);
		if (MaxHealthPerPartySize == null && state.MaxHealthPerPartySize != null)
		{
			MaxHealthPerPartySize = new List<int>();
			for (int i = 0; i < state.MaxHealthPerPartySize.Count; i++)
			{
				int item = state.MaxHealthPerPartySize[i];
				MaxHealthPerPartySize.Add(item);
			}
			references.Add(state.MaxHealthPerPartySize, MaxHealthPerPartySize);
		}
		CurrentHealth = state.CurrentHealth;
		CustomLocKey = state.CustomLocKey;
		ActorType = state.ActorType;
		DeathAction = state.DeathAction;
		ActorSpriteName = state.ActorSpriteName;
		HealthBasedOnXType = state.HealthBasedOnXType;
		HealthBasedOnXVariable = state.HealthBasedOnXVariable;
		HealthBasedOnYVariable = state.HealthBasedOnYVariable;
		IgnoredByAIFocus = state.IgnoredByAIFocus;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("HasHealth", HasHealth);
		info.AddValue("MaxHealthPerPartySize", MaxHealthPerPartySize);
		info.AddValue("CurrentHealth", CurrentHealth);
		info.AddValue("CustomLocKey", CustomLocKey);
		info.AddValue("ActorType", ActorType);
		info.AddValue("DeathAction", DeathAction);
		info.AddValue("ActorSpriteName", ActorSpriteName);
		info.AddValue("HealthBasedOnXType", HealthBasedOnXType);
		info.AddValue("HealthBasedOnXVariable", HealthBasedOnXVariable);
		info.AddValue("HealthBasedOnYVariable", HealthBasedOnYVariable);
		info.AddValue("IgnoredByAIFocus", IgnoredByAIFocus);
	}

	public PropHealthDetails(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "HasHealth":
					HasHealth = info.GetBoolean("HasHealth");
					break;
				case "MaxHealthPerPartySize":
					MaxHealthPerPartySize = (List<int>)info.GetValue("MaxHealthPerPartySize", typeof(List<int>));
					break;
				case "CurrentHealth":
					CurrentHealth = info.GetInt32("CurrentHealth");
					break;
				case "CustomLocKey":
					CustomLocKey = info.GetString("CustomLocKey");
					break;
				case "ActorType":
					ActorType = (CActor.EType)info.GetValue("ActorType", typeof(CActor.EType));
					break;
				case "DeathAction":
					DeathAction = (EDeathAction)info.GetValue("DeathAction", typeof(EDeathAction));
					break;
				case "ActorSpriteName":
					ActorSpriteName = info.GetString("ActorSpriteName");
					break;
				case "HealthBasedOnXType":
					HealthBasedOnXType = (CAbility.EStatIsBasedOnXType)info.GetValue("HealthBasedOnXType", typeof(CAbility.EStatIsBasedOnXType));
					break;
				case "HealthBasedOnXVariable":
					HealthBasedOnXVariable = info.GetSingle("HealthBasedOnXVariable");
					break;
				case "HealthBasedOnYVariable":
					HealthBasedOnYVariable = info.GetSingle("HealthBasedOnYVariable");
					break;
				case "IgnoredByAIFocus":
					IgnoredByAIFocus = info.GetBoolean("IgnoredByAIFocus");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize PropHealthDetails entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public PropHealthDetails()
	{
		HasHealth = false;
		MaxHealthPerPartySize = new List<int> { 0, 0, 0, 0 };
		CurrentHealth = 0;
		CustomLocKey = string.Empty;
		ActorType = CActor.EType.Enemy;
		DeathAction = EDeathAction.Activate;
		ActorSpriteName = string.Empty;
	}

	public int GetPropStartingHealth()
	{
		int result = MaxHealthPerPartySize[Math.Max(0, ScenarioManager.CurrentScenarioState.Players.Count - 1)];
		if (HealthBasedOnXType != CAbility.EStatIsBasedOnXType.None)
		{
			result = CMonsterClass.GetBaseStatIsBasedOnXValue(null, HealthBasedOnXType, HealthBasedOnXVariable, HealthBasedOnYVariable);
		}
		return result;
	}

	public ObjectState CreateStateForPropWithHealth(CTile propTile, int propStartingHealth, CActor.EType type)
	{
		return new ObjectState("PropDummyObject", 0, null, propTile.m_HexMap.MapGuid, new TileIndex(propTile.m_ArrayIndex), CurrentHealth, propStartingHealth, 1, new List<PositiveConditionPair>(), new List<NegativeConditionPair>(), playedThisRound: true, CActor.ECauseOfDeath.StillAlive, isSummon: false, null, 1, type);
	}

	public static List<Tuple<int, string>> Compare(PropHealthDetails propHealth1, PropHealthDetails propHealth2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			switch (StateShared.CheckNullsMatch(propHealth1, propHealth2))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 1630, "PropHealthDetails null state does not match.", new List<string[]> { new string[3]
				{
					"PropHealthDetails",
					(propHealth1 == null) ? "is null" : "is not null",
					(propHealth2 == null) ? "is null" : "is not null"
				} });
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (propHealth1.HasHealth != propHealth2.HasHealth)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1631, "PropHealthDetails HasHealth does not match.", new List<string[]>
					{
						new string[3]
						{
							"ActorType",
							propHealth1.ActorType.ToString(),
							propHealth2.ActorType.ToString()
						},
						new string[3]
						{
							"HasHealth",
							propHealth1.HasHealth.ToString(),
							propHealth2.HasHealth.ToString()
						}
					});
				}
				if (propHealth1.CurrentHealth != propHealth2.CurrentHealth)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1632, "PropHealthDetails CurrentHealth does not match.", new List<string[]>
					{
						new string[3]
						{
							"ActorType",
							propHealth1.ActorType.ToString(),
							propHealth2.ActorType.ToString()
						},
						new string[3]
						{
							"CurrentHealth",
							propHealth1.CurrentHealth.ToString(),
							propHealth2.CurrentHealth.ToString()
						}
					});
				}
				if (propHealth1.CustomLocKey != propHealth2.CustomLocKey)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1633, "PropHealthDetails CustomLocKey does not match.", new List<string[]>
					{
						new string[3]
						{
							"ActorType",
							propHealth1.ActorType.ToString(),
							propHealth2.ActorType.ToString()
						},
						new string[3] { "CustomLocKey", propHealth1.CustomLocKey, propHealth2.CustomLocKey }
					});
				}
				if (propHealth1.ActorType != propHealth2.ActorType)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1634, "PropHealthDetails ActorType does not match.", new List<string[]> { new string[3]
					{
						"ActorType",
						propHealth1.ActorType.ToString(),
						propHealth2.ActorType.ToString()
					} });
				}
				if (propHealth1.DeathAction != propHealth2.DeathAction)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1635, "PropHealthDetails DeathAction does not match.", new List<string[]>
					{
						new string[3]
						{
							"ActorType",
							propHealth1.ActorType.ToString(),
							propHealth2.ActorType.ToString()
						},
						new string[3]
						{
							"DeathAction",
							propHealth1.DeathAction.ToString(),
							propHealth2.DeathAction.ToString()
						}
					});
				}
				if (propHealth1.ActorSpriteName != propHealth2.ActorSpriteName)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1636, "PropHealthDetails ActorSpriteName does not match.", new List<string[]>
					{
						new string[3]
						{
							"ActorType",
							propHealth1.ActorType.ToString(),
							propHealth2.ActorType.ToString()
						},
						new string[3] { "ActorSpriteName", propHealth1.ActorSpriteName, propHealth2.ActorSpriteName }
					});
				}
				if (propHealth1.HealthBasedOnXType != propHealth2.HealthBasedOnXType)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1637, "PropHealthDetails HealthBasedOnXType does not match.", new List<string[]>
					{
						new string[3]
						{
							"ActorType",
							propHealth1.ActorType.ToString(),
							propHealth2.ActorType.ToString()
						},
						new string[3]
						{
							"HealthBasedOnXType",
							propHealth1.HealthBasedOnXType.ToString(),
							propHealth2.HealthBasedOnXType.ToString()
						}
					});
				}
				if (propHealth1.HealthBasedOnXVariable != propHealth2.HealthBasedOnXVariable)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1638, "PropHealthDetails HealthBasedOnXVariable does not match.", new List<string[]>
					{
						new string[3]
						{
							"ActorType",
							propHealth1.ActorType.ToString(),
							propHealth2.ActorType.ToString()
						},
						new string[3]
						{
							"HealthBasedOnXVariable",
							propHealth1.HealthBasedOnXVariable.ToString(),
							propHealth2.HealthBasedOnXVariable.ToString()
						}
					});
				}
				if (propHealth1.HealthBasedOnYVariable != propHealth2.HealthBasedOnYVariable)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1639, "PropHealthDetails HealthBasedOnYVariable does not match.", new List<string[]>
					{
						new string[3]
						{
							"ActorType",
							propHealth1.ActorType.ToString(),
							propHealth2.ActorType.ToString()
						},
						new string[3]
						{
							"HealthBasedOnYVariable",
							propHealth1.HealthBasedOnYVariable.ToString(),
							propHealth2.HealthBasedOnYVariable.ToString()
						}
					});
				}
				if (propHealth1.IgnoredByAIFocus != propHealth2.IgnoredByAIFocus)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 1640, "PropHealthDetails IgnoredByAIFocus does not match.", new List<string[]>
					{
						new string[3]
						{
							"ActorType",
							propHealth1.ActorType.ToString(),
							propHealth2.ActorType.ToString()
						},
						new string[3]
						{
							"IgnoredByAIFocus",
							propHealth1.IgnoredByAIFocus.ToString(),
							propHealth2.IgnoredByAIFocus.ToString()
						}
					});
				}
				switch (StateShared.CheckNullsMatch(propHealth1.MaxHealthPerPartySize, propHealth2.MaxHealthPerPartySize))
				{
				case StateShared.ENullStatus.Mismatch:
					ScenarioState.LogMismatch(list, isMPCompare, 1641, "PropHealthDetails MaxHealthPerPartySize null state does not match.", new List<string[]>
					{
						new string[3]
						{
							"ActorType",
							propHealth1.ActorType.ToString(),
							propHealth2.ActorType.ToString()
						},
						new string[3]
						{
							"DeathAction",
							propHealth1.DeathAction.ToString(),
							propHealth2.DeathAction.ToString()
						},
						new string[3]
						{
							"MaxHealthPerPartySize",
							(propHealth1.MaxHealthPerPartySize == null) ? "is null" : "is not null",
							(propHealth2.MaxHealthPerPartySize == null) ? "is null" : "is not null"
						}
					});
					break;
				case StateShared.ENullStatus.BothNotNull:
				{
					if (propHealth1.MaxHealthPerPartySize.Count != propHealth2.MaxHealthPerPartySize.Count)
					{
						ScenarioState.LogMismatch(list, isMPCompare, 1642, "PropHealthDetails total MaxHealthPerPartySize Count does not match.", new List<string[]>
						{
							new string[3]
							{
								"ActorType",
								propHealth1.ActorType.ToString(),
								propHealth2.ActorType.ToString()
							},
							new string[3]
							{
								"DeathAction",
								propHealth1.DeathAction.ToString(),
								propHealth2.DeathAction.ToString()
							},
							new string[3]
							{
								"MaxHealthPerPartySize Count",
								propHealth1.MaxHealthPerPartySize.Count.ToString(),
								propHealth2.MaxHealthPerPartySize.Count.ToString()
							}
						});
						break;
					}
					for (int i = 0; i < propHealth1.MaxHealthPerPartySize.Count; i++)
					{
						if (propHealth2.MaxHealthPerPartySize[i] != propHealth1.MaxHealthPerPartySize[i])
						{
							ScenarioState.LogMismatch(list, isMPCompare, 1643, $"PropHealthDetails MaxHealthPerPartySize entry number {i} does not match.", new List<string[]>
							{
								new string[3]
								{
									"ActorType",
									propHealth1.ActorType.ToString(),
									propHealth2.ActorType.ToString()
								},
								new string[3]
								{
									"DeathAction",
									propHealth1.DeathAction.ToString(),
									propHealth2.DeathAction.ToString()
								},
								new string[3]
								{
									"MaxHealthPerPartySize Entry Value",
									propHealth1.MaxHealthPerPartySize[i].ToString(),
									propHealth2.MaxHealthPerPartySize[i].ToString()
								}
							});
						}
					}
					break;
				}
				}
				break;
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(1698, "Exception during PropHealthDetails compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
