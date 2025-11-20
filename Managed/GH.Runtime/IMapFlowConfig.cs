using UnityEngine;

public class IMapFlowConfig : ScriptableObject
{
	[Header("Camera Options")]
	[Range(0f, 1f)]
	public float ZoomInPercentOnPartyMove = 0.1f;

	public float IncreaseZoomToPartyToken = 40f;

	[Header("Movement")]
	public float PartyMovementSpeed = 5f;

	public float DelayToStartMoving = 1f;
}
