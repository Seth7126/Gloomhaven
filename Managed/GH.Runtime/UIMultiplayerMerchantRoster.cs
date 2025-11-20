using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using UnityEngine;
using UnityEngine.Events;

public class UIMultiplayerMerchantRoster : MonoBehaviour
{
	public class HeroSlotEvent : UnityEvent<UIMultiplayerMerchantSlot>
	{
	}

	[SerializeField]
	private List<UIMultiplayerMerchantSlot> slots;

	public UnityEvent OnDeselectedSlot = new UnityEvent();

	public HeroSlotEvent OnSelectedSlot = new HeroSlotEvent();

	private UIMultiplayerMerchantSlot currentSlot;

	private Dictionary<string, UIMultiplayerMerchantSlot> assignedSlots = new Dictionary<string, UIMultiplayerMerchantSlot>();

	private CMapCharacter[] currentCharacters;

	public UIMultiplayerMerchantSlot CurrentSlot => currentSlot;

	private void Awake()
	{
		slots.ForEach(delegate(UIMultiplayerMerchantSlot it)
		{
			it.Init(delegate
			{
				OnSelectSlot(it);
			}, delegate
			{
				OnDeselectSlot(it);
			});
		});
	}

	protected void OnDestroy()
	{
		OnSelectedSlot.RemoveAllListeners();
		OnDeselectedSlot.RemoveAllListeners();
	}

	private void OnDeselectSlot(UIMultiplayerMerchantSlot slot)
	{
		if (currentSlot == slot)
		{
			currentSlot = null;
			OnDeselectedSlot.Invoke();
		}
	}

	private void OnSelectSlot(UIMultiplayerMerchantSlot slot)
	{
		if (currentSlot == null || currentSlot != slot)
		{
			UIMultiplayerMerchantSlot uIMultiplayerMerchantSlot = currentSlot;
			currentSlot = slot;
			if (uIMultiplayerMerchantSlot != null)
			{
				uIMultiplayerMerchantSlot.Deselect();
			}
			OnSelectedSlot.Invoke(slot);
		}
	}

	public void Init(CMapCharacter[] characters)
	{
		currentSlot = null;
		assignedSlots.Clear();
		currentCharacters = characters;
		List<UIMultiplayerMerchantSlot> list = new List<UIMultiplayerMerchantSlot>();
		for (int i = 0; i < characters.Count(); i++)
		{
			if (characters[i] != null)
			{
				assignedSlots[characters[i].CharacterID] = slots[i];
				slots[i].SetCharacter(characters[i], slots[i] == currentSlot);
			}
			else
			{
				slots[i].SetEmpty(string.Format(LocalizationManager.GetTranslation("GUI_MULTIPLAYER_HERO_SLOT"), i + 1), slots[i] == currentSlot);
				list.Add(slots[i]);
			}
		}
		foreach (UIMultiplayerMerchantSlot item in list)
		{
			assignedSlots[assignedSlots.Count.ToString()] = item;
		}
		RefreshInteractable();
	}

	public void RefreshInteractable()
	{
		bool flag = PlayerRegistry.AllPlayers.All((NetworkPlayer a) => !a.IsCreatingCharacter);
		foreach (UIMultiplayerMerchantSlot value in assignedSlots.Values)
		{
			value.SetInteractable(flag);
		}
		if (!flag && currentSlot != null)
		{
			currentSlot.Deselect();
		}
	}

	public void AssignPlayerTo(NetworkPlayer playerActor, string characterID, bool isOnline)
	{
		if (assignedSlots.ContainsKey(characterID))
		{
			assignedSlots[characterID].AssignTo(playerActor, isOnline);
		}
		else
		{
			Init(currentCharacters);
		}
	}

	public void UpdateConnectionStateSlot(string characterID, bool isOnline)
	{
		assignedSlots[characterID].UpdateConnectionStatus(isOnline);
	}

	public void UpdateConnectionStateSlot(NetworkPlayer player, bool isOnline)
	{
		foreach (UIMultiplayerMerchantSlot value in assignedSlots.Values)
		{
			if (value.AssignedPlayer == player)
			{
				value.UpdateConnectionStatus(isOnline);
			}
		}
	}

	public void AssignPlayerToSelectedSlot(NetworkPlayer player, bool isOnline)
	{
		CurrentSlot?.AssignTo(player, isOnline);
	}

	public void AssignPlayerToEmptySlots(NetworkPlayer playerActor, bool isOnline)
	{
		if (AdventureState.MapState != null)
		{
			int[] emptyCharacterSlots = AdventureState.MapState.MapParty.GetEmptyCharacterSlots();
			for (int i = 0; i < emptyCharacterSlots.Length; i++)
			{
				int num = emptyCharacterSlots[i];
				if (assignedSlots.ContainsKey(num.ToString()))
				{
					assignedSlots[num.ToString()].AssignTo(playerActor, isOnline);
				}
				else
				{
					Debug.LogError("Unassigned slot for " + num);
				}
			}
		}
		else
		{
			Debug.LogError("Unable to assign empty player slots");
		}
	}

	public void ResetSelection()
	{
		if (currentSlot != null)
		{
			currentSlot.SetSelected(selected: false);
		}
		currentSlot = null;
	}

	public void Focus(bool focus, bool interactable = true)
	{
		for (int i = 0; i < slots.Count; i++)
		{
			slots[i].Focus(focus || currentSlot == slots[i], interactable);
		}
	}

	public void DisableNavigation()
	{
		for (int i = 0; i < slots.Count; i++)
		{
			slots[i].DisableNavigation();
		}
	}

	public void EnableNavigation()
	{
		for (int i = 0; i < slots.Count; i++)
		{
			slots[i].EnableNavigation();
		}
	}
}
