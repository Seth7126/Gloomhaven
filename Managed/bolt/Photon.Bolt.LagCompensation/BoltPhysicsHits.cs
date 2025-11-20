using System;
using System.Collections.Generic;

namespace Photon.Bolt.LagCompensation;

[Documentation]
public class BoltPhysicsHits : BoltObject, IDisposable
{
	internal static readonly ObjectPool<BoltPhysicsHits> _pool = new ObjectPool<BoltPhysicsHits>();

	internal List<BoltPhysicsHit> _hits = new List<BoltPhysicsHit>();

	public int count => _hits.Count;

	public BoltPhysicsHit this[int index] => _hits[index];

	public BoltPhysicsHit GetHit(int index)
	{
		return _hits[index];
	}

	public void Dispose()
	{
		_hits.Clear();
		_pool.Return(this);
	}

	internal void AddHit(BoltHitboxBody body, BoltHitbox hitbox, float distance)
	{
		_hits.Add(new BoltPhysicsHit
		{
			body = body,
			hitbox = hitbox,
			distance = distance
		});
	}
}
