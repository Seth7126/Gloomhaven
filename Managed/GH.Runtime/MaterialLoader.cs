using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MaterialLoader : MonoBehaviour
{
	[SerializeField]
	public List<MaterialLoaderData> LoadersData = new List<MaterialLoaderData>();

	private async void Start()
	{
		if (GetComponent<DetailsDisabler>() != null)
		{
			await Task.Yield();
		}
		LoadMaterials();
	}

	private void OnDestroy()
	{
		foreach (MaterialLoaderData loadersDatum in LoadersData)
		{
			loadersDatum.Release();
		}
	}

	public void LoadMaterials()
	{
		List<MaterialLoaderData> list = new List<MaterialLoaderData>();
		foreach (MaterialLoaderData loadersDatum in LoadersData)
		{
			if (loadersDatum.Renderer == null)
			{
				UnityEngine.Debug.LogError("Renderer component is null in MaterialLoaderData. Object " + base.gameObject.name);
			}
			else if (loadersDatum.Renderer.gameObject.activeInHierarchy || loadersDatum.ForceLoad)
			{
				list.Add(loadersDatum);
			}
		}
		list.ForEach(delegate(MaterialLoaderData item)
		{
			item.LoadMaterials();
		});
	}
}
