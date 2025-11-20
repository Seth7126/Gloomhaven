using System;
using UnityEngine;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.UI;

internal class DropdownGUI<TDropdownOptionViewModel, TSelection> where TDropdownOptionViewModel : DropdownOptionViewModel<TSelection> where TSelection : class
{
	private const float MaxHeight = 105f;

	private GUIContent _noSelection;

	private GUIContent _unselect;

	private DropdownViewModel<TDropdownOptionViewModel, TSelection> _viewModel;

	private float _x;

	private float _y;

	private float _width;

	private bool _isShown;

	private Vector2 _scrollPosition;

	private bool _supportsScrollView = true;

	public DropdownGUI(float x, float y, float width, DropdownViewModel<TDropdownOptionViewModel, TSelection> viewModel)
	{
		_x = x;
		_y = y;
		_width = width;
		_noSelection = GUIUtil.CreateContent(viewModel.NoSelection, viewModel.NoSelectionTooltip);
		_unselect = GUIUtil.CreateContent(viewModel.Unselect, viewModel.UnselectTooltip);
		_viewModel = viewModel;
	}

	public bool OnGUI(bool enabled)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		bool enabled2 = GUI.enabled;
		try
		{
			GUI.enabled = enabled;
			bool num = GUI.Button(GUIUtil.R(_x, _y, _width, 21f), _viewModel.CurrentSelection?.Text ?? _noSelection, _isShown ? GUIUtil.NoMarginButtonPressedStyle : GUI.skin.button);
			if (num)
			{
				_isShown = !_isShown;
			}
			if (!enabled)
			{
				_isShown = false;
			}
			if (_isShown)
			{
				ShowDropdown(_x, _y + 21f, _width, GUI.skin.button);
			}
			if (!num && Event.current.isMouse)
			{
				_isShown = false;
			}
			return _isShown;
		}
		finally
		{
			GUI.enabled = enabled2;
		}
	}

	private void ShowDropdown(float x, float y, float width, GUIStyle buttonStyle)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		GUILayout.BeginArea(GUIUtil.R(x, y, width, (_supportsScrollView && (float)_viewModel.Options.Count * 21f > 105f) ? 105f : ((float)_viewModel.Options.Count * 21f)), GUIUtil.NoSpacingBoxStyle);
		try
		{
			if (_supportsScrollView)
			{
				_scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUIStyle.none);
			}
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Warn(e, "GUILayout.BeginScrollView not supported. Proceeding without...");
			_supportsScrollView = false;
		}
		GUIStyle val = ((_viewModel.CurrentSelection == null) ? GUIUtil.NoMarginButtonPressedStyle : GUIUtil.NoMarginButtonStyle);
		if (GUILayout.Button(_unselect, val, ArrayHelper.Null<GUILayoutOption>()))
		{
			_viewModel.Select(null);
			_isShown = false;
		}
		foreach (TDropdownOptionViewModel option in _viewModel.Options)
		{
			val = (option.IsSelected() ? GUIUtil.NoMarginButtonPressedStyle : GUIUtil.NoMarginButtonStyle);
			GUI.enabled = option?.IsEnabled() ?? true;
			if (GUILayout.Button(option.Text, val, ArrayHelper.Null<GUILayoutOption>()))
			{
				_viewModel.Select(option);
				_isShown = false;
			}
			GUI.enabled = true;
		}
		if (_supportsScrollView)
		{
			GUILayout.EndScrollView();
		}
		GUILayout.EndArea();
	}
}
