#define ENABLE_LOGS
using System;
using UnityEngine;
using UnityStandardAssets.Utility.EffectOnEnable;

public class AnimFXTrigger : MonoBehaviour
{
	public class AnimFXTriggerHitEventArgs : EventArgs
	{
		public CharacterManager[] m_Targets;

		public AnimFXTriggerHitEventArgs(CharacterManager[] targets)
		{
			m_Targets = targets;
		}
	}

	[NonSerialized]
	public ParticleSystem m_BitsEffect;

	[NonSerialized]
	public ParticleSystem m_FogEffect;

	[NonSerialized]
	public Texture2D m_ColourTex;

	private ParticleSystem.ShapeModule shapeBits;

	private ParticleSystem.ShapeModule shapeFog;

	private bool hasDiffuse = true;

	private VFXLookup vfxLookup;

	public static event EventHandler<AnimFXTriggerHitEventArgs> AnimFXTriggerHit;

	private void OnEnable()
	{
		vfxLookup = base.gameObject.GetComponent<VFXLookup>();
		if (vfxLookup == null)
		{
			Debug.LogError("AnimFXTrigger for " + base.gameObject.name + " was unable to find VFXLookup component");
		}
		if (vfxLookup.CharacterBodyMesh == null)
		{
			return;
		}
		m_BitsEffect = UnityEngine.Object.Instantiate(GlobalSettings.Instance.m_JumpMoveParticleEffects.PhaseBits, Vector3.zero, Quaternion.identity, base.transform)?.GetComponent<ParticleSystem>();
		if (!(m_BitsEffect == null))
		{
			m_BitsEffect.name = "BitsEffect";
			if (vfxLookup.CharacterBodyMesh.material.HasProperty("_Diffuse"))
			{
				hasDiffuse = true;
			}
			else
			{
				hasDiffuse = false;
			}
			shapeBits = m_BitsEffect.shape;
			shapeBits.skinnedMeshRenderer = vfxLookup.CharacterBodyMesh;
			if (hasDiffuse)
			{
				shapeBits.texture = m_ColourTex;
			}
			ParticleSystem firstParticleSystem = m_BitsEffect.GetComponent<EffectInfoProvider>().FirstParticleSystem;
			m_FogEffect = firstParticleSystem;
			shapeFog = m_FogEffect.shape;
			shapeFog.skinnedMeshRenderer = vfxLookup.CharacterBodyMesh;
			if (hasDiffuse)
			{
				shapeFog.texture = m_ColourTex;
			}
			m_FogEffect.Stop();
			m_BitsEffect.Stop();
		}
	}

	public void SetShapeTexture()
	{
		if (m_BitsEffect != null && hasDiffuse)
		{
			shapeBits.texture = m_ColourTex;
		}
		shapeFog.texture = m_ColourTex;
	}

	private void OnDisable()
	{
		if (m_BitsEffect != null)
		{
			UnityEngine.Object.Destroy(m_BitsEffect.gameObject);
			m_BitsEffect = null;
		}
		m_FogEffect = null;
		m_ColourTex = null;
		shapeBits = default(ParticleSystem.ShapeModule);
		shapeFog = default(ParticleSystem.ShapeModule);
	}

	protected virtual void OnAnimFXTriggerHit(AnimFXTriggerHitEventArgs e)
	{
		if (AnimFXTrigger.AnimFXTriggerHit != null)
		{
			AnimFXTrigger.AnimFXTriggerHit(this, e);
		}
	}

	[Obsolete]
	public void TriggerFX(UnityEngine.Object basicEffect)
	{
		Debug.LogWarning("TriggerFX: Should not be called, coroutine must be invoked instead");
	}

	[Obsolete]
	public void TriggerHitEffectTarget(UnityEngine.Object hitEffectTargetObject)
	{
		Debug.LogWarning("TriggerHitEffectTarget: Should not be called, coroutine must be invoked instead");
	}

	public void TriggerProgressChoreographer(UnityEngine.Object actorEventsObject)
	{
		(actorEventsObject as VFXShared.ActorEventsObject)?.ActorEvents?.ProgressChoreographer();
	}
}
