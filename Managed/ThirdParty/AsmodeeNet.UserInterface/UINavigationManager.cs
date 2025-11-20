using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Utils;
using AsmodeeNet.Utils.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities;

namespace AsmodeeNet.UserInterface;

public class UINavigationManager : MonoBehaviour
{
	public delegate bool ForceToGiveFocus();

	public delegate void HighlightFocusable(Focusable focusable);

	public enum Direction
	{
		Left,
		Up,
		Right,
		Down,
		Next,
		Previous
	}

	private struct FocusablePosition
	{
		public Focusable focusable;

		public Vector2 deltaPosition;

		public float squaredDistance;

		public FocusablePosition(Focusable focusable, Vector2 deltaPosition, float squaredDistance)
		{
			this.focusable = focusable;
			this.deltaPosition = deltaPosition;
			this.squaredDistance = squaredDistance;
		}
	}

	public enum GizmosLevel
	{
		Minimalist,
		HorizontalGradient,
		VerticalGradient
	}

	private const string _documentation = "<b>UINavigationManager</b> works with <b>Focusable</b> and <b>Selectable</b> to provide relevant UI Navigation on all platforms";

	private const string _kModuleName = "UINavigationManager";

	private FocusableLayer _rootFocusableLayer;

	private List<FocusableLayer> _focusableLayers = new List<FocusableLayer>();

	private Focusable _currentFocusable;

	private List<Focusable> _focusables = new List<Focusable>();

	private bool _isEditingInputField;

	public ForceToGiveFocus forceToGiveFocus;

	public HighlightFocusable highlightFocusable;

	public UINavigationInput navigationInput;

	private BasicControl _basicControl;

	[Range(0f, 10f)]
	public float directionalWeight = 2f;

	public GizmosLevel gizmosLevel;

	private int _ignoringInteractionEventsRequestCount;

	public FocusableLayer RootFocusableLayer
	{
		get
		{
			if (_rootFocusableLayer == null)
			{
				if (_focusableLayers.Count > 0)
				{
					_rootFocusableLayer = Enumerable.First(_focusableLayers);
				}
				else
				{
					AsmoLogger.Error("UINavigationManager", "Missing RootFocusableLayer");
				}
			}
			return _rootFocusableLayer;
		}
	}

	public bool HasFocus => _currentFocusable != null;

	public event Action OnBackAction;

	private void Awake()
	{
		forceToGiveFocus = () => InputSystemUtilities.GetJoystickNames().Length != 0;
		highlightFocusable = delegate(Focusable focusable)
		{
			focusable.StopCoroutine(focusable.transform.Bounce());
			focusable.StartCoroutine(focusable.transform.Bounce());
		};
	}

	private void Start()
	{
		if (navigationInput == null)
		{
			AsmoLogger.Error("UINavigationManager", "\"navigationInput\" is null. #define UINAVIGATION, provide your own version or deactivate UINavigationManager in CoreApplication");
		}
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	public void RegisterFocusableLayer(FocusableLayer focusableLayer)
	{
		AsmoLogger.Trace("UINavigationManager", "RegisterFocusableLayer " + focusableLayer.gameObject.name);
		_focusableLayers.Add(focusableLayer);
	}

	public void UnRegisterFocusableLayer(FocusableLayer focusableLayer)
	{
		AsmoLogger.Trace("UINavigationManager", "UnRegisterFocusableLayer " + focusableLayer.gameObject.name);
		_focusableLayers.Remove(focusableLayer);
	}

	public void MoveFocus(Direction direction)
	{
		if (_focusables.Count == 0)
		{
			return;
		}
		Focusable focusable = null;
		if (_currentFocusable == null)
		{
			foreach (Focusable focusable2 in _focusables)
			{
				if (focusable2.firstFocusable)
				{
					focusable = focusable2;
					break;
				}
			}
			if (focusable == null)
			{
				focusable = Enumerable.First(_focusables);
			}
		}
		else
		{
			switch (direction)
			{
			case Direction.Next:
				focusable = _FindNextFocusableOf(_currentFocusable);
				break;
			case Direction.Previous:
				focusable = _FindPreviousFocusableOf(_currentFocusable);
				break;
			}
			if (focusable == null && !_isEditingInputField)
			{
				focusable = _FindClosestFocusable(_currentFocusable, direction);
				if (focusable == null)
				{
					focusable = _currentFocusable;
				}
			}
		}
		if (focusable != null)
		{
			if (focusable != _currentFocusable)
			{
				_UpdateFocus(focusable);
			}
			else
			{
				highlightFocusable(focusable);
			}
		}
	}

	private Focusable _FindNextFocusableOf(Focusable currentFocusable)
	{
		if (currentFocusable == null)
		{
			return null;
		}
		Focusable next = currentFocusable.next;
		while (next != null && next != currentFocusable)
		{
			if (next.isActiveAndEnabled && next.Selectable.IsInteractable())
			{
				return next;
			}
			next = next.next;
		}
		return next;
	}

	private Focusable _FindPreviousFocusableOf(Focusable currentFocusable)
	{
		if (currentFocusable == null)
		{
			return null;
		}
		Focusable previous = currentFocusable.previous;
		while (previous != null && previous != currentFocusable)
		{
			if (previous.isActiveAndEnabled && previous.Selectable.IsInteractable())
			{
				return previous;
			}
			previous = previous.previous;
		}
		return previous;
	}

	public void FindFocusableInputFieldAndEnterEditMode()
	{
		if (_IsInputField(_currentFocusable))
		{
			if (!_isEditingInputField)
			{
				_ActivateInputField();
			}
			return;
		}
		foreach (Focusable focusable in _focusables)
		{
			if (_IsInputField(focusable))
			{
				_isEditingInputField = true;
				_UpdateFocus(focusable);
				break;
			}
		}
	}

	public void HandleInputString(string inputString)
	{
		if (_IsInputField(_currentFocusable) && !_isEditingInputField)
		{
			_ActivateInputField();
		}
	}

	public void CancelCurrentContext()
	{
		if (HasFocus)
		{
			if (_isEditingInputField)
			{
				_DeactivateInputField();
			}
			else
			{
				LoseFocus();
			}
			return;
		}
		for (int num = _focusableLayers.Count - 1; num >= 0; num--)
		{
			FocusableLayer focusableLayer = _focusableLayers[num];
			if (focusableLayer.OnBackAction != null)
			{
				focusableLayer.OnBackAction.Invoke();
				break;
			}
			if (focusableLayer.modal)
			{
				break;
			}
		}
		this.OnBackAction?.Invoke();
	}

	public void ValidateFocusable()
	{
		if (HasFocus)
		{
			if (_IsInputField(_currentFocusable))
			{
				if (!_isEditingInputField)
				{
					_ActivateInputField();
				}
				else
				{
					_DeactivateInputField();
				}
			}
			else
			{
				ExecuteEvents.Execute(_currentFocusable.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
			}
		}
		else
		{
			FindFocusableInputFieldAndEnterEditMode();
		}
	}

	public void LoseFocus()
	{
		AsmoLogger.Trace("UINavigationManager", "LoseFocus");
		_isEditingInputField = false;
		_UpdateFocus(null);
		EventSystem.current.SetSelectedGameObject(null);
	}

	private void Update()
	{
		EventSystem current = EventSystem.current;
		if (current == null)
		{
			return;
		}
		current.sendNavigationEvents = false;
		_focusables.Clear();
		for (int num = _focusableLayers.Count - 1; num >= 0; num--)
		{
			FocusableLayer focusableLayer = _focusableLayers[num];
			_focusables.AddRange(focusableLayer.Focusables);
			if (focusableLayer.modal)
			{
				break;
			}
		}
		if (HasFocus && !_focusables.Contains(_currentFocusable))
		{
			LoseFocus();
			MoveFocus(Direction.Next);
		}
		Focusable focusable = ((current.currentSelectedGameObject != null) ? current.currentSelectedGameObject.GetComponent<Focusable>() : null);
		if (focusable != _currentFocusable)
		{
			if (focusable == null)
			{
				_UpdateFocus(_currentFocusable);
			}
			else
			{
				if (_IsInputField(focusable))
				{
					_isEditingInputField = true;
				}
				_UpdateFocus(focusable);
			}
		}
		navigationInput?.ProcessInput(this);
		if (!HasFocus && forceToGiveFocus())
		{
			MoveFocus(Direction.Next);
		}
		current.SetSelectedGameObject((_currentFocusable != null) ? _currentFocusable.gameObject : null);
		if (_IsInputField(_currentFocusable))
		{
			if (!_isEditingInputField)
			{
				_DeactivateInputField();
			}
		}
		else
		{
			_isEditingInputField = false;
		}
	}

	private bool _IsInputField(Focusable focusable)
	{
		if (focusable == null)
		{
			return false;
		}
		return focusable.IsInputField;
	}

	private void _ActivateInputField()
	{
		_isEditingInputField = true;
		if (_currentFocusable.InputField != null)
		{
			if (!_currentFocusable.InputField.isFocused)
			{
				_currentFocusable.InputField.ActivateInputField();
			}
		}
		else if (_currentFocusable.TMP_InputField != null && !_currentFocusable.TMP_InputField.isFocused)
		{
			_currentFocusable.TMP_InputField.ActivateInputField();
		}
	}

	private void _DeactivateInputField()
	{
		_isEditingInputField = false;
		if (_currentFocusable.InputField != null)
		{
			if (_currentFocusable.InputField.isFocused)
			{
				_currentFocusable.InputField.DeactivateInputField();
			}
		}
		else if (_currentFocusable.TMP_InputField != null && _currentFocusable.TMP_InputField.isFocused)
		{
			_currentFocusable.TMP_InputField.DeactivateInputField();
		}
	}

	private void _UpdateFocus(Focusable focusable)
	{
		_currentFocusable = focusable;
	}

	private Focusable _FindClosestFocusable(Focusable origin, Direction direction)
	{
		List<FocusablePosition> positions = _ComputeFocusablePositions(origin);
		List<FocusablePosition> list = _TrimAndSortPositions(positions, direction);
		if (list.Count <= 0)
		{
			return null;
		}
		return Enumerable.First(list).focusable;
	}

	private List<FocusablePosition> _TrimAndSortPositions(List<FocusablePosition> positions, Direction direction)
	{
		List<FocusablePosition> result = null;
		switch (direction)
		{
		case Direction.Left:
			result = (from p in positions
				where p.deltaPosition.x < -0.001f
				orderby p.squaredDistance * Mathf.Max(1f, Mathf.Abs(directionalWeight * p.deltaPosition.y / p.deltaPosition.x))
				select p).ToList();
			break;
		case Direction.Up:
		case Direction.Previous:
			result = (from p in positions
				where p.deltaPosition.y > 0.001f
				orderby p.squaredDistance * Mathf.Max(1f, Mathf.Abs(directionalWeight * p.deltaPosition.x / p.deltaPosition.y))
				select p).ToList();
			break;
		case Direction.Right:
			result = (from p in positions
				where p.deltaPosition.x > 0.001f
				orderby p.squaredDistance * Mathf.Max(1f, Mathf.Abs(directionalWeight * p.deltaPosition.y / p.deltaPosition.x))
				select p).ToList();
			break;
		case Direction.Down:
		case Direction.Next:
			result = (from p in positions
				where p.deltaPosition.y < -0.001f
				orderby p.squaredDistance * Mathf.Max(1f, Mathf.Abs(directionalWeight * p.deltaPosition.x / p.deltaPosition.y))
				select p).ToList();
			break;
		}
		return result;
	}

	private List<FocusablePosition> _ComputeFocusablePositions(Focusable origin)
	{
		List<FocusablePosition> list = new List<FocusablePosition>();
		Vector2 viewportPosition = origin.ViewportPosition;
		foreach (Focusable focusable in _focusables)
		{
			if (focusable.isActiveAndEnabled && focusable.Selectable.IsInteractable() && focusable != origin)
			{
				Vector2 deltaPosition = focusable.ViewportPosition - viewportPosition;
				list.Add(new FocusablePosition(focusable, deltaPosition, deltaPosition.sqrMagnitude));
			}
		}
		return list;
	}

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying || _currentFocusable == null || !_currentFocusable.isActiveAndEnabled || !_currentFocusable.Selectable.IsInteractable())
		{
			return;
		}
		Focusable focusable = _FindClosestFocusable(_currentFocusable, Direction.Left);
		Focusable focusable2 = _FindClosestFocusable(_currentFocusable, Direction.Right);
		Focusable focusable3 = _FindClosestFocusable(_currentFocusable, Direction.Up);
		Focusable focusable4 = _FindClosestFocusable(_currentFocusable, Direction.Down);
		Focusable focusable5 = _FindNextFocusableOf(_currentFocusable);
		Focusable focusable6 = _FindPreviousFocusableOf(_currentFocusable);
		if (gizmosLevel != GizmosLevel.Minimalist)
		{
			List<FocusablePosition> positions = _ComputeFocusablePositions(_currentFocusable);
			Direction[] array = ((gizmosLevel != GizmosLevel.HorizontalGradient) ? new Direction[2]
			{
				Direction.Up,
				Direction.Down
			} : new Direction[2]
			{
				Direction.Left,
				Direction.Right
			});
			foreach (Direction direction in array)
			{
				List<FocusablePosition> list = _TrimAndSortPositions(positions, direction);
				float num = 0f;
				float num2 = 0.67f / (float)list.Count;
				foreach (FocusablePosition item in list)
				{
					Gizmos.color = Color.HSVToRGB(num, 1f, 1f);
					Gizmos.DrawSphere(item.focusable.transform.position, 5f);
					num += num2;
				}
			}
			Gizmos.color = Color.white;
			Gizmos.DrawSphere(_currentFocusable.transform.position, 5f);
		}
		Gizmos.color = Color.red;
		uint width = ((gizmosLevel != GizmosLevel.Minimalist) ? 3u : 2u);
		if (focusable5 != null)
		{
			_GizmosDrawLine(_currentFocusable.transform.position, focusable5.transform.position, width);
		}
		if (focusable6 != null)
		{
			_GizmosDrawLine(_currentFocusable.transform.position, focusable6.transform.position, width);
		}
		Gizmos.color = Color.blue;
		width = ((gizmosLevel == GizmosLevel.Minimalist) ? 1u : 2u);
		if (focusable != null)
		{
			_GizmosDrawLine(_currentFocusable.transform.position, focusable.transform.position, width);
		}
		if (focusable2 != null)
		{
			_GizmosDrawLine(_currentFocusable.transform.position, focusable2.transform.position, width);
		}
		if (focusable3 != null)
		{
			_GizmosDrawLine(_currentFocusable.transform.position, focusable3.transform.position, width);
		}
		if (focusable4 != null)
		{
			_GizmosDrawLine(_currentFocusable.transform.position, focusable4.transform.position, width);
		}
	}

	private static void _GizmosDrawLine(Vector3 from, Vector3 to, uint width)
	{
		if (width == 1)
		{
			Gizmos.DrawLine(from, to);
			return;
		}
		Camera current = Camera.current;
		if (!(current == null))
		{
			Vector3 normalized = (to - from).normalized;
			Vector3 normalized2 = (current.transform.position - from).normalized;
			Vector3 vector = Vector3.Cross(normalized, normalized2);
			float num = (1f - (float)width) * 0.5f;
			for (int i = 0; i < width; i++)
			{
				Vector3 vector2 = vector * num;
				Gizmos.DrawLine(from + vector2, to + vector2);
				num += 0.5f;
			}
		}
	}

	public void BeginIgnoringInteractionEvents(string requester)
	{
		_ignoringInteractionEventsRequestCount++;
		AsmoLogger.Trace("UINavigationManager", "Request to IGNORE interaction events", new Hashtable
		{
			{ "requester", requester },
			{ "request count", _ignoringInteractionEventsRequestCount }
		});
		_AllowOrIgnoreInteractionEvents();
	}

	public void EndIgnoringInteractionEvents(string requester)
	{
		_ignoringInteractionEventsRequestCount--;
		AsmoLogger.Trace("UINavigationManager", "Request to ALLOW interaction events", new Hashtable
		{
			{ "requester", requester },
			{ "request count", _ignoringInteractionEventsRequestCount }
		});
		_AllowOrIgnoreInteractionEvents();
	}

	private void _AllowOrIgnoreInteractionEvents()
	{
		bool flag = _ignoringInteractionEventsRequestCount == 0;
		EventSystem[] array = UnityEngine.Object.FindObjectsOfType(typeof(EventSystem)) as EventSystem[];
		string message = (flag ? $"Allow Interaction Events ({array.Length} eventSystems)" : $"Ignore Interaction Events, request count:{_ignoringInteractionEventsRequestCount} ({array.Length} eventSystems)");
		AsmoLogger.Debug("UINavigationManager", message);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = flag;
		}
	}

	public string PrettyPrint()
	{
		string text = $"UINavigationManager  {_focusableLayers.Count} layers  _currentFocusable:{_currentFocusable?.name}";
		foreach (FocusableLayer focusableLayer in _focusableLayers)
		{
			text = text + "\n- " + focusableLayer.name;
			if (focusableLayer == _rootFocusableLayer)
			{
				text += "  (root)";
			}
			if (focusableLayer.modal)
			{
				text += "  modal";
			}
			if (focusableLayer.OnBackAction != null)
			{
				text += "  OnBackAction";
			}
			foreach (Focusable focusable in focusableLayer.Focusables)
			{
				text += "\n    '-- ";
				if (_focusables.Contains(focusable))
				{
					text += "# ";
				}
				text += focusable.name;
				if (focusable.firstFocusable)
				{
					text += "  firstFocusable";
				}
				if (focusable == _currentFocusable)
				{
					text += "  <==";
				}
			}
		}
		return text;
	}
}
