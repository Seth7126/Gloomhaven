using System.Collections.Generic;
using Apparance.Net;
using UnityEngine;

public class AssetInfo
{
	public string Name;

	public int ID;

	public Object Object;

	public UnityEngine.Vector3 MinBounds;

	public UnityEngine.Vector3 MaxBounds;

	public int VariantCount;

	private bool m_InstancingEnabled;

	private List<AssetInstancing> m_InstancingInfo;

	private static List<int[]> instanceSignatures = new List<int[]>();

	public int MonitorHash { get; set; }

	public int MonitorAge { get; set; }

	public bool HasBounds { get; set; }

	public bool instancingSupported
	{
		get
		{
			if (m_InstancingInfo == null)
			{
				m_InstancingEnabled = ExtractInstancingInfo();
			}
			return m_InstancingEnabled;
		}
	}

	public List<AssetInstancing> instancingInfo
	{
		get
		{
			if (m_InstancingInfo == null)
			{
				m_InstancingEnabled = ExtractInstancingInfo();
			}
			return m_InstancingInfo;
		}
	}

	public void UpdateBounds()
	{
		GameObject gameObject = Object as GameObject;
		if (gameObject != null)
		{
			Bounds b = default(Bounds);
			if (ApparanceResources.AccumulateColliderBounds(gameObject, ref b))
			{
				UnityEngine.Vector3 position = gameObject.transform.position;
				MinBounds = b.min - position;
				MaxBounds = b.max - position;
			}
			else if (ApparanceResources.AccumulateMeshBounds(gameObject, ref b))
			{
				UnityEngine.Vector3 position2 = gameObject.transform.position;
				MinBounds = b.min - position2;
				MaxBounds = b.max - position2;
			}
			HasBounds = true;
		}
		else
		{
			MinBounds = UnityEngine.Vector3.zero;
			MaxBounds = UnityEngine.Vector3.zero;
			HasBounds = false;
		}
	}

	public void Respond(int entity_context)
	{
		Apparance.Net.Vector3 min_extents = Conversion.AVfromUV(MinBounds);
		Apparance.Net.Vector3 max_extents = Conversion.AVfromUV(MaxBounds);
		Engine.AssetRequestResponse(entity_context, Name, ID, HasBounds, min_extents, max_extents, VariantCount);
	}

	private bool ExtractInstancingInfo()
	{
		m_InstancingInfo = new List<AssetInstancing>();
		GameObject gameObject = Object as GameObject;
		if (gameObject != null)
		{
			Matrix4x4 undo_root_offset = Matrix4x4.Translate(-gameObject.transform.position);
			if (!ExtractInstancingInfo(gameObject, m_InstancingInfo, undo_root_offset))
			{
				return false;
			}
		}
		return m_InstancingInfo.Count > 0;
	}

	private bool ExtractInstancingInfo(GameObject o, List<AssetInstancing> info, Matrix4x4 undo_root_offset)
	{
		Mesh mesh = null;
		Material[] array = null;
		MeshFilter component = o.GetComponent<MeshFilter>();
		MeshRenderer component2 = o.GetComponent<MeshRenderer>();
		Transform component3 = o.GetComponent<Transform>();
		BoxCollider component4 = o.GetComponent<BoxCollider>();
		int num = ((component != null) ? 1 : 0) + ((component2 != null) ? 1 : 0) + ((component3 != null) ? 1 : 0) + ((component4 != null) ? 1 : 0);
		int num2 = o.GetComponents(typeof(Component)).Length;
		if (num2 > 0 && num2 > num)
		{
			return false;
		}
		if (component != null && component2 != null)
		{
			mesh = component.sharedMesh;
			array = component2.sharedMaterials;
			if (mesh != null && array != null)
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] != null && !array[i].enableInstancing)
					{
						return false;
					}
				}
				AssetInstancing assetInstancing = new AssetInstancing();
				assetInstancing.mesh = mesh;
				assetInstancing.materials = array;
				assetInstancing.transform = undo_root_offset * o.transform.localToWorldMatrix;
				assetInstancing.handle = MakeHandle(mesh, array);
				info.Add(assetInstancing);
			}
		}
		for (int j = 0; j < o.transform.childCount; j++)
		{
			GameObject gameObject = o.transform.GetChild(j).gameObject;
			if (!ExtractInstancingInfo(gameObject, info, undo_root_offset))
			{
				return false;
			}
		}
		return true;
	}

	private static int MakeHandle(Mesh mesh, Material[] materials)
	{
		int num = 1 + materials.Length;
		for (int i = 0; i < instanceSignatures.Count; i++)
		{
			int[] array = instanceSignatures[i];
			if (array.Length != num || array[0] != mesh.GetHashCode())
			{
				continue;
			}
			bool flag = true;
			for (int j = 0; j < materials.Length; j++)
			{
				if (materials[j] != null && array[j + 1] != materials[j].GetHashCode())
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return i;
			}
		}
		int[] array2 = new int[num];
		array2[0] = mesh.GetHashCode();
		for (int k = 0; k < materials.Length; k++)
		{
			array2[k + 1] = ((materials[k] != null) ? materials[k].GetHashCode() : 0);
		}
		instanceSignatures.Add(array2);
		return instanceSignatures.Count - 1;
	}
}
