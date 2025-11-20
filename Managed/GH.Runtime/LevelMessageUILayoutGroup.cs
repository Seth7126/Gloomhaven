using System;
using System.Collections;
using System.Collections.Generic;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class LevelMessageUILayoutGroup : MonoBehaviour
{
	[Serializable]
	public struct UILevelMessagePrefabTuple
	{
		public CLevelMessage.ELevelMessageLayoutType type;

		public LevelMessageUILayout uiMessage;
	}

	public List<UILevelMessagePrefabTuple> LevelMessageLayouts;

	private UIWindow window;

	private Action onCloseButtonPressed;

	private int lastFrameHidden = -1;

	private Coroutine autocloseCoroutine;

	private UIWindow.EscapeKeyAction defaultEscapable;

	private LevelMessageUILayout _currentMessage;

	public static bool IsShown { get; private set; }

	private void Awake()
	{
		window = GetComponent<UIWindow>();
		window.onHidden.AddListener(StopListenAutoclose);
		defaultEscapable = window.escapeKeyAction;
	}

	public void Show(CLevelMessage message, Action onClosedButtonPressed = null, Func<bool> autocloseCondition = null)
	{
		IsShown = true;
		LevelMessageUILayout uiMsg = null;
		bool wasOpen = false;
		StopListenAutoclose();
		LevelMessageLayouts.ForEach(delegate(UILevelMessagePrefabTuple it)
		{
			if (it.type == message.LayoutType)
			{
				uiMsg = it.uiMessage;
				wasOpen = it.uiMessage.gameObject.activeSelf;
			}
			it.uiMessage.Hide();
		});
		uiMsg.Init(message, onClosedButtonPressed, !wasOpen || (!window.IsOpen && lastFrameHidden != Time.frameCount));
		onCloseButtonPressed = onClosedButtonPressed;
		window.escapeKeyAction = ((message.LayoutType != CLevelMessage.ELevelMessageLayoutType.HelpText) ? defaultEscapable : UIWindow.EscapeKeyAction.None);
		window.Show();
		if (autocloseCondition != null && onCloseButtonPressed != null)
		{
			autocloseCoroutine = StartCoroutine(Autoclose(autocloseCondition));
		}
		if (Singleton<ESCMenu>.Instance != null && Singleton<ESCMenu>.Instance.IsOpen)
		{
			UIWindowManager.UnregisterEscapable(window);
		}
		_currentMessage = uiMsg;
	}

	public void EnableShownAreas()
	{
		UIWindowManager.RegisterEscapable(window);
		_currentMessage?.EnableArea();
	}

	private IEnumerator Autoclose(Func<bool> checker)
	{
		yield return new WaitUntil(checker);
		autocloseCoroutine = null;
		onCloseButtonPressed?.Invoke();
	}

	public void HideWindow()
	{
		LevelMessageLayouts.ForEach(delegate(UILevelMessagePrefabTuple it)
		{
			it.uiMessage.Clear();
		});
		window.Hide();
		_currentMessage = null;
		StartCoroutine(SkipAFrameAndSetIsShownToFalse());
	}

	private IEnumerator SkipAFrameAndSetIsShownToFalse()
	{
		yield return new WaitForEndOfFrame();
		IsShown = false;
	}

	private void StopListenAutoclose()
	{
		if (autocloseCoroutine != null)
		{
			StopCoroutine(autocloseCoroutine);
			autocloseCoroutine = null;
		}
	}
}
