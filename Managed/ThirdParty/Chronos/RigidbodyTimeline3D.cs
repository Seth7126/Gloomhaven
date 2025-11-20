using UnityEngine;
using UnityEngine.AI;

namespace Chronos;

public class RigidbodyTimeline3D : RigidbodyTimeline<Rigidbody, RigidbodyTimeline3D.Snapshot>
{
	public struct Snapshot
	{
		public Vector3 position;

		public Quaternion rotation;

		public Vector3 velocity;

		public Vector3 angularVelocity;

		public float drag;

		public float angularDrag;

		public float lastPositiveTimeScale;

		public static Snapshot Lerp(Snapshot from, Snapshot to, float t)
		{
			return new Snapshot
			{
				position = Vector3.Lerp(from.position, to.position, t),
				rotation = Quaternion.Lerp(from.rotation, to.rotation, t),
				velocity = Vector3.Lerp(from.velocity, to.velocity, t),
				angularVelocity = Vector3.Lerp(from.angularVelocity, to.angularVelocity, t),
				drag = Mathf.Lerp(from.drag, to.drag, t),
				angularDrag = Mathf.Lerp(from.angularDrag, to.angularDrag, t),
				lastPositiveTimeScale = Mathf.Lerp(from.lastPositiveTimeScale, to.lastPositiveTimeScale, t)
			};
		}
	}

	private bool _isKinematic;

	protected bool reallyUsesGravity
	{
		get
		{
			return base.component.useGravity;
		}
		set
		{
			base.component.useGravity = value;
		}
	}

	private bool isReallyKinematic
	{
		get
		{
			return base.component.isKinematic;
		}
		set
		{
			base.component.isKinematic = value;
		}
	}

	protected override bool isReallyManual
	{
		get
		{
			return isReallyKinematic;
		}
		set
		{
			isReallyKinematic = value;
		}
	}

	protected override float realMass
	{
		get
		{
			return base.component.mass;
		}
		set
		{
			base.component.mass = value;
		}
	}

	protected override Vector3 realVelocity
	{
		get
		{
			return base.component.velocity;
		}
		set
		{
			base.component.velocity = value;
		}
	}

	protected override Vector3 realAngularVelocity
	{
		get
		{
			return base.component.angularVelocity;
		}
		set
		{
			base.component.angularVelocity = value;
		}
	}

	protected override float realDrag
	{
		get
		{
			return base.component.drag;
		}
		set
		{
			base.component.drag = value;
		}
	}

	protected override float realAngularDrag
	{
		get
		{
			return base.component.angularDrag;
		}
		set
		{
			base.component.angularDrag = value;
		}
	}

	public bool isKinematic
	{
		get
		{
			return _isKinematic;
		}
		set
		{
			_isKinematic = value;
			if (base.timeline.timeScale > 0f)
			{
				isReallyKinematic = value;
			}
		}
	}

	protected override bool isManual => isKinematic;

	public bool useGravity { get; set; }

	public Vector3 velocity
	{
		get
		{
			if (base.timeline.timeScale == 0f)
			{
				return zeroSnapshot.velocity;
			}
			return realVelocity / base.timeline.timeScale;
		}
		set
		{
			if (AssertForwardProperty("velocity", Severity.Ignore))
			{
				realVelocity = value * base.timeline.timeScale;
			}
		}
	}

	public Vector3 angularVelocity
	{
		get
		{
			if (base.timeline.timeScale == 0f)
			{
				return zeroSnapshot.angularVelocity;
			}
			return realAngularVelocity / base.timeline.timeScale;
		}
		set
		{
			if (AssertForwardProperty("angularVelocity", Severity.Ignore))
			{
				realAngularVelocity = value * base.timeline.timeScale;
			}
		}
	}

	public RigidbodyTimeline3D(Timeline timeline, Rigidbody component)
		: base(timeline, component)
	{
	}

	public override void CopyProperties(Rigidbody source)
	{
		isKinematic = source.isKinematic;
		useGravity = source.useGravity;
		source.useGravity = false;
	}

	public override void FixedUpdate()
	{
		if (useGravity && !base.component.isKinematic && base.timeline.timeScale > 0f)
		{
			velocity += Physics.gravity * base.timeline.fixedDeltaTime;
		}
	}

	protected override Snapshot LerpSnapshots(Snapshot from, Snapshot to, float t)
	{
		return Snapshot.Lerp(from, to, t);
	}

	private NavMeshAgent GetNavMeshAgentOverride()
	{
		if (base.timeline.navMeshAgent == null || base.timeline.navMeshAgent.component == null || !base.timeline.navMeshAgent.component.enabled)
		{
			return null;
		}
		return base.timeline.navMeshAgent.component;
	}

	protected override Snapshot CopySnapshot()
	{
		NavMeshAgent navMeshAgentOverride = GetNavMeshAgentOverride();
		return new Snapshot
		{
			position = base.component.transform.position,
			rotation = base.component.transform.rotation,
			velocity = ((navMeshAgentOverride == null) ? base.component.velocity : navMeshAgentOverride.velocity),
			angularVelocity = base.component.angularVelocity,
			drag = base.component.drag,
			angularDrag = base.component.angularDrag,
			lastPositiveTimeScale = lastPositiveTimeScale
		};
	}

	protected override void ApplySnapshot(Snapshot snapshot)
	{
		NavMeshAgent navMeshAgentOverride = GetNavMeshAgentOverride();
		if (navMeshAgentOverride == null)
		{
			base.component.transform.position = snapshot.position;
		}
		else
		{
			navMeshAgentOverride.Warp(snapshot.position);
		}
		base.component.transform.rotation = snapshot.rotation;
		if (base.timeline.timeScale > 0f)
		{
			if (navMeshAgentOverride == null)
			{
				base.component.velocity = snapshot.velocity;
			}
			else
			{
				navMeshAgentOverride.velocity = snapshot.velocity;
			}
			base.component.angularVelocity = snapshot.angularVelocity;
		}
		base.component.drag = snapshot.drag;
		base.component.angularDrag = snapshot.angularDrag;
		lastPositiveTimeScale = snapshot.lastPositiveTimeScale;
	}

	protected override void WakeUp()
	{
		base.component.WakeUp();
	}

	protected override bool IsSleeping()
	{
		return base.component.IsSleeping();
	}

	public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force)
	{
		if (AssertForwardForce(Severity.Ignore))
		{
			base.component.AddForce(AdjustForce(force), mode);
		}
	}

	public void AddRelativeForce(Vector3 force, ForceMode mode = ForceMode.Force)
	{
		if (AssertForwardForce(Severity.Ignore))
		{
			base.component.AddRelativeForce(AdjustForce(force), mode);
		}
	}

	public void AddForceAtPosition(Vector3 force, Vector3 position, ForceMode mode = ForceMode.Force)
	{
		if (AssertForwardForce(Severity.Ignore))
		{
			base.component.AddForceAtPosition(AdjustForce(force), position, mode);
		}
	}

	public void AddExplosionForce(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModifier = 0f, ForceMode mode = ForceMode.Force)
	{
		if (AssertForwardForce(Severity.Ignore))
		{
			base.component.AddExplosionForce(AdjustForce(explosionForce), explosionPosition, explosionRadius, upwardsModifier, mode);
		}
	}

	public void AddTorque(Vector3 torque, ForceMode mode = ForceMode.Force)
	{
		if (AssertForwardForce(Severity.Ignore))
		{
			base.component.AddTorque(AdjustForce(torque), mode);
		}
	}

	public void AddRelativeTorque(Vector3 torque, ForceMode mode = ForceMode.Force)
	{
		if (AssertForwardForce(Severity.Ignore))
		{
			base.component.AddRelativeTorque(AdjustForce(torque), mode);
		}
	}
}
