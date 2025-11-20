using System;
using System.IO;

namespace UnityEngine.Formats.Alembic.Importer;

[Serializable]
internal class EmbeddedAlembicStreamDescriptor : IStreamDescriptor
{
	[SerializeField]
	private string pathToAbc = string.Empty;

	[SerializeField]
	private AlembicStreamSettings settings = new AlembicStreamSettings();

	[SerializeField]
	private float mediaStartTime;

	[SerializeField]
	private float mediaEndTime;

	public string PathToAbc
	{
		get
		{
			if (!Path.IsPathRooted(pathToAbc) && !string.IsNullOrEmpty(pathToAbc))
			{
				return Path.Combine(Application.streamingAssetsPath, pathToAbc);
			}
			return pathToAbc;
		}
		set
		{
			pathToAbc = value;
		}
	}

	public AlembicStreamSettings Settings
	{
		get
		{
			return settings;
		}
		set
		{
			settings = value;
		}
	}

	public float MediaStartTime
	{
		get
		{
			return mediaStartTime;
		}
		set
		{
			mediaStartTime = value;
		}
	}

	public float MediaEndTime
	{
		get
		{
			return mediaEndTime;
		}
		set
		{
			mediaEndTime = value;
		}
	}

	public float MediaDuration => MediaEndTime - MediaStartTime;

	public IStreamDescriptor Clone()
	{
		AlembicStreamSettings.AlembicStreamSettingsCopier alembicStreamSettingsCopier = ScriptableObject.CreateInstance<AlembicStreamSettings.AlembicStreamSettingsCopier>();
		alembicStreamSettingsCopier.abcSettings = Settings;
		return new EmbeddedAlembicStreamDescriptor
		{
			pathToAbc = PathToAbc,
			settings = Object.Instantiate(alembicStreamSettingsCopier).abcSettings,
			mediaStartTime = MediaStartTime,
			mediaEndTime = MediaEndTime
		};
	}
}
