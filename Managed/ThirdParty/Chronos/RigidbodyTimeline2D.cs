using UnityEngine;

namespace Chronos;

public class RigidbodyTimeline2D : RigidbodyTimeline<Rigidbody2D, RigidbodyTimeline2D.Snapshot>
{
	public struct Snapshot
	{
		public Vector3 position;

		public Quaternion rotation;

		public Vector2 velocity;

		public float angularVelocity;

		public float drag;

		public float angularDrag;

		public float lastPositiveTimeScale;

		public static Snapshot Lerp(Snapshot from, Snapshot to, float t)
		{
			return new Snapshot
			{
				position = Vector3.Lerp(from.position, to.position, t),
				rotation = Quaternion.Lerp(from.rotation, to.rotation, t),
				velocity = Vector2.Lerp(from.velocity, to.velocity, t),
				angularVelocity = Mathf.Lerp(from.angularVelocity, to.angularVelocity, t),
				drag = Mathf.Lerp(from.drag, to.drag, t),
				angularDrag = Mathf.Lerp(from.angularDrag, to.angularDrag, t),
				lastPositiveTimeScale = Mathf.Lerp(from.lastPositiveTimeScale, to.lastPositiveTimeScale, t)
			};
		}
	}

	private RigidbodyType2D _bodyType;

	protected float realGravityScale
	{
		get
		{
			return base.component.gravityScale;
		}
		set
		{
			base.component.gravityScale = value;
		}
	}

	protected override bool isReallyManual
	{
		get
		{
			return realBodyType == RigidbodyType2D.Kinematic;
		}
		set
		{
			realBodyType = (value ? RigidbodyType2D.Kinematic : bodyType);
		}
	}

	private RigidbodyType2D realBodyType
	{
		get
		{
			return base.component.bodyType;
		}
		set
		{
			base.component.bodyType = value;
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
			return base.component.angularVelocity * Vector3.one;
		}
		set
		{
			base.component.angularVelocity = value.x;
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

	public RigidbodyType2D bodyType
	{
		get
		{
			return _bodyType;
		}
		set
		{
			_bodyType = value;
			if (base.timeline.timeScale > 0f)
			{
				realBodyType = value;
			}
		}
	}

	protected override bool isManual => bodyType == RigidbodyType2D.Kinematic;

	public float gravityScale { get; set; }

	public Vector2 velocity
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

	public float angularVelocity
	{
		get
		{
			if (base.timeline.timeScale == 0f)
			{
				return zeroSnapshot.angularVelocity;
			}
			return realAngularVelocity.x / base.timeline.timeScale;
		}
		set
		{
			if (AssertForwardProperty("angularVelocity", Severity.Ignore))
			{
				realAngularVelocity = value * Vector3.one * base.timeline.timeScale;
			}
		}
	}

	public RigidbodyTimeline2D(Timeline timeline, Rigidbody2D component)
		: base(timeline, component)
	{
	}

	public override void CopyProperties(Rigidbody2D source)
	{
		bodyType = source.bodyType;
		gravityScale = source.gravityScale;
		source.gravityScale = 0f;
	}

	public override void FixedUpdate()
	{
		if (isReallyManual && !isManual)
		{
			realVelocity = Vector3.zero;
			realAngularVelocity = Vector3.zero;
		}
		if (!isReallyManual && base.timeline.timeScale > 0f)
		{
			velocity += Physics2D.gravity * gravityScale * base.timeline.fixedDeltaTime;
		}
	}

	protected override Snapshot LerpSnapshots(Snapshot from, Snapshot to, float t)
	{
		return Snapshot.Lerp(from, to, t);
	}

	protected override Snapshot CopySnapshot()
	{
		return new Snapshot
		{
			position = base.component.transform.position,
			rotation = base.component.transform.rotation,
			velocity = base.component.velocity,
			angularVelocity = base.component.angularVelocity,
			drag = base.component.drag,
			angularDrag = base.component.angularDrag,
			lastPositiveTimeScale = lastPositiveTimeScale
		};
	}

	protected override void ApplySnapshot(Snapshot snapshot)
	{
		base.component.transform.position = snapshot.position;
		base.component.transform.rotation = snapshot.rotation;
		if (base.timeline.timeScale > 0f)
		{
			base.component.velocity = snapshot.velocity;
			base.component.angularVelocity = snapshot.angularVelocity;
		}
		base.component.drag = snapshot.drag;
		base.component.angularDrag = snapshot.angularDrag;
		lastPositiveTimeScale = snapshot.lastPositiveTimeScale;
	}

	protected override bool IsSleeping()
	{
		return base.component.IsSleeping();
	}

	protected override void WakeUp()
	{
		base.component.WakeUp();
	}

	public void AddForce(Vector2 force, ForceMode2D mode = ForceMode2D.Force)
	{
		if (AssertForwardForce(Severity.Ignore))
		{
			base.component.AddForce(AdjustForce(force), mode);
		}
	}

	public void AddRelativeForce(Vector2 force, ForceMode2D mode = ForceMode2D.Force)
	{
		if (AssertForwardForce(Severity.Ignore))
		{
			base.component.AddRelativeForce(AdjustForce(force), mode);
		}
	}

	public void AddForceAtPosition(Vector2 force, Vector2 position, ForceMode2D mode = ForceMode2D.Force)
	{
		if (AssertForwardForce(Severity.Ignore))
		{
			base.component.AddForceAtPosition(AdjustForce(force), position, mode);
		}
	}

	public void AddTorque(float torque, ForceMode2D mode = ForceMode2D.Force)
	{
		if (AssertForwardForce(Severity.Ignore))
		{
			base.component.AddTorque(AdjustForce(torque), mode);
		}
	}
}
