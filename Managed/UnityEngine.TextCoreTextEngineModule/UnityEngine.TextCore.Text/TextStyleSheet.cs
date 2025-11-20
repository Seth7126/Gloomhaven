using System;
using System.Collections.Generic;

namespace UnityEngine.TextCore.Text;

[Serializable]
[ExcludeFromObjectFactory]
[ExcludeFromPreset]
public class TextStyleSheet : ScriptableObject
{
	[SerializeField]
	private List<TextStyle> m_StyleList = new List<TextStyle>(1);

	private Dictionary<int, TextStyle> m_StyleLookupDictionary;

	internal List<TextStyle> styles => m_StyleList;

	public TextStyle GetStyle(int hashCode)
	{
		if (m_StyleLookupDictionary == null)
		{
			LoadStyleDictionaryInternal();
		}
		if (m_StyleLookupDictionary.TryGetValue(hashCode, out var value))
		{
			return value;
		}
		return null;
	}

	public TextStyle GetStyle(string name)
	{
		if (m_StyleLookupDictionary == null)
		{
			LoadStyleDictionaryInternal();
		}
		int hashCodeCaseInSensitive = TextUtilities.GetHashCodeCaseInSensitive(name);
		if (m_StyleLookupDictionary.TryGetValue(hashCodeCaseInSensitive, out var value))
		{
			return value;
		}
		return null;
	}

	public void RefreshStyles()
	{
		LoadStyleDictionaryInternal();
	}

	private void LoadStyleDictionaryInternal()
	{
		if (m_StyleLookupDictionary == null)
		{
			m_StyleLookupDictionary = new Dictionary<int, TextStyle>();
		}
		else
		{
			m_StyleLookupDictionary.Clear();
		}
		for (int i = 0; i < m_StyleList.Count; i++)
		{
			m_StyleList[i].RefreshStyle();
			if (!m_StyleLookupDictionary.ContainsKey(m_StyleList[i].hashCode))
			{
				m_StyleLookupDictionary.Add(m_StyleList[i].hashCode, m_StyleList[i]);
			}
		}
	}
}
