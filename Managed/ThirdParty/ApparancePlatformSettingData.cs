using UnityEngine;

[CreateAssetMenu(fileName = "Apparance Platform Setting Data", menuName = "Data/Platform Settings/Apparance Platform Setting Data")]
public class ApparancePlatformSettingData : ScriptableObject
{
	public int _qualityLevel = 1;

	public bool _disableWallClutterGeneration;

	public bool _disableWallTorchesGeneration;

	public bool _showUnderground = true;

	public bool _disableSurfaceFeaturesGeneration;

	public DetailsDisablingLevel _detailsDisablingLevel;
}
