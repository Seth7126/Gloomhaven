using System;
using System.Collections;
using Chronos;
using UnityEngine;

public class ParticlesFadeScript : MonoBehaviour
{
	public float fadeTime;

	public ParticleSystem[] fadeParticles;

	public ParticleSystem[] fadeParticlesOpacityValue;

	public ParticleSystem[] fadeParticleTrails;

	public AnimationCurve FadeCurve;

	public float startTime;

	[HideInInspector]
	public float fadeOnImpactMult;

	public Light particleLight;

	public bool StartOnHit;

	private const string fadeProperty = "_Opac";

	public bool alsoFade_Opacity;

	private float timePassed;

	private float val;

	private float animTime;

	private Coroutine fadeCoroutine;

	private bool isCoroutineRunning;

	private void Awake()
	{
		ParticleSystem[] array = fadeParticleTrails;
		foreach (ParticleSystem particleSystem in array)
		{
			if (!(particleSystem == null))
			{
				ParticleSystemRenderer component = particleSystem.gameObject.GetComponent<ParticleSystemRenderer>();
				if (component != null)
				{
					Material trailMaterial = new Material(component.trailMaterial);
					component.trailMaterial = trailMaterial;
				}
			}
		}
	}

	private void OnEnable()
	{
		if (!StartOnHit)
		{
			fadeCoroutine = StartCoroutine(Fade());
		}
		else
		{
			FireSpawnedProjectileSMB.SpawnedProjectileHit += HandleOnSpawnedProjectileHit;
		}
	}

	private void OnDisable()
	{
		if (isCoroutineRunning)
		{
			StopCoroutine(fadeCoroutine);
		}
		for (int i = 0; i < fadeParticles.Length; i++)
		{
			if (fadeParticles[i] != null)
			{
				fadeParticles[i].gameObject.GetComponent<ParticleSystemRenderer>().sharedMaterial.SetFloat("_Opac", 1f);
			}
		}
		for (int j = 0; j < fadeParticleTrails.Length; j++)
		{
			if (fadeParticleTrails[j] != null)
			{
				fadeParticleTrails[j].gameObject.GetComponent<ParticleSystemRenderer>().trailMaterial.SetFloat("_Opac", 1f);
			}
		}
		if (StartOnHit)
		{
			FireSpawnedProjectileSMB.SpawnedProjectileHit -= HandleOnSpawnedProjectileHit;
		}
	}

	private void HandleOnSpawnedProjectileHit(object sender, EventArgs e)
	{
		fadeCoroutine = StartCoroutine(Fade());
	}

	private IEnumerator Fade()
	{
		isCoroutineRunning = true;
		fadeOnImpactMult = 1f;
		timePassed = 0f;
		if (startTime > 0f)
		{
			yield return Timekeeper.instance.WaitForSeconds(startTime);
			timePassed = startTime;
		}
		while (timePassed <= fadeTime + startTime * fadeOnImpactMult)
		{
			timePassed += Timekeeper.instance.m_GlobalClock.deltaTime;
			if ((bool)particleLight)
			{
				particleLight.GetComponent<Light>().intensity = 0f;
			}
			animTime = timePassed / fadeTime;
			val = FadeCurve.Evaluate(animTime);
			for (int i = 0; i < fadeParticles.Length; i++)
			{
				if (fadeParticles[i] != null)
				{
					fadeParticles[i].gameObject.GetComponent<ParticleSystemRenderer>().material.SetFloat("_Opac", val * val);
					if (alsoFade_Opacity)
					{
						fadeParticles[i].gameObject.GetComponent<ParticleSystemRenderer>().material.SetFloat("_Opacity", val * val);
					}
				}
			}
			for (int j = 0; j < fadeParticleTrails.Length; j++)
			{
				if (fadeParticleTrails[j] != null)
				{
					fadeParticleTrails[j].gameObject.GetComponent<ParticleSystemRenderer>().trailMaterial.SetFloat("_Opac", val);
				}
			}
			yield return new WaitForEndOfFrame();
		}
		isCoroutineRunning = false;
	}
}
