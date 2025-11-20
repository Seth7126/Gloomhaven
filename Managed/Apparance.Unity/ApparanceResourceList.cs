using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Resource List", menuName = "Apparance/Resource List", order = 1)]
public class ApparanceResourceList : ScriptableObject
{
	[HideInInspector]
	public string Category;

	public List<ApparanceObjectResource> Objects = new List<ApparanceObjectResource>();

	private void OnEnable()
	{
		RefreshNames();
	}

	private int CountVariantsOf(ApparanceObjectResource r)
	{
		int num = (Enumerable.Contains(r.ID, '#') ? r.ID.IndexOf('#') : r.ID.Length);
		int num2 = 0;
		foreach (ApparanceObjectResource @object in Objects)
		{
			if (@object.IsVariant && r.Object != null && (Enumerable.Contains(@object.ID, '#') ? @object.ID.IndexOf('#') : @object.ID.Length) == num && string.Compare(@object.ID, 0, r.ID, 0, num, ignoreCase: true) == 0)
			{
				num2++;
			}
		}
		return num2;
	}

	private void RefreshNames()
	{
		foreach (ApparanceObjectResource @object in Objects)
		{
			_ = @object.Name;
			@object.UpdateName(Category);
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		foreach (ApparanceObjectResource object2 in Objects)
		{
			if (object2.IsVariant)
			{
				object2.Variants = CountVariantsOf(object2);
				dictionary[object2.ID] = null;
			}
		}
		foreach (string key in dictionary.Keys)
		{
			int num = 1;
			for (int i = 0; i < Objects.Count; i++)
			{
				ApparanceObjectResource apparanceObjectResource = Objects[i];
				if (apparanceObjectResource.IsVariant && apparanceObjectResource.Object != null && string.Compare(apparanceObjectResource.ID, key) == 0)
				{
					apparanceObjectResource.Variant = num;
					num++;
				}
			}
		}
	}

	private string StripCategory(string full_name)
	{
		if (!string.IsNullOrWhiteSpace(full_name))
		{
			if (string.IsNullOrWhiteSpace(Category) || !full_name.StartsWith(Category) || (full_name.Length != Category.Length && full_name[Category.Length] != '.'))
			{
				return full_name;
			}
			return full_name.Substring(Category.Length + 1);
		}
		return "";
	}

	internal ApparanceObjectResource FindExternalAsset(string full_name, string description)
	{
		bool flag = StripCategory(full_name) != full_name;
		bool flag2 = !string.IsNullOrWhiteSpace(Category);
		if (flag || !flag2)
		{
			ApparanceObjectResource apparanceObjectResource = Objects.Where((ApparanceObjectResource o) => string.Compare(o.Name, full_name, ignoreCase: true) == 0).FirstOrDefault();
			if (apparanceObjectResource != null)
			{
				return apparanceObjectResource;
			}
		}
		if (flag)
		{
			ApparanceObjectResource apparanceObjectResource2 = new ApparanceObjectResource();
			apparanceObjectResource2.Name = full_name;
			apparanceObjectResource2.Description = description;
			apparanceObjectResource2.Object = null;
			apparanceObjectResource2.UpdateName(Category);
			Objects.Add(apparanceObjectResource2);
			return apparanceObjectResource2;
		}
		return null;
	}

	public string GetCategory()
	{
		return Category;
	}

	public void SetCategory(string new_category)
	{
		Category = new_category;
		foreach (ApparanceObjectResource @object in Objects)
		{
			@object.UpdateName(Category);
		}
	}

	public void RefreshResourceList(bool clear_unused)
	{
		if (clear_unused)
		{
			Objects.RemoveAll((ApparanceObjectResource r) => r.Object == null);
		}
		RefreshNames();
		ApparanceEngine.Instance.Resources.RefreshResourceList(clear_unused);
	}
}
