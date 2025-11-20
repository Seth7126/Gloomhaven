using System.Collections;
using Chronos;
using UnityEngine;

public class BreakableObject : MonoBehaviour
{
	private Renderer[] Breakables;

	private Material[][] BreakMats;

	public ParticleSystem p_breakeffect;

	public bool ManualMeshOverride;

	public Renderer[] OverrideMeshes;

	public bool debugBreak;

	public float debugWaitTime;

	private bool animCutoutNotOpacity;

	private static readonly int _toggleDissolve = Shader.PropertyToID("_ToggleDissolve");

	private static readonly int _opacity = Shader.PropertyToID("_Opacity");

	private void Start()
	{
		if (!ManualMeshOverride)
		{
			Breakables = GetComponentsInChildren<Renderer>();
		}
		else
		{
			Breakables = OverrideMeshes;
		}
		BreakMats = new Material[Breakables.Length][];
		for (int i = 0; i < Breakables.Length; i++)
		{
			BreakMats[i] = Breakables[i].GetComponent<Renderer>().materials;
			for (int j = 0; j < BreakMats[i].Length; j++)
			{
				BreakMats[i][j].SetFloat(_toggleDissolve, 1f);
			}
		}
	}

	private void Update()
	{
		if (debugBreak)
		{
			BreakObject();
		}
	}

	public void BreakObject()
	{
		StartCoroutine(BreakTimeline());
	}

	public IEnumerator BreakTimeline()
	{
		if (debugBreak)
		{
			debugBreak = false;
		}
		else
		{
			debugWaitTime = 0f;
		}
		yield return Timekeeper.instance.WaitForSeconds(debugWaitTime);
		if (p_breakeffect != null)
		{
			Object.Instantiate(p_breakeffect, base.transform.position, base.transform.rotation, base.transform);
		}
		float startTime = Timekeeper.instance.m_GlobalClock.time;
		float animTime = 1f;
		while (Timekeeper.instance.m_GlobalClock.time - startTime < animTime)
		{
			float num = Timekeeper.instance.m_GlobalClock.time - startTime;
			for (int i = 0; i < Breakables.Length; i++)
			{
				for (int j = 0; j < BreakMats[i].Length; j++)
				{
					if (animCutoutNotOpacity)
					{
						BreakMats[i][j].SetFloat(_opacity, 1f - num / animTime);
					}
					else
					{
						BreakMats[i][j].SetFloat(_opacity, 1f - num / animTime);
					}
				}
			}
			yield return new WaitForEndOfFrame();
		}
	}
}
