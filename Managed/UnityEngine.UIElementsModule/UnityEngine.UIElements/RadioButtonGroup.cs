using System.Collections.Generic;

namespace UnityEngine.UIElements;

public class RadioButtonGroup : BaseField<int>, IGroupBox
{
	public new class UxmlFactory : UxmlFactory<RadioButtonGroup, UxmlTraits>
	{
	}

	public new class UxmlTraits : BaseFieldTraits<int, UxmlIntAttributeDescription>
	{
		private UxmlStringAttributeDescription m_Choices = new UxmlStringAttributeDescription
		{
			name = "choices"
		};

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			RadioButtonGroup radioButtonGroup = (RadioButtonGroup)ve;
			radioButtonGroup.choices = BaseField<int>.UxmlTraits.ParseChoiceList(m_Choices.GetValueFromBag(bag, cc));
		}
	}

	public new static readonly string ussClassName = "unity-radio-button-group";

	private IEnumerable<string> m_Choices;

	private List<RadioButton> m_RadioButtons = new List<RadioButton>();

	private EventCallback<ChangeEvent<bool>> m_RadioButtonValueChangedCallback;

	public IEnumerable<string> choices
	{
		get
		{
			return m_Choices;
		}
		set
		{
			m_Choices = value;
			foreach (RadioButton radioButton2 in m_RadioButtons)
			{
				radioButton2.UnregisterValueChangedCallback(m_RadioButtonValueChangedCallback);
				radioButton2.RemoveFromHierarchy();
			}
			m_RadioButtons.Clear();
			if (m_Choices == null)
			{
				return;
			}
			foreach (string choice in m_Choices)
			{
				RadioButton radioButton = new RadioButton
				{
					text = choice
				};
				radioButton.RegisterValueChangedCallback(m_RadioButtonValueChangedCallback);
				m_RadioButtons.Add(radioButton);
				base.visualInput.Add(radioButton);
			}
			UpdateRadioButtons();
		}
	}

	public RadioButtonGroup()
		: this(null)
	{
	}

	public RadioButtonGroup(string label, List<string> radioButtonChoices = null)
		: base(label, (VisualElement)null)
	{
		AddToClassList(ussClassName);
		m_RadioButtonValueChangedCallback = RadioButtonValueChangedCallback;
		choices = radioButtonChoices;
		value = -1;
		base.visualInput.focusable = false;
		base.delegatesFocus = true;
	}

	private void RadioButtonValueChangedCallback(ChangeEvent<bool> evt)
	{
		if (evt.newValue)
		{
			value = m_RadioButtons.IndexOf(evt.target as RadioButton);
			evt.StopPropagation();
		}
	}

	public override void SetValueWithoutNotify(int newValue)
	{
		base.SetValueWithoutNotify(newValue);
		UpdateRadioButtons();
	}

	private void UpdateRadioButtons()
	{
		if (value >= 0 && value < m_RadioButtons.Count)
		{
			m_RadioButtons[value].value = true;
			return;
		}
		foreach (RadioButton radioButton in m_RadioButtons)
		{
			radioButton.value = false;
		}
	}
}
