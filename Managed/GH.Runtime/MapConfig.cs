using UnityEngine;

[CreateAssetMenu(menuName = "UI Config/Map/Map Config", fileName = "MapConfig")]
public class MapConfig : ScriptableObject
{
	public float CameraMoveSpeed = 50f;

	public float ZoomOutExtraHeight = 20f;

	public float DefaultFOV = 75f;

	public float MinimumFOV = 30f;

	public float MaxFOV = 75f;

	public float ZoomWheelSpeed = 50f;

	public Bounds CameraBounds;

	[Header("Campaign Only")]
	public float ZoomAmountExit = 10f;

	public float ZoomAmountEnter = 10f;

	[Header("Markers")]
	[Tooltip("Max zoom where the map markers will be visible. If we reach or exceed this zoom, the map markers will hide")]
	public float VisibleMapMarkersFOV = 70f;

	[Header("Focus")]
	public float FocusLocationTime = 0.5f;

	public float FocusLocationZoom;
}
