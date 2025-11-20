using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal class DefaultGroupManager : IGroupManager
{
	private List<IGroupBoxOption> m_GroupOptions = new List<IGroupBoxOption>();

	private IGroupBoxOption m_SelectedOption;

	public IGroupBoxOption GetSelectedOption()
	{
		return m_SelectedOption;
	}

	public void OnOptionSelectionChanged(IGroupBoxOption selectedOption)
	{
		if (m_SelectedOption == selectedOption)
		{
			return;
		}
		m_SelectedOption = selectedOption;
		foreach (IGroupBoxOption groupOption in m_GroupOptions)
		{
			groupOption.SetSelected(groupOption == m_SelectedOption);
		}
	}

	public void RegisterOption(IGroupBoxOption option)
	{
		if (!m_GroupOptions.Contains(option))
		{
			m_GroupOptions.Add(option);
		}
	}

	public void UnregisterOption(IGroupBoxOption option)
	{
		m_GroupOptions.Remove(option);
	}
}
