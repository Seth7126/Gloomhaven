using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Script.GUI.SMNavigation;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI.Tweens;

namespace UnityEngine.UI;

[DisallowMultipleComponent]
[ExecuteInEditMode]
[AddComponentMenu("UI/UIWindow", 58)]
[RequireComponent(typeof(CanvasGroup))]
public class UIWindow : MonoBehaviour, ISelectHandler, IEventSystemHandler, IPointerDownHandler, IEscapable, IShowActivity
{
	public enum Transition
	{
		Instant,
		Fade
	}

	public enum VisualState
	{
		Shown,
		Hidden
	}

	public enum EscapeKeyAction
	{
		None,
		Hide,
		HideIfFocused,
		Toggle,
		HideOnlyThis,
		Skip
	}

	public enum BackKeyAction
	{
		None,
		Hide,
		HideIfFocused,
		Toggle
	}

	public enum FocusedAction
	{
		BringToFront,
		None,
		SetAsLastChildren
	}

	[Serializable]
	public class TransitionBeginEvent : UnityEvent<UIWindow, VisualState, bool>
	{
	}

	[Serializable]
	public class TransitionCompleteEvent : UnityEvent<UIWindow, VisualState>
	{
	}

	[Serializable]
	public class VisibilityEvent : UnityEvent
	{
	}

	private static readonly HashSet<UIWindow> _uiWindows = new HashSet<UIWindow>();

	protected static UIWindow m_FocusedWindow;

	[SerializeField]
	private UIWindowID m_WindowId;

	[SerializeField]
	private int m_CustomWindowId;

	[SerializeField]
	private VisualState m_StartingState = VisualState.Hidden;

	[SerializeField]
	private EscapeKeyAction m_EscapeKeyAction = EscapeKeyAction.Hide;

	[SerializeField]
	private BackKeyAction m_BackKeyAction = BackKeyAction.Hide;

	[SerializeField]
	private FocusedAction m_FocusedAction;

	[SerializeField]
	private List<UIWindowID> m_EscapeActionAdditionalWindows;

	[SerializeField]
	private Transition m_Transition;

	[SerializeField]
	private TweenEasing m_TransitionEasing = TweenEasing.InOutQuint;

	[SerializeField]
	private float m_TransitionDuration = 0.1f;

	[SerializeField]
	private string m_AudioItemShow;

	[SerializeField]
	private string m_AudioItemHide;

	[SerializeField]
	private bool m_DisabledClickToFocus;

	[SerializeField]
	private bool m_DisableOnZeroAlpha;

	[SerializeField]
	private bool m_IsAllowedToEscapeDuringSave;

	[SerializeField]
	private bool _disableCanvas;

	protected bool m_IsFocused;

	private VisualState m_CurrentVisualState = VisualState.Hidden;

	private CanvasGroup m_CanvasGroup;

	private Canvas _canvas;

	public bool enabledInOptions = true;

	public TransitionBeginEvent onTransitionBegin = new TransitionBeginEvent();

	public TransitionCompleteEvent onTransitionComplete = new TransitionCompleteEvent();

	public VisibilityEvent onShown = new VisibilityEvent();

	public VisibilityEvent onHidden = new VisibilityEvent();

	public UnityEvent onFocused = new UnityEvent();

	[NonSerialized]
	private TweenRunner<FloatTween> m_FloatTweenRunner;

	private bool _isActive;

	private bool _otherActive;

	public static UIWindow FocusedWindow
	{
		get
		{
			return m_FocusedWindow;
		}
		private set
		{
			m_FocusedWindow = value;
		}
	}

	public bool IsPopUp { get; set; }

	public bool IsAllowedToEscapeDuringSave => m_IsAllowedToEscapeDuringSave;

	public UIWindowID ID
	{
		get
		{
			return m_WindowId;
		}
		set
		{
			m_WindowId = value;
		}
	}

	public int CustomID
	{
		get
		{
			return m_CustomWindowId;
		}
		set
		{
			m_CustomWindowId = value;
		}
	}

	public EscapeKeyAction escapeKeyAction
	{
		get
		{
			return m_EscapeKeyAction;
		}
		set
		{
			if (m_EscapeKeyAction != value)
			{
				m_EscapeKeyAction = value;
				if ((m_EscapeKeyAction != EscapeKeyAction.None && IsOpen) || escapeKeyAction == EscapeKeyAction.Toggle)
				{
					UIWindowManager.RegisterEscapable(this);
				}
				else
				{
					UIWindowManager.UnregisterEscapable(this);
				}
			}
		}
	}

	public BackKeyAction backKeyAction
	{
		get
		{
			return m_BackKeyAction;
		}
		set
		{
			m_BackKeyAction = value;
		}
	}

	public Transition transition
	{
		get
		{
			return m_Transition;
		}
		set
		{
			m_Transition = value;
		}
	}

	public TweenEasing transitionEasing
	{
		get
		{
			return m_TransitionEasing;
		}
		set
		{
			m_TransitionEasing = value;
		}
	}

	public float transitionDuration
	{
		get
		{
			return m_TransitionDuration;
		}
		set
		{
			m_TransitionDuration = value;
		}
	}

	public List<UIWindowID> escapeActionAdditionalWindows
	{
		get
		{
			return m_EscapeActionAdditionalWindows;
		}
		set
		{
			m_EscapeActionAdditionalWindows = value;
		}
	}

	public Action OnShow { get; set; }

	public Action OnHide { get; set; }

	public Action<bool> OnActivityChanged { get; set; }

	bool IShowActivity.IsActive => base.gameObject.activeSelf;

	public bool HasGoneToStartingState { get; private set; }

	public string AudioItemHide
	{
		get
		{
			return m_AudioItemHide;
		}
		set
		{
			m_AudioItemHide = value;
		}
	}

	public string AudioItemShow
	{
		get
		{
			return m_AudioItemShow;
		}
		set
		{
			m_AudioItemShow = value;
		}
	}

	public bool IsVisible
	{
		get
		{
			if (!(m_CanvasGroup != null) || !(m_CanvasGroup.alpha > 0f))
			{
				return false;
			}
			return true;
		}
	}

	public bool IsOpen => m_CurrentVisualState == VisualState.Shown;

	public bool IsFocused => m_IsFocused;

	public static int NextUnusedCustomID
	{
		get
		{
			HashSet<UIWindow> windows = GetWindows();
			if (windows.Count > 0)
			{
				List<UIWindow> list = new List<UIWindow>(windows);
				list.Sort(SortByCustomWindowID);
				return list[windows.Count - 1].CustomID + 1;
			}
			return 0;
		}
	}

	protected UIWindow()
	{
	}

	[UsedImplicitly]
	private void Awake()
	{
		if (m_FloatTweenRunner == null)
		{
			m_FloatTweenRunner = new TweenRunner<FloatTween>();
		}
		m_FloatTweenRunner.Init(this);
		if (_disableCanvas)
		{
			_canvas = GetComponent<Canvas>();
		}
		if (!_otherActive)
		{
			m_CanvasGroup = base.gameObject.GetComponent<CanvasGroup>();
		}
	}

	protected virtual void Start()
	{
		if (!_otherActive)
		{
			if (CustomID == 0)
			{
				CustomID = NextUnusedCustomID;
			}
			if (Application.isPlaying)
			{
				EvaluateAndTransitionToVisualState(m_StartingState, instant: true);
			}
			HasGoneToStartingState = true;
			if ((escapeKeyAction != EscapeKeyAction.None && IsOpen) || escapeKeyAction == EscapeKeyAction.Toggle)
			{
				UIWindowManager.RegisterEscapable(this);
			}
			ChangeActive();
		}
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		_uiWindows.Remove(this);
		if (m_FocusedWindow == this)
		{
			m_FocusedWindow = null;
		}
		onShown.RemoveAllListeners();
		onHidden.RemoveAllListeners();
		onFocused.RemoveAllListeners();
		OnShow = null;
		OnHide = null;
		OnActivityChanged = null;
		onTransitionComplete.RemoveAllListeners();
		onTransitionBegin.RemoveAllListeners();
	}

	[UsedImplicitly]
	private void OnEnable()
	{
		_uiWindows.Add(this);
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		_uiWindows.Remove(this);
		UIWindowManager.UnregisterEscapable(this);
	}

	public void OtherInit()
	{
		m_CanvasGroup = base.gameObject.GetComponent<CanvasGroup>();
		Start();
		_otherActive = true;
	}

	protected virtual bool IsActive()
	{
		if (base.enabled)
		{
			return base.gameObject.activeInHierarchy;
		}
		return false;
	}

	public virtual void OnSelect(BaseEventData eventData)
	{
		Focus();
	}

	public virtual void OnPointerDown(PointerEventData eventData)
	{
		if (!m_DisabledClickToFocus)
		{
			Focus();
		}
	}

	public virtual void Focus()
	{
		if (!m_IsFocused)
		{
			m_IsFocused = true;
			OnBeforeFocusWindow(this);
			if (m_FocusedAction == FocusedAction.BringToFront)
			{
				UIUtility.BringToFront(base.gameObject);
			}
			else if (m_FocusedAction == FocusedAction.SetAsLastChildren)
			{
				base.gameObject.transform.SetAsLastSibling();
			}
			onFocused.Invoke();
		}
	}

	public virtual void Toggle()
	{
		if (m_CurrentVisualState == VisualState.Shown)
		{
			Hide();
		}
		else
		{
			Show();
		}
	}

	public virtual void Show()
	{
		Show(instant: false);
	}

	public virtual void Show(bool instant)
	{
		if (!IsActive())
		{
			return;
		}
		Focus();
		if (m_CurrentVisualState != VisualState.Shown)
		{
			AudioControllerUtils.PlaySound(m_AudioItemShow);
			if (escapeKeyAction != EscapeKeyAction.None)
			{
				UIWindowManager.RegisterEscapable(this);
			}
			onShown.Invoke();
			OnShow?.Invoke();
			OnActivityChanged?.Invoke(obj: true);
			EvaluateAndTransitionToVisualState(VisualState.Shown, instant);
		}
	}

	public virtual void Hide()
	{
		Hide(instant: false);
	}

	public virtual void HideOrUpdateStartingState(bool instant = false)
	{
		if (HasGoneToStartingState)
		{
			Hide(instant);
		}
		else
		{
			m_StartingState = VisualState.Hidden;
		}
	}

	public virtual void ShowOrUpdateStartingState(bool instant = false)
	{
		if (HasGoneToStartingState)
		{
			Show(instant);
		}
		else
		{
			m_StartingState = VisualState.Shown;
		}
	}

	public virtual void Hide(bool instant)
	{
		if (IsActive() && m_CurrentVisualState != VisualState.Hidden)
		{
			AudioControllerUtils.PlaySound(m_AudioItemHide);
			if (escapeKeyAction != EscapeKeyAction.None && escapeKeyAction != EscapeKeyAction.Toggle)
			{
				UIWindowManager.UnregisterEscapable(this);
			}
			onHidden.Invoke();
			OnHide?.Invoke();
			OnActivityChanged?.Invoke(obj: false);
			EvaluateAndTransitionToVisualState(VisualState.Hidden, instant);
		}
	}

	protected virtual void EvaluateAndTransitionToVisualState(VisualState state, bool instant)
	{
		float num = ((state == VisualState.Shown) ? 1f : 0f);
		if (onTransitionBegin != null)
		{
			onTransitionBegin.Invoke(this, state, instant || m_Transition == Transition.Instant);
		}
		OnTransitionStarted(state, instant);
		m_CurrentVisualState = state;
		if (m_Transition == Transition.Fade)
		{
			float duration = (instant ? 0f : m_TransitionDuration);
			StartAlphaTween(num, duration, ignoreTimeScale: true);
		}
		else
		{
			SetCanvasAlpha(num);
			if (onTransitionComplete != null)
			{
				onTransitionComplete.Invoke(this, state);
			}
			OnTransitionCompleted();
		}
		if (state == VisualState.Shown)
		{
			m_CanvasGroup.blocksRaycasts = true;
			m_CanvasGroup.interactable = true;
		}
	}

	private void OnTransitionStarted(VisualState targetVisualState, bool instant)
	{
		if (!_disableCanvas)
		{
			return;
		}
		switch (targetVisualState)
		{
		case VisualState.Shown:
			_canvas.enabled = true;
			break;
		case VisualState.Hidden:
			if (instant || m_Transition == Transition.Instant)
			{
				_canvas.enabled = false;
			}
			break;
		}
	}

	private void OnTransitionCompleted()
	{
		if (_disableCanvas && m_CurrentVisualState == VisualState.Hidden)
		{
			_canvas.enabled = false;
		}
	}

	public void StartAlphaTween(float targetAlpha, float duration, bool ignoreTimeScale)
	{
		if (m_CanvasGroup == null)
		{
			OnTweenFinished();
			return;
		}
		m_CanvasGroup.blocksRaycasts = false;
		FloatTween info = new FloatTween
		{
			duration = duration,
			startFloat = m_CanvasGroup.alpha,
			targetFloat = targetAlpha
		};
		info.AddOnChangedCallback(SetCanvasAlpha);
		info.AddOnFinishCallback(OnTweenFinished);
		info.ignoreTimeScale = ignoreTimeScale;
		info.easing = m_TransitionEasing;
		m_FloatTweenRunner.StartTween(info);
	}

	protected void SetCanvasAlpha(float alpha)
	{
		if (!(m_CanvasGroup == null))
		{
			m_CanvasGroup.alpha = alpha;
			ChangeActive();
			if (alpha == 0f)
			{
				m_CanvasGroup.blocksRaycasts = false;
				m_CanvasGroup.interactable = false;
			}
		}
	}

	protected virtual void OnTweenFinished()
	{
		m_CanvasGroup.blocksRaycasts = m_CanvasGroup.interactable;
		if (onTransitionComplete != null)
		{
			onTransitionComplete.Invoke(this, m_CurrentVisualState);
		}
		OnTransitionCompleted();
	}

	public static HashSet<UIWindow> GetWindows()
	{
		return _uiWindows;
	}

	public static List<UIWindow> GetWindowsByID(UIWindowID windowID)
	{
		List<UIWindow> list = new List<UIWindow>();
		foreach (UIWindow uiWindow in _uiWindows)
		{
			if (uiWindow.ID == windowID)
			{
				list.Add(uiWindow);
			}
		}
		return list;
	}

	public static int SortByCustomWindowID(UIWindow w1, UIWindow w2)
	{
		return w1.CustomID.CompareTo(w2.CustomID);
	}

	public static UIWindow GetWindow(UIWindowID id)
	{
		foreach (UIWindow window in GetWindows())
		{
			if (window.ID == id)
			{
				return window;
			}
		}
		return null;
	}

	public static UIWindow GetWindowByCustomID(int customId)
	{
		foreach (UIWindow window in GetWindows())
		{
			if (window.CustomID == customId)
			{
				return window;
			}
		}
		return null;
	}

	public static void FocusWindow(UIWindowID id)
	{
		if (GetWindow(id) != null)
		{
			GetWindow(id).Focus();
		}
	}

	protected static void OnBeforeFocusWindow(UIWindow window)
	{
		if (m_FocusedWindow != null)
		{
			m_FocusedWindow.m_IsFocused = false;
		}
		m_FocusedWindow = window;
	}

	public bool Escape()
	{
		if (this == null)
		{
			UIWindowManager.UnregisterEscapable(this);
			return false;
		}
		if (escapeKeyAction == EscapeKeyAction.None)
		{
			return false;
		}
		if (escapeKeyAction == EscapeKeyAction.Skip)
		{
			return true;
		}
		if (IsOpen && (escapeKeyAction == EscapeKeyAction.Hide || escapeKeyAction == EscapeKeyAction.Toggle || escapeKeyAction == EscapeKeyAction.HideOnlyThis || (escapeKeyAction == EscapeKeyAction.HideIfFocused && IsFocused)))
		{
			Hide();
			return true;
		}
		if (!IsOpen && escapeKeyAction == EscapeKeyAction.Toggle)
		{
			Show();
		}
		return false;
	}

	public int Order()
	{
		if (escapeKeyAction != EscapeKeyAction.Toggle || IsOpen)
		{
			return 0;
		}
		return 100;
	}

	private void ChangeActive()
	{
		if (m_DisableOnZeroAlpha)
		{
			base.gameObject.SetActive(m_CanvasGroup.alpha > Mathf.Epsilon);
		}
	}

	public HashSet<UIWindow> GetUIWindows()
	{
		return _uiWindows;
	}
}
