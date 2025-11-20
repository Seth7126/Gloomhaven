using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Utils.Extensions;
using GLOOM;
using ScenarioRuleLibrary;
using SpriteMemoryManagement;
using UnityEngine;
using UnityEngine.UI;

public class AbilityEffectManager : Singleton<AbilityEffectManager>
{
	private class Effect
	{
		public string id;

		public CAbility ability;

		public ActorBehaviour actor;

		public Color effectColor;

		public Action onApplied;

		public Action onUnapplied;
	}

	[SerializeField]
	private Image backgroundEffect;

	[SerializeField]
	private GUIAnimator effectAnimation;

	private List<Effect> effects = new List<Effect>();

	private Effect currentEffect;

	private const string EXTRA_TURN_ID = "ExtraTurn";

	public string GetAbilityEffectTitle(string id)
	{
		Effect effect = FindEffect(id);
		if (effect == null)
		{
			return null;
		}
		return string.Format("<color=#{1}>{0}</color>", LocalizationManager.GetTranslation((effect.ability.AbilityBaseCard != null) ? effect.ability.AbilityBaseCard.Name : effect.ability.ParentAbilityBaseCard.Name), effect.effectColor.ToHex());
	}

	protected override void Awake()
	{
		base.Awake();
		SetInstance(this);
	}

	public bool IsControlAbilityAffectingActor(CActor actor)
	{
		if (currentEffect?.ability is CAbilityControlActor)
		{
			return currentEffect.actor.Actor == actor;
		}
		return false;
	}

	public bool IsItemAffectingActor(CActor actor, CItem item)
	{
		if (currentEffect?.ability?.AbilityBaseCard is CItem cItem && currentEffect.actor.Actor == actor)
		{
			return item.ID == cItem.ID;
		}
		return false;
	}

	private Effect FindEffect(string id)
	{
		return effects.FirstOrDefault((Effect it) => it.id == id);
	}

	private List<Effect> FindAllEffects(string id)
	{
		return effects.FindAll((Effect it) => it.id == id);
	}

	public void ShowEffect(CAbility ability, CActor actor, CPlayerActor caster, Action onApplied = null, Action onUnapplied = null)
	{
		ShowEffect(ability.ID.ToString(), ability, ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.FindClientActorGameObject(actor)), caster, onApplied, onUnapplied);
	}

	public void ShowEffect(string id, CAbility ability, ActorBehaviour actor, CPlayerActor caster, Action onApplied = null, Action onUnapplied = null)
	{
		ShowEffect(id, ability, actor, UIInfoTools.Instance.GetCharacterColor(caster), ability.PreviewEffectId.IsNullOrEmpty() ? caster.CharacterClass.DefaultModel : ability.PreviewEffectId, onApplied, onUnapplied);
	}

	public void ShowEffect(CAbility ability, ActorBehaviour actor, Color effectColor, string previewEffectId, Action onApplied = null, Action onUnapplied = null)
	{
		ShowEffect(ability.ID.ToString(), ability, actor, effectColor, previewEffectId, onApplied, onUnapplied);
	}

	public void ShowEffect(string id, CAbility ability, ActorBehaviour actor, Color effectColor, string previewEffectId = null, Action onApplied = null, Action onUnapplied = null)
	{
		Effect effect = FindEffect(id);
		if (effect == null || !(effect.actor == actor))
		{
			Effect effect2 = new Effect
			{
				id = id,
				ability = ability,
				actor = actor,
				effectColor = effectColor,
				onUnapplied = onUnapplied,
				onApplied = onApplied
			};
			effects.Add(effect2);
			ApplyEffect(effect2);
			PreviewEffectInfo previewEffectConfig = UIInfoTools.Instance.GetPreviewEffectConfig(previewEffectId ?? ability.PreviewEffectId);
			if (previewEffectConfig != null)
			{
				string text = (ability.PreviewEffectText.IsNOTNullOrEmpty() ? ability.PreviewEffectText : previewEffectConfig.previewEffectText);
				actor.m_WorldspacePanelUI.AddEffect(id, new ReferenceToSprite(previewEffectConfig.previewEffectIcon), string.Format("<color=#{1}>{0}</color>", LocalizationManager.GetTranslation(ability.AbilityBaseCard.Name), effectColor.ToHex()), text.IsNOTNullOrEmpty() ? LocalizationManager.GetTranslation(text) : null);
			}
		}
	}

	private void ApplyEffect(Effect effect)
	{
		ClearCurrentEffect();
		currentEffect = effect;
		backgroundEffect.color = effect.effectColor;
		backgroundEffect.gameObject.SetActive(value: true);
		effectAnimation.Play();
		effect.onApplied?.Invoke();
	}

	private void ClearCurrentEffect()
	{
		if (currentEffect != null)
		{
			effectAnimation.Stop();
			backgroundEffect.gameObject.SetActive(value: false);
			currentEffect.onUnapplied?.Invoke();
			currentEffect = null;
		}
	}

	public void RemoveEffect(CAbility ability)
	{
		RemoveAllEffects(ability.ID.ToString());
	}

	public void RemoveEffect(string id)
	{
		Effect effect = FindEffect(id);
		if (effect == null)
		{
			return;
		}
		if (ScenarioManager.Scenario.HasActor(effect.actor.Actor))
		{
			effect.actor.m_WorldspacePanelUI.RemoveEffect(id);
		}
		effects.Remove(effect);
		if (effect == currentEffect)
		{
			ClearCurrentEffect();
			if (effects.Count > 0)
			{
				ApplyEffect(effects[effects.Count - 1]);
			}
		}
	}

	public void RemoveAllEffects(string id)
	{
		foreach (Effect item in FindAllEffects(id))
		{
			if (ScenarioManager.Scenario.HasActor(item.actor.Actor))
			{
				item.actor.m_WorldspacePanelUI.RemoveEffect(id);
			}
			effects.Remove(item);
			if (item == currentEffect)
			{
				ClearCurrentEffect();
				if (effects.Count > 0)
				{
					ApplyEffect(effects[effects.Count - 1]);
				}
			}
		}
	}

	public void ShowExtraTurn(CAbility ability, ActorBehaviour actor, CPlayerActor caster)
	{
		string id = "ExtraTurn" + actor.Actor.Class.ID;
		if (caster == actor.Actor)
		{
			ShowEffect(id, ability, actor, UIInfoTools.Instance.basicTextColor);
			return;
		}
		ShowEffect(id, ability, actor, caster, delegate
		{
			InitiativeTrack.Instance.FindInitiativeTrackActor(actor.Actor)?.ShowExtraTurn(show: true);
		}, delegate
		{
			InitiativeTrack.Instance.FindInitiativeTrackActor(actor.Actor)?.ShowExtraTurn(show: false);
		});
	}

	public string ExtraTurnAbilityEffectTitle(CActor actor)
	{
		return GetAbilityEffectTitle("ExtraTurn" + actor.Class.ID);
	}

	public void RemoveExtraTurnEffect(CActor actor)
	{
		RemoveEffect("ExtraTurn" + actor.Class.ID);
	}
}
