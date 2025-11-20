using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
[DebuggerDisplay("{PrefabName} : {PropGuid}")]
public class CObjectObstacle : CObjectProp, ISerializable
{
	public List<TileIndex> PathingBlockers { get; set; }

	public string PropGUIDToCopy { get; set; }

	public bool SetPositionToCenterOfAllPathBlockers { get; protected set; }

	public bool IgnoresFlyAndJump { get; protected set; }

	public CObjectObstacle()
	{
	}

	public CObjectObstacle(CObjectObstacle state, ReferenceDictionary references)
		: base(state, references)
	{
		PathingBlockers = references.Get(state.PathingBlockers);
		if (PathingBlockers == null && state.PathingBlockers != null)
		{
			PathingBlockers = new List<TileIndex>();
			for (int i = 0; i < state.PathingBlockers.Count; i++)
			{
				TileIndex tileIndex = state.PathingBlockers[i];
				TileIndex tileIndex2 = references.Get(tileIndex);
				if (tileIndex2 == null && tileIndex != null)
				{
					tileIndex2 = new TileIndex(tileIndex, references);
					references.Add(tileIndex, tileIndex2);
				}
				PathingBlockers.Add(tileIndex2);
			}
			references.Add(state.PathingBlockers, PathingBlockers);
		}
		SetPositionToCenterOfAllPathBlockers = state.SetPositionToCenterOfAllPathBlockers;
		IgnoresFlyAndJump = state.IgnoresFlyAndJump;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("PathingBlockers", PathingBlockers);
		info.AddValue("SetPositionToCenterOfAllPathBlockers", SetPositionToCenterOfAllPathBlockers);
		info.AddValue("IgnoresFlyAndJump", IgnoresFlyAndJump);
	}

	public CObjectObstacle(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "PathingBlockers":
					PathingBlockers = (List<TileIndex>)info.GetValue("PathingBlockers", typeof(List<TileIndex>));
					break;
				case "SetPositionToCenterOfAllPathBlockers":
					SetPositionToCenterOfAllPathBlockers = info.GetBoolean("SetPositionToCenterOfAllPathBlockers");
					break;
				case "IgnoresFlyAndJump":
					IgnoresFlyAndJump = info.GetBoolean("IgnoresFlyAndJump");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CObjectObstacle entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CObjectObstacle(string name, ScenarioManager.ObjectImportType type, TileIndex arrayIndex, CVector3 position, CVector3 rotation, List<TileIndex> pathingBlockers, CActor owner, string mapGuid, bool ignoresFlyAndJump, bool setPositionToCenterOfAllPathBlockers = false)
		: base(name, type, arrayIndex, position, rotation, owner, mapGuid)
	{
		PathingBlockers = pathingBlockers;
		SetPositionToCenterOfAllPathBlockers = setPositionToCenterOfAllPathBlockers;
		IgnoresFlyAndJump = ignoresFlyAndJump;
	}

	public CObjectObstacle(string name, ScenarioManager.ObjectImportType type, string mapGuid)
		: base(name, type, null, null, null, null, mapGuid)
	{
	}

	public CObjectObstacle(SharedLibrary.Random scenarioGenerationRNG, string name, ScenarioManager.ObjectImportType type, string mapGuid)
		: base(scenarioGenerationRNG, name, type, null, null, null, null, mapGuid)
	{
	}

	public override void DestroyProp(float spawnDelay = 0f, bool sendMessageToClient = true)
	{
		GameState.LastDestroyedObstacle = this;
		base.DestroyProp(spawnDelay, sendMessageToClient);
	}

	public void SetIgnoreFlyAndJump(bool ignoresFlyAndJump)
	{
		IgnoresFlyAndJump = ignoresFlyAndJump;
	}

	public static List<Tuple<int, string>> Compare(CObjectObstacle obs1, CObjectObstacle obs2, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			list.AddRange(CObjectProp.Compare(obs1, obs2, isMPCompare));
			if (obs1.SetPositionToCenterOfAllPathBlockers != obs2.SetPositionToCenterOfAllPathBlockers)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2001, "CObjectObstacle SetPositionToCenterOfAllPathBlockers does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", obs1.PropGuid, obs2.PropGuid },
					new string[3]
					{
						"ObjectType",
						obs1.ObjectType.ToString(),
						obs2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", obs1.PrefabName, obs2.PrefabName },
					new string[3]
					{
						"SetPositionToCenterOfAllPathBlockers",
						obs1.SetPositionToCenterOfAllPathBlockers.ToString(),
						obs2.SetPositionToCenterOfAllPathBlockers.ToString()
					}
				});
			}
			if (obs1.SetPositionToCenterOfAllPathBlockers != obs2.SetPositionToCenterOfAllPathBlockers)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 2005, "CObjectObstacle IgnoresFlyAndJump does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", obs1.PropGuid, obs2.PropGuid },
					new string[3]
					{
						"ObjectType",
						obs1.ObjectType.ToString(),
						obs2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", obs1.PrefabName, obs2.PrefabName },
					new string[3]
					{
						"IgnoresFlyAndJump",
						obs1.IgnoresFlyAndJump.ToString(),
						obs2.IgnoresFlyAndJump.ToString()
					}
				});
			}
			switch (StateShared.CheckNullsMatch(obs1.PathingBlockers, obs2.PathingBlockers))
			{
			case StateShared.ENullStatus.Mismatch:
				ScenarioState.LogMismatch(list, isMPCompare, 2002, "CObjectObstacle PathingBlockers Null state does not match.", new List<string[]>
				{
					new string[3] { "Prop GUID", obs1.PropGuid, obs2.PropGuid },
					new string[3]
					{
						"ObjectType",
						obs1.ObjectType.ToString(),
						obs2.ObjectType.ToString()
					},
					new string[3] { "PrefabName", obs1.PrefabName, obs2.PrefabName },
					new string[3]
					{
						"PathingBlockers",
						(obs1.PathingBlockers == null) ? "is null" : "is not null",
						(obs2.PathingBlockers == null) ? "is null" : "is not null"
					}
				});
				break;
			case StateShared.ENullStatus.BothNotNull:
				if (obs1.PathingBlockers.Count != obs2.PathingBlockers.Count)
				{
					ScenarioState.LogMismatch(list, isMPCompare, 2003, "CObjectObstacle PathingBlockers Count does not match.", new List<string[]>
					{
						new string[3] { "Prop GUID", obs1.PropGuid, obs2.PropGuid },
						new string[3]
						{
							"ObjectType",
							obs1.ObjectType.ToString(),
							obs2.ObjectType.ToString()
						},
						new string[3] { "PrefabName", obs1.PrefabName, obs2.PrefabName },
						new string[3]
						{
							"PathingBlockers Count",
							obs1.PathingBlockers.Count.ToString(),
							obs2.PathingBlockers.Count.ToString()
						}
					});
					break;
				}
				foreach (TileIndex tileIndex in obs1.PathingBlockers)
				{
					if (!obs2.PathingBlockers.Any((TileIndex a) => TileIndex.Compare(a, tileIndex)))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2004, "CObjectObstacle PathingBlocker in " + ScenarioState.GetStateName(isState1: false, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: true, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Prop GUID", obs1.PropGuid, obs2.PropGuid },
							new string[3]
							{
								"ObjectType",
								obs1.ObjectType.ToString(),
								obs2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", obs1.PrefabName, obs2.PrefabName },
							new string[3]
							{
								"PathingBlocker",
								tileIndex.ToString(),
								"Missing"
							}
						});
					}
				}
				foreach (TileIndex tileIndex2 in obs2.PathingBlockers)
				{
					if (!obs1.PathingBlockers.Any((TileIndex a) => TileIndex.Compare(a, tileIndex2)))
					{
						ScenarioState.LogMismatch(list, isMPCompare, 2004, "CObjectObstacle PathingBlocker in " + ScenarioState.GetStateName(isState1: true, isMPCompare) + " is missing a card from " + ScenarioState.GetStateName(isState1: false, isMPCompare) + ".", new List<string[]>
						{
							new string[3] { "Prop GUID", obs1.PropGuid, obs2.PropGuid },
							new string[3]
							{
								"ObjectType",
								obs1.ObjectType.ToString(),
								obs2.ObjectType.ToString()
							},
							new string[3] { "PrefabName", obs1.PrefabName, obs2.PrefabName },
							new string[3]
							{
								"PathingBlocker",
								"Missing",
								tileIndex2.ToString()
							}
						});
					}
				}
				break;
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(2099, "Exception during CObjectObstacle compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
