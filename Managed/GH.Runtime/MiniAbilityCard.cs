using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using GLOOM;
using SM.Gamepad;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniAbilityCard : MonoBehaviour
{
	private enum SelectionMode
	{
		Skin,
		Glow
	}

	[Header("Internal references")]
	[SerializeField]
	private TextMeshProUGUI initiativeText;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private DigitViewer digitViewer;

	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private Image miniAbilityBackgroundImage;

	[SerializeField]
	private Image highlightImage;

	[SerializeField]
	private GUIAnimator warningAnimation;

	[SerializeField]
	private Image glowImage;

	[SerializeField]
	private float glowAnimationDuration = 1f;

	[SerializeField]
	private Color defaultGlowColor = new Color32(143, 58, 44, byte.MaxValue);

	[SerializeField]
	private Color burnGlowColorFrom;

	[SerializeField]
	private Color burnGlowColorTo;

	[SerializeField]
	private GameObject _hoverImage;

	[SerializeField]
	private GameObject _selectedImage;

	[SerializeField]
	private Hotkey _hotkey;

	[SerializeField]
	private MiniCardEffects effects;

	public RectTransform HighlightRect;

	[SerializeField]
	private List<MiniAbilityCardIndicator> indicators;

	private AbilityCardUISkin cardSkin;

	private MiniAbilityCardIndicator currentIndicator;

	[SerializeField]
	private bool isLongRest;

	[SerializeField]
	private GameObject unfocusedMask;

	private IUiNavigationSelectable _navigationSelectable;

	private SelectionMode selectionMode;

	private LTDescr colorGlowAnim;

	private Action onGlow;

	private const string DebugCancelGlow = "CancelGlowAnimation";

	private const string DebugCancelLose = "LostAnimation";

	private LTDescr lostAnimation;

	public Button InitiativeButton { get; private set; }

	private bool UseDigitText
	{
		get
		{
			if (PlatformLayer.Setting.SimplifiedUI)
			{
				return digitViewer == null;
			}
			return true;
		}
	}

	private void Awake()
	{
		InitiativeButton = initiativeText.GetComponent<Button>();
		Clear();
		if (InputManager.GamePadInUse)
		{
			_navigationSelectable = GetComponent<IUiNavigationSelectable>();
		}
		_hotkey.gameObject.SetActive(value: false);
		if (digitViewer != null)
		{
			digitViewer.Initialize();
		}
		if (!UseDigitText)
		{
			initiativeText.enabled = false;
			if (digitViewer != null)
			{
				digitViewer.Show();
			}
		}
	}

	private void CancelGlowAnimation()
	{
		if (colorGlowAnim != null)
		{
			LeanTween.cancel(colorGlowAnim.id, "CancelGlowAnimation");
		}
		colorGlowAnim = null;
	}

	public IUiNavigationSelectable GetNavigationSelectable()
	{
		return _navigationSelectable;
	}

	public void EnableGlowSelected(Color? glowColor = null, Action onGlow = null)
	{
		CancelGlowAnimation();
		selectionMode = SelectionMode.Glow;
		glowImage.color = glowColor ?? defaultGlowColor;
		glowImage.enabled = false;
		glowImage.gameObject.SetActive(value: true);
		this.onGlow = onGlow;
	}

	public void EnableGlowBurnSelected()
	{
		EnableGlowSelected(burnGlowColorFrom, delegate
		{
			CancelGlowAnimation();
			glowImage.color = burnGlowColorFrom;
			colorGlowAnim = LeanTween.color(glowImage.rectTransform, burnGlowColorTo, glowAnimationDuration).setLoopPingPong();
		});
	}

	public void DisableGlowSelected()
	{
		onGlow = null;
		CancelGlowAnimation();
		selectionMode = SelectionMode.Skin;
		glowImage.enabled = false;
		glowImage.gameObject.SetActive(value: false);
	}

	public void SetDisplayMode(CardPileType type, CAbilityCard abilityCard, bool isHighlighted, bool isHovered = false)
	{
		CancelGlowAnimation();
		bool flag = type != CardPileType.Active;
		if (UseDigitText)
		{
			initiativeText.enabled = flag;
		}
		else if (flag)
		{
			digitViewer.Show();
		}
		else
		{
			digitViewer.Hide();
		}
		if (iconImage != null && abilityCard != null)
		{
			iconImage.enabled = type == CardPileType.Active;
			if (abilityCard.ActiveBonuses.Count > 0)
			{
				iconImage.sprite = UIInfoTools.Instance.GetActiveAbilityIcon(abilityCard.ActiveBonuses[0]);
			}
		}
		if (type != CardPileType.Round)
		{
			HideIndicatorNumber();
		}
		if (InputManager.GamePadInUse)
		{
			_hoverImage.SetActive(!isLongRest && isHovered);
			_selectedImage.SetActive(!isLongRest && type == CardPileType.Round && SaveData.Instance.Global.CurrentGameState != EGameState.Map);
		}
		else
		{
			_hoverImage.SetActive(value: false);
			_selectedImage.SetActive(value: false);
		}
		currentIndicator?.Highlight(isHovered || type == CardPileType.Round);
		if (selectionMode == SelectionMode.Glow)
		{
			glowImage.enabled = isHovered || type == CardPileType.Round;
			if (glowImage.enabled)
			{
				onGlow?.Invoke();
			}
			return;
		}
		Color previewTextColor = cardSkin.GetPreviewTextColor(type, isHighlighted);
		if (UseDigitText)
		{
			initiativeText.color = previewTextColor;
		}
		else
		{
			digitViewer.SetColor(previewTextColor);
		}
		titleText.color = previewTextColor;
		switch (type)
		{
		case CardPileType.Active:
			if (effects != null)
			{
				effects.RestoreCard();
			}
			break;
		case CardPileType.Lost:
			if (effects != null)
			{
				effects.ToggleEffect(active: false, MiniCardEffects.FXTask.LostMode);
			}
			break;
		case CardPileType.Discarded:
			if (effects != null)
			{
				effects.ToggleEffect(active: false, MiniCardEffects.FXTask.DiscardMode);
			}
			break;
		case CardPileType.Round:
			if (effects != null)
			{
				effects.RestoreCard();
			}
			break;
		case CardPileType.Permalost:
			if (effects != null)
			{
				effects.ToggleEffect(active: false, MiniCardEffects.FXTask.LostMode);
			}
			break;
		default:
			if (effects != null)
			{
				effects.RestoreCard();
			}
			break;
		}
		miniAbilityBackgroundImage.sprite = cardSkin.GetPreviewBackground(type, isHighlighted, isLongRest);
	}

	public void MakeMiniCard(string cardName, AbilityCardYMLData cardData, bool isLongRest = false)
	{
		this.isLongRest = isLongRest;
		SetCardName(cardName);
		if (UseDigitText)
		{
			initiativeText.text = cardData.Initiative.ToString();
		}
		else
		{
			digitViewer.ShowValue(cardData.Initiative, show: false);
		}
	}

	public void SetSkin(AbilityCardUISkin skin)
	{
		cardSkin = skin;
	}

	public MiniAbilityCard GenerateCopy(Transform parent)
	{
		MiniAbilityCard miniAbilityCard = UnityEngine.Object.Instantiate(this, parent);
		miniAbilityCard.SetSkin(cardSkin);
		miniAbilityCard.ShowWarning(show: false);
		return miniAbilityCard;
	}

	public IEnumerator LostAnimation(bool active, float cardHighlightTime)
	{
		CancelLostAnimationInternal();
		if (active)
		{
			highlightImage.enabled = true;
			lostAnimation = LeanTween.alpha(highlightImage.GetComponent<RectTransform>(), 1f, cardHighlightTime).setIgnoreTimeScale(useUnScaledTime: true).setOnComplete((Action)delegate
			{
				lostAnimation = null;
			});
			yield break;
		}
		lostAnimation = LeanTween.alpha(highlightImage.GetComponent<RectTransform>(), 0f, cardHighlightTime).setIgnoreTimeScale(useUnScaledTime: true).setOnComplete((Action)delegate
		{
			lostAnimation = null;
		});
		yield return new WaitForSecondsRealtime(cardHighlightTime);
		highlightImage.enabled = false;
	}

	private void CancelLostAnimationInternal()
	{
		if (lostAnimation != null)
		{
			LeanTween.cancel(lostAnimation.id, "LostAnimation");
			lostAnimation = null;
		}
	}

	public void CancelLostAnimation()
	{
		CancelLostAnimationInternal();
		StopAllCoroutines();
		highlightImage.enabled = false;
		Color color = highlightImage.color;
		color.a = 0f;
		highlightImage.color = color;
	}

	public void DisplayIndicator(bool active, IndicatorType indicatorType = IndicatorType.General)
	{
		if (active)
		{
			if (currentIndicator == null || currentIndicator.type != indicatorType)
			{
				if (currentIndicator != null)
				{
					currentIndicator.Hide();
				}
				currentIndicator = indicators.FirstOrDefault((MiniAbilityCardIndicator it) => it.type == indicatorType);
			}
			currentIndicator?.Show();
			if (!isLongRest)
			{
				GetComponent<RectTransform>().anchoredPosition = new Vector3(currentIndicator.offset, 0f, 0f);
			}
		}
		else
		{
			if (currentIndicator != null)
			{
				currentIndicator.Hide();
			}
			currentIndicator = null;
			if (!isLongRest)
			{
				GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
			}
		}
	}

	public void SetCardName(string cardName)
	{
		titleText.text = LocalizationManager.GetTranslation(cardName);
	}

	public void ShowWarning(bool show)
	{
		if (show)
		{
			warningAnimation?.Play();
		}
		else
		{
			warningAnimation?.Stop();
		}
	}

	private void OnDisable()
	{
		CancelGlowAnimation();
		ShowWarning(show: false);
		if (highlightImage.enabled)
		{
			highlightImage.enabled = false;
		}
	}

	private void OnDestroy()
	{
		if (!CoreApplication.IsQuitting)
		{
			CancelGlowAnimation();
			CancelLostAnimationInternal();
		}
	}

	public void Clear()
	{
		ShowWarning(show: false);
		if (effects != null)
		{
			effects.RestoreCard();
		}
		DisplayIndicator(active: false);
		DisableGlowSelected();
		HideIndicatorNumber();
	}

	public void ShowIndicatorNumber(int number)
	{
		if (!(currentIndicator == null))
		{
			currentIndicator.ShowNumber(number);
		}
	}

	public void HideIndicatorNumber()
	{
		if (currentIndicator != null)
		{
			currentIndicator.HideNumber();
		}
	}

	public void SetUnfocused(bool unfocused)
	{
		unfocusedMask.SetActive(unfocused);
	}
}
