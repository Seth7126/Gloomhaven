using System;
using AsmodeeNet.Foundation;
using TMPro;
using UnityEngine;

namespace AsmodeeNet.UserInterface;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ResponsiveFont : MonoBehaviour
{
	private const string _documentation = "Attach this component to a TextMesh Pro, and its font will be automatically set to the given value according to the current Display Mode";

	public float smallFontSize;

	public float regularFontSize;

	public float bigFontSize;

	private TextMeshProUGUI _text;

	public event Action OnFontIsResized;

	private void Awake()
	{
		_text = GetComponent<TextMeshProUGUI>();
	}

	private void OnEnable()
	{
		CoreApplication.Instance.Preferences.InterfaceDisplayModeDidChange += _ModifyFontSize;
		_ModifyFontSize();
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			CoreApplication.Instance.Preferences.InterfaceDisplayModeDidChange -= _ModifyFontSize;
		}
	}

	private void _ModifyFontSize()
	{
		_text.fontSize = FontSizeForDisplayMode(CoreApplication.Instance.Preferences.InterfaceDisplayMode);
		this.OnFontIsResized?.Invoke();
	}

	public float FontSizeForDisplayMode(Preferences.DisplayMode displayMode)
	{
		return CoreApplication.Instance.Preferences.InterfaceDisplayMode switch
		{
			Preferences.DisplayMode.Small => smallFontSize, 
			Preferences.DisplayMode.Big => bigFontSize, 
			_ => regularFontSize, 
		};
	}
}
