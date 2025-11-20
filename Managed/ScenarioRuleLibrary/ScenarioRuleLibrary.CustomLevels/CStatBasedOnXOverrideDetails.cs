using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary.CustomLevels;

[Serializable]
public class CStatBasedOnXOverrideDetails : ISerializable
{
	public string AssociatedClassID;

	public AbilityData.StatIsBasedOnXData OverrideData;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("AssociatedClassID", AssociatedClassID);
		info.AddValue("OverrideData", OverrideData);
	}

	public CStatBasedOnXOverrideDetails(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "AssociatedClassID"))
				{
					if (name == "OverrideData")
					{
						OverrideData = (AbilityData.StatIsBasedOnXData)info.GetValue("OverrideData", typeof(AbilityData.StatIsBasedOnXData));
					}
				}
				else
				{
					AssociatedClassID = info.GetString("AssociatedClassID");
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CStatBasedOnXOverrideDetails entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CStatBasedOnXOverrideDetails(string classId)
	{
		AssociatedClassID = classId;
		OverrideData = new AbilityData.StatIsBasedOnXData();
	}

	public CStatBasedOnXOverrideDetails()
	{
	}

	public CStatBasedOnXOverrideDetails(CStatBasedOnXOverrideDetails state, ReferenceDictionary references)
	{
		AssociatedClassID = state.AssociatedClassID;
		OverrideData = references.Get(state.OverrideData);
		if (OverrideData == null && state.OverrideData != null)
		{
			OverrideData = new AbilityData.StatIsBasedOnXData(state.OverrideData, references);
			references.Add(state.OverrideData, OverrideData);
		}
	}
}
