using System;
using System.Linq;
using UnityEngine;

public class WeaponCollision : MonoBehaviour
{
	public class WeaponCollisionHitEventArgs : EventArgs
	{
		public ActorBehaviour TargetActorBehaviour;

		public WeaponCollisionHitEventArgs(ActorBehaviour actorBehaviour)
		{
			TargetActorBehaviour = actorBehaviour;
		}
	}

	private Collider[] _colliders;

	private Rigidbody _rigidbody;

	public static event EventHandler<WeaponCollisionHitEventArgs> WeaponCollisionHit;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody>();
		UnityEngine.Object.Destroy(_rigidbody);
		_colliders = GetComponentsInChildren<Collider>(this);
	}

	private void OnEnable()
	{
		if (_rigidbody == null)
		{
			_rigidbody = base.gameObject.AddComponent<Rigidbody>();
			_rigidbody.useGravity = false;
			_rigidbody.isKinematic = true;
		}
		Collider[] colliders = _colliders;
		for (int i = 0; i < colliders.Length; i++)
		{
			colliders[i].enabled = true;
		}
	}

	private void OnDisable()
	{
		Collider[] colliders = _colliders;
		for (int i = 0; i < colliders.Length; i++)
		{
			colliders[i].enabled = false;
		}
		if (_rigidbody != null)
		{
			UnityEngine.Object.Destroy(_rigidbody);
		}
	}

	protected virtual void OnWeaponCollisionHit(WeaponCollisionHitEventArgs e)
	{
		if (WeaponCollision.WeaponCollisionHit != null)
		{
			WeaponCollision.WeaponCollisionHit(this, e);
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (!base.enabled)
		{
			return;
		}
		CharacterManager componentInParent = base.gameObject.GetComponentInParent<CharacterManager>();
		if (componentInParent == null)
		{
			return;
		}
		CharacterManager componentInParent2 = collider.GetComponentInParent<CharacterManager>();
		if (!(componentInParent2 == null) && !componentInParent.gameObject.GetComponentsInChildren<Collider>().Contains(collider) && componentInParent2.CharacterActor != null && componentInParent.CharacterActor.Type != componentInParent2.CharacterActor.Type)
		{
			ActorBehaviour actorBehaviour = ActorBehaviour.GetActorBehaviour(collider.gameObject);
			OnWeaponCollisionHit(new WeaponCollisionHitEventArgs(actorBehaviour));
			Vector3 position = collider.ClosestPointOnBounds(base.gameObject.transform.position);
			int num = 0;
			num = ((!DebugMenu.DebugMenuNotNull || DebugMenu.Instance.AttackValueOverride == int.MaxValue) ? componentInParent2.CharacterActor.IncomingAttackDamage : DebugMenu.Instance.AttackValueOverride);
			GameObject prefab = ((num == 0) ? ((componentInParent2.ShieldHitEffect != null) ? componentInParent2.ShieldHitEffect : GlobalSettings.Instance.m_DefaultHitEffects.DefaultShieldHitEffect) : ((num < GlobalSettings.Instance.m_DefaultHitEffects.CriticalHitThreshold) ? ((componentInParent2.StandardHitEffect != null) ? componentInParent2.StandardHitEffect : GlobalSettings.Instance.m_DefaultHitEffects.DefaultStandardHitEffect) : ((componentInParent2.CriticalHitEffect != null) ? componentInParent2.CriticalHitEffect : GlobalSettings.Instance.m_DefaultHitEffects.DefaultCriticalHitEffect)));
			GameObject obj = ObjectPool.Spawn(prefab, null, position, Quaternion.LookRotation(base.gameObject.transform.forward, base.gameObject.transform.up));
			ObjectPool.Recycle(obj, VFXShared.GetEffectLifetime(obj), prefab);
			base.enabled = false;
			componentInParent.gameObject.GetComponentInChildren<ActorEvents>().ProgressChoreographer();
			Weapon component = GetComponent<Weapon>();
			if (component != null)
			{
				component.PlayHitFX(componentInParent2.m_SurfaceType);
			}
		}
	}
}
