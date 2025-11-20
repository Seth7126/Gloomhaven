using UnityEngine;

[CreateAssetMenu(menuName = "UI Config/Map Points flow", fileName = "Map Point Flow Config")]
public class MapPointsFlowConfig : IMapFlowConfig
{
	[Range(0f, 1f)]
	public float PercentMovedToEncounter = 0.5714286f;

	[Header("SFX")]
	public string MovingAudioItem = "PlaySound_UITravelCartLoop";

	public float FadeOutMovingAudioItemDuration = 0.2f;

	public string ReachDestinationAudioItem = "PlaySound_UITravelReachLocation";

	public float PlayReachDestinationSFXTimeOffset = 0.7f;
}
