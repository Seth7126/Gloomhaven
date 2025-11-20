using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using MapRuleLibrary.Adventure;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UIPerksActiveModifiers : MonoBehaviour
{
	[Serializable]
	public class HoverPerkEvent : UnityEvent<AttackModifierYMLData>
	{
	}

	private const string FORMAT_ITEM_MODIFIERS = "{0} ({1}{2})";

	private const string ITEM_MODIFIER_SEPARATION = ", ";

	[SerializeField]
	private UIPerkAttackModifier modifierPrefab;

	public HoverPerkEvent OnHovered = new HoverPerkEvent();

	[Header("Attack Modifiers")]
	[SerializeField]
	private UIPerkAttackModifier characterMinusTwoMod;

	[SerializeField]
	private UIPerkAttackModifier characterMissMod;

	[SerializeField]
	private UIPerkAttackModifier characterMinusOneMod;

	[SerializeField]
	private UIPerkAttackModifier characterZeroMod;

	[SerializeField]
	private UIPerkAttackModifier characterPlusOneMod;

	[SerializeField]
	private UIPerkAttackModifier characterPlusTwoMod;

	[SerializeField]
	private UIPerkAttackModifier characterTimesTwoMod;

	[Header("Conditional Modifiers")]
	[SerializeField]
	private Transform conditionalModifContainer;

	private List<UIPerkAttackModifier> conditionalPool = new List<UIPerkAttackModifier>();

	[Header("Passive Attributes")]
	[SerializeField]
	private UIPerkAttackModifier passiveIgnoreItemEffects;

	[SerializeField]
	private UIPerkAttackModifier passiveIgnoreScenarioEffects;

	[Header("Additional")]
	[SerializeField]
	private GameObject additionalModifiers;

	[SerializeField]
	private RectTransform additionalModifiersContainer;

	[SerializeField]
	private TextMeshProUGUI additionalModifiersTitle;

	[SerializeField]
	private TextMeshProUGUI additionalModifiersDescription;

	private List<UIPerkAttackModifier> additionalModifiersPool = new List<UIPerkAttackModifier>();

	private ICharacterService character;

	private CCharacterClass characterClass;

	private Color additionalModifiersTitleColor;

	private Color additionalModifiersDescrColor;

	private void Awake()
	{
		additionalModifiersTitleColor = additionalModifiersTitle.color;
		additionalModifiersDescrColor = additionalModifiersDescription.color;
		characterMissMod.InitEmpty();
		characterTimesTwoMod.InitEmpty();
	}

	public void Display(ICharacterService character)
	{
		this.character = character;
		characterClass = CharacterClassManager.Classes.SingleOrDefault((CCharacterClass x) => x.ID == character.CharacterID);
		RefreshPassives();
		RefreshAttackModifiers();
		RefreshConditionalModifiers();
		RefreshAdditionalModifiers();
		RefreshIgnoreAdditionalModifiers();
	}

	private void RefreshIgnoreAdditionalModifiers()
	{
		bool flag = true;
		foreach (UIPerkAttackModifier item in additionalModifiersPool.TakeWhile((UIPerkAttackModifier it) => it.gameObject.activeSelf))
		{
			if (HasPerkIgnoreNegativeItemEffects() && !item.isRemover)
			{
				item.SetCancelled(isCancelled: true);
				continue;
			}
			item.SetCancelled(isCancelled: false);
			flag = false;
		}
		if (flag)
		{
			additionalModifiersTitle.color = UIInfoTools.Instance.greyedOutTextColor;
			additionalModifiersDescription.color = UIInfoTools.Instance.greyedOutTextColor;
		}
		else
		{
			additionalModifiersTitle.color = additionalModifiersTitleColor;
			additionalModifiersDescription.color = additionalModifiersDescrColor;
		}
	}

	private bool HasPerkIgnoreNegativeItemEffects()
	{
		return character.Perks.Exists((CharacterPerk it) => it.IsActive && it.Perk.IgnoreNegativeItemEffects);
	}

	private bool HasPerkIgnoreNegativeScenarioEffects()
	{
		return character.Perks.Exists((CharacterPerk it) => it.IsActive && it.Perk.IgnoreNegativeScenarioEffects);
	}

	private void RefreshAdditionalModifiers()
	{
		List<CItem> list = character.EquippedItems.FindAll((CItem it) => it.YMLData.Data.AdditionalModifiers.Count > 0 || it.YMLData.Data.RemoveModifiers.Count > 0);
		if (list.Count > 0)
		{
			additionalModifiersDescription.text = string.Join(", ", list.ConvertAll((CItem it) => string.Format("{0} ({1}{2})", LocalizationManager.GetTranslation(it.YMLData.Name), (((it.YMLData.Data.AdditionalModifiers.Count > 0) ? it.YMLData.Data.AdditionalModifiers.Sum((KeyValuePair<AttackModifierYMLData, int> modifiers) => modifiers.Value) : (-it.YMLData.Data.RemoveModifiers.Sum((KeyValuePair<AttackModifierYMLData, int> modifiers) => modifiers.Value))) > 0) ? "+" : "-", (it.YMLData.Data.AdditionalModifiers.Count > 0) ? it.YMLData.Data.AdditionalModifiers.Sum((KeyValuePair<AttackModifierYMLData, int> modifiers) => modifiers.Value) : it.YMLData.Data.RemoveModifiers.Sum((KeyValuePair<AttackModifierYMLData, int> modifiers) => modifiers.Value))));
			List<IGrouping<AttackModifierYMLData, KeyValuePair<AttackModifierYMLData, int>>> list2 = (from it in list.SelectMany((CItem it) => (it.YMLData.Data.AdditionalModifiers.Count <= 0) ? it.YMLData.Data.RemoveModifiers : it.YMLData.Data.AdditionalModifiers)
				group it by it.Key).ToList();
			HelperTools.NormalizePool(ref additionalModifiersPool, modifierPrefab.gameObject, additionalModifiersContainer, list2.Count);
			bool remover = list.Any((CItem it) => it.YMLData.Data.RemoveModifiers.Count > 0);
			for (int num = 0; num < list2.Count; num++)
			{
				AttackModifierYMLData key = list2[num].Key;
				additionalModifiersPool[num].Init(key, list2[num].Sum((KeyValuePair<AttackModifierYMLData, int> it) => it.Value), character.IsNewPerk(key), OnHover, remover);
			}
			additionalModifiers.SetActive(value: true);
		}
		else
		{
			additionalModifiers.SetActive(value: false);
		}
	}

	private void OnHover(AttackModifierYMLData perk, bool hovered)
	{
		if (hovered)
		{
			OnHovered.Invoke(perk);
		}
	}

	private void RefreshConditionalModifiers(List<AttackModifierYMLData> modifiersToAdd = null, List<AttackModifierYMLData> modifiersToRemove = null)
	{
		List<IGrouping<AttackModifierYMLData, AttackModifierYMLData>> perksConditionalModifierGroups = GetPerksConditionalModifierGroups();
		HelperTools.NormalizePool(ref conditionalPool, modifierPrefab.gameObject, conditionalModifContainer, perksConditionalModifierGroups.Count);
		for (int i = 0; i < perksConditionalModifierGroups.Count; i++)
		{
			AttackModifierYMLData modif = perksConditionalModifierGroups[i].Key;
			int counters = character.Perks.Where((CharacterPerk it) => it.IsActive).Sum((CharacterPerk it) => it.Perk.CardsToAdd.Count((AttackModifierYMLData m) => m.Name == modif.Name) - it.Perk.CardsToRemove.Count((AttackModifierYMLData m) => m.Name == modif.Name));
			int previewAdd = modifiersToAdd?.Count((AttackModifierYMLData it) => it.Name == modif.Name) ?? 0;
			int previewRemove = modifiersToRemove?.Count((AttackModifierYMLData it) => it.Name == modif.Name) ?? 0;
			conditionalPool[i].Init(modif, counters);
			conditionalPool[i].UpdateCounters(counters, previewAdd, previewRemove);
		}
	}

	private List<IGrouping<AttackModifierYMLData, AttackModifierYMLData>> GetPerksConditionalModifierGroups()
	{
		return (from it in character.Perks.SelectMany((CharacterPerk it) => it.Perk.CardsToAdd)
			where it.IsConditionalModifier
			group it by it).ToList();
	}

	private bool IsConditionalModifier(AttackModifierYMLData modif)
	{
		return modif.IsConditionalModifier;
	}

	public void RefreshAttackModifiers(List<AttackModifierYMLData> modifiersToAdd = null, List<AttackModifierYMLData> modifiersToRemove = null)
	{
		RefreshAttackModifier(characterMissMod, "*0", modifiersToAdd, modifiersToRemove);
		RefreshAttackModifier(characterMinusTwoMod, "-2", modifiersToAdd, modifiersToRemove);
		RefreshAttackModifier(characterMinusOneMod, "-1", modifiersToAdd, modifiersToRemove);
		RefreshAttackModifier(characterZeroMod, "+0", modifiersToAdd, modifiersToRemove);
		RefreshAttackModifier(characterPlusOneMod, "+1", modifiersToAdd, modifiersToRemove);
		RefreshAttackModifier(characterPlusTwoMod, "+2", modifiersToAdd, modifiersToRemove);
		RefreshAttackModifier(characterTimesTwoMod, "*2", modifiersToAdd, modifiersToRemove);
	}

	private void RefreshAttackModifier(UIPerkAttackModifier modifierUI, string id, List<AttackModifierYMLData> modifiersToAdd = null, List<AttackModifierYMLData> modifiersToRemove = null)
	{
		int previewAdd = modifiersToAdd?.Count((AttackModifierYMLData it) => it.MathModifier == id && !IsConditionalModifier(it)) ?? 0;
		int previewRemove = modifiersToRemove?.Count((AttackModifierYMLData it) => it.MathModifier == id && !IsConditionalModifier(it)) ?? 0;
		int counters = character.AttackModifierDeck.Count((AttackModifierYMLData x) => x.MathModifier.Equals(id) && !x.IsConditionalModifier) + CountNextScenarioAttackModifiers(id);
		modifierUI.UpdateCounters(counters, previewAdd, previewRemove);
	}

	private int CountNextScenarioAttackModifiers(string mathModifier)
	{
		if (AdventureState.MapState.MapParty?.NextScenarioEffects?.AttackModifiers == null)
		{
			return 0;
		}
		return (from y in AdventureState.MapState.MapParty.NextScenarioEffects.AttackModifiers.FindAll((Tuple<string, Dictionary<string, int>> x) => x.Item1 == "party" || x.Item1 == "NoneID" || x.Item1 == characterClass.CharacterID).ToList()
			select y.Item2).ToList().Sum((Dictionary<string, int> it) => it.Where(delegate(KeyValuePair<string, int> mod)
		{
			AttackModifierYMLData attackModifierYMLData = ScenarioRuleClient.SRLYML.AttackModifiers.SingleOrDefault((AttackModifierYMLData s) => s.Name == mod.Key);
			return attackModifierYMLData != null && attackModifierYMLData.MathModifier == mathModifier && !IsConditionalModifier(attackModifierYMLData);
		}).Sum((KeyValuePair<string, int> mod) => mod.Value));
	}

	public void RefreshPassives()
	{
		if (HasPerkIgnoreNegativeItemEffects())
		{
			passiveIgnoreItemEffects.UpdateCounters(1);
			passiveIgnoreItemEffects.SetActive(character.EquippedItems.Exists((CItem it) => it.YMLData.Data.AdditionalModifiers.Count > 0));
		}
		else
		{
			passiveIgnoreItemEffects.InitEmpty();
		}
		if (HasPerkIgnoreNegativeScenarioEffects())
		{
			passiveIgnoreScenarioEffects.UpdateCounters(1);
		}
		else
		{
			passiveIgnoreScenarioEffects.InitEmpty();
		}
	}

	public void PreviewAdd(CharacterPerk perk)
	{
		RefreshAttackModifiers(perk.Perk.CardsToAdd, perk.Perk.CardsToRemove);
		RefreshConditionalModifiers(perk.Perk.CardsToAdd, perk.Perk.CardsToRemove);
		foreach (AttackModifierYMLData item in perk.Perk.CardsToAdd)
		{
			GetAttackModifierUI(item)?.Highlight();
		}
		foreach (AttackModifierYMLData item2 in perk.Perk.CardsToRemove)
		{
			GetAttackModifierUI(item2)?.Highlight();
		}
		if (passiveIgnoreItemEffects.GetActiveCounters() == 0 && perk.Perk.IgnoreNegativeItemEffects)
		{
			passiveIgnoreItemEffects.UpdateCounters(0, 1, 0);
			passiveIgnoreItemEffects.Highlight();
		}
		if (passiveIgnoreScenarioEffects.GetActiveCounters() == 0 && perk.Perk.IgnoreNegativeScenarioEffects)
		{
			passiveIgnoreScenarioEffects.UpdateCounters(0, 1, 0);
			passiveIgnoreScenarioEffects.Highlight();
		}
	}

	public void ClearPreview()
	{
		RefreshPassives();
		RefreshAttackModifiers();
		RefreshConditionalModifiers();
		RefreshIgnoreAdditionalModifiers();
	}

	private UIPerkAttackModifier GetAttackModifierUI(AttackModifierYMLData modif)
	{
		UIPerkAttackModifier uIPerkAttackModifier = conditionalPool.FirstOrDefault((UIPerkAttackModifier it) => it.ModifierId != null && it.ModifierId == modif.Name);
		if (uIPerkAttackModifier != null)
		{
			return uIPerkAttackModifier;
		}
		if (modif.MathModifier == "*0")
		{
			return characterMissMod;
		}
		if (modif.MathModifier == "-2")
		{
			return characterMinusTwoMod;
		}
		if (modif.MathModifier == "-1")
		{
			return characterMinusOneMod;
		}
		if (modif.MathModifier == "+0")
		{
			return characterZeroMod;
		}
		if (modif.MathModifier == "+1")
		{
			return characterPlusOneMod;
		}
		if (modif.MathModifier == "+2")
		{
			return characterPlusTwoMod;
		}
		if (modif.MathModifier == "*2")
		{
			return characterTimesTwoMod;
		}
		return null;
	}
}
