using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Formats.Alembic.Sdk;
using UnityEngine.Rendering;

namespace UnityEngine.Formats.Alembic.Importer;

internal class AlembicMesh : AlembicElement
{
	internal class Submesh : IDisposable
	{
		public PinnedList<int> indexes = new PinnedList<int>();

		public readonly char[] facesetName = new char[255];

		public bool update = true;

		public void Dispose()
		{
			if (indexes != null)
			{
				indexes.Dispose();
			}
		}
	}

	internal class Split : IDisposable
	{
		public NativeArray<Vector3> velocities;

		public NativeArray<Vector3> zeroVelocities;

		public NativeArray<Vector3> points;

		public NativeArray<Vector3> normals;

		public NativeArray<Vector4> tangents;

		public NativeArray<Vector2> uv0;

		public NativeArray<Vector2> uv1;

		public NativeArray<Color> rgba;

		public NativeArray<Color> rgb;

		public Mesh mesh;

		public GameObject host;

		public bool active = true;

		public Vector3 center = Vector3.zero;

		public Vector3 size = Vector3.zero;

		public bool velocitiesSet;

		private bool disposed;

		public void Dispose()
		{
			if (!disposed)
			{
				RuntimeUtils.DisposeIfPossible(ref velocities);
				RuntimeUtils.DisposeIfPossible(ref zeroVelocities);
				RuntimeUtils.DisposeIfPossible(ref points);
				RuntimeUtils.DisposeIfPossible(ref normals);
				RuntimeUtils.DisposeIfPossible(ref tangents);
				RuntimeUtils.DisposeIfPossible(ref uv0);
				RuntimeUtils.DisposeIfPossible(ref uv1);
				RuntimeUtils.DisposeIfPossible(ref rgba);
				RuntimeUtils.DisposeIfPossible(ref rgb);
				if (mesh != null && (mesh.hideFlags & HideFlags.DontSave) != HideFlags.None)
				{
					RuntimeUtils.DestroyUnityObject(mesh);
					mesh = null;
				}
				disposed = true;
			}
		}
	}

	private struct FillVertexBufferJob : IJob
	{
		public aiPolyMeshSample sample;

		public NativeArray<aiPolyMeshData> splitData;

		public NativeArray<aiSubmeshData> submeshData;

		public void Execute()
		{
			sample.FillVertexBuffer(splitData, submeshData);
		}
	}

	[BurstCompile]
	private struct MultiplyByConstant : IJobParallelFor
	{
		public NativeArray<Vector3> data;

		public float scalar;

		public void Execute(int index)
		{
			data[index] = scalar * data[index];
		}
	}

	private aiPolyMesh m_abcSchema;

	protected aiMeshSummary m_summary;

	private aiMeshSampleSummary m_sampleSummary;

	private NativeArray<aiMeshSplitSummary> m_splitSummaries;

	private NativeArray<aiSubmeshSummary> m_submeshSummaries;

	private NativeArray<aiPolyMeshData> m_splitData;

	private NativeArray<aiSubmeshData> m_submeshData;

	private JobHandle fillVertexBufferHandle;

	private List<Split> m_splits = new List<Split>();

	private List<JobHandle> m_PostProcessJobs = new List<JobHandle>();

	private List<Submesh> m_submeshes = new List<Submesh>();

	internal override aiSchema abcSchema => m_abcSchema;

	public override bool visibility => m_sampleSummary.visibility;

	public aiMeshSummary summary => m_summary;

	public aiMeshSampleSummary sampleSummary => m_sampleSummary;

	protected override void Dispose(bool v)
	{
		base.Dispose(v);
		for (int i = 0; i < m_splits.Count; i++)
		{
			m_PostProcessJobs[i].Complete();
			m_splits[i].Dispose();
		}
		RuntimeUtils.DisposeIfPossible(ref m_splitSummaries);
		RuntimeUtils.DisposeIfPossible(ref m_submeshSummaries);
		RuntimeUtils.DisposeIfPossible(ref m_splitData);
		RuntimeUtils.DisposeIfPossible(ref m_submeshData);
	}

	private void UpdateSplits(int numSplits)
	{
		Split split = null;
		if (m_summary.topologyVariance == aiTopologyVariance.Heterogeneous || numSplits > 1)
		{
			for (int i = 0; i < numSplits; i++)
			{
				if (i >= m_splits.Count)
				{
					m_splits.Add(new Split());
					m_PostProcessJobs.Add(default(JobHandle));
				}
				else
				{
					m_splits[i].active = true;
				}
			}
		}
		else if (m_splits.Count == 0)
		{
			split = new Split
			{
				host = base.abcTreeNode.gameObject
			};
			m_splits.Add(split);
			m_PostProcessJobs.Add(default(JobHandle));
		}
		else
		{
			m_splits[0].active = true;
		}
		for (int j = numSplits; j < m_splits.Count; j++)
		{
			m_splits[j].active = false;
		}
	}

	internal override void AbcSetup(aiObject abcObj, aiSchema abcSchema)
	{
		base.AbcSetup(abcObj, abcSchema);
		m_abcSchema = (aiPolyMesh)abcSchema;
		m_abcSchema.GetSummary(ref m_summary);
	}

	public unsafe override void AbcSyncDataBegin()
	{
		if (base.disposed || !m_abcSchema.schema.isDataUpdated)
		{
			return;
		}
		aiPolyMeshSample sample = m_abcSchema.sample;
		sample.GetSummary(ref m_sampleSummary);
		int splitCount = m_sampleSummary.splitCount;
		int submeshCount = m_sampleSummary.submeshCount;
		RuntimeUtils.ResizeIfNeeded(ref m_splitSummaries, splitCount);
		RuntimeUtils.ResizeIfNeeded(ref m_splitData, splitCount);
		RuntimeUtils.ResizeIfNeeded(ref m_submeshSummaries, submeshCount);
		RuntimeUtils.ResizeIfNeeded(ref m_submeshData, submeshCount);
		sample.GetSplitSummaries(m_splitSummaries);
		sample.GetSubmeshSummaries(m_submeshSummaries);
		UpdateSplits(splitCount);
		bool flag = m_sampleSummary.topologyChanged;
		aiPolyMeshData value = default(aiPolyMeshData);
		for (int i = 0; i < splitCount; i++)
		{
			Split split = m_splits[i];
			split.active = true;
			int vertexCount = m_splitSummaries[i].vertexCount;
			if (!m_summary.constantPoints || flag)
			{
				RuntimeUtils.ResizeIfNeeded(ref split.points, vertexCount);
			}
			else
			{
				RuntimeUtils.ResizeIfNeeded(ref split.points, 0);
			}
			value.positions = split.points.GetPointer();
			m_PostProcessJobs[i].Complete();
			if ((bool)m_summary.hasVelocities && (!m_summary.constantVelocities || flag))
			{
				RuntimeUtils.ResizeIfNeeded(ref split.velocities, vertexCount);
				RuntimeUtils.ResizeIfNeeded(ref split.zeroVelocities, vertexCount);
			}
			else
			{
				RuntimeUtils.ResizeIfNeeded(ref split.velocities, 0);
			}
			value.velocities = split.velocities.GetPointer();
			if ((bool)m_summary.hasNormals && (!m_summary.constantNormals || flag))
			{
				RuntimeUtils.ResizeIfNeeded(ref split.normals, vertexCount);
			}
			else
			{
				RuntimeUtils.ResizeIfNeeded(ref split.normals, 0);
			}
			value.normals = split.normals.GetPointer();
			if ((bool)m_summary.hasTangents && (!m_summary.constantTangents || flag))
			{
				RuntimeUtils.ResizeIfNeeded(ref split.tangents, vertexCount);
			}
			else
			{
				RuntimeUtils.ResizeIfNeeded(ref split.tangents, 0);
			}
			value.tangents = split.tangents.GetPointer();
			if ((bool)m_summary.hasUV0 && (!m_summary.constantUV0 || flag))
			{
				RuntimeUtils.ResizeIfNeeded(ref split.uv0, vertexCount);
			}
			else
			{
				RuntimeUtils.ResizeIfNeeded(ref split.uv0, 0);
			}
			value.uv0 = split.uv0.GetPointer();
			if ((bool)m_summary.hasUV1 && (!m_summary.constantUV1 || flag))
			{
				RuntimeUtils.ResizeIfNeeded(ref split.uv1, vertexCount);
			}
			else
			{
				RuntimeUtils.ResizeIfNeeded(ref split.uv1, 0);
			}
			value.uv1 = split.uv1.GetPointer();
			if ((bool)m_summary.hasRgba && (!m_summary.constantRgba || flag))
			{
				RuntimeUtils.ResizeIfNeeded(ref split.rgba, vertexCount);
			}
			else
			{
				RuntimeUtils.ResizeIfNeeded(ref split.rgba, 0);
			}
			value.rgba = split.rgba.GetPointer();
			if ((bool)m_summary.hasRgb && (!m_summary.constantRgb || flag))
			{
				RuntimeUtils.ResizeIfNeeded(ref split.rgb, vertexCount);
			}
			else
			{
				RuntimeUtils.ResizeIfNeeded(ref split.rgb, 0);
			}
			value.rgb = split.rgb.GetPointer();
			m_splitData[i] = value;
		}
		if (m_submeshes.Count > submeshCount)
		{
			m_submeshes.RemoveRange(submeshCount, m_submeshes.Count - submeshCount);
		}
		while (m_submeshes.Count < submeshCount)
		{
			m_submeshes.Add(new Submesh());
		}
		aiSubmeshData value2 = default(aiSubmeshData);
		for (int j = 0; j < submeshCount; j++)
		{
			Submesh submesh = m_submeshes[j];
			m_submeshes[j].update = true;
			submesh.indexes.ResizeDiscard(m_submeshSummaries[j].indexCount);
			value2.indexes = submesh.indexes;
			fixed (char* facesetName = submesh.facesetName)
			{
				value2.facesetNames = facesetName;
			}
			m_submeshData[j] = value2;
		}
		FillVertexBufferJob jobData = new FillVertexBufferJob
		{
			sample = sample,
			splitData = m_splitData,
			submeshData = m_submeshData
		};
		fillVertexBufferHandle = jobData.Schedule();
	}

	public override void AbcSyncDataEnd()
	{
		if (base.disposed || !m_abcSchema.schema.isDataUpdated)
		{
			return;
		}
		fillVertexBufferHandle.Complete();
		for (int i = 0; i < m_splits.Count; i++)
		{
			Split split = m_splits[i];
			if (split.active && split.velocities.Length > 0)
			{
				MultiplyByConstant jobData = new MultiplyByConstant
				{
					data = split.velocities,
					scalar = -1f
				};
				m_PostProcessJobs[i].Complete();
				m_PostProcessJobs[i] = jobData.Schedule(split.velocities.Length, 2048);
			}
		}
		if (!m_abcSchema.schema.isDataUpdated)
		{
			return;
		}
		bool flag = m_sampleSummary.topologyChanged;
		if (base.abcTreeNode.stream.streamDescriptor.Settings.ImportVisibility)
		{
			Bool obj = m_sampleSummary.visibility;
			base.abcTreeNode.gameObject.SetActive(obj);
			if (!obj && !flag)
			{
				return;
			}
		}
		bool flag2 = m_sampleSummary.splitCount > 1;
		for (int j = 0; j < m_splits.Count; j++)
		{
			Split split2 = m_splits[j];
			if (split2.active)
			{
				if (split2.host == null)
				{
					if (flag2)
					{
						string text = base.abcTreeNode.gameObject.name + "_split_" + j;
						Transform transform = base.abcTreeNode.gameObject.transform.Find(text);
						if (transform == null)
						{
							GameObject gameObject = RuntimeUtils.CreateGameObjectWithUndo("Create AlembicObject");
							gameObject.name = text;
							transform = gameObject.GetComponent<Transform>();
							transform.parent = base.abcTreeNode.gameObject.transform;
							transform.localPosition = Vector3.zero;
							transform.localEulerAngles = Vector3.zero;
							transform.localScale = Vector3.one;
						}
						split2.host = transform.gameObject;
					}
					else
					{
						split2.host = base.abcTreeNode.gameObject;
					}
				}
				if (split2.mesh == null)
				{
					split2.mesh = AddMeshComponents(split2.host);
				}
				if (flag)
				{
					split2.mesh.Clear();
					split2.mesh.subMeshCount = m_splitSummaries[j].submeshCount;
				}
				if (split2.points.Length > 0)
				{
					split2.mesh.SetVertices(split2.points);
				}
				if (split2.normals.Length > 0)
				{
					split2.mesh.SetNormals(split2.normals);
				}
				if (split2.tangents.Length > 0)
				{
					split2.mesh.SetTangents(split2.tangents);
				}
				if (split2.uv0.Length > 0)
				{
					split2.mesh.SetUVs(0, split2.uv0);
				}
				if (split2.uv1.Length > 0)
				{
					split2.mesh.SetUVs(1, split2.uv1);
				}
				if (split2.velocities.Length > 0)
				{
					m_PostProcessJobs[j].Complete();
					split2.mesh.SetUVs(5, split2.velocities);
					split2.velocitiesSet = true;
				}
				if (split2.rgba.Length > 0)
				{
					split2.mesh.SetColors(split2.rgba);
				}
				else if (split2.rgb.Length > 0)
				{
					split2.mesh.SetColors(split2.rgb);
				}
				aiPolyMeshData aiPolyMeshData = m_splitData[j];
				split2.mesh.bounds = new Bounds(aiPolyMeshData.center, aiPolyMeshData.extents);
				split2.host.SetActive(value: true);
			}
			else
			{
				split2.host.SetActive(value: false);
			}
		}
		for (int k = 0; k < m_sampleSummary.submeshCount; k++)
		{
			Submesh submesh = m_submeshes[k];
			if (submesh.update)
			{
				aiSubmeshSummary aiSubmeshSummary = m_submeshSummaries[k];
				Split split3 = m_splits[aiSubmeshSummary.splitIndex];
				if (aiSubmeshSummary.topology == aiTopology.Triangles)
				{
					split3.mesh.SetTriangles(submesh.indexes.List, aiSubmeshSummary.submeshIndex, calculateBounds: false);
				}
				else if (aiSubmeshSummary.topology == aiTopology.Lines)
				{
					split3.mesh.SetIndices(submesh.indexes.GetArray(), MeshTopology.Lines, aiSubmeshSummary.submeshIndex, calculateBounds: false);
				}
				else if (aiSubmeshSummary.topology == aiTopology.Points)
				{
					split3.mesh.SetIndices(submesh.indexes.GetArray(), MeshTopology.Points, aiSubmeshSummary.submeshIndex, calculateBounds: false);
				}
				else if (aiSubmeshSummary.topology == aiTopology.Quads)
				{
					split3.mesh.SetIndices(submesh.indexes.GetArray(), MeshTopology.Quads, aiSubmeshSummary.submeshIndex, calculateBounds: false);
				}
			}
		}
		if (!flag)
		{
			return;
		}
		Dictionary<int, List<string>> dictionary = new Dictionary<int, List<string>>();
		for (int l = 0; l < m_sampleSummary.submeshCount; l++)
		{
			aiSubmeshSummary aiSubmeshSummary2 = m_submeshSummaries[l];
			Submesh submesh2 = m_submeshes[l];
			if (!dictionary.TryGetValue(aiSubmeshSummary2.splitIndex, out var value))
			{
				value = new List<string>();
			}
			string item = new string(submesh2.facesetName);
			value.Add(item);
			dictionary[aiSubmeshSummary2.splitIndex] = value;
		}
		for (int m = 0; m < m_sampleSummary.submeshCount; m++)
		{
			aiSubmeshSummary aiSubmeshSummary3 = m_submeshSummaries[m];
			m_splits[aiSubmeshSummary3.splitIndex].host.GetOrAddComponent<AlembicCustomData>().SetFacesetNames(dictionary[aiSubmeshSummary3.splitIndex]);
		}
	}

	internal void ClearMotionVectors()
	{
		foreach (Split split in m_splits)
		{
			if (split.active && split.velocitiesSet && split.velocities.Length > 0)
			{
				split.mesh.SetUVs(5, split.zeroVelocities);
				split.velocitiesSet = false;
			}
		}
	}

	private Mesh AddMeshComponents(GameObject go)
	{
		Mesh mesh = null;
		MeshFilter component = go.GetComponent<MeshFilter>();
		if (!(component != null) || !(component.sharedMesh != null) || component.sharedMesh.name.IndexOf("dyn: ") != 0)
		{
			mesh = new Mesh
			{
				name = "dyn: " + go.name
			};
			mesh.indexFormat = IndexFormat.UInt32;
			mesh.MarkDynamic();
			component = go.GetOrAddComponent<MeshFilter>();
			component.sharedMesh = mesh;
			MeshRenderer orAddComponent = go.GetOrAddComponent<MeshRenderer>();
			if (orAddComponent.sharedMaterial == null)
			{
				orAddComponent.sharedMaterial = GetDefaultMaterial();
			}
		}
		else
		{
			mesh = (component.sharedMesh = Object.Instantiate(component.sharedMesh));
			mesh.name = "dyn: " + go.name;
		}
		return mesh;
	}

	internal static Material GetDefaultMaterial()
	{
		RenderPipelineAsset renderPipelineAsset = GraphicsSettings.renderPipelineAsset;
		if (renderPipelineAsset != null)
		{
			return renderPipelineAsset.defaultMaterial;
		}
		Shader shader = Shader.Find("Standard");
		if (shader != null)
		{
			return new Material(shader);
		}
		return null;
	}
}
