using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Events;

public class InfusionBoardUI : MonoBehaviour
{
	[Serializable]
	private struct UIInfusionElementConfig
	{
		public ElementInfusionBoardManager.EElement element;

		public Sprite creationIcon;

		public Sprite strongIcon;

		public Sprite waningIcon;

		public Sprite textHighlightBackground;

		public Color textColor;
	}

	[Serializable]
	public class InfusionEvent : UnityEvent<List<ElementInfusionBoardManager.EElement>>
	{
	}

	[SerializeField]
	private InfusionElementUI elementPrefab;

	[SerializeField]
	private Transform elementsHolder;

	[SerializeField]
	private UIInfusionElementConfig[] elementConfigs;

	public InfusionEvent OnReservedElement = new InfusionEvent();

	public InfusionEvent OnUnreserveElement = new InfusionEvent();

	private Dictionary<ElementInfusionBoardManager.EElement, InfusionElementUI> elementsUI = new Dictionary<ElementInfusionBoardManager.EElement, InfusionElementUI>();

	private List<ElementInfusionBoardManager.EElement> elementsInCreation = new List<ElementInfusionBoardManager.EElement>();

	private List<ElementInfusionBoardManager.EElement> elementsReserved = new List<ElementInfusionBoardManager.EElement>();

	private List<ElementInfusionBoardManager.EElement> elementsInCreationSaved = new List<ElementInfusionBoardManager.EElement>();

	private List<ElementInfusionBoardManager.EElement> elementsReservedSaved = new List<ElementInfusionBoardManager.EElement>();

	private bool stateSaved;

	public static InfusionBoardUI Instance { get; private set; }

	[UsedImplicitly]
	private void Awake()
	{
		Instance = this;
		stateSaved = false;
		for (int i = 0; i < 6; i++)
		{
			ElementInfusionBoardManager.EElement element = (ElementInfusionBoardManager.EElement)i;
			elementsUI[element] = UnityEngine.Object.Instantiate(elementPrefab, elementsHolder);
			UIInfusionElementConfig uIInfusionElementConfig = elementConfigs.First((UIInfusionElementConfig it) => it.element == element);
			elementsUI[element].Init(element, uIInfusionElementConfig.strongIcon, uIInfusionElementConfig.creationIcon, uIInfusionElementConfig.waningIcon, uIInfusionElementConfig.textHighlightBackground, uIInfusionElementConfig.textColor);
			elementsUI[element].gameObject.SetActive(value: false);
		}
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		Instance = null;
	}

	public void UnreserveElements()
	{
		elementsReserved.Clear();
		stateSaved = false;
		UpdateBoard();
	}

	public void ReserveElement(ElementInfusionBoardManager.EElement element, bool active)
	{
		if (active)
		{
			if (!elementsReserved.Contains(element))
			{
				elementsReserved.Add(element);
				UpdateBoard();
				OnReservedElement.Invoke(new List<ElementInfusionBoardManager.EElement> { element });
			}
		}
		else if (elementsReserved.Contains(element))
		{
			elementsReserved.Remove(element);
			UpdateBoard();
			OnUnreserveElement.Invoke(new List<ElementInfusionBoardManager.EElement> { element });
		}
	}

	public void ReserveElements(List<ElementInfusionBoardManager.EElement> elements, bool active)
	{
		List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
		foreach (ElementInfusionBoardManager.EElement element in elements)
		{
			if (active)
			{
				if (!elementsReserved.Contains(element))
				{
					elementsReserved.Add(element);
					list.Add(element);
				}
			}
			else if (elementsReserved.Contains(element))
			{
				elementsReserved.Remove(element);
				list.Add(element);
			}
		}
		if (list.Count > 0)
		{
			UpdateBoard();
			if (active)
			{
				OnReservedElement?.Invoke(list);
			}
			else
			{
				OnUnreserveElement?.Invoke(list);
			}
		}
	}

	public bool IsElementReserved(ElementInfusionBoardManager.EElement element)
	{
		if (element == ElementInfusionBoardManager.EElement.Any)
		{
			for (int i = 0; i < 6; i++)
			{
				if (ElementInfusionBoardManager.ElementColumn((ElementInfusionBoardManager.EElement)i) != ElementInfusionBoardManager.EColumn.Inert && !IsElementReserved((ElementInfusionBoardManager.EElement)i))
				{
					return false;
				}
			}
			return true;
		}
		return elementsReserved.Contains(element);
	}

	public List<ElementInfusionBoardManager.EElement> GetAvailableElements()
	{
		List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
		for (int i = 0; i < 6; i++)
		{
			ElementInfusionBoardManager.EElement eElement = (ElementInfusionBoardManager.EElement)i;
			if (ElementInfusionBoardManager.ElementColumn(eElement) != ElementInfusionBoardManager.EColumn.Inert && !IsElementReserved(eElement))
			{
				list.Add(eElement);
			}
		}
		return list;
	}

	public void SaveState()
	{
		elementsInCreationSaved = new List<ElementInfusionBoardManager.EElement>(elementsInCreation);
		elementsReservedSaved = new List<ElementInfusionBoardManager.EElement>(elementsReserved);
		stateSaved = true;
	}

	public void RestoreState()
	{
		if (stateSaved)
		{
			elementsInCreation = new List<ElementInfusionBoardManager.EElement>(elementsInCreationSaved);
			elementsReserved = new List<ElementInfusionBoardManager.EElement>(elementsReservedSaved);
		}
		stateSaved = false;
	}

	public void UpdateBoard(ElementInfusionBoardManager.EElement element)
	{
		UpdateBoard(new List<ElementInfusionBoardManager.EElement> { element });
	}

	public void RemoveElementsInCreation(List<ElementInfusionBoardManager.EElement> elements)
	{
		bool flag = false;
		foreach (ElementInfusionBoardManager.EElement element in elements)
		{
			flag |= elementsInCreation.Remove(element);
		}
		if (flag)
		{
			UpdateBoard();
		}
	}

	public void UpdateBoard(List<ElementInfusionBoardManager.EElement> createdElements = null)
	{
		if (createdElements != null && createdElements.Count > 0 && createdElements[0] == ElementInfusionBoardManager.EElement.Any)
		{
			return;
		}
		if (createdElements != null)
		{
			foreach (ElementInfusionBoardManager.EElement createdElement in createdElements)
			{
				if (!elementsInCreation.Contains(createdElement))
				{
					elementsInCreation.Add(createdElement);
				}
			}
		}
		List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
		for (int i = 0; i < 6; i++)
		{
			if (ElementInfusionBoardManager.ElementColumn((ElementInfusionBoardManager.EElement)i) > ElementInfusionBoardManager.EColumn.Inert)
			{
				if (elementsInCreation.Contains((ElementInfusionBoardManager.EElement)i))
				{
					elementsInCreation.Remove((ElementInfusionBoardManager.EElement)i);
				}
				list.Add((ElementInfusionBoardManager.EElement)i);
			}
		}
		list.AddRange(elementsInCreation);
		list.Sort();
		foreach (KeyValuePair<ElementInfusionBoardManager.EElement, InfusionElementUI> item in elementsUI)
		{
			if (!list.Contains(item.Key))
			{
				item.Value.SetState(null);
				item.Value.gameObject.SetActive(value: false);
			}
			else if (IsElementReserved(item.Key))
			{
				item.Value.gameObject.SetActive(value: false);
			}
			else
			{
				item.Value.gameObject.SetActive(value: true);
				item.Value.SetState(ElementInfusionBoardManager.ElementColumn(item.Key), elementsInCreation.Contains(item.Key) && ElementInfusionBoardManager.ElementColumn(item.Key) == ElementInfusionBoardManager.EColumn.Inert, elementsReserved.Contains(item.Key));
			}
		}
	}

	public void SetAvailableElements(List<ElementInfusionBoardManager.EElement> elements)
	{
		foreach (KeyValuePair<ElementInfusionBoardManager.EElement, InfusionElementUI> item in elementsUI)
		{
			item.Value.SetAvailable(elements.Contains(ElementInfusionBoardManager.EElement.Any) || elements.Contains(item.Key));
		}
	}
}
