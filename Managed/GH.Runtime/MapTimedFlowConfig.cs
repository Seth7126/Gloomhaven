using UnityEngine;

[CreateAssetMenu(menuName = "UI Config/Map Time flow", fileName = "Map Time Flow Config")]
public class MapTimedFlowConfig : IMapFlowConfig
{
	public float DelayToEncounter = 2f;

	public float DelayToFadeInBlack = 2f;

	public float DelayToArriveDestination = 2f;

	[Range(0f, 1f)]
	[Tooltip("Percent of the way that party must move  befor starting to fade black")]
	public float PartyPercentMoveToStartFadeInBlack = 0.1f;

	[Range(0f, 1f)]
	[Tooltip("Percent of the way that party must move  befor starting to fade black")]
	public float PartyPercentMoveToFinishFadeOutBlack = 0.9f;

	[Tooltip("Seconds screen is black between a fade in and fade out to black")]
	public float PartyTravelFadedBlackDuration = 2f;

	public float PartyMinStartTravelDistance = 5f;

	[Header("SFX")]
	public string StartMovingAudioItem = "PlaySound_UITravelCartStartDrum";

	public string MovingAudioItem = "PlaySound_UITravelCartLoop";

	public float FadeOutMovingAudioItemDuration = 0.2f;

	public string ReachDestinationAudioItem = "PlaySound_UITravelReachLocation";

	public float PlayReachDestinationSFXTimeOffset = 0.7f;
}
