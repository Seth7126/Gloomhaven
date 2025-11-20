using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ara;
using Chronos;
using ScenarioRuleLibrary;
using UnityEngine;

public class FireSpawnedProjectileSMB : StateMachineBehaviour
{
	public class SpawnedProjectileHitEventArgs : EventArgs
	{
		public ActorBehaviour TargetActorBehaviour;

		public SpawnedProjectileHitEventArgs(ActorBehaviour actorBehaviour)
		{
			TargetActorBehaviour = actorBehaviour;
		}
	}

	public class SpawnedProjectileFireEventArgs : EventArgs
	{
		public Transform Target;

		public SpawnedProjectileFireEventArgs(Transform t)
		{
			Target = t;
		}
	}

	public GameObject Projectile;

	public string ProjectileAttachPoint;

	public float ProjectileStartTime;

	public float ProjectileSpeed;

	[AudioEventName]
	public string AbilityAudioEvent;

	[AudioEventName]
	public string AbilityHitAudioEvent;

	public GameObject ProjectileHitEffect;

	public List<VFXShared.SpawnedEffect> SpawnedProjectileEffects;

	public bool orientOnFire;

	public bool clearPointDataOnFire = true;

	public bool fadeProjectileStartOnImpact;

	private GameObject[] clearPointDataObjs;

	[Header("Area Effects On Hit")]
	[Tooltip("How to rotate the area effects.\n None: Use rotation on the effect prefab.\n Radial: All area effect tiles will align to face the target.\n Linear: All effects will be aligned with the caster\n CasterToTarget: All effects will be aligned in the direction between the caster and the target")]
	public VFXShared.AreaEffectAlignments AreaEffectAlignment;

	[Tooltip("If true the caster will be considered the centre position for aligning area effects.  If false the tile clicked to target the ability will be the centre.")]
	public bool CasterIsCentre;

	[Tooltip("The effects to play on the target hex (the one clicked by the user)")]
	public VFXShared.TargetHexEffects TargetHexEffects;

	[Tooltip("The effects to play on tiles within the area effect that are not the target tile")]
	public VFXShared.TargetHexEffects AreaHexEffects;

	private float m_OriginalProjectileSpeed;

	private float m_OriginalProjectileStartTime;

	public static event EventHandler<SpawnedProjectileHitEventArgs> SpawnedProjectileHit;

	public static event EventHandler<SpawnedProjectileFireEventArgs> SpawnedProjectileFire;

	protected virtual void OnSpawnedProjectileHit(SpawnedProjectileHitEventArgs e)
	{
		if (FireSpawnedProjectileSMB.SpawnedProjectileHit != null)
		{
			FireSpawnedProjectileSMB.SpawnedProjectileHit(this, e);
		}
	}

	protected virtual void OnSpawnedProjectileFire(SpawnedProjectileFireEventArgs e)
	{
		if (FireSpawnedProjectileSMB.SpawnedProjectileFire != null)
		{
			FireSpawnedProjectileSMB.SpawnedProjectileFire(this, e);
		}
	}

	private void Awake()
	{
		m_OriginalProjectileSpeed = ProjectileSpeed;
		m_OriginalProjectileStartTime = ProjectileStartTime;
		foreach (VFXShared.SpawnedEffect spawnedProjectileEffect in SpawnedProjectileEffects)
		{
			spawnedProjectileEffect.OriginalStartTime = spawnedProjectileEffect.StartTime;
		}
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
		CoroutineHelper.RunCoroutine(EnterCoroutine(animator, stateInfo, layerIndex));
	}

	private IEnumerator EnterCoroutine(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		VFXShared.ControlChoreographer(control: true, this);
		VFXShared.PlaySpawnedEffectsFromTimeline(SpawnedProjectileEffects, animator.gameObject);
		CharacterManager componentInParent = animator.gameObject.GetComponentInParent<CharacterManager>();
		if (!string.IsNullOrEmpty(AbilityAudioEvent))
		{
			AudioController.Play(AbilityAudioEvent, animator.gameObject.transform, null, attachToParent: false);
		}
		GameObject gameObject = animator.gameObject.FindInChildren(ProjectileAttachPoint);
		if (gameObject == null)
		{
			Debug.LogError("Error: The Effect Attach Point " + ProjectileAttachPoint + " can not be found in " + animator.gameObject.name);
		}
		else
		{
			yield return FireCoroutine(animator.gameObject, ProjectileStartTime, Projectile, gameObject.transform, Quaternion.LookRotation(animator.transform.forward, animator.transform.up), ProjectileSpeed, ProjectileHitEffect, animator, componentInParent.CharacterActor.Type);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		VFXShared.ControlChoreographer(control: false, this);
	}

	private IEnumerator FireCoroutine(GameObject firingCharacter, float startTime, GameObject projectile, Transform spawnPoint, Quaternion rotation, float projectileSpeed, GameObject hitEffect, Animator animator, CActor.EType casterType)
	{
		float t = 0f;
		if (startTime > 0f)
		{
			yield return Timekeeper.instance.WaitForSeconds(startTime);
			t = startTime;
		}
		CharacterManager characterManager = CharacterManager.GetCharacterManager(Choreographer.s_Choreographer.CurrentAttackTargets[0]);
		Transform target = characterManager.TargetBoneInstance.transform;
		OnSpawnedProjectileFire(new SpawnedProjectileFireEventArgs(target));
		GameObject projectileInstance = ObjectPool.Spawn(projectile, null, spawnPoint.position, rotation);
		if (orientOnFire)
		{
			Quaternion rotation2 = Quaternion.LookRotation(target.position - projectileInstance.transform.position, Vector3.up);
			projectileInstance.transform.rotation = rotation2;
		}
		if (clearPointDataOnFire)
		{
			AraTrail[] componentsInChildren = projectileInstance.GetComponentsInChildren<AraTrail>();
			if (componentsInChildren.Length != 0)
			{
				AraTrail[] array = componentsInChildren;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Clear();
				}
			}
		}
		VFXShared.HitEffectTargets hitEffectTargets = new VFXShared.HitEffectTargets(animator.gameObject);
		List<VFXShared.BasicEffectTyped> basicEffects = new List<VFXShared.BasicEffectTyped>();
		VFXShared.ProcessAreaEffects(AreaHexEffects, TargetHexEffects, animator, AreaEffectAlignment, CasterIsCentre, areaEffectAdjacentToTargets: false, ref hitEffectTargets, ref basicEffects, casterType);
		while (projectileInstance.transform.position != target.position)
		{
			yield return new WaitForEndOfFrame();
			projectileInstance.transform.position = Vector3.MoveTowards(projectileInstance.transform.position, target.position, projectileSpeed * Timekeeper.instance.m_GlobalClock.deltaTime);
			t += Timekeeper.instance.m_GlobalClock.deltaTime;
		}
		ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.CurrentAttackTargets[0]);
		OnSpawnedProjectileHit(new SpawnedProjectileHitEventArgs(actorBehaviour));
		if (string.IsNullOrEmpty(AbilityAudioEvent))
		{
			AudioController.Play(AbilityHitAudioEvent, Choreographer.s_Choreographer.CurrentAttackTargets[0].transform, null, attachToParent: false);
		}
		if (fadeProjectileStartOnImpact)
		{
			projectileInstance.GetComponent<ParticlesFadeScript>().fadeOnImpactMult = 0f;
		}
		float num = VFXShared.GetEffectLifetime(projectileInstance) - t;
		if (num > 0f)
		{
			ObjectPool.Recycle(projectileInstance, num, projectile);
		}
		else
		{
			ObjectPool.Recycle(projectileInstance, projectile);
		}
		firingCharacter.GetComponent<ActorEvents>().ProgressChoreographer(this);
		VFXShared.ControlChoreographer(control: false, this);
		if (hitEffect != null)
		{
			GameObject gameObject = ObjectPool.Spawn(hitEffect, null, target.position, rotation);
			ObjectPool.Recycle(gameObject, VFXShared.GetEffectLifetime(gameObject), hitEffect);
		}
		if (basicEffects.Count > 0)
		{
			VFXShared.PlayAreaEffectsFromTimeline(basicEffects);
		}
		if (hitEffectTargets.HitTimes.Count > 0)
		{
			CoroutineHelper.RunCoroutine(HitEffectTargetsCoroutine(hitEffectTargets));
		}
	}

	private IEnumerator HitEffectTargetsCoroutine(VFXShared.HitEffectTargets targets)
	{
		ActorEvents ae = targets.CurrentActor.GetComponent<ActorEvents>();
		List<VFXShared.HitTime> hitTimes = targets.HitTimes.OrderBy((VFXShared.HitTime x) => x.Time).ToList();
		float elapsedTime = 0f;
		for (int i = 0; i < hitTimes.Count; i++)
		{
			if (hitTimes[i].targets.Count > 0)
			{
				hitTimes[i].Time -= elapsedTime;
				if (hitTimes[i].Time > 0f)
				{
					yield return Timekeeper.instance.WaitForSeconds(hitTimes[i].Time);
					elapsedTime += hitTimes[i].Time;
				}
				ae.ProgressChoreographer(this);
			}
		}
	}
}
