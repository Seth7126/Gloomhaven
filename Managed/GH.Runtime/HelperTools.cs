#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Text;
using ScenarioRuleLibrary;
using SharedLibrary.Logger;
using SpriteMemoryManagement;
using UnityEngine;

public static class HelperTools
{
	public static void NormalizePool<T>(ref List<T> pool, GameObject prefab, Transform parent, int newLength, Action<T> onCreateAction = null, Action<T> onHideAction = null) where T : Component
	{
		if (pool == null)
		{
			pool = new List<T>();
		}
		int count = pool.Count;
		for (int i = 0; i < Math.Min(newLength, count); i++)
		{
			pool[i].gameObject.SetActive(value: true);
		}
		for (int j = 0; j < newLength - count; j++)
		{
			T component = UnityEngine.Object.Instantiate(prefab).GetComponent<T>();
			onCreateAction?.Invoke(component);
			component.name = prefab.name;
			component.transform.SetParent(parent, worldPositionStays: false);
			component.gameObject.SetActive(value: true);
			pool.Add(component);
		}
		count = pool.Count;
		for (int k = 0; k < count - newLength; k++)
		{
			onHideAction?.Invoke(pool[k + newLength]);
			pool[k + newLength].gameObject.SetActive(value: false);
		}
	}

	public static void NormalizePool(ref List<GameObject> pool, GameObject prefab, Transform parent, int newLength)
	{
		if (pool == null)
		{
			pool = new List<GameObject>();
		}
		foreach (GameObject item in pool)
		{
			item.SetActive(value: true);
		}
		int count = pool.Count;
		for (int i = 0; i < newLength - count; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
			gameObject.transform.SetParent(parent, worldPositionStays: false);
			pool.Add(gameObject);
		}
		count = pool.Count;
		for (int j = 0; j < count - newLength; j++)
		{
			pool[j + newLength].SetActive(value: false);
		}
	}

	public static float HorizontalGroupPositionOffset(float orderNumber, float totalNumber, float elementWidth, float spacing)
	{
		return (orderNumber - (totalNumber - 1f) / 2f) * (elementWidth * totalNumber + spacing * (totalNumber - 1f)) / totalNumber;
	}

	public static Sprite GetSprite(this Sprite[] sprites, string name, bool toLower = true, bool useDefaultWhenMissing = true)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		foreach (Sprite sprite in sprites)
		{
			if (sprite == null)
			{
				continue;
			}
			if (toLower)
			{
				if (sprite.name.Replace(" ", "").Replace("_", "").ToLower()
					.Contains(name.Replace(" ", "").Replace("_", "").ToLower()))
				{
					return sprite;
				}
			}
			else if (sprite.name.Replace(" ", "").Replace("_", "").Contains(name.Replace(" ", "").Replace("_", "")))
			{
				return sprite;
			}
		}
		if (useDefaultWhenMissing)
		{
			Debug.LogWarning("Can't find '" + name + "' sprite");
		}
		if (!useDefaultWhenMissing || sprites.Length == 0)
		{
			return null;
		}
		return sprites[0];
	}

	public static ReferenceToSprite GetReferenceToSprite(this ReferenceToSprite[] sprites, string name, bool toLower = true, bool useDefaultWhenMissing = true)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		foreach (ReferenceToSprite referenceToSprite in sprites)
		{
			if (referenceToSprite == null)
			{
				continue;
			}
			if (toLower)
			{
				if (referenceToSprite.SpriteName.Replace(" ", "").Replace("_", "").ToLower()
					.EndsWith(name.Replace(" ", "").Replace("_", "").ToLower()))
				{
					return referenceToSprite;
				}
			}
			else if (referenceToSprite.SpriteName.Replace(" ", "").Replace("_", "").EndsWith(name.Replace(" ", "").Replace("_", "")))
			{
				return referenceToSprite;
			}
		}
		if (useDefaultWhenMissing)
		{
			Debug.LogWarning("Can't find '" + name + "' sprite");
		}
		if (!useDefaultWhenMissing || sprites.Length == 0)
		{
			return null;
		}
		return sprites[0];
	}

	public static T GetObject<T>(this T[] objects, string name) where T : UnityEngine.Object
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		foreach (T val in objects)
		{
			if (!(val == null) && val.name.Replace(" ", "").Replace("_", "").ToLower()
				.Contains(name.Replace(" ", "").Replace("_", "").ToLower()))
			{
				return val;
			}
		}
		Debug.LogError("Can't find '" + name + "' object");
		if (objects.Length == 0)
		{
			return null;
		}
		return objects[0];
	}

	public static string GetObjectName<T>(this T[] objects, string name) where T : UnityEngine.Object
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		foreach (T val in objects)
		{
			if (!(val == null) && val.name.Replace(" ", "").Replace("_", "").ToLower()
				.Contains(name.Replace(" ", "").Replace("_", "").ToLower()))
			{
				return val.name;
			}
		}
		Debug.LogError("Can't find '" + name + "' object");
		return null;
	}

	public static bool IsDefaultAction(this CBaseCard.ActionType actionType)
	{
		if (actionType != CBaseCard.ActionType.DefaultAttackAction)
		{
			return actionType == CBaseCard.ActionType.DefaultMoveAction;
		}
		return true;
	}

	public static CBaseCard.ActionType GetOppositeAction(this CBaseCard.ActionType actionType)
	{
		return actionType switch
		{
			CBaseCard.ActionType.DefaultMoveAction => CBaseCard.ActionType.DefaultAttackAction, 
			CBaseCard.ActionType.BottomAction => CBaseCard.ActionType.TopAction, 
			CBaseCard.ActionType.TopAction => CBaseCard.ActionType.BottomAction, 
			_ => CBaseCard.ActionType.DefaultMoveAction, 
		};
	}

	public static string ToHtmlColor(this CActiveBonus activeBonus)
	{
		string text = BitConverter.ToString(Encoding.Default.GetBytes(activeBonus.BaseCard.Name)).Replace("-", "");
		if (text.Length > 6)
		{
			text = text.Substring(0, 6);
		}
		else if (text.Length < 6)
		{
			for (int i = text.Length; i < 6; i++)
			{
				text += "0";
			}
		}
		return text;
	}

	public static int ToSortedValue(this CardPileType cardPileType)
	{
		return cardPileType switch
		{
			CardPileType.Active => 0, 
			CardPileType.Hand => 2, 
			CardPileType.Round => 1, 
			CardPileType.Discarded => 3, 
			CardPileType.Lost => 4, 
			CardPileType.Permalost => 5, 
			_ => 5, 
		};
	}

	public static CBaseCard.ECardPile GetRecoverECardPile(this CardHandMode cardHandMode)
	{
		switch (cardHandMode)
		{
		case CardHandMode.RecoverDiscardedCard:
			return CBaseCard.ECardPile.Discarded;
		case CardHandMode.RecoverLostCard:
			return CBaseCard.ECardPile.Lost;
		default:
			DLLDebug.LogWarning($"Probably, an invalid recover card pile from cardHandMode:{cardHandMode.ToString()}! Please, review this!");
			return CBaseCard.ECardPile.None;
		}
	}

	public static string GetCorrectSprite(CAbility.EAbilityType abilityType)
	{
		string empty = string.Empty;
		switch (abilityType)
		{
		case CAbility.EAbilityType.AddHeal:
		case CAbility.EAbilityType.Overheal:
			return CAbility.EAbilityType.Heal.ToString();
		case CAbility.EAbilityType.AddRange:
			return "Range";
		case CAbility.EAbilityType.AttackersGainDisadvantage:
			return "Attack";
		case CAbility.EAbilityType.Kill:
			return "AA_Kill";
		case CAbility.EAbilityType.Advantage:
			return "AA_Advantage";
		default:
			return abilityType.ToString();
		}
	}

	public static void TrimAll(this List<string> stringList)
	{
		for (int i = 0; i < stringList.Count; i++)
		{
			stringList[i] = stringList[i].Trim();
		}
	}
}
