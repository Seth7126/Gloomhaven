#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using Chronos;
using JetBrains.Annotations;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

namespace WorldspaceUI;

public class AttackModBar : MonoBehaviour
{
	[Serializable]
	private class Config
	{
		[Header("Phase 1 - Label fade out & Icon scale in")]
		public float InitialLabelFadeAwayDuration = 1f;

		public AnimationCurve InitialLabelFadeAwayCurve;

		public float InitialIconScaleInDelay = 1f;

		public float InitialIconScaleInDuration = 1f;

		public AnimationCurve InitialIconScaleInCurve;

		[AudioEventName]
		public string InitialIconAppearAudioEvent;

		public float InitialIconAudioEventDelay;

		[Header("Phase 2 - Icon swivel & modifier reveal")]
		public float IconSwivelDelay = 1f;

		public float IconSwivelDuration = 1f;

		public AnimationCurve IconSwivelCurve;

		[Header("Phase 2 - Icon modifier replace")]
		public float ReplaceShowDelay;

		public float ReplaceShowDuration = 1f;

		public AnimationCurve ReplaceShowCurve;

		public float ReplaceGlowDuration = 1f;

		public AnimationCurve ReplaceGlowCurve;

		public float ReplaceShineDelay;

		public float ReplaceShineDuration = 1f;

		public AnimationCurve ReplaceShineCurve;

		[Header("Phase 2 - Rolling")]
		public float ModifierRollingDelay = 1f;

		internal Vector3 ModifierRollingScale = new Vector3(2f, 2f);

		public float ModifierRollingDuration = 1f;

		public AnimationCurve ModifierRollingDurationCurve;

		public float ModifierRollingMoveDuration = 1f;

		public AnimationCurve ModifierRollingMoveDurationCurve;

		public float ModifierRollingFinishDelay = 0.3f;

		[Header("Phase 3 - Attack has hit & Damage being displayed")]
		public float UnusedModifierFadeAwayDelay = 1f;

		public float UnusedModifierFadeAwayDuration = 1f;

		public AnimationCurve UnusedModifierFadeAwayCurve;

		public float DamageLabelDisplayDelay = 1f;

		public float LabelFadeAwayDelay = 1f;

		public float LabelFadeAwayDuration = 1f;

		public AnimationCurve LabelFadeAwayCurve;
	}

	public static bool s_AttackModifierBarFlowCanBegin;

	public static int s_LongestAttackModSequence;

	public static bool s_FrozenForAttackModifiers;

	[Space]
	[SerializeField]
	private InfoBar m_InfoBar;

	[SerializeField]
	private HealthBar m_HealthBar;

	[Space]
	[SerializeField]
	private RectTransform m_ContainerRect;

	[SerializeField]
	private RectTransform m_ContainerDrawnRect;

	[SerializeField]
	private RectTransform m_ContainerPosLowered;

	[SerializeField]
	private RectTransform m_ContainerPosRaised;

	[Space]
	[SerializeField]
	private RectTransform m_ModifierPosNormal;

	[SerializeField]
	private RectTransform m_ModifierPosAdvantage1;

	[SerializeField]
	private RectTransform m_ModifierPosAdvantage2;

	[Space]
	[SerializeField]
	private AttackModElementUI m_ModifierPrefab;

	private AttackModElementUI m_Modifier;

	private AttackModElementUI m_ModifierAdvantadge;

	private List<AttackModElementUI> m_RollingModifiers;

	private bool _isFlowActive;

	[Space]
	[SerializeField]
	private CanvasGroup m_IconRolling;

	[Space]
	[SerializeField]
	private Color m_AdvantageColor;

	[SerializeField]
	private Color m_DisadvantageColor;

	[Space]
	[SerializeField]
	private Config m_Config;

	[HideInInspector]
	public Coroutine m_AttackModCoroutine;

	[HideInInspector]
	public Coroutine m_AttackModFinishCoroutine;

	[HideInInspector]
	public IEnumerator m_AttackModFinishEnumerator;

	public float testTimeScaler = 10f;

	private IEnumerator m_IconFadeRollingRoutine;

	private IEnumerator m_IconScaleRollingRoutine;

	private bool isDrawingModifiers;

	private Vector3 iconRollingPosition;

	public bool IsFlowActive
	{
		get
		{
			return _isFlowActive;
		}
		set
		{
			_isFlowActive = value;
			base.gameObject.SetActive(value);
		}
	}

	public GameObject Actor { get; set; }

	[UsedImplicitly]
	private void Awake()
	{
		m_RollingModifiers = new List<AttackModElementUI>();
		iconRollingPosition = m_IconRolling.GetComponent<RectTransform>().anchoredPosition;
		base.gameObject.SetActive(value: false);
	}

	private void SetAdvantageColors(CAttackSummary.TargetSummary attackSummary = null)
	{
		if (attackSummary == null)
		{
			m_Modifier.SetAdvantadgeColor(Color.white);
			m_ModifierAdvantadge?.SetAdvantadgeColor(Color.white);
			return;
		}
		try
		{
			switch (attackSummary.OverallAdvantage)
			{
			case EAdvantageStatuses.None:
				m_Modifier.SetAdvantadgeColor(Color.white);
				m_ModifierAdvantadge?.SetAdvantadgeColor(Color.white);
				break;
			case EAdvantageStatuses.Advantage:
				m_Modifier.SetAdvantadgeColor(m_AdvantageColor);
				m_ModifierAdvantadge?.SetAdvantadgeColor(m_AdvantageColor);
				break;
			case EAdvantageStatuses.Disadvantage:
				m_Modifier.SetAdvantadgeColor(m_DisadvantageColor);
				m_ModifierAdvantadge?.SetAdvantadgeColor(m_DisadvantageColor);
				break;
			}
		}
		catch (Exception ex)
		{
			Debug.Log("Error trying to Set Advantage Colors in attack modifier UI:" + ex.Message + " : Stack:" + ex.StackTrace);
		}
	}

	private void SetTextStyleForModifiers(AttackModifierYMLData modifier1, AttackModifierYMLData modifier2, CAttackSummary.TargetSummary summary, bool useOriginalModifier = true)
	{
		if (modifier1 != null)
		{
			EAttackModifierDamageColorCode colorCode = ((modifier1.AddTarget && !summary.AddedTargetFromAttackModSuccessfully) ? EAttackModifierDamageColorCode.negativeDamage : GetColorCodeForAttackModifier((useOriginalModifier && modifier1.OriginalModifier.IsNOTNullOrEmpty()) ? modifier1.OriginalModifier : modifier1.MathModifier));
			m_Modifier.SetTextStyleForModifiers(modifier1, m_InfoBar.GetColorForColorCode(colorCode), useOriginalModifier);
		}
		else
		{
			m_Modifier.SetTextStyleForModifiers(null, Color.gray);
		}
		if (modifier2 != null)
		{
			EAttackModifierDamageColorCode colorCode2 = ((modifier2.AddTarget && !summary.AddedTargetFromAttackModSuccessfully) ? EAttackModifierDamageColorCode.negativeDamage : GetColorCodeForAttackModifier((useOriginalModifier && modifier2.OriginalModifier.IsNOTNullOrEmpty()) ? modifier2.OriginalModifier : modifier2.MathModifier));
			m_ModifierAdvantadge.SetTextStyleForModifiers(modifier2, m_InfoBar.GetColorForColorCode(colorCode2), useOriginalModifier);
		}
	}

	public void PlayPfxForModifiers(AttackModifierYMLData modifier1, AttackModifierYMLData modifier2, bool useOriginalModifier = true)
	{
		if (modifier1 != null)
		{
			EAttackModifierDamageColorCode colorCodeForAttackModifier = GetColorCodeForAttackModifier((useOriginalModifier && modifier1.OriginalModifier.IsNOTNullOrEmpty()) ? modifier1.OriginalModifier : modifier1.MathModifier);
			m_Modifier?.PlayPfxForModifiers(colorCodeForAttackModifier);
		}
		if (modifier2 != null)
		{
			EAttackModifierDamageColorCode colorCodeForAttackModifier2 = GetColorCodeForAttackModifier((useOriginalModifier && modifier2.OriginalModifier.IsNOTNullOrEmpty()) ? modifier2.OriginalModifier : modifier2.MathModifier);
			m_ModifierAdvantadge?.PlayPfxForModifiers(colorCodeForAttackModifier2);
		}
	}

	public EAttackModifierDamageColorCode GetColorCodeForAttackModifier(string mathModifier)
	{
		switch (mathModifier)
		{
		case "x2":
		case "*2":
			return EAttackModifierDamageColorCode.CriticalDamage;
		case "x0":
		case "*0":
			return EAttackModifierDamageColorCode.ZeroDamage;
		case "+1":
			return EAttackModifierDamageColorCode.PositiveDamage;
		case "-1":
		case "-2":
			return EAttackModifierDamageColorCode.negativeDamage;
		case "+0":
		case "-0":
			return EAttackModifierDamageColorCode.RegularDamage;
		default:
			return EAttackModifierDamageColorCode.RegularDamage;
		}
	}

	private void ResetUI()
	{
		Debug.Log("[AttackModifiers]: ResetUI " + m_RollingModifiers.Count);
		try
		{
			foreach (AttackModElementUI rollingModifier in m_RollingModifiers)
			{
				ObjectPool.Recycle(rollingModifier.gameObject);
			}
			if (m_IconScaleRollingRoutine != null)
			{
				StopCoroutine(m_IconScaleRollingRoutine);
			}
			if (m_IconFadeRollingRoutine != null)
			{
				StopCoroutine(m_IconFadeRollingRoutine);
			}
			m_IconRolling.alpha = 0f;
			m_RollingModifiers.Clear();
			if (m_Modifier != null)
			{
				m_Modifier.Hide();
				ObjectPool.Recycle(m_Modifier.gameObject);
				m_Modifier = null;
				Debug.Log("[AttackModifiers]: Reset Modifier");
			}
			if (m_ModifierAdvantadge != null)
			{
				m_ModifierAdvantadge.Hide();
				ObjectPool.Recycle(m_ModifierAdvantadge.gameObject);
				m_ModifierAdvantadge = null;
			}
			m_ContainerRect.anchoredPosition = m_ContainerPosLowered.anchoredPosition;
			m_ContainerDrawnRect.anchoredPosition = Vector2.zero;
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Error trying to reset the attack modifier UI:" + ex.Message + " : Stack:" + ex.StackTrace);
		}
	}

	public IEnumerator ShowModifiers(CAttackSummary.TargetSummary attackSummary, CAbilityAttack attackAbility)
	{
		s_FrozenForAttackModifiers = true;
		Debug.Log("[AttackModifiers]: ShowModifiers");
		ResetUI();
		s_AttackModifierBarFlowCanBegin = false;
		IsFlowActive = true;
		isDrawingModifiers = true;
		ActorEvents.GetActorEvents(Actor).RegisterEvent(ActorEvents.ActorEvent.StartDrawingModifiers);
		float initialSafetyTimeout = GlobalSettings.Instance.m_AttackModifierSettings.AttackModifierFlowBeginTimeout;
		while (!s_AttackModifierBarFlowCanBegin)
		{
			initialSafetyTimeout -= Timekeeper.instance.m_GlobalClock.unscaledDeltaTime;
			if (initialSafetyTimeout < 0f)
			{
				s_AttackModifierBarFlowCanBegin = true;
			}
			yield return null;
		}
		CActor actorToAttack = attackSummary.ActorToAttack;
		if ((!(actorToAttack is CHeroSummonActor) && !(actorToAttack is CPlayerActor) && !GameState.IsParentUnderMyControl(attackSummary.ActorToAttack) && !attackSummary.ActorToAttack.IsUnderMyControl) || attackSummary.FinalAttackStrength == 0)
		{
			Debug.Log("[AttackModifiers]: ShowModifiers - Fade Attack Label");
			m_InfoBar.MoveToAttackMode(m_Config.InitialLabelFadeAwayDuration * testTimeScaler, m_Config.InitialLabelFadeAwayCurve);
		}
		bool hasAdvantageStatus = attackSummary.OverallAdvantage != EAdvantageStatuses.None;
		for (int i = 0; i < attackSummary.UsedAttackMods.Count; i++)
		{
			Debug.Log("[AttackModifiers]: ShowModifiers - Spawn Modifier");
			bool hasAdvantageWithRolling = false;
			m_Modifier = ObjectPool.Spawn(m_ModifierPrefab, m_ContainerRect);
			m_Modifier.transform.localScale = Vector3.one;
			if (m_Modifier == null)
			{
				Debug.LogError("[AttackModifiers]: Invalid object pooled from the ObjectPool");
			}
			AttackModifierYMLData modifier1Used = attackSummary.UsedAttackMods[i];
			AttackModifierYMLData modifier2Used = null;
			if (hasAdvantageStatus)
			{
				if (m_ModifierAdvantadge == null)
				{
					m_ModifierAdvantadge = ObjectPool.Spawn(m_ModifierPrefab, m_ContainerRect);
					m_ModifierAdvantadge.transform.localScale = Vector3.one;
				}
				List<AttackModifierYMLData> notUsedAttackMods = attackSummary.NotUsedAttackMods;
				if (notUsedAttackMods != null && notUsedAttackMods.Count > 0)
				{
					modifier2Used = attackSummary.NotUsedAttackMods[0];
				}
				else if (i + 1 < attackSummary.UsedAttackMods.Count && (modifier1Used.Rolling || attackSummary.UsedAttackMods[i + 1].Rolling))
				{
					if (modifier1Used.Rolling)
					{
						modifier2Used = attackSummary.UsedAttackMods[i + 1];
					}
					else
					{
						modifier2Used = modifier1Used;
						modifier1Used = attackSummary.UsedAttackMods[i + 1];
					}
					i++;
					hasAdvantageWithRolling = true;
				}
				else
				{
					Debug.LogWarning("Error finding attack modifier to display for attack summary with advantage");
				}
			}
			SetAdvantageColors((i == 0) ? attackSummary : null);
			m_Modifier.transform.position = (hasAdvantageStatus ? m_ModifierPosAdvantage1.position : m_ModifierPosNormal.position);
			m_Modifier.ShowIcon(m_Config.InitialIconScaleInDelay * testTimeScaler, m_Config.InitialIconScaleInDuration * testTimeScaler, m_Config.InitialIconScaleInCurve);
			AudioController.Play(m_Config.InitialIconAppearAudioEvent, 1f, m_Config.InitialIconAudioEventDelay);
			if (hasAdvantageStatus)
			{
				m_ModifierAdvantadge.transform.position = m_ModifierPosAdvantage2.position;
				m_ModifierAdvantadge.ShowIcon(m_Config.InitialIconScaleInDelay * testTimeScaler, m_Config.InitialIconScaleInDuration * testTimeScaler, m_Config.InitialIconScaleInCurve);
			}
			float time = m_Config.InitialIconScaleInDelay * testTimeScaler + m_Config.InitialIconScaleInDuration * testTimeScaler + m_Config.IconSwivelDelay * testTimeScaler;
			yield return new WaitForSecondsFrozen(time);
			Debug.Log("[AttackModifiers]: ShowModifiers - Show Modifier");
			SetTextStyleForModifiers(modifier1Used, modifier2Used, attackSummary);
			m_Modifier.ShowValue(m_Config.IconSwivelDuration * testTimeScaler, m_Config.IconSwivelCurve);
			if (hasAdvantageStatus)
			{
				m_ModifierAdvantadge.ShowValue(m_Config.IconSwivelDuration * testTimeScaler, m_Config.IconSwivelCurve);
			}
			yield return new WaitForSecondsFrozen(m_Config.IconSwivelDuration * testTimeScaler);
			UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.AttackModifierValueRevealed));
			PlayPfxForModifiers(modifier1Used, modifier2Used);
			if (modifier1Used.OriginalModifier.IsNOTNullOrEmpty() && modifier1Used.MathModifier != modifier1Used.OriginalModifier)
			{
				yield return Timekeeper.instance.WaitForSeconds(m_Config.ReplaceShowDelay * testTimeScaler);
				bool finishedReveal = false;
				Debug.Log("[AttackModifiers]: ShowModifiers - Play replace");
				m_Modifier.ShowReplaceValue(m_Config.ReplaceShowDuration * testTimeScaler, m_Config.ReplaceShowCurve, m_Config.ReplaceGlowDuration * testTimeScaler, m_Config.ReplaceGlowCurve, m_Config.ReplaceShineDelay * testTimeScaler, m_Config.ReplaceShineDuration * testTimeScaler, m_Config.ReplaceShineCurve, m_Config.IconSwivelDuration * testTimeScaler, m_Config.IconSwivelCurve, delegate
				{
					SetTextStyleForModifiers(modifier1Used, modifier2Used, attackSummary, useOriginalModifier: false);
				}, delegate
				{
					finishedReveal = true;
				});
				if (hasAdvantageStatus && modifier2Used.MathModifier != modifier2Used.OriginalModifier)
				{
					m_ModifierAdvantadge.ShowReplaceValue(m_Config.ReplaceShowDuration * testTimeScaler, m_Config.ReplaceShowCurve, m_Config.ReplaceGlowDuration * testTimeScaler, m_Config.ReplaceGlowCurve, m_Config.ReplaceShineDelay * testTimeScaler, m_Config.ReplaceShineDuration * testTimeScaler, m_Config.ReplaceShineCurve, m_Config.IconSwivelDuration * testTimeScaler, m_Config.IconSwivelCurve, null);
				}
				yield return new WaitUntil(() => finishedReveal);
				PlayPfxForModifiers(modifier1Used, modifier2Used, useOriginalModifier: false);
			}
			if (m_ModifierAdvantadge != null && hasAdvantageStatus && !hasAdvantageWithRolling)
			{
				m_ModifierAdvantadge.HideValue(m_Config.UnusedModifierFadeAwayDelay * testTimeScaler, m_Config.UnusedModifierFadeAwayDuration * testTimeScaler, m_Config.UnusedModifierFadeAwayCurve);
				yield return Timekeeper.instance.WaitForSeconds(m_Config.UnusedModifierFadeAwayDelay * testTimeScaler + m_Config.UnusedModifierFadeAwayDuration * testTimeScaler);
				if (m_ModifierAdvantadge != null)
				{
					ObjectPool.Recycle(m_ModifierAdvantadge.gameObject);
					m_ModifierAdvantadge = null;
				}
			}
			if (hasAdvantageWithRolling && i + 1 < attackSummary.UsedAttackMods.Count)
			{
				Debug.Log("[AttackModifiers]: ShowModifiers - Add modifier to rolling");
				m_RollingModifiers.Add(m_ModifierAdvantadge);
				m_RollingModifiers.Add(m_Modifier);
				AttackModElementUI secondModifier = m_ModifierAdvantadge;
				m_ModifierAdvantadge = null;
				m_IconRolling.transform.localScale = Vector2.one;
				m_IconRolling.alpha = 0f;
				(m_IconRolling.transform as RectTransform).anchoredPosition = iconRollingPosition;
				yield return new WaitForSecondsFrozen(m_Config.ModifierRollingDelay * testTimeScaler);
				m_Modifier.transform.SetParent(m_ContainerDrawnRect);
				RollModifier();
				yield return new WaitForSecondsFrozen((m_Config.ModifierRollingDuration + m_Config.ModifierRollingFinishDelay) * testTimeScaler);
				yield return CorrutineUtils.RectTransformMove(m_ContainerDrawnRect, 0f, m_Config.ModifierRollingMoveDuration * testTimeScaler, new Vector3(0f - m_ContainerDrawnRect.sizeDelta.x, 0f), m_Config.ModifierRollingMoveDurationCurve);
				yield return new WaitForSecondsFrozen(m_Config.ModifierRollingDelay * testTimeScaler);
				secondModifier.transform.SetParent(m_ContainerDrawnRect);
				RollModifier();
				yield return new WaitForSecondsFrozen(m_Config.ModifierRollingDuration * testTimeScaler);
				yield return CorrutineUtils.RectTransformMove(m_ContainerDrawnRect, 0f, m_Config.ModifierRollingMoveDuration * testTimeScaler, new Vector3(0f - m_ContainerDrawnRect.sizeDelta.x, 0f), m_Config.ModifierRollingMoveDurationCurve);
				hasAdvantageStatus = false;
			}
			else if (i + 1 < attackSummary.UsedAttackMods.Count)
			{
				Debug.Log("[AttackModifiers]: ShowModifiers - Make Space for Next Rolling Modifier");
				m_RollingModifiers.Add(m_Modifier);
				m_IconRolling.transform.localScale = Vector2.one;
				m_IconRolling.alpha = 0f;
				(m_IconRolling.transform as RectTransform).anchoredPosition = iconRollingPosition;
				yield return new WaitForSecondsFrozen(m_Config.ModifierRollingDelay * testTimeScaler);
				yield return new WaitForSecondsFrozen(m_Config.ModifierRollingDuration / 2f * testTimeScaler);
				m_Modifier.transform.SetParent(m_ContainerDrawnRect);
				yield return new WaitForSecondsFrozen(m_Config.ModifierRollingDuration / 2f * testTimeScaler);
				yield return CorrutineUtils.RectTransformMove(m_ContainerDrawnRect, 0f, m_Config.ModifierRollingMoveDuration * testTimeScaler, new Vector3(0f - m_ContainerDrawnRect.sizeDelta.x, 0f), m_Config.ModifierRollingMoveDurationCurve);
				hasAdvantageStatus = attackSummary.NotUsedAttackMods != null && attackSummary.NotUsedAttackMods.Count - 1 > i;
			}
			else if (hasAdvantageWithRolling)
			{
				Debug.Log("[AttackModifiers]: ShowModifiers - Add rolling Modifier with adv");
				AttackModElementUI secondModifier = m_ModifierAdvantadge;
				m_RollingModifiers.Add(m_ModifierAdvantadge);
				m_ModifierAdvantadge = null;
				m_IconRolling.transform.localScale = Vector2.one;
				m_IconRolling.alpha = 0f;
				m_IconRolling.transform.position = m_Modifier.transform.position;
				yield return new WaitForSecondsFrozen(m_Config.ModifierRollingDelay * testTimeScaler);
				RollModifier();
				yield return new WaitForSecondsFrozen((m_Config.ModifierRollingDuration + m_Config.ModifierRollingFinishDelay) * testTimeScaler);
				secondModifier.ShowHighlight(m_Config.ModifierRollingDuration * testTimeScaler, m_Config.ModifierRollingDurationCurve);
				yield return new WaitForSecondsFrozen(m_Config.ModifierRollingDuration * testTimeScaler);
				hasAdvantageStatus = false;
			}
		}
		yield return new WaitForSecondsFrozen(m_Config.ModifierRollingFinishDelay * testTimeScaler);
		Debug.Log("[AttackModifiers]: ShowModifiers - Finish");
		m_AttackModCoroutine = null;
		isDrawingModifiers = false;
		ActorEvents.GetActorEvents(Actor).RegisterEvent(ActorEvents.ActorEvent.FinishedDrawingModifiers);
	}

	private void RollModifier()
	{
		m_IconScaleRollingRoutine = CorrutineUtils.RectTransformLocalScale(m_IconRolling.transform as RectTransform, 0f, m_Config.ModifierRollingDuration * testTimeScaler, m_Config.ModifierRollingScale, m_Config.ModifierRollingDurationCurve);
		StartCoroutine(m_IconScaleRollingRoutine);
		m_IconFadeRollingRoutine = CorrutineUtils.FadeInOut(m_IconRolling, m_Config.ModifierRollingDuration * testTimeScaler, m_Config.ModifierRollingDurationCurve, 0.8f);
		StartCoroutine(m_IconFadeRollingRoutine);
	}

	public IEnumerator ShowAttackModifDamage(CAttackSummary.TargetSummary attackSummary, CAbilityAttack attackAbility, Action onFinished)
	{
		Debug.Log("[AttackModifiers]: ShowAttackModifDamage " + (TimeManager.IsPaused || m_Modifier == null));
		while (TimeManager.IsPaused || m_Modifier == null)
		{
			yield return null;
		}
		if (isDrawingModifiers)
		{
			isDrawingModifiers = false;
			ActorEvents.GetActorEvents(Actor).RegisterEvent(ActorEvents.ActorEvent.FinishedDrawingModifiers);
		}
		yield return Timekeeper.instance.WaitForSeconds(m_Config.DamageLabelDisplayDelay * testTimeScaler);
		Debug.Log("[AttackModifiers]: ShowAttackModifDamage show label");
		m_ContainerRect.position = m_ContainerPosRaised.position;
		m_InfoBar.ShowDamage(attackSummary.FinalAttackStrength, forceShow: true, m_InfoBar.GetCodeForDamage(attackSummary.FinalAttackStrength - attackSummary.AttackModifierCardsStrength, attackAbility.Strength, attackSummary.OverallAdvantage == EAdvantageStatuses.Advantage, attackSummary.OverallAdvantage == EAdvantageStatuses.Disadvantage));
		m_InfoBar.FadeInfoLabel(0f, m_Config.LabelFadeAwayDelay * testTimeScaler, m_Config.LabelFadeAwayDuration * testTimeScaler, m_Config.LabelFadeAwayCurve);
		m_RollingModifiers.ForEach(delegate(AttackModElementUI it)
		{
			it.HideValue(m_Config.LabelFadeAwayDelay * testTimeScaler, m_Config.LabelFadeAwayDuration * testTimeScaler, m_Config.LabelFadeAwayCurve);
		});
		m_Modifier.HideValue(m_Config.LabelFadeAwayDelay * testTimeScaler, m_Config.LabelFadeAwayDuration * testTimeScaler, m_Config.LabelFadeAwayCurve);
		yield return Timekeeper.instance.WaitForSeconds(Mathf.Max(m_Config.LabelFadeAwayDelay * testTimeScaler + m_Config.LabelFadeAwayDuration * testTimeScaler, 0f));
		Debug.Log("[AttackModifiers]: ShowAttackModifDamage finish");
		m_AttackModFinishCoroutine = null;
		FinalizeFlow();
		onFinished?.Invoke();
	}

	public void FinalizeFlow()
	{
		Debug.Log("[AttackModifiers]: FinalizeFlow " + m_RollingModifiers.Count);
		IsFlowActive = false;
		if (isDrawingModifiers)
		{
			isDrawingModifiers = false;
			ActorEvents.GetActorEvents(Actor).RegisterEvent(ActorEvents.ActorEvent.FinishedDrawingModifiers);
		}
		if (m_ModifierAdvantadge != null)
		{
			m_ModifierAdvantadge.Hide();
			ObjectPool.Recycle(m_ModifierAdvantadge.gameObject);
			m_ModifierAdvantadge = null;
		}
		m_InfoBar.FinalizeAttackFlow();
		m_HealthBar.FinalizeAttackFlow();
		if (m_Modifier != null)
		{
			m_Modifier.Hide();
			ObjectPool.Recycle(m_Modifier.gameObject);
			m_Modifier = null;
		}
		m_RollingModifiers.ForEach(delegate(AttackModElementUI it)
		{
			ObjectPool.Recycle(it.gameObject);
		});
		m_RollingModifiers.Clear();
	}
}
