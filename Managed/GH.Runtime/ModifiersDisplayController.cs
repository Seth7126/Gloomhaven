using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.UI;

public class ModifiersDisplayController : MonoBehaviour
{
	[SerializeField]
	private UIAttackModifierCalculator characterMissMod;

	[SerializeField]
	private UIAttackModifierCalculator characterMinusTwoMod;

	[SerializeField]
	private UIAttackModifierCalculator characterMinusOneMod;

	[SerializeField]
	private UIAttackModifierCalculator characterZeroMod;

	[SerializeField]
	private UIAttackModifierCalculator characterPlusOneMod;

	[SerializeField]
	private UIAttackModifierCalculator characterPlusTwoMod;

	[SerializeField]
	private UIAttackModifierCalculator characterTimesTwoMod;

	[SerializeField]
	private RectTransform modifiersContent;

	[SerializeField]
	private GameObject modifiersHighlight;

	[SerializeField]
	private float animationTime = 0.5f;

	[SerializeField]
	private VerticalLayoutGroup popupGroup;

	private ContentSizeFitter poupContentSizeFitter;

	private CanvasGroup popupCanvasGroup;

	[Header("Conditionals")]
	[SerializeField]
	private UIScenarioAttackModifier modifierPrefab;

	[SerializeField]
	private RectTransform conditionalModifierContainer;

	private LTDescr currentAnimation;

	private bool active;

	private bool resetPositionToHide = true;

	private List<UIScenarioAttackModifier> conditionalPool = new List<UIScenarioAttackModifier>();

	private const string DebugCancelHide = "HideModifiersDisplay";

	private const string DebugCancelDisplay = "DisplayeModifiersDisplay";

	private const string DebugCancelDisable = "OnDisable ModifiersDisplay";

	[UsedImplicitly]
	private void Awake()
	{
		popupCanvasGroup = popupGroup.GetComponent<CanvasGroup>();
		poupContentSizeFitter = popupGroup.GetComponent<ContentSizeFitter>();
		Hide(immendiately: true);
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		CancelLeanTween();
	}

	public void Hide(bool immendiately = false)
	{
		if (!active && !immendiately)
		{
			return;
		}
		active = false;
		CancelLeanTween();
		if (immendiately)
		{
			popupGroup.enabled = true;
			if ((object)poupContentSizeFitter == null)
			{
				poupContentSizeFitter = popupGroup.GetComponent<ContentSizeFitter>();
			}
			poupContentSizeFitter.enabled = true;
			HideFinish();
		}
		else
		{
			popupGroup.enabled = false;
			poupContentSizeFitter.enabled = false;
			currentAnimation = LeanTween.moveX(modifiersContent, 0f, animationTime).setOnComplete((Action)delegate
			{
				HideFinish();
			});
		}
	}

	private void CancelLeanTween()
	{
		if (currentAnimation != null)
		{
			LeanTween.cancel(currentAnimation.id, "HideModifiersDisplay");
			currentAnimation = null;
		}
	}

	private void HideFinish()
	{
		currentAnimation = null;
		resetPositionToHide = true;
		UIManager.Instance.UnhighlightElement(modifiersHighlight.gameObject, unlockUI: false);
		if ((object)popupCanvasGroup == null)
		{
			popupCanvasGroup = popupGroup.GetComponent<CanvasGroup>();
		}
		popupCanvasGroup.alpha = 0f;
		popupCanvasGroup.interactable = false;
		popupCanvasGroup.blocksRaycasts = false;
	}

	public void Display(CCharacterClass characterClass, bool isFocused = true)
	{
		active = true;
		UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.AttackModifiersHovered));
		characterMissMod.SetupCounters("*0", null, characterClass, isFocused);
		characterMinusTwoMod.SetupCounters("-2", null, characterClass, isFocused);
		characterMinusOneMod.SetupCounters("-1", null, characterClass, isFocused);
		characterZeroMod.SetupCounters("+0", null, characterClass, isFocused);
		characterPlusOneMod.SetupCounters("+1", null, characterClass, isFocused);
		characterPlusTwoMod.SetupCounters("+2", null, characterClass, isFocused);
		characterTimesTwoMod.SetupCounters("*2", null, characterClass, isFocused);
		RefreshConditionalModifiers(characterClass, isFocused);
		if (currentAnimation != null)
		{
			LeanTween.cancel(currentAnimation.id, "DisplayeModifiersDisplay");
			currentAnimation = null;
		}
		popupGroup.enabled = false;
		poupContentSizeFitter.enabled = false;
		popupCanvasGroup.alpha = 1f;
		popupCanvasGroup.interactable = true;
		popupCanvasGroup.blocksRaycasts = true;
		if (resetPositionToHide)
		{
			resetPositionToHide = false;
			modifiersContent.anchoredPosition3D = new Vector3(0f, modifiersContent.anchoredPosition3D.y, modifiersContent.anchoredPosition3D.z);
		}
		currentAnimation = LeanTween.moveX(modifiersContent, modifiersContent.rect.width, animationTime).setOnComplete((Action)delegate
		{
			currentAnimation = null;
			popupGroup.enabled = true;
			poupContentSizeFitter.enabled = true;
		});
	}

	private void RefreshConditionalModifiers(CCharacterClass characterClass, bool isFocused = true)
	{
		List<IGrouping<string, AttackModifierYMLData>> list = (from it in characterClass.AttackModifierCards.Concat(characterClass.DiscardedAttackModifierCards)
			where it.IsConditionalModifier || it.IsBless || it.IsCurse
			group it by it.Name).ToList();
		HelperTools.NormalizePool(ref conditionalPool, modifierPrefab.gameObject, conditionalModifierContainer, list.Count);
		for (int num = 0; num < list.Count; num++)
		{
			AttackModifierYMLData modif = list[num].First();
			conditionalPool[num].Init(modif, list[num].Count(), isFocused);
			conditionalPool[num].UpdateCounters(list[num].Count(), characterClass.DiscardedAttackModifierCards.Count((AttackModifierYMLData x) => x.Name == modif.Name));
		}
	}

	private void OnDisable()
	{
		if (currentAnimation != null)
		{
			LeanTween.cancel(currentAnimation.id, "OnDisable ModifiersDisplay");
			currentAnimation = null;
		}
	}

	public void SetUnfocused(bool unfocused)
	{
		if (IsOpen())
		{
			characterMissMod.SetUnfocused(unfocused);
			characterMinusTwoMod.SetUnfocused(unfocused);
			characterMinusOneMod.SetUnfocused(unfocused);
			characterZeroMod.SetUnfocused(unfocused);
			characterPlusOneMod.SetUnfocused(unfocused);
			characterPlusTwoMod.SetUnfocused(unfocused);
			characterTimesTwoMod.SetUnfocused(unfocused);
			for (int i = 0; i < conditionalPool.Count && conditionalPool[i].gameObject.activeSelf; i++)
			{
				conditionalPool[i].SetUnfocused(unfocused);
			}
		}
	}

	public bool IsOpen()
	{
		return active;
	}
}
