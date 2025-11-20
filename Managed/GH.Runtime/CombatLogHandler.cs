#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AsmodeeNet.Foundation;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using SharedLibrary.SimpleLog;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class CombatLogHandler : Singleton<CombatLogHandler>, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private class LogInfo
	{
		public string text;

		public List<CombatLogFilter> filters;

		public CombatLogText obj;

		public Sprite background;

		public CombatLogText prefab;

		public LogInfo(string text, CombatLogText prefab, Sprite background = null, List<CombatLogFilter> filters = null, CombatLogText obj = null)
		{
			this.text = text;
			this.filters = filters;
			this.obj = obj;
			this.background = background;
			this.prefab = prefab;
		}
	}

	[SerializeField]
	private ControllerInputArea controllerArea;

	[SerializeField]
	private Hotkey hotkey;

	[Header("Texts")]
	[SerializeField]
	private CombatLogText combatLogTextHighlighted;

	[SerializeField]
	private CombatLogText combatLogText;

	[SerializeField]
	private Sprite enemyHighlightImage;

	[SerializeField]
	private Sprite playerHighlightImage;

	[SerializeField]
	private Sprite summonHighlightImage;

	[SerializeField]
	private Sprite roundHighlightImage;

	[Header("Structure")]
	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private RectTransform combatLogWindow;

	[SerializeField]
	private GameObject moreImageTop;

	[SerializeField]
	private GameObject moreImageBottom;

	[Header("Minimize/Expand")]
	[SerializeField]
	private float minimizeToPercent = 0.5f;

	[SerializeField]
	private float expandAnimationTime = 0.5f;

	private bool expanded;

	private bool hovered;

	private List<LogInfo> logs = new List<LogInfo>();

	private LTDescr minAnimation;

	private UIWindow window;

	private bool isWindowShown;

	private const string DebugCancel = "CombatLog";

	protected override void Awake()
	{
		base.Awake();
		window = GetComponent<UIWindow>();
		window.onShown.AddListener(OnWindowShown);
		window.onHidden.AddListener(OnWindowHidden);
		scrollRect.onValueChanged.AddListener(delegate
		{
			CalculateLimits();
		});
		moreImageTop.SetActive(value: false);
		moreImageTop.SetActive(value: false);
		combatLogWindow.anchorMax = new Vector2(1f, minimizeToPercent);
		expanded = false;
		if (InputManager.GamePadInUse)
		{
			hotkey.Initialize(Singleton<UINavigation>.Instance.Input);
		}
	}

	public void ToggleHotkey(bool activity)
	{
		if (InputManager.GamePadInUse)
		{
			hotkey.gameObject.SetActive(activity);
		}
	}

	private void OnWindowShown()
	{
		isWindowShown = true;
		UpdateLogs();
		scrollRect.verticalNormalizedPosition = 0f;
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.CONTROL_COMBAT_LOG, ToggleExpand));
	}

	private void OnWindowHidden()
	{
		isWindowShown = false;
		Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.CONTROL_COMBAT_LOG, ToggleExpand);
	}

	private void ToggleExpand()
	{
		if (expanded)
		{
			MinimizeLog();
		}
		else
		{
			ExpandLog();
		}
	}

	protected override void OnDestroy()
	{
		window.onShown.RemoveAllListeners();
		window.onHidden.RemoveAllListeners();
		scrollRect.onValueChanged.RemoveAllListeners();
		if (InputManager.GamePadInUse)
		{
			hotkey.Deinitialize();
		}
		if (isWindowShown)
		{
			OnWindowHidden();
		}
		CancelAnimation();
		base.OnDestroy();
	}

	private void Start()
	{
		StartCoroutine(Initialize());
	}

	private IEnumerator Initialize()
	{
		yield return new WaitForEndOfFrame();
		if (SaveData.Instance.Global.DisabledCombatLog)
		{
			window.Hide();
		}
		else
		{
			window.Show();
		}
	}

	private void OnEnable()
	{
		CombatLogSettings.OnCombatLogFiltersChanged = (Action)Delegate.Combine(CombatLogSettings.OnCombatLogFiltersChanged, new Action(UpdateLogs));
		CombatLogSettings.OnEnableCombatLog = (Action)Delegate.Combine(CombatLogSettings.OnEnableCombatLog, new Action(ShowWindow));
		CombatLogSettings.OnDisableCombatLog = (Action)Delegate.Combine(CombatLogSettings.OnDisableCombatLog, new Action(HideWindow));
	}

	private void ShowWindow()
	{
		window.Show();
	}

	private void HideWindow()
	{
		window.Hide();
	}

	private void OnDisable()
	{
		if (CoreApplication.IsQuitting)
		{
			return;
		}
		CombatLogSettings.OnCombatLogFiltersChanged = (Action)Delegate.Remove(CombatLogSettings.OnCombatLogFiltersChanged, new Action(UpdateLogs));
		CombatLogSettings.OnEnableCombatLog = (Action)Delegate.Remove(CombatLogSettings.OnEnableCombatLog, new Action(ShowWindow));
		CombatLogSettings.OnDisableCombatLog = (Action)Delegate.Remove(CombatLogSettings.OnDisableCombatLog, new Action(HideWindow));
		if (ObjectPool.instance != null)
		{
			foreach (LogInfo item in logs.Where((LogInfo it) => it.obj != null))
			{
				ObjectPool.Recycle(item.obj.gameObject, item.prefab.gameObject);
			}
		}
		CancelAnimation();
	}

	private bool IsFiltered(List<CombatLogFilter> filters)
	{
		return filters?.Exists((CombatLogFilter filter) => !SaveData.Instance.Global.DisabledCombatLogFilters.Contains(filter)) ?? true;
	}

	private void UpdateLogs()
	{
		int i = 0;
		logs.ForEach(delegate(LogInfo it)
		{
			if (IsFiltered(it.filters))
			{
				if (it.obj == null)
				{
					it.obj = CreateLog(it.prefab, it.text, it.background);
					it.obj.transform.SetSiblingIndex(i);
				}
				else
				{
					it.obj.gameObject.SetActive(value: true);
				}
				i++;
			}
			else if (it.obj != null)
			{
				ObjectPool.Recycle(it.obj.gameObject);
				it.obj = null;
			}
		});
		CalculateLimits();
	}

	public void AddLog(string text, CombatLogFilter filter)
	{
		AddLog(text, new List<CombatLogFilter> { filter });
	}

	public void AddLog(string text, List<CombatLogFilter> filters = null)
	{
		AddLog(combatLogText, text, null, filters, null);
	}

	public void AddHighlightedLog(string text, CActor.EType? type, List<CombatLogFilter> filters = null, string iconText = null)
	{
		Sprite background;
		if (type == CActor.EType.Player)
		{
			text = ((iconText == null) ? $"<color=#a7cc7b>{text}</color>" : $"{iconText} <color=#a7cc7b>{text}</color>");
			background = playerHighlightImage;
		}
		else if (type == CActor.EType.Enemy)
		{
			text = ((iconText == null) ? $"<color=#f16e6e>{text}</color>" : $"{iconText} <color=#f16e6e>{text}</color>");
			background = enemyHighlightImage;
		}
		else if (type == CActor.EType.HeroSummon)
		{
			text = ((iconText == null) ? $"<color=#d4d979>{text}</color>" : $"{iconText} <color=#d4d979>{text}</color>");
			background = summonHighlightImage;
		}
		else
		{
			background = roundHighlightImage;
		}
		AddLog(combatLogTextHighlighted, text, type, filters, background);
	}

	private void AddLog(CombatLogText log, string text, CActor.EType? type, List<CombatLogFilter> filters, Sprite background)
	{
		CombatLogText obj = null;
		if (window.IsOpen && IsFiltered(filters))
		{
			obj = CreateLog(log, text, background);
			CalculateLimits();
		}
		logs.Add(new LogInfo(text, log, background, filters, obj));
		text = Regex.Replace(text, "<sprite name=([^>]+)>", "[$1]");
		SimpleLog.AddToSimpleLog(text);
	}

	private CombatLogText CreateLog(CombatLogText log, string text, Sprite background)
	{
		CombatLogText combatLogText = ObjectPool.Spawn(log, scrollRect.content);
		Debug.Log("COMBAT LOG DEBUGGING: Spawned log with text: " + text + "and Prefab Name: " + combatLogText.gameObject.name);
		combatLogText.Init(text, background);
		return combatLogText;
	}

	[ContextMenu("Clear Log")]
	public void ClearLog()
	{
		foreach (LogInfo item in logs.Where((LogInfo it) => it.obj != null))
		{
			ObjectPool.Recycle(item.obj.gameObject);
		}
		logs.Clear();
		moreImageBottom.SetActive(value: false);
		moreImageTop.SetActive(value: false);
		combatLogWindow.anchorMax = new Vector2(1f, minimizeToPercent);
		expanded = false;
	}

	private void CalculateLimits()
	{
		if (NeedsVerticalScroll())
		{
			moreImageBottom.SetActive((double)scrollRect.normalizedPosition.y > 0.01);
			moreImageTop.SetActive((double)scrollRect.normalizedPosition.y < 0.99);
			if (hovered)
			{
				ExpandLog();
			}
		}
		else
		{
			moreImageBottom.SetActive(value: false);
			moreImageTop.SetActive(value: false);
		}
	}

	private bool NeedsVerticalScroll()
	{
		if (scrollRect == null || scrollRect.content == null || scrollRect.viewport == null)
		{
			return false;
		}
		return scrollRect.content.sizeDelta.y > scrollRect.viewport.sizeDelta.y;
	}

	public void ExpandLog()
	{
		if (!expanded)
		{
			expanded = true;
			CancelAnimation();
			minAnimation = LeanTween.value(combatLogWindow.gameObject, delegate(float val)
			{
				combatLogWindow.anchorMax = new Vector2(1f, val);
			}, combatLogWindow.anchorMax.y, 1f, expandAnimationTime).setOnComplete((Action)delegate
			{
				minAnimation = null;
			});
			CalculateLimits();
		}
	}

	public void MinimizeLog()
	{
		if (expanded)
		{
			expanded = false;
			CancelAnimation();
			minAnimation = LeanTween.value(combatLogWindow.gameObject, delegate(float val)
			{
				combatLogWindow.anchorMax = new Vector2(1f, val);
			}, combatLogWindow.anchorMax.y, minimizeToPercent, expandAnimationTime).setOnComplete((Action)delegate
			{
				minAnimation = null;
			});
			CalculateLimits();
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		hovered = true;
		ExpandLog();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		hovered = false;
		MinimizeLog();
	}

	private void CancelAnimation()
	{
		if (minAnimation != null)
		{
			LeanTween.cancel(minAnimation.id, "CombatLog");
		}
		minAnimation = null;
	}

	private void OnControllerAreaFocused()
	{
		ExpandLog();
		InputManager.RegisterToOnPressed(KeyAction.CONTROL_COMBAT_LOG, controllerArea.Unfocus);
	}

	private void OnControllerAreaUnfocused()
	{
		InputManager.UnregisterToOnPressed(KeyAction.CONTROL_COMBAT_LOG, controllerArea.Unfocus);
		MinimizeLog();
	}
}
