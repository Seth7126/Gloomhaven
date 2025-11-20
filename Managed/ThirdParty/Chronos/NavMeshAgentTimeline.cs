using UnityEngine;
using UnityEngine.AI;

namespace Chronos;

public class NavMeshAgentTimeline : ComponentTimeline<NavMeshAgent>
{
	private float _speed;

	private float _angularSpeed;

	public float speed
	{
		get
		{
			return _speed;
		}
		set
		{
			_speed = value;
			AdjustProperties();
		}
	}

	public float angularSpeed
	{
		get
		{
			return _angularSpeed;
		}
		set
		{
			_angularSpeed = value;
			AdjustProperties();
		}
	}

	public NavMeshAgentTimeline(Timeline timeline, NavMeshAgent component)
		: base(timeline, component)
	{
	}

	public override void Update()
	{
		if (base.timeline.lastTimeScale > 0f && base.timeline.timeScale == 0f)
		{
			base.component.velocity = Vector3.zero;
		}
	}

	public override void CopyProperties(NavMeshAgent source)
	{
		_speed = source.speed;
		_angularSpeed = source.angularSpeed;
	}

	public override void AdjustProperties(float timeScale)
	{
		base.component.speed = speed * timeScale;
		base.component.angularSpeed = angularSpeed * timeScale;
	}
}
