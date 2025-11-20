using System;
using System.Collections.Generic;
using System.IO;
using Unity.Jobs;
using UnityEngine.Formats.Alembic.Sdk;

namespace UnityEngine.Formats.Alembic.Importer;

internal sealed class AlembicStream : IDisposable
{
	public struct SafeContext
	{
		private struct UpdateSamplesJob : IJob
		{
			public aiContext context;

			public double time;

			public void Execute()
			{
				context.UpdateSamples(time);
			}
		}

		private aiContext context;

		public JobHandle updateJobHandle { get; private set; }

		public bool isValid => context;

		public aiObject root => context.topObject;

		public int timeSamplingCount => context.timeSamplingCount;

		public SafeContext(aiContext c)
		{
			context = c;
			updateJobHandle = default(JobHandle);
		}

		public aiTimeSampling GetTimeSampling(int i)
		{
			return context.GetTimeSampling(i);
		}

		public bool IsHDF5()
		{
			return context.IsHDF5();
		}

		public string GetApplication()
		{
			return context.GetApplication();
		}

		public void GetTimeRange(out double begin, out double end)
		{
			context.GetTimeRange(out begin, out end);
		}

		public void SetConfig(ref aiConfig conf)
		{
			updateJobHandle.Complete();
			context.SetConfig(ref conf);
		}

		public bool Load(string path)
		{
			updateJobHandle.Complete();
			return context.Load(path);
		}

		public void Destroy()
		{
			updateJobHandle.Complete();
			context.Destroy();
		}

		public void ScheduleUpdateSamples(double time)
		{
			UpdateSamplesJob jobData = new UpdateSamplesJob
			{
				context = context,
				time = time
			};
			updateJobHandle = jobData.Schedule();
		}
	}

	private class ImportContext
	{
		public AlembicTreeNode alembicTreeNode;

		public aiSampleSelector ss;

		public bool createMissingNodes;
	}

	private static List<AlembicStream> s_streams = new List<AlembicStream>();

	private IStreamDescriptor m_streamDesc;

	private AlembicTreeNode m_abcTreeRoot;

	private aiConfig m_config;

	private SafeContext m_context;

	private double m_time;

	private bool m_loaded;

	private bool m_streamInterupted;

	private ImportContext m_importContext;

	internal IStreamDescriptor streamDescriptor => m_streamDesc;

	public AlembicTreeNode abcTreeRoot => m_abcTreeRoot;

	internal SafeContext abcContext => m_context;

	public bool abcIsValid => m_context.isValid;

	internal aiConfig config => m_config;

	public static void DisconnectStreamsWithPath(string path)
	{
		aiContext.DestroyByPath(path);
		s_streams.ForEach(delegate(AlembicStream s)
		{
			if (s.m_streamDesc.PathToAbc == path)
			{
				s.m_streamInterupted = true;
				s.m_context = new SafeContext(default(aiContext));
				s.m_loaded = false;
			}
		});
	}

	public static void RemapStreamsWithPath(string oldPath, string newPath)
	{
		s_streams.ForEach(delegate(AlembicStream s)
		{
			if (s.m_streamDesc.PathToAbc == oldPath)
			{
				s.m_streamInterupted = true;
				s.m_streamDesc.PathToAbc = newPath;
			}
		});
	}

	public static void ReconnectStreamsWithPath(string path)
	{
		s_streams.ForEach(delegate(AlembicStream s)
		{
			if (s.m_streamDesc.PathToAbc == path)
			{
				s.m_streamInterupted = false;
			}
		});
	}

	internal bool IsHDF5()
	{
		return m_context.IsHDF5();
	}

	public void SetVertexMotionScale(float value)
	{
		m_config.vertexMotionScale = value;
	}

	public void GetTimeRange(out double begin, out double end)
	{
		m_context.GetTimeRange(out begin, out end);
	}

	internal AlembicStream(GameObject rootGo, IStreamDescriptor streamDesc)
	{
		m_config.SetDefaults();
		m_abcTreeRoot = new AlembicTreeNode
		{
			stream = this,
			gameObject = rootGo
		};
		m_streamDesc = streamDesc;
	}

	private void AbcBeforeUpdateSamples(AlembicTreeNode node)
	{
		if (node.abcObject != null && node.gameObject != null)
		{
			node.abcObject.AbcPrepareSample();
		}
		foreach (AlembicTreeNode child in node.Children)
		{
			AbcBeforeUpdateSamples(child);
		}
	}

	private void AbcBeginSyncData(AlembicTreeNode node)
	{
		if (node != null && node.abcObject != null && node.gameObject != null)
		{
			node.abcObject.AbcSyncDataBegin();
		}
		foreach (AlembicTreeNode child in node.Children)
		{
			AbcBeginSyncData(child);
		}
	}

	private void AbcEndSyncData(AlembicTreeNode node)
	{
		if (node.abcObject != null && node.gameObject != null)
		{
			node.abcObject.AbcSyncDataEnd();
		}
		foreach (AlembicTreeNode child in node.Children)
		{
			AbcEndSyncData(child);
		}
	}

	public bool AbcUpdateBegin(double time)
	{
		if (m_streamInterupted)
		{
			return true;
		}
		if (!abcIsValid || !m_loaded)
		{
			return false;
		}
		m_time = time;
		m_context.SetConfig(ref m_config);
		AbcBeforeUpdateSamples(m_abcTreeRoot);
		m_context.ScheduleUpdateSamples(time);
		return true;
	}

	public void AbcUpdateEnd()
	{
		if (!m_streamInterupted)
		{
			m_context.updateJobHandle.Complete();
			AbcBeginSyncData(m_abcTreeRoot);
			AbcEndSyncData(m_abcTreeRoot);
		}
	}

	public void ClearMotionVectors()
	{
		ClearMotionVectors(m_abcTreeRoot);
	}

	private void ClearMotionVectors(AlembicTreeNode node)
	{
		if (node.abcObject is AlembicMesh alembicMesh)
		{
			alembicMesh.ClearMotionVectors();
		}
		foreach (AlembicTreeNode child in node.Children)
		{
			ClearMotionVectors(child);
		}
	}

	public bool AbcLoad(bool createMissingNodes, bool serializeMesh)
	{
		m_time = 0.0;
		m_context = new SafeContext(aiContext.Create(m_abcTreeRoot.gameObject.GetInstanceID()));
		AlembicStreamSettings settings = m_streamDesc.Settings;
		m_config.swapHandedness = settings.SwapHandedness;
		m_config.flipFaces = settings.FlipFaces;
		m_config.aspectRatio = GetAspectRatio(settings.CameraAspectRatio);
		m_config.scaleFactor = settings.ScaleFactor;
		m_config.normalsMode = settings.Normals;
		m_config.tangentsMode = settings.Tangents;
		m_config.interpolateSamples = settings.InterpolateSamples;
		m_config.importPointPolygon = settings.ImportPointPolygon;
		m_config.importLinePolygon = settings.ImportLinePolygon;
		m_config.importTrianglePolygon = settings.ImportTrianglePolygon;
		m_context.SetConfig(ref m_config);
		m_loaded = m_context.Load(m_streamDesc.PathToAbc);
		if (m_loaded)
		{
			UpdateAbcTree(m_context.root, m_abcTreeRoot, m_time, createMissingNodes, serializeMesh);
			s_streams.Add(this);
		}
		else if (!File.Exists(m_streamDesc.PathToAbc))
		{
			Debug.LogError("File does not exist: " + m_streamDesc.PathToAbc);
		}
		else if (m_context.IsHDF5())
		{
			Debug.LogError("Failed to load HDF5 alembic. Please convert to Ogawa: " + m_streamDesc.PathToAbc);
		}
		else
		{
			Debug.LogError("File is in unknown format: " + m_streamDesc.PathToAbc);
		}
		return m_loaded;
	}

	public void Dispose()
	{
		s_streams.Remove(this);
		if (m_abcTreeRoot != null)
		{
			m_abcTreeRoot.Dispose();
			m_abcTreeRoot = null;
		}
		if (abcIsValid)
		{
			m_context.Destroy();
		}
	}

	private void UpdateAbcTree(aiObject top, AlembicTreeNode node, double time, bool createMissingNodes, bool serializeMesh)
	{
		if (!top)
		{
			return;
		}
		m_importContext = new ImportContext
		{
			alembicTreeNode = node,
			ss = NativeMethods.aiTimeToSampleSelector(time),
			createMissingNodes = createMissingNodes
		};
		top.EachChild(ImportCallback);
		if (!serializeMesh)
		{
			MeshFilter[] componentsInChildren = node.gameObject.GetComponentsInChildren<MeshFilter>();
			foreach (MeshFilter meshFilter in componentsInChildren)
			{
				if (meshFilter.sharedMesh != null)
				{
					meshFilter.sharedMesh.hideFlags |= HideFlags.DontSave;
				}
			}
		}
		m_importContext = null;
	}

	private void ImportCallback(aiObject obj)
	{
		ImportContext importContext = m_importContext;
		AlembicTreeNode alembicTreeNode = importContext.alembicTreeNode;
		AlembicTreeNode alembicTreeNode2 = null;
		aiSchema aiSchema = obj.AsXform();
		if (!aiSchema)
		{
			aiSchema = obj.AsPolyMesh();
		}
		if (!aiSchema)
		{
			aiSchema = obj.AsSubD();
		}
		if (!aiSchema)
		{
			aiSchema = obj.AsCamera();
		}
		if (!aiSchema)
		{
			aiSchema = obj.AsPoints();
		}
		if (!aiSchema)
		{
			aiSchema = obj.AsCurves();
		}
		if ((bool)aiSchema)
		{
			string name = obj.name;
			GameObject gameObject = null;
			Transform transform = ((alembicTreeNode.gameObject == null) ? null : alembicTreeNode.gameObject.transform.Find(name));
			if (transform == null)
			{
				if (!importContext.createMissingNodes)
				{
					obj.SetEnabled(value: false);
					return;
				}
				obj.SetEnabled(value: true);
				gameObject = RuntimeUtils.CreateGameObjectWithUndo("Create AlembicObject");
				gameObject.name = name;
				gameObject.GetComponent<Transform>().SetParent(alembicTreeNode.gameObject.transform, worldPositionStays: false);
			}
			else
			{
				gameObject = transform.gameObject;
			}
			alembicTreeNode2 = new AlembicTreeNode
			{
				stream = this,
				gameObject = gameObject
			};
			alembicTreeNode.Children.Add(alembicTreeNode2);
			AlembicElement alembicElement = null;
			if ((bool)obj.AsXform() && m_streamDesc.Settings.ImportXform)
			{
				alembicElement = alembicTreeNode2.GetOrAddAlembicObj<AlembicXform>();
			}
			else if ((bool)obj.AsCamera() && m_streamDesc.Settings.ImportCameras)
			{
				alembicElement = alembicTreeNode2.GetOrAddAlembicObj<AlembicCamera>();
			}
			else if ((bool)obj.AsPolyMesh() && m_streamDesc.Settings.ImportMeshes)
			{
				alembicElement = alembicTreeNode2.GetOrAddAlembicObj<AlembicMesh>();
			}
			else if ((bool)obj.AsSubD() && m_streamDesc.Settings.ImportMeshes)
			{
				alembicElement = alembicTreeNode2.GetOrAddAlembicObj<AlembicSubD>();
			}
			else if ((bool)obj.AsPoints() && m_streamDesc.Settings.ImportPoints)
			{
				alembicElement = alembicTreeNode2.GetOrAddAlembicObj<AlembicPoints>();
			}
			else if ((bool)obj.AsCurves() && m_streamDesc.Settings.ImportCurves)
			{
				AlembicCurvesElement orAddAlembicObj = alembicTreeNode2.GetOrAddAlembicObj<AlembicCurvesElement>();
				orAddAlembicObj.CreateRenderingComponent = m_streamDesc.Settings.CreateCurveRenderers;
				alembicElement = orAddAlembicObj;
			}
			if (alembicElement != null)
			{
				alembicElement.AbcSetup(obj, aiSchema);
				alembicElement.AbcPrepareSample();
				aiSchema.UpdateSample(ref importContext.ss);
				alembicElement.AbcSyncDataBegin();
				alembicElement.AbcSyncDataEnd();
			}
		}
		else
		{
			obj.SetEnabled(value: false);
		}
		importContext.alembicTreeNode = alembicTreeNode2;
		obj.EachChild(ImportCallback);
		importContext.alembicTreeNode = alembicTreeNode;
	}

	internal static float GetAspectRatio(AspectRatioMode mode)
	{
		if (mode == AspectRatioMode.CameraAperture)
		{
			return 0f;
		}
		return (float)Screen.width / (float)Screen.height;
	}
}
