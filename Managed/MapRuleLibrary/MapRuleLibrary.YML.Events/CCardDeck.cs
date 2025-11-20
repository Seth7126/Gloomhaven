using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using MapRuleLibrary.Adventure;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;
using StateCodeGenerator;

namespace MapRuleLibrary.YML.Events;

[Serializable]
public class CCardDeck : ISerializable
{
	public enum EShuffle
	{
		None,
		All,
		DeckOnly
	}

	public enum EAddCard
	{
		None,
		Any,
		Bottom,
		Top
	}

	public enum EDiscard
	{
		None,
		Discard,
		Bottom,
		Destroy
	}

	private List<string> m_InitialCards = new List<string>();

	private List<string> m_Cards = new List<string>();

	private List<string> m_DiscardedCards = new List<string>();

	private EShuffle m_NeedsShuffle;

	public List<string> Cards => m_Cards;

	public List<string> DiscardedCards => m_DiscardedCards;

	public List<string> InitialCards => m_InitialCards;

	public CCardDeck()
	{
	}

	public CCardDeck(CCardDeck state, ReferenceDictionary references)
	{
		m_InitialCards = references.Get(state.m_InitialCards);
		if (m_InitialCards == null && state.m_InitialCards != null)
		{
			m_InitialCards = new List<string>();
			for (int i = 0; i < state.m_InitialCards.Count; i++)
			{
				string item = state.m_InitialCards[i];
				m_InitialCards.Add(item);
			}
			references.Add(state.m_InitialCards, m_InitialCards);
		}
		m_Cards = references.Get(state.m_Cards);
		if (m_Cards == null && state.m_Cards != null)
		{
			m_Cards = new List<string>();
			for (int j = 0; j < state.m_Cards.Count; j++)
			{
				string item2 = state.m_Cards[j];
				m_Cards.Add(item2);
			}
			references.Add(state.m_Cards, m_Cards);
		}
		m_DiscardedCards = references.Get(state.m_DiscardedCards);
		if (m_DiscardedCards == null && state.m_DiscardedCards != null)
		{
			m_DiscardedCards = new List<string>();
			for (int k = 0; k < state.m_DiscardedCards.Count; k++)
			{
				string item3 = state.m_DiscardedCards[k];
				m_DiscardedCards.Add(item3);
			}
			references.Add(state.m_DiscardedCards, m_DiscardedCards);
		}
		m_NeedsShuffle = state.m_NeedsShuffle;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("m_InitialCards", m_InitialCards);
		info.AddValue("m_Cards", m_Cards);
		info.AddValue("m_DiscardedCards", m_DiscardedCards);
		info.AddValue("m_NeedsShuffle", m_NeedsShuffle);
	}

	public CCardDeck(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "m_InitialCards":
					m_InitialCards = (List<string>)info.GetValue("m_InitialCards", typeof(List<string>));
					break;
				case "m_Cards":
					m_Cards = (List<string>)info.GetValue("m_Cards", typeof(List<string>));
					break;
				case "m_DiscardedCards":
					m_DiscardedCards = (List<string>)info.GetValue("m_DiscardedCards", typeof(List<string>));
					break;
				case "m_NeedsShuffle":
					m_NeedsShuffle = (EShuffle)info.GetValue("m_NeedsShuffle", typeof(EShuffle));
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CCardDeck entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		if (m_InitialCards == null)
		{
			m_InitialCards = new List<string>();
		}
		if (m_Cards == null)
		{
			m_Cards = new List<string>();
		}
		if (m_DiscardedCards == null)
		{
			m_DiscardedCards = new List<string>();
		}
	}

	public CCardDeck(List<string> cards)
	{
		m_Cards = ((cards == null) ? new List<string>() : cards);
		m_InitialCards = ((cards == null) ? new List<string>() : cards);
		m_DiscardedCards = new List<string>();
		m_NeedsShuffle = EShuffle.None;
	}

	public void Reset(List<string> cards = null)
	{
		m_Cards.Clear();
		m_DiscardedCards.Clear();
		m_InitialCards = ((cards == null) ? m_InitialCards : cards);
		m_Cards = m_InitialCards;
	}

	public int CardCount()
	{
		return m_Cards.Count();
	}

	public void Shuffle(EShuffle shuffle = EShuffle.All)
	{
		if (shuffle == EShuffle.None)
		{
			return;
		}
		List<string> list = new List<string>();
		switch (shuffle)
		{
		case EShuffle.All:
			list.AddRange(m_Cards);
			list.AddRange(m_DiscardedCards);
			m_Cards.Clear();
			m_DiscardedCards.Clear();
			break;
		case EShuffle.DeckOnly:
			list.AddRange(m_Cards);
			m_Cards.Clear();
			break;
		}
		if (AdventureState.MapState != null)
		{
			m_NeedsShuffle = EShuffle.None;
			for (int i = 0; i < list.Count(); i++)
			{
				int num = AdventureState.MapState.MapRNG.Next(0, i + 1);
				if (num == i)
				{
					m_Cards.Add(list[i]);
					continue;
				}
				m_Cards.Add(m_Cards[num]);
				m_Cards[num] = list[i];
			}
		}
		else
		{
			DLLDebug.LogError("Attempted to shuffle event deck when AdventureState was not set");
		}
		if (AdventureState.MapState != null)
		{
			SimpleLog.AddToSimpleLog("Event Deck (Shuffled Event Deck): " + AdventureState.MapState.PeekMapRNG + "\n" + string.Join(", ", m_Cards) + "\n" + Environment.StackTrace);
		}
	}

	public void ExpandDeck(List<string> cards)
	{
		m_Cards.AddRange(cards);
	}

	public void AddCard(string card, EAddCard addCard = EAddCard.Any, bool allowDups = true)
	{
		if (!allowDups && (allowDups || m_Cards.Contains(card)))
		{
			return;
		}
		switch (addCard)
		{
		case EAddCard.Any:
			if (AdventureState.MapState != null)
			{
				int index = AdventureState.MapState.MapRNG.Next(0, m_Cards.Count);
				m_Cards.Insert(index, card);
				SimpleLog.AddToSimpleLog("Event Deck  (add card): " + AdventureState.MapState.PeekMapRNG + "\n" + string.Join(", ", m_Cards));
			}
			else
			{
				m_Cards.Add(card);
			}
			break;
		case EAddCard.Top:
			m_Cards.Insert(0, card);
			break;
		default:
			m_Cards.Add(card);
			break;
		}
		if (!allowDups && m_DiscardedCards.Contains(card))
		{
			m_DiscardedCards.Remove(card);
		}
	}

	public void RemoveCard(string card, EDiscard discard = EDiscard.Discard)
	{
		SimpleLog.AddToSimpleLog("Event Deck (RemoveCard): \n" + string.Join(", ", m_Cards));
		if (m_Cards.FirstOrDefault((string x) => x == card) != null)
		{
			m_Cards.RemoveAll((string x) => x == card);
		}
		if (discard == EDiscard.Discard)
		{
			m_DiscardedCards.Add(card);
		}
	}

	public string DrawCard(EShuffle shuffle = EShuffle.All, EDiscard discard = EDiscard.Discard)
	{
		if (m_NeedsShuffle != EShuffle.None)
		{
			Shuffle(m_NeedsShuffle);
		}
		string text = null;
		if (m_Cards.Count == 0)
		{
			Shuffle(shuffle);
		}
		if (m_Cards.Count > 0)
		{
			text = m_Cards[0];
			m_Cards.Remove(text);
			switch (discard)
			{
			case EDiscard.Bottom:
				AddCard(text, EAddCard.Bottom);
				break;
			case EDiscard.Discard:
				m_DiscardedCards.Add(text);
				break;
			}
		}
		return text;
	}

	public string GetEventCardsOnTop(int cardAmount)
	{
		if (m_Cards.Count < cardAmount)
		{
			cardAmount = m_Cards.Count;
		}
		string text = "";
		for (int i = 0; i < cardAmount; i++)
		{
			text = text + "\nCard " + i + ": " + m_Cards[i];
		}
		return text;
	}
}
