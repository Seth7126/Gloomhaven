using System;
using System.Collections.Generic;
using AsmodeeNet.Foundation;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class UIEnhanceCardSlot : MonoBehaviour
{
	[SerializeField]
	private Button button;

	[SerializeField]
	private RectTransform cardHolder;

	[SerializeField]
	private Image selectedImage;

	[SerializeField]
	private List<UIEnhanceCardPoint> enhancementPoints;

	private AbilityCardUI abilityCard;

	private Action<AbilityCardUI> onSelected;

	private Action<bool, AbilityCardUI> onHovered;

	private bool isSelected;

	private List<UIEnhanceCardPoint> assignedPoints = new List<UIEnhanceCardPoint>();

	private UIEnhanceCardPoint nextPointUI;

	private bool isBuyMode;

	private bool isHoverBlock;

	public AbilityCardUI AbilityCard => abilityCard;

	public Selectable Selectable => button;

	private void Awake()
	{
	}

	public void Init(CAbilityCard card, ICharacter character, Action<AbilityCardUI> onSelected, Action<bool, AbilityCardUI> onHovered, bool isBuy)
	{
		this.onSelected = onSelected;
		this.onHovered = onHovered;
		isBuyMode = isBuy;
		nextPointUI = null;
		if (abilityCard != null && card.Name != abilityCard.CardName)
		{
			ObjectPool.RecycleCard(abilityCard.CardID, ObjectPool.ECardType.Ability, abilityCard.gameObject);
			abilityCard = null;
		}
		if (abilityCard == null)
		{
			abilityCard = ObjectPool.SpawnCard(card.ID, ObjectPool.ECardType.Ability, cardHolder, resetLocalScale: true, resetToMiddle: true).GetComponent<AbilityCardUI>();
		}
		abilityCard.Init(card, null, character.CharacterID, null, null, null);
		abilityCard.PlayEffect(InitiativeTrackActorAvatar.InitiativeEffects.None);
		CreateEnhancementPoints(isBuy);
		UpdateMode(isBuy);
		SetSelected(selected: false);
	}

	private void SetHoverBlock(bool value)
	{
		isHoverBlock = value;
	}

	public void OnHovered(bool hovered)
	{
		if (!isHoverBlock)
		{
			onHovered?.Invoke(hovered, abilityCard);
		}
	}

	private void CreateEnhancementPoints(bool isBuy)
	{
		assignedPoints.Clear();
		Color color = (isBuy ? UIInfoTools.Instance.buyEnhancementColor : UIInfoTools.Instance.sellEnhancementColor);
		int count = abilityCard.EnhancementElements.Buttons.Count;
		int count2 = abilityCard.EnhancementElements.AreaHexes.Count;
		int count3 = abilityCard.EnhancementElements.All.Count;
		int enhancedCount = abilityCard.EnhancementElements.GetEnhancedCount();
		HelperTools.NormalizePool(ref enhancementPoints, enhancementPoints[0].gameObject, enhancementPoints[0].transform, count3);
		for (int i = 0; i < count; i++)
		{
			enhancementPoints[i].Initialize(i < enhancedCount, color);
			assignedPoints.Add(enhancementPoints[i]);
		}
		for (int j = 0; j < count2; j++)
		{
			enhancementPoints[j + count].Initialize(j + count < enhancedCount, color);
			assignedPoints.Add(enhancementPoints[j]);
		}
		if (enhancedCount < assignedPoints.Count)
		{
			nextPointUI = assignedPoints[enhancedCount];
		}
	}

	public void Select()
	{
		if (!(abilityCard != null) || EnhancementUtils.CanBeEnhanced(abilityCard))
		{
			SetSelected(selected: true);
			onSelected?.Invoke(abilityCard);
		}
	}

	private void SetSelected(bool selected)
	{
		SetHoverBlock(selected);
		isSelected = selected;
		selectedImage.enabled = selected;
		for (int i = 0; i < assignedPoints.Count; i++)
		{
			assignedPoints[i].HighlightSelected(selected);
		}
		nextPointUI?.HighlightNextPoint(isSelected);
	}

	public void ShowEnhanced(bool show)
	{
		int enhancedCount = abilityCard.EnhancementElements.GetEnhancedCount();
		if (show)
		{
			assignedPoints[enhancedCount - 1].Enhance();
		}
		else
		{
			assignedPoints[enhancedCount].RemoveEnhance();
		}
		if (nextPointUI != null)
		{
			nextPointUI.HighlightNextPoint(highlight: false);
			nextPointUI = null;
		}
		if (enhancedCount < assignedPoints.Count)
		{
			nextPointUI = assignedPoints[enhancedCount];
			nextPointUI.HighlightNextPoint(isBuyMode);
		}
	}

	public void Deselect()
	{
		SetSelected(selected: false);
	}

	public void Clear()
	{
		if (abilityCard != null)
		{
			ObjectPool.RecycleCard(abilityCard.CardID, ObjectPool.ECardType.Ability, abilityCard.gameObject);
			abilityCard = null;
		}
		nextPointUI?.HighlightNextPoint(highlight: false);
	}

	public void UpdateMode(bool isBuy)
	{
		isBuyMode = isBuy;
		Color color = (isBuy ? UIInfoTools.Instance.buyEnhancementColor : UIInfoTools.Instance.sellEnhancementColor);
		selectedImage.color = color;
		foreach (UIEnhanceCardPoint assignedPoint in assignedPoints)
		{
			assignedPoint.UpdateColor(color);
		}
		if (isSelected)
		{
			nextPointUI?.HighlightNextPoint(isBuy);
		}
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			Clear();
		}
	}
}
