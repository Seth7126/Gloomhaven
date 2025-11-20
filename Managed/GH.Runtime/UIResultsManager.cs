using System.Collections.Generic;
using Chronos;
using FFSNet;
using MEC;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIResultsManager : Singleton<UIResultsManager>
{
	public delegate void EndGameCallback(EResult result);

	public enum ERetryOptionType
	{
		None,
		Only,
		Additional
	}

	[SerializeField]
	protected UIResultsOption returnToMapOption;

	[SerializeField]
	protected UIResultsOption retryOption;

	protected UIWindow myWindow;

	protected List<UIScenarioAchievement> partyAchievements = new List<UIScenarioAchievement>();

	protected EndGameCallback m_EndGameCallback;

	protected EResult m_Result;

	protected ERetryOptionType m_Retry;

	protected bool m_InCustomLevelMode;

	protected bool m_QueuedShow;

	protected List<ScenarioAchievementProgress> m_ProgressedAchievements;

	private CoroutineHandle m_runningCoroutine;

	protected UnityAction m_OnRetry;

	public bool IsShown
	{
		get
		{
			if (!myWindow.IsOpen)
			{
				return m_QueuedShow;
			}
			return true;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		myWindow = GetComponent<UIWindow>();
		m_QueuedShow = false;
	}

	protected override void OnDestroy()
	{
		if (Timing.Instance != null)
		{
			Timing.Instance.KillCoroutinesOnInstance(m_runningCoroutine);
		}
		UnregisterOptions();
		base.OnDestroy();
	}

	public virtual void Show(float delay = 0f, EndGameCallback onButtonClicked = null, EResult result = EResult.None, ERetryOptionType retryOption = ERetryOptionType.None, UnityAction retryCallback = null, List<ScenarioAchievementProgress> progressedAchievements = null)
	{
		SaveData.Instance.Global.CurrentAdventureData?.ScrapeStats(result);
		if (!AutoTestController.s_Instance.AutotestStarted)
		{
			if (FFSNetwork.IsClient)
			{
				ActionProcessor.SetState(ActionProcessorStateType.Halted);
			}
			m_EndGameCallback = onButtonClicked;
			m_ProgressedAchievements = progressedAchievements;
			m_InCustomLevelMode = SaveData.Instance.Global.CurrentlyPlayingCustomLevel;
			m_Retry = ((retryCallback != null) ? retryOption : ERetryOptionType.None);
			AnalyticsWrapper.LogScreenDisplay(AWScreenName.results_screen);
			RegisterOptions();
			m_OnRetry = retryCallback;
			m_Result = result;
			if (!m_QueuedShow && !IsShown)
			{
				m_runningCoroutine = Timing.RunCoroutine(WaitAndShow(delay));
				m_QueuedShow = true;
				InputManager.RequestDisableInput(this, KeyAction.UI_PAUSE);
			}
		}
	}

	protected void RegisterOptions()
	{
		ToggleOptions(isEnabled: true);
	}

	protected void UnregisterOptions()
	{
		ToggleOptions(isEnabled: false);
	}

	private void ToggleOptions(bool isEnabled)
	{
		if (isEnabled)
		{
			returnToMapOption.Register(ReturnToMap);
			retryOption.Register(Retry);
			return;
		}
		retryOption.Unregister();
		returnToMapOption.Unregister();
		retryOption.gameObject.SetActive(value: false);
		returnToMapOption.gameObject.SetActive(value: false);
	}

	protected virtual void Retry()
	{
		m_OnRetry?.Invoke();
	}

	protected virtual void ReturnToMap()
	{
		m_EndGameCallback?.Invoke(m_Result);
		Hide();
	}

	public void MPSessionEndedOnResults()
	{
		returnToMapOption.SetActive(active: true);
	}

	private IEnumerator<float> WaitAndShow(float delay)
	{
		float showTime = Timekeeper.instance.m_GlobalClock.time + delay;
		while (Timekeeper.instance.m_GlobalClock.time < showTime)
		{
			yield return 0f;
		}
		Show();
	}

	protected virtual void Show()
	{
		if (!AutoTestController.s_Instance.AutotestStarted)
		{
			InputManager.RequestDisableInput(this, KeyAction.HIGHLIGHT);
			m_QueuedShow = false;
			myWindow.Show();
		}
	}

	public virtual void Hide()
	{
		InputManager.RequestEnableInput(this, KeyAction.HIGHLIGHT);
		myWindow.Hide();
	}
}
