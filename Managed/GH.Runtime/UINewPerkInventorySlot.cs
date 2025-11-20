using System;
using System.Collections.Generic;
using AsmodeeNet.Foundation;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.UI;

public class UINewPerkInventorySlot : UIPerkInventorySlot
{
	[SerializeField]
	private float hoverBackgroundOpacity = 0.9f;

	[Header("Active")]
	[SerializeField]
	private float activeMaskBackgroundPosition;

	[SerializeField]
	private float activeHoverDuration;

	[SerializeField]
	private float activeBackgroundOpacity = 1f;

	[SerializeField]
	private Image pairHighlgiht;

	private LTDescr activeHoverAnimation;

	private bool isHovered;

	private bool _isFirstSelect = true;

	private static readonly int _mask = Shader.PropertyToID("_Mask");

	protected override void Awake()
	{
		base.Awake();
		background.material = new Material(background.material);
	}

	public override void Init(ICharacterService character, List<CharacterPerk> perks, Action<bool, UIPerkInventorySlot> onHoverCallback, Action<UIPerkInventorySlot> onSelectedCallback, bool isInteractable = true)
	{
		base.Init(character, perks, onHoverCallback, onSelectedCallback, isInteractable);
		isHovered = false;
	}

	public override void OnHover(bool hovered)
	{
		if (!LevelMessageUILayoutGroup.IsShown || _isFirstSelect)
		{
			isHovered = hovered;
			base.OnHover(hovered);
			_isFirstSelect = false;
			RefreshActiveHover();
		}
	}

	protected override void PreviewSelect(bool preview)
	{
		if (preview)
		{
			background.sprite = UIInfoTools.Instance.GetNewAdventureCharacterPortrait((character.PerkPoints > 0 || base.ActiveCounters > 0) ? character.CharacterModel : ECharacter.Elementalist, highlighted: true, character.Class.CustomCharacterConfig);
			background.color = ((character.PerkPoints > 0 || base.ActiveCounters > 0) ? UIInfoTools.Instance.White : Color.gray);
		}
		base.PreviewSelect(preview && character.PerkPoints > 0);
	}

	private void RefreshActiveHover()
	{
		if (base.ActiveCounters == base.TotalCounters)
		{
			background.SetAlpha(activeBackgroundOpacity);
			AnimateActiveHighlight(isHovered ? 0f : activeMaskBackgroundPosition, animate: true);
		}
		else
		{
			background.SetAlpha(isHovered ? hoverBackgroundOpacity : 0f);
			AnimateActiveHighlight(0f, animate: true);
		}
	}

	private void AnimateActiveHighlight(float to, bool animate = false)
	{
		CancelHoverActiveAnimation();
		if (!background.materialForRendering.HasProperty(_mask))
		{
			return;
		}
		Vector2 tiling = background.materialForRendering.GetTextureScale(_mask);
		if (!animate)
		{
			background.materialForRendering.SetTextureScale(_mask, new Vector2(to, tiling.y));
			return;
		}
		float time = Math.Abs(to - tiling.x) / activeMaskBackgroundPosition * activeHoverDuration;
		activeHoverAnimation = LeanTween.value(background.gameObject, delegate(float val)
		{
			background.materialForRendering.SetTextureScale(_mask, new Vector2(val, tiling.y));
		}, tiling.x, to, time).setOnComplete((Action)delegate
		{
			activeHoverAnimation = null;
		});
	}

	public override void RefreshCounters()
	{
		base.RefreshCounters();
		RefreshActiveHover();
	}

	private void CancelHoverActiveAnimation()
	{
		if (activeHoverAnimation != null)
		{
			LeanTween.cancel(activeHoverAnimation.id);
		}
		activeHoverAnimation = null;
	}

	protected override void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			base.OnDisable();
			CancelHoverActiveAnimation();
		}
	}
}
