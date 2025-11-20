using System;
using System.Collections.Generic;
using System.Globalization;

namespace UnityEngine.UIElements;

public abstract class BaseSlider<TValueType> : BaseField<TValueType> where TValueType : IComparable<TValueType>
{
	internal enum SliderKey
	{
		None,
		Lowest,
		LowerPage,
		Lower,
		Higher,
		HigherPage,
		Highest
	}

	[SerializeField]
	private TValueType m_LowValue;

	[SerializeField]
	private TValueType m_HighValue;

	private float m_PageSize;

	private bool m_ShowInputField = false;

	private Rect m_DragElementStartPos;

	private SliderDirection m_Direction;

	private bool m_Inverted = false;

	internal const float kDefaultPageSize = 0f;

	internal const bool kDefaultShowInputField = false;

	internal const bool kDefaultInverted = false;

	public new static readonly string ussClassName = "unity-base-slider";

	public new static readonly string labelUssClassName = ussClassName + "__label";

	public new static readonly string inputUssClassName = ussClassName + "__input";

	public static readonly string horizontalVariantUssClassName = ussClassName + "--horizontal";

	public static readonly string verticalVariantUssClassName = ussClassName + "--vertical";

	public static readonly string dragContainerUssClassName = ussClassName + "__drag-container";

	public static readonly string trackerUssClassName = ussClassName + "__tracker";

	public static readonly string draggerUssClassName = ussClassName + "__dragger";

	public static readonly string draggerBorderUssClassName = ussClassName + "__dragger-border";

	public static readonly string textFieldClassName = ussClassName + "__text-field";

	internal VisualElement dragContainer { get; private set; }

	internal VisualElement dragElement { get; private set; }

	internal VisualElement dragBorderElement { get; private set; }

	internal TextField inputTextField { get; private set; }

	public TValueType lowValue
	{
		get
		{
			return m_LowValue;
		}
		set
		{
			if (!EqualityComparer<TValueType>.Default.Equals(m_LowValue, value))
			{
				m_LowValue = value;
				ClampValue();
				UpdateDragElementPosition();
				SaveViewData();
			}
		}
	}

	public TValueType highValue
	{
		get
		{
			return m_HighValue;
		}
		set
		{
			if (!EqualityComparer<TValueType>.Default.Equals(m_HighValue, value))
			{
				m_HighValue = value;
				ClampValue();
				UpdateDragElementPosition();
				SaveViewData();
			}
		}
	}

	public TValueType range => SliderRange();

	public virtual float pageSize
	{
		get
		{
			return m_PageSize;
		}
		set
		{
			m_PageSize = value;
		}
	}

	public virtual bool showInputField
	{
		get
		{
			return m_ShowInputField;
		}
		set
		{
			if (m_ShowInputField != value)
			{
				m_ShowInputField = value;
				UpdateTextFieldVisibility();
			}
		}
	}

	internal bool clamped { get; set; } = true;

	internal ClampedDragger<TValueType> clampedDragger { get; private set; }

	public override TValueType value
	{
		get
		{
			return base.value;
		}
		set
		{
			TValueType val = (clamped ? GetClampedValue(value) : value);
			base.value = val;
		}
	}

	public SliderDirection direction
	{
		get
		{
			return m_Direction;
		}
		set
		{
			m_Direction = value;
			if (m_Direction == SliderDirection.Horizontal)
			{
				RemoveFromClassList(verticalVariantUssClassName);
				AddToClassList(horizontalVariantUssClassName);
			}
			else
			{
				RemoveFromClassList(horizontalVariantUssClassName);
				AddToClassList(verticalVariantUssClassName);
			}
		}
	}

	public bool inverted
	{
		get
		{
			return m_Inverted;
		}
		set
		{
			if (m_Inverted != value)
			{
				m_Inverted = value;
				UpdateDragElementPosition();
			}
		}
	}

	internal void SetHighValueWithoutNotify(TValueType newHighValue)
	{
		m_HighValue = newHighValue;
		TValueType valueWithoutNotify = (clamped ? GetClampedValue(value) : value);
		SetValueWithoutNotify(valueWithoutNotify);
		UpdateDragElementPosition();
		SaveViewData();
	}

	private TValueType Clamp(TValueType value, TValueType lowBound, TValueType highBound)
	{
		TValueType result = value;
		if (lowBound.CompareTo(value) > 0)
		{
			result = lowBound;
		}
		else if (highBound.CompareTo(value) < 0)
		{
			result = highBound;
		}
		return result;
	}

	private TValueType GetClampedValue(TValueType newValue)
	{
		TValueType val = lowValue;
		TValueType val2 = highValue;
		if (val.CompareTo(val2) > 0)
		{
			TValueType val3 = val;
			val = val2;
			val2 = val3;
		}
		return Clamp(newValue, val, val2);
	}

	public override void SetValueWithoutNotify(TValueType newValue)
	{
		TValueType valueWithoutNotify = (clamped ? GetClampedValue(newValue) : newValue);
		base.SetValueWithoutNotify(valueWithoutNotify);
		UpdateDragElementPosition();
		UpdateTextFieldValue();
	}

	internal BaseSlider(string label, TValueType start, TValueType end, SliderDirection direction = SliderDirection.Horizontal, float pageSize = 0f)
		: base(label, (VisualElement)null)
	{
		AddToClassList(ussClassName);
		base.labelElement.AddToClassList(labelUssClassName);
		base.visualInput.AddToClassList(inputUssClassName);
		this.direction = direction;
		this.pageSize = pageSize;
		lowValue = start;
		highValue = end;
		base.pickingMode = PickingMode.Ignore;
		dragContainer = new VisualElement
		{
			name = "unity-drag-container"
		};
		dragContainer.AddToClassList(dragContainerUssClassName);
		base.visualInput.Add(dragContainer);
		VisualElement visualElement = new VisualElement
		{
			name = "unity-tracker"
		};
		visualElement.AddToClassList(trackerUssClassName);
		dragContainer.Add(visualElement);
		dragBorderElement = new VisualElement
		{
			name = "unity-dragger-border"
		};
		dragBorderElement.AddToClassList(draggerBorderUssClassName);
		dragContainer.Add(dragBorderElement);
		dragElement = new VisualElement
		{
			name = "unity-dragger"
		};
		dragElement.RegisterCallback<GeometryChangedEvent>(UpdateDragElementPosition);
		dragElement.AddToClassList(draggerUssClassName);
		dragContainer.Add(dragElement);
		clampedDragger = new ClampedDragger<TValueType>(this, SetSliderValueFromClick, SetSliderValueFromDrag);
		dragContainer.pickingMode = PickingMode.Position;
		dragContainer.AddManipulator(clampedDragger);
		RegisterCallback<KeyDownEvent>(OnKeyDown);
		UpdateTextFieldVisibility();
	}

	protected static float GetClosestPowerOfTen(float positiveNumber)
	{
		if (positiveNumber <= 0f)
		{
			return 1f;
		}
		return Mathf.Pow(10f, Mathf.RoundToInt(Mathf.Log10(positiveNumber)));
	}

	protected static float RoundToMultipleOf(float value, float roundingValue)
	{
		if (roundingValue == 0f)
		{
			return value;
		}
		return Mathf.Round(value / roundingValue) * roundingValue;
	}

	private void ClampValue()
	{
		value = base.rawValue;
	}

	internal abstract TValueType SliderLerpUnclamped(TValueType a, TValueType b, float interpolant);

	internal abstract float SliderNormalizeValue(TValueType currentValue, TValueType lowerValue, TValueType higherValue);

	internal abstract TValueType SliderRange();

	internal abstract TValueType ParseStringToValue(string stringValue);

	internal abstract void ComputeValueFromKey(SliderKey sliderKey, bool isShift);

	private TValueType SliderLerpDirectionalUnclamped(TValueType a, TValueType b, float positionInterpolant)
	{
		float interpolant = ((direction == SliderDirection.Vertical) ? (1f - positionInterpolant) : positionInterpolant);
		if (inverted)
		{
			return SliderLerpUnclamped(b, a, interpolant);
		}
		return SliderLerpUnclamped(a, b, interpolant);
	}

	private void SetSliderValueFromDrag()
	{
		if (clampedDragger.dragDirection == ClampedDragger<TValueType>.DragDirection.Free)
		{
			Vector2 delta = clampedDragger.delta;
			if (direction == SliderDirection.Horizontal)
			{
				ComputeValueAndDirectionFromDrag(dragContainer.resolvedStyle.width, dragElement.resolvedStyle.width, m_DragElementStartPos.x + delta.x);
			}
			else
			{
				ComputeValueAndDirectionFromDrag(dragContainer.resolvedStyle.height, dragElement.resolvedStyle.height, m_DragElementStartPos.y + delta.y);
			}
		}
	}

	private void ComputeValueAndDirectionFromDrag(float sliderLength, float dragElementLength, float dragElementPos)
	{
		float num = sliderLength - dragElementLength;
		if (!(Mathf.Abs(num) < 1E-30f))
		{
			float positionInterpolant = Mathf.Max(0f, Mathf.Min(dragElementPos, num)) / num;
			value = SliderLerpDirectionalUnclamped(lowValue, highValue, positionInterpolant);
		}
	}

	private void SetSliderValueFromClick()
	{
		if (clampedDragger.dragDirection == ClampedDragger<TValueType>.DragDirection.Free)
		{
			return;
		}
		if (clampedDragger.dragDirection == ClampedDragger<TValueType>.DragDirection.None)
		{
			if (Mathf.Approximately(pageSize, 0f))
			{
				float x = ((direction == SliderDirection.Horizontal) ? (clampedDragger.startMousePosition.x - dragElement.resolvedStyle.width / 2f) : dragElement.transform.position.x);
				float y = ((direction == SliderDirection.Horizontal) ? dragElement.transform.position.y : (clampedDragger.startMousePosition.y - dragElement.resolvedStyle.height / 2f));
				Vector3 position = new Vector3(x, y, 0f);
				dragElement.transform.position = position;
				dragBorderElement.transform.position = position;
				m_DragElementStartPos = new Rect(x, y, dragElement.resolvedStyle.width, dragElement.resolvedStyle.height);
				clampedDragger.dragDirection = ClampedDragger<TValueType>.DragDirection.Free;
				if (direction == SliderDirection.Horizontal)
				{
					ComputeValueAndDirectionFromDrag(dragContainer.resolvedStyle.width, dragElement.resolvedStyle.width, m_DragElementStartPos.x);
				}
				else
				{
					ComputeValueAndDirectionFromDrag(dragContainer.resolvedStyle.height, dragElement.resolvedStyle.height, m_DragElementStartPos.y);
				}
				return;
			}
			m_DragElementStartPos = new Rect(dragElement.transform.position.x, dragElement.transform.position.y, dragElement.resolvedStyle.width, dragElement.resolvedStyle.height);
		}
		if (direction == SliderDirection.Horizontal)
		{
			ComputeValueAndDirectionFromClick(dragContainer.resolvedStyle.width, dragElement.resolvedStyle.width, dragElement.transform.position.x, clampedDragger.lastMousePosition.x);
		}
		else
		{
			ComputeValueAndDirectionFromClick(dragContainer.resolvedStyle.height, dragElement.resolvedStyle.height, dragElement.transform.position.y, clampedDragger.lastMousePosition.y);
		}
	}

	private void OnKeyDown(KeyDownEvent evt)
	{
		SliderKey sliderKey = SliderKey.None;
		bool flag = direction == SliderDirection.Horizontal;
		if ((flag && evt.keyCode == KeyCode.Home) || (!flag && evt.keyCode == KeyCode.End))
		{
			sliderKey = ((!inverted) ? SliderKey.Lowest : SliderKey.Highest);
		}
		else if ((flag && evt.keyCode == KeyCode.End) || (!flag && evt.keyCode == KeyCode.Home))
		{
			sliderKey = (inverted ? SliderKey.Lowest : SliderKey.Highest);
		}
		else if ((flag && evt.keyCode == KeyCode.PageUp) || (!flag && evt.keyCode == KeyCode.PageDown))
		{
			sliderKey = (inverted ? SliderKey.HigherPage : SliderKey.LowerPage);
		}
		else if ((flag && evt.keyCode == KeyCode.PageDown) || (!flag && evt.keyCode == KeyCode.PageUp))
		{
			sliderKey = (inverted ? SliderKey.LowerPage : SliderKey.HigherPage);
		}
		else if ((flag && evt.keyCode == KeyCode.LeftArrow) || (!flag && evt.keyCode == KeyCode.DownArrow))
		{
			sliderKey = (inverted ? SliderKey.Higher : SliderKey.Lower);
		}
		else if ((flag && evt.keyCode == KeyCode.RightArrow) || (!flag && evt.keyCode == KeyCode.UpArrow))
		{
			sliderKey = (inverted ? SliderKey.Lower : SliderKey.Higher);
		}
		if (sliderKey != SliderKey.None)
		{
			ComputeValueFromKey(sliderKey, evt.shiftKey);
			evt.StopPropagation();
		}
	}

	internal virtual void ComputeValueAndDirectionFromClick(float sliderLength, float dragElementLength, float dragElementPos, float dragElementLastPos)
	{
		float num = sliderLength - dragElementLength;
		if (!(Mathf.Abs(num) < 1E-30f))
		{
			bool flag = dragElementLastPos < dragElementPos;
			bool flag2 = dragElementLastPos > dragElementPos + dragElementLength;
			bool flag3 = (inverted ? flag2 : flag);
			bool flag4 = (inverted ? flag : flag2);
			float num2 = (inverted ? (0f - pageSize) : pageSize);
			if (flag3 && clampedDragger.dragDirection != ClampedDragger<TValueType>.DragDirection.LowToHigh)
			{
				clampedDragger.dragDirection = ClampedDragger<TValueType>.DragDirection.HighToLow;
				float positionInterpolant = Mathf.Max(0f, Mathf.Min(dragElementPos - num2, num)) / num;
				value = SliderLerpDirectionalUnclamped(lowValue, highValue, positionInterpolant);
			}
			else if (flag4 && clampedDragger.dragDirection != ClampedDragger<TValueType>.DragDirection.HighToLow)
			{
				clampedDragger.dragDirection = ClampedDragger<TValueType>.DragDirection.LowToHigh;
				float positionInterpolant2 = Mathf.Max(0f, Mathf.Min(dragElementPos + num2, num)) / num;
				value = SliderLerpDirectionalUnclamped(lowValue, highValue, positionInterpolant2);
			}
		}
	}

	public void AdjustDragElement(float factor)
	{
		bool flag = factor < 1f;
		dragElement.visible = flag;
		if (flag)
		{
			IStyle style = dragElement.style;
			dragElement.style.visibility = StyleKeyword.Null;
			if (direction == SliderDirection.Horizontal)
			{
				float b = ((base.resolvedStyle.minWidth == StyleKeyword.Auto) ? 0f : base.resolvedStyle.minWidth.value);
				style.width = Mathf.Round(Mathf.Max(dragContainer.layout.width * factor, b));
			}
			else
			{
				float b2 = ((base.resolvedStyle.minHeight == StyleKeyword.Auto) ? 0f : base.resolvedStyle.minHeight.value);
				style.height = Mathf.Round(Mathf.Max(dragContainer.layout.height * factor, b2));
			}
		}
		dragBorderElement.visible = dragElement.visible;
	}

	private void UpdateDragElementPosition(GeometryChangedEvent evt)
	{
		if (!(evt.oldRect.size == evt.newRect.size))
		{
			UpdateDragElementPosition();
		}
	}

	internal override void OnViewDataReady()
	{
		base.OnViewDataReady();
		UpdateDragElementPosition();
	}

	private bool SameValues(float a, float b, float epsilon)
	{
		return Mathf.Abs(b - a) < epsilon;
	}

	private void UpdateDragElementPosition()
	{
		if (base.panel == null)
		{
			return;
		}
		float num = SliderNormalizeValue(value, lowValue, highValue);
		float num2 = (inverted ? (1f - num) : num);
		float epsilon = base.scaledPixelsPerPoint * 0.5f;
		if (direction == SliderDirection.Horizontal)
		{
			float width = dragElement.resolvedStyle.width;
			float num3 = 0f - dragElement.resolvedStyle.marginLeft - dragElement.resolvedStyle.marginRight;
			float num4 = dragContainer.layout.width - width + num3;
			float num5 = num2 * num4;
			if (!float.IsNaN(num5))
			{
				float x = dragElement.transform.position.x;
				if (!SameValues(x, num5, epsilon))
				{
					Vector3 position = new Vector3(num5, 0f, 0f);
					dragElement.transform.position = position;
					dragBorderElement.transform.position = position;
				}
			}
			return;
		}
		float height = dragElement.resolvedStyle.height;
		float num6 = dragContainer.resolvedStyle.height - height;
		float num7 = (1f - num2) * num6;
		if (!float.IsNaN(num7))
		{
			float y = dragElement.transform.position.y;
			if (!SameValues(y, num7, epsilon))
			{
				Vector3 position2 = new Vector3(0f, num7, 0f);
				dragElement.transform.position = position2;
				dragBorderElement.transform.position = position2;
			}
		}
	}

	protected override void ExecuteDefaultAction(EventBase evt)
	{
		base.ExecuteDefaultAction(evt);
		if (evt != null && evt.eventTypeId == EventBase<GeometryChangedEvent>.TypeId())
		{
			UpdateDragElementPosition((GeometryChangedEvent)evt);
		}
	}

	private void UpdateTextFieldVisibility()
	{
		if (showInputField)
		{
			if (inputTextField == null)
			{
				inputTextField = new TextField
				{
					name = "unity-text-field"
				};
				inputTextField.AddToClassList(textFieldClassName);
				inputTextField.RegisterValueChangedCallback(OnTextFieldValueChange);
				inputTextField.RegisterCallback<FocusOutEvent>(OnTextFieldFocusOut);
				base.visualInput.Add(inputTextField);
				UpdateTextFieldValue();
			}
		}
		else if (inputTextField != null && inputTextField.panel != null)
		{
			if (inputTextField.panel != null)
			{
				inputTextField.RemoveFromHierarchy();
			}
			inputTextField.UnregisterValueChangedCallback(OnTextFieldValueChange);
			inputTextField.UnregisterCallback<FocusOutEvent>(OnTextFieldFocusOut);
			inputTextField = null;
		}
	}

	private void UpdateTextFieldValue()
	{
		if (inputTextField != null)
		{
			inputTextField.SetValueWithoutNotify(string.Format(CultureInfo.InvariantCulture, "{0:g7}", new object[1] { value }));
		}
	}

	private void OnTextFieldFocusOut(FocusOutEvent evt)
	{
		UpdateTextFieldValue();
	}

	private void OnTextFieldValueChange(ChangeEvent<string> evt)
	{
		TValueType clampedValue = GetClampedValue(ParseStringToValue(evt.newValue));
		if (!EqualityComparer<TValueType>.Default.Equals(clampedValue, value))
		{
			value = clampedValue;
			evt.StopPropagation();
			if (base.elementPanel != null)
			{
				OnViewDataReady();
			}
		}
	}

	protected override void UpdateMixedValueContent()
	{
		if (base.showMixedValue)
		{
			dragElement?.RemoveFromHierarchy();
		}
		else
		{
			base.visualInput.Add(dragElement);
		}
	}
}
