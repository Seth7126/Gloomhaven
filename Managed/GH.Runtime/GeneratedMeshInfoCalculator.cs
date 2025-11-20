#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SM.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityMeshSimplifier;

public static class GeneratedMeshInfoCalculator
{
	private static string _sceneName = "ProcGen";

	private static string _contentName = "Generated Content";

	private static string _path = "pathToMeshInfoFolder";

	public static async void ChangeMeshesByCube()
	{
		Scene sceneByName = SceneManager.GetSceneByName(_sceneName);
		if (!sceneByName.IsValid())
		{
			LogUtils.LogError("There is no scene " + _sceneName);
			return;
		}
		GameObject go = sceneByName.GetRootGameObjects().First((GameObject o) => o.GetComponent<ProceduralScenario>() != null);
		MeshFilter component = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshFilter>();
		List<GameObject> list = (from content in go.FindAllInChildren(_contentName)
			where content.activeInHierarchy
			select content).ToList();
		List<MeshFilter> list2 = new List<MeshFilter>();
		foreach (GameObject item in list)
		{
			list2.AddRange(from mesh in item.GetComponentsInChildren<MeshFilter>()
				where mesh.GetComponent<MeshRenderer>() != null && mesh.GetComponent<MeshRenderer>().enabled && mesh.gameObject.activeInHierarchy
				select mesh);
		}
		LogUtils.Log("Start changing");
		foreach (MeshFilter item2 in list2)
		{
			item2.mesh = component.mesh;
		}
		await Task.Delay(TimeSpan.FromSeconds(1.0));
		Resources.UnloadUnusedAssets();
		LogUtils.Log("Stop changing");
	}

	public static async void SimplifyModelToLimit(float quality)
	{
		if (!SceneManager.GetSceneByName(_sceneName).IsValid())
		{
			LogUtils.LogError("There is no scene " + _sceneName);
			return;
		}
		LogUtils.Log("Start changing");
		MeshFilter[] array = UnityEngine.Object.FindObjectsOfType<MeshFilter>(includeInactive: true);
		Dictionary<Mesh, List<MeshFilter>> dictionary = new Dictionary<Mesh, List<MeshFilter>>();
		for (int i = 0; i < array.Length; i++)
		{
			if (!(array[i].mesh == null) && array[i].mesh.isReadable && array[i].mesh.vertexCount >= 80)
			{
				if (dictionary.ContainsKey(array[i].mesh))
				{
					dictionary[array[i].mesh].Add(array[i]);
					continue;
				}
				dictionary.Add(array[i].mesh, new List<MeshFilter> { array[i] });
			}
		}
		MeshSimplifier simplifier = new MeshSimplifier();
		foreach (KeyValuePair<Mesh, List<MeshFilter>> item in dictionary)
		{
			simplifier.Initialize(item.Key);
			simplifier.SimplifyMesh(quality);
			Mesh mesh = simplifier.ToMesh();
			foreach (MeshFilter item2 in item.Value)
			{
				Debug.Log(item2.gameObject);
				item2.mesh = mesh;
			}
			await Task.Delay(TimeSpan.FromSeconds(0.009999999776482582));
		}
		SkinnedMeshRenderer[] array2 = UnityEngine.Object.FindObjectsOfType<SkinnedMeshRenderer>(includeInactive: true);
		Dictionary<Mesh, List<SkinnedMeshRenderer>> dictionary2 = new Dictionary<Mesh, List<SkinnedMeshRenderer>>();
		for (int j = 0; j < array2.Length; j++)
		{
			if (!(array2[j].sharedMesh == null) && array2[j].sharedMesh.isReadable)
			{
				if (dictionary2.ContainsKey(array2[j].sharedMesh))
				{
					dictionary2[array2[j].sharedMesh].Add(array2[j]);
					continue;
				}
				dictionary2.Add(array2[j].sharedMesh, new List<SkinnedMeshRenderer> { array2[j] });
			}
		}
		foreach (KeyValuePair<Mesh, List<SkinnedMeshRenderer>> item3 in dictionary2)
		{
			simplifier.Initialize(item3.Key);
			simplifier.SimplifyMesh(quality);
			Mesh sharedMesh = simplifier.ToMesh();
			foreach (SkinnedMeshRenderer item4 in item3.Value)
			{
				Debug.Log(item4.gameObject);
				item4.sharedMesh = sharedMesh;
			}
			await Task.Delay(TimeSpan.FromSeconds(0.009999999776482582));
		}
		Resources.UnloadUnusedAssets();
		LogUtils.Log("Stop changing");
	}
}
