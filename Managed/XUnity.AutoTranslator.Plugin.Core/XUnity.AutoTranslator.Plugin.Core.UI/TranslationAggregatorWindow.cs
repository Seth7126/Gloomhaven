using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.UI;

internal class TranslationAggregatorWindow
{
	private static string[] Empty = new string[0];

	private const int WindowId = 2387602;

	private Rect _windowRect;

	private bool _isMouseDownOnWindow;

	private TranslationAggregatorViewModel _viewModel;

	private ScrollPositioned _originalText;

	private ScrollPositioned _defaultTranslation;

	private ScrollPositioned<TranslatorViewModel>[] _translationViews;

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

	private float WindowHeight => (float)(_viewModel.AvailableTranslators.Count((TranslatorViewModel x) => x.IsEnabled) + 2) * _viewModel.Height + 30f + 21f + 10f;

	public TranslationAggregatorWindow(TranslationAggregatorViewModel viewModel)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		_viewModel = viewModel;
		_windowRect = new Rect(20f, 20f, _viewModel.Width, WindowHeight);
		_originalText = new ScrollPositioned();
		_defaultTranslation = new ScrollPositioned();
		_translationViews = viewModel.AvailableTranslators.Select((TranslatorViewModel x) => new ScrollPositioned<TranslatorViewModel>(x)).ToArray();
	}

	public void OnGUI()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		((Rect)(ref _windowRect)).height = WindowHeight;
		((Rect)(ref _windowRect)).width = _viewModel.Width;
		_windowRect = GUI.Window(2387602, _windowRect, new WindowFunction(CreateWindowUI), "---- Translation Aggregator ----");
		if (GUIUtil.IsAnyMouseButtonOrScrollWheelDownSafe)
		{
			Vector2 val = default(Vector2);
			((Vector2)(ref val))._002Ector(Input.mousePosition.x, (float)Screen.height - Input.mousePosition.y);
			_isMouseDownOnWindow = ((Rect)(ref _windowRect)).Contains(val);
		}
		if (_isMouseDownOnWindow && GUIUtil.IsAnyMouseButtonOrScrollWheelSafe)
		{
			GUI.FocusWindow(2387602);
			Vector2 val2 = default(Vector2);
			((Vector2)(ref val2))._002Ector(Input.mousePosition.x, (float)Screen.height - Input.mousePosition.y);
			if (((Rect)(ref _windowRect)).Contains(val2))
			{
				Input.ResetInputAxes();
			}
		}
	}

	public void Update()
	{
		_viewModel.Update();
	}

	public void OnNewTranslationAdded(string originalText, string defaultTranslation)
	{
		_viewModel.OnNewTranslationAdded(originalText, defaultTranslation);
	}

	private void CreateWindowUI(int id)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0413: Unknown result type (might be due to invalid IL or missing references)
		//IL_044f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			AutoTranslationPlugin.Current.DisableAutoTranslator();
			float num = 20f;
			if (GUI.Button(GUIUtil.R(_viewModel.Width - 22f, 2f, 20f, 16f), "X"))
			{
				IsShown = false;
			}
			AggregatedTranslationViewModel current = _viewModel.Current;
			if (current != null)
			{
				if (GUI.Button(GUIUtil.R(_viewModel.Width - 5f - 50f, num + 5f + 1f, 50f, 21f), "Copy"))
				{
					current.CopyOriginalTextToClipboard();
				}
				DrawTextArea(num, _originalText, "Original Text", current.OriginalTexts);
				num += _viewModel.Height;
				if (GUI.Button(GUIUtil.R(_viewModel.Width - 5f - 50f, num + 5f + 1f, 50f, 21f), "Copy"))
				{
					current.CopyDefaultTranslationToClipboard();
				}
				DrawTextArea(num, _defaultTranslation, "Default Translation", current.DefaultTranslations);
				num += _viewModel.Height;
				for (int i = 0; i < current.AggregatedTranslations.Count; i++)
				{
					IndividualTranslatorTranslationViewModel individualTranslatorTranslationViewModel = current.AggregatedTranslations[i];
					if (individualTranslatorTranslationViewModel.Translator.IsEnabled)
					{
						ScrollPositioned<TranslatorViewModel> positioned = _translationViews[i];
						GUI.enabled = individualTranslatorTranslationViewModel.CanCopyToClipboard();
						if (GUI.Button(GUIUtil.R(_viewModel.Width - 5f - 50f, num + 5f + 1f, 50f, 21f), "Copy"))
						{
							individualTranslatorTranslationViewModel.CopyToClipboard();
						}
						GUI.enabled = true;
						DrawTextArea(num, positioned, individualTranslatorTranslationViewModel.Translator.Endpoint.Endpoint.FriendlyName, individualTranslatorTranslationViewModel.Translation.Translations);
						num += _viewModel.Height;
					}
				}
			}
			else
			{
				GUI.enabled = false;
				GUI.Button(GUIUtil.R(_viewModel.Width - 5f - 50f, num + 5f + 1f, 50f, 21f), "Copy");
				GUI.enabled = true;
				DrawTextArea(num, _originalText, "Original Text", Empty);
				num += _viewModel.Height;
				GUI.enabled = false;
				GUI.Button(GUIUtil.R(_viewModel.Width - 5f - 50f, num + 5f + 1f, 50f, 21f), "Copy");
				GUI.enabled = true;
				DrawTextArea(num, _defaultTranslation, "Default Translation", Empty);
				num += _viewModel.Height;
				for (int j = 0; j < _viewModel.AvailableTranslators.Count; j++)
				{
					TranslatorViewModel translatorViewModel = _viewModel.AvailableTranslators[j];
					if (translatorViewModel.IsEnabled)
					{
						ScrollPositioned<TranslatorViewModel> positioned2 = _translationViews[j];
						GUI.enabled = false;
						GUI.Button(GUIUtil.R(_viewModel.Width - 5f - 50f, num + 5f + 1f, 50f, 21f), "Copy");
						GUI.enabled = true;
						DrawTextArea(num, positioned2, translatorViewModel.Endpoint.Endpoint.FriendlyName, Empty);
						num += _viewModel.Height;
					}
				}
			}
			num += 15f;
			bool enabled = GUI.enabled;
			GUI.enabled = _viewModel.HasPrevious();
			if (GUI.Button(GUIUtil.R(5f, num, 75f, 21f), "Previous"))
			{
				_viewModel.MovePrevious();
			}
			GUI.enabled = _viewModel.HasNext();
			if (GUI.Button(GUIUtil.R(85f, num, 75f, 21f), "Next"))
			{
				_viewModel.MoveNext();
			}
			GUI.enabled = _viewModel.HasNext();
			if (GUI.Button(GUIUtil.R(165f, num, 75f, 21f), "Last"))
			{
				_viewModel.MoveLatest();
			}
			GUI.enabled = true;
			if (GUI.Button(GUIUtil.R(_viewModel.Width - 5f - 75f, num, 75f, 21f), "Options"))
			{
				_viewModel.IsShowingOptions = true;
			}
			GUI.enabled = enabled;
			GUI.DragWindow();
		}
		finally
		{
			AutoTranslationPlugin.Current.EnableAutoTranslator();
		}
	}

	private void DrawTextArea(float posy, ScrollPositioned positioned, string title, IEnumerable<string> texts)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		GUI.Label(GUIUtil.R(10f, posy + 5f, _viewModel.Width - 10f, 21f), title);
		posy += 26f;
		float width = _viewModel.Width - 10f;
		float height = _viewModel.Height - 21f;
		GUILayout.BeginArea(GUIUtil.R(5f, posy, width, height));
		positioned.ScrollPosition = GUILayout.BeginScrollView(positioned.ScrollPosition, GUI.skin.box);
		foreach (string text in texts)
		{
			GUILayout.Label(text, GUIUtil.LabelTranslation, ArrayHelper.Null<GUILayoutOption>());
		}
		GUILayout.EndScrollView();
		GUILayout.EndArea();
	}
}
