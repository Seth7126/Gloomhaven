namespace UnityEngine.UI;

[AddComponentMenu("Layout/Layout Element Extended", 140)]
[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class LayoutElementExtended : LayoutElement, ILayoutElementExtended
{
	[SerializeField]
	private RectOffset m_Margin = new RectOffset();

	[SerializeField]
	private bool m_OverrideMinHeightWithPrefered;

	[SerializeField]
	private bool m_OverrideMinWidthWithPrefered;

	[SerializeField]
	private float m_ScalePreferredWidth = 1f;

	[SerializeField]
	private float m_ScalePreferredHeight = 1f;

	[SerializeField]
	private float m_MaxWidth = -1f;

	[SerializeField]
	private float m_MaxHeight = -1f;

	public virtual RectOffset margin
	{
		get
		{
			return m_Margin;
		}
		set
		{
			m_Margin = value;
			SetDirty();
		}
	}

	public int LeftMargin
	{
		get
		{
			return m_Margin.left;
		}
		set
		{
			m_Margin.left = value;
			SetDirty();
		}
	}

	public virtual float MaxHeight
	{
		get
		{
			return m_MaxHeight;
		}
		set
		{
			m_MaxHeight = value;
			SetDirty();
		}
	}

	public virtual float MaxWidth
	{
		get
		{
			return m_MaxWidth;
		}
		set
		{
			m_MaxWidth = value;
			SetDirty();
		}
	}

	public float MinHeight
	{
		get
		{
			return minHeight;
		}
		set
		{
			minHeight = value;
			SetDirty();
		}
	}

	public virtual bool overrideMinHeightWithPrefered
	{
		get
		{
			return m_OverrideMinHeightWithPrefered;
		}
		set
		{
			m_OverrideMinHeightWithPrefered = value;
			SetDirty();
		}
	}

	public virtual bool overrideMinWidthWithPrefered
	{
		get
		{
			return m_OverrideMinWidthWithPrefered;
		}
		set
		{
			m_OverrideMinWidthWithPrefered = value;
			SetDirty();
		}
	}

	public float scalePreferredHeight
	{
		get
		{
			return m_ScalePreferredHeight;
		}
		set
		{
			if (m_ScalePreferredHeight != value)
			{
				m_ScalePreferredHeight = value;
				SetDirty();
			}
		}
	}

	public float scalePreferredWidth
	{
		get
		{
			return m_ScalePreferredWidth;
		}
		set
		{
			if (m_ScalePreferredWidth != value)
			{
				m_ScalePreferredWidth = value;
				SetDirty();
			}
		}
	}

	public override void CalculateLayoutInputHorizontal()
	{
	}

	public override void CalculateLayoutInputVertical()
	{
	}
}
