using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace WorldspaceUI;

public class HealthBar : MonoBehaviour
{
	[Header("HealthBar")]
	[SerializeField]
	private TextMeshProUGUI m_HealthPoints;

	[SerializeField]
	private DigitViewer m_DigitViewer;

	[SerializeField]
	private Slider m_HealthBar;

	[SerializeField]
	private Image m_HealthIcon;

	[SerializeField]
	private Image m_HealthImage;

	[Header("PreviewBar & Info")]
	[SerializeField]
	private InfoBar m_InfoBar;

	[SerializeField]
	private Slider m_PreviewBar;

	[SerializeField]
	[Range(0f, 2f)]
	private float m_AnimDuration = 1f;

	[SerializeField]
	private TextMeshProUGUI m_InfoLabel;

	[SerializeField]
	private TextMeshProUGUI m_InfoNumber;

	[SerializeField]
	private Image m_InfoIcon;

	[SerializeField]
	private Image m_PreviewBarFill;

	[SerializeField]
	private CanvasGroup m_PreviewCanvasGroup;

	[SerializeField]
	private float m_PreviewFadeDuration = 0.5f;

	[SerializeField]
	private float m_PreviewFadeToValue = 0.3f;

	[Header("Regular Damage")]
	[SerializeField]
	private Color m_RegularDamageColor;

	[SerializeField]
	private Sprite m_RegularDamageBarIcon;

	[SerializeField]
	private Sprite m_RegularDeadIcon;

	[Header("High Damage")]
	[SerializeField]
	private Color m_HigherDamageColor;

	[SerializeField]
	private Sprite m_HigherDamageBarIcon;

	[SerializeField]
	private Sprite m_RetaliateDamageIcon;

	[SerializeField]
	private Sprite m_HigherDeadIcon;

	[Header("Low Damage")]
	[SerializeField]
	private Color m_LowerDamageColor;

	[SerializeField]
	private Sprite m_LowerDamageBarIcon;

	[SerializeField]
	private Sprite m_LowerDeadIcon;

	[Header("Healing")]
	[SerializeField]
	private Color m_HealColor;

	[SerializeField]
	private Sprite m_HealBarIcon;

	[SerializeField]
	private Color m_OverhealColor;

	[SerializeField]
	private Slider m_OverhealBar;

	[SerializeField]
	private Slider m_OverhealOriginalMaxHealthIndicator;

	[Header("Point Marks Mask")]
	[SerializeField]
	private RectTransform m_PointsContainer;

	[SerializeField]
	private Vector2 m_Scale1Health = Vector2.one;

	[SerializeField]
	private Vector2 m_Scale5Health = Vector2.one;

	[SerializeField]
	private List<HealthConfigUI> m_HealthConfigs;

	private bool m_IsAnimActive;

	private bool m_InAttackFlow;

	private IEnumerator m_PreviewSliderRoutine;

	private IEnumerator m_HealthSliderRoutine;

	private List<RectTransform> healthDivisions;

	private Color healthColor;

	private LTDescr animPreview;

	private int maxHealth = -1;

	private float zoom = -1f;

	private HealthZoomConfigUI _selectedConfig;

	private bool _neverUpdatedBarMarks = true;

	private Queue<Action> pendingActions = new Queue<Action>();

	private const string DebugCancelPreview = "CancelPreviewAnimation";

	private bool UseTextDigits
	{
		get
		{
			if (PlatformLayer.Setting.SimplifiedUI)
			{
				return m_DigitViewer == null;
			}
			return true;
		}
	}

	public bool IsAnimated => m_IsAnimActive;

	[UsedImplicitly]
	private void Awake()
	{
		if (m_DigitViewer != null)
		{
			m_DigitViewer.Initialize();
			if (!UseTextDigits)
			{
				m_HealthPoints.gameObject.SetActive(value: false);
			}
		}
		m_HealthConfigs = m_HealthConfigs.OrderByDescending((HealthConfigUI it) => it.health).ToList();
		Initialize();
	}

	private void Initialize()
	{
		if (healthDivisions == null)
		{
			healthDivisions = new List<RectTransform>();
			for (int i = 1; i < m_PointsContainer.childCount; i++)
			{
				healthDivisions.Add(m_PointsContainer.GetChild(i) as RectTransform);
			}
			(m_PointsContainer.GetChild(0) as RectTransform).sizeDelta = new Vector2(0f, 0f);
		}
	}

	private void AdjustHealthBarMarks(int maxHealth)
	{
		if (maxHealth != this.maxHealth)
		{
			this.maxHealth = maxHealth;
			AdjustHealthBarMarks(maxHealth, zoom);
		}
	}

	private void AdjustHealthBarMarks(int maxHealth, float zoom)
	{
		if (maxHealth < 0 || (zoom < 0f && m_HealthConfigs.Count > 0))
		{
			return;
		}
		HealthZoomConfigUI healthZoomConfigUI = null;
		for (int i = 0; i < m_HealthConfigs.Count; i++)
		{
			if (m_HealthConfigs[i].health <= maxHealth)
			{
				healthZoomConfigUI = m_HealthConfigs[i].GetZoomConfig(zoom);
				break;
			}
		}
		if (!_neverUpdatedBarMarks && healthZoomConfigUI == _selectedConfig)
		{
			return;
		}
		_neverUpdatedBarMarks = false;
		_selectedConfig = healthZoomConfigUI;
		Initialize();
		HelperTools.NormalizePool(ref healthDivisions, healthDivisions.Last().gameObject, m_PointsContainer, Math.Max(0, maxHealth - 1));
		float a = m_PointsContainer.rect.width / (float)(maxHealth - 1);
		Vector2 sizeDelta = new Vector2(Mathf.Min(a, _selectedConfig?.baseWidthDivision ?? healthDivisions.First().sizeDelta.x), healthDivisions.First().sizeDelta.y);
		for (int j = 0; j < maxHealth - 1; j++)
		{
			if (_selectedConfig == null)
			{
				healthDivisions[j].localScale = (((j + 1) % 5 == 0) ? m_Scale5Health : m_Scale1Health);
				continue;
			}
			healthDivisions[j].sizeDelta = sizeDelta;
			healthDivisions[j].localScale = _selectedConfig.GetScaleByDivision(j + 1, maxHealth);
		}
	}

	public void OnUpdatedZoom(float zoom)
	{
		this.zoom = zoom;
		AdjustHealthBarMarks(maxHealth, zoom);
	}

	public void UpdateHealth(int maxHealth, int currentHealth, int originalMaxHealth, bool animate, Color allegianceColor, Action onUpdate = null)
	{
		if (m_IsAnimActive)
		{
			pendingActions.Enqueue(delegate
			{
				UpdateHealth(maxHealth, currentHealth, originalMaxHealth, animate, allegianceColor, onUpdate);
			});
			return;
		}
		m_HealthImage.color = allegianceColor;
		onUpdate?.Invoke();
		CancelPreviewAnimation();
		m_HealthBar.maxValue = maxHealth;
		m_PreviewBar.maxValue = maxHealth;
		m_OverhealBar.maxValue = maxHealth;
		if (maxHealth > originalMaxHealth)
		{
			m_OverhealOriginalMaxHealthIndicator.maxValue = maxHealth;
			m_OverhealOriginalMaxHealthIndicator.value = originalMaxHealth;
			m_OverhealOriginalMaxHealthIndicator.gameObject.SetActive(value: true);
			m_OverhealBar.gameObject.SetActive(value: true);
		}
		else
		{
			m_OverhealBar.gameObject.SetActive(value: false);
			m_OverhealOriginalMaxHealthIndicator.gameObject.SetActive(value: false);
		}
		int num = Mathf.Min(currentHealth, originalMaxHealth);
		Color color = ((currentHealth > originalMaxHealth) ? m_OverhealColor : UIInfoTools.Instance.White);
		if (UseTextDigits)
		{
			m_HealthPoints.text = currentHealth.ToString();
			m_HealthPoints.color = color;
		}
		else
		{
			m_DigitViewer.ShowValue(currentHealth);
			m_DigitViewer.SetColor(color);
		}
		if (m_PreviewBar.value > (float)currentHealth)
		{
			m_HealthBar.value = num;
			m_OverhealBar.value = currentHealth;
			if (animate)
			{
				m_IsAnimActive = true;
				if (m_PreviewSliderRoutine != null)
				{
					StopCoroutine(m_PreviewSliderRoutine);
					m_PreviewSliderRoutine = null;
				}
				m_PreviewSliderRoutine = AnimateSlider(m_PreviewBar, currentHealth, m_AnimDuration, delegate
				{
					m_IsAnimActive = false;
					ResetColors();
					if (pendingActions.Count > 0)
					{
						pendingActions.Dequeue()?.Invoke();
					}
				});
				StartCoroutine(m_PreviewSliderRoutine);
			}
			else
			{
				m_PreviewBar.value = currentHealth;
			}
		}
		else if (m_HealthBar.value < (float)currentHealth)
		{
			m_PreviewBar.value = currentHealth;
			if (animate)
			{
				m_IsAnimActive = true;
				if (m_HealthSliderRoutine != null)
				{
					StopCoroutine(m_HealthSliderRoutine);
					m_HealthSliderRoutine = null;
				}
				m_HealthSliderRoutine = AnimateSlider(m_HealthBar, num, m_AnimDuration, delegate
				{
					m_HealthSliderRoutine = AnimateSlider(m_OverhealBar, currentHealth, m_AnimDuration, delegate
					{
						m_IsAnimActive = false;
						ResetColors();
						if (pendingActions.Count > 0)
						{
							pendingActions.Dequeue()?.Invoke();
						}
					});
					StartCoroutine(m_HealthSliderRoutine);
				});
				StartCoroutine(m_HealthSliderRoutine);
			}
			else
			{
				m_OverhealBar.value = currentHealth;
				m_HealthBar.value = num;
			}
		}
		AdjustHealthBarMarks(maxHealth);
	}

	private IEnumerator AnimateSlider(Slider slider, float valueToAnimateTo, float timeToAnimate, UnityAction completionAction)
	{
		float timeElapsed = 0f;
		float startVal = slider.value;
		if (startVal != valueToAnimateTo)
		{
			while (timeElapsed < timeToAnimate)
			{
				slider.value = Mathf.Lerp(startVal, valueToAnimateTo, timeElapsed / timeToAnimate);
				if (!TimeManager.IsPaused)
				{
					timeElapsed += Timekeeper.instance.m_GlobalClock.unscaledDeltaTime;
				}
				yield return null;
			}
			slider.value = valueToAnimateTo;
		}
		completionAction?.Invoke();
	}

	public void UpdateColors(int totalAttack, int baseAttack)
	{
		if (totalAttack == baseAttack)
		{
			m_PreviewBarFill.color = m_RegularDamageColor;
		}
		else if (totalAttack > baseAttack)
		{
			m_PreviewBarFill.color = m_HigherDamageColor;
		}
		else
		{
			m_PreviewBarFill.color = m_LowerDamageColor;
		}
	}

	public void PreviewAttack(int currentHealth, int totalAttack, int baseAttack, int originalMaxHealth, bool highlight = true)
	{
		pendingActions.Clear();
		totalAttack = Mathf.Max(totalAttack, 0);
		int num = currentHealth - totalAttack;
		m_OverhealBar.value = num;
		m_HealthBar.value = Mathf.Min(num, originalMaxHealth);
		m_PreviewBar.value = currentHealth;
		HighlightPreview(highlight);
		UpdateColors(totalAttack, baseAttack);
	}

	public void PreviewRetaliate(int currentHealth, int retaliateDamage, int originalMaxHealth, bool highlight = true)
	{
		pendingActions.Clear();
		retaliateDamage = Mathf.Max(retaliateDamage, 0);
		int num = currentHealth - retaliateDamage;
		m_OverhealBar.value = num;
		m_HealthBar.value = Mathf.Min(num, originalMaxHealth);
		m_PreviewBar.value = currentHealth;
		m_PreviewBarFill.color = m_HigherDamageColor;
		HighlightPreview(highlight);
	}

	public void PreviewHeal(int maxHealth, int currentHealth, int originalMaxHealth, int healAmount, bool highlight = true)
	{
		pendingActions.Clear();
		if (maxHealth != currentHealth)
		{
			HighlightPreview(highlight);
			healAmount = Mathf.Max(healAmount, 0);
			int num = Mathf.Min(maxHealth, currentHealth + healAmount);
			m_HealthBar.value = Mathf.Min(currentHealth, originalMaxHealth);
			m_OverhealBar.value = currentHealth;
			m_PreviewBar.value = num;
			m_PreviewBarFill.color = m_HealColor;
		}
	}

	public void HighlightPreview(bool highlight)
	{
		if (highlight)
		{
			if (animPreview == null)
			{
				AnimatePreviewAlpha(1f, m_PreviewFadeToValue);
			}
		}
		else
		{
			CancelPreviewAnimation();
		}
	}

	private void AnimatePreviewAlpha(float fromAlpha, float toAlpha)
	{
		if (m_PreviewCanvasGroup != null)
		{
			animPreview = LeanTween.alphaCanvas(m_PreviewCanvasGroup, toAlpha, m_PreviewFadeDuration).setOnComplete((Action)delegate
			{
				AnimatePreviewAlpha(toAlpha, fromAlpha);
			});
		}
	}

	private void CancelPreviewAnimation()
	{
		m_PreviewCanvasGroup.alpha = 1f;
		if (animPreview != null)
		{
			LeanTween.cancel(animPreview.id, "CancelPreviewAnimation");
		}
		animPreview = null;
	}

	public void ResetColors()
	{
		if (!m_IsAnimActive && !m_InAttackFlow && m_PreviewBarFill != null)
		{
			m_PreviewBarFill.color = m_RegularDamageColor;
		}
	}

	public void ResetPreview(int currentHealth, int originalMaxHealth)
	{
		if (!m_IsAnimActive && !m_InAttackFlow)
		{
			pendingActions.Clear();
			if (UseTextDigits)
			{
				m_HealthPoints.text = currentHealth.ToString();
			}
			else
			{
				m_DigitViewer.ShowValue(currentHealth);
			}
			m_HealthBar.value = Mathf.Min(currentHealth, originalMaxHealth);
			m_PreviewBar.value = currentHealth;
			m_OverhealBar.value = currentHealth;
			CancelPreviewAnimation();
		}
	}

	public void PrepareForAttack(int currentHealth, int originalMaxHealth)
	{
		pendingActions.Clear();
		m_InAttackFlow = true;
		m_HealthBar.value = Mathf.Min(currentHealth, originalMaxHealth);
		m_PreviewBar.value = currentHealth;
		m_OverhealBar.value = currentHealth;
		CancelPreviewAnimation();
	}

	public void FinalizeAttackFlow()
	{
		m_InAttackFlow = false;
	}

	public void InitHealth(int maxHealth, int currentHealth, int originalMaxHealth, Color color, Sprite healthIcon)
	{
		Slider healthBar = m_HealthBar;
		Slider overhealBar = m_OverhealBar;
		float num = (m_PreviewBar.maxValue = maxHealth);
		float maxValue = (overhealBar.maxValue = num);
		healthBar.maxValue = maxValue;
		m_HealthBar.value = Mathf.Min(currentHealth, originalMaxHealth);
		Slider overhealBar2 = m_OverhealBar;
		maxValue = (m_PreviewBar.value = currentHealth);
		overhealBar2.value = maxValue;
		m_OverhealBar.gameObject.SetActive(maxHealth > originalMaxHealth);
		Slider overhealOriginalMaxHealthIndicator = m_OverhealOriginalMaxHealthIndicator;
		maxValue = (m_OverhealOriginalMaxHealthIndicator.maxValue = maxHealth);
		overhealOriginalMaxHealthIndicator.value = maxValue;
		m_OverhealOriginalMaxHealthIndicator.gameObject.SetActive(maxHealth > originalMaxHealth);
		m_HealthImage.color = color;
		m_HealthIcon.sprite = healthIcon;
	}

	public void Focus(bool focus)
	{
		m_PreviewBar.gameObject.SetActive(focus);
	}

	private void OnDisable()
	{
		CancelPreviewAnimation();
	}
}
