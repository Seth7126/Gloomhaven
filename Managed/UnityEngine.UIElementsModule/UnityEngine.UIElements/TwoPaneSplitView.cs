using System.Collections.Generic;

namespace UnityEngine.UIElements;

public class TwoPaneSplitView : VisualElement
{
	public new class UxmlFactory : UxmlFactory<TwoPaneSplitView, UxmlTraits>
	{
	}

	public new class UxmlTraits : VisualElement.UxmlTraits
	{
		private UxmlIntAttributeDescription m_FixedPaneIndex = new UxmlIntAttributeDescription
		{
			name = "fixed-pane-index",
			defaultValue = 0
		};

		private UxmlIntAttributeDescription m_FixedPaneInitialDimension = new UxmlIntAttributeDescription
		{
			name = "fixed-pane-initial-dimension",
			defaultValue = 100
		};

		private UxmlEnumAttributeDescription<TwoPaneSplitViewOrientation> m_Orientation = new UxmlEnumAttributeDescription<TwoPaneSplitViewOrientation>
		{
			name = "orientation",
			defaultValue = TwoPaneSplitViewOrientation.Horizontal
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
			int valueFromBag = m_FixedPaneIndex.GetValueFromBag(bag, cc);
			int valueFromBag2 = m_FixedPaneInitialDimension.GetValueFromBag(bag, cc);
			TwoPaneSplitViewOrientation valueFromBag3 = m_Orientation.GetValueFromBag(bag, cc);
			((TwoPaneSplitView)ve).Init(valueFromBag, valueFromBag2, valueFromBag3);
		}
	}

	private static readonly string s_UssClassName = "unity-two-pane-split-view";

	private static readonly string s_ContentContainerClassName = "unity-two-pane-split-view__content-container";

	private static readonly string s_HandleDragLineClassName = "unity-two-pane-split-view__dragline";

	private static readonly string s_HandleDragLineVerticalClassName = s_HandleDragLineClassName + "--vertical";

	private static readonly string s_HandleDragLineHorizontalClassName = s_HandleDragLineClassName + "--horizontal";

	private static readonly string s_HandleDragLineAnchorClassName = "unity-two-pane-split-view__dragline-anchor";

	private static readonly string s_HandleDragLineAnchorVerticalClassName = s_HandleDragLineAnchorClassName + "--vertical";

	private static readonly string s_HandleDragLineAnchorHorizontalClassName = s_HandleDragLineAnchorClassName + "--horizontal";

	private static readonly string s_VerticalClassName = "unity-two-pane-split-view--vertical";

	private static readonly string s_HorizontalClassName = "unity-two-pane-split-view--horizontal";

	private VisualElement m_LeftPane;

	private VisualElement m_RightPane;

	private VisualElement m_FixedPane;

	private VisualElement m_FlexedPane;

	[SerializeField]
	private float m_FixedPaneDimension = -1f;

	private VisualElement m_DragLine;

	private VisualElement m_DragLineAnchor;

	private bool m_CollapseMode;

	private VisualElement m_Content;

	private TwoPaneSplitViewOrientation m_Orientation;

	private int m_FixedPaneIndex;

	private float m_FixedPaneInitialDimension;

	internal TwoPaneSplitViewResizer m_Resizer;

	public VisualElement fixedPane => m_FixedPane;

	public VisualElement flexedPane => m_FlexedPane;

	public int fixedPaneIndex
	{
		get
		{
			return m_FixedPaneIndex;
		}
		set
		{
			if (value != m_FixedPaneIndex)
			{
				Init(value, m_FixedPaneInitialDimension, m_Orientation);
			}
		}
	}

	public float fixedPaneInitialDimension
	{
		get
		{
			return m_FixedPaneInitialDimension;
		}
		set
		{
			if (value != m_FixedPaneInitialDimension)
			{
				Init(m_FixedPaneIndex, value, m_Orientation);
			}
		}
	}

	public TwoPaneSplitViewOrientation orientation
	{
		get
		{
			return m_Orientation;
		}
		set
		{
			if (value != m_Orientation)
			{
				Init(m_FixedPaneIndex, m_FixedPaneInitialDimension, value);
			}
		}
	}

	internal float fixedPaneDimension
	{
		get
		{
			return string.IsNullOrEmpty(base.viewDataKey) ? m_FixedPaneInitialDimension : m_FixedPaneDimension;
		}
		set
		{
			if (value != m_FixedPaneDimension)
			{
				m_FixedPaneDimension = value;
				SaveViewData();
			}
		}
	}

	public override VisualElement contentContainer => m_Content;

	public TwoPaneSplitView()
	{
		AddToClassList(s_UssClassName);
		m_Content = new VisualElement();
		m_Content.name = "unity-content-container";
		m_Content.AddToClassList(s_ContentContainerClassName);
		base.hierarchy.Add(m_Content);
		m_DragLineAnchor = new VisualElement();
		m_DragLineAnchor.name = "unity-dragline-anchor";
		m_DragLineAnchor.AddToClassList(s_HandleDragLineAnchorClassName);
		base.hierarchy.Add(m_DragLineAnchor);
		m_DragLine = new VisualElement();
		m_DragLine.name = "unity-dragline";
		m_DragLine.AddToClassList(s_HandleDragLineClassName);
		m_DragLineAnchor.Add(m_DragLine);
	}

	public TwoPaneSplitView(int fixedPaneIndex, float fixedPaneStartDimension, TwoPaneSplitViewOrientation orientation)
		: this()
	{
		Init(fixedPaneIndex, fixedPaneStartDimension, orientation);
	}

	public void CollapseChild(int index)
	{
		if (m_LeftPane != null)
		{
			m_DragLine.style.display = DisplayStyle.None;
			m_DragLineAnchor.style.display = DisplayStyle.None;
			if (index == 0)
			{
				m_RightPane.style.width = StyleKeyword.Initial;
				m_RightPane.style.height = StyleKeyword.Initial;
				m_RightPane.style.flexGrow = 1f;
				m_LeftPane.style.display = DisplayStyle.None;
			}
			else
			{
				m_LeftPane.style.width = StyleKeyword.Initial;
				m_LeftPane.style.height = StyleKeyword.Initial;
				m_LeftPane.style.flexGrow = 1f;
				m_RightPane.style.display = DisplayStyle.None;
			}
			m_CollapseMode = true;
		}
	}

	public void UnCollapse()
	{
		if (m_LeftPane != null)
		{
			m_LeftPane.style.display = DisplayStyle.Flex;
			m_RightPane.style.display = DisplayStyle.Flex;
			m_DragLine.style.display = DisplayStyle.Flex;
			m_DragLineAnchor.style.display = DisplayStyle.Flex;
			m_LeftPane.style.flexGrow = 0f;
			m_RightPane.style.flexGrow = 0f;
			m_CollapseMode = false;
			Init(m_FixedPaneIndex, m_FixedPaneInitialDimension, m_Orientation);
		}
	}

	internal void Init(int fixedPaneIndex, float fixedPaneInitialDimension, TwoPaneSplitViewOrientation orientation)
	{
		m_Orientation = orientation;
		m_FixedPaneIndex = fixedPaneIndex;
		m_FixedPaneInitialDimension = fixedPaneInitialDimension;
		m_Content.RemoveFromClassList(s_HorizontalClassName);
		m_Content.RemoveFromClassList(s_VerticalClassName);
		if (m_Orientation == TwoPaneSplitViewOrientation.Horizontal)
		{
			m_Content.AddToClassList(s_HorizontalClassName);
		}
		else
		{
			m_Content.AddToClassList(s_VerticalClassName);
		}
		m_DragLineAnchor.RemoveFromClassList(s_HandleDragLineAnchorHorizontalClassName);
		m_DragLineAnchor.RemoveFromClassList(s_HandleDragLineAnchorVerticalClassName);
		if (m_Orientation == TwoPaneSplitViewOrientation.Horizontal)
		{
			m_DragLineAnchor.AddToClassList(s_HandleDragLineAnchorHorizontalClassName);
		}
		else
		{
			m_DragLineAnchor.AddToClassList(s_HandleDragLineAnchorVerticalClassName);
		}
		m_DragLine.RemoveFromClassList(s_HandleDragLineHorizontalClassName);
		m_DragLine.RemoveFromClassList(s_HandleDragLineVerticalClassName);
		if (m_Orientation == TwoPaneSplitViewOrientation.Horizontal)
		{
			m_DragLine.AddToClassList(s_HandleDragLineHorizontalClassName);
		}
		else
		{
			m_DragLine.AddToClassList(s_HandleDragLineVerticalClassName);
		}
		if (m_Resizer != null)
		{
			m_DragLineAnchor.RemoveManipulator(m_Resizer);
			m_Resizer = null;
		}
		if (m_Content.childCount != 2)
		{
			RegisterCallback<GeometryChangedEvent>(OnPostDisplaySetup);
		}
		else
		{
			PostDisplaySetup();
		}
	}

	private void OnPostDisplaySetup(GeometryChangedEvent evt)
	{
		if (m_Content.childCount != 2)
		{
			Debug.LogError("TwoPaneSplitView needs exactly 2 children.");
			return;
		}
		PostDisplaySetup();
		UnregisterCallback<GeometryChangedEvent>(OnPostDisplaySetup);
	}

	private void PostDisplaySetup()
	{
		if (m_Content.childCount != 2)
		{
			Debug.LogError("TwoPaneSplitView needs exactly 2 children.");
			return;
		}
		if (fixedPaneDimension < 0f)
		{
			fixedPaneDimension = m_FixedPaneInitialDimension;
		}
		float num = fixedPaneDimension;
		m_LeftPane = m_Content[0];
		if (m_FixedPaneIndex == 0)
		{
			m_FixedPane = m_LeftPane;
		}
		else
		{
			m_FlexedPane = m_LeftPane;
		}
		m_RightPane = m_Content[1];
		if (m_FixedPaneIndex == 1)
		{
			m_FixedPane = m_RightPane;
		}
		else
		{
			m_FlexedPane = m_RightPane;
		}
		m_FixedPane.style.flexBasis = StyleKeyword.Null;
		m_FixedPane.style.flexShrink = StyleKeyword.Null;
		m_FixedPane.style.flexGrow = StyleKeyword.Null;
		m_FlexedPane.style.flexGrow = StyleKeyword.Null;
		m_FlexedPane.style.flexShrink = StyleKeyword.Null;
		m_FlexedPane.style.flexBasis = StyleKeyword.Null;
		m_FixedPane.style.width = StyleKeyword.Null;
		m_FixedPane.style.height = StyleKeyword.Null;
		m_FlexedPane.style.width = StyleKeyword.Null;
		m_FlexedPane.style.height = StyleKeyword.Null;
		if (m_Orientation == TwoPaneSplitViewOrientation.Horizontal)
		{
			m_FixedPane.style.width = num;
			m_FixedPane.style.height = StyleKeyword.Null;
		}
		else
		{
			m_FixedPane.style.width = StyleKeyword.Null;
			m_FixedPane.style.height = num;
		}
		m_FixedPane.style.flexShrink = 0f;
		m_FixedPane.style.flexGrow = 0f;
		m_FlexedPane.style.flexGrow = 1f;
		m_FlexedPane.style.flexShrink = 0f;
		m_FlexedPane.style.flexBasis = 0f;
		if (m_Orientation == TwoPaneSplitViewOrientation.Horizontal)
		{
			if (m_FixedPaneIndex == 0)
			{
				m_DragLineAnchor.style.left = num;
			}
			else
			{
				m_DragLineAnchor.style.left = base.resolvedStyle.width - num;
			}
		}
		else if (m_FixedPaneIndex == 0)
		{
			m_DragLineAnchor.style.top = num;
		}
		else
		{
			m_DragLineAnchor.style.top = base.resolvedStyle.height - num;
		}
		int num2 = 1;
		num2 = ((m_FixedPaneIndex == 0) ? 1 : (-1));
		if (m_FixedPaneIndex == 0)
		{
			m_Resizer = new TwoPaneSplitViewResizer(this, num2, m_Orientation);
		}
		else
		{
			m_Resizer = new TwoPaneSplitViewResizer(this, num2, m_Orientation);
		}
		m_DragLineAnchor.AddManipulator(m_Resizer);
		RegisterCallback<GeometryChangedEvent>(OnSizeChange);
	}

	private void OnSizeChange(GeometryChangedEvent evt)
	{
		OnSizeChange();
	}

	private void OnSizeChange()
	{
		if (!m_CollapseMode)
		{
			float num = base.resolvedStyle.width;
			float num2 = m_FixedPane.resolvedStyle.width;
			float value = m_FixedPane.resolvedStyle.minWidth.value;
			float value2 = m_FlexedPane.resolvedStyle.minWidth.value;
			if (m_Orientation == TwoPaneSplitViewOrientation.Vertical)
			{
				num = base.resolvedStyle.height;
				num2 = m_FixedPane.resolvedStyle.height;
				value = m_FixedPane.resolvedStyle.minHeight.value;
				value2 = m_FlexedPane.resolvedStyle.minHeight.value;
			}
			if (num >= num2 + value2)
			{
				SetDragLineOffset((m_FixedPaneIndex == 0) ? num2 : (num - num2));
			}
			else if (num >= value + value2)
			{
				float num3 = num - value2;
				SetFixedPaneDimension(num3);
				SetDragLineOffset((m_FixedPaneIndex == 0) ? num3 : value2);
			}
			else
			{
				SetFixedPaneDimension(value);
				SetDragLineOffset((m_FixedPaneIndex == 0) ? value : value2);
			}
		}
	}

	internal override void OnViewDataReady()
	{
		base.OnViewDataReady();
		string fullHierarchicalViewDataKey = GetFullHierarchicalViewDataKey();
		OverwriteFromViewData(this, fullHierarchicalViewDataKey);
		PostDisplaySetup();
	}

	private void SetDragLineOffset(float offset)
	{
		if (m_Orientation == TwoPaneSplitViewOrientation.Horizontal)
		{
			m_DragLineAnchor.style.left = offset;
		}
		else
		{
			m_DragLineAnchor.style.top = offset;
		}
	}

	private void SetFixedPaneDimension(float dimension)
	{
		if (m_Orientation == TwoPaneSplitViewOrientation.Horizontal)
		{
			m_FixedPane.style.width = dimension;
		}
		else
		{
			m_FixedPane.style.height = dimension;
		}
	}
}
