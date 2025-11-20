using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using UnityEngine;

namespace AsmodeeNet.UserInterface;

[RequireComponent(typeof(RectTransform))]
public class RectTransformModifier : MonoBehaviour
{
	[Flags]
	public enum Parameters
	{
		FrameAndAnchors = 1,
		Pivot = 2,
		Rotation = 4,
		Scale = 8
	}

	public enum Strategy
	{
		PerAspect,
		PerDisplayMode
	}

	[Serializable]
	public class AspectSpecification
	{
		public float aspect = 1f;

		public List<DisplayModeSpecification> displayModeSpecifications = new List<DisplayModeSpecification>();
	}

	[Serializable]
	public class DisplayModeSpecification
	{
		public Preferences.DisplayMode displayMode;

		public RectTransformSpecification specification;
	}

	[Serializable]
	public class RectTransformSpecification
	{
		public Vector2 anchoredPosition;

		public Vector2 sizeDelta;

		public float zPosition;

		public Vector2 anchorMin;

		public Vector2 anchorMax;

		public Vector2 pivot;

		public Vector3 rotation;

		public Vector3 scale;
	}

	private const string _documentation = "<b>RectTransformModifier</b> automatically updates a <b>RectTransform</b> according to the current aspect ratio and interface <b>DisplayMode</b>";

	public Parameters parameters;

	public Strategy strategy = Strategy.PerDisplayMode;

	public RectTransform reference;

	private float _referenceAspect;

	public List<AspectSpecification> aspectSpecifications = new List<AspectSpecification>();

	public List<DisplayModeSpecification> displayModeSpecifications = new List<DisplayModeSpecification>();

	private bool _needsUpdate;

	public bool HasFrameAndAnchors => (parameters & Parameters.FrameAndAnchors) != 0;

	public bool HasPivot => (parameters & Parameters.Pivot) != 0;

	public bool HasRotation => (parameters & Parameters.Rotation) != 0;

	public bool HasScale => (parameters & Parameters.Scale) != 0;

	private void Start()
	{
		aspectSpecifications.Sort((AspectSpecification a, AspectSpecification b) => a.aspect.CompareTo(b.aspect));
	}

	private void OnEnable()
	{
		CoreApplication.Instance.Preferences.AspectDidChange += _SetNeedsUpdate;
		CoreApplication.Instance.Preferences.InterfaceDisplayModeDidChange += _SetNeedsUpdate;
		_SetNeedsUpdate();
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			CoreApplication.Instance.Preferences.AspectDidChange -= _SetNeedsUpdate;
			CoreApplication.Instance.Preferences.InterfaceDisplayModeDidChange -= _SetNeedsUpdate;
		}
	}

	private void _SetNeedsUpdate()
	{
		_needsUpdate = true;
	}

	private void Update()
	{
		if (strategy == Strategy.PerAspect && reference != null && !Mathf.Approximately(reference.rect.y, 0f))
		{
			float num = reference.rect.x / reference.rect.y;
			if (!Mathf.Approximately(num, _referenceAspect))
			{
				_referenceAspect = num;
				_needsUpdate = true;
			}
		}
		if (_needsUpdate)
		{
			_needsUpdate = false;
			if (strategy == Strategy.PerAspect)
			{
				_ApplyPerAspectStrategy();
			}
			else
			{
				_ApplyPerDisplayModeStrategy();
			}
		}
	}

	private void _ApplyPerAspectStrategy()
	{
		Preferences preferences = CoreApplication.Instance.Preferences;
		float num = ((reference != null) ? _referenceAspect : preferences.Aspect);
		Preferences.DisplayMode interfaceDisplayMode = preferences.InterfaceDisplayMode;
		RectTransform rectTransform = base.transform as RectTransform;
		AspectSpecification aspectSpecification = aspectSpecifications.First();
		if (num <= aspectSpecification.aspect)
		{
			RectTransformSpecification rectTransformSpecification = _FindRectTransformSpecificationForDisplayMode(aspectSpecification.displayModeSpecifications, interfaceDisplayMode);
			if (HasFrameAndAnchors)
			{
				rectTransform.anchorMin = rectTransformSpecification.anchorMin;
				rectTransform.anchorMax = rectTransformSpecification.anchorMax;
				rectTransform.sizeDelta = rectTransformSpecification.sizeDelta;
				rectTransform.localPosition = new Vector3(0f, 0f, rectTransformSpecification.zPosition);
				rectTransform.anchoredPosition = rectTransformSpecification.anchoredPosition;
			}
			if (HasPivot)
			{
				rectTransform.pivot = rectTransformSpecification.pivot;
			}
			if (HasRotation)
			{
				rectTransform.localEulerAngles = rectTransformSpecification.rotation;
			}
			if (HasScale)
			{
				rectTransform.localScale = rectTransformSpecification.scale;
			}
			return;
		}
		AspectSpecification aspectSpecification2 = aspectSpecifications.Last();
		if (num >= aspectSpecification2.aspect)
		{
			RectTransformSpecification rectTransformSpecification2 = _FindRectTransformSpecificationForDisplayMode(aspectSpecification2.displayModeSpecifications, interfaceDisplayMode);
			if (HasFrameAndAnchors)
			{
				rectTransform.anchorMin = rectTransformSpecification2.anchorMin;
				rectTransform.anchorMax = rectTransformSpecification2.anchorMax;
				rectTransform.sizeDelta = rectTransformSpecification2.sizeDelta;
				rectTransform.localPosition = new Vector3(0f, 0f, rectTransformSpecification2.zPosition);
				rectTransform.anchoredPosition = rectTransformSpecification2.anchoredPosition;
			}
			if (HasPivot)
			{
				rectTransform.pivot = rectTransformSpecification2.pivot;
			}
			if (HasRotation)
			{
				rectTransform.localEulerAngles = rectTransformSpecification2.rotation;
			}
			if (HasScale)
			{
				rectTransform.localScale = rectTransformSpecification2.scale;
			}
			return;
		}
		AspectSpecification aspectSpecification4;
		AspectSpecification aspectSpecification3 = (aspectSpecification4 = aspectSpecifications.First());
		foreach (AspectSpecification aspectSpecification5 in aspectSpecifications)
		{
			if (aspectSpecification5.aspect >= num)
			{
				aspectSpecification4 = aspectSpecification5;
				break;
			}
			aspectSpecification3 = (aspectSpecification4 = aspectSpecification5);
		}
		RectTransformSpecification rectTransformSpecification3 = _FindRectTransformSpecificationForDisplayMode(aspectSpecification3.displayModeSpecifications, interfaceDisplayMode);
		RectTransformSpecification rectTransformSpecification4 = _FindRectTransformSpecificationForDisplayMode(aspectSpecification4.displayModeSpecifications, interfaceDisplayMode);
		float t = (num - aspectSpecification3.aspect) / (aspectSpecification4.aspect - aspectSpecification3.aspect);
		if (HasFrameAndAnchors)
		{
			rectTransform.anchorMin = Vector2.Lerp(rectTransformSpecification3.anchorMin, rectTransformSpecification4.anchorMin, t);
			rectTransform.anchorMax = Vector2.Lerp(rectTransformSpecification3.anchorMax, rectTransformSpecification4.anchorMax, t);
			rectTransform.sizeDelta = Vector2.Lerp(rectTransformSpecification3.sizeDelta, rectTransformSpecification4.sizeDelta, t);
			rectTransform.localPosition = new Vector3(0f, 0f, Mathf.Lerp(rectTransformSpecification3.zPosition, rectTransformSpecification4.zPosition, t));
			rectTransform.anchoredPosition = Vector2.Lerp(rectTransformSpecification3.anchoredPosition, rectTransformSpecification4.anchoredPosition, t);
		}
		if (HasPivot)
		{
			rectTransform.pivot = Vector2.Lerp(rectTransformSpecification3.pivot, rectTransformSpecification4.pivot, t);
		}
		if (HasRotation)
		{
			rectTransform.localEulerAngles = Vector3.Lerp(rectTransformSpecification3.rotation, rectTransformSpecification4.rotation, t);
		}
		if (HasScale)
		{
			rectTransform.localScale = Vector3.Lerp(rectTransformSpecification3.scale, rectTransformSpecification4.scale, t);
		}
	}

	private void _ApplyPerDisplayModeStrategy()
	{
		Preferences.DisplayMode interfaceDisplayMode = CoreApplication.Instance.Preferences.InterfaceDisplayMode;
		RectTransform rectTransform = base.transform as RectTransform;
		RectTransformSpecification rectTransformSpecification = _FindRectTransformSpecificationForDisplayMode(displayModeSpecifications, interfaceDisplayMode);
		if (HasFrameAndAnchors)
		{
			rectTransform.anchorMin = rectTransformSpecification.anchorMin;
			rectTransform.anchorMax = rectTransformSpecification.anchorMax;
			rectTransform.sizeDelta = rectTransformSpecification.sizeDelta;
			rectTransform.localPosition = new Vector3(0f, 0f, rectTransformSpecification.zPosition);
			rectTransform.anchoredPosition = rectTransformSpecification.anchoredPosition;
		}
		if (HasPivot)
		{
			rectTransform.pivot = rectTransformSpecification.pivot;
		}
		if (HasRotation)
		{
			rectTransform.localEulerAngles = rectTransformSpecification.rotation;
		}
		if (HasScale)
		{
			rectTransform.localScale = rectTransformSpecification.scale;
		}
	}

	private static RectTransformSpecification _FindRectTransformSpecificationForDisplayMode(List<DisplayModeSpecification> displayModeSpecifications, Preferences.DisplayMode displayMode)
	{
		RectTransformSpecification result = null;
		foreach (DisplayModeSpecification displayModeSpecification in displayModeSpecifications)
		{
			if (displayModeSpecification.displayMode == Preferences.DisplayMode.Unknown)
			{
				result = displayModeSpecification.specification;
			}
			else if (displayModeSpecification.displayMode == displayMode)
			{
				return displayModeSpecification.specification;
			}
		}
		return result;
	}

	private void OnDrawGizmosSelected()
	{
		_ = Application.isPlaying;
	}

	private Color _GizmosColorForDisplayMode(Preferences.DisplayMode? displayMode)
	{
		if (displayMode.HasValue)
		{
			switch (displayMode.Value)
			{
			case Preferences.DisplayMode.Small:
				return Color.green;
			case Preferences.DisplayMode.Regular:
				return Color.cyan;
			case Preferences.DisplayMode.Big:
				return Color.blue;
			}
		}
		return Color.gray;
	}
}
