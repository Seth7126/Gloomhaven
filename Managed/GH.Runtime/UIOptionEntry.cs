using TMPro;
using UnityEngine;

public class UIOptionEntry : LocalizedListener
{
	[SerializeField]
	private TMP_Text description;

	[SerializeField]
	private string m_DescriptionLoc;

	[SerializeField]
	private bool m_IsEnabled;

	[SerializeField]
	private Color m_DisabledColorText;

	[SerializeField]
	private CustomOptionEntryEnableStrategy _customOptionEntryEnableStrategy;

	private IOptionEntryStrategy _currentStrategy;

	public string MDescriptionLoc => m_DescriptionLoc;

	public Color DisabledColorText => m_DisabledColorText;

	private void Awake()
	{
		if (m_DescriptionLoc.IsNOTNullOrEmpty())
		{
			Init(m_DescriptionLoc);
		}
	}

	public void Init(string descriptionLoc)
	{
		m_DescriptionLoc = descriptionLoc;
		Enable(m_IsEnabled);
	}

	private void InitCurrentStrategy()
	{
		if (_currentStrategy == null)
		{
			if (_customOptionEntryEnableStrategy == null)
			{
				_currentStrategy = new DefaultOptionEntryEnableStrategy();
			}
			else
			{
				_currentStrategy = _customOptionEntryEnableStrategy;
			}
		}
	}

	public void Enable(bool enable)
	{
		m_IsEnabled = enable;
		InitCurrentStrategy();
		_currentStrategy.Enable(this, enable);
	}

	public void SetDescriptionText(string text)
	{
		description.text = text;
	}

	protected override void OnLanguageChanged()
	{
		if (m_DescriptionLoc.IsNOTNullOrEmpty())
		{
			Enable(m_IsEnabled);
		}
	}
}
