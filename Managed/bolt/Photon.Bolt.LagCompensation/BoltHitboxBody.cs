using Photon.Bolt.Collections;
using UnityEngine;

namespace Photon.Bolt.LagCompensation;

[Documentation]
public class BoltHitboxBody : MonoBehaviour, IBoltListNode<BoltHitboxBody>
{
	[SerializeField]
	internal bool ShowSnapshotHistory;

	[SerializeField]
	internal BoltHitbox _proximity;

	[SerializeField]
	internal BoltHitbox[] _hitboxes = new BoltHitbox[0];

	BoltHitboxBody IBoltListNode<BoltHitboxBody>.prev { get; set; }

	BoltHitboxBody IBoltListNode<BoltHitboxBody>.next { get; set; }

	object IBoltListNode<BoltHitboxBody>.list { get; set; }

	public BoltHitbox proximity
	{
		get
		{
			return _proximity;
		}
		set
		{
			_proximity = value;
		}
	}

	public BoltHitbox[] hitboxes
	{
		get
		{
			return _hitboxes;
		}
		set
		{
			_hitboxes = value;
		}
	}

	private void OnEnable()
	{
		BoltPhysics.RegisterBody(this);
	}

	private void OnDisable()
	{
		BoltPhysics.UnregisterBody(this);
	}
}
