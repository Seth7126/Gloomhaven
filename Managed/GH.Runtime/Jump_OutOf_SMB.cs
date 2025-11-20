using Chronos;
using UnityEngine;

public class Jump_OutOf_SMB : StateMachineBehaviour
{
	[AudioEventName]
	public string m_SFX;

	private Renderer[] m_Renderers;

	private float m_Fade;

	private const float c_FadeSpeed = 5f;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!string.IsNullOrEmpty(m_SFX))
		{
			AudioController.Play(m_SFX, animator.gameObject.transform);
		}
		m_Renderers = animator.gameObject.GetComponentsInChildren<Renderer>();
		GameObject gameObject = ObjectPool.Spawn(GlobalSettings.Instance.m_JumpMoveParticleEffects.PhaseArrive, null, animator.transform.position, animator.transform.rotation);
		ObjectPool.Recycle(gameObject, VFXShared.GetEffectLifetime(gameObject), GlobalSettings.Instance.m_JumpMoveParticleEffects.PhaseArrive);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		m_Fade = Mathf.Min(m_Fade + Timekeeper.instance.m_GlobalClock.deltaTime * 5f, 1f);
		Renderer[] renderers = m_Renderers;
		foreach (Renderer renderer in renderers)
		{
			if (renderer != null)
			{
				Material[] materials = renderer.materials;
				for (int j = 0; j < materials.Length; j++)
				{
					materials[j].SetFloat("_Opacity", m_Fade);
				}
			}
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		Renderer[] renderers = m_Renderers;
		foreach (Renderer renderer in renderers)
		{
			if (renderer != null)
			{
				Material[] materials = renderer.materials;
				for (int j = 0; j < materials.Length; j++)
				{
					materials[j].SetFloat("_Opacity", 1f);
				}
			}
		}
	}
}
