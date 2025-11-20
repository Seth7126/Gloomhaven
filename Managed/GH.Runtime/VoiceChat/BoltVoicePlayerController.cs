using Photon.Bolt;
using UnityEngine;

namespace VoiceChat;

public class BoltVoicePlayerController : EntityBehaviour<IVoicePlayer>
{
	[SerializeField]
	private float Speed = 4f;

	private bool _isSpeakSetup;

	private const string VoiceAreaTag = "GameController";

	public override void Attached()
	{
		base.state.SetTransforms(base.state.Transform, base.transform);
		GetComponent<AudioListener>().enabled = base.entity.IsOwner;
	}

	private void Update()
	{
		SetupSpeaker();
	}

	public override void SimulateOwner()
	{
	}

	private void SetupSpeaker()
	{
		if (!_isSpeakSetup)
		{
			if (base.entity.IsOwner && base.state.VoicePlayerID == 0 && BoltVoiceBridge.Instance != null && BoltVoiceBridge.Instance.LocalPlayerID != -1)
			{
				base.state.VoicePlayerID = BoltVoiceBridge.Instance.LocalPlayerID;
			}
			if (BoltVoiceBridge.Instance != null && BoltVoiceBridge.Instance.GetSpeaker(base.state.VoicePlayerID, out var speaker) && speaker.transform.parent == null)
			{
				speaker.transform.parent = base.transform;
				speaker.transform.localPosition = Vector3.zero;
				_isSpeakSetup = true;
			}
		}
	}
}
