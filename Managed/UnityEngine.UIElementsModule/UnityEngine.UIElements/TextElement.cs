using System;
using System.Collections.Generic;
using UnityEngine.UIElements.UIR;

namespace UnityEngine.UIElements;

public class TextElement : BindableElement, ITextElement, INotifyValueChanged<string>
{
	public new class UxmlFactory : UxmlFactory<TextElement, UxmlTraits>
	{
	}

	public new class UxmlTraits : BindableElement.UxmlTraits
	{
		private UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription
		{
			name = "text"
		};

		private UxmlBoolAttributeDescription m_EnableRichText = new UxmlBoolAttributeDescription
		{
			name = "enable-rich-text",
			defaultValue = true
		};

		private UxmlBoolAttributeDescription m_DisplayTooltipWhenElided = new UxmlBoolAttributeDescription
		{
			name = "display-tooltip-when-elided"
		};

		public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
		{
			get
			{
				yield break;
			}
		}

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			TextElement textElement = (TextElement)ve;
			textElement.text = m_Text.GetValueFromBag(bag, cc);
			textElement.enableRichText = m_EnableRichText.GetValueFromBag(bag, cc);
			textElement.displayTooltipWhenElided = m_DisplayTooltipWhenElided.GetValueFromBag(bag, cc);
		}
	}

	public static readonly string ussClassName = "unity-text-element";

	private ITextHandle m_TextHandle;

	internal static int maxTextVertices = MeshBuilder.s_MaxTextMeshVertices;

	[SerializeField]
	private string m_Text = string.Empty;

	private bool m_EnableRichText = true;

	private bool m_DisplayTooltipWhenElided = true;

	internal static readonly string k_EllipsisText = "...";

	private bool m_WasElided;

	private bool m_UpdateTextParams = true;

	private MeshGenerationContextUtils.TextParams m_TextParams;

	private int m_PreviousTextParamsHashCode = int.MaxValue;

	internal ITextHandle textHandle
	{
		get
		{
			return m_TextHandle;
		}
		set
		{
			m_TextHandle = value;
		}
	}

	public virtual string text
	{
		get
		{
			return ((INotifyValueChanged<string>)this).value;
		}
		set
		{
			((INotifyValueChanged<string>)this).value = value;
		}
	}

	public bool enableRichText
	{
		get
		{
			return m_EnableRichText;
		}
		set
		{
			if (m_EnableRichText != value)
			{
				m_EnableRichText = value;
				MarkDirtyRepaint();
			}
		}
	}

	public bool displayTooltipWhenElided
	{
		get
		{
			return m_DisplayTooltipWhenElided;
		}
		set
		{
			if (m_DisplayTooltipWhenElided != value)
			{
				m_DisplayTooltipWhenElided = value;
				UpdateVisibleText();
				MarkDirtyRepaint();
			}
		}
	}

	public bool isElided { get; private set; }

	string INotifyValueChanged<string>.value
	{
		get
		{
			return m_Text ?? string.Empty;
		}
		set
		{
			if (!(m_Text != value))
			{
				return;
			}
			if (base.panel != null)
			{
				using (ChangeEvent<string> changeEvent = ChangeEvent<string>.GetPooled(text, value))
				{
					changeEvent.target = this;
					((INotifyValueChanged<string>)this).SetValueWithoutNotify(value);
					SendEvent(changeEvent);
					return;
				}
			}
			((INotifyValueChanged<string>)this).SetValueWithoutNotify(value);
		}
	}

	public TextElement()
	{
		base.requireMeasureFunction = true;
		AddToClassList(ussClassName);
		base.generateVisualContent = (Action<MeshGenerationContext>)Delegate.Combine(base.generateVisualContent, new Action<MeshGenerationContext>(OnGenerateVisualContent));
		RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
	}

	public override void HandleEvent(EventBase evt)
	{
		if (evt.eventTypeId == EventBase<AttachToPanelEvent>.TypeId() && evt is AttachToPanelEvent)
		{
			textHandle = TextCoreHandle.New();
		}
		else if (evt.eventTypeId == EventBase<DetachFromPanelEvent>.TypeId())
		{
			DetachFromPanelEvent detachFromPanelEvent = evt as DetachFromPanelEvent;
			if (detachFromPanelEvent == null)
			{
			}
		}
		base.HandleEvent(evt);
	}

	private void OnGeometryChanged(GeometryChangedEvent e)
	{
		UpdateVisibleText();
	}

	private void OnGenerateVisualContent(MeshGenerationContext mgc)
	{
		UpdateVisibleText();
		mgc.Text(m_TextParams, m_TextHandle, base.scaledPixelsPerPoint);
		if (ShouldElide() && TextLibraryCanElide())
		{
			isElided = textHandle.IsElided();
		}
		UpdateTooltip();
		m_UpdateTextParams = true;
	}

	internal string ElideText(string drawText, string ellipsisText, float width, TextOverflowPosition textOverflowPosition)
	{
		float num = base.resolvedStyle.paddingRight;
		if (float.IsNaN(num))
		{
			num = 0f;
		}
		float num2 = Mathf.Clamp(num, 1f / base.scaledPixelsPerPoint, 1f);
		if (MeasureTextSize(drawText, 0f, MeasureMode.Undefined, 0f, MeasureMode.Undefined).x <= width + num2 || string.IsNullOrEmpty(ellipsisText))
		{
			return drawText;
		}
		string text = ((drawText.Length > 1) ? ellipsisText : drawText);
		if (MeasureTextSize(text, 0f, MeasureMode.Undefined, 0f, MeasureMode.Undefined).x >= width)
		{
			return text;
		}
		int num3 = drawText.Length - 1;
		int num4 = -1;
		string text2 = drawText;
		int num5 = ((textOverflowPosition == TextOverflowPosition.Start) ? 1 : 0);
		int num6 = ((textOverflowPosition == TextOverflowPosition.Start || textOverflowPosition == TextOverflowPosition.Middle) ? num3 : (num3 - 1));
		for (int num7 = (num5 + num6) / 2; num5 <= num6; num7 = (num5 + num6) / 2)
		{
			switch (textOverflowPosition)
			{
			case TextOverflowPosition.Start:
				text2 = ellipsisText + drawText.Substring(num7, num3 - (num7 - 1));
				break;
			case TextOverflowPosition.End:
				text2 = drawText.Substring(0, num7) + ellipsisText;
				break;
			case TextOverflowPosition.Middle:
				text2 = ((num7 - 1 <= 0) ? "" : drawText.Substring(0, num7 - 1)) + ellipsisText + ((num3 - (num7 - 1) <= 0) ? "" : drawText.Substring(num3 - (num7 - 1)));
				break;
			}
			Vector2 vector = MeasureTextSize(text2, 0f, MeasureMode.Undefined, 0f, MeasureMode.Undefined);
			if (Math.Abs(vector.x - width) < 1E-30f)
			{
				return text2;
			}
			switch (textOverflowPosition)
			{
			case TextOverflowPosition.Start:
				if (vector.x > width)
				{
					if (num4 == num7 - 1)
					{
						return ellipsisText + drawText.Substring(num4, num3 - (num4 - 1));
					}
					num5 = num7 + 1;
				}
				else
				{
					num6 = num7 - 1;
					num4 = num7;
				}
				continue;
			default:
				if (textOverflowPosition != TextOverflowPosition.Middle)
				{
					continue;
				}
				break;
			case TextOverflowPosition.End:
				break;
			}
			if (vector.x > width)
			{
				if (num4 == num7 - 1)
				{
					if (textOverflowPosition == TextOverflowPosition.End)
					{
						return drawText.Substring(0, num4) + ellipsisText;
					}
					return drawText.Substring(0, Mathf.Max(num4 - 1, 0)) + ellipsisText + drawText.Substring(num3 - Mathf.Max(num4 - 1, 0));
				}
				num6 = num7 - 1;
			}
			else
			{
				num5 = num7 + 1;
				num4 = num7;
			}
		}
		return text2;
	}

	private void UpdateTooltip()
	{
		if (displayTooltipWhenElided && isElided)
		{
			base.tooltip = text;
			m_WasElided = true;
		}
		else if (m_WasElided)
		{
			base.tooltip = null;
			m_WasElided = false;
		}
	}

	private void UpdateVisibleText()
	{
		MeshGenerationContextUtils.TextParams textParams = MeshGenerationContextUtils.TextParams.MakeStyleBased(this, text);
		int hashCode = textParams.GetHashCode();
		if (!m_UpdateTextParams && hashCode == m_PreviousTextParamsHashCode)
		{
			return;
		}
		m_TextParams = textParams;
		bool flag = ShouldElide();
		if (!flag || !TextLibraryCanElide())
		{
			if (flag)
			{
				m_TextParams.text = ElideText(m_TextParams.text, k_EllipsisText, m_TextParams.rect.width, m_TextParams.textOverflowPosition);
				isElided = flag && m_TextParams.text != text;
				m_TextParams.textOverflow = TextOverflow.Clip;
			}
			else
			{
				m_TextParams.textOverflow = TextOverflow.Clip;
				isElided = false;
			}
		}
		m_PreviousTextParamsHashCode = hashCode;
		m_UpdateTextParams = false;
	}

	private bool ShouldElide()
	{
		return base.computedStyle.textOverflow == TextOverflow.Ellipsis && base.computedStyle.overflow == OverflowInternal.Hidden && base.computedStyle.whiteSpace == WhiteSpace.NoWrap;
	}

	private bool TextLibraryCanElide()
	{
		if (textHandle.IsLegacy())
		{
			return false;
		}
		if (m_TextParams.textOverflowPosition == TextOverflowPosition.End)
		{
			return true;
		}
		return false;
	}

	public Vector2 MeasureTextSize(string textToMeasure, float width, MeasureMode widthMode, float height, MeasureMode heightMode)
	{
		return TextUtilities.MeasureVisualElementTextSize(this, textToMeasure, width, widthMode, height, heightMode, m_TextHandle);
	}

	internal static Vector2 MeasureVisualElementTextSize(VisualElement ve, string textToMeasure, float width, MeasureMode widthMode, float height, MeasureMode heightMode, TextHandle textHandle)
	{
		return TextUtilities.MeasureVisualElementTextSize(ve, textToMeasure, width, widthMode, height, heightMode, textHandle.textHandle);
	}

	protected internal override Vector2 DoMeasure(float desiredWidth, MeasureMode widthMode, float desiredHeight, MeasureMode heightMode)
	{
		return MeasureTextSize(text, desiredWidth, widthMode, desiredHeight, heightMode);
	}

	internal int VerticesCount(string text)
	{
		MeshGenerationContextUtils.TextParams textParams = m_TextParams;
		textParams.text = text;
		return textHandle.VerticesCount(textParams, base.scaledPixelsPerPoint);
	}

	void INotifyValueChanged<string>.SetValueWithoutNotify(string newValue)
	{
		if (m_Text != newValue)
		{
			m_Text = newValue;
			IncrementVersion(VersionChangeType.Layout | VersionChangeType.Repaint);
			if (!string.IsNullOrEmpty(base.viewDataKey))
			{
				SaveViewData();
			}
		}
	}
}
