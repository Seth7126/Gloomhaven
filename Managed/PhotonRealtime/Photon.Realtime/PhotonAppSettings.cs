using System;
using UnityEngine;

namespace Photon.Realtime;

[Serializable]
[HelpURL("https://doc.photonengine.com/en-us/pun/v2/getting-started/initial-setup")]
public class PhotonAppSettings : ScriptableObject
{
	[Tooltip("Core Photon Server/Cloud settings.")]
	public AppSettings AppSettings;

	private static PhotonAppSettings instance;

	public static PhotonAppSettings Instance
	{
		get
		{
			if (instance == null)
			{
				LoadOrCreateSettings();
			}
			return instance;
		}
		private set
		{
			instance = value;
		}
	}

	public static void LoadOrCreateSettings()
	{
		if (instance != null)
		{
			Debug.LogWarning("Instance is not null. Will not LoadOrCreateSettings().");
			return;
		}
		instance = (PhotonAppSettings)Resources.Load(typeof(PhotonAppSettings).Name, typeof(PhotonAppSettings));
		if (!(instance != null) && instance == null)
		{
			instance = (PhotonAppSettings)ScriptableObject.CreateInstance(typeof(PhotonAppSettings));
			if (instance == null)
			{
				Debug.LogError("Failed to create ServerSettings. PUN is unable to run this way. If you deleted it from the project, reload the Editor.");
			}
		}
	}
}
