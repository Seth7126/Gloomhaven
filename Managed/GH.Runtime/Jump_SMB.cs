using System.Collections;
using System.Collections.Generic;
using Chronos;
using UnityEngine;

public class Jump_SMB : StateMachineBehaviour
{
	public enum JumpTrailCol
	{
		Brown,
		Pink,
		Red,
		Distort
	}

	public Texture2D m_DiffuseTexture;

	[AudioEventName]
	public string m_SFX;

	public JumpTrailCol JumpTrailColour;

	private List<GameObject> m_TrailBrown = new List<GameObject>();

	private GameObject m_TrailDistort;

	private VFXLookup m_VFXLookup;

	private float m_Fade;

	private bool m_JumpSMBActive;

	private const float c_FadeSpeed = 5f;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!m_JumpSMBActive)
		{
			ProcessStateEnter(animator);
		}
	}

	public void ProcessStateEnter(Animator animator)
	{
		m_JumpSMBActive = true;
		if (!string.IsNullOrEmpty(m_SFX))
		{
			AudioController.Play(m_SFX, animator.gameObject.transform);
		}
		m_VFXLookup = animator.gameObject.GetComponent<VFXLookup>();
		GameObject gameObject = null;
		if (JumpTrailColour == JumpTrailCol.Brown)
		{
			gameObject = GlobalSettings.Instance.m_JumpMoveParticleEffects.PhaseTrailBrown;
		}
		else if (JumpTrailColour == JumpTrailCol.Pink)
		{
			gameObject = GlobalSettings.Instance.m_JumpMoveParticleEffects.PhaseTrailPink;
		}
		else if (JumpTrailColour == JumpTrailCol.Distort)
		{
			gameObject = GlobalSettings.Instance.m_JumpMoveParticleEffects.PhaseTrailDistort;
		}
		else if (JumpTrailColour == JumpTrailCol.Red)
		{
			gameObject = GlobalSettings.Instance.m_JumpMoveParticleEffects.PhaseTrailRed;
		}
		AnimFXTrigger component = animator.GetComponent<AnimFXTrigger>();
		if (gameObject != null && component != null && m_VFXLookup.TrailPoints.Count > 0)
		{
			m_TrailDistort = ObjectPool.Spawn(GlobalSettings.Instance.m_JumpMoveParticleEffects.PhaseTrailDistort, m_VFXLookup.TrailPoints[0]);
			foreach (Transform trailPoint in m_VFXLookup.TrailPoints)
			{
				m_TrailBrown.Add(ObjectPool.Spawn(gameObject, trailPoint));
			}
		}
		if ((bool)m_DiffuseTexture)
		{
			component.m_ColourTex = m_DiffuseTexture;
			component.SetShapeTexture();
		}
		component.m_BitsEffect.Play();
		component.m_FogEffect.Play();
		m_Fade = 1f;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!m_JumpSMBActive)
		{
			return;
		}
		m_Fade = Mathf.Max(m_Fade - Timekeeper.instance.m_GlobalClock.deltaTime * 5f, 0f);
		if (animator.GetBool("ExitJump"))
		{
			return;
		}
		foreach (Renderer characterRenderer in m_VFXLookup.CharacterRenderers)
		{
			if (characterRenderer != null)
			{
				Material[] materials = characterRenderer.materials;
				for (int i = 0; i < materials.Length; i++)
				{
					materials[i].SetFloat("_Opacity", m_Fade);
				}
			}
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!m_JumpSMBActive)
		{
			return;
		}
		if (JumpTrailColour == JumpTrailCol.Brown)
		{
			foreach (GameObject item in m_TrailBrown)
			{
				ObjectPool.Recycle(item, VFXShared.GetEffectLifetime(item), GlobalSettings.Instance.m_JumpMoveParticleEffects.PhaseTrailBrown);
			}
		}
		else if (JumpTrailColour == JumpTrailCol.Pink)
		{
			foreach (GameObject item2 in m_TrailBrown)
			{
				ObjectPool.Recycle(item2, VFXShared.GetEffectLifetime(item2), GlobalSettings.Instance.m_JumpMoveParticleEffects.PhaseTrailPink);
			}
		}
		ObjectPool.Recycle(m_TrailDistort, GlobalSettings.Instance.m_JumpMoveParticleEffects.PhaseTrailDistort);
		AnimFXTrigger component = animator.GetComponent<AnimFXTrigger>();
		if (!(component == null))
		{
			component.m_FogEffect.Stop();
			component.m_BitsEffect.Stop();
			CoroutineHelper.RunCoroutine(StopFX(6f, component));
			m_JumpSMBActive = false;
		}
	}

	private IEnumerator StopFX(float waitTime, AnimFXTrigger animFX)
	{
		yield return Timekeeper.instance.WaitForSeconds(waitTime);
		if (animFX != null && animFX.m_BitsEffect != null && animFX.m_BitsEffect.gameObject != null)
		{
			animFX.m_BitsEffect.Stop();
		}
	}
}
