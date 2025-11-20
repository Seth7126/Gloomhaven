using System;
using System.Collections;
using AsmodeeNet.Foundation;
using AsmodeeNet.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace AsmodeeNet.UserInterface;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
public class ResponsivePopUp : MonoBehaviour
{
	[Serializable]
	public enum ResponsiveScope
	{
		Global,
		PerDisplayMode
	}

	[Serializable]
	public enum ResponsiveStrategy
	{
		FixedSize,
		FillSpace
	}

	[Serializable]
	public struct ResponsiveSettings
	{
		public ResponsiveStrategy strategy;

		public Vector2 size;
	}

	private enum FadeType
	{
		In,
		Out
	}

	private const string _documentation = "<b>ResponsivePopUp</b> will handle the basic layout of a popup window according to the <b>DisplayMode</b> [<b>Small</b>|<b>Regular</b>|<b>Big</b>]\nIt should be added to the root element of your popup (containing the <b>Canvas</b> and <b>CanvasScaler</b>)";

	public RectTransform background;

	public RectTransform container;

	private RectTransform _root;

	private CanvasGroup _canvasGroup;

	private CanvasScaler _canvasScaler;

	private Vector2 _canvasScalerOriginalReferenceResolution;

	public ResponsiveScope responsiveScope;

	public ResponsiveSettings globalSettings;

	public ResponsiveSettings smallSettings;

	public ResponsiveSettings regularSettings;

	public ResponsiveSettings bigSettings;

	public float smallRatio = 0.9f;

	public float regularRatio = 0.6f;

	public float bigRatio = 0.4f;

	public bool fadeDisplay = true;

	public const float fadeDuration = 0.3f;

	public bool autoFadeOnEnable = true;

	private Preferences _prefs;

	private bool _needsUpdate;

	public float Ratio => _prefs.InterfaceDisplayMode switch
	{
		Preferences.DisplayMode.Small => smallRatio, 
		Preferences.DisplayMode.Big => bigRatio, 
		_ => regularRatio, 
	};

	public event Action OnUpdateFinished;

	private void Awake()
	{
		Canvas component = GetComponent<Canvas>();
		_canvasScaler = GetComponent<CanvasScaler>();
		if (component == null || _canvasScaler == null)
		{
			AsmoLogger.Error("ResponsivePopUp", "ResponsivePopUp component should be added to the root element of your popup (containing the Canvas and <b>CanvasScaler</b>)");
			base.gameObject.SetActive(value: false);
			return;
		}
		_root = base.transform as RectTransform;
		_canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		_canvasScalerOriginalReferenceResolution = _canvasScaler.referenceResolution;
		container.anchorMin = new Vector2(0.5f, 0.5f);
		container.anchorMax = new Vector2(0.5f, 0.5f);
		container.localPosition = new Vector3(0f, 0f, 0f);
		_prefs = CoreApplication.Instance.Preferences;
		if (fadeDisplay)
		{
			_canvasGroup = GetComponent<CanvasGroup>();
		}
	}

	private void OnEnable()
	{
		_prefs.AspectDidChange += SetNeedsUpdate;
		_prefs.InterfaceDisplayModeDidChange += SetNeedsUpdate;
		_needsUpdate = true;
		Update();
		if (autoFadeOnEnable)
		{
			FadeIn();
		}
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			_prefs.AspectDidChange -= SetNeedsUpdate;
			_prefs.InterfaceDisplayModeDidChange -= SetNeedsUpdate;
		}
	}

	private void SetNeedsUpdate()
	{
		_needsUpdate = true;
	}

	private void Update()
	{
		if (_needsUpdate)
		{
			_needsUpdate = false;
			ResponsiveSettings responsiveSettings;
			float num;
			switch (_prefs.InterfaceDisplayMode)
			{
			case Preferences.DisplayMode.Small:
				responsiveSettings = smallSettings;
				num = smallRatio;
				break;
			default:
				responsiveSettings = regularSettings;
				num = regularRatio;
				break;
			case Preferences.DisplayMode.Big:
				responsiveSettings = bigSettings;
				num = bigRatio;
				break;
			}
			if (responsiveScope == ResponsiveScope.Global)
			{
				responsiveSettings = globalSettings;
			}
			if (responsiveSettings.strategy == ResponsiveStrategy.FixedSize)
			{
				_canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
				Vector2 size = responsiveSettings.size;
				float num2 = size.x / size.y;
				float num3 = Mathf.Max(_canvasScalerOriginalReferenceResolution.x, _canvasScalerOriginalReferenceResolution.y);
				size = ((responsiveSettings.size.x >= responsiveSettings.size.y) ? new Vector2(num3, num3 / num2) : new Vector2(num3 * num2, num3));
				_canvasScaler.referenceResolution = size;
				container.sizeDelta = responsiveSettings.size;
			}
			else
			{
				_canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Shrink;
				float num4 = Mathf.Max(_canvasScalerOriginalReferenceResolution.x, _canvasScalerOriginalReferenceResolution.y);
				_canvasScaler.referenceResolution = new Vector2(num4, num4);
				float num5 = ((_root.sizeDelta.x >= _root.sizeDelta.y) ? (num4 / _root.sizeDelta.x) : (num4 / _root.sizeDelta.y));
				container.sizeDelta = _root.sizeDelta * num5;
			}
			container.localScale = new Vector3(num, num, num);
			if (this.OnUpdateFinished != null)
			{
				this.OnUpdateFinished();
			}
		}
	}

	public void FadeIn(Action completion = null)
	{
		_Fade(FadeType.In, completion);
	}

	public void FadeOut(Action completion = null)
	{
		_Fade(FadeType.Out, completion);
	}

	private void _Fade(FadeType type, Action completion)
	{
		if (!fadeDisplay)
		{
			completion?.Invoke();
			return;
		}
		if (_canvasGroup == null)
		{
			AsmoLogger.Error("ResponsivePopUp", "Fade In/Out requires a CanvasGroup");
		}
		StartCoroutine(_FadeAnimation(type, completion));
	}

	private IEnumerator _FadeAnimation(FadeType type, Action completion)
	{
		if (type == FadeType.In)
		{
			_canvasGroup.alpha = 0f;
			float invDuration = 3.3333333f;
			while (_canvasGroup.alpha < 1f)
			{
				_canvasGroup.alpha += Time.deltaTime * invDuration;
				yield return null;
			}
		}
		else
		{
			_canvasGroup.alpha = 1f;
			float invDuration = 3.3333333f;
			while (_canvasGroup.alpha > 0f)
			{
				_canvasGroup.alpha -= Time.deltaTime * invDuration;
				yield return null;
			}
		}
		completion?.Invoke();
	}
}
