using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("{ClassID}")]
public class ObjectState : EnemyState, ISerializable
{
	public CObjectActor Object => ScenarioManager.Scenario.AllObjects.SingleOrDefault((CObjectActor s) => s.ActorGuid == base.ActorGuid);

	public bool IsAttachedToProp { get; set; }

	public string PropGuidAttachedTo { get; set; }

	public ObjectState()
	{
	}

	public ObjectState(ObjectState state, ReferenceDictionary references)
		: base(state, references)
	{
		IsAttachedToProp = state.IsAttachedToProp;
		PropGuidAttachedTo = state.PropGuidAttachedTo;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("IsAttachedToProp", IsAttachedToProp);
		info.AddValue("PropGuidAttachedTo", PropGuidAttachedTo);
	}

	public ObjectState(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "IsAttachedToProp"))
				{
					if (name == "PropGuidAttachedTo")
					{
						PropGuidAttachedTo = info.GetString("PropGuidAttachedTo");
					}
				}
				else
				{
					IsAttachedToProp = info.GetBoolean("IsAttachedToProp");
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize ObjectState entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public ObjectState(string classID, int chosenModelIndex, string actorGuid, string mapGuid, TileIndex location, int health, int maxHealth, int level, List<PositiveConditionPair> posCons, List<NegativeConditionPair> negCons, bool playedThisRound, CActor.ECauseOfDeath causeOfDeath, bool isSummon, string summonerGuid, int augmentSlots, CActor.EType type)
		: base(classID, chosenModelIndex, actorGuid, mapGuid, location, health, maxHealth, level, posCons, negCons, playedThisRound, causeOfDeath, isSummon, summonerGuid, augmentSlots, type)
	{
	}

	public ObjectState(string classID, int chosenModelIndex, string mapGuid, CActor.EType type)
		: base(classID, chosenModelIndex, mapGuid, type)
	{
	}

	public ObjectState(CObjectActor objectActor, string mapGuid)
		: base(objectActor, mapGuid)
	{
		IsAttachedToProp = objectActor.IsAttachedToProp;
		PropGuidAttachedTo = (objectActor.IsAttachedToProp ? objectActor.AttachedProp.PropGuid : string.Empty);
	}

	public void InitialisePropAttachment(CObjectProp propToAttach, CObjectActor dummyObjectActor)
	{
		PropGuidAttachedTo = propToAttach.PropGuid;
		IsAttachedToProp = true;
		propToAttach.SetActorAttachedAtRuntime(dummyObjectActor);
		dummyObjectActor?.SetAttachedToProp(propToAttach);
	}

	public override void Load()
	{
		if (Object != null)
		{
			Object.LoadEnemy(this);
		}
		else
		{
			DLLDebug.Log("Could not find Object");
		}
	}

	public override void Save(bool initial, bool forceSave = false)
	{
		if (!(base.IsRevealed || forceSave))
		{
			return;
		}
		CObjectActor cObjectActor = null;
		cObjectActor = ((!initial) ? Object : ScenarioManager.Scenario.InitialObjects.SingleOrDefault((CObjectActor s) => s.ActorGuid == base.ActorGuid));
		if (cObjectActor == null)
		{
			return;
		}
		base.ID = cObjectActor.ID;
		base.Location = new TileIndex(cObjectActor.ArrayIndex.X, cObjectActor.ArrayIndex.Y);
		base.Health = cObjectActor.Health;
		base.MaxHealth = cObjectActor.MaxHealth;
		base.Level = cObjectActor.Level;
		base.PositiveConditions = cObjectActor.Tokens.CheckPositiveTokens.ToList();
		base.NegativeConditions = cObjectActor.Tokens.CheckNegativeTokens.ToList();
		base.PlayedThisRound = cObjectActor.PlayedThisRound;
		base.CauseOfDeath = cObjectActor.CauseOfDeath;
		base.KilledByActorGuid = cObjectActor.KilledByActorGuid;
		base.IsSummon = cObjectActor.IsSummon;
		base.SummonerGuid = cObjectActor.Summoner?.ActorGuid;
		base.Type = cObjectActor.Type;
		base.CharacterResources.Clear();
		foreach (CCharacterResource characterResource in cObjectActor.CharacterResources)
		{
			base.CharacterResources.Add(characterResource.ID, characterResource.Amount);
		}
	}

	public static List<Tuple<int, string>> Compare(ObjectState state1, ObjectState state2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		list.AddRange(EnemyState.Compare(state1, state2, isMPCompare));
		try
		{
			if (state1.IsAttachedToProp != state2.IsAttachedToProp)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 621, "Object State IsAttachedToProp does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Standee ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"IsAttachedToProp",
						state1.IsAttachedToProp.ToString(),
						state2.IsAttachedToProp.ToString()
					}
				});
			}
			if (state1.PropGuidAttachedTo != state2.PropGuidAttachedTo)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 622, "Object State PropGuidAttachedTo does not match.", new List<string[]>
				{
					new string[3] { "Actor GUID", state1.ActorGuid, state2.ActorGuid },
					new string[3] { "Class ID", state1.ClassID, state2.ClassID },
					new string[3]
					{
						"Standee ID",
						state1.ID.ToString(),
						state2.ID.ToString()
					},
					new string[3]
					{
						"Location",
						state1.Location.ToString(),
						state2.Location.ToString()
					},
					new string[3]
					{
						"PropGuidAttachedTo",
						(state1.PropGuidAttachedTo == null) ? "null " : state1.PropGuidAttachedTo.ToString(),
						(state2.PropGuidAttachedTo == null) ? "null " : state2.PropGuidAttachedTo.ToString()
					}
				});
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(698, "Exception during Object State compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
