using System.IO;

namespace UnityEngine.Formats.Alembic.Importer;

internal class AlembicStreamDescriptor : ScriptableObject, IStreamDescriptor
{
	[SerializeField]
	private string pathToAbc;

	[SerializeField]
	private AlembicStreamSettings settings = new AlembicStreamSettings();

	[SerializeField]
	private float abcStartTime = float.MinValue;

	[SerializeField]
	private float abcEndTime = float.MaxValue;

	public string PathToAbc
	{
		get
		{
			return Path.Combine(Application.streamingAssetsPath, pathToAbc);
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
			return abcStartTime;
		}
		set
		{
			abcStartTime = value;
		}
	}

	public float MediaEndTime
	{
		get
		{
			return abcEndTime;
		}
		set
		{
			abcEndTime = value;
		}
	}

	public float MediaDuration => abcEndTime - abcStartTime;

	public IStreamDescriptor Clone()
	{
		return Object.Instantiate(this);
	}
}
