using AsmodeeNet.Foundation;
using AsmodeeNet.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AsmodeeNet.UserInterface;

[RequireComponent(typeof(Selectable))]
public class Focusable : MonoBehaviour
{
	private const string _documentation = "<b>Focusable</b> registers a <b>UI.Selectable</b> control with the <b>FocusManager</b>. It will be part of the UI cross-platform navigation system.";

	private const string _kModuleName = "Focusable";

	[Tooltip("Flag this Focusable as the first element to take focus")]
	public bool firstFocusable;

	public Focusable next;

	[HideInInspector]
	public Focusable previous;

	private Canvas _canvas;

	public Selectable Selectable { get; private set; }

	public InputField InputField { get; private set; }

	public TMP_InputField TMP_InputField { get; private set; }

	public bool IsInputField
	{
		get
		{
			if (!(InputField != null))
			{
				return TMP_InputField != null;
			}
			return true;
		}
	}

	public FocusableLayer FocusableLayer
	{
		get
		{
			Transform parent = base.gameObject.transform;
			while (parent.parent != null)
			{
				FocusableLayer component = parent.parent.GetComponent<FocusableLayer>();
				if (component != null)
				{
					return component;
				}
				parent = parent.parent;
			}
			return CoreApplication.Instance.UINavigationManager.RootFocusableLayer;
		}
	}

	public Vector2 ViewportPosition
	{
		get
		{
			if (_canvas == null)
			{
				return Camera.main.WorldToViewportPoint(base.transform.position);
			}
			if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
			{
				RectTransform obj = _canvas.transform as RectTransform;
				Vector2 size = obj.rect.size;
				Vector2 vector = RectTransformUtility.CalculateRelativeRectTransformBounds(obj, base.transform as RectTransform).center;
				return new Vector2(vector.x / size.x + 0.5f, vector.y / size.y + 0.5f);
			}
			return (_canvas?.worldCamera ?? Camera.main).WorldToViewportPoint(base.transform.position);
		}
	}

	private void Start()
	{
		Selectable = GetComponent<Selectable>();
		if (Selectable == null)
		{
			AsmoLogger.Error("Focusable", "Missing Selectable component");
		}
		_canvas = GetComponentInParent<Canvas>();
		if (next != null)
		{
			next.previous = this;
		}
		InputField = GetComponent<InputField>();
		TMP_InputField = GetComponent<TMP_InputField>();
	}

	private void OnEnable()
	{
		FocusableLayer.RegisterFocusable(this);
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			FocusableLayer.UnRegisterFocusable(this);
		}
	}
}
