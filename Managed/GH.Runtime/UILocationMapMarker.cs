using System;
using GLOOM;
using I2.Loc;
using MapRuleLibrary.MapState;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILocationMapMarker : UIMapMarker
{
	[Header("Marker components")]
	[SerializeField]
	private Image icon;

	[SerializeField]
	private RectTransform shield;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private RectTransform mask;

	[SerializeField]
	private RectTransform labelContainer;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[Header("Show animation params")]
	[SerializeField]
	[Range(0f, 1f)]
	private float fadeFrom;

	[SerializeField]
	[Range(0f, 1f)]
	private float fadeTo = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float scaleFrom;

	[SerializeField]
	[Range(0f, 1f)]
	private float scaleTo = 1f;

	[SerializeField]
	private float fadeTime = 0.5f;

	[SerializeField]
	private float slideLabelTime = 0.5f;

	private LTDescr animationDescr;

	private MapLocation _location;

	private Func<bool> visibilityChecker;

	public override void SetLocation(MapLocation location, Vector3 offset)
	{
		base.SetLocation(location, offset);
		visibilityChecker = () => location.gameObject.activeSelf;
		if (location.Location is CStoreLocationState cStoreLocationState)
		{
			nameText.text = GLOOM.LocalizationManager.GetTranslation($"GUI_GUILD_{cStoreLocationState.StoreLocation.StoreType}");
		}
		else
		{
			nameText.text = CreateLayout.LocaliseText(location.Location.Location.LocalisedName);
		}
		_location = location;
	}

	public void SetLocation(Vector3 location, string information, Func<bool> visibilityChecker)
	{
		this.visibilityChecker = visibilityChecker;
		Track(location);
		nameText.text = GLOOM.LocalizationManager.GetTranslation(information);
	}

	public bool IsEnabled()
	{
		if (visibilityChecker != null)
		{
			return visibilityChecker();
		}
		return true;
	}

	public override void Show()
	{
		StopAnimation();
		canvasGroup.ignoreParentGroups = true;
		canvasGroup.alpha = fadeFrom;
		mask.sizeDelta = new Vector2(0f, mask.sizeDelta.y);
		shield.localScale = new Vector2(scaleFrom, scaleFrom);
		base.Show();
		animationDescr = LeanTween.value(canvasGroup.gameObject, delegate(float val)
		{
			canvasGroup.alpha = fadeFrom + (fadeTo - fadeFrom) * val;
			float num = scaleFrom + (scaleTo - scaleFrom) * val;
			shield.localScale = new Vector2(num, num);
		}, 0f, 1f, fadeTime).setOnComplete((Action)delegate
		{
			animationDescr = LeanTween.value(mask.gameObject, delegate(float val)
			{
				mask.sizeDelta = new Vector2(val, mask.sizeDelta.y);
			}, mask.sizeDelta.x, labelContainer.sizeDelta.x, slideLabelTime).setOnComplete((Action)delegate
			{
				animationDescr = null;
			});
		});
	}

	public void ShowFadeIn()
	{
		StopAnimation();
		canvasGroup.ignoreParentGroups = true;
		canvasGroup.alpha = fadeFrom;
		shield.localScale = new Vector2(scaleTo, scaleTo);
		mask.sizeDelta = new Vector2(labelContainer.sizeDelta.x, mask.sizeDelta.y);
		base.Show();
		animationDescr = LeanTween.value(canvasGroup.gameObject, delegate(float val)
		{
			mask.sizeDelta = new Vector2(labelContainer.sizeDelta.x, mask.sizeDelta.y);
			canvasGroup.alpha = fadeFrom + (fadeTo - fadeFrom) * val;
		}, 0f, 1f, fadeTime).setOnComplete((Action)delegate
		{
			animationDescr = null;
		});
	}

	public override void Hide()
	{
		canvasGroup.ignoreParentGroups = false;
		StopAnimation();
		base.Hide();
	}

	public void HideFadeOut()
	{
		StopAnimation();
		animationDescr = LeanTween.alphaCanvas(canvasGroup, fadeFrom, fadeTime).setOnComplete((Action)delegate
		{
			animationDescr = null;
			Hide();
		});
	}

	private void StopAnimation()
	{
		if (animationDescr != null)
		{
			LeanTween.cancel(animationDescr.id);
			animationDescr = null;
		}
	}

	private new void OnEnable()
	{
		base.OnEnable();
		I2.Loc.LocalizationManager.OnLocalizeEvent += OnLanguageChanged;
	}

	private void OnDisable()
	{
		StopAnimation();
		I2.Loc.LocalizationManager.OnLocalizeEvent -= OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		if (!(_location == null))
		{
			if (_location.Location is CStoreLocationState cStoreLocationState)
			{
				nameText.text = GLOOM.LocalizationManager.GetTranslation($"GUI_GUILD_{cStoreLocationState.StoreLocation.StoreType}");
			}
			else
			{
				nameText.text = CreateLayout.LocaliseText(_location.Location.Location.LocalisedName);
			}
		}
	}
}
