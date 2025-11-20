using System;
using System.Collections.Generic;
using System.Linq;
using SharedLibrary.SimpleLog;

namespace ScenarioRuleLibrary;

public class ElementInfusionBoardManager
{
	[Serializable]
	public enum EElement
	{
		Fire,
		Ice,
		Air,
		Earth,
		Light,
		Dark,
		Any
	}

	[Serializable]
	public enum EColumn
	{
		Inert,
		Waning,
		Strong
	}

	public static EElement[] Elements = (EElement[])Enum.GetValues(typeof(EElement));

	private static EColumn[] s_ElementColumn = new EColumn[Enum.GetNames(typeof(EElement)).Length];

	private static EColumn[] s_ElementColumnSaved = new EColumn[Enum.GetNames(typeof(EElement)).Length];

	private static Dictionary<string, List<EElement>> s_EnemyConsumedElements = new Dictionary<string, List<EElement>>();

	private static Dictionary<string, List<EElement>> s_ElementsToInfuse = new Dictionary<string, List<EElement>>();

	private static Dictionary<string, List<EElement>> s_ElementsToInfuseSaved = new Dictionary<string, List<EElement>>();

	public static EColumn[] GetElementColumn => s_ElementColumn;

	public static void Reset()
	{
		EElement[] elements = Elements;
		for (int i = 0; i < elements.Length; i++)
		{
			Consume(elements[i], null);
		}
		s_ElementColumn = new EColumn[Enum.GetNames(typeof(EElement)).Length];
		s_ElementColumnSaved = new EColumn[Enum.GetNames(typeof(EElement)).Length];
		s_EnemyConsumedElements.Clear();
		s_ElementsToInfuse.Clear();
		s_ElementsToInfuseSaved.Clear();
	}

	public static void SaveState()
	{
		s_ElementColumnSaved = new EColumn[Enum.GetNames(typeof(EElement)).Length];
		s_ElementColumn.CopyTo(s_ElementColumnSaved, 0);
		s_ElementsToInfuseSaved = new Dictionary<string, List<EElement>>(s_ElementsToInfuse);
	}

	public static void RestoreState()
	{
		s_ElementColumn = new EColumn[Enum.GetNames(typeof(EElement)).Length];
		s_ElementColumnSaved.CopyTo(s_ElementColumn, 0);
		s_ElementsToInfuse = new Dictionary<string, List<EElement>>(s_ElementsToInfuseSaved);
	}

	public static void SetElementColumn(EColumn[] elementColumn)
	{
		s_ElementColumn = elementColumn;
	}

	public static List<EElement> GetAvailableElements(CActor actor = null)
	{
		List<EElement> list = new List<EElement>();
		List<EElement> list2 = ((!(PhaseManager.CurrentPhase is CPhaseAction cPhaseAction) || (actor != null && actor.Class is CMonsterClass)) ? new List<EElement>() : cPhaseAction.GetReservedElements());
		EElement[] elements = Elements;
		foreach (EElement eElement in elements)
		{
			if (eElement != EElement.Any && ElementColumn(eElement) != EColumn.Inert && !list2.Contains(eElement))
			{
				list.Add(eElement);
			}
		}
		return list;
	}

	public static EColumn ElementColumn(EElement element)
	{
		return s_ElementColumn[(int)element];
	}

	public static void EndTurn(CActor.EType actortype = CActor.EType.Unknown)
	{
		SimpleLog.AddToSimpleLog("[ELEMENTS] Ending turn - infusing pending elements");
		if (s_ElementsToInfuse != null)
		{
			foreach (KeyValuePair<string, List<EElement>> item in s_ElementsToInfuse)
			{
				if (item.Value == null)
				{
					continue;
				}
				foreach (EElement item2 in item.Value)
				{
					if (item2 != EElement.Any)
					{
						s_ElementColumn[(int)item2] = EColumn.Strong;
						SEventLogMessageHandler.AddEventLogMessage(new SEventElement(ESESubTypeElement.Infused, item2, item.Key, actortype, IsSummon: false, null, null));
					}
				}
			}
		}
		s_ElementsToInfuse.Clear();
		PrintElementColumns();
	}

	public static void EndRound(CActor.EType lastActorType)
	{
		EndTurn(lastActorType);
		SimpleLog.AddToSimpleLog("[ELEMENTS] Ending round - moving element columns down 1");
		for (int i = 0; i < s_ElementColumn.Length; i++)
		{
			if (s_ElementColumn[i] > EColumn.Inert)
			{
				s_ElementColumn[i]--;
			}
		}
		s_EnemyConsumedElements.Clear();
		PrintElementColumns();
	}

	public static void TellClientInfusing(EElement element, CActor actorInfusing)
	{
		CActorIsInfusing_MessageData message = new CActorIsInfusing_MessageData(actorInfusing)
		{
			m_Element = element
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	public static void Infuse(EElement element, CActor actorInfusing)
	{
		string key = ((actorInfusing != null) ? actorInfusing.Class.ID : "");
		if (element == EElement.Any && actorInfusing.IsMonsterType)
		{
			EElement[] array = Elements.Where((EElement x) => ElementColumn(x) != EColumn.Strong && x != EElement.Any).ToArray();
			if (array.Count() > 0)
			{
				int num = ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(0, array.Count());
				EElement eElement = array[num];
				if (s_ElementsToInfuse.TryGetValue(key, out var value))
				{
					value.Add(eElement);
				}
				else
				{
					value = new List<EElement>();
					value.Add(eElement);
					s_ElementsToInfuse.Add(key, value);
				}
				TellClientInfusing(eElement, actorInfusing);
			}
		}
		else
		{
			if (s_ElementsToInfuse.TryGetValue(key, out var value2))
			{
				value2.Add(element);
			}
			else
			{
				value2 = new List<EElement>();
				value2.Add(element);
				s_ElementsToInfuse.Add(key, value2);
			}
			TellClientInfusing(element, actorInfusing);
		}
	}

	public static void Infuse(List<EElement> elements, CActor actorInfusing)
	{
		elements.ForEach(delegate(EElement element)
		{
			Infuse(element, actorInfusing);
		});
	}

	public static void UndoInfuse(List<EElement> elements, CActor actorInfusing)
	{
		string key = ((actorInfusing != null) ? actorInfusing.Class.ID : "");
		if (s_ElementsToInfuse.TryGetValue(key, out var elementsToInfuse))
		{
			elements.ForEach(delegate(EElement element)
			{
				elementsToInfuse.Remove(element);
			});
		}
	}

	public static void Consume(EElement element, CActor actorConsuming)
	{
		SimpleLog.AddToSimpleLog("[ELEMENTS] Consuming " + element);
		s_ElementColumn[(int)element] = EColumn.Inert;
		if (actorConsuming == null)
		{
			return;
		}
		bool isSummon = false;
		if (actorConsuming.Type == CActor.EType.Enemy)
		{
			CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == actorConsuming.ActorGuid);
			if (cEnemyActor != null)
			{
				isSummon = cEnemyActor.IsSummon;
			}
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventElement(ESESubTypeElement.Consumed, element, actorConsuming.Class.ID, actorConsuming.Type, isSummon, actorConsuming.Tokens.CheckPositiveTokens, actorConsuming.Tokens.CheckNegativeTokens));
	}

	public static void EnemyConsume(EElement element, string monsterClass, CActor actor)
	{
		SimpleLog.AddToSimpleLog("[ELEMENTS] Enemy consuming " + element);
		s_ElementColumn[(int)element] = EColumn.Inert;
		foreach (CEnemyActor item in ScenarioManager.Scenario.AllMonsters.Where((CEnemyActor x) => x.MonsterClass.ID.Replace("Elite", "") == monsterClass))
		{
			List<EElement> value = null;
			if (s_EnemyConsumedElements.TryGetValue(item.ActorGuid, out value))
			{
				s_EnemyConsumedElements[item.ActorGuid].Add(element);
				continue;
			}
			value = new List<EElement>();
			value.Add(element);
			s_EnemyConsumedElements.Add(item.ActorGuid, value);
		}
		if (monsterClass != null)
		{
			bool isSummon = false;
			if (actor.Type == CActor.EType.Enemy)
			{
				CEnemyActor cEnemyActor = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == actor.ActorGuid);
				if (cEnemyActor != null)
				{
					isSummon = cEnemyActor.IsSummon;
				}
			}
			SEventLogMessageHandler.AddEventLogMessage(new SEventElement(ESESubTypeElement.Consumed, element, monsterClass, actor.Type, isSummon, actor.Tokens.CheckPositiveTokens, actor.Tokens.CheckNegativeTokens));
		}
		PrintElementColumns();
	}

	public static List<EElement> GetEnemyConsumedElements(string monsterActorGuid)
	{
		List<EElement> value = new List<EElement>();
		s_EnemyConsumedElements.TryGetValue(monsterActorGuid, out value);
		return value;
	}

	public static void SetElementInstantly(EElement element, EColumn column)
	{
		SimpleLog.AddToSimpleLog("[ELEMENTS] Setting " + element.ToString() + " to " + column.ToString() + " instantly");
		s_ElementColumn[(int)element] = column;
		PrintElementColumns();
	}

	public static void PrintElementColumns()
	{
		string text = "[ELEMENTS] Element Board State:";
		EElement[] elements = Elements;
		for (int i = 0; i < elements.Length; i++)
		{
			EElement eElement = elements[i];
			text = text + "\n" + eElement.ToString() + " is " + s_ElementColumn[(int)eElement];
		}
		SimpleLog.AddToSimpleLog(text);
	}
}
