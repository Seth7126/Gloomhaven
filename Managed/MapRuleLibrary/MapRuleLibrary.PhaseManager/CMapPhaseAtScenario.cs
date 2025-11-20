using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Client;
using MapRuleLibrary.MapState;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace MapRuleLibrary.PhaseManager;

[Serializable]
public class CMapPhaseAtScenario : CMapPhase, ISerializable
{
	public string ScenarioLocationName { get; private set; }

	public CMapScenarioState ScenarioLocation => AdventureState.MapState.AllScenarios.SingleOrDefault((CMapScenarioState s) => s.ID == ScenarioLocationName);

	public CMapPhaseAtScenario()
	{
	}

	public CMapPhaseAtScenario(CMapPhaseAtScenario state, ReferenceDictionary references)
		: base(state, references)
	{
		ScenarioLocationName = state.ScenarioLocationName;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ScenarioLocationName", ScenarioLocationName);
	}

	public CMapPhaseAtScenario(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				if (current.Name == "ScenarioLocationName")
				{
					ScenarioLocationName = info.GetString("ScenarioLocationName");
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CMapPhaseAtScenario entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CMapPhaseAtScenario(string scenarioLocationName)
		: base(EMapPhaseType.AtScenario)
	{
		ScenarioLocationName = scenarioLocationName;
	}

	protected override void OnNextStep()
	{
		foreach (CQuestState allQuest in AdventureState.MapState.AllQuests)
		{
			if (allQuest.ScenarioState == ScenarioLocation)
			{
				if (allQuest.QuestState < CQuestState.EQuestState.Completed)
				{
					allQuest.SetInProgressQuest();
				}
				else
				{
					allQuest.SetInProgressCasualMode();
				}
				break;
			}
		}
		CEnterScenario_MapClientMessage message = new CEnterScenario_MapClientMessage(ScenarioLocation);
		MapRuleLibraryClient.Instance.MessageHandler(message);
	}
}
