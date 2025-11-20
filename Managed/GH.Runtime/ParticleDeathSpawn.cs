#define ENABLE_LOGS
using System.Collections.Generic;
using Chronos;
using UnityEngine;

public class ParticleDeathSpawn : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem _particleSystem;

	private ParticleSystem.Particle[] _particles;

	private float _shortestTimeAlive = float.MaxValue;

	private List<float> _aliveParticlesRemainingTime = new List<float>();

	public GameObject spawnAsset;

	private void Awake()
	{
		if (!(_particleSystem == null))
		{
			_particles = new ParticleSystem.Particle[_particleSystem.main.maxParticles];
		}
	}

	private void Update()
	{
		TryBroadcastParticleDeath();
		if (_particleSystem == null || _particleSystem.particleCount == 0)
		{
			return;
		}
		int particles = _particleSystem.GetParticles(_particles);
		float youngestParticleTimeAlive = float.MaxValue;
		ParticleSystem.Particle[] youngestParticles = GetYoungestParticles(particles, _particles, ref youngestParticleTimeAlive);
		if (_shortestTimeAlive > youngestParticleTimeAlive)
		{
			for (int i = 0; i < youngestParticles.Length; i++)
			{
				_aliveParticlesRemainingTime.Add(youngestParticles[i].remainingLifetime);
			}
		}
		_shortestTimeAlive = youngestParticleTimeAlive;
	}

	private void TryBroadcastParticleDeath()
	{
		for (int num = _aliveParticlesRemainingTime.Count - 1; num > -1; num--)
		{
			_aliveParticlesRemainingTime[num] -= Timekeeper.instance.m_GlobalClock.deltaTime;
			if ((double)_aliveParticlesRemainingTime[num] <= 0.1)
			{
				ParticleSystem.Particle particle = _particles[num];
				Debug.Log(particle.position);
				Object.Instantiate(spawnAsset, particle.position, Quaternion.Euler(particle.rotation3D));
				_aliveParticlesRemainingTime.RemoveAt(num);
			}
		}
	}

	private ParticleSystem.Particle[] GetYoungestParticles(int numPartAlive, ParticleSystem.Particle[] particles, ref float youngestParticleTimeAlive)
	{
		List<ParticleSystem.Particle> list = new List<ParticleSystem.Particle>();
		for (int i = 0; i < numPartAlive; i++)
		{
			float num = particles[i].startLifetime - particles[i].remainingLifetime;
			if (num < youngestParticleTimeAlive)
			{
				youngestParticleTimeAlive = num;
			}
		}
		for (int j = 0; j < numPartAlive; j++)
		{
			if (particles[j].startLifetime - particles[j].remainingLifetime == youngestParticleTimeAlive)
			{
				list.Add(particles[j]);
			}
		}
		return list.ToArray();
	}

	private void Reset()
	{
		_particleSystem = GetComponent<ParticleSystem>();
	}
}
