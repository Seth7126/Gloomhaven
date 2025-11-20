using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;

namespace UnityEngine;

internal class InternalStaticBatchingUtility
{
	public class StaticBatcherGOSorter
	{
		public virtual long GetMaterialId(Renderer renderer)
		{
			if (renderer == null || renderer.sharedMaterial == null)
			{
				return 0L;
			}
			return renderer.sharedMaterial.GetInstanceID();
		}

		public int GetLightmapIndex(Renderer renderer)
		{
			if (renderer == null)
			{
				return -1;
			}
			return renderer.lightmapIndex;
		}

		public static Renderer GetRenderer(GameObject go)
		{
			if (go == null)
			{
				return null;
			}
			MeshFilter meshFilter = go.GetComponent(typeof(MeshFilter)) as MeshFilter;
			if (meshFilter == null)
			{
				return null;
			}
			return meshFilter.GetComponent<Renderer>();
		}

		public static Mesh GetMesh(GameObject go)
		{
			if (go == null)
			{
				return null;
			}
			MeshFilter component = go.GetComponent<MeshFilter>();
			if (component == null)
			{
				return null;
			}
			return component.sharedMesh;
		}

		public virtual long GetRendererId(Renderer renderer)
		{
			if (renderer == null)
			{
				return -1L;
			}
			return renderer.GetInstanceID();
		}

		public static bool GetScaleFlip(GameObject go)
		{
			Transform transform = go.transform;
			float determinant = transform.localToWorldMatrix.determinant;
			return determinant < 0f;
		}
	}

	private const int MaxVerticesInBatch = 64000;

	private const string CombinedMeshPrefix = "Combined Mesh";

	public static void CombineRoot(GameObject staticBatchRoot, StaticBatcherGOSorter sorter)
	{
		Combine(staticBatchRoot, combineOnlyStatic: false, isEditorPostprocessScene: false, sorter);
	}

	public static void Combine(GameObject staticBatchRoot, bool combineOnlyStatic, bool isEditorPostprocessScene, StaticBatcherGOSorter sorter)
	{
		GameObject[] array = (GameObject[])Object.FindObjectsOfType(typeof(GameObject));
		List<GameObject> list = new List<GameObject>();
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			if ((!(staticBatchRoot != null) || gameObject.transform.IsChildOf(staticBatchRoot.transform)) && (!combineOnlyStatic || gameObject.isStaticBatchable))
			{
				list.Add(gameObject);
			}
		}
		array = list.ToArray();
		CombineGameObjects(array, staticBatchRoot, isEditorPostprocessScene, sorter);
	}

	private static uint GetMeshFormatHash(Mesh mesh)
	{
		if (mesh == null)
		{
			return 0u;
		}
		uint num = 1u;
		int vertexAttributeCount = mesh.vertexAttributeCount;
		for (int i = 0; i < vertexAttributeCount; i++)
		{
			VertexAttributeDescriptor vertexAttribute = mesh.GetVertexAttribute(i);
			uint num2 = (uint)vertexAttribute.attribute | (uint)((int)vertexAttribute.format << 4) | (uint)(vertexAttribute.dimension << 8);
			num = (uint)((int)num * -1640531535) + num2;
		}
		return num;
	}

	private static GameObject[] SortGameObjectsForStaticBatching(GameObject[] gos, StaticBatcherGOSorter sorter)
	{
		gos = gos.OrderBy((GameObject g) => StaticBatcherGOSorter.GetScaleFlip(g)).ThenBy(delegate(GameObject g)
		{
			Renderer renderer = StaticBatcherGOSorter.GetRenderer(g);
			return sorter.GetMaterialId(renderer);
		}).ThenBy(delegate(GameObject g)
		{
			Renderer renderer = StaticBatcherGOSorter.GetRenderer(g);
			return sorter.GetLightmapIndex(renderer);
		})
			.ThenBy(delegate(GameObject g)
			{
				Mesh mesh = StaticBatcherGOSorter.GetMesh(g);
				return GetMeshFormatHash(mesh);
			})
			.ThenBy(delegate(GameObject g)
			{
				Renderer renderer = StaticBatcherGOSorter.GetRenderer(g);
				return sorter.GetRendererId(renderer);
			})
			.ToArray();
		return gos;
	}

	public static void CombineGameObjects(GameObject[] gos, GameObject staticBatchRoot, bool isEditorPostprocessScene, StaticBatcherGOSorter sorter)
	{
		Matrix4x4 matrix4x = Matrix4x4.identity;
		Transform staticBatchRootTransform = null;
		if ((bool)staticBatchRoot)
		{
			matrix4x = staticBatchRoot.transform.worldToLocalMatrix;
			staticBatchRootTransform = staticBatchRoot.transform;
		}
		int batchIndex = 0;
		int num = 0;
		List<MeshSubsetCombineUtility.MeshContainer> list = new List<MeshSubsetCombineUtility.MeshContainer>();
		using (StaticBatchingUtility.s_SortMarker.Auto())
		{
			gos = SortGameObjectsForStaticBatching(gos, sorter ?? new StaticBatcherGOSorter());
		}
		uint num2 = 0u;
		bool flag = false;
		GameObject[] array = gos;
		foreach (GameObject gameObject in array)
		{
			MeshFilter meshFilter = gameObject.GetComponent(typeof(MeshFilter)) as MeshFilter;
			if (meshFilter == null)
			{
				continue;
			}
			Mesh sharedMesh = meshFilter.sharedMesh;
			if (sharedMesh == null || (!isEditorPostprocessScene && !sharedMesh.canAccess) || !StaticBatchingHelper.IsMeshBatchable(sharedMesh))
			{
				continue;
			}
			Renderer component = meshFilter.GetComponent<Renderer>();
			if (component == null || !component.enabled || component.staticBatchIndex != 0)
			{
				continue;
			}
			Material[] array2 = component.sharedMaterials;
			if (array2.Any((Material m) => m != null && m.shader != null && m.shader.disableBatching != DisableBatchingType.False))
			{
				continue;
			}
			int vertexCount = sharedMesh.vertexCount;
			if (vertexCount == 0)
			{
				continue;
			}
			MeshRenderer meshRenderer = component as MeshRenderer;
			if (meshRenderer != null && ((meshRenderer.additionalVertexStreams != null && vertexCount != meshRenderer.additionalVertexStreams.vertexCount) || (meshRenderer.enlightenVertexStream != null && vertexCount != meshRenderer.enlightenVertexStream.vertexCount)))
			{
				continue;
			}
			uint meshFormatHash = GetMeshFormatHash(sharedMesh);
			bool scaleFlip = StaticBatcherGOSorter.GetScaleFlip(gameObject);
			if (num + vertexCount > 64000 || meshFormatHash != num2 || scaleFlip != flag)
			{
				MakeBatch(list, staticBatchRootTransform, batchIndex++);
				list.Clear();
				num = 0;
				flag = scaleFlip;
			}
			num2 = meshFormatHash;
			MeshSubsetCombineUtility.MeshInstance instance = new MeshSubsetCombineUtility.MeshInstance
			{
				meshInstanceID = sharedMesh.GetInstanceID(),
				rendererInstanceID = component.GetInstanceID()
			};
			if (meshRenderer != null)
			{
				if (meshRenderer.additionalVertexStreams != null)
				{
					instance.additionalVertexStreamsMeshInstanceID = meshRenderer.additionalVertexStreams.GetInstanceID();
				}
				if (meshRenderer.enlightenVertexStream != null)
				{
					instance.enlightenVertexStreamMeshInstanceID = meshRenderer.enlightenVertexStream.GetInstanceID();
				}
			}
			instance.transform = matrix4x * meshFilter.transform.localToWorldMatrix;
			instance.lightmapScaleOffset = component.lightmapScaleOffset;
			instance.realtimeLightmapScaleOffset = component.realtimeLightmapScaleOffset;
			MeshSubsetCombineUtility.MeshContainer item = new MeshSubsetCombineUtility.MeshContainer
			{
				gameObject = gameObject,
				instance = instance,
				subMeshInstances = new List<MeshSubsetCombineUtility.SubMeshInstance>()
			};
			list.Add(item);
			if (array2.Length > sharedMesh.subMeshCount)
			{
				Debug.LogWarning("Mesh '" + sharedMesh.name + "' has more materials (" + array2.Length + ") than subsets (" + sharedMesh.subMeshCount + ")", component);
				Material[] array3 = new Material[sharedMesh.subMeshCount];
				for (int num3 = 0; num3 < sharedMesh.subMeshCount; num3++)
				{
					array3[num3] = component.sharedMaterials[num3];
				}
				component.sharedMaterials = array3;
				array2 = array3;
			}
			for (int num4 = 0; num4 < Math.Min(array2.Length, sharedMesh.subMeshCount); num4++)
			{
				MeshSubsetCombineUtility.SubMeshInstance item2 = new MeshSubsetCombineUtility.SubMeshInstance
				{
					meshInstanceID = meshFilter.sharedMesh.GetInstanceID(),
					vertexOffset = num,
					subMeshIndex = num4,
					gameObjectInstanceID = gameObject.GetInstanceID(),
					transform = instance.transform
				};
				item.subMeshInstances.Add(item2);
			}
			num += sharedMesh.vertexCount;
		}
		MakeBatch(list, staticBatchRootTransform, batchIndex);
	}

	private static void MakeBatch(List<MeshSubsetCombineUtility.MeshContainer> meshes, Transform staticBatchRootTransform, int batchIndex)
	{
		if (meshes.Count < 2)
		{
			return;
		}
		using (StaticBatchingUtility.s_MakeBatchMarker.Auto())
		{
			List<MeshSubsetCombineUtility.MeshInstance> list = new List<MeshSubsetCombineUtility.MeshInstance>();
			List<MeshSubsetCombineUtility.SubMeshInstance> list2 = new List<MeshSubsetCombineUtility.SubMeshInstance>();
			foreach (MeshSubsetCombineUtility.MeshContainer mesh2 in meshes)
			{
				list.Add(mesh2.instance);
				list2.AddRange(mesh2.subMeshInstances);
			}
			string text = "Combined Mesh";
			text = text + " (root: " + ((staticBatchRootTransform != null) ? staticBatchRootTransform.name : "scene") + ")";
			if (batchIndex > 0)
			{
				text = text + " " + (batchIndex + 1);
			}
			Mesh mesh = StaticBatchingHelper.InternalCombineVertices(list.ToArray(), text);
			StaticBatchingHelper.InternalCombineIndices(list2.ToArray(), mesh);
			int num = 0;
			foreach (MeshSubsetCombineUtility.MeshContainer mesh3 in meshes)
			{
				MeshFilter meshFilter = (MeshFilter)mesh3.gameObject.GetComponent(typeof(MeshFilter));
				meshFilter.sharedMesh = mesh;
				int count = mesh3.subMeshInstances.Count;
				Renderer component = mesh3.gameObject.GetComponent<Renderer>();
				component.SetStaticBatchInfo(num, count);
				component.staticBatchRootTransform = staticBatchRootTransform;
				component.enabled = false;
				component.enabled = true;
				MeshRenderer meshRenderer = component as MeshRenderer;
				if (meshRenderer != null)
				{
					meshRenderer.additionalVertexStreams = null;
					meshRenderer.enlightenVertexStream = null;
				}
				num += count;
			}
		}
	}
}
