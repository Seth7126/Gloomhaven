#define ENABLE_LOGS
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using UnityEngine;

public class TeleportCharacterScript : MonoBehaviour
{
	private static readonly int _cindersColour = Shader.PropertyToID("_CindersColour");

	private static readonly int _toggleFlipDissolveDirection = Shader.PropertyToID("_Toggle_FlipDissolveDirection");

	private static readonly int _toggleDissolve = Shader.PropertyToID("_Toggle_Dissolve");

	private static readonly int _cutout = Shader.PropertyToID("_Cutout");

	private int skinMeshCount;

	public Color TeleportColour = new Color(1f, 1f, 1f, 1f);

	public float teleportDelay = 1f;

	public float animTime = 1f;

	public AnimationCurve FadeCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public float particleIdleWaitTime = 1f;

	public bool teleportOut = true;

	public bool ToggleDissolveBackOff;

	public GameObject MeshEmitter;

	public Texture2D EmitterMeshTexture;

	private Color[][] m_SkinnedMeshesCol;

	private GameObject m_particleInstance;

	private SkinnedMeshRenderer m_EmitterMesh;

	private SkinnedMeshRenderer[] m_SkinnedMeshes;

	private Material[][] m_SkinnedMeshMaterials;

	private VFXLookup m_VFXLookup;

	private GameObject particlesIdle;

	private void Start()
	{
		Initialize();
	}

	private void Update()
	{
	}

	private void Initialize()
	{
		GameObject[] array = base.transform.GetComponentInParent<CharacterManager>().EquippedWeapons.ToArray();
		List<SkinnedMeshRenderer> list = new List<SkinnedMeshRenderer>();
		m_VFXLookup = base.transform.GetComponentInParent<VFXLookup>();
		particlesIdle = base.transform.GetComponentInParent<DeathDissolve>().particlesIdle;
		int num = m_VFXLookup.CharacterRenderers.Count((Renderer x) => x is SkinnedMeshRenderer);
		m_SkinnedMeshes = new SkinnedMeshRenderer[num];
		int num2 = 0;
		foreach (Renderer characterRenderer in m_VFXLookup.CharacterRenderers)
		{
			if (characterRenderer is SkinnedMeshRenderer skinnedMeshRenderer)
			{
				m_SkinnedMeshes[num2] = skinnedMeshRenderer;
				num2++;
			}
		}
		for (int num3 = 0; num3 < array.Length; num3++)
		{
			SkinnedMeshRenderer componentInChildren = array[num3].GetComponentInChildren<SkinnedMeshRenderer>();
			if (componentInChildren != null)
			{
				list.Add(componentInChildren);
			}
		}
		m_SkinnedMeshes = m_SkinnedMeshes.Concat(list).ToArray();
		skinMeshCount = m_SkinnedMeshes.Length;
		m_SkinnedMeshMaterials = new Material[skinMeshCount][];
		m_SkinnedMeshesCol = new Color[skinMeshCount][];
		for (int num4 = 0; num4 < skinMeshCount; num4++)
		{
			m_SkinnedMeshMaterials[num4] = new Material[m_SkinnedMeshes[num4].materials.Length];
			m_SkinnedMeshesCol[num4] = new Color[m_SkinnedMeshes[num4].materials.Length];
			for (int num5 = 0; num5 < m_SkinnedMeshMaterials[num4].Length; num5++)
			{
				m_SkinnedMeshMaterials[num4][num5] = m_SkinnedMeshes[num4].materials[num5];
				m_SkinnedMeshesCol[num4][num5] = m_SkinnedMeshes[num4].materials[num5].GetColor(_cindersColour);
				m_SkinnedMeshes[num4].materials[num5].SetColor(_cindersColour, TeleportColour);
				m_SkinnedMeshes[num4].materials[num5].SetFloat(_toggleFlipDissolveDirection, 1f);
				m_SkinnedMeshes[num4].materials[num5].SetFloat(_toggleDissolve, 1f);
			}
		}
		Teleport();
	}

	private void Teleport()
	{
		m_EmitterMesh = m_VFXLookup.CharacterBodyMesh;
		m_particleInstance = ObjectPool.Spawn(MeshEmitter, base.gameObject.transform);
		ParticleSystem component = m_particleInstance.GetComponent<ParticleSystem>();
		if (component != null)
		{
			ParticleSystem.ShapeModule shape = component.shape;
			shape.skinnedMeshRenderer = m_EmitterMesh;
			shape.texture = EmitterMeshTexture;
			component.Play();
		}
		if (particlesIdle != null)
		{
			if (teleportOut)
			{
				StartCoroutine(ParticleIdleWaitOut());
			}
			else
			{
				StartCoroutine(ParticleIdleWaitIn());
			}
		}
		StartCoroutine(TeleportOutTimeline());
	}

	public IEnumerator ParticleIdleWaitOut()
	{
		yield return Timekeeper.instance.WaitForSeconds(particleIdleWaitTime);
		ParticleSystem component = particlesIdle.GetComponent<ParticleSystem>();
		if (component != null)
		{
			component.Stop();
		}
		Debug.Log("TeleOut");
	}

	public IEnumerator ParticleIdleWaitIn()
	{
		yield return Timekeeper.instance.WaitForSeconds(particleIdleWaitTime);
		ParticleSystem component = particlesIdle.GetComponent<ParticleSystem>();
		if (component != null)
		{
			component.Stop();
		}
		Debug.Log("TeleIn");
	}

	public IEnumerator TeleportOutTimeline()
	{
		yield return Timekeeper.instance.WaitForSeconds(teleportDelay);
		float dTime = 0f;
		float startTime = Timekeeper.instance.m_GlobalClock.time;
		while (Timekeeper.instance.m_GlobalClock.time - startTime < animTime)
		{
			dTime += Timekeeper.instance.m_GlobalClock.deltaTime / animTime;
			for (int i = 0; i < skinMeshCount; i++)
			{
				for (int j = 0; j < m_SkinnedMeshMaterials[i].Length; j++)
				{
					m_SkinnedMeshMaterials[i][j].SetFloat(_cutout, FadeCurve.Evaluate(dTime));
				}
			}
			yield return new WaitForEndOfFrame();
		}
		CleanUp();
	}

	private void CleanUp()
	{
		Debug.Log("Cleanup started");
		if (particlesIdle != null)
		{
			particlesIdle.SetActive(value: true);
		}
		for (int i = 0; i < skinMeshCount; i++)
		{
			for (int j = 0; j < m_SkinnedMeshMaterials[i].Length; j++)
			{
				m_SkinnedMeshMaterials[i][j] = m_SkinnedMeshes[i].materials[j];
				m_SkinnedMeshes[i].materials[j].SetColor(_cindersColour, m_SkinnedMeshesCol[i][j]);
				m_SkinnedMeshes[i].materials[j].SetFloat(_toggleFlipDissolveDirection, 0f);
				if (ToggleDissolveBackOff)
				{
					m_SkinnedMeshes[i].materials[j].SetFloat(_toggleDissolve, 0f);
				}
			}
		}
	}
}
