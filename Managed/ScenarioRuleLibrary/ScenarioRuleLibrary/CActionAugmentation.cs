using System;
using System.Collections.Generic;
using System.Linq;
using SharedLibrary.Logger;

namespace ScenarioRuleLibrary;

public class CActionAugmentation
{
	private string m_Name;

	private List<ElementInfusionBoardManager.EElement> m_Elements = new List<ElementInfusionBoardManager.EElement>();

	private CAbility m_CostAbility;

	private List<ElementInfusionBoardManager.EElement> m_Infusions = new List<ElementInfusionBoardManager.EElement>();

	private List<CActionAugmentationOp> m_AugmentationOps = new List<CActionAugmentationOp>();

	private int m_XP;

	private AbilityConsume.ConsumeLayoutProperties m_LayoutProperties;

	private List<bool> m_IsAnyElement;

	private string m_PreviewEffectId;

	private string m_PreviewEffectText;

	private string m_ConsumeGroup;

	public string Name => m_Name;

	public List<ElementInfusionBoardManager.EElement> Elements => m_Elements;

	public CAbility CostAbility => m_CostAbility;

	public List<ElementInfusionBoardManager.EElement> Infusions => m_Infusions;

	public List<CActionAugmentationOp> AugmentationOps => m_AugmentationOps;

	public int XP => m_XP;

	public AbilityConsume.ConsumeLayoutProperties LayoutProperties => m_LayoutProperties;

	public Guid ActionID { get; set; }

	public string PreviewEffectId => m_PreviewEffectId;

	public string PreviewEffectText => m_PreviewEffectText;

	public string ConsumeGroup => m_ConsumeGroup;

	public CActionAugmentation()
	{
	}

	public CActionAugmentation(string name, List<ElementInfusionBoardManager.EElement> elements, CAbility costAbility, List<ElementInfusionBoardManager.EElement> infusions, List<CActionAugmentationOp> augmentationOps, int xp, AbilityConsume.ConsumeLayoutProperties layoutProperties, string previewEffectId, string previewEffectText, string consumeGroup)
	{
		m_Name = name;
		m_Elements = elements;
		m_CostAbility = costAbility;
		m_Infusions = infusions;
		m_AugmentationOps = augmentationOps;
		m_XP = xp;
		m_LayoutProperties = layoutProperties;
		m_PreviewEffectId = previewEffectId;
		m_PreviewEffectText = previewEffectText;
		m_ConsumeGroup = consumeGroup;
		m_IsAnyElement = new List<bool>();
		foreach (ElementInfusionBoardManager.EElement element in elements)
		{
			m_IsAnyElement.Add(element == ElementInfusionBoardManager.EElement.Any);
		}
	}

	public CActionAugmentation Copy()
	{
		return new CActionAugmentation
		{
			ActionID = ActionID,
			m_Name = m_Name,
			m_Elements = m_Elements.ToList(),
			m_CostAbility = CAbility.CopyAbility(m_CostAbility, generateNewID: false),
			m_Infusions = m_Infusions.ToList(),
			m_AugmentationOps = m_AugmentationOps.Select((CActionAugmentationOp s) => s.Copy()).ToList(),
			m_XP = m_XP,
			m_LayoutProperties = m_LayoutProperties.Copy(),
			m_IsAnyElement = m_IsAnyElement.ToList(),
			m_PreviewEffectId = m_PreviewEffectId,
			m_PreviewEffectText = m_PreviewEffectText,
			m_ConsumeGroup = m_ConsumeGroup
		};
	}

	public void ResetElements()
	{
		for (int i = 0; i < Elements.Count; i++)
		{
			if (m_IsAnyElement[i])
			{
				Elements[i] = ElementInfusionBoardManager.EElement.Any;
			}
		}
	}

	public void ConsumeElements(CActor consumingActor)
	{
		if (consumingActor.Class is CMonsterClass cMonsterClass)
		{
			string monsterClass = ((cMonsterClass.NonEliteVariant != null) ? cMonsterClass.NonEliteVariant.ID : cMonsterClass.ID);
			foreach (ElementInfusionBoardManager.EElement item in Elements.Where((ElementInfusionBoardManager.EElement x) => x != ElementInfusionBoardManager.EElement.Any))
			{
				DLLDebug.LogInfo("Element " + item.ToString() + " has been consumed");
				ElementInfusionBoardManager.EnemyConsume(item, monsterClass, consumingActor);
			}
			{
				foreach (ElementInfusionBoardManager.EElement item2 in Elements.Where((ElementInfusionBoardManager.EElement x) => x == ElementInfusionBoardManager.EElement.Any))
				{
					_ = item2;
					List<ElementInfusionBoardManager.EElement> enemyConsumedElements = ElementInfusionBoardManager.GetEnemyConsumedElements(consumingActor.ActorGuid);
					if (enemyConsumedElements != null && enemyConsumedElements.Count > 0)
					{
						ElementInfusionBoardManager.EnemyConsume(enemyConsumedElements[0], monsterClass, consumingActor);
						continue;
					}
					List<ElementInfusionBoardManager.EElement> availableElements = ElementInfusionBoardManager.GetAvailableElements();
					if (availableElements.Count > 0)
					{
						ElementInfusionBoardManager.EnemyConsume(availableElements[ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(availableElements.Count)], monsterClass, consumingActor);
					}
				}
				return;
			}
		}
		foreach (ElementInfusionBoardManager.EElement item3 in Elements.Where((ElementInfusionBoardManager.EElement x) => x != ElementInfusionBoardManager.EElement.Any))
		{
			DLLDebug.LogInfo("Element " + item3.ToString() + " has been consumed");
			ElementInfusionBoardManager.Consume(item3, consumingActor);
		}
		foreach (ElementInfusionBoardManager.EElement item4 in Elements.Where((ElementInfusionBoardManager.EElement x) => x == ElementInfusionBoardManager.EElement.Any))
		{
			_ = item4;
			List<ElementInfusionBoardManager.EElement> availableElements2 = ElementInfusionBoardManager.GetAvailableElements();
			if (availableElements2.Count > 0)
			{
				ElementInfusionBoardManager.Consume(availableElements2[ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(availableElements2.Count)], consumingActor);
			}
		}
	}
}
