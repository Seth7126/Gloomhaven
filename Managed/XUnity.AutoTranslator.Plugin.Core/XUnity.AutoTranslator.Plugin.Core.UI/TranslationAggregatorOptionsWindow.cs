using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core.UI;

internal class TranslationAggregatorOptionsWindow
{
	private const int WindowId = 45733721;

	private const float WindowWidth = 320f;

	private Rect _windowRect = new Rect(20f, 20f, 320f, 400f);

	private bool _isMouseDownOnWindow;

	private TranslationAggregatorViewModel _viewModel;

	private List<ToggleViewModel> _toggles;

	private Vector2 _scrollPosition;

	public bool IsShown
	{
		get
		{
			return _viewModel.IsShowingOptions;
		}
		set
		{
			_viewModel.IsShowingOptions = value;
		}
	}

	public TranslationAggregatorOptionsWindow(TranslationAggregatorViewModel viewModel)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		_viewModel = viewModel;
		_toggles = _viewModel.AllTranslators.Select((TranslatorViewModel x) => new ToggleViewModel(" " + x.Endpoint.Endpoint.FriendlyName, null, null, delegate
		{
			x.IsEnabled = !x.IsEnabled;
		}, () => x.IsEnabled, x.Endpoint.Error == null)).ToList();
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
		_windowRect = GUI.Window(45733721, _windowRect, new WindowFunction(CreateWindowUI), "---- Translation Aggregator Options ----");
		if (GUIUtil.IsAnyMouseButtonOrScrollWheelDownSafe)
		{
			Vector2 val = default(Vector2);
			((Vector2)(ref val))._002Ector(Input.mousePosition.x, (float)Screen.height - Input.mousePosition.y);
			_isMouseDownOnWindow = ((Rect)(ref _windowRect)).Contains(val);
		}
		if (_isMouseDownOnWindow && GUIUtil.IsAnyMouseButtonOrScrollWheelSafe)
		{
			GUI.FocusWindow(45733721);
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
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			AutoTranslationPlugin.Current.DisableAutoTranslator();
			if (GUI.Button(GUIUtil.R(298f, 2f, 20f, 16f), "X"))
			{
				IsShown = false;
			}
			GUILayout.Label("Available Translators", (GUILayoutOption[])(object)new GUILayoutOption[0]);
			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUI.skin.box);
			foreach (ToggleViewModel toggle in _toggles)
			{
				bool enabled = GUI.enabled;
				GUI.enabled = toggle.Enabled;
				bool flag = toggle.IsToggled();
				bool flag2 = GUILayout.Toggle(flag, toggle.Text, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				if (flag != flag2)
				{
					toggle.OnToggled();
				}
				GUI.enabled = enabled;
			}
			GUILayout.EndScrollView();
			GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
			GUILayout.Label("Height", (GUILayoutOption[])(object)new GUILayoutOption[0]);
			_viewModel.Height = Mathf.Round(GUILayout.HorizontalSlider(_viewModel.Height, 50f, 300f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.MaxWidth(250f) }));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
			GUILayout.Label("Width", (GUILayoutOption[])(object)new GUILayoutOption[0]);
			_viewModel.Width = Mathf.Round(GUILayout.HorizontalSlider(_viewModel.Width, 200f, 1000f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.MaxWidth(250f) }));
			GUILayout.EndHorizontal();
			GUI.DragWindow();
		}
		finally
		{
			AutoTranslationPlugin.Current.EnableAutoTranslator();
		}
	}
}
