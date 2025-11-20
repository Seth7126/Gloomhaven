using System;
using System.Collections;
using System.Collections.Generic;
using Chronos;
using UnityEngine;
using WorldspaceUI;

public class ActorProgressModifiers : MonoBehaviour
{
	private float m_RevealTime;

	private Animator m_Animator;

	private void OnEnable()
	{
		m_Animator = GetComponent<Animator>();
	}

	public void ProgressModifiersCallback(float revealTimeout)
	{
		m_RevealTime = revealTimeout;
		StartCoroutine(ProcessSlowMoEffect());
	}

	public IEnumerator ProcessSlowMoEffect()
	{
		AttackModBar.s_AttackModifierBarFlowCanBegin = true;
		if (DebugMenu.DebugMenuNotNull && DebugMenu.Instance.isSlowmoDisabled)
		{
			yield break;
		}
		if (m_Animator != null && m_RevealTime >= 0f)
		{
			AnimatorClipInfo[] currentAnimatorClipInfo = m_Animator.GetCurrentAnimatorClipInfo(0);
			if (currentAnimatorClipInfo != null && currentAnimatorClipInfo.Length != 0)
			{
				AnimationClip clip = currentAnimatorClipInfo[0].clip;
				List<AnimationEvent> list = new List<AnimationEvent>();
				AnimationEvent[] events = clip.events;
				foreach (AnimationEvent animationEvent in events)
				{
					if (animationEvent.functionName != "ProgressModifiersCallback")
					{
						list.Add(animationEvent);
					}
				}
				clip.events = list.ToArray();
			}
		}
		var (minTimeScale, fadeInTime, slowMoDuration) = CalculateTimeParams();
		while (TimeManager.IsPaused)
		{
			yield return null;
		}
		LeanTween.value(1f, minTimeScale, fadeInTime).setOnUpdate(delegate(float value)
		{
			if (slowMoDuration > 0f)
			{
				Timekeeper.instance.m_GlobalClock.localTimeScale = value;
				SaveData.Instance.Global.InvokeGameSpeedChanged();
				AudioController.RefreshTimescale();
			}
		}).setOnComplete((Action)delegate
		{
		})
			.setIgnoreTimeScale(useUnScaledTime: true);
		yield return new WaitForSecondsRealtime(slowMoDuration);
		while (TimeManager.IsPaused)
		{
			yield return null;
		}
		if (slowMoDuration > 0f)
		{
			Timekeeper.instance.m_GlobalClock.localTimeScale = TimeManager.DefaultTimeScale;
			SaveData.Instance.Global.InvokeGameSpeedChanged();
			AudioController.RefreshTimescale();
		}
	}

	private (float MinTimeScale, float FadeInTime, float SlowMoDuration) CalculateTimeParams()
	{
		float slowMoMinimalTimeScale = GlobalSettings.Instance.m_AttackModifierSettings.SlowMoMinimalTimeScale;
		float slowMoFadeInTime = GlobalSettings.Instance.m_AttackModifierSettings.SlowMoFadeInTime;
		float slowMoDuration = GlobalSettings.Instance.m_AttackModifierSettings.SlowMoDuration;
		if (GlobalSettings.Instance.m_AttackModifierSettings.InstantSlowMo)
		{
			return (MinTimeScale: 0f, FadeInTime: 0f, SlowMoDuration: slowMoDuration);
		}
		if (m_RevealTime >= 0f)
		{
			return (MinTimeScale: slowMoMinimalTimeScale, FadeInTime: slowMoFadeInTime, SlowMoDuration: slowMoDuration);
		}
		float num = (1f - slowMoMinimalTimeScale) * slowMoFadeInTime / 2f;
		float num2 = slowMoDuration - slowMoFadeInTime;
		float num3 = slowMoMinimalTimeScale * num2;
		float num4 = num + num3 + m_RevealTime;
		slowMoMinimalTimeScale = num4 / (2f * num2);
		slowMoFadeInTime = num4 / (1f - slowMoMinimalTimeScale);
		return (MinTimeScale: slowMoMinimalTimeScale, FadeInTime: slowMoFadeInTime, SlowMoDuration: slowMoDuration);
	}
}
