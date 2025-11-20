using System;

namespace UnityEngine.UIElements;

public class Button : TextElement
{
	public new class UxmlFactory : UxmlFactory<Button, UxmlTraits>
	{
	}

	public new class UxmlTraits : TextElement.UxmlTraits
	{
		public UxmlTraits()
		{
			base.focusable.defaultValue = true;
		}
	}

	public new static readonly string ussClassName = "unity-button";

	private Clickable m_Clickable;

	private static readonly string NonEmptyString = " ";

	public Clickable clickable
	{
		get
		{
			return m_Clickable;
		}
		set
		{
			if (m_Clickable != null && m_Clickable.target == this)
			{
				this.RemoveManipulator(m_Clickable);
			}
			m_Clickable = value;
			if (m_Clickable != null)
			{
				this.AddManipulator(m_Clickable);
			}
		}
	}

	[Obsolete("onClick is obsolete. Use clicked instead (UnityUpgradable) -> clicked", true)]
	public event Action onClick
	{
		add
		{
			clicked += value;
		}
		remove
		{
			clicked -= value;
		}
	}

	public event Action clicked
	{
		add
		{
			if (m_Clickable == null)
			{
				clickable = new Clickable(value);
			}
			else
			{
				m_Clickable.clicked += value;
			}
		}
		remove
		{
			if (m_Clickable != null)
			{
				m_Clickable.clicked -= value;
			}
		}
	}

	public Button()
		: this(null)
	{
	}

	public Button(Action clickEvent)
	{
		AddToClassList(ussClassName);
		clickable = new Clickable(clickEvent);
		base.focusable = true;
		RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
		RegisterCallback<KeyDownEvent>(OnKeyDown);
	}

	private void OnNavigationSubmit(NavigationSubmitEvent evt)
	{
		clickable?.SimulateSingleClick(evt);
		evt.StopPropagation();
	}

	private void OnKeyDown(KeyDownEvent evt)
	{
		IPanel obj = base.panel;
		if (obj != null && obj.contextType == ContextType.Editor && (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space))
		{
			clickable?.SimulateSingleClick(evt);
			evt.StopPropagation();
		}
	}

	protected internal override Vector2 DoMeasure(float desiredWidth, MeasureMode widthMode, float desiredHeight, MeasureMode heightMode)
	{
		string nonEmptyString = text;
		if (string.IsNullOrEmpty(nonEmptyString))
		{
			nonEmptyString = NonEmptyString;
		}
		return MeasureTextSize(nonEmptyString, desiredWidth, widthMode, desiredHeight, heightMode);
	}
}
