using SRDebugger.Gamepad;
using SRDebugger.VirtualMouse;
using UnityEngine;

namespace SRDebugger.Services;

public interface IDebugService
{
	Settings Settings { get; }

	IGamepad Gamepad { get; }

	IVirtualMouse VirtualMouse { get; }

	bool IsDebugPanelVisible { get; }

	bool IsTriggerEnabled { get; set; }

	bool IsTriggerErrorNotificationEnabled { get; set; }

	IDockConsoleService DockConsole { get; }

	IConsoleFilterState ConsoleFilter { get; }

	bool IsProfilerDocked { get; set; }

	event VisibilityChangedDelegate PanelVisibilityChanged;

	event PinnedUiCanvasCreated PinnedUiCanvasCreated;

	void AddSystemInfo(InfoEntry entry, string category = "Default");

	void ConnectAnotherGamepad(IGamepad gamepad);

	void ConnectVirtualMouse(IVirtualMouse virtualMouse);

	void ShowDebugPanel(bool requireEntryCode = true);

	void ShowDebugPanel(DefaultTabs tab, bool requireEntryCode = true);

	void HideDebugPanel();

	void SetEntryCode(EntryCode newCode);

	void DisableEntryCode();

	void DestroyDebugPanel();

	void AddOptionContainer(object container);

	void RemoveOptionContainer(object container);

	void AddOption(OptionDefinition option);

	bool RemoveOption(OptionDefinition option);

	void PinAllOptions(string category);

	void UnpinAllOptions(string category);

	void PinOption(string name);

	void UnpinOption(string name);

	void ClearPinnedOptions();

	void ShowBugReportSheet(ActionCompleteCallback onComplete = null, bool takeScreenshot = true, string descriptionContent = null);

	RectTransform EnableWorldSpaceMode();

	void SetBugReporterHandler(IBugReporterHandler bugReporterHandler);
}
