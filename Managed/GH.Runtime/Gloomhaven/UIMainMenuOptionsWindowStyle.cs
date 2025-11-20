using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gloomhaven;

public class UIMainMenuOptionsWindowStyle : MonoBehaviour
{
	[Serializable]
	private class RectTransformSize
	{
		public RectTransform rect;

		public Vector2 deltaSize;

		private Vector2? defaultDeltaSize;

		public void Init()
		{
			if (!defaultDeltaSize.HasValue)
			{
				defaultDeltaSize = rect.sizeDelta;
			}
		}

		public void Reset()
		{
			if (defaultDeltaSize.HasValue)
			{
				rect.sizeDelta = defaultDeltaSize.Value;
			}
		}

		public void Apply()
		{
			Init();
			rect.sizeDelta = deltaSize;
		}
	}

	[SerializeField]
	private GameObject hiddenInMainMenu;

	[SerializeField]
	private List<RectTransformSize> rectTransformSizes;

	[SerializeField]
	private Image _background;

	[SerializeField]
	private Sprite _pcBackground;

	[SerializeField]
	private Sprite _consoleBackground;

	private void Awake()
	{
		for (int i = 0; i < rectTransformSizes.Count; i++)
		{
			rectTransformSizes[i].Init();
		}
	}

	public void Refresh()
	{
		if (SaveData.Instance?.Global == null || SaveData.Instance.Global.GameMode == EGameMode.MainMenu)
		{
			hiddenInMainMenu.SetActive(value: false);
			for (int i = 0; i < rectTransformSizes.Count; i++)
			{
				rectTransformSizes[i].Apply();
			}
		}
		else
		{
			hiddenInMainMenu.SetActive(value: true);
			for (int j = 0; j < rectTransformSizes.Count; j++)
			{
				rectTransformSizes[j].Reset();
			}
		}
		_background.sprite = (InputManager.GamePadInUse ? _consoleBackground : _pcBackground);
	}
}
