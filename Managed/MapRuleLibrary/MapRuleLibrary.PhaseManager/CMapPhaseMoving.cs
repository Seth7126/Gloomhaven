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
public class CMapPhaseMoving : CMapPhase, ISerializable
{
	public string StartLocationName { get; private set; }

	public string EndLocationName { get; private set; }

	public EMovePhase MovePhase { get; private set; }

	public CLocationState StartLocation => AdventureState.MapState.AllLocations.SingleOrDefault((CLocationState s) => s.ID == StartLocationName);

	public CLocationState EndLocation => AdventureState.MapState.AllLocations.SingleOrDefault((CLocationState s) => s.ID == EndLocationName);

	public CMapPhaseMoving()
	{
	}

	public CMapPhaseMoving(CMapPhaseMoving state, ReferenceDictionary references)
		: base(state, references)
	{
		StartLocationName = state.StartLocationName;
		EndLocationName = state.EndLocationName;
		MovePhase = state.MovePhase;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("StartLocationName", StartLocationName);
		info.AddValue("EndLocationName", EndLocationName);
		info.AddValue("MovePhase", MovePhase);
	}

	public CMapPhaseMoving(SerializationInfo info, StreamingContext context)
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
				case "StartLocationName":
					StartLocationName = info.GetString("StartLocationName");
					break;
				case "EndLocationName":
					EndLocationName = info.GetString("EndLocationName");
					break;
				case "MovePhase":
					MovePhase = (EMovePhase)info.GetValue("MovePhase", typeof(EMovePhase));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CMapPhaseMoving entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CMapPhaseMoving(string startLocationName, string endLocationName)
		: base(EMapPhaseType.Moving)
	{
		StartLocationName = startLocationName;
		EndLocationName = endLocationName;
		MovePhase = EMovePhase.BeforeRoadEvent;
	}

	protected override void OnNextStep()
	{
		CStartMoving_MapClientMessage message = new CStartMoving_MapClientMessage(StartLocation, EndLocation);
		MapRuleLibraryClient.Instance.MessageHandler(message);
	}
}
