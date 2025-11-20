using System.Collections.Generic;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.UI;

public class UIItemModifiersTooltip : MonoBehaviour
{
	[SerializeField]
	private Vector2 offset;

	[SerializeField]
	private RectTransform window;

	[SerializeField]
	private RectTransform modifiersContainer;

	[SerializeField]
	private UIPerkAttackModifier modifierPrefab;

	private List<UIPerkAttackModifier> modifiersUI = new List<UIPerkAttackModifier>();

	public RectTransform RectTransform => window;

	public bool EnableTooltip { get; set; }

	public void Initialize(Dictionary<AttackModifierYMLData, int> modifiers)
	{
		modifiersUI.ForEach(delegate(UIPerkAttackModifier it)
		{
			ObjectPool.Recycle(it.gameObject);
		});
		modifiersUI.Clear();
		if (modifiers == null || modifiers.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<AttackModifierYMLData, int> modifier in modifiers)
		{
			UIPerkAttackModifier uIPerkAttackModifier = ObjectPool.Spawn(modifierPrefab, modifiersContainer);
			uIPerkAttackModifier.Init(modifier.Key, modifier.Value);
			modifiersUI.Add(uIPerkAttackModifier);
		}
	}

	public void ShowTooltip(bool showLeftToParent)
	{
		base.gameObject.SetActive(EnableTooltip && modifiersUI.Count > 0);
		if (EnableTooltip && modifiersUI.Count > 0)
		{
			window.anchorMax = new Vector2((!showLeftToParent) ? 1 : 0, window.anchorMax.y);
			window.anchorMin = new Vector2((!showLeftToParent) ? 1 : 0, window.anchorMin.y);
			window.pivot = new Vector2(showLeftToParent ? 1 : 0, window.pivot.y);
			window.anchoredPosition = offset * new Vector2((!showLeftToParent) ? 1 : (-1), 1f);
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.gameObject.transform as RectTransform);
	}

	public void HideTooltip()
	{
		base.gameObject.SetActive(value: false);
	}
}
