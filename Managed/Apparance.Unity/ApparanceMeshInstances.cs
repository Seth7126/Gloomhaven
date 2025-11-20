using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(BoxCollider))]
public class ApparanceMeshInstances : MonoBehaviour
{
	public Mesh instanceMesh;

	public Material[] instanceMaterials;

	public int instanceTypeHandle;

	public int instanceCount;

	public int instanceDrawCalls;

	private List<Matrix4x4[]> instanceTransforms = new List<Matrix4x4[]>();

	private List<Matrix4x4> instanceAccumulator;

	public void Setup(Mesh mesh, Material[] materials, int type_handle)
	{
		instanceTypeHandle = type_handle;
		instanceMesh = mesh;
		instanceMaterials = materials;
		instanceAccumulator = new List<Matrix4x4>();
	}

	public void AddInstance(Matrix4x4 transform)
	{
		instanceAccumulator.Add(transform);
	}

	public int ApplyInstances()
	{
		int i = 0;
		int count;
		int num;
		for (count = instanceAccumulator.Count; i < count; i += num)
		{
			num = Math.Min(count - i, 1023);
			Matrix4x4[] array = new Matrix4x4[num];
			for (int j = 0; j < num; j++)
			{
				array[j] = instanceAccumulator[i + j];
			}
			instanceTransforms.Add(array);
		}
		instanceAccumulator.Clear();
		instanceCount = count;
		instanceDrawCalls = instanceTransforms.Count * instanceMaterials.Length;
		return count;
	}

	public void ClearInstances()
	{
		instanceTransforms.Clear();
		instanceAccumulator.Clear();
	}

	private void Update()
	{
		int num = Math.Min(instanceMaterials.Length, instanceMesh.subMeshCount);
		for (int i = 0; i < num; i++)
		{
			if (instanceMaterials[i] != null)
			{
				for (int j = 0; j < instanceTransforms.Count; j++)
				{
					Graphics.DrawMeshInstanced(instanceMesh, i, instanceMaterials[i], instanceTransforms[j], instanceTransforms[j].Length);
				}
			}
		}
	}
}
