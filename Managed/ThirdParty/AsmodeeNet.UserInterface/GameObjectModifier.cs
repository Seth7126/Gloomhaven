using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using UnityEngine;

namespace AsmodeeNet.UserInterface;

public class GameObjectModifier : MonoBehaviour
{
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

		public List<GameObjectSpecification> specifications;
	}

	[Serializable]
	public class GameObjectSpecification
	{
		public bool active;
	}

	private const string _documentation = "<b>GameObjectModifier</b> automatically activate or deactivate <b>GameObject</b>s according to the current aspect ratio and interface <b>DisplayMode</b>";

	[SerializeField]
	private GameObject[] _gameObjects;

	public Strategy strategy = Strategy.PerDisplayMode;

	public RectTransform reference;

	private float _referenceAspect;

	public List<AspectSpecification> aspectSpecifications = new List<AspectSpecification>();

	public List<DisplayModeSpecification> displayModeSpecifications = new List<DisplayModeSpecification>();

	private bool _needsUpdate;

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
		AspectSpecification aspectSpecification = aspectSpecifications.First();
		if (num <= aspectSpecification.aspect)
		{
			List<GameObjectSpecification> list = _FindGameObjectSpecificationsForDisplayMode(aspectSpecification.displayModeSpecifications, interfaceDisplayMode);
			for (int i = 0; i < _gameObjects.Length; i++)
			{
				_gameObjects[i].SetActive(list[i].active);
			}
			return;
		}
		AspectSpecification aspectSpecification2 = aspectSpecifications.Last();
		if (num >= aspectSpecification2.aspect)
		{
			List<GameObjectSpecification> list2 = _FindGameObjectSpecificationsForDisplayMode(aspectSpecification2.displayModeSpecifications, interfaceDisplayMode);
			for (int j = 0; j < _gameObjects.Length; j++)
			{
				_gameObjects[j].SetActive(list2[j].active);
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
		List<GameObjectSpecification> list3 = _FindGameObjectSpecificationsForDisplayMode(aspectSpecification3.displayModeSpecifications, interfaceDisplayMode);
		List<GameObjectSpecification> list4 = _FindGameObjectSpecificationsForDisplayMode(aspectSpecification4.displayModeSpecifications, interfaceDisplayMode);
		float num2 = (num - aspectSpecification3.aspect) / (aspectSpecification4.aspect - aspectSpecification3.aspect);
		for (int k = 0; k < _gameObjects.Length; k++)
		{
			bool active = ((num2 < 0.5f) ? list3[k].active : list4[k].active);
			_gameObjects[k].SetActive(active);
		}
	}

	private void _ApplyPerDisplayModeStrategy()
	{
		Preferences.DisplayMode interfaceDisplayMode = CoreApplication.Instance.Preferences.InterfaceDisplayMode;
		List<GameObjectSpecification> list = _FindGameObjectSpecificationsForDisplayMode(displayModeSpecifications, interfaceDisplayMode);
		for (int i = 0; i < _gameObjects.Length; i++)
		{
			_gameObjects[i].SetActive(list[i].active);
		}
	}

	private static List<GameObjectSpecification> _FindGameObjectSpecificationsForDisplayMode(List<DisplayModeSpecification> displayModeSpecifications, Preferences.DisplayMode displayMode)
	{
		List<GameObjectSpecification> result = null;
		foreach (DisplayModeSpecification displayModeSpecification in displayModeSpecifications)
		{
			if (displayModeSpecification.displayMode == Preferences.DisplayMode.Unknown)
			{
				result = displayModeSpecification.specifications;
			}
			else if (displayModeSpecification.displayMode == displayMode)
			{
				return displayModeSpecification.specifications;
			}
		}
		return result;
	}
}
