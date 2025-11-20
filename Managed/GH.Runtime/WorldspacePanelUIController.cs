#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Utils.Extensions;
using EPOOutline;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SpriteMemoryManagement;
using UnityEngine;
using UnityEngine.UI;
using WorldspaceUI;

public class WorldspacePanelUIController : WorldspaceDisplayPanelBase
{
	[SerializeField]
	public WorldspaceCharacterInfoPanel InfoUI;

	[SerializeField]
	private HealthBar m_HealthBar;

	[SerializeField]
	private EffectsBar m_EffectsBar;

	[SerializeField]
	private ShieldBar m_ShieldBar;

	[SerializeField]
	private AttackModBar m_AttackModBar;

	[SerializeField]
	private InfoBar m_InfoBar;

	[SerializeField]
	private EffectsBar m_PreviewEffectsBar;

	[SerializeField]
	private UITextTooltipTarget m_Tooltip;

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[Range(0f, 1f)]
	[SerializeField]
	private float m_UnfocusedOpacity = 0.5f;

	[SerializeField]
	private Color m_PlayerHealthColor = Color.green;

	[SerializeField]
	private Color m_EnemyHealthColor = Color.red;

	[SerializeField]
	private Color m_NeutralHealthColor = Color.cyan;

	[SerializeField]
	private Color m_Enemy2HealthColor = Color.blue;

	[SerializeField]
	private Color m_AllyMonsterHealthColor = Color.blue;

	[SerializeField]
	private Color m_HeroSummonHealthColor = Color.blue;

	[SerializeField]
	private Sprite defaultHealthIcon;

	[SerializeField]
	private Sprite eliteHealthIcon;

	[SerializeField]
	private Sprite bossHealthIcon;

	public Vector2 m_BossScaleIncrease = Vector2.one;

	[SerializeField]
	private Toggle m_SelectionIndicador;

	[SerializeField]
	private float m_HiddenPreviewEffectPanelPositionY = 48f;

	[SerializeField]
	private RectTransform m_HealthRoot;

	[SerializeField]
	private RectTransform m_ShieldsRoot;

	[SerializeField]
	private GUIAnimator addTargetAnimation;

	private GameObject CharacterToTrack;

	private Outlinable Outlinable;

	private float _zoom;

	private HashSet<string> focusedRequests = new HashSet<string>();

	private int retaliateDisplayed;

	public Vector3 TargetPosition => CharacterToTrack.transform.position;

	public Color AllegianceColor => ((m_Actor == null) ? CActor.EType.Enemy : m_Actor.OriginalType) switch
	{
		CActor.EType.Neutral => m_NeutralHealthColor, 
		CActor.EType.Enemy2 => m_Enemy2HealthColor, 
		CActor.EType.Enemy => m_EnemyHealthColor, 
		CActor.EType.Ally => m_AllyMonsterHealthColor, 
		CActor.EType.HeroSummon => m_HeroSummonHealthColor, 
		CActor.EType.Player => m_PlayerHealthColor, 
		_ => m_EnemyHealthColor, 
	};

	public void Init(GameObject character, CActor actor, float height = 1.8f)
	{
		Debug.Log("Init");
		CharacterToTrack = character;
		Outlinable = character.GetComponentInChildren<Outlinable>();
		Transform uIZoomMeasurePoint = CharacterToTrack.FindInChildren("UIZoomMeasurePoint").transform;
		ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(character);
		base.transform.localScale = Vector3.one;
		RectTransform previewEffectTransform = (RectTransform)m_PreviewEffectsBar.transform;
		Vector2 shownPreviewEffectPosition = previewEffectTransform.anchoredPosition;
		Vector2 hiddenPreviewEffectPosition = new Vector2(shownPreviewEffectPosition.x, m_HiddenPreviewEffectPanelPositionY);
		m_InfoBar.OnShow.AddListener(delegate
		{
			previewEffectTransform.anchoredPosition = shownPreviewEffectPosition;
		});
		m_InfoBar.OnHide.AddListener(delegate
		{
			previewEffectTransform.anchoredPosition = hiddenPreviewEffectPosition;
		});
		m_InfoBar.SetInfoGroupVisible(shouldSetVisible: false);
		bool flag = actor is CEnemyActor cEnemyActor && cEnemyActor.MonsterClass.Boss;
		Init(CharacterToTrack, uIZoomMeasurePoint, actor, actorBehaviour, flag ? m_BossScaleIncrease : Vector2.one);
		InfoUI.m_ActorBehavior = actorBehaviour;
		m_AttackModBar.Actor = character;
		m_WorldspaceOffsetY = height;
		UnhighlightPreview();
		if (actor is CEnemyActor)
		{
			m_HealthBar.InitHealth(actor.MaxHealth, actor.Health, actor.OriginalMaxHealth, AllegianceColor, flag ? bossHealthIcon : ((((CEnemyActor)actor).MonsterClass.NonEliteVariant != null) ? eliteHealthIcon : defaultHealthIcon));
		}
		else
		{
			m_HealthBar.InitHealth(actor.MaxHealth, actor.Health, actor.OriginalMaxHealth, AllegianceColor, defaultHealthIcon);
		}
		WorldspaceUITools.Instance.AddWorldspacePanelUIController(this);
	}

	public void UpdateHealth(int maxHealth, int currentHealth, int originalMaxHealth, bool animate = true, Action onUpdateHealth = null)
	{
		m_HealthBar.UpdateHealth(maxHealth, Mathf.Max(0, currentHealth), originalMaxHealth, animate, AllegianceColor, onUpdateHealth);
		UnhighlightPreview();
	}

	public void ShowHeal(int amount, bool overheal)
	{
		Focus(focus: true);
		m_InfoBar.ShowHeal(amount, overheal);
	}

	public void ShowDamage(int amount)
	{
		if (!m_AttackModBar.IsFlowActive)
		{
			Focus(focus: true);
			m_InfoBar.ShowDamage(amount);
		}
	}

	public void PreviewSimpleDamage(int amount)
	{
		if (!m_Actor.IsDead)
		{
			PreviewSimpleDamage(amount, m_Actor.Health);
		}
	}

	public void PreviewSimpleDamage(int amount, int health)
	{
		if (!m_Actor.IsDead)
		{
			m_HealthBar.PreviewAttack(health + amount, amount, 0, m_Actor.OriginalMaxHealth);
			m_InfoBar.PreviewAttack(health + amount, amount, 0, isEnemyAttacking: false, justDamage: true);
		}
	}

	public void PreviewHeal(int healAmount)
	{
		m_HealthBar.PreviewHeal(m_Actor.MaxHealth, m_Actor.Health, m_Actor.OriginalMaxHealth, healAmount);
		m_InfoBar.PreviewHeal(m_Actor.MaxHealth, m_Actor.Health, m_Actor.MaxHealth > m_Actor.OriginalMaxHealth && m_Actor.Health + healAmount > m_Actor.OriginalMaxHealth, healAmount);
	}

	public void PreviewDamage(int amount)
	{
		m_HealthBar.PreviewAttack(m_Actor.Health, amount, 0, m_Actor.OriginalMaxHealth);
		m_InfoBar.PreviewAttack(m_Actor.Health, amount, amount, isEnemyAttacking: false, justDamage: true);
	}

	public void ResetDamagePreview(int amount)
	{
		retaliateDisplayed = 0;
		if (m_HealthBar != null)
		{
			m_HealthBar.ResetPreview(Mathf.Max(0, m_Actor.Health + amount), m_Actor.OriginalMaxHealth);
			m_HealthBar.ResetColors();
		}
		m_InfoBar.ResetPreview();
		m_InfoBar.ResetColors();
		UnhighlightPreview();
	}

	public void ResetPreview()
	{
		ResetDamagePreview(0);
		m_PreviewEffectsBar.DisableFlags();
		PreviewAddTarget(show: false);
		Focus(focus: false, "PREVIEW_FOCUS");
		if (Outlinable != null)
		{
			WorldspaceUITools.Instance.DisableAbilityFocusOutline(Outlinable);
		}
	}

	public void UpdateEffects(bool blessed = false, bool controlled = false, bool cursed = false, bool disarmed = false, bool immobilized = false, bool invisible = false, bool muddled = false, bool pierce = false, bool poisoned = false, bool retaliate = false, bool strengthened = false, bool stunned = false, bool wounded = false, bool immovable = false, bool improvedShortEffect = false, bool fly = false, List<CActiveBonus> doomed = null, List<CActiveBonus> activeItemEffectsOnMonsters = null, bool sleep = false, bool blockHealing = false, bool neutralizeShield = false)
	{
		m_EffectsBar.UpdateEffects(blessed, controlled, cursed, disarmed, immobilized, invisible, muddled, pierce, poisoned, retaliate, strengthened, stunned, wounded, immovable, improvedShortEffect, fly, doomed, activeItemEffectsOnMonsters, sleep, m_Actor.CarriedQuestItems.Any(), blockHealing, neutralizeShield, m_Actor.CharacterResources);
	}

	public void AccentuateEffect(EffectsBar.FEffect effect)
	{
		m_EffectsBar.AccentuateEffect(effect);
	}

	public void RetaliateActivatedEffect()
	{
		m_EffectsBar.RetaliateActiveEffect();
	}

	public void RetaliateWarnEffect(bool active)
	{
		m_EffectsBar.RetaliateWarnEffect(active);
	}

	public void CancelRetaliateEffect()
	{
		m_EffectsBar.CancelRetaliateEffect();
	}

	public void ImmobilizedWarnEffect(bool active)
	{
		m_EffectsBar.ImmobilizedWarnEffect(active);
	}

	public void DisarmedWarnEffect(bool active)
	{
		m_EffectsBar.DisarmedWarnEffect(active);
	}

	public void StunnedWarnEffect(bool active)
	{
		m_EffectsBar.StunnedWarnEffect(active);
	}

	public void SleepingWarnEffect(bool active)
	{
		m_EffectsBar.SleepingWarnEffect(active);
	}

	public void AddEffect(string id, ReferenceToSprite icon, string title = null, string description = null, bool blink = false)
	{
		m_EffectsBar.AddCustomEffect(id, icon, title, description, blink);
	}

	public void RemoveEffect(string id)
	{
		m_EffectsBar.RemoveCustomEffect(id);
	}

	public void RefreshCarryingQuestItem()
	{
		if (m_Actor.CarriedQuestItems.Count == 0)
		{
			m_EffectsBar.ToggleCarryingQuestItemEffect();
		}
		else
		{
			m_EffectsBar.UntoggleCarryingQuestItemEffect();
		}
	}

	public void UpdateShields(int activeShields, bool ignoreShields)
	{
		m_ShieldBar.UpdateShields(activeShields, ignoreShields);
	}

	public void Focus(bool focus, string request = "MAIN_FOCUS")
	{
		if (focus)
		{
			focusedRequests.Add(request ?? base.gameObject.GetInstanceID().ToString());
		}
		else
		{
			focusedRequests.Remove(request ?? base.gameObject.GetInstanceID().ToString());
		}
		if (focusedRequests.Count == 0)
		{
			m_CanvasGroup.alpha = m_UnfocusedOpacity;
			m_HealthBar.Focus(focus: false);
			m_EffectsBar.DisableFlag(EffectsBar.FEffect.Advantage);
		}
		else
		{
			m_CanvasGroup.alpha = 1f;
			m_HealthBar.Focus(focus: true);
		}
	}

	public void UpdateRetaliateDamage(int retaliateDamage, bool forceUpdate = false)
	{
		if (retaliateDamage == 0)
		{
			if (forceUpdate || retaliateDisplayed != 0)
			{
				m_HealthBar.ResetPreview(Mathf.Max(0, m_Actor.Health), m_Actor.OriginalMaxHealth);
				m_InfoBar.SetInfoGroupVisible(shouldSetVisible: false);
			}
		}
		else
		{
			m_HealthBar.PreviewRetaliate(m_Actor.Health, retaliateDamage, m_Actor.OriginalMaxHealth);
			m_InfoBar.PreviewRetaliate(m_Actor.Health, retaliateDamage);
		}
		retaliateDisplayed = retaliateDamage;
	}

	public void OnSelectingAttackFocus(CAbilityAttack abilityAttack, CActor attacker, CAttackSummary.TargetSummary attackSummary, bool isEnemyAttacking = false)
	{
		Focus(focus: true, "PREVIEW_FOCUS");
		if (isEnemyAttacking || abilityAttack.AllTargetsOnMovePath)
		{
			m_HealthBar.UpdateColors(attackSummary.FinalAttackStrength - attackSummary.AttackModifierCardsStrength, abilityAttack.Strength);
			m_InfoBar.PreviewAttack(m_Actor.Health, attackSummary.FinalAttackStrength - attackSummary.AttackModifierCardsStrength, abilityAttack.Strength, isEnemyAttacking: true, justDamage: false, attackSummary.OverallAdvantage == EAdvantageStatuses.Advantage, attackSummary.OverallAdvantage == EAdvantageStatuses.Disadvantage);
		}
		else
		{
			m_HealthBar.PreviewAttack(m_Actor.Health, attackSummary.FinalAttackStrength, abilityAttack.Strength - (attackSummary.Parent.ConsumeStrengthBuff + attackSummary.Parent.ItemOverrideStrengthBuff), m_Actor.OriginalMaxHealth, highlight: false);
			m_InfoBar.PreviewAttack(m_Actor.Health, attackSummary.FinalAttackStrength, abilityAttack.Strength - (attackSummary.Parent.ConsumeStrengthBuff + attackSummary.Parent.ItemOverrideStrengthBuff), isEnemyAttacking: false, justDamage: false, attackSummary.OverallAdvantage == EAdvantageStatuses.Advantage, attackSummary.OverallAdvantage == EAdvantageStatuses.Disadvantage);
			m_InfoBar.InitBreakdown(attackSummary, abilityAttack.Strength - (attackSummary.Parent.ConsumeStrengthBuff + attackSummary.Parent.ItemOverrideStrengthBuff));
			PreviewEffects(attackSummary.AttackAbilityWithOverrides, m_Actor);
		}
		m_ShieldBar.SetPierce(attackSummary.ActorToAttack.CachedShieldNeutralized ? 99999 : attackSummary.Pierce);
		if (attackSummary.OverallAdvantage == EAdvantageStatuses.Advantage)
		{
			m_EffectsBar.EnableFlag(EffectsBar.FEffect.Advantage);
		}
		else
		{
			m_EffectsBar.DisableFlag(EffectsBar.FEffect.Advantage);
		}
		if (attackSummary.OverallAdvantage == EAdvantageStatuses.Disadvantage)
		{
			m_EffectsBar.EnableFlag(EffectsBar.FEffect.Disadvantage);
		}
		else
		{
			m_EffectsBar.DisableFlag(EffectsBar.FEffect.Disadvantage);
		}
		PreviewAddTarget(attackSummary.UsedAttackMods != null && attackSummary.UsedAttackMods.Exists((AttackModifierYMLData it) => it.AddTarget));
		if (Outlinable != null)
		{
			WorldspaceUITools.Instance.EnableAbilityFocusOutline(Outlinable);
		}
	}

	private void PreviewAddTarget(bool show)
	{
		if (show)
		{
			addTargetAnimation.gameObject.SetActive(value: true);
			addTargetAnimation.Play();
		}
		else
		{
			addTargetAnimation.gameObject.SetActive(value: false);
			addTargetAnimation.Stop();
		}
	}

	public void OnSelectingDamageFocus(CAbility abilityDamage, CActor attacker, CAttackSummary.TargetSummary damageSummary, bool isEnemyAttacking = false)
	{
		Focus(focus: true);
		m_HealthBar.PreviewAttack(m_Actor.Health, damageSummary.FinalAttackStrength, abilityDamage.Strength - (damageSummary.Parent.ConsumeStrengthBuff + damageSummary.Parent.ItemOverrideStrengthBuff), m_Actor.OriginalMaxHealth, highlight: false);
		m_InfoBar.PreviewAttack(m_Actor.Health, damageSummary.FinalAttackStrength, abilityDamage.Strength - (damageSummary.Parent.ConsumeStrengthBuff + damageSummary.Parent.ItemOverrideStrengthBuff), isEnemyAttacking: false, justDamage: true, damageSummary.OverallAdvantage == EAdvantageStatuses.Advantage, damageSummary.OverallAdvantage == EAdvantageStatuses.Disadvantage);
		m_InfoBar.InitBreakdown(damageSummary, abilityDamage.Strength - (damageSummary.Parent.ConsumeStrengthBuff + damageSummary.Parent.ItemOverrideStrengthBuff));
		PreviewEffects(abilityDamage, m_Actor);
		if (Outlinable != null)
		{
			WorldspaceUITools.Instance.EnableAbilityFocusOutline(Outlinable);
		}
		if (abilityDamage.AbilityText.IsNOTNullOrEmpty())
		{
			Singleton<HelpBox>.Instance.Show(abilityDamage.AbilityText.Replace("$", ""));
		}
		if (abilityDamage.ResourcesToTakeFromTargets == null)
		{
			return;
		}
		foreach (string key in abilityDamage.ResourcesToTakeFromTargets.Keys)
		{
			m_EffectsBar.SetCustomEffectBlinkActive(key, active: true);
		}
	}

	public void PreviewEffects(CAbility ability, CActor actor, bool removePreview = false)
	{
		PreviewEffect(CCondition.EPositiveCondition.Bless, EffectsBar.FEffect.Blessed, ability, actor, removePreview);
		PreviewEffect(CCondition.ENegativeCondition.Curse, EffectsBar.FEffect.Cursed, ability, actor, removePreview);
		PreviewEffect(CCondition.ENegativeCondition.Disarm, EffectsBar.FEffect.Disarmed, ability, actor, removePreview);
		PreviewEffect(CCondition.ENegativeCondition.Immobilize, EffectsBar.FEffect.Immobilized, ability, actor, removePreview);
		PreviewEffect(CCondition.EPositiveCondition.Invisible, EffectsBar.FEffect.Invisible, ability, actor, removePreview);
		PreviewEffect(CCondition.ENegativeCondition.Muddle, EffectsBar.FEffect.Muddled, ability, actor, removePreview);
		PreviewEffect(CCondition.ENegativeCondition.Poison, EffectsBar.FEffect.Poisoned, ability, actor, removePreview);
		PreviewEffect(CCondition.ENegativeCondition.Stun, EffectsBar.FEffect.Stunned, ability, actor, removePreview);
		PreviewEffect(CCondition.ENegativeCondition.Wound, EffectsBar.FEffect.Wounded, ability, actor, removePreview);
		PreviewEffect(CCondition.EPositiveCondition.Strengthen, EffectsBar.FEffect.Strengthened, ability, actor, removePreview);
		PreviewEffect(CCondition.ENegativeCondition.StopFlying, EffectsBar.FEffect.StopFlying, ability, actor, removePreview);
		PreviewEffect(CCondition.ENegativeCondition.Sleep, EffectsBar.FEffect.Sleep, ability, actor, removePreview);
		PreviewAbilityEffect(ability, removePreview);
	}

	public void PreviewAbilityEffect(CAbility ability, bool removePreview = false)
	{
		PreviewDoomEffect(ability, removePreview);
		PreviewEffectAbilityType(EffectsBar.FEffect.BlockHealing, ability, CAbility.EAbilityType.BlockHealing, removePreview);
		PreviewEffectAbilityType(EffectsBar.FEffect.NeutralizeShield, ability, CAbility.EAbilityType.NeutralizeShield, removePreview);
	}

	private void PreviewEffectAbilityType(EffectsBar.FEffect effect, CAbility ability, CAbility.EAbilityType abilityType, bool removePreview = false)
	{
		m_PreviewEffectsBar.ToggleEffect(effect, !removePreview && (ability.AbilityType == abilityType || ability.SubAbilities.Exists((CAbility it) => it.AbilityType == abilityType)));
	}

	public void PreviewDoomEffect(CAbility ability, bool removePreview = false)
	{
		if (ability is CAbilityAddDoom cAbilityAddDoom && cAbilityAddDoom.Doom.DoomAbilities.Count > 0)
		{
			if (removePreview)
			{
				m_PreviewEffectsBar.UntoggleDoomEffect(ability.ID.ToString());
			}
			else
			{
				m_PreviewEffectsBar.ToggleDoomEffect(ability.ID.ToString(), ability, cAbilityAddDoom.Doom);
			}
		}
		else
		{
			if (!(ability is CAbilityTransferDooms))
			{
				return;
			}
			foreach (CActiveBonus item in from it in CharacterClassManager.FindAllActiveBonuses(ability.TargetingActor)
				where it.Ability is CAbilityAddActiveBonus cAbilityAddActiveBonus && cAbilityAddActiveBonus.AddAbility.ID == ability.ID
				select it)
			{
				if (removePreview)
				{
					m_PreviewEffectsBar.UntoggleDoomEffect(item.BaseCard.ID.ToString() + item.ID);
				}
				else
				{
					m_PreviewEffectsBar.ToggleDoomEffect(item.BaseCard.ID.ToString() + item.ID, item);
				}
			}
		}
	}

	private void PreviewEffect(CCondition.EPositiveCondition condition, EffectsBar.FEffect effect, CAbility ability, CActor actor, bool removePreview = false)
	{
		bool flag = ability.HasCondition(condition) || (actor != null && ability.HaveActiveItemsCondition(condition, actor));
		bool flag2 = ability.HaveActiveItemsCondition(condition);
		if (removePreview)
		{
			flag = !removePreview;
		}
		m_PreviewEffectsBar.ToggleEffect(effect, flag || flag2);
		m_PreviewEffectsBar.SetBlinkActive(effect, !flag && flag2);
	}

	private void PreviewEffect(CCondition.ENegativeCondition condition, EffectsBar.FEffect effect, CAbility ability, CActor actor, bool removePreview = false)
	{
		bool flag = ability.HasCondition(condition) || (actor != null && ability.HaveActiveItemsCondition(condition, actor));
		bool flag2 = ability.HaveActiveItemsCondition(condition);
		if (removePreview)
		{
			flag = !removePreview;
		}
		m_PreviewEffectsBar.ToggleEffect(effect, flag || flag2);
		m_PreviewEffectsBar.SetBlinkActive(effect, !flag && flag2);
	}

	public void OnSelectingHealFocus(int healAmount)
	{
		Focus(focus: true);
		if (m_Actor.Tokens.HasKey(CCondition.ENegativeCondition.Poison))
		{
			m_EffectsBar.SetBlinkActive(EffectsBar.FEffect.Poisoned, isActive: true);
			return;
		}
		if (m_Actor.Tokens.HasKey(CCondition.ENegativeCondition.Wound))
		{
			m_EffectsBar.SetBlinkActive(EffectsBar.FEffect.Wounded, isActive: true);
		}
		PreviewHeal(healAmount);
		if (Outlinable != null)
		{
			WorldspaceUITools.Instance.EnableAbilityFocusOutline(Outlinable);
		}
	}

	public void OnSelectingDoomFocus(CAbility ability)
	{
		Focus(focus: true);
		PreviewDoomEffect(ability);
	}

	public void OnWonGold(int gold)
	{
		m_InfoBar.ShowGold(gold);
	}

	public void OnEarnedXP(int xp)
	{
		m_InfoBar.ShowXP(xp);
	}

	public void OnMessage(CMessageData message)
	{
		if (message.m_Type == CMessageData.MessageType.ActionSelection || message.m_Type == CMessageData.MessageType.ActorIsAttacking || message.m_Type == CMessageData.MessageType.EndAbilityAnimSync)
		{
			ResetPreview();
			m_ShieldBar.ResetShieldIcons();
			if (m_Actor.CachedShieldNeutralized)
			{
				m_ShieldBar.SetPierce(99999);
			}
			m_EffectsBar.DisableFlag(EffectsBar.FEffect.Advantage);
			m_EffectsBar.DisableFlag(EffectsBar.FEffect.Disadvantage);
			m_EffectsBar.SetBlinkActive(EffectsBar.FEffect.Poisoned);
			m_EffectsBar.SetBlinkActive(EffectsBar.FEffect.Wounded);
			m_EffectsBar.ResetCustomEffectBlink();
			HideInfoTooltip();
		}
		if (message.m_Type != CMessageData.MessageType.PlayerIsImmobilized)
		{
			ImmobilizedWarnEffect(active: false);
		}
		if (message.m_Type != CMessageData.MessageType.PlayerIsDisarmed)
		{
			DisarmedWarnEffect(active: false);
		}
		if (message.m_Type != CMessageData.MessageType.PlayerIsStunned)
		{
			StunnedWarnEffect(active: false);
		}
		if (message.m_Type == CMessageData.MessageType.StartTurn)
		{
			UnhighlightPreview();
			m_HealthBar.ResetPreview(Mathf.Max(0, m_Actor.Health), m_Actor.OriginalMaxHealth);
			if (m_Actor == message.m_ActorSpawningMessage)
			{
				Focus(focus: true);
			}
			else if (message.m_ActorSpawningMessage is CEnemyActor && (!(m_Actor is CEnemyActor) || m_Actor.Type == CActor.EType.Ally))
			{
				Focus(focus: true);
			}
			else
			{
				Focus(focus: false);
			}
		}
		else if (message.m_Type == CMessageData.MessageType.ActionSelection)
		{
			m_HealthBar.ResetColors();
			m_InfoBar.ResetColors();
			UnhighlightPreview();
			m_HealthBar.ResetPreview(Mathf.Max(0, m_Actor.Health), m_Actor.OriginalMaxHealth);
			Focus(m_Actor == message.m_ActorSpawningMessage);
		}
		else if (message.m_Type == CMessageData.MessageType.ActorIsHealing || message.m_Type == CMessageData.MessageType.ActorIsApplyingConditionActiveBonus)
		{
			UnhighlightPreview();
			if (message is CActorIsApplyingConditionActiveBonus_MessageData cActorIsApplyingConditionActiveBonus_MessageData)
			{
				PreviewDoomEffect(cActorIsApplyingConditionActiveBonus_MessageData.m_Ability, removePreview: true);
			}
		}
		else if (message.m_Type == CMessageData.MessageType.NextRound)
		{
			Focus(focus: true);
		}
		else if (message.m_Type == CMessageData.MessageType.StartAbility || message.m_Type == CMessageData.MessageType.EndAbility || message.m_Type == CMessageData.MessageType.UpdateCurrentActor)
		{
			Focus(message.m_ActorSpawningMessage == m_Actor);
		}
		else if (message.m_Type == CMessageData.MessageType.ActorSelectedAttackFocus)
		{
			CActorSelectedAttackFocus_MessageData cActorSelectedAttackFocus_MessageData = (CActorSelectedAttackFocus_MessageData)message;
			PreviewEffects(cActorSelectedAttackFocus_MessageData.m_Ability, m_Actor, !cActorSelectedAttackFocus_MessageData.m_Ability.ActorsToTarget.Contains(m_Actor));
		}
	}

	public void DisplayAttackModifierFlow(CAttackSummary.TargetSummary targetSummary, CAbilityAttack abilityAttack)
	{
		Debug.Log("[AttackModifiers]: DisplayAttackModifierFlow " + (m_AttackModBar.m_AttackModFinishCoroutine != null));
		Focus(focus: true);
		UnhighlightPreview();
		m_InfoBar.PrepareForAttack(targetSummary);
		m_HealthBar.PrepareForAttack(m_Actor.Health, m_Actor.OriginalMaxHealth);
		if (m_AttackModBar.m_AttackModCoroutine != null)
		{
			StopCoroutine(m_AttackModBar.m_AttackModCoroutine);
		}
		m_AttackModBar.m_AttackModCoroutine = StartCoroutine(m_AttackModBar.ShowModifiers(targetSummary, abilityAttack));
		if (m_AttackModBar.m_AttackModFinishCoroutine != null)
		{
			StopCoroutine(m_AttackModBar.m_AttackModFinishCoroutine);
			m_AttackModBar.m_AttackModFinishCoroutine = null;
		}
		m_AttackModBar.m_AttackModFinishEnumerator = m_AttackModBar.ShowAttackModifDamage(targetSummary, abilityAttack, delegate
		{
			Debug.Log("[AttackModifiers]: Finish  ShowAttackModifDamage coroutine " + (m_AttackModBar.m_AttackModFinishCoroutine != null));
			if (m_AttackModBar.m_AttackModFinishCoroutine != null)
			{
				StopCoroutine(m_AttackModBar.m_AttackModFinishCoroutine);
				m_AttackModBar.m_AttackModFinishCoroutine = null;
			}
		});
	}

	public void DisplayAttackModifierDamageFlow()
	{
		Debug.Log("[AttackModifiers]: DisplayAttackModifierDamageFlow");
		Focus(focus: true);
		UnhighlightPreview();
		if (m_AttackModBar.m_AttackModFinishCoroutine != null)
		{
			StopCoroutine(m_AttackModBar.m_AttackModFinishCoroutine);
		}
		m_AttackModBar.m_AttackModFinishCoroutine = StartCoroutine(m_AttackModBar.m_AttackModFinishEnumerator);
	}

	public void FinalizeAttackFlow()
	{
		Debug.Log("[AttackModifiers]: FinalizeAttackFlow");
		if (m_AttackModBar.m_AttackModCoroutine != null)
		{
			StopCoroutine(m_AttackModBar.m_AttackModCoroutine);
		}
		m_AttackModBar.m_AttackModCoroutine = null;
		if (m_AttackModBar.m_AttackModFinishCoroutine != null)
		{
			StopCoroutine(m_AttackModBar.m_AttackModFinishCoroutine);
		}
		m_AttackModBar.m_AttackModFinishCoroutine = null;
		m_AttackModBar.FinalizeFlow();
	}

	public bool FlowControlActive()
	{
		return m_AttackModBar.IsFlowActive;
	}

	public void Destroy(Action onDestroy = null)
	{
		WorldspaceUITools.Instance.RemovePanelUIController(this);
		Focus(focus: true);
		if (!FlowControlActive() && !m_HealthBar.IsAnimated)
		{
			Debug.Log("Destroyed healthbar " + m_Actor.ActorLocKey());
			onDestroy?.Invoke();
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			StartCoroutine(DestroyDelayed(onDestroy));
		}
	}

	public void Hide()
	{
		Focus(focus: true);
		if (!FlowControlActive() && !m_HealthBar.IsAnimated)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		StartCoroutine(WaitEndAnimation(delegate
		{
			base.gameObject.SetActive(value: false);
		}));
	}

	public void Show()
	{
		if (!FlowControlActive() && !m_HealthBar.IsAnimated)
		{
			base.gameObject.SetActive(value: true);
			return;
		}
		StartCoroutine(WaitEndAnimation(delegate
		{
			base.gameObject.SetActive(value: true);
		}));
	}

	private IEnumerator DestroyDelayed(Action onDestroy)
	{
		yield return new WaitWhile(() => FlowControlActive() || m_HealthBar.IsAnimated);
		Debug.Log("DestroyDelayed healthbar " + m_Actor.ActorLocKey());
		onDestroy?.Invoke();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private IEnumerator WaitEndAnimation(Action callback)
	{
		yield return new WaitWhile(() => FlowControlActive() || m_HealthBar.IsAnimated);
		callback();
	}

	public void HighlightPreview(bool isPositive)
	{
		m_SelectionIndicador.isOn = isPositive;
		m_SelectionIndicador.gameObject.SetActive(value: true);
		Focus(focus: true, "HIGHLIGHT_PREVIEW_FOCUS");
	}

	public void UnhighlightPreview()
	{
		m_SelectionIndicador.gameObject.SetActive(value: false);
		Focus(focus: false, "HIGHLIGHT_PREVIEW_FOCUS");
	}

	public void HighlightSelected()
	{
		m_HealthBar.HighlightPreview(highlight: true);
	}

	public void UnhighlightSelected()
	{
		m_HealthBar.HighlightPreview(highlight: false);
	}

	public void ShowInfoTooltip(string tooltipTitle, string tooltipText)
	{
		m_Tooltip.SetText((tooltipText == null) ? tooltipTitle : ("<color=#" + UIInfoTools.Instance.mainColor.ToHex() + ">" + tooltipTitle + "</color>"), refreshTooltip: false, tooltipText);
		m_Tooltip.OnPointerEnter(null);
	}

	public void HideInfoTooltip()
	{
		m_Tooltip.OnPointerExit(null);
	}

	private void OnDisable()
	{
		HideInfoTooltip();
		addTargetAnimation.Stop();
	}

	public void ReplaceCondition(EffectsBar.FEffect oldCondition, List<CCondition.EPositiveCondition> newConditions)
	{
		if (newConditions.Count > 0)
		{
			m_EffectsBar.ReplaceCondition(oldCondition, newConditions[0]);
		}
	}

	public void OnUpdatedZoom(float zoom)
	{
		if (!(Mathf.Abs(_zoom - zoom) < 0.0001f))
		{
			_zoom = zoom;
			m_HealthBar.OnUpdatedZoom(zoom);
			Vector3 localScale = m_HealthBar.transform.localScale;
			float x = 1f / localScale.x;
			float y = 1f / localScale.y;
			Vector3 localScale2 = new Vector3(x, y, 1f);
			m_HealthRoot.transform.localScale = localScale2;
			m_ShieldsRoot.transform.localScale = localScale2;
		}
	}
}
