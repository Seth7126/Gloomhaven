#define ENABLE_LOGS
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class CardsHandTabs : MonoBehaviour
{
	[SerializeField]
	private GameObject characterTabPrefab;

	[SerializeField]
	private ToggleGroup toggleGroup;

	[SerializeField]
	private float higlightTimeAnimation = 1f;

	private CharacterSymbolTab _currentCharacterTab;

	private List<CharacterSymbolTab> tabsList = new List<CharacterSymbolTab>();

	private const string TabName = "Character tab";

	private LTDescr highlightAnim;

	private const string DebugCancel = "StoptHighlightAnim";

	public CharacterSymbolTab CurrentCharacterTab
	{
		get
		{
			return _currentCharacterTab;
		}
		private set
		{
			if (_currentCharacterTab != value)
			{
				_currentCharacterTab = value;
				OnCurrentCharacterTabChanged();
			}
		}
	}

	public void Init(List<CPlayerActor> players)
	{
		HelperTools.NormalizePool(ref tabsList, characterTabPrefab, base.transform, players.Count);
		int num = 0;
		foreach (CPlayerActor player in players)
		{
			tabsList[num].name = "Character tab" + player.Class.ID;
			tabsList[num].Init(toggleGroup, player, OnTabClick);
			num++;
		}
	}

	public void Reset(CPlayerActor actorToReset = null)
	{
		foreach (CharacterSymbolTab tabs in tabsList)
		{
			if (actorToReset == null || actorToReset == tabs.PlayerActor)
			{
				tabs.UpdateSelectedCardsNum(0);
			}
		}
		RefreshConnectionState();
		UpdateTabsInteraction();
		toggleGroup.SetAllTogglesOff();
		tabsList[0].Select();
	}

	public void UpdateSelectedCardsNum(CPlayerActor playerActor, int cardsNumber)
	{
		foreach (CharacterSymbolTab tabs in tabsList)
		{
			if (tabs.PlayerActor == playerActor)
			{
				tabs.UpdateSelectedCardsNum(cardsNumber);
				break;
			}
		}
	}

	public void UpdateSelectedCardsNum(CPlayerActor playerActor, int cardsNumber, int maxNumber)
	{
		foreach (CharacterSymbolTab tabs in tabsList)
		{
			if (tabs.PlayerActor == playerActor)
			{
				tabs.UpdateSelectedCardsNum(cardsNumber);
				tabs.UpdateMaxSelectedCardsNum(maxNumber);
				break;
			}
		}
	}

	public void SelectTab(CPlayerActor playerActor)
	{
		foreach (CharacterSymbolTab tabs in tabsList)
		{
			if (tabs.PlayerActor == playerActor)
			{
				tabs.Select();
				break;
			}
		}
	}

	public void RefreshTabs()
	{
		CharacterSymbolTab hand = CardsHandManager.Instance.CurrentCharacterSymbolTab;
		if (!(hand == null))
		{
			tabsList.ForEach(delegate(CharacterSymbolTab x)
			{
				x.AnimateSelection(x.PlayerActor == hand.PlayerActor);
			});
		}
	}

	private void OnTabClick(CharacterClickedData characterClickedData)
	{
		Debug.Log("OnTabClick click");
		CurrentCharacterTab = characterClickedData.CharacterSymbolTab;
	}

	private void OnCurrentCharacterTabChanged()
	{
		InitiativeTrack.Instance.Select(_currentCharacterTab.PlayerActor);
	}

	public void UpdateTabsInteraction(bool interactable = true)
	{
		foreach (CharacterSymbolTab tabs in tabsList)
		{
			tabs.SetInteractable(interactable && !tabs.PlayerActor.IsDead);
			tabs.RefreshDead();
		}
	}

	public void RefreshConnectionState()
	{
		bool flag = false;
		foreach (CharacterSymbolTab tabs in tabsList)
		{
			tabs.RefreshPlayerOnlineState();
			flag |= FFSNetwork.IsOnline && tabs.PlayerActor.IsUnderMyControl;
		}
		if (flag)
		{
			StartHighlightAnim();
		}
		else
		{
			StoptHighlightAnim();
		}
	}

	private void StartHighlightAnim()
	{
		if (highlightAnim == null)
		{
			highlightAnim = LeanTween.value(base.gameObject, Highlight, 0f, 1f, higlightTimeAnimation).setLoopClamp().setOnCompleteOnRepeat(isOn: true);
		}
	}

	private void StoptHighlightAnim()
	{
		if (highlightAnim != null)
		{
			LeanTween.cancel(highlightAnim.id, "StoptHighlightAnim");
			highlightAnim = null;
		}
	}

	private void Highlight(float percent)
	{
		foreach (CharacterSymbolTab tabs in tabsList)
		{
			tabs.SetHighlightProgress(percent);
		}
	}

	private void OnDisable()
	{
		StoptHighlightAnim();
	}

	private void OnEnable()
	{
		RefreshConnectionState();
	}

	public void SelectNextTab()
	{
		List<CharacterSymbolTab> list = tabsList.FindAll((CharacterSymbolTab it) => !it.PlayerActor.IsDead || it.IsSelected);
		int num = list.FindIndex((CharacterSymbolTab it) => it.IsSelected);
		list[(num + 1) % list.Count].Click();
	}

	public void SelectPreviousTab()
	{
		List<CharacterSymbolTab> list = tabsList.FindAll((CharacterSymbolTab it) => !it.PlayerActor.IsDead || it.IsSelected);
		int num = list.FindIndex((CharacterSymbolTab it) => it.IsSelected);
		((num == 0) ? list.Last() : list[num - 1]).Click();
	}
}
