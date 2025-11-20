using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsBase : ISerializable
{
	public string AdventureGuid { get; private set; }

	public string ScenarioGuid { get; private set; }

	public string QuestType { get; private set; }

	public int Round { get; private set; }

	public CPlayerStatsBase()
	{
	}

	public CPlayerStatsBase(CPlayerStatsBase state, ReferenceDictionary references)
	{
		AdventureGuid = state.AdventureGuid;
		ScenarioGuid = state.ScenarioGuid;
		QuestType = state.QuestType;
		Round = state.Round;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("AdventureGuid", AdventureGuid);
		info.AddValue("ScenarioGuid", ScenarioGuid);
		info.AddValue("QuestType", QuestType);
		info.AddValue("Round", Round);
	}

	public CPlayerStatsBase(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "AdventureGuid":
					AdventureGuid = info.GetString("AdventureGuid");
					break;
				case "ScenarioGuid":
					ScenarioGuid = info.GetString("ScenarioGuid");
					break;
				case "QuestType":
					QuestType = info.GetString("QuestType");
					break;
				case "Round":
					Round = info.GetInt32("Round");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CPlayerStatsBase entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsBase(string advGuid, string sceGuid, string questType, int round)
	{
		AdventureGuid = advGuid;
		ScenarioGuid = sceGuid;
		QuestType = questType;
		Round = round;
	}
}
