using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using UnityEngine;
using UnityEngine.UI;

namespace AsmodeeNet.UserInterface;

[RequireComponent(typeof(Image))]
public class ImageModifier : MonoBehaviour
{
	[Serializable]
	public class DisplayModeToSprite
	{
		public Preferences.DisplayMode displayMode;

		public Sprite sprite;
	}

	private const string _documentation = "Allow you to set a different sprite for the image which will be display according to the current display mode (small, regular, big).\nIf none is specified for a specific display mode then default will be used instead";

	private Image _image;

	public List<DisplayModeToSprite> displayModeToSprite = new List<DisplayModeToSprite>();

	private void Awake()
	{
		_image = GetComponent<Image>();
	}

	private void OnEnable()
	{
		CoreApplication.Instance.Preferences.InterfaceDisplayModeDidChange += _Display;
		_Display();
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			CoreApplication.Instance.Preferences.InterfaceDisplayModeDidChange -= _Display;
		}
	}

	private void _Display()
	{
		Preferences.DisplayMode currentDisplayMode = CoreApplication.Instance.Preferences.InterfaceDisplayMode;
		if (!displayModeToSprite.Any((DisplayModeToSprite x) => x.displayMode == currentDisplayMode))
		{
			currentDisplayMode = Preferences.DisplayMode.Unknown;
		}
		_image.sprite = displayModeToSprite.Single((DisplayModeToSprite x) => x.displayMode == currentDisplayMode).sprite;
	}
}
