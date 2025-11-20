using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using SM.Utils;
using ScenarioRuleLibrary;

[Serializable]
public class ClientIndependantValues : ISerializable
{
	[Serializable]
	public class CIVKeyValuePair
	{
		public string Key { get; private set; }

		public bool Value { get; private set; }

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Key", Key);
			info.AddValue("Value", Value);
		}

		public CIVKeyValuePair(SerializationInfo info, StreamingContext context)
		{
			SerializationInfoEnumerator enumerator = info.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SerializationEntry current = enumerator.Current;
				try
				{
					string name = current.Name;
					if (!(name == "Key"))
					{
						if (name == "Value")
						{
							Value = info.GetBoolean("Value");
						}
					}
					else
					{
						Key = info.GetString("Key");
					}
				}
				catch (Exception ex)
				{
					LogUtils.LogError("Exception while trying to deserialize CIVKeyValuePair entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
					throw ex;
				}
			}
		}

		public CIVKeyValuePair(string key, bool value)
		{
			Key = key;
			Value = value;
		}
	}

	public List<CIVKeyValuePair> CIVItemIsNewDictionary { get; private set; }

	public List<CIVKeyValuePair> CIVAchievementIsNewDictionary { get; private set; }

	public List<CIVKeyValuePair> CIVQuestIsNewDictionary { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("CIVItemIsNewDictionary", CIVItemIsNewDictionary);
		info.AddValue("CIVAchievementIsNewDictionary", CIVAchievementIsNewDictionary);
		info.AddValue("CIVQuestIsNewDictionary", CIVQuestIsNewDictionary);
	}

	public ClientIndependantValues(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "CIVItemIsNewDictionary":
					CIVItemIsNewDictionary = (List<CIVKeyValuePair>)info.GetValue("CIVItemIsNewDictionary", typeof(List<CIVKeyValuePair>));
					break;
				case "CIVAchievementIsNewDictionary":
					CIVAchievementIsNewDictionary = (List<CIVKeyValuePair>)info.GetValue("CIVAchievementIsNewDictionary", typeof(List<CIVKeyValuePair>));
					break;
				case "CIVQuestIsNewDictionary":
					CIVQuestIsNewDictionary = (List<CIVKeyValuePair>)info.GetValue("CIVQuestIsNewDictionary", typeof(List<CIVKeyValuePair>));
					break;
				}
			}
			catch (Exception ex)
			{
				LogUtils.LogError("Exception while trying to deserialize ClientIndependantValues entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (CIVItemIsNewDictionary == null)
		{
			CIVItemIsNewDictionary = new List<CIVKeyValuePair>();
		}
		if (CIVAchievementIsNewDictionary == null)
		{
			CIVAchievementIsNewDictionary = new List<CIVKeyValuePair>();
		}
		if (CIVQuestIsNewDictionary == null)
		{
			CIVQuestIsNewDictionary = new List<CIVKeyValuePair>();
		}
	}

	public ClientIndependantValues()
	{
		CIVItemIsNewDictionary = new List<CIVKeyValuePair>();
		CIVAchievementIsNewDictionary = new List<CIVKeyValuePair>();
		CIVQuestIsNewDictionary = new List<CIVKeyValuePair>();
	}

	public void UpdateItemIsNewDictionary(List<CItem> items)
	{
		try
		{
			CIVItemIsNewDictionary.Clear();
			foreach (CItem item in items)
			{
				if (item != null)
				{
					if (CIVItemIsNewDictionary.Exists((CIVKeyValuePair e) => e.Key == item.ItemGuid))
					{
						Debug.LogError("Duplicate item in UpdateItemIsNewDictionary.  Item: " + item.Name + "\n" + Environment.StackTrace);
					}
					else
					{
						CIVItemIsNewDictionary.Add(new CIVKeyValuePair(item.ItemGuid, item.IsNew));
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception at Update Item Is New Dictionary " + ex.Message + ex.StackTrace);
		}
	}

	public void UpdateAchievementIsNewDictionary(List<CPartyAchievement> partyAchievements)
	{
		CIVAchievementIsNewDictionary.Clear();
		foreach (CPartyAchievement partyAchievement in partyAchievements)
		{
			CIVAchievementIsNewDictionary.Add(new CIVKeyValuePair(partyAchievement.ID, partyAchievement.IsNew));
		}
	}

	public void UpdateQuestIsNewDictionary(List<CQuestState> questStates)
	{
		CIVQuestIsNewDictionary.Clear();
		foreach (CQuestState questState in questStates)
		{
			CIVQuestIsNewDictionary.Add(new CIVKeyValuePair(questState.ID, questState.IsNew));
		}
	}
}
