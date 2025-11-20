#define ENABLE_LOGS
using System.Collections;
using Chronos;
using UnityEngine;

public class EffectDirectorScript : MonoBehaviour
{
	public GameObject attacker;

	public string attackState;

	public GameObject receiver;

	public string receiveState;

	public GameObject effect;

	public GameObject effectLocator;

	public float effectLifetime = 3f;

	public float attackDelay = 2f;

	public float receiveDelay = 0.5f;

	public float effectDelay = 0.1f;

	public AudioSource soundSource;

	public AudioClip playSound;

	public float globalSoundDelay = 2f;

	public bool effectIsProjectile;

	public GameObject projectileTarget;

	public float projectileSpeed = 1f;

	public float projectileFadeTime = 0.5f;

	public AnimationCurve projectileFadeCurve;

	public GameObject projectileHitEffect;

	public float endDelay = 2f;

	private float t;

	private bool restartAll;

	public GameObject launchEffect;

	public GameObject buildUpEffect;

	public GameObject buildUpEffect2;

	public GameObject buildUpLocator;

	public GameObject buildUpLocator2;

	private GameObject effectInstance2;

	private GameObject effectInstance3;

	private GameObject effectInstance4;

	public float buildUpDelay;

	public float buildUpDelay2;

	private void Start()
	{
		StartCoroutine(VFXTimeline());
	}

	private void Update()
	{
		if (restartAll)
		{
			StopAllCoroutines();
			StartCoroutine(VFXTimeline());
		}
	}

	private IEnumerator ActorTimeline()
	{
		yield return Timekeeper.instance.WaitForSeconds(attackDelay);
		if ((bool)attacker)
		{
			attacker.GetComponent<Animator>().Play(attackState);
		}
		if (receiver != null)
		{
			yield return Timekeeper.instance.WaitForSeconds(receiveDelay);
			receiver.GetComponent<Animator>().Play(receiveState);
		}
	}

	private IEnumerator VFXTimeline()
	{
		restartAll = false;
		StartCoroutine(SFXTimeline());
		StartCoroutine(ActorTimeline());
		yield return Timekeeper.instance.WaitForSeconds(attackDelay);
		if ((bool)effect)
		{
			if (!effectIsProjectile)
			{
				yield return Timekeeper.instance.WaitForSeconds(effectDelay);
				Object.Destroy(Object.Instantiate(effect, effectLocator.transform), effectLifetime);
			}
			if (effectIsProjectile)
			{
				if ((bool)buildUpEffect)
				{
					yield return Timekeeper.instance.WaitForSeconds(buildUpDelay);
					effectInstance3 = Object.Instantiate(buildUpEffect, buildUpLocator.transform);
				}
				if ((bool)buildUpEffect2)
				{
					yield return Timekeeper.instance.WaitForSeconds(buildUpDelay2);
					effectInstance4 = Object.Instantiate(buildUpEffect2, buildUpLocator2.transform);
				}
				yield return Timekeeper.instance.WaitForSeconds(effectDelay);
				if ((bool)launchEffect)
				{
					effectInstance2 = Object.Instantiate(launchEffect, effectLocator.transform);
				}
				GameObject effectInstance = Object.Instantiate(effect, effectLocator.transform);
				effectInstance.transform.SetParent(null);
				while (effectInstance.transform.position != projectileTarget.transform.position)
				{
					effectInstance.transform.position = Vector3.MoveTowards(effectInstance.transform.position, projectileTarget.transform.position, projectileSpeed * Timekeeper.instance.m_GlobalClock.deltaTime);
					effectInstance.transform.rotation = Quaternion.LookRotation(projectileTarget.transform.position - effectLocator.transform.position);
					yield return new WaitForEndOfFrame();
					t += Timekeeper.instance.m_GlobalClock.deltaTime;
				}
				Debug.Log("The time is:" + t);
				t = 0f;
				Object.Destroy(Object.Instantiate(projectileHitEffect, projectileTarget.transform), 2f);
				Object.Destroy(effectInstance, effectLifetime);
				if ((bool)launchEffect)
				{
					Object.Destroy(effectInstance2, effectLifetime);
				}
				if ((bool)buildUpEffect)
				{
					Object.Destroy(effectInstance3, effectLifetime);
				}
				if ((bool)buildUpEffect2)
				{
					Object.Destroy(effectInstance4, effectLifetime);
				}
			}
		}
		yield return Timekeeper.instance.WaitForSeconds(endDelay);
		restartAll = true;
	}

	private IEnumerator SFXTimeline()
	{
		yield return Timekeeper.instance.WaitForSeconds(globalSoundDelay);
		soundSource.PlayOneShot(playSound, 1f);
	}
}
