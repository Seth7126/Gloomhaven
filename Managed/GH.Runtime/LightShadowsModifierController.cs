#define ENABLE_LOGS
using System.Collections.Generic;
using System.Linq;

public class LightShadowsModifierController : Singleton<LightShadowsModifierController>
{
	private HashSet<LightShadowsModifier> _lightShadowsModifiers;

	private Dictionary<ProceduralMapTile, bool> _proceduralMapTiles;

	protected override void Awake()
	{
		base.Awake();
		_lightShadowsModifiers = new HashSet<LightShadowsModifier>();
		_proceduralMapTiles = new Dictionary<ProceduralMapTile, bool>();
		RoomVisibilityTracker.ProceduralMapTileVisibilityStateChanged += OnProceduralMapTileVisibilityStateChanged;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		RoomVisibilityTracker.ProceduralMapTileVisibilityStateChanged -= OnProceduralMapTileVisibilityStateChanged;
	}

	public void RegisterLightShadowModifier(LightShadowsModifier lightShadowsModifier)
	{
		Debug.LogWarning("Registered new light shadow modifier!");
		if (_lightShadowsModifiers.Contains(lightShadowsModifier))
		{
			Debug.LogWarning(lightShadowsModifier.name + " already registered!");
			return;
		}
		_lightShadowsModifiers.Add(lightShadowsModifier);
		HandleInternalLightShadowsAdded(lightShadowsModifier);
	}

	public void UnregisterLightShadowModifier(LightShadowsModifier lightShadowsModifier)
	{
		if (_lightShadowsModifiers.Contains(lightShadowsModifier))
		{
			_lightShadowsModifiers.Remove(lightShadowsModifier);
		}
	}

	private void OnProceduralMapTileVisibilityStateChanged(ProceduralMapTile proceduralMapTile, bool isShow)
	{
		_proceduralMapTiles[proceduralMapTile] = isShow;
		HandleInternalMapTileStateChanged();
	}

	private void HandleInternalLightShadowsAdded(LightShadowsModifier lightShadowsModifier)
	{
		int num = _proceduralMapTiles.Count((KeyValuePair<ProceduralMapTile, bool> x) => x.Value);
		int maxOpenedRoomsWithLight = PlatformLayer.Setting.GetMaxOpenedRoomsWithLight();
		if (num > maxOpenedRoomsWithLight)
		{
			lightShadowsModifier.DisableShadows();
		}
		else
		{
			lightShadowsModifier.ReinitShadows();
		}
	}

	private void HandleInternalMapTileStateChanged()
	{
		Debug.LogWarning("[LightShadowsModifierController]: HandleInternalMapTileStateChanged ");
		int num = _proceduralMapTiles.Count((KeyValuePair<ProceduralMapTile, bool> x) => x.Value);
		int maxOpenedRoomsWithLight = PlatformLayer.Setting.GetMaxOpenedRoomsWithLight();
		if (num > maxOpenedRoomsWithLight)
		{
			Debug.LogWarning("[LightShadowsModifierController]: Light Shadows are disabled! ");
			{
				foreach (LightShadowsModifier lightShadowsModifier in _lightShadowsModifiers)
				{
					lightShadowsModifier.DisableShadows();
				}
				return;
			}
		}
		Debug.LogWarning("[LightShadowsModifierController]: Light Shadows are reinited! ");
		foreach (LightShadowsModifier lightShadowsModifier2 in _lightShadowsModifiers)
		{
			lightShadowsModifier2.ReinitShadows();
		}
	}
}
