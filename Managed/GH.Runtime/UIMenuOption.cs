using System;
using AsmodeeNet.Foundation;
using JetBrains.Annotations;
using SM.Gamepad;
using Script.GUI.IngameMenu;
using UnityEngine;
using UnityEngine.UI;

public class UIMenuOption : MonoBehaviour
{
	[Header("Highlight")]
	[SerializeField]
	protected Image highlightImage;

	[SerializeField]
	protected Color highlightColor;

	[SerializeField]
	protected float unhighlightDuration = 0.1f;

	[Header("Select")]
	[SerializeField]
	private GUIAnimator selectAnimation;

	[SerializeField]
	private LoopAnimator selectedAnimation;

	[SerializeField]
	private FrameView _frame;

	[SerializeField]
	private UINavigationSelectable _navigationSelectable;

	[SerializeField]
	private bool _ignoreSelectedOnRefresh;

	protected bool isHovered;

	private Action<bool> onHovered;

	private Action onSelected;

	private Action onDeselected;

	protected bool isSelected;

	protected LTDescr unhighlightAnimation;

	public UINavigationSelectable Selectable => _navigationSelectable;

	public bool IsSelected => isSelected;

	private void OnValidate()
	{
		if ((object)_navigationSelectable == null)
		{
			_navigationSelectable = GetComponent<UINavigationSelectable>();
		}
	}

	private void OnEnable()
	{
		if (InputManager.GamePadInUse && _frame != null)
		{
			RefreshOnEventsGamepad();
		}
	}

	protected virtual void OnDisable()
	{
		if (InputManager.GamePadInUse)
		{
			UnsubscribeOnEventsGamepad();
		}
		CancelHighlightAnimations();
		CancelSelectedAnimation();
	}

	public virtual void Init(Action onSelected, Action onDeselected = null, Action<bool> onHovered = null, bool isSelected = false)
	{
		this.onHovered = onHovered;
		this.onSelected = onSelected;
		this.onDeselected = onDeselected;
		CancelSelectedAnimation();
		CancelHighlightAnimations();
		OnHovered(hovered: false);
		SetSelected(isSelected);
		InputManager.DeviceChangedEvent = (Action)Delegate.Combine(InputManager.DeviceChangedEvent, new Action(OnChangeInputDevice));
	}

	[UsedImplicitly]
	protected virtual void OnDestroy()
	{
		onHovered = null;
		onSelected = null;
		onDeselected = null;
		CancelHighlightAnimations();
		InputManager.DeviceChangedEvent = (Action)Delegate.Remove(InputManager.DeviceChangedEvent, new Action(OnChangeInputDevice));
	}

	public virtual void SetSelected(bool isSelected)
	{
		this.isSelected = isSelected;
		if (!isSelected)
		{
			CancelSelectedAnimation();
		}
		RefreshHighlight();
	}

	protected virtual void OnHovered(bool hovered)
	{
		isHovered = hovered;
		if (!isSelected || _ignoreSelectedOnRefresh)
		{
			RefreshHighlight();
		}
		onHovered?.Invoke(hovered);
	}

	public virtual void Select()
	{
		if (isSelected)
		{
			return;
		}
		SetSelected(isSelected: true);
		selectAnimation.Play();
		onSelected?.Invoke();
		if (_frame != null && isSelected)
		{
			if (InputManager.GamePadInUse)
			{
				_frame.Deactivate();
			}
			else
			{
				_frame.Activate();
			}
		}
	}

	public virtual void Deselect()
	{
		if (!isSelected)
		{
			return;
		}
		CancelSelectedAnimation();
		SetSelected(isSelected: false);
		onDeselected?.Invoke();
		if (_frame != null && !isSelected)
		{
			if (InputManager.GamePadInUse)
			{
				_frame.Activate();
			}
			else
			{
				_frame.Deactivate();
			}
		}
	}

	protected void ToggleSelection(bool isOn)
	{
		if (isOn)
		{
			Select();
		}
		else
		{
			Deselect();
		}
	}

	protected void RefreshHighlight()
	{
		RefreshHighlight(isHovered || isSelected);
	}

	protected virtual void RefreshHighlight(bool isHighlighted)
	{
		if (isHighlighted)
		{
			CancelHighlightAnimations();
			highlightImage.color = highlightColor;
		}
		else if (unhighlightAnimation == null)
		{
			CancelHighlightAnimations();
			unhighlightAnimation = LeanTween.alpha(highlightImage.transform as RectTransform, 0f, unhighlightDuration).setIgnoreTimeScale(useUnScaledTime: true).setOnComplete((Action)delegate
			{
				unhighlightAnimation = null;
			});
		}
	}

	private void CancelSelectedAnimation()
	{
		selectAnimation.Stop();
		selectedAnimation.StopLoop();
		selectAnimation.GoInitState();
	}

	protected void CancelHighlightAnimations()
	{
		if (unhighlightAnimation != null)
		{
			LeanTween.cancel(unhighlightAnimation.id);
			unhighlightAnimation = null;
			highlightImage.SetAlpha(0f);
		}
	}

	private void RefreshOnEventsGamepad()
	{
		UnsubscribeOnEventsGamepad();
		SubscribeOnEventsGamepad();
	}

	private void SubscribeOnEventsGamepad()
	{
		_navigationSelectable.OnNavigationSelectedEvent += OnSelectableSelected;
		_navigationSelectable.OnNavigationDeselectedEvent += OnSelectableDeselected;
	}

	private void UnsubscribeOnEventsGamepad()
	{
		_navigationSelectable.OnNavigationSelectedEvent -= OnSelectableSelected;
		_navigationSelectable.OnNavigationDeselectedEvent -= OnSelectableDeselected;
	}

	private void OnSelectableSelected(IUiNavigationSelectable obj)
	{
		_frame.Activate();
	}

	private void OnSelectableDeselected(IUiNavigationSelectable obj)
	{
		if (!CoreApplication.IsQuitting)
		{
			_frame.Deactivate();
		}
	}

	public void ClearFrame()
	{
		if (_frame != null)
		{
			_frame.Deactivate();
		}
	}

	private void OnChangeInputDevice()
	{
		if (_frame != null)
		{
			if (InputManager.GamePadInUse)
			{
				RefreshOnEventsGamepad();
			}
			else
			{
				UnsubscribeOnEventsGamepad();
			}
		}
	}
}
