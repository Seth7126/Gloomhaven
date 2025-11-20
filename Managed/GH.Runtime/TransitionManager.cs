using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{
	public static TransitionManager s_Instance;

	[SerializeField]
	private RectTransform m_FadeInPanel;

	[SerializeField]
	private RectTransform m_LoadingSplashScreen;

	[SerializeField]
	[Tooltip("Min amount of frames we wait before start transitions, it's needed to awoid hitches during scene load")]
	private float m_MinTransitionFrames;

	[SerializeField]
	private float m_TransitionTime;

	[SerializeField]
	private float m_TransitionDelay;

	private Graphic fadeGraphic;

	public bool TransitionDone { get; private set; }

	public event BasicEventHandler OnTransitionFinished
	{
		add
		{
			onTransitionFinished += value;
		}
		remove
		{
			onTransitionFinished -= value;
		}
	}

	private event BasicEventHandler onTransitionFinished;

	[UsedImplicitly]
	private void Awake()
	{
		s_Instance = this;
		fadeGraphic = m_FadeInPanel.GetComponent<Graphic>();
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		s_Instance = null;
		this.onTransitionFinished = null;
	}

	public void FadeInLevel()
	{
		if (LevelEditorController.s_Instance.IsEditing || AutoTestController.s_AutoTestCurrentlyLoaded)
		{
			m_FadeInPanel.gameObject.SetActive(value: false);
			TransitionDone = true;
			this.onTransitionFinished?.Invoke();
			this.onTransitionFinished = null;
			return;
		}
		InputManager.RequestDisableInput(this, EKeyActionTag.All);
		onTransitionFinished += delegate
		{
			InputManager.RequestEnableInput(this, EKeyActionTag.All);
		};
		TransitionDone = false;
		StartCoroutine(FadeOutCoroutine());
	}

	private IEnumerator FadeOutCoroutine()
	{
		m_FadeInPanel.gameObject.SetActive(value: true);
		Image fadePanelImage = m_FadeInPanel.gameObject.GetComponent<Image>();
		if (fadePanelImage != null)
		{
			fadePanelImage.raycastTarget = true;
		}
		WaitForEndOfFrame waitForEndOfFrameDelay = new WaitForEndOfFrame();
		for (int i = 0; (float)i < m_MinTransitionFrames; i++)
		{
			yield return waitForEndOfFrameDelay;
		}
		LeanTween.alpha(m_FadeInPanel, 0f, m_TransitionTime).setEaseInQuart().setDelay(m_TransitionDelay)
			.setIgnoreTimeScale(useUnScaledTime: true)
			.setOnComplete((Action)delegate
			{
				TransitionDone = true;
				if (fadePanelImage != null)
				{
					fadePanelImage.raycastTarget = false;
				}
				m_FadeInPanel.gameObject.SetActive(value: false);
				this.onTransitionFinished?.Invoke();
				this.onTransitionFinished = null;
			});
	}

	public void FadeIn(Action onFinished)
	{
		TransitionDone = false;
		StartCoroutine(FadeInCoroutine(onFinished));
	}

	private IEnumerator FadeInCoroutine(Action onFinished)
	{
		m_FadeInPanel.gameObject.SetActive(value: true);
		WaitForEndOfFrame waitForEndOfFrameDelay = new WaitForEndOfFrame();
		for (int i = 0; (float)i < m_MinTransitionFrames; i++)
		{
			yield return waitForEndOfFrameDelay;
		}
		fadeGraphic.SetAlpha(0f);
		LeanTween.alpha(m_FadeInPanel, 1f, m_TransitionTime).setDelay(m_TransitionDelay).setEaseInQuart()
			.setIgnoreTimeScale(useUnScaledTime: true)
			.setOnComplete((Action)delegate
			{
				TransitionDone = true;
				this.onTransitionFinished?.Invoke();
				this.onTransitionFinished = null;
				onFinished?.Invoke();
			});
	}

	public void RegisterCallbackOnTransitionFinished(Action callback)
	{
		if (TransitionDone)
		{
			callback();
		}
		else
		{
			onTransitionFinished += callback.Invoke;
		}
	}

	public void SetFade(float fade)
	{
		fadeGraphic.SetAlpha(fade);
		m_FadeInPanel.gameObject.SetActive(fade > 0f);
	}

	public void Fade(Action onFinished, float duration, float from, float to)
	{
		m_FadeInPanel.gameObject.SetActive(value: true);
		fadeGraphic.SetAlpha(from);
		LeanTween.alpha(m_FadeInPanel, to, duration).setIgnoreTimeScale(useUnScaledTime: true).setOnComplete(onFinished);
	}
}
