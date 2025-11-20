using System.Collections.Generic;
using UnityEngine;

public class LevelUseParticles : MonoBehaviour
{
	public enum TypeParticle
	{
		Environment = 1,
		Hit,
		Other,
		Props,
		Weapons,
		GUI,
		Characters
	}

	[SerializeField]
	private TypeParticle _typeParticle;

	private ParticleSystem[] _particleSystems;

	public TypeParticle CurrentTypeParticle
	{
		get
		{
			return _typeParticle;
		}
		set
		{
			_typeParticle = value;
		}
	}

	private void Awake()
	{
		List<ParticleSystem> list = new List<ParticleSystem>();
		GetComponents(list);
		GetComponentsInChildren(includeInactive: true, list);
		_particleSystems = list.ToArray();
		Clear();
	}

	private void Clear()
	{
		if (!(PlatformLayer.Setting == null))
		{
			if (_typeParticle == TypeParticle.Environment && PlatformLayer.Setting.LowParticlesUse.EnvironmentLow)
			{
				Stop();
			}
			else if (_typeParticle == TypeParticle.Hit && PlatformLayer.Setting.LowParticlesUse.HitLow)
			{
				Stop();
			}
			else if (_typeParticle == TypeParticle.Other && PlatformLayer.Setting.LowParticlesUse.OtherLow)
			{
				Stop();
			}
			else if (_typeParticle == TypeParticle.Props && PlatformLayer.Setting.LowParticlesUse.PropsLow)
			{
				Stop();
			}
			else if (_typeParticle == TypeParticle.Weapons && PlatformLayer.Setting.LowParticlesUse.WeaponsLow)
			{
				Stop();
			}
			else if (_typeParticle == TypeParticle.GUI && PlatformLayer.Setting.LowParticlesUse.GUILow)
			{
				Stop();
			}
			else if (_typeParticle == TypeParticle.Characters && PlatformLayer.Setting.LowParticlesUse.CharactersLow)
			{
				Stop();
			}
		}
	}

	private void Stop()
	{
		ParticleSystem[] particleSystems = _particleSystems;
		foreach (ParticleSystem particleSystem in particleSystems)
		{
			particleSystem.Stop();
			DeleteMaterials(particleSystem);
			Object.Destroy(particleSystem.gameObject);
		}
	}

	private void DeleteMaterials(ParticleSystem particle)
	{
		ParticleSystemRenderer component = particle.gameObject.GetComponent<ParticleSystemRenderer>();
		if (component.materials.Length > 1)
		{
			Material[] materials = component.materials;
			for (int i = 0; i < materials.Length; i++)
			{
				Object.Destroy(materials[i]);
			}
		}
		else
		{
			Object.Destroy(component.material);
		}
	}
}
