using System.Linq;
using Apparance.Net;
using UnityEngine;

[RequireComponent(typeof(ApparanceEntity))]
[RequireComponent(typeof(ProceduralStyle))]
[ExecuteInEditMode]
public class ProceduralDoorway : ProceduralBase, IProceduralContent
{
	private ParameterCollection Parameters;

	private ProceduralMapTile.Visibility currentVisibility = ProceduralMapTile.Visibility.All;

	private bool visibilityOverride;

	public void OverrideVisibility(ProceduralMapTile.Visibility visibility)
	{
		visibilityOverride = true;
		currentVisibility = visibility;
		ApplyVisibility();
	}

	public void SetVisibility(ProceduralMapTile.Visibility visibility)
	{
		if (!visibilityOverride)
		{
			currentVisibility = visibility;
		}
		ApplyVisibility();
	}

	private void ApplyVisibility()
	{
		bool show_preview = currentVisibility == ProceduralMapTile.Visibility.Preview || currentVisibility == ProceduralMapTile.Visibility.PreviewWithDoors;
		bool show_full = currentVisibility == ProceduralMapTile.Visibility.All || currentVisibility == ProceduralMapTile.Visibility.PreviewWithDoors;
		ProceduralMapTile.ShowContent(base.gameObject, show_full, show_preview);
		GameObject gameObject = null;
		UnityGameEditorDoorProp unityGameEditorDoorProp = base.gameObject.FindInImmediateChildren<UnityGameEditorDoorProp>().FirstOrDefault();
		if (unityGameEditorDoorProp != null)
		{
			gameObject = unityGameEditorDoorProp.gameObject;
		}
		else
		{
			UnityGameEditorDoorProp unityGameEditorDoorProp2 = base.gameObject.transform.parent?.gameObject.GetComponent<UnityGameEditorDoorProp>();
			if (unityGameEditorDoorProp2 != null)
			{
				gameObject = unityGameEditorDoorProp2.gameObject;
			}
		}
		if (gameObject != null)
		{
			ProceduralMapTile.ShowContent(gameObject, show_full, show_preview);
		}
	}

	private void Start()
	{
		Rebuild();
		Apply(force_build: false);
	}

	private void Rebuild()
	{
		Parameters = ParameterCollection.CreateEmpty();
		Parameters.BeginWrite();
		int scenarioSeed = GetScenarioSeed();
		ProceduralStyle frontStyle = GetFrontStyle();
		int num = ((frontStyle != null) ? frontStyle.Seed : scenarioSeed);
		ParameterCollection style_struct = Parameters.WriteListBegin(3);
		if (FindParentStyle(isFront: true) != null)
		{
			frontStyle.WriteStyleParameters(style_struct, scenarioSeed, num, num);
		}
		Parameters.WriteListEnd();
		ProceduralStyle backStyle = GetBackStyle();
		int num2 = ((backStyle != null) ? backStyle.Seed : scenarioSeed);
		ParameterCollection style_struct2 = Parameters.WriteListBegin(4);
		if (FindParentStyle(isFront: false) != null)
		{
			backStyle.WriteStyleParameters(style_struct2, scenarioSeed, num2, num2);
		}
		Parameters.WriteListEnd();
		UnityEngine.Vector3 v = FindWallSize(isFront: true);
		Parameters.WriteVector3(Conversion.AVfromUV(v), 5);
		UnityEngine.Vector3 v2 = FindWallSize(isFront: false);
		Parameters.WriteVector3(Conversion.AVfromUV(v2), 6);
		Parameters.EndWrite();
	}

	private void Apply(bool force_build = true)
	{
		ApparanceEntity component = GetComponent<ApparanceEntity>();
		component.PartialParameterOverride = Parameters;
		component.IsPopulated = false;
		component.NotifyPropertyChanged();
		component.IsPopulated = true;
		component.NotifyPropertyChanged();
	}

	void IProceduralContent.RebuildContent()
	{
		Rebuild();
		Apply();
	}

	internal void CheckStyleChanges(bool invalidate_all)
	{
		GetFrontStyle()?.CheckChanges(invalidate_all);
		GetBackStyle()?.CheckChanges(invalidate_all);
	}

	private ProceduralStyle GetFrontStyle()
	{
		ProceduralStyle[] components = GetComponents<ProceduralStyle>();
		if (components.Length >= 1)
		{
			return components[0];
		}
		Debug.LogError("Doorway '" + base.name + "' (at " + base.transform.position.ToString() + ") has no ProceduralStyle components, doorways expect two.");
		return null;
	}

	private ProceduralStyle GetBackStyle()
	{
		ProceduralStyle[] components = GetComponents<ProceduralStyle>();
		if (components.Length >= 2)
		{
			return components[1];
		}
		Debug.LogError("Doorway '" + base.name + "' (at " + base.transform.position.ToString() + ") has only one ProceduralStyle component, doorways expect two.");
		return null;
	}

	private UnityEngine.Vector3 GetFrontPosition()
	{
		GameObject gameObject = base.gameObject.FindInImmediateChildren("outFront");
		if (gameObject != null)
		{
			return gameObject.transform.position;
		}
		return base.transform.position;
	}

	private UnityEngine.Vector3 GetBackPosition()
	{
		GameObject gameObject = base.gameObject.FindInImmediateChildren("outBack");
		if (gameObject != null)
		{
			return gameObject.transform.position;
		}
		return base.transform.position;
	}

	private ProceduralMapTile FindMapTileByPosition(UnityEngine.Vector3 position, Color diags_colour, out UnityEngine.Vector3 wall_space)
	{
		ProceduralTile component = GetComponent<ProceduralTile>();
		if (component != null)
		{
			return component.FindMapTileByPosition(position, diags_colour, out wall_space);
		}
		wall_space = UnityEngine.Vector3.zero;
		return null;
	}

	public ProceduralStyle FindParentStyle(bool isFront)
	{
		UnityEngine.Vector3 position = ((!isFront) ? GetBackPosition() : GetFrontPosition());
		UnityEngine.Vector3 wall_space;
		return FindMapTileByPosition(position, Color.black, out wall_space)?.GetComponent<ProceduralStyle>();
	}

	public ProceduralMapTile GetFrontMapTile()
	{
		UnityEngine.Vector3 wall_space;
		return FindMapTileByPosition(GetFrontPosition(), Color.blue, out wall_space);
	}

	public ProceduralMapTile GetBackMapTile()
	{
		UnityEngine.Vector3 wall_space;
		return FindMapTileByPosition(GetBackPosition(), Color.blue, out wall_space);
	}

	private UnityEngine.Vector3 FindWallSize(bool isFront)
	{
		UnityEngine.Vector3 position = ((!isFront) ? GetBackPosition() : GetFrontPosition());
		FindMapTileByPosition(position, Color.black, out var wall_space);
		return wall_space;
	}
}
