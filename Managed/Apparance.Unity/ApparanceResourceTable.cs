using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Resource Table", menuName = "Apparance/Resource Table", order = 1)]
public class ApparanceResourceTable : ScriptableObject
{
	[Serializable]
	public struct ResourceListMapping
	{
		[Tooltip("Category prefix for assets in a Resource List")]
		public string Category;

		[Tooltip("Path within a Resources folder to locate the Resource List")]
		public string AssetPath;
	}

	public string AssetPathPrefix;

	[Tooltip("Try category as asset name (with prefix) if no entry found?")]
	public bool autoFallback;

	public List<ResourceListMapping> ResourceLists = new List<ResourceListMapping>();

	private static string GetPrefix(string descriptor)
	{
		int num = descriptor.IndexOf('.');
		if (num != -1)
		{
			return descriptor.Substring(0, num);
		}
		return null;
	}

	private static void HandleAsyncResourceListLoad(string resource_list_asset_path, ApparanceResourceList list)
	{
		ApparanceEngine.Instance.NotifyAsyncResourceListCompleted(resource_list_asset_path, list);
	}

	public ApparanceResourceList LookupResourceList(GameObject requester, ref AssetRequest request)
	{
		string descriptor = request.Descriptor;
		string prefix = GetPrefix(descriptor);
		if (!string.IsNullOrWhiteSpace(prefix))
		{
			string asset_path = null;
			for (int i = 0; i < ResourceLists.Count; i++)
			{
				if (string.Compare(ResourceLists[i].Category, prefix, ignoreCase: true) == 0)
				{
					asset_path = AssetPathPrefix + ResourceLists[i].AssetPath;
					break;
				}
			}
			if (asset_path == null && autoFallback)
			{
				asset_path = AssetPathPrefix + prefix;
			}
			if (asset_path != null)
			{
				IApparanceResourceListLoader component = requester.GetComponent<IApparanceResourceListLoader>();
				ApparanceResourceList apparanceResourceList;
				if (component != null)
				{
					apparanceResourceList = component.Load(asset_path);
					if (apparanceResourceList != null)
					{
						return apparanceResourceList;
					}
				}
				IApparanceResourceListLoaderAsync component2 = requester.GetComponent<IApparanceResourceListLoaderAsync>();
				if (component2 != null)
				{
					apparanceResourceList = component2.LoadCheck(asset_path);
					bool flag = true;
					if (request.IsWaiting)
					{
						if (apparanceResourceList != null)
						{
							request.StopWaiting();
							return apparanceResourceList;
						}
						flag = false;
					}
					else if (apparanceResourceList != null)
					{
						return apparanceResourceList;
					}
					if (flag)
					{
						bool flag2 = true;
						if (!ApparanceEngine.Instance.CheckAsyncResourceListPending(asset_path) && !component2.LoadAsync(asset_path, delegate(ApparanceResourceList rl)
						{
							HandleAsyncResourceListLoad(asset_path, rl);
						}))
						{
							flag2 = false;
						}
						if (flag2)
						{
							ApparanceEngine.Instance.NotifyResourceListDependentRequest(asset_path, ref request);
							return null;
						}
					}
				}
				apparanceResourceList = Resources.Load<ApparanceResourceList>(asset_path);
				if (apparanceResourceList != null)
				{
					return apparanceResourceList;
				}
			}
			Debug.LogWarning("Failed to find Resource List for category '" + prefix + "' in " + base.name + " (resolving '" + descriptor + "') via path '" + asset_path + "'");
		}
		return null;
	}
}
