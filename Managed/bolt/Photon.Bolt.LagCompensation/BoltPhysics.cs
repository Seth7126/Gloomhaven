using Photon.Bolt.Collections;
using Photon.Bolt.Internal;
using UnityEngine;

namespace Photon.Bolt.LagCompensation;

internal static class BoltPhysics
{
	private const int MaxWorldSnapshots = 60;

	private static readonly BoltDoubleList<BoltHitboxBody> _hitboxBodies = new BoltDoubleList<BoltHitboxBody>();

	private static readonly BoltDoubleList<BoltHitboxWorldSnapshot> _worldSnapshots = new BoltDoubleList<BoltHitboxWorldSnapshot>();

	internal static void RegisterBody(BoltHitboxBody body)
	{
		_hitboxBodies.AddLast(body);
	}

	internal static void UnregisterBody(BoltHitboxBody body)
	{
		_hitboxBodies.Remove(body);
	}

	internal static void SnapshotWorld()
	{
		BoltIterator<BoltHitboxBody> iterator = _hitboxBodies.GetIterator();
		BoltHitboxWorldSnapshot boltHitboxWorldSnapshot = BoltHitboxWorldSnapshot._pool.Get();
		boltHitboxWorldSnapshot._frame = BoltCore.frame;
		while (iterator.Next())
		{
			boltHitboxWorldSnapshot.Snapshot(iterator.val);
		}
		_worldSnapshots.AddLast(boltHitboxWorldSnapshot);
		while (_worldSnapshots.count > 60)
		{
			_worldSnapshots.RemoveFirst().Dispose();
		}
	}

	internal static void DrawSnapshot()
	{
		foreach (BoltHitboxWorldSnapshot worldSnapshot in _worldSnapshots)
		{
			worldSnapshot.Draw();
		}
	}

	internal static Vector3 PositionAtFrame(BoltHitboxBody hitbox, int frame)
	{
		if (TryGetSnapshot(frame, out var snapshot))
		{
			return PositionAtFrame(hitbox, snapshot);
		}
		return hitbox._proximity._center;
	}

	private static Vector3 PositionAtFrame(BoltHitboxBody hitbox, BoltHitboxWorldSnapshot snapshot)
	{
		BoltIterator<BoltHitboxBodySnapshot> iterator = snapshot._bodySnapshots.GetIterator();
		while (iterator.Next())
		{
			if (iterator.val.HitBoxBody == hitbox)
			{
				return iterator.val.GetPosition();
			}
		}
		return hitbox._proximity._center;
	}

	internal static BoltPhysicsHits Raycast(Ray ray, int frame = -1)
	{
		if (TryGetSnapshot(frame, out var snapshot))
		{
			return Raycast(ray, snapshot);
		}
		return BoltPhysicsHits._pool.Get();
	}

	private static BoltPhysicsHits Raycast(Ray ray, BoltHitboxWorldSnapshot snapshot)
	{
		BoltIterator<BoltHitboxBodySnapshot> iterator = snapshot._bodySnapshots.GetIterator();
		BoltPhysicsHits boltPhysicsHits = BoltPhysicsHits._pool.Get();
		while (iterator.Next())
		{
			iterator.val.Raycast(ray.origin, ray.direction, boltPhysicsHits);
		}
		return boltPhysicsHits;
	}

	internal static BoltPhysicsHits OverlapSphere(Vector3 origin, float radius, int frame = -1)
	{
		if (TryGetSnapshot(frame, out var snapshot))
		{
			return OverlapSphere(origin, radius, snapshot);
		}
		return BoltPhysicsHits._pool.Get();
	}

	private static BoltPhysicsHits OverlapSphere(Vector3 origin, float radius, BoltHitboxWorldSnapshot snapshot)
	{
		BoltIterator<BoltHitboxBodySnapshot> iterator = snapshot._bodySnapshots.GetIterator();
		BoltPhysicsHits boltPhysicsHits = BoltPhysicsHits._pool.Get();
		while (iterator.Next())
		{
			iterator.val.OverlapSphere(origin, radius, boltPhysicsHits);
		}
		return boltPhysicsHits;
	}

	private static bool TryGetSnapshot(int frame, out BoltHitboxWorldSnapshot snapshot)
	{
		if (frame >= 0)
		{
			BoltIterator<BoltHitboxWorldSnapshot> iterator = _worldSnapshots.GetIterator();
			while (iterator.Next())
			{
				if (iterator.val._frame == frame)
				{
					snapshot = iterator.val;
					return true;
				}
			}
		}
		if (_worldSnapshots.count > 0)
		{
			snapshot = _worldSnapshots.last;
			return true;
		}
		snapshot = null;
		return false;
	}
}
