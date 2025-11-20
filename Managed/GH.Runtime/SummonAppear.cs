using System.Collections;
using System.Linq;
using Chronos;
using UnityEngine;
using UnityStandardAssets.Utility.EffectOnEnable;

public class SummonAppear : MonoBehaviour
{
	public GameObject Particles;

	public bool ShowParticleEffect = true;

	public float Lifetime = 5f;

	public float initialDelay;

	public float particleDelay;

	public AnimationCurve Curve;

	public float AnimStrength = 1f;

	public float AnimTime = 1f;

	public SkinnedMeshRenderer CharacterMesh;

	private const string m_dissolveToggle = "_Toggle_Dissolve";

	private const string m_animProperty = "_Cutout";

	private float m_animTimeTaken;

	private GameObject m_particleInstance;

	private SkinnedMeshRenderer[] m_renderers;

	private bool playMe = true;

	public GameObject ParticlesIdle;

	public float particleIdleWaitTime;

	[AudioEventName]
	[SerializeField]
	private string m_SFXOnAppear;

	[SerializeField]
	[Tooltip("Audio timeout work in parallel to 'Initial Delay' (they not summed up)")]
	private float m_SFXTimeout;

	private void Awake()
	{
		base.enabled = false;
	}

	private void OnDisable()
	{
		base.enabled = false;
	}

	private void OnEnable()
	{
		m_renderers = base.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
		SkinnedMeshRenderer[] renderers = m_renderers;
		for (int i = 0; i < renderers.Length; i++)
		{
			Material[] materials = renderers[i].materials;
			foreach (Material material in materials)
			{
				if (material.HasProperty("_Toggle_Dissolve"))
				{
					material.SetFloat("_Toggle_Dissolve", 1f);
				}
				material.SetFloat("_Cutout", 1f);
			}
		}
		if (ShowParticleEffect)
		{
			SkinnedMeshRenderer skinnedMeshRenderer = base.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>().FirstOrDefault((SkinnedMeshRenderer x) => x.name == CharacterMesh.name);
			m_particleInstance = ObjectPool.Spawn(Particles, base.gameObject.transform, new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 1f));
			ParticleSystem component = m_particleInstance.GetComponent<ParticleSystem>();
			if (component != null)
			{
				ParticleSystem.ShapeModule shape = component.shape;
				shape.skinnedMeshRenderer = skinnedMeshRenderer;
				EffectInfoProvider component2 = component.GetComponent<EffectInfoProvider>();
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
		StartCoroutine(Play());
		if ((bool)ParticlesIdle)
		{
			StartCoroutine(ParticleIdleWait());
		}
		StartCoroutine(PlaySFX());
	}

	private IEnumerator PlaySFX()
	{
		if (m_SFXTimeout > 0f)
		{
			yield return Timekeeper.instance.WaitForSeconds(m_SFXTimeout);
		}
		if (!string.IsNullOrEmpty(m_SFXOnAppear))
		{
			AudioController.Play(m_SFXOnAppear, base.transform);
		}
	}

	public IEnumerator ParticleIdleWait()
	{
		if (particleIdleWaitTime + initialDelay > 0f)
		{
			ParticlesIdle.SetActive(value: false);
			yield return Timekeeper.instance.WaitForSeconds(particleIdleWaitTime + initialDelay);
		}
		ParticlesIdle.SetActive(value: true);
		yield return null;
	}

	public IEnumerator Play()
	{
		m_animTimeTaken = 0f;
		float startTime = Timekeeper.instance.m_GlobalClock.time;
		SkinnedMeshRenderer[] renderers;
		while (Timekeeper.instance.m_GlobalClock.time - startTime < Lifetime)
		{
			if (m_animTimeTaken < Lifetime)
			{
				m_animTimeTaken += Timekeeper.instance.m_GlobalClock.deltaTime / AnimTime;
				float value = Curve.Evaluate(m_animTimeTaken) * AnimStrength;
				renderers = m_renderers;
				foreach (Renderer renderer in renderers)
				{
					if (renderer != null)
					{
						Material[] materials = renderer.materials;
						for (int j = 0; j < materials.Length; j++)
						{
							materials[j].SetFloat("_Cutout", value);
						}
					}
				}
			}
			yield return new WaitForEndOfFrame();
		}
		renderers = m_renderers;
		for (int i = 0; i < renderers.Length; i++)
		{
			Material[] materials = renderers[i].materials;
			foreach (Material material in materials)
			{
				if (material.HasProperty("_Toggle_Dissolve"))
				{
					material.SetFloat("_Toggle_Dissolve", 0f);
				}
			}
		}
	}
}
