using System;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class InitiativeTrackEnemyAvatar : InitiativeTrackActorAvatar
{
	[SerializeField]
	protected LayoutElement layout;

	[Header("Boss appearance")]
	[SerializeField]
	protected Sprite bossSelectedSprite;

	[SerializeField]
	protected float bossWidth;

	[Header("Enemy appearance")]
	[SerializeField]
	protected Sprite enemySelectedSprite;

	[SerializeField]
	protected float enemyWidth;

	public override CActor.EType ActorType()
	{
		return CActor.EType.Enemy;
	}

	public override void PlayEffect(InitiativeEffects effect)
	{
		base.PlayEffect(effect);
		fxControls.ToggleEnable(effect != InitiativeEffects.None);
	}

	public override void Init(Action<bool> onHighlightCallback, CActor actor, bool interactable)
	{
		DecorateBoss(actor);
		base.Init(onHighlightCallback, actor, interactable);
		fxControls.ToggleEnable(active: false);
	}

	public override void SetAttributesDirect(CActor actor, bool activeIDBackplate, bool activeID, string textID, bool activeInitiative, int initiative, int currentHealth, int maxHealth, bool isActive, bool activeHilight, bool activeButton)
	{
		DecorateBoss(actor);
		base.SetAttributesDirect(actor, activeIDBackplate, activeID, textID, activeInitiative, initiative, currentHealth, maxHealth, isActive, activeHilight, activeButton);
		fxControls.ToggleEnable(active: false);
	}

	private void DecorateBoss(CActor actor)
	{
		CMonsterClass monsterClass = ((CEnemyActor)actor).MonsterClass;
		if (monsterClass.Boss && !monsterClass.MonsterYML.UseNormalSizeAvatarForBoss)
		{
			BuildBoss();
		}
		else
		{
			BuildEnemy();
		}
	}

	[ContextMenu("Build boss")]
	protected void BuildBoss()
	{
	}

	[ContextMenu("Build enemyActor")]
	protected void BuildEnemy()
	{
	}
}
