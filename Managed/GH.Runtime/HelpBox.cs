using System;
using System.Collections.Generic;
using GLOOM;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class HelpBox : Singleton<HelpBox>
{
	public enum FormatTarget
	{
		NONE,
		TITLE,
		TEXT,
		ALL
	}

	private class HelpInfo
	{
		public string keyboardTitle;

		public string controllerTitle;

		public Func<string> controllerLines;

		public string keybaordLines;

		public readonly FormatTarget useWarningFormat;

		public HelpInfo(string titleKeyboard, string keybaordLine, string titleController = null, Func<string> controllerLine = null, FormatTarget useWarningFormat = FormatTarget.NONE)
		{
			controllerTitle = titleKeyboard;
			controllerLines = controllerLine;
			keybaordLines = keybaordLine;
			this.useWarningFormat = useWarningFormat;
			controllerTitle = titleController;
		}
	}

	private UIWindow myWindow;

	[SerializeField]
	private HelpBoxLine _boxLinePrefab;

	[SerializeField]
	private List<HelpBoxLine> linesPool;

	[SerializeField]
	private Canvas _canvas;

	private string currentId;

	private HelpInfo m_HelpInfo;

	private readonly int _canvasTopSortingOrder = 51;

	private readonly int _canvasDefaultSortingOrder;

	public Func<string> ControllerText => m_HelpInfo?.controllerLines;

	public string ControllerTitle => m_HelpInfo?.controllerTitle;

	protected override void Awake()
	{
		base.Awake();
		myWindow = GetComponent<UIWindow>();
	}

	private void ShowGamepad()
	{
		if (m_HelpInfo != null && m_HelpInfo.controllerLines != null)
		{
			string text = m_HelpInfo.controllerLines();
			if (text.IsNullOrEmpty())
			{
				myWindow.Hide();
			}
			else
			{
				RefreshTranslated(text, m_HelpInfo.controllerTitle ?? m_HelpInfo.keyboardTitle, m_HelpInfo.useWarningFormat);
			}
		}
	}

	private void ShowKeyboard()
	{
		if (m_HelpInfo != null)
		{
			if (m_HelpInfo.keybaordLines.IsNullOrEmpty())
			{
				myWindow.Hide();
			}
			else
			{
				RefreshTranslated(m_HelpInfo.keybaordLines, m_HelpInfo.keyboardTitle, m_HelpInfo.useWarningFormat);
			}
		}
	}

	private void ClearHelpInfo()
	{
		m_HelpInfo = null;
		SetSortingOrder(_canvasDefaultSortingOrder);
		InputManager.OnControllerTypeChangedEvent = (Action<ControllerType>)Delegate.Remove(InputManager.OnControllerTypeChangedEvent, new Action<ControllerType>(OnControllerTypeChanged));
	}

	private void SetSortingOrder(int sortingOrder)
	{
		if (_canvas != null)
		{
			_canvas.sortingOrder = sortingOrder;
		}
	}

	public void ShowControllerOrKeyboardTip(string keyboardText, Func<string> controllerText, string titleKeyboard = null, string titleController = null, string id = null)
	{
		currentId = id;
		m_HelpInfo = new HelpInfo(titleKeyboard, keyboardText, titleController, controllerText);
		InputManager.OnControllerTypeChangedEvent = (Action<ControllerType>)Delegate.Remove(InputManager.OnControllerTypeChangedEvent, new Action<ControllerType>(OnControllerTypeChanged));
		InputManager.OnControllerTypeChangedEvent = (Action<ControllerType>)Delegate.Combine(InputManager.OnControllerTypeChangedEvent, new Action<ControllerType>(OnControllerTypeChanged));
		if (InputManager.GamePadInUse)
		{
			ShowGamepad();
		}
		else
		{
			ShowKeyboard();
		}
	}

	public void OverrideControllerOrKeyboardTip(Func<string> controllerText, string controllerTitle = null)
	{
		if (m_HelpInfo == null)
		{
			ShowControllerOrKeyboardTip(null, controllerText, null, controllerTitle);
			return;
		}
		m_HelpInfo.controllerLines = controllerText;
		m_HelpInfo.controllerTitle = controllerTitle;
		if (InputManager.GamePadInUse)
		{
			ShowGamepad();
		}
	}

	public void OverrideKeyboardTip(string text, string title = null, string id = null)
	{
		if (m_HelpInfo != null)
		{
			currentId = id;
			m_HelpInfo.keyboardTitle = title;
			m_HelpInfo.keyboardTitle = text;
			if (myWindow.IsOpen && !InputManager.GamePadInUse)
			{
				ShowKeyboard();
			}
		}
	}

	public void ClearOverrideController()
	{
		if (m_HelpInfo != null)
		{
			m_HelpInfo.controllerLines = null;
			if (myWindow.IsOpen && InputManager.GamePadInUse)
			{
				ShowKeyboard();
			}
		}
	}

	private void OnControllerTypeChanged(ControllerType obj)
	{
		if (InputManager.GamePadInUse)
		{
			ShowGamepad();
		}
		else
		{
			ShowKeyboard();
		}
	}

	public void ShowTranslated(List<string> texts, bool useWarningFormat = false, string id = null)
	{
		ClearHelpInfo();
		currentId = id;
		HelperTools.NormalizePool(ref linesPool, _boxLinePrefab.gameObject, base.transform, texts.Count);
		for (int i = 0; i < texts.Count; i++)
		{
			linesPool[i].ShowTranslated(texts[i], null, null, useWarningFormat);
		}
		myWindow.Show();
	}

	public void Show(List<string> texts, string titleLoc = null, FormatTarget useWarningFormat = FormatTarget.NONE, string id = null)
	{
		ClearHelpInfo();
		currentId = id;
		HelperTools.NormalizePool(ref linesPool, _boxLinePrefab.gameObject, base.transform, texts.Count);
		for (int i = 0; i < texts.Count; i++)
		{
			linesPool[i].Show(texts[i], titleLoc, null, useWarningFormat);
		}
		myWindow.Show();
	}

	public void Show(string textLoc, string titleLoc = null, string prefix = null, FormatTarget useWarningFormat = FormatTarget.NONE, string id = null)
	{
		ShowTranslated(LocalizationManager.GetTranslation(textLoc), titleLoc.IsNullOrEmpty() ? null : LocalizationManager.GetTranslation(titleLoc), useWarningFormat, id);
	}

	public void Show(string tipKey, string titleKey, bool useWarningFormat)
	{
		Show(tipKey, titleKey, null, useWarningFormat ? FormatTarget.ALL : FormatTarget.NONE);
	}

	public void Show(string tipKey, bool useWarningFormat)
	{
		Show(tipKey, null, null, useWarningFormat ? FormatTarget.ALL : FormatTarget.NONE);
	}

	public void ShowTranslated(string text, string title = null, FormatTarget useWarningFormat = FormatTarget.NONE, string id = null)
	{
		ClearHelpInfo();
		currentId = id;
		m_HelpInfo = new HelpInfo(title, text, null, null, useWarningFormat);
		InputManager.OnControllerTypeChangedEvent = (Action<ControllerType>)Delegate.Remove(InputManager.OnControllerTypeChangedEvent, new Action<ControllerType>(OnControllerTypeChanged));
		InputManager.OnControllerTypeChangedEvent = (Action<ControllerType>)Delegate.Combine(InputManager.OnControllerTypeChangedEvent, new Action<ControllerType>(OnControllerTypeChanged));
		RefreshTranslated(text, title, useWarningFormat);
	}

	private void RefreshTranslated(string text, string title = null, FormatTarget useWarningFormat = FormatTarget.NONE)
	{
		HelperTools.NormalizePool(ref linesPool, _boxLinePrefab.gameObject, base.transform, 1);
		linesPool[0].ShowTranslatedWarning(useWarningFormat, text, title);
		myWindow.Show();
	}

	public void Hide(string id = null, bool onlyIfIdMatches = false)
	{
		if (id == null || (!onlyIfIdMatches && currentId == null) || !(id != currentId))
		{
			currentId = null;
			ClearHelpInfo();
			for (int i = 1; i < linesPool.Count && linesPool[i].gameObject.activeSelf; i++)
			{
				linesPool[i].Hide();
			}
			myWindow.Hide();
		}
	}

	public void HideKeyboard(string id = null, bool onlyIfIdMatches = false)
	{
		if (id != null && (onlyIfIdMatches || currentId != null) && id != currentId)
		{
			return;
		}
		if (m_HelpInfo == null)
		{
			Hide();
			return;
		}
		currentId = null;
		m_HelpInfo.keybaordLines = null;
		m_HelpInfo.keyboardTitle = null;
		if (!InputManager.GamePadInUse)
		{
			ShowKeyboard();
		}
	}

	public void HighlightWarning()
	{
		for (int i = 0; i < linesPool.Count && linesPool[i].gameObject.activeSelf; i++)
		{
			linesPool[i].HighlightWarning();
		}
	}

	public void BringToFront()
	{
		SetSortingOrder(_canvasTopSortingOrder);
		UIUtility.BringToFront(myWindow.gameObject);
	}

	private void OnDisable()
	{
		ClearHelpInfo();
	}
}
