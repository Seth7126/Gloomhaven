using System;
using System.Collections;
using SRDebugger.Internal;
using SRDebugger.Services;
using SRDebugger.UI.Controls;
using SRF;
using UnityEngine;
using UnityEngine.UI;

namespace SRDebugger.UI.Tabs;

public class ConsoleTabController : SRMonoBehaviourEx
{
	public enum CopyToClipboardStates
	{
		Hidden,
		Visible,
		Activated
	}

	private const int MaxLength = 2600;

	private Canvas _consoleCanvas;

	private bool _isDirty;

	private static bool _hasWarnedAboutLogHandler;

	private static bool _hasWarnedAboutLoggingDisabled;

	[Import]
	public IConsoleFilterState FilterState;

	[RequiredField]
	public ConsoleLogControl ConsoleLogControl;

	[RequiredField]
	public Toggle PinToggle;

	[RequiredField]
	public ScrollRect StackTraceScrollRect;

	[RequiredField]
	public Text StackTraceText;

	[RequiredField]
	public Toggle ToggleErrors;

	[RequiredField]
	public Text ToggleErrorsText;

	[RequiredField]
	public Toggle ToggleInfo;

	[RequiredField]
	public Text ToggleInfoText;

	[RequiredField]
	public Toggle ToggleWarnings;

	[RequiredField]
	public Text ToggleWarningsText;

	[RequiredField]
	public GameObject CopyToClipboardContainer;

	[RequiredField]
	public GameObject CopyToClipboardButton;

	[RequiredField]
	public GameObject CopyToClipboardMessage;

	[RequiredField]
	public CanvasGroup CopyToClipboardMessageCanvasGroup;

	[RequiredField]
	public GameObject LoggingIsDisabledCanvasGroup;

	[RequiredField]
	public GameObject LogHandlerHasBeenOverridenGroup;

	[RequiredField]
	public Toggle FilterToggle;

	[RequiredField]
	public InputField FilterField;

	[RequiredField]
	public GameObject FilterBarContainer;

	private ConsoleEntry _selectedItem;

	private Coroutine _fadeButtonCoroutine;

	protected override void Start()
	{
		base.Start();
		_consoleCanvas = GetComponent<Canvas>();
		ToggleErrors.isOn = FilterState.GetConsoleFilterState(LogType.Error);
		ToggleWarnings.isOn = FilterState.GetConsoleFilterState(LogType.Warning);
		ToggleInfo.isOn = FilterState.GetConsoleFilterState(LogType.Log);
		ToggleErrors.onValueChanged.AddListener(delegate(bool isOn)
		{
			FilterState.SetConsoleFilterState(LogType.Error, isOn);
			_isDirty = true;
		});
		ToggleWarnings.onValueChanged.AddListener(delegate(bool isOn)
		{
			FilterState.SetConsoleFilterState(LogType.Warning, isOn);
			_isDirty = true;
		});
		ToggleInfo.onValueChanged.AddListener(delegate(bool isOn)
		{
			FilterState.SetConsoleFilterState(LogType.Log, isOn);
			_isDirty = true;
		});
		PinToggle.onValueChanged.AddListener(PinToggleValueChanged);
		FilterToggle.onValueChanged.AddListener(FilterToggleValueChanged);
		FilterBarContainer.SetActive(FilterToggle.isOn);
		FilterField.onValueChanged.AddListener(FilterValueChanged);
		ConsoleLogControl.SelectedItemChanged = ConsoleLogSelectedItemChanged;
		Service.Console.Updated += ConsoleOnUpdated;
		Service.Panel.VisibilityChanged += PanelOnVisibilityChanged;
		FilterState.FilterStateChange += OnFilterStateChange;
		StackTraceText.supportRichText = Settings.Instance.RichTextInConsole;
		PopulateStackTraceArea(null);
		Refresh();
	}

	private void OnFilterStateChange(LogType logtype, bool newstate)
	{
		switch (logtype)
		{
		case LogType.Error:
			ToggleErrors.isOn = newstate;
			break;
		case LogType.Warning:
			ToggleWarnings.isOn = newstate;
			break;
		case LogType.Log:
			ToggleInfo.isOn = newstate;
			break;
		case LogType.Assert:
			break;
		}
	}

	private void FilterToggleValueChanged(bool isOn)
	{
		if (isOn)
		{
			FilterBarContainer.SetActive(value: true);
			ConsoleLogControl.Filter = FilterField.text;
		}
		else
		{
			ConsoleLogControl.Filter = null;
			FilterBarContainer.SetActive(value: false);
		}
	}

	private void FilterValueChanged(string filterText)
	{
		if (FilterToggle.isOn && !string.IsNullOrEmpty(filterText) && filterText.Trim().Length != 0)
		{
			ConsoleLogControl.Filter = filterText;
		}
		else
		{
			ConsoleLogControl.Filter = null;
		}
	}

	private void PanelOnVisibilityChanged(IDebugPanelService debugPanelService, bool b)
	{
		if (!(_consoleCanvas == null))
		{
			if (b)
			{
				_consoleCanvas.enabled = true;
				return;
			}
			_consoleCanvas.enabled = false;
			StopAnimations();
		}
	}

	private void PinToggleValueChanged(bool isOn)
	{
		Service.DockConsole.IsVisible = isOn;
	}

	protected override void OnDestroy()
	{
		StopAnimations();
		if (Service.Console != null)
		{
			Service.Console.Updated -= ConsoleOnUpdated;
		}
		FilterState.FilterStateChange -= OnFilterStateChange;
		base.OnDestroy();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_isDirty = true;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		StopAnimations();
	}

	private void ConsoleLogSelectedItemChanged(object item)
	{
		ConsoleEntry entry = item as ConsoleEntry;
		PopulateStackTraceArea(entry);
	}

	protected override void Update()
	{
		base.Update();
		if (_isDirty)
		{
			Refresh();
		}
	}

	private void PopulateStackTraceArea(ConsoleEntry entry)
	{
		if (entry == null)
		{
			SetCopyToClipboardButtonState(CopyToClipboardStates.Hidden);
			StackTraceText.text = "";
		}
		else
		{
			if (SRDebug.CopyConsoleItemCallback != null)
			{
				SetCopyToClipboardButtonState(CopyToClipboardStates.Visible);
			}
			string text = entry.Message + Environment.NewLine + ((!string.IsNullOrEmpty(entry.StackTrace)) ? entry.StackTrace : SRDebugStrings.Current.Console_NoStackTrace);
			if (text.Length > 2600)
			{
				text = text.Substring(0, 2600);
				text = text + "\n" + SRDebugStrings.Current.Console_MessageTruncated;
			}
			StackTraceText.text = text;
		}
		StackTraceScrollRect.normalizedPosition = new Vector2(0f, 1f);
		_selectedItem = entry;
	}

	public void CopyToClipboard()
	{
		if (_selectedItem != null)
		{
			SetCopyToClipboardButtonState(CopyToClipboardStates.Activated);
			if (SRDebug.CopyConsoleItemCallback != null)
			{
				SRDebug.CopyConsoleItemCallback(_selectedItem);
			}
			else
			{
				Debug.LogError("[SRDebugger] Copy to clipboard is not available.");
			}
		}
	}

	private void SetCopyToClipboardButtonState(CopyToClipboardStates state)
	{
		StopAnimations();
		switch (state)
		{
		case CopyToClipboardStates.Hidden:
			CopyToClipboardContainer.SetActive(value: false);
			CopyToClipboardButton.SetActive(value: false);
			CopyToClipboardMessage.SetActive(value: false);
			break;
		case CopyToClipboardStates.Visible:
			CopyToClipboardContainer.SetActive(value: true);
			CopyToClipboardButton.SetActive(value: true);
			CopyToClipboardMessage.SetActive(value: false);
			break;
		case CopyToClipboardStates.Activated:
			CopyToClipboardMessageCanvasGroup.alpha = 1f;
			CopyToClipboardContainer.SetActive(value: true);
			CopyToClipboardButton.SetActive(value: false);
			CopyToClipboardMessage.SetActive(value: true);
			_fadeButtonCoroutine = StartCoroutine(FadeCopyButton());
			break;
		default:
			throw new ArgumentOutOfRangeException("state", state, null);
		}
	}

	private IEnumerator FadeCopyButton()
	{
		yield return new WaitForSecondsRealtime(2f);
		float startTime = Time.realtimeSinceStartup;
		float endTime = Time.realtimeSinceStartup + 1f;
		while (Time.realtimeSinceStartup < endTime)
		{
			float alpha = Mathf.InverseLerp(endTime, startTime, Time.realtimeSinceStartup);
			CopyToClipboardMessageCanvasGroup.alpha = alpha;
			yield return new WaitForEndOfFrame();
		}
		CopyToClipboardMessageCanvasGroup.alpha = 0f;
		_fadeButtonCoroutine = null;
	}

	private void StopAnimations()
	{
		if (_fadeButtonCoroutine != null)
		{
			StopCoroutine(_fadeButtonCoroutine);
			_fadeButtonCoroutine = null;
			CopyToClipboardMessageCanvasGroup.alpha = 0f;
		}
	}

	private void Refresh()
	{
		ToggleInfoText.text = SRDebuggerUtil.GetNumberString(Service.Console.InfoCount, 999, "999+");
		ToggleWarningsText.text = SRDebuggerUtil.GetNumberString(Service.Console.WarningCount, 999, "999+");
		ToggleErrorsText.text = SRDebuggerUtil.GetNumberString(Service.Console.ErrorCount, 999, "999+");
		ConsoleLogControl.ShowErrors = ToggleErrors.isOn;
		ConsoleLogControl.ShowWarnings = ToggleWarnings.isOn;
		ConsoleLogControl.ShowInfo = ToggleInfo.isOn;
		PinToggle.isOn = Service.DockConsole.IsVisible;
		_isDirty = false;
		if (!_hasWarnedAboutLogHandler && Service.Console.LogHandlerIsOverriden)
		{
			LogHandlerHasBeenOverridenGroup.SetActive(value: true);
			_hasWarnedAboutLogHandler = true;
		}
		if (!_hasWarnedAboutLoggingDisabled && !Service.Console.LoggingEnabled)
		{
			LoggingIsDisabledCanvasGroup.SetActive(value: true);
		}
	}

	private void ConsoleOnUpdated(IConsoleService console)
	{
		_isDirty = true;
	}

	public void Clear()
	{
		Service.Console.Clear();
		_isDirty = true;
	}

	public void LogHandlerHasBeenOverridenOkayButtonPress()
	{
		_hasWarnedAboutLogHandler = true;
		LogHandlerHasBeenOverridenGroup.SetActive(value: false);
	}

	public void LoggingDisableCloseAndIgnorePressed()
	{
		LoggingIsDisabledCanvasGroup.SetActive(value: false);
		_hasWarnedAboutLoggingDisabled = true;
	}

	public void LoggingDisableReenablePressed()
	{
		Service.Console.LoggingEnabled = true;
		LoggingIsDisabledCanvasGroup.SetActive(value: false);
		Debug.Log("[SRDebugger] Re-enabled logging.");
	}
}
