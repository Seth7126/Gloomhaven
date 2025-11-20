using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine.Formats.Alembic.Sdk;

namespace UnityEngine.Formats.Alembic.Util;

[Serializable]
public sealed class AlembicRecorder : IDisposable
{
	private class MeshBuffer : IDisposable
	{
		public bool visibility = true;

		public PinnedList<Vector3> points = new PinnedList<Vector3>();

		public PinnedList<Vector3> normals = new PinnedList<Vector3>();

		public PinnedList<Vector2> uv0 = new PinnedList<Vector2>();

		public PinnedList<Vector2> uv1 = new PinnedList<Vector2>();

		public PinnedList<Color> colors = new PinnedList<Color>();

		public PinnedList<aeSubmeshData> submeshData = new PinnedList<aeSubmeshData>();

		public List<PinnedList<int>> submeshIndices = new List<PinnedList<int>>();

		public void Clear()
		{
			points.Clear();
			normals.Clear();
			uv0.Clear();
			uv1.Clear();
			colors.Clear();
			submeshData.Clear();
			submeshIndices.Clear();
		}

		public void SetupSubmeshes(aeObject abc, Mesh mesh)
		{
			for (int i = 0; i < mesh.subMeshCount; i++)
			{
				abc.AddFaceSet($"submesh[{i}]");
			}
		}

		public void Capture(Mesh mesh, Matrix4x4 world2local, bool captureNormals, bool captureUV0, bool captureUV1, bool captureColors)
		{
			if (mesh == null)
			{
				Clear();
				return;
			}
			if (world2local != Matrix4x4.identity)
			{
				List<Vector3> list = new List<Vector3>();
				mesh.GetVertices(list);
				for (int i = 0; i < list.Count; i++)
				{
					Vector3 point = list[i];
					list[i] = world2local.MultiplyPoint(point);
				}
				points.Assign(list);
			}
			else
			{
				points.LockList(delegate(List<Vector3> ls)
				{
					mesh.GetVertices(ls);
				});
			}
			if (captureNormals)
			{
				if (world2local != Matrix4x4.identity)
				{
					List<Vector3> list2 = new List<Vector3>();
					mesh.GetNormals(list2);
					for (int num = 0; num < list2.Count; num++)
					{
						Vector3 vector = list2[num];
						list2[num] = world2local.MultiplyVector(vector);
					}
					normals.Assign(list2);
				}
				else
				{
					normals.LockList(delegate(List<Vector3> ls)
					{
						mesh.GetNormals(ls);
					});
				}
			}
			else
			{
				normals.Clear();
			}
			if (captureUV0)
			{
				uv0.LockList(delegate(List<Vector2> ls)
				{
					mesh.GetUVs(0, ls);
				});
			}
			else
			{
				uv0.Clear();
			}
			if (captureUV1)
			{
				uv1.LockList(delegate(List<Vector2> ls)
				{
					mesh.GetUVs(1, ls);
				});
			}
			else
			{
				uv1.Clear();
			}
			if (captureColors)
			{
				colors.LockList(delegate(List<Color> ls)
				{
					mesh.GetColors(ls);
				});
			}
			else
			{
				colors.Clear();
			}
			int subMeshCount = mesh.subMeshCount;
			submeshData.Resize(subMeshCount);
			if (submeshIndices.Count > subMeshCount)
			{
				submeshIndices.RemoveRange(subMeshCount, submeshIndices.Count - subMeshCount);
			}
			while (submeshIndices.Count < subMeshCount)
			{
				submeshIndices.Add(new PinnedList<int>());
			}
			int smi = 0;
			while (smi < subMeshCount)
			{
				PinnedList<int> pinnedList = submeshIndices[smi];
				pinnedList.LockList(delegate(List<int> l)
				{
					mesh.GetIndices(l, smi);
				});
				aeSubmeshData value = default(aeSubmeshData);
				switch (mesh.GetTopology(smi))
				{
				case MeshTopology.Triangles:
					value.topology = aeTopology.Triangles;
					break;
				case MeshTopology.Lines:
					value.topology = aeTopology.Lines;
					break;
				case MeshTopology.Quads:
					value.topology = aeTopology.Quads;
					break;
				default:
					value.topology = aeTopology.Points;
					break;
				}
				value.indexes = pinnedList;
				value.indexCount = pinnedList.Count;
				submeshData[smi] = value;
				int num2 = smi + 1;
				smi = num2;
			}
		}

		public void Capture(Mesh mesh, AlembicRecorderSettings settings)
		{
			Capture(mesh, Matrix4x4.identity, settings.MeshNormals, settings.MeshUV0, settings.MeshUV1, settings.MeshColors);
		}

		public void Capture(Mesh mesh, Matrix4x4 world2local, AlembicRecorderSettings settings)
		{
			Capture(mesh, world2local, settings.MeshNormals, settings.MeshUV0, settings.MeshUV1, settings.MeshColors);
		}

		public void WriteSample(aeObject abc)
		{
			aePolyMeshData data = new aePolyMeshData
			{
				visibility = visibility,
				points = points,
				pointCount = points.Count,
				normals = normals,
				uv0 = uv0,
				uv1 = uv1,
				colors = colors,
				submeshes = submeshData,
				submeshCount = submeshData.Count
			};
			abc.WriteSample(ref data);
		}

		public void Dispose()
		{
			if (points != null)
			{
				points.Dispose();
			}
			if (normals != null)
			{
				normals.Dispose();
			}
			if (uv0 != null)
			{
				uv0.Dispose();
			}
			if (uv1 != null)
			{
				uv1.Dispose();
			}
			if (colors != null)
			{
				colors.Dispose();
			}
			if (submeshData != null)
			{
				submeshData.Dispose();
			}
			if (submeshIndices != null)
			{
				submeshIndices.ForEach(delegate(PinnedList<int> i)
				{
					i?.Dispose();
				});
			}
		}
	}

	private class ClothBuffer : IDisposable
	{
		public PinnedList<int> remap = new PinnedList<int>();

		public PinnedList<Vector3> vertices = new PinnedList<Vector3>();

		public PinnedList<Vector3> normals = new PinnedList<Vector3>();

		public Transform rootBone;

		public int numRemappedVertices;

		private void GenerateRemapIndices(Mesh mesh, MeshBuffer mbuf)
		{
			mbuf.Capture(mesh, Matrix4x4.identity, captureNormals: false, captureUV0: false, captureUV1: false, captureColors: false);
			PinnedList<BoneWeight> pinnedList = new PinnedList<BoneWeight>();
			pinnedList.LockList(delegate(List<BoneWeight> l)
			{
				mesh.GetBoneWeights(l);
			});
			remap.Resize(mbuf.points.Count);
			numRemappedVertices = NativeMethods.aeGenerateRemapIndices(remap, mbuf.points, pinnedList, mbuf.points.Count);
		}

		public void Capture(Mesh mesh, Cloth cloth, MeshBuffer mbuf, AlembicRecorderSettings settings)
		{
			if (mesh == null || cloth == null)
			{
				mbuf.Clear();
				return;
			}
			if (remap.Count != mesh.vertexCount)
			{
				GenerateRemapIndices(mesh, mbuf);
			}
			vertices.Assign(cloth.vertices);
			if (numRemappedVertices != vertices.Count)
			{
				Debug.LogWarning("numRemappedVertices != vertices.Count");
				return;
			}
			if (settings.MeshNormals)
			{
				normals.Assign(cloth.normals);
			}
			else
			{
				normals.Clear();
			}
			if (rootBone != null)
			{
				Matrix4x4 mat = Matrix4x4.TRS(rootBone.position, rootBone.rotation, Vector3.one);
				if (rootBone.parent != null)
				{
					mat = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, rootBone.parent.lossyScale).inverse * mat;
				}
				NativeMethods.aeApplyMatrixP(vertices, vertices.Count, ref mat);
				NativeMethods.aeApplyMatrixV(normals, normals.Count, ref mat);
			}
			for (int i = 0; i < remap.Count; i++)
			{
				mbuf.points[i] = vertices[remap[i]];
			}
			if (normals.Count > 0)
			{
				mbuf.normals.ResizeDiscard(remap.Count);
				for (int j = 0; j < remap.Count; j++)
				{
					mbuf.normals[j] = normals[remap[j]];
				}
			}
			if (settings.MeshUV0)
			{
				mbuf.uv0.LockList(delegate(List<Vector2> ls)
				{
					mesh.GetUVs(0, ls);
				});
			}
			else
			{
				mbuf.uv0.Clear();
			}
			if (settings.MeshUV1)
			{
				mbuf.uv1.LockList(delegate(List<Vector2> ls)
				{
					mesh.GetUVs(1, ls);
				});
			}
			else
			{
				mbuf.uv1.Clear();
			}
			if (settings.MeshColors)
			{
				mbuf.colors.LockList(delegate(List<Color> ls)
				{
					mesh.GetColors(ls);
				});
			}
			else
			{
				mbuf.colors.Clear();
			}
		}

		public void Dispose()
		{
			if (remap != null)
			{
				remap.Dispose();
			}
			if (vertices != null)
			{
				vertices.Dispose();
			}
			if (normals != null)
			{
				normals.Dispose();
			}
		}
	}

	private class RootCapturer : ComponentCapturer
	{
		public RootCapturer(AlembicRecorder rec, aeObject abc)
		{
			recorder = rec;
			abcObject = abc;
		}

		public override void Setup(Component c)
		{
		}

		public override void Capture()
		{
		}
	}

	[CaptureTarget(typeof(Transform))]
	private class TransformCapturer : ComponentCapturer
	{
		private Transform m_target;

		private bool m_inherits;

		private bool m_invertForward;

		private bool m_capturePosition = true;

		private bool m_captureRotation = true;

		private bool m_captureScale = true;

		private aeXformData m_data;

		public bool inherits
		{
			set
			{
				m_inherits = value;
			}
		}

		public bool invertForward
		{
			set
			{
				m_invertForward = value;
			}
		}

		public bool capturePosition
		{
			set
			{
				m_capturePosition = value;
			}
		}

		public bool captureRotation
		{
			set
			{
				m_captureRotation = value;
			}
		}

		public bool captureScale
		{
			set
			{
				m_captureScale = value;
			}
		}

		public override void Setup(Component c)
		{
			Transform transform = c as Transform;
			if (parent == null || transform == null)
			{
				if (parent == null)
				{
					Debug.LogWarning("Parent was null");
				}
				else
				{
					Debug.LogWarning("Target was null");
				}
				m_target = null;
			}
			else
			{
				abcObject = parent.abcObject.NewXform(transform.name + " (" + transform.GetInstanceID().ToString("X8") + ")", timeSamplingIndex);
				m_target = transform;
			}
		}

		public override void Capture()
		{
			if (m_target == null)
			{
				m_data.visibility = false;
			}
			else
			{
				Capture(ref m_data);
			}
			abcObject.WriteSample(ref m_data);
		}

		private void Capture(ref aeXformData dst)
		{
			Transform target = m_target;
			dst.visibility = target.gameObject.activeSelf;
			dst.inherits = m_inherits;
			if (m_invertForward)
			{
				target.rotation = Quaternion.LookRotation(-1f * target.forward, target.up);
			}
			if (m_inherits)
			{
				dst.translation = (m_capturePosition ? target.localPosition : Vector3.zero);
				dst.rotation = (m_captureRotation ? target.localRotation : Quaternion.identity);
				dst.scale = (m_captureScale ? target.localScale : Vector3.one);
			}
			else
			{
				dst.translation = (m_capturePosition ? target.position : Vector3.zero);
				dst.rotation = (m_captureRotation ? target.rotation : Quaternion.identity);
				dst.scale = (m_captureScale ? target.lossyScale : Vector3.one);
			}
			if (m_invertForward)
			{
				target.rotation = Quaternion.LookRotation(-1f * target.forward, target.up);
			}
		}
	}

	[CaptureTarget(typeof(Camera))]
	private class CameraCapturer : ComponentCapturer
	{
		private Camera m_target;

		private CameraData m_data;

		public override void Setup(Component c)
		{
			Camera camera = c as Camera;
			abcObject = parent.abcObject.NewCamera(camera.name, timeSamplingIndex);
			m_target = camera;
			if (parent is TransformCapturer transformCapturer)
			{
				transformCapturer.invertForward = true;
			}
		}

		public override void Capture()
		{
			if (m_target == null)
			{
				m_data.visibility = false;
			}
			else
			{
				Capture(ref m_data);
			}
			abcObject.WriteSample(ref m_data);
		}

		private void Capture(ref CameraData dst)
		{
			Camera target = m_target;
			dst.visibility = target.gameObject.activeSelf;
			dst.nearClipPlane = target.nearClipPlane;
			dst.farClipPlane = target.farClipPlane;
			if (target.usePhysicalProperties)
			{
				dst.focalLength = target.focalLength;
				dst.lensShift = target.lensShift;
				dst.sensorSize = target.sensorSize;
			}
			else
			{
				dst.focalLength = (float)((double)(Screen.height / 2) / Math.Tan(MathF.PI / 180f * target.fieldOfView / 2f));
				dst.sensorSize = new Vector2(Screen.width, Screen.height);
			}
		}
	}

	[CaptureTarget(typeof(MeshRenderer))]
	private class MeshCapturer : ComponentCapturer, IDisposable
	{
		private MeshRenderer m_target;

		private MeshBuffer m_mbuf = new MeshBuffer();

		public override void Setup(Component c)
		{
			m_target = c as MeshRenderer;
			MeshFilter component = m_target.GetComponent<MeshFilter>();
			if (component == null)
			{
				m_target = null;
				return;
			}
			Mesh mesh = component.mesh;
			if (!(mesh == null))
			{
				abcObject = parent.abcObject.NewPolyMesh(m_target.name, timeSamplingIndex);
				if (recorder.Settings.MeshSubmeshes)
				{
					m_mbuf.SetupSubmeshes(abcObject, mesh);
				}
			}
		}

		public override void Capture()
		{
			if (m_target == null)
			{
				m_mbuf.visibility = false;
			}
			else
			{
				m_mbuf.visibility = m_target.gameObject.activeSelf;
				Mesh sharedMesh = m_target.GetComponent<MeshFilter>().sharedMesh;
				if (!recorder.m_settings.AssumeNonSkinnedMeshesAreConstant || m_mbuf.points.Capacity == 0)
				{
					m_mbuf.Capture(sharedMesh, recorder.m_settings);
				}
			}
			m_mbuf.WriteSample(abcObject);
		}

		public void Dispose()
		{
			if (m_mbuf != null)
			{
				m_mbuf.Dispose();
			}
		}
	}

	[CaptureTarget(typeof(SkinnedMeshRenderer))]
	private class SkinnedMeshCapturer : ComponentCapturer, IDisposable
	{
		private SkinnedMeshRenderer m_target;

		private Mesh m_meshSrc;

		private Mesh m_meshBake;

		private Cloth m_cloth;

		private MeshBuffer m_mbuf = new MeshBuffer();

		private ClothBuffer m_cbuf;

		public override void Setup(Component c)
		{
			SkinnedMeshRenderer skinnedMeshRenderer = (m_target = c as SkinnedMeshRenderer);
			Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
			if (sharedMesh == null)
			{
				return;
			}
			abcObject = parent.abcObject.NewPolyMesh(skinnedMeshRenderer.name, timeSamplingIndex);
			if (recorder.Settings.MeshSubmeshes)
			{
				m_mbuf.SetupSubmeshes(abcObject, sharedMesh);
			}
			m_meshSrc = skinnedMeshRenderer.sharedMesh;
			m_cloth = m_target.GetComponent<Cloth>();
			if (m_cloth != null)
			{
				m_cbuf = new ClothBuffer();
				m_cbuf.rootBone = ((m_target.rootBone != null) ? m_target.rootBone : m_target.GetComponent<Transform>());
				if (parent is TransformCapturer transformCapturer)
				{
					transformCapturer.capturePosition = false;
					transformCapturer.captureRotation = false;
					transformCapturer.captureScale = false;
				}
			}
		}

		public override void Capture()
		{
			if (m_target == null)
			{
				m_mbuf.visibility = false;
			}
			else
			{
				m_mbuf.visibility = m_target.gameObject.activeSelf;
				if (m_cloth != null)
				{
					m_cbuf.Capture(m_meshSrc, m_cloth, m_mbuf, recorder.m_settings);
				}
				else
				{
					if (m_meshBake == null)
					{
						m_meshBake = new Mesh
						{
							name = m_target.name
						};
					}
					m_meshBake.Clear();
					m_target.BakeMesh(m_meshBake, useScale: true);
					m_mbuf.Capture(m_meshBake, Matrix4x4.identity, recorder.m_settings);
				}
			}
			m_mbuf.WriteSample(abcObject);
		}

		public void Dispose()
		{
			if (m_mbuf != null)
			{
				m_mbuf.Dispose();
			}
			if (m_cbuf != null)
			{
				m_cbuf.Dispose();
			}
		}
	}

	[CaptureTarget(typeof(ParticleSystem))]
	private class ParticleCapturer : ComponentCapturer, IDisposable
	{
		private ParticleSystem m_target;

		private ParticleSystem.Particle[] m_bufParticles;

		private PinnedList<Vector3> m_bufPoints = new PinnedList<Vector3>();

		private PinnedList<Quaternion> m_bufRotations = new PinnedList<Quaternion>();

		private aePointsData m_data;

		public override void Setup(Component c)
		{
			m_target = c as ParticleSystem;
			abcObject = parent.abcObject.NewPoints(m_target.name, timeSamplingIndex);
		}

		public override void Capture()
		{
			if (m_target == null)
			{
				m_data.visibility = false;
			}
			else
			{
				int maxParticles = m_target.main.maxParticles;
				if (m_bufParticles == null || m_bufParticles.Length != maxParticles)
				{
					m_bufParticles = new ParticleSystem.Particle[maxParticles];
					m_bufPoints.Resize(maxParticles);
					m_bufRotations.Resize(maxParticles);
				}
				int particles = m_target.GetParticles(m_bufParticles);
				for (int i = 0; i < particles; i++)
				{
					m_bufPoints[i] = m_bufParticles[i].position;
					m_bufRotations[i] = Quaternion.AngleAxis(m_bufParticles[i].rotation, m_bufParticles[i].axisOfRotation);
				}
				m_data.visibility = m_target.gameObject.activeSelf;
				m_data.positions = m_bufPoints;
				m_data.count = particles;
			}
			abcObject.WriteSample(ref m_data);
		}

		public void Dispose()
		{
			if (m_bufPoints != null)
			{
				m_bufPoints.Dispose();
			}
			if (m_bufRotations != null)
			{
				m_bufRotations.Dispose();
			}
		}
	}

	private class CaptureNode
	{
		public int instanceID;

		public Type componentType;

		public CaptureNode parent;

		public Transform transform;

		public TransformCapturer transformCapturer;

		public ComponentCapturer componentCapturer;

		public bool setup;

		public void MarkForceInvisible()
		{
			if (transformCapturer != null)
			{
				transformCapturer.MarkForceInvisible();
			}
			if (componentCapturer != null)
			{
				componentCapturer.MarkForceInvisible();
			}
		}

		public void Capture()
		{
			if (transformCapturer != null)
			{
				transformCapturer.Capture();
			}
			if (componentCapturer != null)
			{
				componentCapturer.Capture();
			}
		}
	}

	private class CapturerRecord
	{
		public bool enabled = true;

		public Type type;
	}

	[SerializeField]
	private AlembicRecorderSettings m_settings = new AlembicRecorderSettings();

	private aeContext m_ctx;

	private ComponentCapturer m_root;

	private Dictionary<int, CaptureNode> m_nodes;

	private List<CaptureNode> m_newNodes;

	private List<int> m_iidToRemove;

	private int m_lastTimeSamplingIndex;

	private int m_startFrameOfLastTimeSampling;

	private bool m_recording;

	private float m_time;

	private float m_timePrev;

	private float m_elapsed;

	private int m_frameCount;

	private Dictionary<Type, CapturerRecord> m_capturerTable = new Dictionary<Type, CapturerRecord>();

	public AlembicRecorderSettings Settings
	{
		get
		{
			return m_settings;
		}
		set
		{
			m_settings = value;
		}
	}

	public GameObject TargetBranch
	{
		get
		{
			return m_settings.TargetBranch;
		}
		set
		{
			m_settings.TargetBranch = value;
		}
	}

	public bool Recording => m_recording;

	public int FrameCount
	{
		get
		{
			return m_frameCount;
		}
		set
		{
			m_frameCount = value;
		}
	}

	private Component[] GetTargets(Type type)
	{
		if (m_settings.Scope == ExportScope.TargetBranch && TargetBranch != null)
		{
			return TargetBranch.GetComponentsInChildren(type);
		}
		return Array.ConvertAll(Object.FindObjectsOfType(type), (Object e) => (Component)e);
	}

	private int GetCurrentTimeSamplingIndex()
	{
		if (m_frameCount != m_startFrameOfLastTimeSampling)
		{
			m_startFrameOfLastTimeSampling = m_frameCount;
			m_lastTimeSamplingIndex = m_ctx.AddTimeSampling(m_timePrev);
		}
		return m_lastTimeSamplingIndex;
	}

	private CaptureNode ConstructTree(Transform node)
	{
		if (node == null)
		{
			return null;
		}
		int instanceID = node.gameObject.GetInstanceID();
		if (m_nodes.TryGetValue(instanceID, out var value))
		{
			return value;
		}
		value = new CaptureNode();
		value.instanceID = instanceID;
		value.transform = node;
		value.parent = ConstructTree(node.parent);
		m_nodes.Add(instanceID, value);
		m_newNodes.Add(value);
		return value;
	}

	private void SetupCapturerTable()
	{
		if (m_capturerTable.Count != 0)
		{
			return;
		}
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			Type[] types = assemblies[i].GetTypes();
			foreach (Type type in types)
			{
				object[] customAttributes = type.GetCustomAttributes(typeof(CaptureTarget), inherit: true);
				if (customAttributes.Length != 0)
				{
					m_capturerTable[(customAttributes[0] as CaptureTarget).componentType] = new CapturerRecord
					{
						type = type
					};
				}
			}
		}
		if (!m_settings.CaptureCamera)
		{
			m_capturerTable[typeof(Camera)].enabled = false;
		}
		if (!m_settings.CaptureMeshRenderer)
		{
			m_capturerTable[typeof(MeshRenderer)].enabled = false;
		}
		if (!m_settings.CaptureSkinnedMeshRenderer)
		{
			m_capturerTable[typeof(SkinnedMeshRenderer)].enabled = false;
		}
		if (!m_settings.CaptureParticleSystem)
		{
			m_capturerTable[typeof(ParticleSystem)].enabled = false;
		}
	}

	private void SetupComponentCapturer(CaptureNode node)
	{
		if (node.setup)
		{
			return;
		}
		int currentTimeSamplingIndex = GetCurrentTimeSamplingIndex();
		CaptureNode parent = node.parent;
		if (parent != null && parent.transformCapturer == null)
		{
			SetupComponentCapturer(parent);
			if (!m_nodes.ContainsKey(parent.instanceID) || !m_newNodes.Contains(parent))
			{
				m_nodes.Add(parent.instanceID, parent);
				m_newNodes.Add(parent);
			}
		}
		node.transformCapturer = new TransformCapturer();
		node.transformCapturer.recorder = this;
		node.transformCapturer.parent = ((parent == null) ? m_root : parent.transformCapturer);
		node.transformCapturer.timeSamplingIndex = currentTimeSamplingIndex;
		node.transformCapturer.inherits = true;
		node.transformCapturer.Setup(node.transform);
		if (node.componentType != null && node.componentType != typeof(Transform))
		{
			Component component = node.transform.GetComponent(node.componentType);
			if (component != null)
			{
				CapturerRecord capturerRecord = m_capturerTable[node.componentType];
				node.componentCapturer = Activator.CreateInstance(capturerRecord.type) as ComponentCapturer;
				node.componentCapturer.recorder = this;
				node.componentCapturer.parent = node.transformCapturer;
				node.componentCapturer.timeSamplingIndex = currentTimeSamplingIndex;
				node.componentCapturer.Setup(component);
			}
		}
		node.setup = true;
	}

	private void UpdateCaptureNodes()
	{
		foreach (KeyValuePair<Type, CapturerRecord> item in m_capturerTable)
		{
			if (item.Value.enabled)
			{
				Component[] targets = GetTargets(item.Key);
				foreach (Component component in targets)
				{
					ConstructTree(component.GetComponent<Transform>()).componentType = component.GetType();
				}
			}
		}
		foreach (KeyValuePair<int, CaptureNode> node in m_nodes)
		{
			SetupComponentCapturer(node.Value);
		}
	}

	public void Dispose()
	{
		m_ctx.Destroy();
	}

	public bool BeginRecording()
	{
		if (m_recording)
		{
			Debug.LogWarning("AlembicRecorder: already recording");
			return false;
		}
		if (m_settings.Scope == ExportScope.TargetBranch && TargetBranch == null)
		{
			Debug.LogWarning("AlembicRecorder: target object is not set");
			return false;
		}
		string directoryName = Path.GetDirectoryName(m_settings.OutputPath);
		if (!Directory.Exists(directoryName))
		{
			try
			{
				Directory.CreateDirectory(directoryName);
			}
			catch (Exception)
			{
				Debug.LogWarning("AlembicRecorder: Failed to create directory " + directoryName);
				return false;
			}
		}
		m_ctx = aeContext.Create();
		if (m_ctx.self == IntPtr.Zero)
		{
			Debug.LogWarning("AlembicRecorder: failed to create context");
			return false;
		}
		m_ctx.SetConfig(m_settings.ExportOptions);
		if (!m_ctx.OpenArchive(m_settings.OutputPath))
		{
			Debug.LogWarning("AlembicRecorder: failed to open file " + m_settings.OutputPath);
			m_ctx.Destroy();
			return false;
		}
		m_root = new RootCapturer(this, m_ctx.topObject);
		m_nodes = new Dictionary<int, CaptureNode>();
		m_newNodes = new List<CaptureNode>();
		m_iidToRemove = new List<int>();
		m_lastTimeSamplingIndex = 1;
		m_startFrameOfLastTimeSampling = 0;
		SetupCapturerTable();
		m_recording = true;
		m_time = (m_timePrev = 0f);
		m_frameCount = 0;
		if (m_settings.ExportOptions.TimeSamplingType == TimeSamplingType.Uniform && m_settings.FixDeltaTime)
		{
			Time.maximumDeltaTime = 1f / m_settings.ExportOptions.FrameRate;
		}
		Debug.Log("AlembicRecorder: start " + m_settings.OutputPath);
		return true;
	}

	public void EndRecording()
	{
		if (m_recording)
		{
			m_iidToRemove = null;
			m_newNodes = null;
			m_nodes = null;
			m_root = null;
			m_ctx.Destroy();
			m_recording = false;
			Debug.Log("AlembicRecorder: end: " + m_settings.OutputPath);
		}
	}

	public void ProcessRecording()
	{
		if (!m_recording)
		{
			return;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		UpdateCaptureNodes();
		if (m_frameCount > 0 && m_newNodes.Count > 0)
		{
			m_ctx.MarkFrameBegin();
			foreach (CaptureNode newNode in m_newNodes)
			{
				newNode.MarkForceInvisible();
				newNode.Capture();
			}
			m_ctx.MarkFrameEnd();
		}
		m_newNodes.Clear();
		m_ctx.MarkFrameBegin();
		m_ctx.AddTime(m_time);
		foreach (KeyValuePair<int, CaptureNode> node in m_nodes)
		{
			CaptureNode value = node.Value;
			value.Capture();
			if (value.transform == null)
			{
				m_iidToRemove.Add(value.instanceID);
			}
		}
		m_ctx.MarkFrameEnd();
		foreach (int item in m_iidToRemove)
		{
			m_nodes.Remove(item);
		}
		m_iidToRemove.Clear();
		m_frameCount++;
		m_timePrev = m_time;
		switch (m_settings.ExportOptions.TimeSamplingType)
		{
		case TimeSamplingType.Uniform:
			m_time = 1f / m_settings.ExportOptions.FrameRate * (float)m_frameCount;
			break;
		case TimeSamplingType.Acyclic:
			m_time += Time.deltaTime;
			break;
		}
		m_elapsed = Time.realtimeSinceStartup - realtimeSinceStartup;
		if (m_settings.ExportOptions.TimeSamplingType == TimeSamplingType.Uniform && m_settings.FixDeltaTime)
		{
			AbcAPI.aeWaitMaxDeltaTime();
		}
		if (m_settings.DetailedLog)
		{
			Debug.Log("AlembicRecorder: frame " + m_frameCount + " (" + m_elapsed * 1000f + " ms)");
		}
	}
}
