using ClockStone;
using UnityEngine;

public class SFXOnEnable : MonoBehaviour
{
	[AudioEventName]
	[SerializeField]
	private string m_AudioEvent;

	[SerializeField]
	private bool m_PlayOnce;

	[SerializeField]
	private bool m_StopOnDisable;

	private Transform m_Transform;

	private bool m_Played;

	private AudioObject m_AudioObject;

	private float m_startTime;

	private bool m_checkDelay;

	public float delay;

	private void Awake()
	{
		m_Transform = base.transform;
		if (m_PlayOnce)
		{
			m_Played = false;
		}
	}

	private void OnEnable()
	{
		if (!string.IsNullOrEmpty(m_AudioEvent) && GetComponentInParent<ObjectPool>() == null && (!m_PlayOnce || (m_PlayOnce && !m_Played)))
		{
			m_startTime = Time.time;
			m_checkDelay = true;
		}
	}

	private void OnDisable()
	{
		if (m_StopOnDisable && m_AudioObject != null)
		{
			m_AudioObject.Stop();
			m_AudioObject = null;
			m_startTime = 0f;
		}
	}

	private void Update()
	{
		if (m_checkDelay && Time.time - m_startTime > delay)
		{
			m_checkDelay = false;
			PlaySound();
		}
	}

	private void PlaySound()
	{
		m_AudioObject = AudioController.Play(m_AudioEvent, m_Transform, null, attachToParent: false);
		if (m_PlayOnce)
		{
			m_Played = true;
		}
	}
}
