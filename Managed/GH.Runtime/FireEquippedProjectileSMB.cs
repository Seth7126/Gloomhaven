using System;
using System.Collections;
using Chronos;
using UnityEngine;

public class FireEquippedProjectileSMB : StateMachineBehaviour
{
	public class EquippedProjectileHitEventArgs : EventArgs
	{
		public ActorBehaviour TargetActorBehaviour;

		public EquippedProjectileHitEventArgs(ActorBehaviour actorBehaviour)
		{
			TargetActorBehaviour = actorBehaviour;
		}
	}

	public float FireTime;

	public float RespawnTime;

	public GameObject Projectile;

	public float ProjectileSpeed;

	public bool FadeProjectile = true;

	public bool projectileHasPFX;

	public float FadeTime = 3f;

	public float DelayBeforeFade = 2.5f;

	[AudioEventName]
	public string FireSFXEvent;

	public GameObject ProjectileHitEffect;

	public bool hiddenOnFire;

	public bool orientOnFire;

	private SkinnedMeshRenderer ProjectileMesh;

	private Material[] projectileMats;

	private Renderer projectileRend;

	private bool fadeInProgress;

	private float fadingTime;

	private float fadeStartTime;

	public static event EventHandler<EquippedProjectileHitEventArgs> EquippedProjectileHit;

	protected virtual void OnEquippedProjectileHit(EquippedProjectileHitEventArgs e)
	{
		if (FireEquippedProjectileSMB.EquippedProjectileHit != null)
		{
			FireEquippedProjectileSMB.EquippedProjectileHit(this, e);
		}
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
		VFXShared.ControlChoreographer(control: true, this);
		GameObject gameObject = animator.gameObject.FindInChildren(Projectile.name);
		if (gameObject == null)
		{
			Debug.LogError("Error: Could not find projectile " + Projectile.name + " as a child of " + animator.gameObject.name);
			return;
		}
		if (gameObject != null)
		{
			fadeInProgress = true;
			fadingTime = 0f;
			projectileMats = new Material[0];
			projectileRend = gameObject.GetComponentInChildren<Renderer>();
			if (projectileRend != null)
			{
				projectileMats = gameObject.GetComponentInChildren<Renderer>().materials;
			}
		}
		Transform parent = gameObject.transform.parent;
		CoroutineHelper.RunCoroutine(FireCoroutine(gameObject, ProjectileSpeed, FireTime, animator.transform.parent.gameObject, FadeProjectile, FadeTime, DelayBeforeFade, FireSFXEvent, ProjectileHitEffect));
		CoroutineHelper.RunCoroutine(SpawnNewProjectileCoroutine(Projectile, parent, RespawnTime, animator.transform.parent.gameObject));
		if (hiddenOnFire)
		{
			ProjectileMesh = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		VFXShared.ControlChoreographer(control: false, this);
	}

	private IEnumerator FireCoroutine(GameObject projectileInstance, float projectileSpeed, float fireTime, GameObject firingCharacter, bool fadeProjectile, float fadeTime, float delayBeforeFade, string fireSFXEvent, GameObject hitEffect)
	{
		float t = 0f;
		yield return Timekeeper.instance.WaitForSeconds(fireTime);
		if (projectileInstance == null)
		{
			yield break;
		}
		projectileInstance.transform.SetParent(null);
		if (hiddenOnFire)
		{
			ProjectileMesh.enabled = false;
		}
		CharacterManager cmSource = CharacterManager.GetCharacterManager(firingCharacter);
		cmSource.EquippedWeapons.Remove(projectileInstance);
		CharacterManager cmTarget = CharacterManager.GetCharacterManager(Choreographer.s_Choreographer.CurrentAttackTargets[0]);
		Vector3 target = cmTarget.GetTargetZone();
		if (orientOnFire)
		{
			Quaternion rotation = Quaternion.LookRotation(target - projectileInstance.transform.position, Vector3.up);
			projectileInstance.transform.rotation = rotation;
		}
		if (!string.IsNullOrEmpty(fireSFXEvent))
		{
			AudioController.Play(fireSFXEvent, projectileInstance.transform, null, attachToParent: false);
		}
		while (projectileInstance.transform.position != target)
		{
			yield return new WaitForEndOfFrame();
			projectileInstance.transform.position = Vector3.MoveTowards(projectileInstance.transform.position, target, projectileSpeed * Timekeeper.instance.m_GlobalClock.deltaTime);
			t += Timekeeper.instance.m_GlobalClock.deltaTime;
		}
		ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(Choreographer.s_Choreographer.CurrentAttackTargets[0]);
		OnEquippedProjectileHit(new EquippedProjectileHitEventArgs(actorBehaviour));
		projectileInstance.transform.SetParent(cmTarget.TargetBoneInstance.transform);
		cmSource.EquippedWeapons.Remove(projectileInstance);
		firingCharacter.GetComponentInChildren<ActorEvents>().ProgressChoreographer(this);
		VFXShared.ControlChoreographer(control: false, this);
		Weapon component = projectileInstance.GetComponent<Weapon>();
		if (component != null)
		{
			component.PlayHitFX(cmTarget.m_SurfaceType);
		}
		if (hitEffect != null)
		{
			GameObject gameObject = ObjectPool.Spawn(hitEffect, null, projectileInstance.transform.position, projectileInstance.transform.rotation);
			ObjectPool.Recycle(gameObject, VFXShared.GetEffectLifetime(gameObject), hitEffect);
		}
		if (projectileInstance == null)
		{
			yield break;
		}
		Material[] array;
		if (fadeProjectile)
		{
			yield return Timekeeper.instance.WaitForSeconds(delayBeforeFade);
			if (projectileInstance == null)
			{
				yield break;
			}
			if (projectileHasPFX)
			{
				ParticleSystem[] componentsInChildren = projectileInstance.GetComponentsInChildren<ParticleSystem>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].Stop();
				}
			}
			fadeStartTime = Timekeeper.instance.m_GlobalClock.time;
			while (fadeInProgress)
			{
				array = projectileMats;
				foreach (Material material in array)
				{
					fadingTime = (Timekeeper.instance.m_GlobalClock.time - fadeStartTime) / fadeTime;
					if (material.HasProperty("_Opacity"))
					{
						material.SetFloat("_Opacity", 1f - fadingTime);
					}
					if (fadingTime >= fadeTime)
					{
						fadeInProgress = false;
					}
				}
				yield return new WaitForEndOfFrame();
				if (projectileInstance == null)
				{
					yield break;
				}
			}
		}
		array = projectileMats;
		foreach (Material material2 in array)
		{
			if (material2.HasProperty("_Opacity"))
			{
				material2.SetFloat("_Opacity", 1f);
			}
		}
		ObjectPool.Recycle(projectileInstance, Projectile);
	}

	private IEnumerator SpawnNewProjectileCoroutine(GameObject projectile, Transform parent, float respawnTime, GameObject firingCharacter)
	{
		yield return Timekeeper.instance.WaitForSeconds(respawnTime);
		CharacterManager characterManager = CharacterManager.GetCharacterManager(firingCharacter);
		if (characterManager != null)
		{
			characterManager.DoEquip(projectile, parent);
		}
	}
}
