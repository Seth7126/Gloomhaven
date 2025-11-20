using UnityEngine;

public class RFX4_ParticleLight : MonoBehaviour
{
	public float LightIntencityMultiplayer = 1f;

	public LightShadows Shadows;

	private ParticleSystem ps;

	private ParticleSystem.Particle[] particles;

	private Light[] lights;

	private int lightLimit = 20;

	private void Start()
	{
		ps = GetComponent<ParticleSystem>();
		if (!(ps == null))
		{
			ParticleSystem.MainModule main = ps.main;
			if (main.maxParticles > lightLimit)
			{
				main.maxParticles = lightLimit;
			}
			particles = new ParticleSystem.Particle[main.maxParticles];
			lights = new Light[main.maxParticles];
			for (int i = 0; i < lights.Length; i++)
			{
				GameObject gameObject = new GameObject();
				lights[i] = gameObject.AddComponent<Light>();
				lights[i].transform.parent = base.transform;
				lights[i].intensity = 0f;
				lights[i].shadows = Shadows;
			}
		}
	}

	private void Update()
	{
		if (!(ps == null))
		{
			int num = ps.GetParticles(particles);
			for (int i = 0; i < num; i++)
			{
				lights[i].gameObject.SetActive(value: true);
				lights[i].transform.position = particles[i].position;
				lights[i].color = particles[i].GetCurrentColor(ps);
				lights[i].range = particles[i].GetCurrentSize(ps);
				lights[i].intensity = (float)(int)particles[i].GetCurrentColor(ps).a / 255f * LightIntencityMultiplayer;
			}
			for (int j = num; j < particles.Length; j++)
			{
				lights[j].gameObject.SetActive(value: false);
			}
		}
	}
}
