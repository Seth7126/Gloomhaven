using UnityEngine;

public class FootstepSound : MonoBehaviour
{
	[AudioEventName]
	[SerializeField]
	private string m_FootstepAudioEvent;

	[SerializeField]
	private GameObject m_PFXPrefab;

	[SerializeField]
	[Tooltip("0 - Left Foot, 1 - Right Foot, 2+ everything else for weird creatures")]
	private Transform[] m_FeetArray;

	public void MakeFootstepSound(AnimationEvent animEvent)
	{
		if (animEvent.animatorClipInfo.weight < 0.15f)
		{
			return;
		}
		if (!string.IsNullOrEmpty(m_FootstepAudioEvent))
		{
			AudioController.Play(m_FootstepAudioEvent, base.gameObject);
		}
		if (m_PFXPrefab == null || m_FeetArray.Length < animEvent.intParameter)
		{
			Debug.LogError("Entry Missing for character " + base.gameObject.name + ", PFX prefab is not set or foot index more then size of FeetArray");
			return;
		}
		if (animEvent.intParameter <= 0)
		{
			Debug.LogError("Foot index in AnimEvent is below 1");
			return;
		}
		GameObject gameObject = ObjectPool.Spawn(m_PFXPrefab, m_FeetArray[animEvent.intParameter - 1]);
		if (gameObject != null)
		{
			ObjectPool.Recycle(gameObject, VFXShared.GetEffectLifetime(m_PFXPrefab), m_PFXPrefab);
		}
	}
}
