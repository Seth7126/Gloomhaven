using System;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace UnityEngine.Formats.Alembic.Importer;

[ExecuteInEditMode]
[RequireComponent(typeof(AlembicPointsCloud))]
public class AlembicPointsRenderer : MonoBehaviour
{
	[SerializeField]
	private Mesh m_mesh;

	[SerializeField]
	private Material[] m_materials;

	[SerializeField]
	private Material m_motionVectorMaterial;

	[SerializeField]
	private ShadowCastingMode m_castShadows = ShadowCastingMode.On;

	[SerializeField]
	private bool m_applyTransform = true;

	[SerializeField]
	private bool m_receiveShadows = true;

	[SerializeField]
	private bool m_generateMotionVector = true;

	[SerializeField]
	private float m_pointSize = 0.2f;

	private Mesh m_prevMesh;

	private ComputeBuffer m_cbPoints;

	private ComputeBuffer m_cbVelocities;

	private ComputeBuffer m_cbIDs;

	private ComputeBuffer[] m_cbArgs;

	private CommandBuffer m_cmdMotionVector;

	private int[] m_args = new int[5];

	private Bounds m_bounds;

	private MaterialPropertyBlock m_mpb;

	private Vector3 m_position;

	private Vector3 m_positionOld;

	private Quaternion m_rotation;

	private Quaternion m_rotationOld;

	private Vector3 m_scale;

	private Vector3 m_scaleOld;

	public Mesh InstancedMesh
	{
		get
		{
			return m_mesh;
		}
		set
		{
			m_mesh = value;
		}
	}

	public List<Material> Materials
	{
		get
		{
			List<Material> list = new List<Material>(m_materials.Length);
			Material[] materials = m_materials;
			foreach (Material value in materials)
			{
				list[0] = value;
			}
			return list;
		}
		set
		{
			m_materials = value.ToArray();
		}
	}

	public Material MotionVectorMaterial
	{
		get
		{
			return m_motionVectorMaterial;
		}
		set
		{
			m_motionVectorMaterial = value;
		}
	}

	private void Flush()
	{
		AlembicPointsCloud component = GetComponent<AlembicPointsCloud>();
		List<Vector3> positions = component.Positions;
		int count = positions.Count;
		if (count == 0)
		{
			return;
		}
		List<Vector3> velocities = component.Velocities;
		List<uint> ids = component.Ids;
		Material[] materials = m_materials;
		Mesh mesh = m_mesh;
		if (mesh == null || materials == null)
		{
			return;
		}
		int num = Math.Min(mesh.subMeshCount, materials.Length);
		int layer = base.gameObject.layer;
		if (!SystemInfo.supportsInstancing || !SystemInfo.supportsComputeShaders)
		{
			Debug.LogWarning("AlembicPointsRenderer: Instancing is not supported on this system.");
			return;
		}
		if (m_prevMesh != mesh)
		{
			m_prevMesh = mesh;
			if (m_cbArgs != null)
			{
				ComputeBuffer[] cbArgs = m_cbArgs;
				for (int i = 0; i < cbArgs.Length; i++)
				{
					cbArgs[i].Release();
				}
				m_cbArgs = null;
			}
		}
		bool flag = false;
		bool flag2 = false;
		if (m_cbPoints != null && m_cbPoints.count < count)
		{
			m_cbPoints.Release();
			m_cbPoints = null;
		}
		if (m_cbPoints == null)
		{
			m_cbPoints = new ComputeBuffer(count, 12);
		}
		m_cbPoints.SetData(positions);
		if (velocities.Count == count)
		{
			flag = true;
			if (m_cbVelocities != null && m_cbVelocities.count < count)
			{
				m_cbVelocities.Release();
				m_cbVelocities = null;
			}
			if (m_cbVelocities == null)
			{
				m_cbVelocities = new ComputeBuffer(count, 12);
			}
			m_cbVelocities.SetData(velocities);
		}
		if (ids.Count == count)
		{
			flag2 = true;
			if (m_cbIDs != null && m_cbIDs.count < count)
			{
				m_cbIDs.Release();
				m_cbIDs = null;
			}
			if (m_cbIDs == null)
			{
				m_cbIDs = new ComputeBuffer(count, 4);
			}
			m_cbIDs.SetData(ids);
		}
		m_bounds = new Bounds(component.BoundsCenter, component.BoundsExtents + mesh.bounds.extents);
		if (m_mpb == null)
		{
			m_mpb = new MaterialPropertyBlock();
		}
		if (m_applyTransform)
		{
			m_mpb.SetVector("_Position", m_position);
			m_mpb.SetVector("_PositionOld", m_positionOld);
			m_mpb.SetVector("_Rotation", new Vector4(m_rotation.x, m_rotation.y, m_rotation.z, m_rotation.w));
			m_mpb.SetVector("_RotationOld", new Vector4(m_rotationOld.x, m_rotationOld.y, m_rotationOld.z, m_rotationOld.w));
			m_mpb.SetVector("_Scale", m_scale);
			m_mpb.SetVector("_ScaleOld", m_scaleOld);
		}
		else
		{
			m_mpb.SetVector("_Position", Vector3.zero);
			m_mpb.SetVector("_PositionOld", Vector3.zero);
			m_mpb.SetVector("_Rotation", new Vector4(0f, 0f, 0f, 1f));
			m_mpb.SetVector("_RotationOld", new Vector4(0f, 0f, 0f, 1f));
			m_mpb.SetVector("_Scale", Vector3.one);
			m_mpb.SetVector("_ScaleOld", Vector3.one);
		}
		m_mpb.SetFloat("_PointSize", m_pointSize);
		m_mpb.SetBuffer("_AlembicPoints", m_cbPoints);
		if (flag2)
		{
			m_mpb.SetFloat("_AlembicHasIDs", 1f);
			m_mpb.SetBuffer("_AlembicIDs", m_cbIDs);
		}
		if (flag)
		{
			m_mpb.SetFloat("_AlembicHasVelocities", 1f);
			m_mpb.SetBuffer("_AlembicVelocities", m_cbVelocities);
		}
		if (m_cbArgs == null || m_cbArgs.Length != num)
		{
			if (m_cbArgs != null)
			{
				ComputeBuffer[] cbArgs = m_cbArgs;
				for (int i = 0; i < cbArgs.Length; i++)
				{
					cbArgs[i].Release();
				}
				m_cbArgs = null;
			}
			m_cbArgs = new ComputeBuffer[num];
			for (int j = 0; j < num; j++)
			{
				m_cbArgs[j] = new ComputeBuffer(1, m_args.Length * 4, ComputeBufferType.DrawIndirect);
			}
		}
		for (int k = 0; k < num; k++)
		{
			m_args[0] = (int)mesh.GetIndexCount(k);
			m_args[1] = count;
			m_cbArgs[k].SetData(m_args);
		}
		int num2 = Math.Min(num, materials.Length);
		for (int l = 0; l < num2; l++)
		{
			ComputeBuffer bufferWithArgs = m_cbArgs[l];
			Material material = materials[l];
			if (!(material == null))
			{
				Graphics.DrawMeshInstancedIndirect(mesh, l, material, m_bounds, bufferWithArgs, 0, m_mpb, m_castShadows, m_receiveShadows, layer);
			}
		}
	}

	private void FlushMotionVector()
	{
		if (!m_generateMotionVector || Camera.current == null || (Camera.current.depthTextureMode & DepthTextureMode.MotionVectors) == 0)
		{
			return;
		}
		Material motionVectorMaterial = m_motionVectorMaterial;
		Mesh mesh = m_mesh;
		AlembicPointsCloud component = GetComponent<AlembicPointsCloud>();
		if (!(mesh == null) && !(motionVectorMaterial == null) && component.Velocities.Count != 0)
		{
			motionVectorMaterial.SetMatrix("_PreviousVP", VPMatrices.GetPrevious(Camera.current));
			motionVectorMaterial.SetMatrix("_NonJitteredVP", VPMatrices.Get(Camera.current));
			_ = base.gameObject.layer;
			if (m_cmdMotionVector == null)
			{
				m_cmdMotionVector = new CommandBuffer();
				m_cmdMotionVector.name = "AlembicPointsRenderer";
			}
			m_cmdMotionVector.Clear();
			m_cmdMotionVector.SetRenderTarget(BuiltinRenderTextureType.MotionVectors, BuiltinRenderTextureType.CameraTarget);
			for (int i = 0; i < mesh.subMeshCount; i++)
			{
				m_cmdMotionVector.DrawMeshInstancedIndirect(mesh, i, motionVectorMaterial, 0, m_cbArgs[i], 0, m_mpb);
			}
			Graphics.ExecuteCommandBuffer(m_cmdMotionVector);
		}
	}

	private void Release()
	{
		if (m_cbArgs != null)
		{
			ComputeBuffer[] cbArgs = m_cbArgs;
			for (int i = 0; i < cbArgs.Length; i++)
			{
				cbArgs[i].Release();
			}
			m_cbArgs = null;
		}
		if (m_cbPoints != null)
		{
			m_cbPoints.Release();
			m_cbPoints = null;
		}
		if (m_cbVelocities != null)
		{
			m_cbVelocities.Release();
			m_cbVelocities = null;
		}
		if (m_cbIDs != null)
		{
			m_cbIDs.Release();
			m_cbIDs = null;
		}
		if (m_cmdMotionVector != null)
		{
			m_cmdMotionVector.Release();
			m_cmdMotionVector = null;
		}
	}

	private void OnDisable()
	{
		Release();
	}

	private void LateUpdate()
	{
		m_positionOld = m_position;
		m_rotationOld = m_rotation;
		m_scaleOld = m_scale;
		Transform component = GetComponent<Transform>();
		m_position = component.position;
		m_rotation = component.rotation;
		m_scale = component.lossyScale;
		Flush();
	}

	private void OnRenderObject()
	{
		FlushMotionVector();
	}

	private void Start()
	{
		Transform component = GetComponent<Transform>();
		m_position = (m_positionOld = component.position);
		m_rotation = (m_rotationOld = component.rotation);
		m_scale = (m_scaleOld = component.lossyScale);
	}

	private void OnDestroy()
	{
		if (m_cbPoints != null)
		{
			m_cbPoints.Dispose();
		}
		if (m_cbVelocities != null)
		{
			m_cbVelocities.Dispose();
		}
		if (m_cbIDs != null)
		{
			m_cbIDs.Dispose();
		}
		if (m_cmdMotionVector != null)
		{
			m_cmdMotionVector.Dispose();
		}
		if (m_cbArgs != null)
		{
			Array.ForEach(m_cbArgs, delegate(ComputeBuffer cb)
			{
				cb?.Dispose();
			});
		}
	}
}
