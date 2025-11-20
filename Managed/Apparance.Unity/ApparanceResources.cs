using System;
using System.Collections.Generic;
using System.Linq;
using Apparance.Net;
using Apparance.Unity;
using UnityEngine;

[ExecuteInEditMode]
public class ApparanceResources : MonoBehaviour
{
	public List<ApparanceObjectResource> Objects = new List<ApparanceObjectResource>();

	public List<ApparanceResourceList> Externals = new List<ApparanceResourceList>();

	public List<ApparanceResourceTable> Indirects = new List<ApparanceResourceTable>();

	[NonSerialized]
	private Dictionary<string, AssetInfo> m_NameToAssetMap = new Dictionary<string, AssetInfo>();

	[NonSerialized]
	private Dictionary<int, AssetInfo> m_IDToAssetMap = new Dictionary<int, AssetInfo>();

	private List<AssetInfo> m_NewAssetMonitor = new List<AssetInfo>();

	private List<AssetInfo> m_FastAssetMonitor = new List<AssetInfo>();

	private List<AssetInfo> m_SlowAssetMonitor = new List<AssetInfo>();

	private int m_SlowAssetMonitorIndex;

	private const int k_AssetMonitorRelaxAge = 180;

	private int m_ResourceCheckIndex;

	private IApparanceResourceProvider[] ResourceProviders => GetComponents<IApparanceResourceProvider>().ToArray();

	private bool IsLocalResources => GetComponent<ApparanceEntity>() != null;

	private void OnValidate()
	{
		PurgeEmptyExternals();
	}

	public GameObject GetPrefab(int id, out UnityEngine.Vector3 min, out UnityEngine.Vector3 max, out List<AssetInstancing> instancingInfo, out string error_message)
	{
		AssetInfo value = null;
		error_message = null;
		m_IDToAssetMap.TryGetValue(id, out value);
		if (value != null && value.Object != null)
		{
			min = value.MinBounds;
			max = value.MaxBounds;
			instancingInfo = (value.instancingSupported ? value.instancingInfo : null);
			return value.Object as GameObject;
		}
		if (IsLocalResources)
		{
			return ApparanceEngine.Instance.Resources.GetPrefab(id, out min, out max, out instancingInfo, out error_message);
		}
		AssetInfo assetInfo = value;
		value = ApparanceEngine.Instance.GetDebugMissingObject();
		if (value != null)
		{
			min = value.MinBounds;
			max = value.MaxBounds;
			error_message = "Unable to find prefab: " + ((assetInfo != null) ? assetInfo.Name : "unknown");
			instancingInfo = null;
			return value.Object as GameObject;
		}
		min = UnityEngine.Vector3.zero;
		max = UnityEngine.Vector3.zero;
		instancingInfo = null;
		return value.Object as GameObject;
	}

	public Material GetMaterial(int id, out string error_message)
	{
		AssetInfo value = null;
		error_message = null;
		m_IDToAssetMap.TryGetValue(id, out value);
		if (value != null && value.Object != null)
		{
			return value.Object as Material;
		}
		if (IsLocalResources)
		{
			return ApparanceEngine.Instance.Resources.GetMaterial(id, out error_message);
		}
		AssetInfo assetInfo = value;
		value = ApparanceEngine.Instance.GetDebugMissingMaterial();
		if (value != null)
		{
			error_message = "Unable to find material: " + ((assetInfo != null) ? assetInfo.Name : "unknown");
			return value.Object as Material;
		}
		return value.Object as Material;
	}

	public void Update()
	{
		if (EditorServices.isEditor)
		{
			UpdateAssetMonitor();
		}
	}

	public void RefreshResourceList(bool clear_unused)
	{
		if (clear_unused)
		{
			Objects.RemoveAll((ApparanceObjectResource r) => r.Object == null);
		}
		ClearCache();
		PurgeEmptyExternals();
	}

	public Material GetMaterialAsset(string name)
	{
		if (m_NameToAssetMap.TryGetValue(name, out var value) && value.Object is Material)
		{
			return (Material)value.Object;
		}
		return null;
	}

	public GameObject GetPrefabAsset(string name)
	{
		if (m_NameToAssetMap.TryGetValue(name, out var value) && value.Object is GameObject)
		{
			return (GameObject)value.Object;
		}
		return null;
	}

	internal AssetInfo HandleAssetRequest(ref AssetRequest request)
	{
		AssetInfo value = null;
		if (m_IDToAssetMap.TryGetValue(request.ID, out value))
		{
			return value;
		}
		string text = ExtractAssetName(request.Descriptor);
		string description = ExtractAssetDescription(request.Descriptor);
		ExtractAssetVariant(request.Descriptor);
		IApparanceResourceProvider[] resourceProviders = ResourceProviders;
		if (resourceProviders != null)
		{
			IApparanceResourceProvider[] array = resourceProviders;
			foreach (IApparanceResourceProvider apparanceResourceProvider in array)
			{
				if (apparanceResourceProvider as UnityEngine.Object != null)
				{
					value = apparanceResourceProvider.ResourceRequest(text, request.ID);
					break;
				}
			}
		}
		if (value == null)
		{
			for (int j = 0; j < Objects.Count; j++)
			{
				if (Objects[j].Name == text)
				{
					value = new AssetInfo();
					value.ID = request.ID;
					value.Name = text;
					value.VariantCount = Objects[j].Variants;
					value.Object = (Objects[j].Copy ? CreateCopyOfObject(Objects[j].Object) : Objects[j].Object);
					value.UpdateBounds();
				}
			}
		}
		if (value == null)
		{
			foreach (ApparanceResourceList external in Externals)
			{
				if (external != null)
				{
					ApparanceObjectResource apparanceObjectResource = external.FindExternalAsset(text, description);
					if (apparanceObjectResource != null)
					{
						value = new AssetInfo();
						value.ID = request.ID;
						value.Name = text;
						value.VariantCount = apparanceObjectResource.Variants;
						value.Object = (apparanceObjectResource.Copy ? CreateCopyOfObject(apparanceObjectResource.Object) : apparanceObjectResource.Object);
						value.UpdateBounds();
						m_IDToAssetMap[value.ID] = value;
						m_NameToAssetMap[value.Name] = value;
						MonitorAsset(value);
						return value;
					}
				}
			}
		}
		if (value == null)
		{
			foreach (ApparanceResourceTable indirect in Indirects)
			{
				if (!(indirect != null))
				{
					continue;
				}
				ApparanceResourceList apparanceResourceList = indirect.LookupResourceList(base.gameObject, ref request);
				if (apparanceResourceList != null)
				{
					ApparanceObjectResource apparanceObjectResource2 = apparanceResourceList.FindExternalAsset(text, description);
					if (apparanceObjectResource2 != null)
					{
						value = new AssetInfo();
						value.ID = request.ID;
						value.Name = text;
						value.VariantCount = apparanceObjectResource2.Variants;
						value.Object = apparanceObjectResource2.Object;
						value.UpdateBounds();
						m_IDToAssetMap[value.ID] = value;
						m_NameToAssetMap[value.Name] = value;
						MonitorAsset(value);
						return value;
					}
				}
				if (request.IsWaiting)
				{
					return null;
				}
			}
		}
		if (value == null && !IsLocalResources)
		{
			value = GenerateFallbackAssetInfo(ref request);
		}
		if (value != null)
		{
			CacheAssetInfo(value);
		}
		return value;
	}

	internal AssetInfo GenerateFallbackAssetInfo(ref AssetRequest request)
	{
		string text = ExtractAssetName(request.Descriptor);
		string description = ExtractAssetDescription(request.Descriptor);
		int variant = ExtractAssetVariant(request.Descriptor);
		ApparanceObjectResource apparanceObjectResource = new ApparanceObjectResource();
		apparanceObjectResource.Name = text;
		apparanceObjectResource.Object = null;
		apparanceObjectResource.Description = description;
		apparanceObjectResource.IsNew = true;
		apparanceObjectResource.Variant = variant;
		Objects.Add(apparanceObjectResource);
		return new AssetInfo
		{
			ID = request.ID,
			Name = text,
			VariantCount = apparanceObjectResource.Variants,
			Object = null,
			HasBounds = false
		};
	}

	internal void CacheAssetInfo(AssetInfo info)
	{
		m_IDToAssetMap[info.ID] = info;
		m_NameToAssetMap[info.Name] = info;
		MonitorAsset(info);
	}

	private void ClearCache()
	{
		foreach (int key in m_IDToAssetMap.Keys)
		{
			AssetInfo assetInfo = m_IDToAssetMap[key];
			ApparanceEntity component = GetComponent<ApparanceEntity>();
			int entity_context = ((component != null) ? component.m_EntityHandle : 0);
			Apparance.Net.Vector3 vector = new Apparance.Net.Vector3(0f, 0f, 0f);
			Engine.AssetRequestResponse(entity_context, assetInfo.Name, 0, bounds_available: false, vector, vector, 0);
		}
		m_IDToAssetMap.Clear();
		m_NameToAssetMap.Clear();
		StopMonitoringAllAssets();
	}

	internal void RefreshCache(int entity_context)
	{
	}

	private static string ExtractAssetName(string asset_descriptor)
	{
		int num = asset_descriptor.IndexOf('(');
		if (num != -1)
		{
			int num2 = asset_descriptor.IndexOf(')', num);
			string text = asset_descriptor.Substring(0, num).Trim();
			string text2 = "";
			if (num2 > num)
			{
				text2 = asset_descriptor.Substring(num2 + 1).Trim();
			}
			return text + text2;
		}
		return asset_descriptor;
	}

	private static string ExtractAssetDescription(string asset_descriptor)
	{
		int num = asset_descriptor.IndexOf('(');
		if (num != -1)
		{
			int num2 = asset_descriptor.IndexOf(')', num);
			if (num2 == -1)
			{
				num2 = asset_descriptor.Length;
			}
			return asset_descriptor[(num + 1)..num2];
		}
		return "";
	}

	private static int ExtractAssetVariant(string asset_descriptor)
	{
		int result = 0;
		int num = asset_descriptor.IndexOf('(');
		int num2 = asset_descriptor.IndexOf('#');
		if (num2 != -1)
		{
			if (num != -1)
			{
				if (num2 < num)
				{
					int.TryParse(asset_descriptor.Substring(num2 + 1, num - num2 - 1), out result);
				}
				else
				{
					int.TryParse(asset_descriptor.Substring(num2 + 1), out result);
				}
			}
			else
			{
				int.TryParse(asset_descriptor.Substring(num2 + 1), out result);
			}
		}
		return result;
	}

	private static UnityEngine.Object CreateCopyOfObject(UnityEngine.Object source)
	{
		if (source != null)
		{
			if (source is Material)
			{
				return new Material((Material)source);
			}
			return source;
		}
		return null;
	}

	private void MonitorAsset(AssetInfo asset)
	{
		m_NewAssetMonitor.Add(asset);
	}

	private void StopMonitoringAllAssets()
	{
		m_NewAssetMonitor.Clear();
		m_FastAssetMonitor.Clear();
		m_SlowAssetMonitor.Clear();
	}

	private void UpdateAssetMonitor()
	{
		m_SlowAssetMonitor.AddRange(m_NewAssetMonitor);
		m_NewAssetMonitor.Clear();
		int num = 0;
		while (num < m_FastAssetMonitor.Count)
		{
			AssetInfo assetInfo = m_FastAssetMonitor[num];
			if (assetInfo.Object != null)
			{
				if (!CheckAssetChange(assetInfo))
				{
					assetInfo.MonitorAge++;
				}
				if (assetInfo.MonitorAge > 180)
				{
					m_FastAssetMonitor.RemoveAt(num);
					m_SlowAssetMonitor.Add(assetInfo);
				}
				else
				{
					num++;
				}
			}
		}
		if (m_SlowAssetMonitor.Count > 0)
		{
			int index = ++m_SlowAssetMonitorIndex % m_SlowAssetMonitor.Count;
			AssetInfo assetInfo2 = m_SlowAssetMonitor[index];
			if (CheckAssetChange(assetInfo2))
			{
				m_SlowAssetMonitor.RemoveAt(index);
				m_FastAssetMonitor.Add(assetInfo2);
			}
		}
		ApparanceObjectResource apparanceObjectResource = null;
		if (Objects.Count > 0)
		{
			int index2 = m_ResourceCheckIndex % Objects.Count;
			apparanceObjectResource = Objects[index2];
		}
		m_ResourceCheckIndex++;
		if (apparanceObjectResource != null && m_NameToAssetMap.TryGetValue(apparanceObjectResource.Name, out var value) && apparanceObjectResource.Object != value.Object)
		{
			value.Object = apparanceObjectResource.Object;
			value.MonitorHash = GenerateAssetHash(value.Object);
			value.MonitorAge = 0;
			value.UpdateBounds();
		}
	}

	private bool CheckAssetChange(AssetInfo asset)
	{
		if (asset.Object != null)
		{
			bool flag = asset.MonitorHash == 0;
			int num = GenerateAssetHash(asset.Object);
			bool num2 = num != asset.MonitorHash;
			asset.MonitorHash = num;
			if (num2 && !flag)
			{
				asset.MonitorHash = num;
				asset.MonitorAge = 0;
				asset.UpdateBounds();
				OnAssetChange(asset);
				return true;
			}
			if (!asset.HasBounds)
			{
				asset.UpdateBounds();
			}
		}
		return false;
	}

	private void OnAssetChange(AssetInfo info)
	{
		ApparanceEntity component = GetComponent<ApparanceEntity>();
		int entity_context = ((component != null) ? component.m_EntityHandle : 0);
		info.Respond(entity_context);
		ApparanceEngine.Instance.RequestRebuild();
	}

	private static int GenerateAssetHash(UnityEngine.Object o, int seed = 1)
	{
		if (o is GameObject)
		{
			return GenerateAssetHash((GameObject)o, seed);
		}
		int num = 0;
		if (o != null)
		{
			num ^= Conversion.HashString(o.name, seed);
		}
		return num;
	}

	private static int GenerateAssetHash(GameObject prefab, int seed = 1)
	{
		int num = 0;
		if (prefab != null)
		{
			num = 1;
			num ^= Conversion.HashVector(prefab.transform.position, seed);
			num ^= Conversion.HashQuaternion(prefab.transform.rotation, seed + 1);
			num ^= Conversion.HashVector(prefab.transform.localScale, seed + 2);
			num ^= (prefab.activeSelf ? (seed * 3467) : 0);
			BoxCollider component = prefab.GetComponent<BoxCollider>();
			if (component != null)
			{
				num ^= Conversion.HashVector(component.center, seed + 3);
				num ^= Conversion.HashVector(component.size, seed + 4);
			}
			MeshRenderer component2 = prefab.GetComponent<MeshRenderer>();
			Material[] array = ((component2 != null) ? component2.sharedMaterials : null);
			if (array != null)
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] != null && array[i].enableInstancing)
					{
						num ^= i * 1345 + seed;
					}
				}
			}
			for (int j = 0; j < prefab.transform.childCount; j++)
			{
				num ^= GenerateAssetHash(prefab.transform.GetChild(j).gameObject, seed + 100) + (j + 1) * 1129;
			}
		}
		return num;
	}

	private void PurgeEmptyExternals()
	{
		Externals.RemoveAll((ApparanceResourceList l) => l == null);
	}

	public static Bounds AccumulateObjectBounds(GameObject o)
	{
		Bounds b = default(Bounds);
		if (!AccumulateColliderBounds(o, ref b) && !AccumulateMeshBounds(o, ref b))
		{
			b = new Bounds(new UnityEngine.Vector3(0f, 0f, 0f), new UnityEngine.Vector3(0.5f, 0.5f, 0.5f));
		}
		return b;
	}

	private static void AccumulateBounds(ref Bounds b_out, Bounds worldspace_bounds_in)
	{
		if (b_out.center == UnityEngine.Vector3.zero && b_out.extents == UnityEngine.Vector3.zero)
		{
			b_out = worldspace_bounds_in;
		}
		else
		{
			b_out.Encapsulate(worldspace_bounds_in);
		}
	}

	private static void AccumulateBounds(ref Bounds b_out, UnityEngine.Vector3 worldspace_point_in)
	{
		if (b_out.center == UnityEngine.Vector3.zero && b_out.extents == UnityEngine.Vector3.zero)
		{
			b_out = new Bounds(worldspace_point_in, UnityEngine.Vector3.zero);
		}
		else
		{
			b_out.Encapsulate(worldspace_point_in);
		}
	}

	internal static bool AccumulateMeshBounds(GameObject o, ref Bounds b)
	{
		bool result = false;
		Renderer component = o.GetComponent<Renderer>();
		if (component != null && component.enabled)
		{
			AccumulateBounds(ref b, component.bounds);
			result = true;
		}
		for (int i = 0; i < o.transform.childCount; i++)
		{
			if (AccumulateMeshBounds(o.transform.GetChild(i).gameObject, ref b))
			{
				result = true;
			}
		}
		return result;
	}

	internal static bool AccumulateColliderBounds(GameObject o, ref Bounds b)
	{
		bool result = false;
		BoxCollider component = o.GetComponent<BoxCollider>();
		if (component != null && component.enabled && component.sharedMaterial == null)
		{
			Matrix4x4 localToWorldMatrix = o.transform.localToWorldMatrix;
			UnityEngine.Vector3 center = component.center;
			float num = component.size.x * 0.5f;
			float num2 = component.size.y * 0.5f;
			float num3 = component.size.z * 0.5f;
			AccumulateBounds(ref b, localToWorldMatrix.MultiplyPoint(new UnityEngine.Vector3(center.x - num, center.y - num2, center.z - num3)));
			AccumulateBounds(ref b, localToWorldMatrix.MultiplyPoint(new UnityEngine.Vector3(center.x + num, center.y - num2, center.z - num3)));
			AccumulateBounds(ref b, localToWorldMatrix.MultiplyPoint(new UnityEngine.Vector3(center.x - num, center.y + num2, center.z - num3)));
			AccumulateBounds(ref b, localToWorldMatrix.MultiplyPoint(new UnityEngine.Vector3(center.x + num, center.y + num2, center.z - num3)));
			AccumulateBounds(ref b, localToWorldMatrix.MultiplyPoint(new UnityEngine.Vector3(center.x - num, center.y - num2, center.z + num3)));
			AccumulateBounds(ref b, localToWorldMatrix.MultiplyPoint(new UnityEngine.Vector3(center.x + num, center.y - num2, center.z + num3)));
			AccumulateBounds(ref b, localToWorldMatrix.MultiplyPoint(new UnityEngine.Vector3(center.x - num, center.y + num2, center.z + num3)));
			AccumulateBounds(ref b, localToWorldMatrix.MultiplyPoint(new UnityEngine.Vector3(center.x + num, center.y + num2, center.z + num3)));
			result = true;
		}
		for (int i = 0; i < o.transform.childCount; i++)
		{
			if (AccumulateColliderBounds(o.transform.GetChild(i).gameObject, ref b))
			{
				result = true;
			}
		}
		return result;
	}
}
