using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

public abstract class BaseField<TValueType> : BindableElement, INotifyValueChanged<TValueType>, IMixedValueSupport
{
	public new class UxmlTraits : BindableElement.UxmlTraits
	{
		private UxmlStringAttributeDescription m_Label = new UxmlStringAttributeDescription
		{
			name = "label"
		};

		public UxmlTraits()
		{
			base.focusIndex.defaultValue = 0;
			base.focusable.defaultValue = true;
		}

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			((BaseField<TValueType>)ve).label = m_Label.GetValueFromBag(bag, cc);
		}

		internal static List<string> ParseChoiceList(string choicesFromBag)
		{
			if (string.IsNullOrEmpty(choicesFromBag.Trim()))
			{
				return null;
			}
			string[] array = choicesFromBag.Split(new char[1] { ',' });
			if (array.Length != 0)
			{
				List<string> list = new List<string>();
				string[] array2 = array;
				foreach (string text in array2)
				{
					list.Add(text.Trim());
				}
				return list;
			}
			return null;
		}
	}

	public static readonly string ussClassName = "unity-base-field";

	public static readonly string labelUssClassName = ussClassName + "__label";

	public static readonly string inputUssClassName = ussClassName + "__input";

	public static readonly string noLabelVariantUssClassName = ussClassName + "--no-label";

	public static readonly string labelDraggerVariantUssClassName = labelUssClassName + "--with-dragger";

	public static readonly string mixedValueLabelUssClassName = labelUssClassName + "--mixed-value";

	public static readonly string alignedFieldUssClassName = ussClassName + "__aligned";

	private static readonly string inspectorFieldUssClassName = ussClassName + "__inspector-field";

	private const int kIndentPerLevel = 15;

	protected static readonly string mixedValueString = "—";

	protected internal static readonly PropertyName serializedPropertyCopyName = "SerializedPropertyCopyName";

	private static CustomStyleProperty<float> s_LabelWidthRatioProperty = new CustomStyleProperty<float>("--unity-property-field-label-width-ratio");

	private static CustomStyleProperty<float> s_LabelExtraPaddingProperty = new CustomStyleProperty<float>("--unity-property-field-label-extra-padding");

	private static CustomStyleProperty<float> s_LabelBaseMinWidthProperty = new CustomStyleProperty<float>("--unity-property-field-label-base-min-width");

	private float m_LabelWidthRatio;

	private float m_LabelExtraPadding;

	private float m_LabelBaseMinWidth;

	private VisualElement m_VisualInput;

	[SerializeField]
	private TValueType m_Value;

	private bool m_ShowMixedValue;

	private Label m_MixedValueLabel;

	private VisualElement m_CachedInspectorElement;

	private int m_CachedListAndFoldoutDepth;

	internal VisualElement visualInput
	{
		get
		{
			return m_VisualInput;
		}
		set
		{
			if (m_VisualInput != null)
			{
				if (m_VisualInput.parent == this)
				{
					m_VisualInput.RemoveFromHierarchy();
				}
				m_VisualInput = null;
			}
			if (value != null)
			{
				m_VisualInput = value;
			}
			else
			{
				m_VisualInput = new VisualElement
				{
					pickingMode = PickingMode.Ignore
				};
			}
			m_VisualInput.focusable = true;
			m_VisualInput.AddToClassList(inputUssClassName);
			Add(m_VisualInput);
		}
	}

	protected TValueType rawValue
	{
		get
		{
			return m_Value;
		}
		set
		{
			m_Value = value;
		}
	}

	public virtual TValueType value
	{
		get
		{
			return m_Value;
		}
		set
		{
			if (EqualityComparer<TValueType>.Default.Equals(m_Value, value))
			{
				return;
			}
			if (base.panel != null)
			{
				using (ChangeEvent<TValueType> changeEvent = ChangeEvent<TValueType>.GetPooled(m_Value, value))
				{
					changeEvent.target = this;
					SetValueWithoutNotify(value);
					SendEvent(changeEvent);
					return;
				}
			}
			SetValueWithoutNotify(value);
		}
	}

	public Label labelElement { get; private set; }

	public string label
	{
		get
		{
			return labelElement.text;
		}
		set
		{
			if (labelElement.text != value)
			{
				labelElement.text = value;
				if (string.IsNullOrEmpty(labelElement.text))
				{
					AddToClassList(noLabelVariantUssClassName);
					labelElement.RemoveFromHierarchy();
				}
				else if (!Contains(labelElement))
				{
					Insert(0, labelElement);
					RemoveFromClassList(noLabelVariantUssClassName);
				}
			}
		}
	}

	public bool showMixedValue
	{
		get
		{
			return m_ShowMixedValue;
		}
		set
		{
			if (value != m_ShowMixedValue)
			{
				m_ShowMixedValue = value;
				UpdateMixedValueContent();
			}
		}
	}

	protected Label mixedValueLabel
	{
		get
		{
			if (m_MixedValueLabel == null)
			{
				m_MixedValueLabel = new Label(mixedValueString)
				{
					focusable = true,
					tabIndex = -1
				};
				m_MixedValueLabel.AddToClassList(labelUssClassName);
				m_MixedValueLabel.AddToClassList(mixedValueLabelUssClassName);
			}
			return m_MixedValueLabel;
		}
	}

	internal BaseField(string label)
	{
		base.isCompositeRoot = true;
		base.focusable = true;
		base.tabIndex = 0;
		base.excludeFromFocusRing = true;
		base.delegatesFocus = true;
		AddToClassList(ussClassName);
		labelElement = new Label
		{
			focusable = true,
			tabIndex = -1
		};
		labelElement.AddToClassList(labelUssClassName);
		if (label != null)
		{
			this.label = label;
		}
		else
		{
			AddToClassList(noLabelVariantUssClassName);
		}
		RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
		m_VisualInput = null;
	}

	protected BaseField(string label, VisualElement visualInput)
		: this(label)
	{
		this.visualInput = visualInput;
	}

	private void OnAttachToPanel(AttachToPanelEvent e)
	{
		for (VisualElement visualElement = base.parent; visualElement != null; visualElement = visualElement.parent)
		{
			if (visualElement.ClassListContains("unity-inspector-element"))
			{
				m_LabelWidthRatio = 0.45f;
				m_LabelExtraPadding = 2f;
				m_LabelBaseMinWidth = 120f;
				RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
				AddToClassList(inspectorFieldUssClassName);
				m_CachedInspectorElement = visualElement;
				m_CachedListAndFoldoutDepth = this.GetListAndFoldoutDepth();
				RegisterCallback<GeometryChangedEvent>(OnInspectorFieldGeometryChanged);
				break;
			}
		}
	}

	private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
	{
		if (evt.customStyle.TryGetValue(s_LabelWidthRatioProperty, out var labelWidthRatio))
		{
			m_LabelWidthRatio = labelWidthRatio;
		}
		if (evt.customStyle.TryGetValue(s_LabelExtraPaddingProperty, out var labelExtraPadding))
		{
			m_LabelExtraPadding = labelExtraPadding;
		}
		if (evt.customStyle.TryGetValue(s_LabelBaseMinWidthProperty, out var labelBaseMinWidth))
		{
			m_LabelBaseMinWidth = labelBaseMinWidth;
		}
	}

	private void OnInspectorFieldGeometryChanged(GeometryChangedEvent e)
	{
		AlignLabel();
	}

	private void AlignLabel()
	{
		if (ClassListContains(alignedFieldUssClassName))
		{
			int num = 15 * m_CachedListAndFoldoutDepth;
			float num2 = base.resolvedStyle.paddingLeft + base.resolvedStyle.paddingRight + base.resolvedStyle.marginLeft + base.resolvedStyle.marginRight;
			num2 += m_CachedInspectorElement.resolvedStyle.paddingLeft + m_CachedInspectorElement.resolvedStyle.paddingRight + m_CachedInspectorElement.resolvedStyle.marginLeft + m_CachedInspectorElement.resolvedStyle.marginRight;
			num2 += labelElement.resolvedStyle.paddingLeft + labelElement.resolvedStyle.paddingRight + labelElement.resolvedStyle.marginLeft + labelElement.resolvedStyle.marginRight;
			num2 += base.resolvedStyle.paddingLeft + base.resolvedStyle.paddingRight + base.resolvedStyle.marginLeft + base.resolvedStyle.marginRight;
			num2 += m_LabelExtraPadding;
			num2 += (float)num;
			labelElement.style.minWidth = Mathf.Max(m_LabelBaseMinWidth - (float)num, 0f);
			float num3 = m_CachedInspectorElement.resolvedStyle.width * m_LabelWidthRatio - num2;
			if (Mathf.Abs(labelElement.resolvedStyle.width - num3) > 1E-30f)
			{
				labelElement.style.width = Mathf.Max(0f, num3);
			}
		}
	}

	protected virtual void UpdateMixedValueContent()
	{
		throw new NotImplementedException();
	}

	public virtual void SetValueWithoutNotify(TValueType newValue)
	{
		m_Value = newValue;
		if (!string.IsNullOrEmpty(base.viewDataKey))
		{
			SaveViewData();
		}
		MarkDirtyRepaint();
		if (showMixedValue)
		{
			UpdateMixedValueContent();
		}
	}

	internal override void OnViewDataReady()
	{
		base.OnViewDataReady();
		if (m_VisualInput == null)
		{
			return;
		}
		string fullHierarchicalViewDataKey = GetFullHierarchicalViewDataKey();
		TValueType val = m_Value;
		OverwriteFromViewData(this, fullHierarchicalViewDataKey);
		if (!EqualityComparer<TValueType>.Default.Equals(val, m_Value))
		{
			using (ChangeEvent<TValueType> changeEvent = ChangeEvent<TValueType>.GetPooled(val, m_Value))
			{
				changeEvent.target = this;
				SetValueWithoutNotify(m_Value);
				SendEvent(changeEvent);
			}
		}
	}

	protected override void ExecuteDefaultAction(EventBase evt)
	{
		base.ExecuteDefaultAction(evt);
		if (evt.eventTypeId == EventBase<TooltipEvent>.TypeId())
		{
			TooltipEvent tooltipEvent = (TooltipEvent)evt;
			tooltipEvent.rect = ((!string.IsNullOrEmpty(label)) ? labelElement.worldBound : base.worldBound);
			tooltipEvent.tooltip = base.tooltip;
			tooltipEvent.StopImmediatePropagation();
		}
	}
}
