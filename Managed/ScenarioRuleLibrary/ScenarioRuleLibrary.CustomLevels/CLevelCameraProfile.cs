using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary.CustomLevels;

[Serializable]
public class CLevelCameraProfile : ISerializable
{
	public bool ShouldForceCamera { get; set; }

	public float CameraFieldOfView { get; set; }

	public CVector3 FocalPosition { get; set; }

	public CVector3 CameraPosition { get; set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("ShouldForceCamera", ShouldForceCamera);
		info.AddValue("CameraFieldOfView", CameraFieldOfView);
		info.AddValue("FocalPosition", FocalPosition);
		info.AddValue("CameraPosition", CameraPosition);
	}

	public CLevelCameraProfile(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "ShouldForceCamera":
					ShouldForceCamera = info.GetBoolean("ShouldForceCamera");
					break;
				case "CameraFieldOfView":
					CameraFieldOfView = info.GetSingle("CameraFieldOfView");
					break;
				case "FocalPosition":
					FocalPosition = (CVector3)info.GetValue("FocalPosition", typeof(CVector3));
					break;
				case "CameraPosition":
					CameraPosition = (CVector3)info.GetValue("CameraPosition", typeof(CVector3));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CLevelCameraProfile entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CLevelCameraProfile()
	{
		ShouldForceCamera = false;
		CameraFieldOfView = 0f;
		FocalPosition = new CVector3(0f, 0f, 0f);
		CameraPosition = new CVector3(0f, 0f, 0f);
	}

	public CLevelCameraProfile(CLevelCameraProfile state, ReferenceDictionary references)
	{
		ShouldForceCamera = state.ShouldForceCamera;
		CameraFieldOfView = state.CameraFieldOfView;
		FocalPosition = references.Get(state.FocalPosition);
		if (FocalPosition == null && state.FocalPosition != null)
		{
			FocalPosition = new CVector3(state.FocalPosition, references);
			references.Add(state.FocalPosition, FocalPosition);
		}
		CameraPosition = references.Get(state.CameraPosition);
		if (CameraPosition == null && state.CameraPosition != null)
		{
			CameraPosition = new CVector3(state.CameraPosition, references);
			references.Add(state.CameraPosition, CameraPosition);
		}
	}
}
