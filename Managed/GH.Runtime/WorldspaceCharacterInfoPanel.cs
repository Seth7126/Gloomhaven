#define ENABLE_LOGS
using System;
using GLOOM;
using JetBrains.Annotations;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

public class WorldspaceCharacterInfoPanel : MonoBehaviour
{
	public TextMeshProUGUI CharacterNameText;

	[HideInInspector]
	public ActorBehaviour m_ActorBehavior;

	[Header("Notifications")]
	[SerializeField]
	private Vector3 m_NotificationEndPosition;

	[SerializeField]
	private float m_NotificationDuration;

	[SerializeField]
	private float m_NotificationFadeDuration;

	[SerializeField]
	private TextMeshProUGUI m_PositiveNotifier;

	[SerializeField]
	private TextMeshProUGUI m_NegativeNotifer;

	private bool? m_IsPlayingPositive = false;

	private bool? m_IsPlayingNegative = false;

	protected bool _nameEnabled;

	[UsedImplicitly]
	private void Start()
	{
		CActor actor = m_ActorBehavior.Actor;
		if (actor.IsEnemyByDefault())
		{
			CharacterNameText.text = LocalizationManager.GetTranslation(actor.ActorLocKey()) + " " + actor.ID;
		}
		else
		{
			CharacterNameText.text = LocalizationManager.GetTranslation(actor.ActorLocKey());
			CharacterNameText.color = UIInfoTools.Instance.Orange;
		}
		_nameEnabled = CharacterNameText.enabled;
		CheckToBeActive();
	}

	[UsedImplicitly]
	private void Update()
	{
		if (InputManager.GetIsPressed(KeyAction.HIGHLIGHT))
		{
			CharacterNameText.enabled = true;
			CheckToBeActive();
			return;
		}
		if (_nameEnabled)
		{
			CharacterNameText.enabled = false;
			CheckToBeActive();
		}
		_nameEnabled = false;
	}

	public void NotifyHPIncrease(int amount)
	{
		if (m_IsPlayingPositive.Value)
		{
			Debug.Log("PositiveNotifer Animation in progress");
			return;
		}
		m_PositiveNotifier.text = $"+{amount}";
		PlayHPNotifier(m_PositiveNotifier, m_IsPlayingPositive);
	}

	public void NotifyHPDecrease(int amount)
	{
		if (m_IsPlayingNegative.Value)
		{
			Debug.Log("NegativeNotifier Animation in progress");
			return;
		}
		m_NegativeNotifer.text = $"-{Mathf.Abs(amount)}";
		PlayHPNotifier(m_NegativeNotifer, m_IsPlayingNegative);
	}

	private void PlayHPNotifier(TextMeshProUGUI notifier, bool? isPlayingFlagRef)
	{
		Transform transformCached = notifier.transform;
		transformCached.localPosition = Vector3.zero;
		notifier.alpha = 1f;
		CheckToBeActive();
		isPlayingFlagRef = true;
		Vector3 originalScale = transformCached.localScale;
		Color color = notifier.color;
		LeanTween.moveLocal(notifier.gameObject, m_NotificationEndPosition, m_NotificationDuration + m_NotificationFadeDuration).setEaseOutQuad();
		LeanTween.value(base.gameObject, delegate(Vector3 value)
		{
			transformCached.localScale = value;
		}, originalScale, originalScale * 1.2f, m_NotificationDuration).setEaseOutQuad();
		LeanTween.value(base.gameObject, delegate(Color value)
		{
			notifier.color = value;
		}, Color.white, color, m_NotificationDuration).setEaseOutQuad();
		LeanTween.value(base.gameObject, delegate(Vector3 value)
		{
			transformCached.localScale = value;
		}, originalScale * 1.2f, originalScale * 0.5f, m_NotificationFadeDuration).setDelay(m_NotificationDuration).setEaseOutQuad();
		LeanTween.value(1f, 0f, m_NotificationFadeDuration).setDelay(m_NotificationDuration).setEaseOutQuad()
			.setOnUpdate(delegate(float value)
			{
				notifier.alpha = value;
			})
			.setOnComplete((Action)delegate
			{
				transformCached.localScale = originalScale;
				isPlayingFlagRef = false;
				CheckToBeActive();
			});
	}

	private void CheckToBeActive()
	{
		base.gameObject.SetActive(m_NegativeNotifer.alpha > 0.0001f || m_PositiveNotifier.alpha > 0.0001f || CharacterNameText.enabled);
	}
}
