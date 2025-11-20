using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnimateLoadingIcon : MonoBehaviour
{
	[Header("Prefab References")]
	[SerializeField]
	private GameObject m_LoadingIconBase;

	[SerializeField]
	private GameObject m_LoadingIconOverlay;

	[Header("Animation Settings")]
	[SerializeField]
	private float m_IconSpinSpeed;

	[SerializeField]
	private float m_IconGlowSpeed;

	[SerializeField]
	private float m_IconUpdateSpeed;

	[SerializeField]
	private float m_IconOverlayMinAlpha;

	private RectTransform m_IconRect;

	private RectTransform m_IconOverlayRect;

	private Image m_IconOverlayImage;

	private bool m_IncreaseAlpha;

	private string m_loadingHintPrefix;

	private int m_loadingHintsCount;

	private bool m_isAnimate;

	private WaitForSecondsRealtime _waitForSeconds;

	private Coroutine m_AnimateIconCoroutine;

	private void Awake()
	{
		_waitForSeconds = new WaitForSecondsRealtime(m_IconUpdateSpeed);
		m_IconRect = m_LoadingIconBase.GetComponent<RectTransform>();
		m_IconOverlayRect = m_LoadingIconOverlay.GetComponent<RectTransform>();
		m_IconOverlayImage = m_LoadingIconOverlay.GetComponent<Image>();
	}

	private void OnEnable()
	{
		m_isAnimate = true;
		m_IconOverlayImage.color = new Color(m_IconOverlayImage.color.r, m_IconOverlayImage.color.g, m_IconOverlayImage.color.b, m_IconOverlayMinAlpha);
		m_AnimateIconCoroutine = StartCoroutine(AnimateIcon());
	}

	private void OnDisable()
	{
		if (m_AnimateIconCoroutine != null)
		{
			StopCoroutine(m_AnimateIconCoroutine);
			m_AnimateIconCoroutine = null;
		}
	}

	private IEnumerator AnimateIcon()
	{
		while (true)
		{
			m_IconRect.Rotate(new Vector3(0f, 0f, m_IconSpinSpeed));
			m_IconOverlayRect.Rotate(new Vector3(0f, 0f, m_IconSpinSpeed));
			if (m_IconOverlayImage.color.a >= 1f)
			{
				m_IncreaseAlpha = false;
			}
			else if (m_IconOverlayImage.color.a <= m_IconOverlayMinAlpha)
			{
				m_IncreaseAlpha = true;
			}
			if (m_IncreaseAlpha)
			{
				m_IconOverlayImage.color = new Color(m_IconOverlayImage.color.r, m_IconOverlayImage.color.g, m_IconOverlayImage.color.b, Math.Min(1f, m_IconOverlayImage.color.a + m_IconGlowSpeed));
			}
			else
			{
				m_IconOverlayImage.color = new Color(m_IconOverlayImage.color.r, m_IconOverlayImage.color.g, m_IconOverlayImage.color.b, Math.Max(m_IconOverlayMinAlpha, m_IconOverlayImage.color.a - m_IconGlowSpeed));
			}
			yield return _waitForSeconds;
		}
	}
}
