using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using Script.Controller;
using UnityEngine;

public class LevelEditorApparancePanel : MonoBehaviour
{
	public LevelEditorApparanceOverridePanel ApparanceOverridePanel;

	private void OnEnable()
	{
		InitUI();
	}

	public void InitUI()
	{
		ApparanceOverridePanel?.SetScenarioDisplayed();
	}

	public void OnResetAllChildrenToInherit()
	{
		int objectCount;
		foreach (CMap map2 in ScenarioManager.CurrentScenarioState.Maps)
		{
			GameObject map = Singleton<ObjectCacheService>.Instance.GetMap(map2);
			if (map != null)
			{
				CApparanceOverrideDetails cApparanceOverrideDetails = new CApparanceOverrideDetails(map2.MapGuid);
				ProceduralStyle component = map.GetComponent<ProceduralStyle>();
				if (component != null)
				{
					cApparanceOverrideDetails.OverrideSeed = component.Seed;
					component.Biome = ScenarioPossibleRoom.EBiome.Inherit;
					component.SubBiome = ScenarioPossibleRoom.ESubBiome.Inherit;
					component.Theme = ScenarioPossibleRoom.ETheme.Inherit;
					component.SubTheme = ScenarioPossibleRoom.ESubTheme.Inherit;
					component.Tone = ScenarioPossibleRoom.ETone.Inherit;
					component.FetchObjectHierarchyValues(out var objectIndex, out objectCount);
					cApparanceOverrideDetails.OverrideSiblingIndex = objectIndex;
					component.ForceValidate();
				}
				LevelEditorController.s_Instance.AddOrReplaceApparanceOverride(cApparanceOverrideDetails);
				ProceduralWall[] componentsInChildren = map.GetComponentsInChildren<ProceduralWall>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					CApparanceOverrideDetails cApparanceOverrideDetails2 = new CApparanceOverrideDetails(CApparanceOverrideDetails.GetWallGUID(map2, i));
					ProceduralStyle component2 = componentsInChildren[i].GetComponent<ProceduralStyle>();
					component2.Biome = ScenarioPossibleRoom.EBiome.Inherit;
					component2.SubBiome = ScenarioPossibleRoom.ESubBiome.Inherit;
					component2.Theme = ScenarioPossibleRoom.ETheme.Inherit;
					component2.SubTheme = ScenarioPossibleRoom.ESubTheme.Inherit;
					component2.Tone = ScenarioPossibleRoom.ETone.Inherit;
					component2.FetchObjectHierarchyValues(out var objectIndex2, out objectCount);
					cApparanceOverrideDetails2.OverrideSiblingIndex = objectIndex2;
					component2.ForceValidate();
					LevelEditorController.s_Instance.AddOrReplaceApparanceOverride(cApparanceOverrideDetails2);
				}
			}
		}
		List<UnityGameEditorObject> list = new List<UnityGameEditorObject>();
		foreach (GameObject item in UnityGameEditorRuntime.FindUnityGameObjects(LevelEditorController.s_Instance.MapSceneRoot, ScenarioManager.ObjectImportType.Obstacle))
		{
			UnityGameEditorObject component3 = item.GetComponent<UnityGameEditorObject>();
			if (component3 != null)
			{
				list.Add(component3);
			}
		}
		foreach (GameObject item2 in UnityGameEditorRuntime.FindUnityGameObjects(LevelEditorController.s_Instance.MapSceneRoot, ScenarioManager.ObjectImportType.Door))
		{
			UnityGameEditorObject component4 = item2.GetComponent<UnityGameEditorObject>();
			if (component4 != null)
			{
				list.Add(component4);
			}
		}
		foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
		{
			CApparanceOverrideDetails cApparanceOverrideDetails3 = new CApparanceOverrideDetails(prop.PropGuid);
			UnityGameEditorObject unityGameEditorObject = list.FirstOrDefault((UnityGameEditorObject p) => p != null && p.PropObject != null && p.PropObject.PropGuid == prop.PropGuid);
			if (unityGameEditorObject != null)
			{
				ProceduralStyle component5 = unityGameEditorObject.GetComponent<ProceduralStyle>();
				if (component5 != null)
				{
					cApparanceOverrideDetails3.OverrideSeed = component5.Seed;
					component5.Biome = ScenarioPossibleRoom.EBiome.Inherit;
					component5.SubBiome = ScenarioPossibleRoom.ESubBiome.Inherit;
					component5.Theme = ScenarioPossibleRoom.ETheme.Inherit;
					component5.SubTheme = ScenarioPossibleRoom.ESubTheme.Inherit;
					component5.Tone = ScenarioPossibleRoom.ETone.Inherit;
					component5.FetchObjectHierarchyValues(out var objectIndex3, out objectCount);
					cApparanceOverrideDetails3.OverrideSiblingIndex = objectIndex3;
					component5.ForceValidate();
				}
				LevelEditorController.s_Instance.AddOrReplaceApparanceOverride(cApparanceOverrideDetails3);
			}
		}
	}

	public void EnsureAllStylesHaveAnOverride(bool overrideExistingWithCurrentStyleSettings = false)
	{
		foreach (CMap map2 in ScenarioManager.CurrentScenarioState.Maps)
		{
			GameObject map = Singleton<ObjectCacheService>.Instance.GetMap(map2);
			if (map != null)
			{
				ProceduralStyle component = map.GetComponent<ProceduralStyle>();
				if (component != null)
				{
					LevelEditorController.s_Instance.EnsureApparanceOverrideExistsForGuidAndStyle(map2.MapGuid, component, overrideExistingWithCurrentStyleSettings);
				}
				ProceduralWall[] componentsInChildren = map.GetComponentsInChildren<ProceduralWall>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					LevelEditorController.s_Instance.EnsureApparanceOverrideExistsForGuidAndStyle(CApparanceOverrideDetails.GetWallGUID(map2, i), componentsInChildren[i].GetComponent<ProceduralStyle>(), overrideExistingWithCurrentStyleSettings);
				}
			}
		}
		List<UnityGameEditorObject> list = new List<UnityGameEditorObject>();
		foreach (GameObject item in UnityGameEditorRuntime.FindUnityGameObjects(LevelEditorController.s_Instance.MapSceneRoot, ScenarioManager.ObjectImportType.Obstacle))
		{
			UnityGameEditorObject component2 = item.GetComponent<UnityGameEditorObject>();
			if (component2 != null)
			{
				list.Add(component2);
			}
		}
		foreach (GameObject item2 in UnityGameEditorRuntime.FindUnityGameObjects(LevelEditorController.s_Instance.MapSceneRoot, ScenarioManager.ObjectImportType.Door))
		{
			UnityGameEditorObject component3 = item2.GetComponent<UnityGameEditorObject>();
			if (component3 != null)
			{
				list.Add(component3);
			}
		}
		foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
		{
			UnityGameEditorObject unityGameEditorObject = list.FirstOrDefault((UnityGameEditorObject p) => p != null && p.PropObject != null && p.PropObject.PropGuid == prop.PropGuid);
			if (unityGameEditorObject != null)
			{
				ProceduralStyle component4 = unityGameEditorObject.GetComponent<ProceduralStyle>();
				if (component4 != null)
				{
					LevelEditorController.s_Instance.EnsureApparanceOverrideExistsForGuidAndStyle(prop.PropGuid, component4, overrideExistingWithCurrentStyleSettings);
				}
			}
		}
	}

	public void EnsureAllOverrideConsistency()
	{
		EnsureAllStylesHaveAnOverride(overrideExistingWithCurrentStyleSettings: true);
	}
}
