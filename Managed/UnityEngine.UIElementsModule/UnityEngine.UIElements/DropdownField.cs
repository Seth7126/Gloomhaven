using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

public class DropdownField : BaseField<string>
{
	public new class UxmlFactory : UxmlFactory<DropdownField, UxmlTraits>
	{
	}

	public new class UxmlTraits : BaseField<string>.UxmlTraits
	{
		private UxmlIntAttributeDescription m_Index = new UxmlIntAttributeDescription
		{
			name = "index"
		};

		private UxmlStringAttributeDescription m_Choices = new UxmlStringAttributeDescription
		{
			name = "choices"
		};

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			DropdownField dropdownField = (DropdownField)ve;
			dropdownField.choices = BaseField<string>.UxmlTraits.ParseChoiceList(m_Choices.GetValueFromBag(bag, cc));
			dropdownField.index = m_Index.GetValueFromBag(bag, cc);
		}
	}

	private class PopupTextElement : TextElement
	{
		protected internal override Vector2 DoMeasure(float desiredWidth, MeasureMode widthMode, float desiredHeight, MeasureMode heightMode)
		{
			string text = this.text;
			if (string.IsNullOrEmpty(text))
			{
				text = " ";
			}
			return MeasureTextSize(text, desiredWidth, widthMode, desiredHeight, heightMode);
		}
	}

	internal List<string> m_Choices;

	private TextElement m_TextElement;

	private VisualElement m_ArrowElement;

	internal Func<string, string> m_FormatSelectedValueCallback;

	internal Func<string, string> m_FormatListItemCallback;

	internal Func<IGenericMenu> createMenuCallback = null;

	private int m_Index = -1;

	internal static readonly string ussClassNameBasePopupField = "unity-base-popup-field";

	internal static readonly string textUssClassNameBasePopupField = ussClassNameBasePopupField + "__text";

	internal static readonly string arrowUssClassNameBasePopupField = ussClassNameBasePopupField + "__arrow";

	internal static readonly string labelUssClassNameBasePopupField = ussClassNameBasePopupField + "__label";

	internal static readonly string inputUssClassNameBasePopupField = ussClassNameBasePopupField + "__input";

	internal static readonly string ussClassNamePopupField = "unity-popup-field";

	internal static readonly string labelUssClassNamePopupField = ussClassNamePopupField + "__label";

	internal static readonly string inputUssClassNamePopupField = ussClassNamePopupField + "__input";

	protected TextElement textElement => m_TextElement;

	public string text => m_TextElement.text;

	internal virtual Func<string, string> formatSelectedValueCallback
	{
		get
		{
			return m_FormatSelectedValueCallback;
		}
		set
		{
			m_FormatSelectedValueCallback = value;
			textElement.text = GetValueToDisplay();
		}
	}

	internal virtual Func<string, string> formatListItemCallback
	{
		get
		{
			return m_FormatListItemCallback;
		}
		set
		{
			m_FormatListItemCallback = value;
		}
	}

	public int index
	{
		get
		{
			return m_Index;
		}
		set
		{
			m_Index = value;
			if (m_Choices == null || value >= m_Choices.Count || value < 0)
			{
				this.value = null;
			}
			else
			{
				this.value = m_Choices[m_Index];
			}
		}
	}

	public virtual List<string> choices
	{
		get
		{
			return m_Choices;
		}
		set
		{
			m_Choices = value;
			SetValueWithoutNotify(base.rawValue);
		}
	}

	public override string value
	{
		get
		{
			return base.value;
		}
		set
		{
			m_Index = m_Choices?.IndexOf(value) ?? (-1);
			base.value = value;
		}
	}

	internal string GetValueToDisplay()
	{
		if (m_FormatSelectedValueCallback != null)
		{
			return m_FormatSelectedValueCallback(value);
		}
		return value ?? string.Empty;
	}

	internal string GetListItemToDisplay(string value)
	{
		if (m_FormatListItemCallback != null)
		{
			return m_FormatListItemCallback(value);
		}
		return (value != null && m_Choices.Contains(value)) ? value : string.Empty;
	}

	public DropdownField()
		: this(null)
	{
	}

	public DropdownField(string label)
		: base(label, (VisualElement)null)
	{
		AddToClassList(ussClassNameBasePopupField);
		base.labelElement.AddToClassList(labelUssClassNameBasePopupField);
		m_TextElement = new PopupTextElement
		{
			pickingMode = PickingMode.Ignore
		};
		m_TextElement.AddToClassList(textUssClassNameBasePopupField);
		base.visualInput.AddToClassList(inputUssClassNameBasePopupField);
		base.visualInput.Add(m_TextElement);
		m_ArrowElement = new VisualElement();
		m_ArrowElement.AddToClassList(arrowUssClassNameBasePopupField);
		m_ArrowElement.pickingMode = PickingMode.Ignore;
		base.visualInput.Add(m_ArrowElement);
		choices = new List<string>();
		AddToClassList(ussClassNamePopupField);
		base.labelElement.AddToClassList(labelUssClassNamePopupField);
		base.visualInput.AddToClassList(inputUssClassNamePopupField);
	}

	public DropdownField(List<string> choices, string defaultValue, Func<string, string> formatSelectedValueCallback = null, Func<string, string> formatListItemCallback = null)
		: this(null, choices, defaultValue, formatSelectedValueCallback, formatListItemCallback)
	{
	}

	public DropdownField(string label, List<string> choices, string defaultValue, Func<string, string> formatSelectedValueCallback = null, Func<string, string> formatListItemCallback = null)
		: this(label)
	{
		if (defaultValue == null)
		{
			throw new ArgumentNullException("defaultValue");
		}
		this.choices = choices;
		SetValueWithoutNotify(defaultValue);
		this.formatListItemCallback = formatListItemCallback;
		this.formatSelectedValueCallback = formatSelectedValueCallback;
	}

	public DropdownField(List<string> choices, int defaultIndex, Func<string, string> formatSelectedValueCallback = null, Func<string, string> formatListItemCallback = null)
		: this(null, choices, defaultIndex, formatSelectedValueCallback, formatListItemCallback)
	{
	}

	public DropdownField(string label, List<string> choices, int defaultIndex, Func<string, string> formatSelectedValueCallback = null, Func<string, string> formatListItemCallback = null)
		: this(label)
	{
		this.choices = choices;
		index = defaultIndex;
		this.formatListItemCallback = formatListItemCallback;
		this.formatSelectedValueCallback = formatSelectedValueCallback;
	}

	internal void AddMenuItems(IGenericMenu menu)
	{
		if (menu == null)
		{
			throw new ArgumentNullException("menu");
		}
		if (m_Choices == null)
		{
			return;
		}
		foreach (string item in m_Choices)
		{
			bool isChecked = item == value;
			menu.AddItem(GetListItemToDisplay(item), isChecked, delegate
			{
				ChangeValueFromMenu(item);
			});
		}
	}

	private void ChangeValueFromMenu(string menuItem)
	{
		value = menuItem;
	}

	public override void SetValueWithoutNotify(string newValue)
	{
		m_Index = m_Choices?.IndexOf(newValue) ?? (-1);
		base.SetValueWithoutNotify(newValue);
		((INotifyValueChanged<string>)m_TextElement).SetValueWithoutNotify(GetValueToDisplay());
	}

	protected override void ExecuteDefaultActionAtTarget(EventBase evt)
	{
		base.ExecuteDefaultActionAtTarget(evt);
		if (evt == null)
		{
			return;
		}
		bool flag = false;
		if (evt is KeyDownEvent keyDownEvent)
		{
			if (keyDownEvent.keyCode == KeyCode.Space || keyDownEvent.keyCode == KeyCode.KeypadEnter || keyDownEvent.keyCode == KeyCode.Return)
			{
				flag = true;
			}
		}
		else
		{
			MouseDownEvent obj = evt as MouseDownEvent;
			if (obj != null && obj.button == 0)
			{
				MouseDownEvent mouseDownEvent = (MouseDownEvent)evt;
				if (base.visualInput.ContainsPoint(base.visualInput.WorldToLocal(mouseDownEvent.mousePosition)))
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			ShowMenu();
			evt.StopPropagation();
		}
	}

	private void ShowMenu()
	{
		IGenericMenu genericMenu;
		if (createMenuCallback != null)
		{
			genericMenu = createMenuCallback();
		}
		else
		{
			BaseVisualElementPanel baseVisualElementPanel = base.elementPanel;
			IGenericMenu genericMenu2;
			if (baseVisualElementPanel == null || baseVisualElementPanel.contextType != ContextType.Player)
			{
				genericMenu2 = DropdownUtility.CreateDropdown();
			}
			else
			{
				IGenericMenu genericMenu3 = new GenericDropdownMenu();
				genericMenu2 = genericMenu3;
			}
			genericMenu = genericMenu2;
		}
		AddMenuItems(genericMenu);
		genericMenu.DropDown(base.visualInput.worldBound, this, anchored: true);
	}

	protected override void UpdateMixedValueContent()
	{
		if (base.showMixedValue)
		{
			textElement.text = BaseField<string>.mixedValueString;
		}
		textElement.EnableInClassList(BaseField<string>.mixedValueLabelUssClassName, base.showMixedValue);
	}
}
