using System.Collections;
using Chronos;
using UnityEngine;

public class CharacterRevealScript : MonoBehaviour
{
	private static readonly int _deathDissolveTop = Shader.PropertyToID("_DeathDissolveTop");

	private static readonly int _deathDissolveBottom = Shader.PropertyToID("_DeathDissolveBottom");

	private static readonly int _cindersDistance = Shader.PropertyToID("_CindersDistance");

	private Light[] theLights;

	private SkinnedMeshRenderer[] charMesh;

	private GameObject mainCharMesh;

	private GameObject charFX;

	private Light dirLight;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Instantiation()
	{
		mainCharMesh = base.transform.Find("Elementalist(Clone)").transform.Find("HE_Elementalist_PR(Clone)").transform.Find("HE_Elementalist").transform.Find("HE_Elementalist_Mesh").gameObject;
		float value = mainCharMesh.GetComponent<SkinnedMeshRenderer>().material.GetFloat("_DeathDissolveTop") - 1.35f;
		float value2 = mainCharMesh.GetComponent<SkinnedMeshRenderer>().material.GetFloat("_DeathDissolveBottom") - 1.35f;
		dirLight = base.transform.Find("Directional Light").GetComponent<Light>();
		charFX = base.transform.Find("Elementalist(Clone)").transform.Find("HE_Elementalist_PR(Clone)").transform.Find("Elementalist_Idle_FX").gameObject;
		charMesh = GetComponentsInChildren<SkinnedMeshRenderer>();
		SkinnedMeshRenderer[] array = charMesh;
		foreach (SkinnedMeshRenderer obj in array)
		{
			obj.material.SetFloat(_deathDissolveTop, value);
			obj.material.SetFloat(_deathDissolveBottom, value2);
			obj.material.SetFloat(_cindersDistance, 0.2f);
		}
		RevealChar();
	}

	private void RevealChar()
	{
		SkinnedMeshRenderer[] array = charMesh;
		foreach (SkinnedMeshRenderer obj in array)
		{
			obj.material.SetFloat("_Toggle_Dissolve", 1f);
			obj.material.SetFloat("_Cutout", 1f);
		}
		StartCoroutine(LightAnim());
		if (!GetComponentInChildren<SummonAppear>().enabled)
		{
			GetComponentInChildren<SummonAppear>().enabled = true;
			return;
		}
		StartCoroutine(GetComponentInChildren<SummonAppear>().Play());
		StartCoroutine(GetComponentInChildren<SummonAppear>().ParticleIdleWait());
	}

	private IEnumerator LightAnim()
	{
		float startTime = Timekeeper.instance.m_GlobalClock.time;
		while (Timekeeper.instance.m_GlobalClock.time - startTime < 1f)
		{
			dirLight.intensity += (Timekeeper.instance.m_GlobalClock.time - startTime) / 2f;
			yield return new WaitForEndOfFrame();
		}
		startTime = Timekeeper.instance.m_GlobalClock.time;
		float intVal = dirLight.intensity;
		while (Timekeeper.instance.m_GlobalClock.time - startTime < 1f)
		{
			dirLight.intensity = Mathf.Lerp(intVal, 1.44f, Timekeeper.instance.m_GlobalClock.time - startTime);
			yield return new WaitForEndOfFrame();
		}
	}
}
