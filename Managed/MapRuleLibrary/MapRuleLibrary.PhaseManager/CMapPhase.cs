using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace MapRuleLibrary.PhaseManager;

[Serializable]
public class CMapPhase : ISerializable
{
	public EMapPhaseType Type { get; private set; }

	public CMapPhase()
	{
	}

	public CMapPhase(CMapPhase state, ReferenceDictionary references)
	{
		Type = state.Type;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Type", Type);
	}

	public CMapPhase(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				if (current.Name == "Type")
				{
					Type = (EMapPhaseType)info.GetValue("Type", typeof(EMapPhaseType));
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CMapPhase entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CMapPhase(EMapPhaseType type)
	{
		Type = type;
	}

	public void EndPhase()
	{
		OnEndPhase();
	}

	protected virtual void OnEndPhase()
	{
	}

	public void NextStep()
	{
		OnNextStep();
	}

	protected virtual void OnNextStep()
	{
	}

	public void StepComplete()
	{
		OnStepComplete();
	}

	protected virtual void OnStepComplete()
	{
	}
}
