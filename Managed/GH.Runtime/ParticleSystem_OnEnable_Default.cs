using UnityEngine;

public class ParticleSystem_OnEnable_Default : EffectOnEnable
{
	private void OnEnable()
	{
		if (GetComponentInParent<ObjectPool>() == null)
		{
			ParticleSystem component = base.gameObject.GetComponent<ParticleSystem>();
			if (component != null)
			{
				component.Play();
			}
			ParticleSystem[] componentsInChildren = base.gameObject.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Play();
			}
		}
	}
}
