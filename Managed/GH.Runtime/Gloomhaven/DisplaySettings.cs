#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using MEC;
using UnityEngine;
using UnityEngine.UI;

namespace Gloomhaven;

public sealed class DisplaySettings : LocalizedListener
{
	[SerializeField]
	private GraphicSettings graphicsSettings;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	[SerializeField]
	private ExtendedDropdown displayModeSelectField;

	[SerializeField]
	private ExtendedDropdown resolutionSelectField;

	[SerializeField]
	private ExtendedScrollRect scrollRect;

	private List<Resolution> _resolutions;

	private int _currentResolutionIndex;

	private int _nativeResolutionIndex;

	private bool _fullScreen;

	private bool _visibleInBackground;

	private int _selectedMonitor;

	private SelectorWrapper<DisplayModes> _displayModeSelector;

	private void Awake()
	{
		DisplayModes[] source = (DisplayModes[])Enum.GetValues(typeof(DisplayModes));
		_displayModeSelector = new SelectorWrapper<DisplayModes>(displayModeSelectField, source.Select((DisplayModes it) => new SelectorOptData<DisplayModes>(it, () => LocalizationManager.GetTranslation($"GUI_OPT_VIDEO_{it}"))).ToList());
		_displayModeSelector.OnValuedChanged.AddListener(SetDisplayMode);
		resolutionSelectField.onValueChanged.AddListener(SetResolution);
		if (PlatformLayer.Instance.GetResolutions().Count <= 1)
		{
			DisableSetResolutionDropdown();
		}
		displayModeSelectField.OnSelected.AddListener(delegate
		{
			OnSelected(displayModeSelectField);
		});
		displayModeSelectField.OnClosed.AddListener(delegate
		{
			if (controllerArea.IsFocused)
			{
				displayModeSelectField.Select();
			}
		});
		resolutionSelectField.OnSelected.AddListener(delegate
		{
			OnSelected(resolutionSelectField);
		});
		resolutionSelectField.OnClosed.AddListener(delegate
		{
			if (controllerArea.IsFocused)
			{
				resolutionSelectField.Select();
			}
		});
		controllerArea.OnFocusedArea.AddListener(EnableNavigation);
		controllerArea.OnUnfocusedArea.AddListener(DisableNavigation);
		if (controllerArea.IsFocused)
		{
			EnableNavigation();
		}
		void DisableSetResolutionDropdown()
		{
			resolutionSelectField.transform.parent.parent.gameObject.SetActive(value: false);
		}
	}

	private void OnDestroy()
	{
		_displayModeSelector.OnValuedChanged.RemoveAllListeners();
		resolutionSelectField.onValueChanged.RemoveAllListeners();
	}

	private void OnSelected(ExtendedDropdown dropdown)
	{
		if (InputManager.GamePadInUse)
		{
			scrollRect.ScrollToFit(dropdown.transform as RectTransform);
		}
	}

	private void DisableNavigation()
	{
		resolutionSelectField.DisableNavigation();
		displayModeSelectField.DisableNavigation();
	}

	private void EnableNavigation()
	{
		resolutionSelectField.SetNavigation(Navigation.Mode.Vertical);
		displayModeSelectField.SetNavigation(Navigation.Mode.Vertical);
	}

	private void Start()
	{
		int num = PlayerPrefs.GetInt("Screenmanager Fullscreen mode");
		int num2 = PlayerPrefs.GetInt("Visible In Background", 0);
		_visibleInBackground = num2 == 1;
		_fullScreen = num != 3;
		_displayModeSelector.SetValueWithoutNotify(_fullScreen ? ((!_visibleInBackground) ? DisplayModes.Fullscreen : DisplayModes.BorderlessWindowed) : DisplayModes.Windowed);
		UpdateDisplaySelection();
		UpdateResolutionSelection(launching: true);
	}

	private void UpdateDisplaySelection()
	{
		_selectedMonitor = PlayerPrefs.GetInt("UnitySelectMonitor");
	}

	private void UpdateResolutionSelection(bool launching)
	{
		_resolutions = PlatformLayer.Instance.GetResolutions();
		resolutionSelectField.ClearOptions();
		Debug.Log("Resolutions amount: " + _resolutions.Count);
		_nativeResolutionIndex = 0;
		int num = -1;
		int num2 = SaveData.Instance.Global.TargetResolutionWidth;
		int num3 = SaveData.Instance.Global.TargetResolutionHeight;
		if (num2 == -1 || num3 == -1)
		{
			num2 = Screen.currentResolution.width;
			num3 = Screen.currentResolution.height;
			foreach (Resolution resolution in _resolutions)
			{
				if (resolution.width == num2 && resolution.height == num3)
				{
					Save(resolution.width, resolution.height, _fullScreen, resolution.refreshRate);
				}
			}
		}
		List<string> list = new List<string>();
		for (int i = 0; i < _resolutions.Count; i++)
		{
			if (_resolutions[i].width == Display.displays[_selectedMonitor].systemWidth && _resolutions[i].height == Display.displays[_selectedMonitor].systemHeight)
			{
				_nativeResolutionIndex = i;
			}
			if (launching && _resolutions[i].width == num2 && _resolutions[i].height == num3)
			{
				num = i;
			}
			list.Add(_resolutions[i].ToString());
		}
		resolutionSelectField.AddOptions(list);
		if (num != -1)
		{
			resolutionSelectField.SetValueWithoutNotify(num);
			_currentResolutionIndex = num;
		}
	}

	public void SetDisplayMode(DisplayModes displayModeID)
	{
		_fullScreen = displayModeID != DisplayModes.Windowed;
		_visibleInBackground = displayModeID == DisplayModes.BorderlessWindowed;
		SaveData.Instance.Global.TargetFullScreenMode = _fullScreen;
		UpdateResolution();
		Debug.Log("Display Mode: " + displayModeID);
	}

	public void SetTargetDisplay(int targetDisplayID, string targetDisplayName)
	{
		Timing.RunCoroutine(SetTargetDisplay(targetDisplayID));
	}

	private IEnumerator<float> SetTargetDisplay(int targetDisplayID)
	{
		Debug.Log("Switching target display to Display " + (targetDisplayID + 1));
		PlayerPrefs.SetInt("UnitySelectMonitor", targetDisplayID);
		yield return 0f;
		UpdateResolutionSelection(launching: false);
		Debug.Log("About to change to a temp resolution: " + _resolutions[0].ToString());
		resolutionSelectField.value = 0;
		yield return 0f;
		Debug.Log("About to change to the native resolution: " + _resolutions[_nativeResolutionIndex].ToString());
		resolutionSelectField.value = _nativeResolutionIndex;
	}

	public void SetResolution(int resolutionIndex)
	{
		_currentResolutionIndex = resolutionIndex;
		Save(_resolutions[_currentResolutionIndex].width, _resolutions[_currentResolutionIndex].height, _fullScreen, _resolutions[_currentResolutionIndex].refreshRate);
		UpdateResolution();
	}

	private void UpdateResolution()
	{
		GraphicSettings.SetResolutionStatic();
		Screen.fullScreenMode = ((!_fullScreen) ? FullScreenMode.Windowed : (_visibleInBackground ? FullScreenMode.FullScreenWindow : FullScreenMode.ExclusiveFullScreen));
		graphicsSettings.UpdateAntialiasing();
		Debug.Log("Resolution set to: " + _resolutions[_currentResolutionIndex].ToString());
	}

	private void Save(int width, int height, bool fullScreenMode, int refreshRate)
	{
		SaveData.Instance.Global.TargetResolutionWidth = width;
		SaveData.Instance.Global.TargetResolutionHeight = height;
		SaveData.Instance.Global.TargetFullScreenMode = fullScreenMode;
		SaveData.Instance.Global.TargetFrameRate = refreshRate;
		SaveData.Instance.SaveGlobalData();
	}

	public void CloseAllSelectFields()
	{
		if (displayModeSelectField.IsExpanded)
		{
			displayModeSelectField.Hide();
		}
		if (resolutionSelectField.IsExpanded)
		{
			resolutionSelectField.Hide();
		}
	}

	private void OnApplicationQuit()
	{
		PlayerPrefs.SetInt("Visible In Background", _visibleInBackground ? 1 : 0);
	}

	protected override void OnLanguageChanged()
	{
		_displayModeSelector.RefreshTexts();
	}
}
