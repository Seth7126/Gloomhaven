using UnityEngine.Formats.Alembic.Sdk;

namespace UnityEngine.Formats.Alembic.Importer;

[ExecuteInEditMode]
[DisallowMultipleComponent]
public class AlembicStreamPlayer : MonoBehaviour, ISerializationCallbackReceiver
{
	internal enum AlembicStreamSource
	{
		Internal,
		External
	}

	[SerializeField]
	private AlembicStreamSource streamSource = AlembicStreamSource.External;

	[SerializeField]
	private AlembicStreamDescriptor streamDescriptor;

	[SerializeField]
	private EmbeddedAlembicStreamDescriptor embeddedStreamDescriptor = new EmbeddedAlembicStreamDescriptor();

	[SerializeField]
	private float startTime = float.MinValue;

	[SerializeField]
	private float endTime = float.MaxValue;

	[SerializeField]
	private float currentTime;

	[SerializeField]
	private float vertexMotionScale = 1f;

	private float lastUpdateTime;

	private bool forceUpdate;

	private bool updateStarted;

	internal AlembicStreamSource StreamSource
	{
		get
		{
			return streamSource;
		}
		set
		{
			streamSource = value;
		}
	}

	internal AlembicStream abcStream { get; set; }

	internal IStreamDescriptor StreamDescriptor
	{
		get
		{
			if (StreamSource != AlembicStreamSource.External)
			{
				return streamDescriptor;
			}
			return embeddedStreamDescriptor;
		}
		set
		{
			if (StreamSource == AlembicStreamSource.External)
			{
				embeddedStreamDescriptor = (EmbeddedAlembicStreamDescriptor)value;
			}
			else
			{
				streamDescriptor = (AlembicStreamDescriptor)value;
			}
		}
	}

	public float StartTime
	{
		get
		{
			return startTime;
		}
		set
		{
			startTime = value;
			if (StreamDescriptor != null)
			{
				startTime = Mathf.Clamp(startTime, StreamDescriptor.MediaStartTime, StreamDescriptor.MediaEndTime);
			}
		}
	}

	public float EndTime
	{
		get
		{
			return endTime;
		}
		set
		{
			endTime = value;
			if (StreamDescriptor != null)
			{
				endTime = Mathf.Clamp(endTime, StartTime, StreamDescriptor.MediaEndTime);
			}
		}
	}

	public float CurrentTime
	{
		get
		{
			return currentTime;
		}
		set
		{
			currentTime = Mathf.Clamp(value, 0f, Duration);
		}
	}

	public float Duration => EndTime - StartTime;

	public float VertexMotionScale
	{
		get
		{
			return vertexMotionScale;
		}
		set
		{
			vertexMotionScale = value;
		}
	}

	public float MediaStartTime
	{
		get
		{
			if (StreamDescriptor == null)
			{
				return 0f;
			}
			return StreamDescriptor.MediaStartTime;
		}
	}

	public float MediaEndTime
	{
		get
		{
			if (StreamDescriptor == null)
			{
				return 0f;
			}
			return StreamDescriptor.MediaEndTime;
		}
	}

	public float MediaDuration => MediaEndTime - MediaStartTime;

	public string PathToAbc
	{
		get
		{
			if (StreamDescriptor == null)
			{
				return "";
			}
			return StreamDescriptor.PathToAbc;
		}
	}

	public AlembicStreamSettings Settings
	{
		get
		{
			if (StreamDescriptor == null)
			{
				return null;
			}
			return StreamDescriptor.Settings;
		}
		set
		{
			if (StreamDescriptor == null)
			{
				StreamDescriptor = ScriptableObject.CreateInstance<AlembicStreamDescriptor>();
			}
			StreamDescriptor.Settings = value;
			ReloadStream();
		}
	}

	public void UpdateImmediately(float time)
	{
		CurrentTime = time;
		Update();
		LateUpdate();
	}

	public bool LoadFromFile(string newPath)
	{
		AlembicStreamAnalytics.SendAnalytics();
		if (StreamDescriptor == null)
		{
			StreamDescriptor = ScriptableObject.CreateInstance<AlembicStreamDescriptor>();
		}
		StreamDescriptor.PathToAbc = newPath;
		return InitializeAfterLoad();
	}

	public bool ReloadStream(bool createMissingNodes = false)
	{
		if (abcStream != null)
		{
			abcStream?.Dispose();
			return LoadStream(createMissingNodes);
		}
		return true;
	}

	public void RemoveObsoleteGameObjects()
	{
		ReloadStream(createMissingNodes: true);
		RemoveObsoleteGameObject(base.gameObject);
	}

	private void RemoveObsoleteGameObject(GameObject root)
	{
		for (int num = root.transform.childCount - 1; num >= 0; num--)
		{
			RemoveObsoleteGameObject(root.transform.GetChild(num).gameObject);
		}
		if (abcStream.abcTreeRoot.FindNode(root) == null)
		{
			RuntimeUtils.DestroyUnityObject(root);
		}
	}

	private bool InitializeAfterLoad()
	{
		if (!LoadStream(createMissingNodes: true, serializeMesh: true))
		{
			return false;
		}
		abcStream.GetTimeRange(out var begin, out var end);
		startTime = (float)begin;
		endTime = (float)end;
		StreamDescriptor.MediaStartTime = (float)begin;
		StreamDescriptor.MediaEndTime = (float)end;
		Material defaultMat = AlembicMesh.GetDefaultMaterial();
		base.gameObject.DepthFirstVisitor(delegate(GameObject go)
		{
			MeshFilter component = go.GetComponent<MeshFilter>();
			MeshRenderer component2 = go.GetComponent<MeshRenderer>();
			if (!(component == null) && !(component2 == null))
			{
				Mesh sharedMesh = component.sharedMesh;
				if (!(sharedMesh == null))
				{
					int subMeshCount = sharedMesh.subMeshCount;
					Material[] array = new Material[subMeshCount];
					Material[] sharedMaterials = component2.sharedMaterials;
					for (int i = 0; i < subMeshCount; i++)
					{
						array[i] = ((i < sharedMaterials.Length && sharedMaterials[i] != null) ? sharedMaterials[i] : defaultMat);
					}
					component2.sharedMaterials = array;
				}
			}
		});
		return true;
	}

	internal void CloseStream()
	{
		abcStream?.Dispose();
		abcStream = null;
	}

	private void ClampTime()
	{
		CurrentTime = Mathf.Clamp(CurrentTime, 0f, Duration);
	}

	internal bool LoadStream(bool createMissingNodes, bool serializeMesh = false)
	{
		if (StreamDescriptor == null || string.IsNullOrEmpty(StreamDescriptor.PathToAbc))
		{
			return false;
		}
		CloseStream();
		abcStream = new AlembicStream(base.gameObject, StreamDescriptor);
		bool result = abcStream.AbcLoad(createMissingNodes, serializeMesh);
		forceUpdate = true;
		return result;
	}

	private void Start()
	{
		OnValidate();
	}

	private void OnValidate()
	{
		if (StreamDescriptor != null && abcStream != null)
		{
			if ((double)StreamDescriptor.MediaStartTime == double.MinValue || (double)StreamDescriptor.MediaEndTime == double.MaxValue)
			{
				abcStream.GetTimeRange(out var begin, out var end);
				StreamDescriptor.MediaStartTime = (float)begin;
				StreamDescriptor.MediaEndTime = (float)end;
			}
			StartTime = Mathf.Clamp(StartTime, StreamDescriptor.MediaStartTime, StreamDescriptor.MediaEndTime);
			EndTime = Mathf.Clamp(EndTime, StartTime, StreamDescriptor.MediaEndTime);
			ClampTime();
			forceUpdate = true;
		}
	}

	internal void Update()
	{
		if (abcStream == null || StreamDescriptor == null)
		{
			return;
		}
		ClampTime();
		if (lastUpdateTime != CurrentTime || forceUpdate)
		{
			abcStream.SetVertexMotionScale(VertexMotionScale);
			if (abcStream.AbcUpdateBegin(StartTime + CurrentTime))
			{
				lastUpdateTime = CurrentTime;
				forceUpdate = false;
				updateStarted = true;
			}
			else
			{
				CloseStream();
				LoadStream(createMissingNodes: false);
			}
		}
	}

	private void LateUpdate()
	{
		if (!updateStarted && lastUpdateTime != currentTime)
		{
			Update();
		}
		if (!updateStarted && abcStream != null)
		{
			abcStream.ClearMotionVectors();
			return;
		}
		updateStarted = false;
		if (abcStream != null)
		{
			abcStream.AbcUpdateEnd();
		}
	}

	private void OnEnable()
	{
		if (abcStream == null)
		{
			LoadStream(createMissingNodes: false);
		}
	}

	private void OnDisable()
	{
		CloseStream();
	}

	private void OnApplicationQuit()
	{
		NativeMethods.aiCleanup();
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		if (streamDescriptor != null && streamDescriptor.GetType() == typeof(AlembicStreamDescriptor))
		{
			streamSource = AlembicStreamSource.Internal;
		}
	}
}
