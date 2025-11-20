using System.Collections.Generic;
using SRDebugger.Internal;
using SRF;
using SRF.Service;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace SRDebugger.Services.Implementation;

[Service(typeof(KeyboardShortcutListenerService))]
public class KeyboardShortcutListenerService : SRServiceBase<KeyboardShortcutListenerService>
{
	private List<Settings.KeyboardShortcut> _shortcuts;

	protected override void Awake()
	{
		base.Awake();
		base.CachedTransform.SetParent(Hierarchy.Get("SRDebugger"));
		_shortcuts = new List<Settings.KeyboardShortcut>(Settings.Instance.KeyboardShortcuts);
		foreach (Settings.KeyboardShortcut shortcut in _shortcuts)
		{
			string text = shortcut.Key.ToString();
			KeyControl keyControl = Keyboard.current[text] as KeyControl;
			if (keyControl == null)
			{
				Debug.LogErrorFormat("[SRDebugger] Input System: Unable to find shortcut key: {0}. Shortcut ({1}) will not be functional.", text, shortcut.Action);
				shortcut.Cached_KeyCode = Key.None;
			}
			for (int i = 0; i < Keyboard.current.allKeys.Count; i++)
			{
				if (Keyboard.current.allKeys[i] == keyControl)
				{
					shortcut.Cached_KeyCode = (Key)(i + 1);
					break;
				}
			}
		}
	}

	private void ToggleTab(DefaultTabs t)
	{
		DefaultTabs? activeTab = Service.Panel.ActiveTab;
		if (Service.Panel.IsVisible && activeTab.HasValue && activeTab.Value == t)
		{
			SRDebug.Instance.HideDebugPanel();
		}
		else
		{
			SRDebug.Instance.ShowDebugPanel(t);
		}
	}

	private void ExecuteShortcut(Settings.KeyboardShortcut shortcut)
	{
		switch (shortcut.Action)
		{
		case Settings.ShortcutActions.OpenSystemInfoTab:
			ToggleTab(DefaultTabs.SystemInformation);
			break;
		case Settings.ShortcutActions.OpenConsoleTab:
			ToggleTab(DefaultTabs.Console);
			break;
		case Settings.ShortcutActions.OpenOptionsTab:
			ToggleTab(DefaultTabs.Options);
			break;
		case Settings.ShortcutActions.OpenProfilerTab:
			ToggleTab(DefaultTabs.Profiler);
			break;
		case Settings.ShortcutActions.OpenBugReporterTab:
			ToggleTab(DefaultTabs.BugReporter);
			break;
		case Settings.ShortcutActions.ClosePanel:
			SRDebug.Instance.HideDebugPanel();
			break;
		case Settings.ShortcutActions.OpenPanel:
			SRDebug.Instance.ShowDebugPanel();
			break;
		case Settings.ShortcutActions.TogglePanel:
			if (SRDebug.Instance.IsDebugPanelVisible)
			{
				SRDebug.Instance.HideDebugPanel();
			}
			else
			{
				SRDebug.Instance.ShowDebugPanel();
			}
			break;
		case Settings.ShortcutActions.ShowBugReportPopover:
			SRDebug.Instance.ShowBugReportSheet();
			break;
		case Settings.ShortcutActions.ToggleDockedConsole:
			SRDebug.Instance.DockConsole.IsVisible = !SRDebug.Instance.DockConsole.IsVisible;
			break;
		case Settings.ShortcutActions.ToggleDockedProfiler:
			SRDebug.Instance.IsProfilerDocked = !SRDebug.Instance.IsProfilerDocked;
			break;
		default:
			Debug.LogWarning("[SRDebugger] Unhandled keyboard shortcut: " + shortcut.Action);
			break;
		}
	}

	protected override void Update()
	{
		base.Update();
		UpdateInputSystem();
	}

	private void UpdateInputSystem()
	{
		Keyboard current = Keyboard.current;
		if (current == null)
		{
			Debug.LogWarning("[KeyboardShortcutListenerService] There is no current keyboard => KeyboardShortcutListenerService would not work");
			return;
		}
		if (Settings.Instance.KeyboardEscapeClose && current.escapeKey.isPressed && Service.Panel.IsVisible)
		{
			SRDebug.Instance.HideDebugPanel();
		}
		bool flag = current.leftCtrlKey.isPressed || current.rightCtrlKey.isPressed;
		bool flag2 = current.leftAltKey.isPressed || current.rightAltKey.isPressed;
		bool flag3 = current.leftShiftKey.isPressed || current.rightShiftKey.isPressed;
		for (int i = 0; i < _shortcuts.Count; i++)
		{
			Settings.KeyboardShortcut keyboardShortcut = _shortcuts[i];
			if ((!keyboardShortcut.Control || flag) && (!keyboardShortcut.Shift || flag3) && (!keyboardShortcut.Alt || flag2) && keyboardShortcut.Cached_KeyCode.HasValue && current[keyboardShortcut.Cached_KeyCode.Value].wasPressedThisFrame)
			{
				ExecuteShortcut(keyboardShortcut);
				break;
			}
		}
	}
}
