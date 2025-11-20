using System;
using System.Collections.Generic;

namespace Platforms.Activities;

[Serializable]
public class ActivitiesProgressData
{
	private readonly List<string> _available = new List<string>();

	private readonly List<string> _active = new List<string>();

	private readonly Dictionary<string, List<ActivityResult>> _ended = new Dictionary<string, List<ActivityResult>>();

	private readonly Dictionary<string, List<int>> _endedWithScore = new Dictionary<string, List<int>>();

	private readonly List<string> _needsTobeSendAsStarted = new List<string>();

	private readonly Dictionary<string, List<ActivityResult>> _needsToBeSendAsEnded = new Dictionary<string, List<ActivityResult>>();

	private readonly Dictionary<string, List<int>> _needsToBeSendAsEndedWithScore = new Dictionary<string, List<int>>();

	private List<string> _activeIncludeFilterTags = new List<string>();

	public List<string> Available => _available;

	public List<string> Active => _active;

	public Dictionary<string, List<ActivityResult>> Ended => _ended;

	public Dictionary<string, List<int>> EndedWithScore => _endedWithScore;

	public List<string> NeedsTobeSendAsStarted => _needsTobeSendAsStarted;

	public Dictionary<string, List<ActivityResult>> NeedsToBeSendAsEnded => _needsToBeSendAsEnded;

	public Dictionary<string, List<int>> NeedsToBeSendAsEndedWithScore => _needsToBeSendAsEndedWithScore;

	public List<string> ActiveIncludeFilterTags => _activeIncludeFilterTags;

	public static bool Equals(ActivitiesProgressData first, ActivitiesProgressData second)
	{
		if (first == null || second == null)
		{
			if (first != null || second != null)
			{
				return false;
			}
			return true;
		}
		if (!ListsEquals<string>(first._available, second._available) || !ListsEquals<string>(first._active, second._active) || !ListsEquals<string>(first._needsTobeSendAsStarted, second._needsTobeSendAsStarted) || !ListsEquals<string>(first._activeIncludeFilterTags, second._activeIncludeFilterTags) || !DictionariesEquals<ActivityResult>(first._ended, second._ended) || !DictionariesEquals<int>(first._endedWithScore, second._endedWithScore) || !DictionariesEquals<ActivityResult>(first._needsToBeSendAsEnded, second._needsToBeSendAsEnded) || !DictionariesEquals<int>(first._needsToBeSendAsEndedWithScore, second._needsToBeSendAsEndedWithScore))
		{
			return false;
		}
		return true;
		static bool DictionariesEquals<T>(Dictionary<string, List<T>> firstDictionary, Dictionary<string, List<T>> secondDictionary)
		{
			if (firstDictionary.Count != secondDictionary.Count)
			{
				return false;
			}
			foreach (string key in firstDictionary.Keys)
			{
				if (!ListsEquals<T>(firstDictionary[key], secondDictionary[key]))
				{
					return false;
				}
			}
			return true;
		}
		static bool ListsEquals<T>(List<T> firstList, List<T> secondList)
		{
			if (firstList.Count != secondList.Count)
			{
				return false;
			}
			for (int i = 0; i < firstList.Count; i++)
			{
				if (!firstList[i].Equals(secondList[i]))
				{
					return false;
				}
			}
			return true;
		}
	}

	public ActivitiesProgressData Clone()
	{
		ActivitiesProgressData activitiesProgressData = new ActivitiesProgressData();
		foreach (string activeIncludeFilterTag in _activeIncludeFilterTags)
		{
			activitiesProgressData._activeIncludeFilterTags.Add(activeIncludeFilterTag);
		}
		foreach (string item in _available)
		{
			activitiesProgressData._available.Add(item);
		}
		foreach (string item2 in _active)
		{
			activitiesProgressData._active.Add(item2);
		}
		foreach (KeyValuePair<string, List<ActivityResult>> item3 in _ended)
		{
			List<ActivityResult> list = new List<ActivityResult>();
			foreach (ActivityResult item4 in item3.Value)
			{
				list.Add(item4);
			}
			activitiesProgressData._ended.Add(item3.Key, list);
		}
		foreach (KeyValuePair<string, List<int>> item5 in _endedWithScore)
		{
			List<int> list2 = new List<int>();
			foreach (int item6 in item5.Value)
			{
				list2.Add(item6);
			}
			activitiesProgressData._endedWithScore.Add(item5.Key, list2);
		}
		foreach (string item7 in _needsTobeSendAsStarted)
		{
			activitiesProgressData._needsTobeSendAsStarted.Add(item7);
		}
		foreach (KeyValuePair<string, List<ActivityResult>> item8 in _needsToBeSendAsEnded)
		{
			List<ActivityResult> list3 = new List<ActivityResult>();
			foreach (ActivityResult item9 in item8.Value)
			{
				list3.Add(item9);
			}
			activitiesProgressData._needsToBeSendAsEnded.Add(item8.Key, list3);
		}
		foreach (KeyValuePair<string, List<int>> item10 in _needsToBeSendAsEndedWithScore)
		{
			List<int> list4 = new List<int>();
			foreach (int item11 in item10.Value)
			{
				list4.Add(item11);
			}
			activitiesProgressData._needsToBeSendAsEndedWithScore.Add(item10.Key, list4);
		}
		return activitiesProgressData;
	}
}
