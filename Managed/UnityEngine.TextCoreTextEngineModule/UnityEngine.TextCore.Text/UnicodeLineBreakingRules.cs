using System;
using System.Collections.Generic;

namespace UnityEngine.TextCore.Text;

[Serializable]
public class UnicodeLineBreakingRules
{
	private static UnicodeLineBreakingRules s_Instance = new UnicodeLineBreakingRules();

	[SerializeField]
	private UnityEngine.TextAsset m_UnicodeLineBreakingRules;

	[SerializeField]
	private UnityEngine.TextAsset m_LeadingCharacters;

	[SerializeField]
	private UnityEngine.TextAsset m_FollowingCharacters;

	[SerializeField]
	private bool m_UseModernHangulLineBreakingRules;

	private static HashSet<uint> s_LeadingCharactersLookup;

	private static HashSet<uint> s_FollowingCharactersLookup;

	public UnityEngine.TextAsset lineBreakingRules => m_UnicodeLineBreakingRules;

	public UnityEngine.TextAsset leadingCharacters => m_LeadingCharacters;

	public UnityEngine.TextAsset followingCharacters => m_FollowingCharacters;

	internal HashSet<uint> leadingCharactersLookup
	{
		get
		{
			if (s_LeadingCharactersLookup == null)
			{
				LoadLineBreakingRules();
			}
			return s_LeadingCharactersLookup;
		}
		set
		{
			s_LeadingCharactersLookup = value;
		}
	}

	internal HashSet<uint> followingCharactersLookup
	{
		get
		{
			if (s_LeadingCharactersLookup == null)
			{
				LoadLineBreakingRules();
			}
			return s_FollowingCharactersLookup;
		}
		set
		{
			s_FollowingCharactersLookup = value;
		}
	}

	public bool useModernHangulLineBreakingRules
	{
		get
		{
			return m_UseModernHangulLineBreakingRules;
		}
		set
		{
			m_UseModernHangulLineBreakingRules = value;
		}
	}

	internal static void LoadLineBreakingRules()
	{
		if (s_LeadingCharactersLookup == null)
		{
			if (s_Instance.m_LeadingCharacters == null)
			{
				s_Instance.m_LeadingCharacters = Resources.Load<UnityEngine.TextAsset>("LineBreaking Leading Characters");
			}
			s_LeadingCharactersLookup = ((s_Instance.m_LeadingCharacters != null) ? GetCharacters(s_Instance.m_LeadingCharacters) : new HashSet<uint>());
			if (s_Instance.m_FollowingCharacters == null)
			{
				s_Instance.m_FollowingCharacters = Resources.Load<UnityEngine.TextAsset>("LineBreaking Following Characters");
			}
			s_FollowingCharactersLookup = ((s_Instance.m_FollowingCharacters != null) ? GetCharacters(s_Instance.m_FollowingCharacters) : new HashSet<uint>());
		}
	}

	internal static void LoadLineBreakingRules(UnityEngine.TextAsset leadingRules, UnityEngine.TextAsset followingRules)
	{
		if (s_LeadingCharactersLookup == null)
		{
			if (leadingRules == null)
			{
				leadingRules = Resources.Load<UnityEngine.TextAsset>("LineBreaking Leading Characters");
			}
			s_LeadingCharactersLookup = ((leadingRules != null) ? GetCharacters(leadingRules) : new HashSet<uint>());
			if (followingRules == null)
			{
				followingRules = Resources.Load<UnityEngine.TextAsset>("LineBreaking Following Characters");
			}
			s_FollowingCharactersLookup = ((followingRules != null) ? GetCharacters(followingRules) : new HashSet<uint>());
		}
	}

	private static HashSet<uint> GetCharacters(UnityEngine.TextAsset file)
	{
		HashSet<uint> hashSet = new HashSet<uint>();
		string text = file.text;
		for (int i = 0; i < text.Length; i++)
		{
			hashSet.Add(text[i]);
		}
		return hashSet;
	}
}
