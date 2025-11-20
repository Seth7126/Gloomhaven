using System.ComponentModel;
using SRDebugger.Internal;
using SRF;
using UnityEngine;
using UnityEngine.UI;

namespace SRDebugger.UI.Other;

[RequireComponent(typeof(Canvas))]
public class ConfigureCanvasFromSettings : SRMonoBehaviour
{
	private Canvas _canvas;

	private CanvasScaler _canvasScaler;

	private float _originalScale;

	private float _lastSetScale;

	private Settings _settings;

	private void Start()
	{
		_canvas = GetComponent<Canvas>();
		_canvasScaler = GetComponent<CanvasScaler>();
		SRDebuggerUtil.ConfigureCanvas(_canvas);
		_settings = SRDebug.Instance.Settings;
		_originalScale = _canvasScaler.scaleFactor;
		_canvasScaler.scaleFactor = _originalScale * _settings.UIScale;
		_lastSetScale = _canvasScaler.scaleFactor;
		_settings.PropertyChanged += SettingsOnPropertyChanged;
	}

	private void OnDestroy()
	{
		if (_settings != null)
		{
			_settings.PropertyChanged -= SettingsOnPropertyChanged;
		}
	}

	private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
	{
		if (_canvasScaler.scaleFactor != _lastSetScale)
		{
			_originalScale = _canvasScaler.scaleFactor;
		}
		_canvasScaler.scaleFactor = _originalScale * _settings.UIScale;
		_lastSetScale = _canvasScaler.scaleFactor;
	}
}
