using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace MapRuleLibrary.PhaseManager;

[Serializable]
public class CMapPhaseInScenario : CMapPhase, ISerializable
{
	public string ScenarioLocationName { get; private set; }

	public CLocationState ScenarioLocation => AdventureState.MapState.AllLocations.SingleOrDefault((CLocationState s) => s.ID == ScenarioLocationName);

	public CMapPhaseInScenario()
	{
	}

	public CMapPhaseInScenario(CMapPhaseInScenario state, ReferenceDictionary references)
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

	public CMapPhaseInScenario(SerializationInfo info, StreamingContext context)
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
				DLLDebug.LogError("Exception while trying to deserialize CMapPhaseInScenario entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CMapPhaseInScenario(string scenarioLocationName)
		: base(EMapPhaseType.InScenario)
	{
		ScenarioLocationName = scenarioLocationName;
	}

	protected override void OnNextStep()
	{
	}
}
