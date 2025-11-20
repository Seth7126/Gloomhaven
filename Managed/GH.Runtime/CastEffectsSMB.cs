using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using ScenarioRuleLibrary;
using UnityEngine;
using WorldspaceUI;

public class CastEffectsSMB : StateMachineBehaviour
{
	[Tooltip("How to rotate the area effects.\n None: Use rotation on the effect prefab.\n Radial: All area effect tiles will align to face the target.\n Linear: All effects will be aligned with the caster\n CasterToTarget: All effects will be aligned in the direction between the caster and the target")]
	public VFXShared.AreaEffectAlignments AreaEffectAlignment;

	[Tooltip("If true the caster will be considered the centre position for aligning area effects.  If false the tile clicked to target the ability will be the centre.")]
	public bool CasterIsCentre;

	[Tooltip("If true, the Area Hex Effects will play in all tiles adjacent to Targets")]
	public bool AreaEffectAdjacentToTargets;

	[Tooltip("The SFX to play during ability cast. Most of the ability PFX should contain audio, this field should be used sparingly when we can't add SFX on PFX prefab.")]
	[AudioEventName]
	public string AbilityAudioEvent;

	[Tooltip("[Depricated! - most of the audio now should be directly on PFX prefabs] The SFX to be played while casting the ablity")]
	public SFXBucketList AbilityHitSFX;

	[Tooltip("The effects to play on the target hex (the one clicked by the user)")]
	public VFXShared.TargetHexEffects TargetHexEffects;

	[Tooltip("The effects to play on tiles within the area effect that are not the target tile")]
	public VFXShared.TargetHexEffects AreaHexEffects;

	[Tooltip("The effects to play on the caster when casting this ability - as an Aura, meaning it will persist until the Aura ends")]
	public VFXShared.TargetHexEffects AuraEffects;

	[Tooltip("The effects to play on the caster when casting this ability")]
	public List<VFXShared.SpawnedEffect> CasterEffects;

	public int AuraAbilityID;

	public bool AutoStopAuraEffect;

	public List<VFXShared.Progress> ProgressChoreographer;

	public bool ControlChoreographerWithStateBehaviour;

	public bool IgnoreWeaponCollision;

	private void Awake()
	{
		if (TargetHexEffects != null)
		{
			foreach (VFXShared.BasicEffectTyped effect in TargetHexEffects.Effects)
			{
				effect.OriginalStartTime = effect.StartTime;
			}
			TargetHexEffects.OriginalHitTime = TargetHexEffects.HitTime;
		}
		if (AreaHexEffects != null)
		{
			foreach (VFXShared.BasicEffectTyped effect2 in AreaHexEffects.Effects)
			{
				effect2.OriginalStartTime = effect2.StartTime;
			}
			AreaHexEffects.OriginalHitTime = AreaHexEffects.HitTime;
		}
		if (AuraEffects != null)
		{
			foreach (VFXShared.BasicEffectTyped effect3 in AuraEffects.Effects)
			{
				effect3.OriginalStartTime = effect3.StartTime;
			}
			AuraEffects.OriginalHitTime = AuraEffects.HitTime;
		}
		foreach (VFXShared.SpawnedEffect casterEffect in CasterEffects)
		{
			casterEffect.OriginalStartTime = casterEffect.StartTime;
		}
		foreach (VFXShared.Progress item in ProgressChoreographer)
		{
			item.OriginalProgressTime = item.ProgressTime;
		}
	}

	private void ProcessTargetHexEffects(Animator animator, CharacterManager mainTargetCM, CActor.EType casterType, GameObject targetGO, ref VFXShared.HitEffectTargets hitEffectTargets, ref List<VFXShared.BasicEffectTyped> basicEffects, bool clone)
	{
		if (TargetHexEffects.Effects.Count <= 0)
		{
			return;
		}
		Quaternion rotation = Quaternion.identity;
		if (AreaEffectAlignment == VFXShared.AreaEffectAlignments.Linear || AreaEffectAlignment == VFXShared.AreaEffectAlignments.Radial)
		{
			rotation = Quaternion.LookRotation(animator.gameObject.transform.forward, animator.gameObject.transform.up);
		}
		else if (AreaEffectAlignment == VFXShared.AreaEffectAlignments.CasterToTarget)
		{
			rotation = Quaternion.LookRotation(targetGO.transform.position - animator.gameObject.transform.position, animator.gameObject.transform.up);
		}
		foreach (VFXShared.BasicEffectTyped effect in TargetHexEffects.Effects)
		{
			VFXShared.BasicEffectTyped basicEffectTyped = (clone ? effect.Clone() : effect);
			basicEffectTyped.Parent = targetGO.transform;
			basicEffectTyped.Position = basicEffectTyped.Effect.transform.position;
			basicEffectTyped.Rotation = rotation;
			basicEffectTyped.CasterType = casterType;
			if (mainTargetCM != null)
			{
				basicEffectTyped.TargetActorType = mainTargetCM.CharacterActor.Type;
			}
			if (VFXShared.ShouldPlayEffect(basicEffectTyped))
			{
				basicEffects.Add(basicEffectTyped);
			}
		}
		hitEffectTargets.HitTimes.Add(new VFXShared.HitTime(TargetHexEffects.HitTime));
		if (mainTargetCM != null)
		{
			hitEffectTargets.HitTimes.Last().targets.Add(mainTargetCM);
		}
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (Timekeeper.instance.m_GlobalClock.timeScale > 0f)
		{
			animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
		}
		if (!(Choreographer.s_Choreographer == null))
		{
			CoroutineHelper.RunCoroutine(EnterCoroutine(animator, stateInfo, layerIndex));
		}
	}

	private IEnumerator EnterCoroutine(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		VFXShared.ControlChoreographer(control: true, ControlChoreographerWithStateBehaviour ? this : null);
		VFXShared.HitEffectTargets hitEffectTargets = new VFXShared.HitEffectTargets(animator.gameObject);
		List<VFXShared.BasicEffectTyped> basicEffects = new List<VFXShared.BasicEffectTyped>();
		List<VFXShared.BasicEffectTyped> basicAuraEffects = new List<VFXShared.BasicEffectTyped>();
		VFXShared.PlaySpawnedEffectsFromTimeline(CasterEffects, animator.gameObject);
		while (AttackModBar.s_FrozenForAttackModifiers)
		{
			yield return null;
		}
		if (!string.IsNullOrEmpty(AbilityAudioEvent) && animator != null)
		{
			AudioController.Play(AbilityAudioEvent, animator.gameObject.transform, null, attachToParent: false);
		}
		GameObject mainTarget = ((Choreographer.s_Choreographer.m_lastSelectedTile == null) ? null : CharacterManager.FindTargetAtTile(Choreographer.s_Choreographer.m_lastSelectedTile));
		CharacterManager mainTargetCM = null;
		if (mainTarget != null)
		{
			mainTargetCM = CharacterManager.GetCharacterManager(mainTarget);
		}
		CharacterManager characterManager = null;
		if (animator != null)
		{
			characterManager = animator.gameObject.GetComponentInParent<CharacterManager>();
		}
		if (Choreographer.s_Choreographer.ActorsBeingTargetedForVFX != null && Choreographer.s_Choreographer.ActorsBeingTargetedForVFX.Count() > 0)
		{
			foreach (CActor item in Choreographer.s_Choreographer.ActorsBeingTargetedForVFX)
			{
				GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(item);
				if (gameObject != null && characterManager != null && animator != null)
				{
					ProcessTargetHexEffects(animator, CharacterManager.GetCharacterManager(gameObject), characterManager.CharacterActor.Type, gameObject, ref hitEffectTargets, ref basicEffects, clone: true);
				}
			}
		}
		else if (characterManager != null)
		{
			if (mainTarget != null)
			{
				ProcessTargetHexEffects(animator, mainTargetCM, characterManager.CharacterActor.Type, mainTarget, ref hitEffectTargets, ref basicEffects, clone: false);
			}
			else if (Choreographer.s_Choreographer.m_LastSelectedTiles != null && Choreographer.s_Choreographer.m_LastSelectedTiles.Count > 0)
			{
				foreach (CClientTile lastSelectedTile in Choreographer.s_Choreographer.m_LastSelectedTiles)
				{
					GameObject gameObject2 = lastSelectedTile.m_GameObject;
					if (gameObject2 != null)
					{
						ProcessTargetHexEffects(animator, null, characterManager.CharacterActor.Type, gameObject2, ref hitEffectTargets, ref basicEffects, clone: true);
					}
				}
			}
			else if (Choreographer.s_Choreographer.m_lastSelectedTile != null)
			{
				GameObject gameObject3 = Choreographer.s_Choreographer.m_lastSelectedTile.m_GameObject;
				if (gameObject3 != null)
				{
					ProcessTargetHexEffects(animator, null, characterManager.CharacterActor.Type, gameObject3, ref hitEffectTargets, ref basicEffects, clone: true);
				}
			}
		}
		if (characterManager != null && animator != null)
		{
			VFXShared.ProcessAreaEffects(AreaHexEffects, TargetHexEffects, animator, AreaEffectAlignment, CasterIsCentre, AreaEffectAdjacentToTargets, ref hitEffectTargets, ref basicEffects, characterManager.CharacterActor.Type);
			if (AuraEffects != null && AuraEffects.Effects.Count > 0 && AuraAbilityID > 0 && Choreographer.s_Choreographer.m_CurrentAbility != null)
			{
				VFXShared.RegisterAuraHexEffects(AuraAbilityID, AuraEffects, animator, AreaEffectAlignment, CasterIsCentre, characterManager.CharacterActor.Type);
				VFXShared.ProcessAuraHexEffects(Choreographer.s_Choreographer.m_CurrentAbility, VFXShared.RegisteredAuraHexEffects[AuraAbilityID], characterManager.CharacterActor, ref basicAuraEffects, ref hitEffectTargets);
			}
		}
		try
		{
			if (basicEffects.Count > 0)
			{
				VFXShared.PlayAreaEffectsFromTimeline(basicEffects);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception attempting to play basic area effects in CastEffectsSMB\n" + ex.Message + "\n" + ex.StackTrace);
		}
		try
		{
			if (basicAuraEffects.Count > 0)
			{
				VFXShared.PlayAuraEffectsFromTimeline(basicAuraEffects, AuraAbilityID);
			}
		}
		catch (Exception ex2)
		{
			Debug.LogError("Exception attempting to play basic aura effects in CastEffectsSMB\n" + ex2.Message + "\n" + ex2.StackTrace);
		}
		if (hitEffectTargets.HitTimes.Count > 0 && !IgnoreWeaponCollision)
		{
			CoroutineHelper.RunCoroutine(HitEffectTargetsCoroutine(hitEffectTargets));
		}
		if (ProgressChoreographer != null && ProgressChoreographer.Count > 0)
		{
			ActorEvents casterEvents = animator.gameObject.GetComponent<ActorEvents>();
			if (casterEvents == null)
			{
				UnityGameEditorObject componentInParent = animator.gameObject.GetComponentInParent<UnityGameEditorObject>();
				if (componentInParent != null && componentInParent?.PropObject?.RuntimeAttachedActor != null)
				{
					GameObject gameObject4 = Choreographer.s_Choreographer.FindClientActorGameObject(componentInParent.PropObject.RuntimeAttachedActor);
					if (gameObject4 != null)
					{
						casterEvents = ActorEvents.GetActorEvents(gameObject4);
					}
				}
			}
			ActorEvents mainTargetEvents = null;
			if (mainTarget != null)
			{
				mainTargetEvents = mainTarget.GetComponentInChildren<ActorEvents>();
			}
			float elapsedTime = 0f;
			List<VFXShared.Progress> progressChoreographerRemaining = ProgressChoreographer.ToList();
			progressChoreographerRemaining.RemoveAll((VFXShared.Progress x) => x == null);
			while (progressChoreographerRemaining.Count > 0)
			{
				try
				{
					for (int num = progressChoreographerRemaining.Count - 1; num >= 0; num--)
					{
						VFXShared.Progress progress = progressChoreographerRemaining[num];
						if (progress != null && elapsedTime >= progress.ProgressTime && ((progress.ProgressTarget == VFXShared.Progress.ProgressTargets.Caster && casterEvents != null) || (progress.ProgressTarget == VFXShared.Progress.ProgressTargets.MainTarget && mainTarget != null)))
						{
							((progress.ProgressTarget == VFXShared.Progress.ProgressTargets.Caster) ? casterEvents : mainTargetEvents).ProgressChoreographer();
							progressChoreographerRemaining.Remove(progress);
						}
					}
					elapsedTime += Timekeeper.instance.m_GlobalClock.deltaTime;
				}
				catch (Exception ex3)
				{
					Debug.LogError("Exception looping over progress choreographer list in CastEffectsSMB\n" + ex3.Message + "\n" + ex3.StackTrace);
				}
				yield return null;
			}
		}
		Choreographer.s_Choreographer.CurrentAttackArea.Clear();
		yield return null;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		if (AutoStopAuraEffect)
		{
			VFXShared.StopRegisteredAuraEffects(AuraAbilityID);
		}
		if (!(Choreographer.s_Choreographer == null))
		{
			VFXShared.ControlChoreographer(control: false, ControlChoreographerWithStateBehaviour ? this : null);
		}
	}

	private IEnumerator HitEffectTargetsCoroutine(VFXShared.HitEffectTargets targets)
	{
		ActorEvents ae = targets.CurrentActor.GetComponent<ActorEvents>();
		List<VFXShared.HitTime> hitTimes = targets.HitTimes.OrderBy((VFXShared.HitTime x) => x.Time).ToList();
		float elapsedTime = 0f;
		for (int i = 0; i < hitTimes.Count; i++)
		{
			if (hitTimes[i].targets.Count <= 0)
			{
				continue;
			}
			hitTimes[i].Time -= elapsedTime;
			if (hitTimes[i].Time > 0f)
			{
				yield return Timekeeper.instance.WaitForSeconds(hitTimes[i].Time);
				elapsedTime += hitTimes[i].Time;
			}
			ae.ProgressChoreographer(ControlChoreographerWithStateBehaviour ? this : null);
			if (!AbilityHitSFX)
			{
				continue;
			}
			foreach (CharacterManager target in hitTimes[i].targets)
			{
				SFXBucketList.PlayClipFromBucketList(target.m_SurfaceType, AbilityHitSFX, target.transform);
			}
		}
	}
}
