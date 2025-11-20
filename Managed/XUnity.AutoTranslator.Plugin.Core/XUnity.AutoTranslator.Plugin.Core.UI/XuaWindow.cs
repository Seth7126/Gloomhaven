using System.Collections.Generic;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;

namespace XUnity.AutoTranslator.Plugin.Core.UI;

internal class XuaWindow
{
	private const int WindowId = 5464332;

	private const float WindowHeight = 596f;

	private const float WindowWidth = 320f;

	private Rect _windowRect = new Rect(20f, 20f, 320f, 596f);

	private DropdownGUI<TranslatorDropdownOptionViewModel, TranslationEndpointManager> _endpointDropdown;

	private DropdownGUI<TranslatorDropdownOptionViewModel, TranslationEndpointManager> _fallbackDropdown;

	private XuaViewModel _viewModel;

	private bool _isMouseDownOnWindow;

	public bool IsShown
	{
		get
		{
			return _viewModel.IsShown;
		}
		set
		{
			_viewModel.IsShown = value;
		}
	}

	public XuaWindow(XuaViewModel viewModel)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		_viewModel = viewModel;
	}

	public void OnGUI()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		GUI.Box(_windowRect, GUIUtil.none, GUIUtil.GetWindowBackgroundStyle());
		_windowRect = GUI.Window(5464332, _windowRect, new WindowFunction(CreateWindowUI), "---- XUnity.AutoTranslator UI ----");
		if (GUIUtil.IsAnyMouseButtonOrScrollWheelDownSafe)
		{
			Vector2 val = default(Vector2);
			((Vector2)(ref val))._002Ector(Input.mousePosition.x, (float)Screen.height - Input.mousePosition.y);
			_isMouseDownOnWindow = ((Rect)(ref _windowRect)).Contains(val);
		}
		if (_isMouseDownOnWindow && GUIUtil.IsAnyMouseButtonOrScrollWheelSafe)
		{
			GUI.FocusWindow(5464332);
			Vector2 val2 = default(Vector2);
			((Vector2)(ref val2))._002Ector(Input.mousePosition.x, (float)Screen.height - Input.mousePosition.y);
			if (((Rect)(ref _windowRect)).Contains(val2))
			{
				Input.ResetInputAxes();
			}
		}
	}

	private void CreateWindowUI(int id)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0473: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			AutoTranslationPlugin.Current.DisableAutoTranslator();
			float num = 10f;
			float num2 = 20f;
			if (GUI.Button(GUIUtil.R(298f, 2f, 20f, 16f), "X"))
			{
				IsShown = false;
			}
			List<ToggleViewModel> toggles = _viewModel.Toggles;
			float height = 21f * (float)toggles.Count + 5f * (float)toggles.Count - 10f;
			GUI.Box(GUIUtil.R(5f, num2, 310f, height), "");
			foreach (ToggleViewModel item in toggles)
			{
				bool flag = item.IsToggled();
				bool flag2 = GUI.Toggle(GUIUtil.R(10f, num2 + 3f, 300f, 18f), flag, item.Text);
				if (flag != flag2)
				{
					item.OnToggled();
				}
				num2 += 21f;
			}
			num2 += 10f;
			List<ButtonViewModel> commandButtons = _viewModel.CommandButtons;
			int num3 = commandButtons.Count / 3;
			if (commandButtons.Count % 3 != 0)
			{
				num3++;
			}
			height = 21f + 21f * (float)num3 + 10f * (float)(num3 + 1) - 5f;
			GUI.Box(GUIUtil.R(5f, num2, 310f, height), "");
			GUI.Label(GUIUtil.R(10f, num2, 300f, 21f), "---- Command Panel ----", GUIUtil.LabelCenter);
			num2 += 31f;
			for (int i = 0; i < num3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					int num4 = i * 3 + j;
					if (num4 >= commandButtons.Count)
					{
						break;
					}
					ButtonViewModel buttonViewModel = commandButtons[num4];
					GUI.enabled = buttonViewModel.CanClick?.Invoke() ?? true;
					if (GUI.Button(GUIUtil.R(num, num2, 93.333336f, 21f), buttonViewModel.Text))
					{
						buttonViewModel.OnClicked?.Invoke();
					}
					GUI.enabled = true;
					num += 103.333336f;
				}
				num2 += 31f;
			}
			height = 83f;
			GUI.Box(GUIUtil.R(5f, num2, 310f, height), "");
			GUI.Label(GUIUtil.R(10f, num2, 300f, 21f), "---- Select a Translator ----", GUIUtil.LabelCenter);
			num2 += 31f;
			GUI.Label(GUIUtil.R(10f, num2, 70f, 21f), "Translator: ");
			float y = num2;
			num2 += 26f;
			GUI.Label(GUIUtil.R(10f, num2, 60f, 21f), "Fallback: ");
			float y2 = num2;
			num2 += 31f;
			List<LabelViewModel> labels = _viewModel.Labels;
			height = 21f + 21f * (float)labels.Count + 10f * (float)(labels.Count + 1) - 5f;
			GUI.Box(GUIUtil.R(5f, num2, 310f, height), "");
			GUI.Label(GUIUtil.R(10f, num2, 300f, 21f), "---- Status ----", GUIUtil.LabelCenter);
			num2 += 31f;
			foreach (LabelViewModel item2 in labels)
			{
				GUI.Label(GUIUtil.R(10f, num2, 300f, 21f), item2.Title);
				GUI.Label(GUIUtil.R(80f, num2, 230f, 21f), item2.GetValue(), GUIUtil.LabelRight);
				num2 += 31f;
			}
			bool flag3 = (_endpointDropdown ?? (_endpointDropdown = new DropdownGUI<TranslatorDropdownOptionViewModel, TranslationEndpointManager>(80f, y, 230f, _viewModel.TranslatorDropdown))).OnGUI(enabled: true);
			(_fallbackDropdown ?? (_fallbackDropdown = new DropdownGUI<TranslatorDropdownOptionViewModel, TranslationEndpointManager>(80f, y2, 230f, _viewModel.FallbackDropdown))).OnGUI(!flag3);
			GUI.Label(GUIUtil.R(10f, num2, 300f, 105f), GUI.tooltip, GUIUtil.LabelRich);
			GUI.DragWindow();
		}
		finally
		{
			AutoTranslationPlugin.Current.EnableAutoTranslator();
		}
	}
}
