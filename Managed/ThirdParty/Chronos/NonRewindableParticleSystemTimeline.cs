using UnityEngine;

namespace Chronos;

public class NonRewindableParticleSystemTimeline : ComponentTimeline<ParticleSystem>, IParticleSystemTimeline, IComponentTimeline<ParticleSystem>, IComponentTimeline
{
	private bool warnedRewind;

	private float _playbackSpeed;

	public float playbackSpeed
	{
		get
		{
			return _playbackSpeed;
		}
		set
		{
			_playbackSpeed = value;
			AdjustProperties();
		}
	}

	public float time
	{
		get
		{
			return base.component.time;
		}
		set
		{
			base.component.time = value;
		}
	}

	public bool enableEmission
	{
		get
		{
			return base.component.emission.enabled;
		}
		set
		{
			ParticleSystem.EmissionModule emission = base.component.emission;
			emission.enabled = value;
		}
	}

	public bool isPlaying => base.component.isPlaying;

	public bool isPaused => base.component.isPaused;

	public bool isStopped => base.component.isStopped;

	public NonRewindableParticleSystemTimeline(Timeline timeline, ParticleSystem component)
		: base(timeline, component)
	{
	}

	public void Play(bool withChildren = true)
	{
		base.component.Play(withChildren);
	}

	public void Pause(bool withChildren = true)
	{
		base.component.Pause(withChildren);
	}

	public void Stop(bool withChildren = true)
	{
		base.component.Stop(withChildren);
	}

	public bool IsAlive(bool withChildren = true)
	{
		return base.component.IsAlive(withChildren);
	}

	public override void CopyProperties(ParticleSystem source)
	{
		_playbackSpeed = source.main.simulationSpeed;
	}

	public override void AdjustProperties(float timeScale)
	{
		if (timeScale < 0f && !warnedRewind)
		{
			Debug.LogWarning("Trying to rewind a non-rewindable particle system.", base.timeline);
			warnedRewind = true;
		}
		ParticleSystem.MainModule main = base.component.main;
		main.simulationSpeed = playbackSpeed * timeScale;
	}
}
