#define ENABLE_LOGS
using System.Collections;
using UnityEngine;

public class VFX_TimedExhaust : MonoBehaviour
{
	public float psWaittime = 2f;

	public ParticleSystem[] psExhaustables;

	private bool routineCheck;

	private void Start()
	{
	}

	private void OnEnable()
	{
		ParticleSystem[] array = psExhaustables;
		foreach (ParticleSystem particleSystem in array)
		{
			if (particleSystem != null)
			{
				ParticleSystem.EmissionModule emission = particleSystem.emission;
				emission.enabled = true;
			}
		}
		Debug.Log("startRoutine");
		StartCoroutine(waiter());
	}

	private IEnumerator waiter()
	{
		yield return new WaitForSeconds(psWaittime);
		Debug.Log("stopEmission");
		ParticleSystem[] array = psExhaustables;
		foreach (ParticleSystem particleSystem in array)
		{
			if (particleSystem != null)
			{
				ParticleSystem.EmissionModule emission = particleSystem.emission;
				emission.enabled = false;
			}
		}
	}
}
