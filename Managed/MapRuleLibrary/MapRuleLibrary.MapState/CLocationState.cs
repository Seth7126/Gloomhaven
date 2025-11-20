using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using MapRuleLibrary.YML.Locations;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace MapRuleLibrary.MapState;

[Serializable]
public class CLocationState : ISerializable
{
	public string ID { get; private set; }

	public ELocationState LocationState { get; private set; }

	public string Mesh { get; protected set; }

	public CUnlockConditionState UnlockConditionState { get; protected set; }

	public CLocation Location
	{
		get
		{
			if (!(this is CHeadquartersState cHeadquartersState))
			{
				if (!(this is CVillageState cVillageState))
				{
					if (!(this is CMapScenarioState cMapScenarioState))
					{
						if (this is CStoreLocationState cStoreLocationState)
						{
							return cStoreLocationState.StoreLocation;
						}
						return null;
					}
					return cMapScenarioState.MapScenario;
				}
				return cVillageState.Village;
			}
			return cHeadquartersState.Headquarters;
		}
	}

	public CLocationState()
	{
	}

	public CLocationState(CLocationState state, ReferenceDictionary references)
	{
		ID = state.ID;
		LocationState = state.LocationState;
		Mesh = state.Mesh;
		UnlockConditionState = references.Get(state.UnlockConditionState);
		if (UnlockConditionState == null && state.UnlockConditionState != null)
		{
			UnlockConditionState = new CUnlockConditionState(state.UnlockConditionState, references);
			references.Add(state.UnlockConditionState, UnlockConditionState);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Name", ID);
		info.AddValue("LocationState", LocationState);
		info.AddValue("Mesh", Mesh);
		info.AddValue("UnlockConditionState", UnlockConditionState);
	}

	public CLocationState(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "Name":
					ID = info.GetString("Name");
					break;
				case "LocationState":
					LocationState = (ELocationState)info.GetValue("LocationState", typeof(ELocationState));
					break;
				case "Mesh":
					Mesh = info.GetString("Mesh");
					break;
				case "UnlockConditionState":
					UnlockConditionState = (CUnlockConditionState)info.GetValue("UnlockConditionState", typeof(CUnlockConditionState));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CLocationState entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CLocationState(CLocation location)
	{
		if (location != null)
		{
			ID = location.ID;
			LocationState = ELocationState.Locked;
			if (location.UnlockCondition != null)
			{
				UnlockConditionState = new CUnlockConditionState(location.UnlockCondition);
			}
		}
	}

	public virtual void UnlockLocation()
	{
		if (LocationState != ELocationState.Completed)
		{
			if (this is CMapScenarioState cMapScenarioState)
			{
				cMapScenarioState.ReRollRoadEvent();
			}
			LocationState = ELocationState.Unlocked;
		}
	}

	public void CompleteLocation()
	{
		LocationState = ELocationState.Completed;
	}
}
