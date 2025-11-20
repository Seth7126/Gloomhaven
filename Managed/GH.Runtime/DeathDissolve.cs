#define ENABLE_LOGS
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityStandardAssets.Utility.EffectOnEnable;

public class DeathDissolve : MonoBehaviour
{
	public GameObject Particles;

	public bool ShowParticleEffect = true;

	public float LifeTime = 8f;

	public float initialDelay;

	public AnimationCurve Curve;

	public float AnimStrength = 1f;

	public float AnimTime = 1f;

	public SkinnedMeshRenderer CharacterMesh;

	private const string m_dissolveToggle = "_Toggle_Dissolve";

	private const string m_animProperty = "_Cutout";

	private float m_animTimeTaken;

	private GameObject m_particleInstance;

	private Renderer[] m_renderers;

	public Projector[] dissolveProjectors;

	public GameObject particlesIdle;

	public float particleIdleWaitTime;

	[AudioEventName]
	[SerializeField]
	private string m_SFXOnDissolve;

	[SerializeField]
	[Tooltip("Audio timeout work in parallel to 'Initial Delay' (they not summed up)")]
	private float m_SFXTimeout;

	private bool m_playedExternally;

	public bool enableAdditionalDissolveRenderers;

	public List<Renderer> additionalDissolveRenderers = new List<Renderer>();

	public List<string> findAdditionalDissolveRenderers = new List<string>();

	private List<Renderer> m_renderersList;

	public bool addVertexAnim;

	public AnimationCurve VertexAnim;

	private const string m_vertexAnimToggle = "_AddVertexAnim";

	private const string m_vertexAnimProperty = "_VertexAnim_Intensity";

	public static List<DeathDissolve> s_DeathDissolvesInProgress = new List<DeathDissolve>();

	public IEnumerator Play(bool cleanUpAtEnd = true)
	{
		if (!s_DeathDissolvesInProgress.Contains(this))
		{
			s_DeathDissolvesInProgress.Add(this);
		}
		CoroutineHelper.instance.StartCoroutine(ParticleIdleWait());
		CoroutineHelper.instance.StartCoroutine(PlaySFX());
		if (ShowParticleEffect)
		{
			if (!m_playedExternally)
			{
				yield return Timekeeper.instance.WaitForSeconds(initialDelay);
			}
			SkinnedMeshRenderer skinnedMeshRenderer = base.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>().FirstOrDefault((SkinnedMeshRenderer x) => x.name == CharacterMesh.name);
			m_particleInstance = ObjectPool.Spawn(Particles, base.gameObject.transform);
			ParticleSystem component = m_particleInstance.GetComponent<ParticleSystem>();
			if (component != null)
			{
				ParticleSystem.ShapeModule shape = component.shape;
				shape.skinnedMeshRenderer = skinnedMeshRenderer;
				EffectInfoProvider component2 = m_particleInstance.GetComponent<EffectInfoProvider>();
				if (component2 != null)
				{
					ParticleSystem.ShapeModule shape2 = component2.SecondParticleSystem.shape;
					shape2.skinnedMeshRenderer = skinnedMeshRenderer;
					ParticleSystem.ShapeModule shape3 = component2.FirstParticleSystem.shape;
					shape3.skinnedMeshRenderer = skinnedMeshRenderer;
				}
				component.Play();
			}
		}
		m_renderers = base.gameObject.GetComponentsInChildren<Renderer>();
		m_renderersList = m_renderers.ToList();
		if (enableAdditionalDissolveRenderers)
		{
			if (additionalDissolveRenderers != null)
			{
				foreach (Renderer additionalDissolveRenderer in additionalDissolveRenderers)
				{
					m_renderersList.Add(additionalDissolveRenderer);
				}
			}
			if (findAdditionalDissolveRenderers != null)
			{
				foreach (string findAdditionalDissolveRenderer in findAdditionalDissolveRenderers)
				{
					m_renderersList.Add(base.gameObject.FindInChildren(findAdditionalDissolveRenderer).GetComponent<Renderer>());
				}
			}
			m_renderers = m_renderersList.ToArray();
		}
		if (addVertexAnim)
		{
			Material[] materials = CharacterMesh.materials;
			foreach (Material material in materials)
			{
				if (material.HasProperty("_AddVertexAnim"))
				{
					material.SetFloat("_AddVertexAnim", 1f);
				}
			}
		}
		Renderer[] renderers = m_renderers;
		foreach (Renderer obj in renderers)
		{
			Debug.Log(obj.name);
			Material[] materials = obj.materials;
			foreach (Material material2 in materials)
			{
				if (material2.HasProperty("_Toggle_Dissolve"))
				{
					material2.SetFloat("_Toggle_Dissolve", 1f);
				}
			}
		}
		m_animTimeTaken = 0f;
		float startTime = Timekeeper.instance.m_GlobalClock.time;
		while (Timekeeper.instance.m_GlobalClock.time - startTime < LifeTime)
		{
			if (m_animTimeTaken < LifeTime)
			{
				m_animTimeTaken += Timekeeper.instance.m_GlobalClock.deltaTime / AnimTime;
				float value = Curve.Evaluate(m_animTimeTaken) * AnimStrength;
				renderers = m_renderers;
				foreach (Renderer renderer in renderers)
				{
					if (renderer != null)
					{
						Material[] materials = renderer.materials;
						for (int num2 = 0; num2 < materials.Length; num2++)
						{
							materials[num2].SetFloat("_Cutout", value);
						}
					}
				}
				if (dissolveProjectors != null)
				{
					Projector[] array = dissolveProjectors;
					for (int num = 0; num < array.Length; num++)
					{
						array[num].material.SetFloat("_Cutout", value);
					}
				}
				if (addVertexAnim)
				{
					float value2 = VertexAnim.Evaluate(m_animTimeTaken);
					Material[] materials = CharacterMesh.materials;
					for (int num = 0; num < materials.Length; num++)
					{
						materials[num].SetFloat("_VertexAnim_Intensity", value2);
					}
				}
			}
			yield return new WaitForEndOfFrame();
		}
		if (cleanUpAtEnd)
		{
			Cleanup();
		}
		else
		{
			s_DeathDissolvesInProgress.Remove(this);
		}
	}

	private void Cleanup()
	{
		base.transform.parent.gameObject.SetActive(value: false);
		if (particlesIdle != null)
		{
			particlesIdle.SetActive(value: true);
		}
		if (m_renderers != null && m_renderers.Length != 0)
		{
			Renderer[] renderers = m_renderers;
			for (int i = 0; i < renderers.Length; i++)
			{
				Material[] materials = renderers[i].materials;
				foreach (Material material in materials)
				{
					if (material.HasProperty("_Toggle_Dissolve"))
					{
						material.SetFloat("_Toggle_Dissolve", 0f);
						material.SetFloat("_Cutout", 0f);
					}
				}
			}
		}
		if (dissolveProjectors != null)
		{
			Projector[] array = dissolveProjectors;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].material.SetFloat("_Cutout", 0f);
			}
		}
		if (addVertexAnim)
		{
			Material[] materials = CharacterMesh.materials;
			foreach (Material obj in materials)
			{
				obj.SetFloat("_VertexAnim_Intensity", 0f);
				obj.SetFloat("_AddVertexAnim", 0f);
			}
		}
		if (ShowParticleEffect && m_particleInstance != null)
		{
			ObjectPool.Recycle(m_particleInstance, Particles);
		}
		CharacterManager characterManager = CharacterManager.GetCharacterManager(base.transform.parent.gameObject);
		if ((bool)characterManager)
		{
			characterManager.DeinitializeCharacter();
			if (characterManager.CharacterActor.Type == CActor.EType.Player)
			{
				characterManager.gameObject.transform.position = Choreographer.s_Choreographer.DeadPlayerLocation;
			}
			else
			{
				ObjectPool.Recycle(characterManager.gameObject, characterManager.CharacterPrefab);
			}
		}
		s_DeathDissolvesInProgress.Remove(this);
	}

	public IEnumerator ParticleIdleWait()
	{
		yield return Timekeeper.instance.WaitForSeconds(particleIdleWaitTime + (m_playedExternally ? 0f : initialDelay));
		if (particlesIdle != null)
		{
			ParticleSystem component = particlesIdle.GetComponent<ParticleSystem>();
			if (component != null)
			{
				component.Stop();
			}
		}
	}

	private IEnumerator PlaySFX()
	{
		yield return Timekeeper.instance.WaitForSeconds(m_SFXTimeout);
		if (!string.IsNullOrEmpty(m_SFXOnDissolve))
		{
			AudioController.Play(m_SFXOnDissolve, base.transform);
		}
	}

	public void ExternalPlay()
	{
		m_playedExternally = true;
		StartCoroutine(Play(cleanUpAtEnd: false));
	}

	public void ExternalCleanup()
	{
		if (m_playedExternally)
		{
			if (!s_DeathDissolvesInProgress.Contains(this))
			{
				s_DeathDissolvesInProgress.Add(this);
			}
			StopCoroutine(Play());
			Cleanup();
			m_playedExternally = false;
		}
	}

	public bool PlayedExternally()
	{
		return m_playedExternally;
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
		s_DeathDissolvesInProgress.Remove(this);
	}
}
