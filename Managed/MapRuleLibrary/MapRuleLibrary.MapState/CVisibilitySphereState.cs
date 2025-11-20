using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using MapRuleLibrary.Client;
using MapRuleLibrary.Source.YML.VisibilitySpheres;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace MapRuleLibrary.MapState;

[Serializable]
public class CVisibilitySphereState : ISerializable
{
	[Serializable]
	public enum EVisibilitySphereState
	{
		None,
		Locked,
		Unlocked
	}

	public bool IsInitialised { get; private set; }

	public string ID { get; private set; }

	public EVisibilitySphereState VisibilitySphereState { get; private set; }

	public CUnlockConditionState UnlockConditionState { get; private set; }

	public CVisibilitySphere VisibilitySphere { get; private set; }

	public CVisibilitySphereState()
	{
	}

	public CVisibilitySphereState(CVisibilitySphereState state, ReferenceDictionary references)
	{
		IsInitialised = state.IsInitialised;
		ID = state.ID;
		VisibilitySphereState = state.VisibilitySphereState;
		UnlockConditionState = references.Get(state.UnlockConditionState);
		if (UnlockConditionState == null && state.UnlockConditionState != null)
		{
			UnlockConditionState = new CUnlockConditionState(state.UnlockConditionState, references);
			references.Add(state.UnlockConditionState, UnlockConditionState);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("IsInitialised", IsInitialised);
		info.AddValue("ID", ID);
		info.AddValue("VisibilitySphereState", VisibilitySphereState);
		info.AddValue("UnlockConditionState", UnlockConditionState);
	}

	public CVisibilitySphereState(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "IsInitialised":
					IsInitialised = info.GetBoolean("IsInitialised");
					break;
				case "ID":
					ID = info.GetString("ID");
					break;
				case "VisibilitySphereState":
					VisibilitySphereState = (EVisibilitySphereState)info.GetValue("VisibilitySphereState", typeof(EVisibilitySphereState));
					break;
				case "UnlockConditionState":
					UnlockConditionState = (CUnlockConditionState)info.GetValue("UnlockConditionState", typeof(CUnlockConditionState));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CVisibilitySphereState entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		try
		{
			VisibilitySphere = MapRuleLibraryClient.MRLYML.VisibilitySpheres.SingleOrDefault((CVisibilitySphere s) => s.ID == ID);
			if (VisibilitySphere != null)
			{
				if (UnlockConditionState == null)
				{
					UnlockConditionState = new CUnlockConditionState(VisibilitySphere.UnlockCondition);
				}
				UnlockConditionState.CacheUnlockCondition(VisibilitySphere.UnlockCondition);
			}
		}
		catch (Exception ex)
		{
			DLLDebug.LogError("Exception in OnDeserialized function of CVisibilitySphereState\n" + ex.Message + "\n" + ex.StackTrace);
			throw ex;
		}
	}

	public CVisibilitySphereState(CVisibilitySphere visibilitySphere)
	{
		VisibilitySphere = visibilitySphere;
		ID = visibilitySphere.ID;
		VisibilitySphereState = EVisibilitySphereState.Locked;
		UnlockConditionState = new CUnlockConditionState(visibilitySphere.UnlockCondition);
		IsInitialised = false;
	}

	public void Init()
	{
		if (!IsInitialised)
		{
			IsInitialised = true;
		}
	}

	public void UnlockVisibilitySphere()
	{
		VisibilitySphereState = EVisibilitySphereState.Unlocked;
	}
}
