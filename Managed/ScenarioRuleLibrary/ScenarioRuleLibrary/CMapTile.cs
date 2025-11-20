using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary;
using SharedLibrary.Client;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CMapTile : ISerializable
{
	public string TileGuid { get; private set; }

	public TileIndex ArrayIndex { get; private set; }

	public EFlags Flags { get; private set; }

	public CVector3 Position { get; private set; }

	public CMapTile()
	{
	}

	public CMapTile(CMapTile state, ReferenceDictionary references)
	{
		TileGuid = state.TileGuid;
		ArrayIndex = references.Get(state.ArrayIndex);
		if (ArrayIndex == null && state.ArrayIndex != null)
		{
			ArrayIndex = new TileIndex(state.ArrayIndex, references);
			references.Add(state.ArrayIndex, ArrayIndex);
		}
		Flags = state.Flags;
		Position = references.Get(state.Position);
		if (Position == null && state.Position != null)
		{
			Position = new CVector3(state.Position, references);
			references.Add(state.Position, Position);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("TileGuid", TileGuid);
		info.AddValue("ArrayIndex", ArrayIndex);
		info.AddValue("Flags", Flags);
		info.AddValue("Position", Position);
	}

	public CMapTile(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "TileGuid":
					TileGuid = info.GetString("TileGuid");
					break;
				case "ArrayIndex":
					ArrayIndex = (TileIndex)info.GetValue("ArrayIndex", typeof(TileIndex));
					break;
				case "Flags":
					Flags = (EFlags)info.GetValue("Flags", typeof(EFlags));
					break;
				case "Position":
					Position = (CVector3)info.GetValue("Position", typeof(CVector3));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CMapTile entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CMapTile(bool edgeTile, TileIndex arrayIndex, CVector3 position, SharedLibrary.Random rng)
	{
		TileGuid = SharedClient.GetGUIDBasedOnRNG(rng).ToString();
		ArrayIndex = arrayIndex;
		Position = position;
		Flags |= (EFlags)(edgeTile ? 2 : 0);
	}

	public bool FlagsSet(EFlags flags)
	{
		return (Flags & flags) != 0;
	}

	public static List<Tuple<int, string>> Compare(CMapTile mapTile1, CMapTile mapTile2, string parentMapGuid, EMapType parentMapType, string parentMapRoomName, bool isMPCompare)
	{
		List<Tuple<int, string>> list = new List<Tuple<int, string>>();
		try
		{
			if (!TileIndex.Compare(mapTile1.ArrayIndex, mapTile2.ArrayIndex))
			{
				ScenarioState.LogMismatch(list, isMPCompare, 301, "Map Tile ArrayIndex does not match.", new List<string[]>
				{
					new string[3] { "Map GUID", parentMapGuid, parentMapGuid },
					new string[3]
					{
						"Map Type",
						parentMapType.ToString(),
						parentMapType.ToString()
					},
					new string[3] { "RoomName", parentMapRoomName, parentMapRoomName },
					new string[3]
					{
						"ArrayIndex",
						mapTile1.ArrayIndex.ToString(),
						mapTile2.ArrayIndex.ToString()
					}
				});
			}
			if (mapTile1.Flags != mapTile2.Flags)
			{
				ScenarioState.LogMismatch(list, isMPCompare, 302, "Map Tile Flags do not match.", new List<string[]>
				{
					new string[3] { "Map GUID", parentMapGuid, parentMapGuid },
					new string[3]
					{
						"Map Type",
						parentMapType.ToString(),
						parentMapType.ToString()
					},
					new string[3] { "RoomName", parentMapRoomName, parentMapRoomName },
					new string[3]
					{
						"Flags",
						mapTile1.Flags.ToString(),
						mapTile2.Flags.ToString()
					}
				});
			}
			if (!CVector3.Compare(mapTile1.Position, mapTile2.Position))
			{
				ScenarioState.LogMismatch(list, isMPCompare, 303, "Map Tile Position does not match.", new List<string[]>
				{
					new string[3] { "Map GUID", parentMapGuid, parentMapGuid },
					new string[3]
					{
						"Map Type",
						parentMapType.ToString(),
						parentMapType.ToString()
					},
					new string[3] { "RoomName", parentMapRoomName, parentMapRoomName },
					new string[3]
					{
						"Position",
						mapTile1.Position.ToString(),
						mapTile2.Position.ToString()
					}
				});
			}
		}
		catch (Exception ex)
		{
			list.Add(new Tuple<int, string>(399, "Exception during Map Tile compare.\n" + ex.Message + "\n" + ex.StackTrace));
		}
		return list;
	}
}
