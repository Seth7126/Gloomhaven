#define DEBUG
using System;
using Photon.Bolt.Collections;
using Photon.Bolt.Utils;
using UnityEngine;

namespace Photon.Bolt.LagCompensation;

internal class BoltHitboxBodySnapshot : BoltObject, IDisposable, IBoltListNode<BoltHitboxBodySnapshot>
{
	private static readonly ObjectPool<BoltHitboxBodySnapshot> _pool = new ObjectPool<BoltHitboxBodySnapshot>();

	private int _count = 0;

	private const int MaxSaveHitBoxes = 32;

	private Matrix4x4 _proximityWorldToLocal = Matrix4x4.identity;

	private Matrix4x4 _proximityLocalToWorld = Matrix4x4.identity;

	private readonly Matrix4x4[] _hitboxWorldToLocal = new Matrix4x4[32];

	private readonly Matrix4x4[] _hitboxLocalToWorld = new Matrix4x4[32];

	private readonly Vector3[] _hitboxScale = new Vector3[32];

	public BoltHitboxBody HitBoxBody { get; private set; } = null;

	BoltHitboxBodySnapshot IBoltListNode<BoltHitboxBodySnapshot>.prev { get; set; }

	BoltHitboxBodySnapshot IBoltListNode<BoltHitboxBodySnapshot>.next { get; set; }

	object IBoltListNode<BoltHitboxBodySnapshot>.list { get; set; }

	public static BoltHitboxBodySnapshot Create(BoltHitboxBody body)
	{
		BoltHitboxBodySnapshot boltHitboxBodySnapshot = _pool.Get();
		boltHitboxBodySnapshot.Snapshot(body);
		return boltHitboxBodySnapshot;
	}

	public void Snapshot(BoltHitboxBody body)
	{
		HitBoxBody = body;
		if (body._hitboxes.Length > 32)
		{
			BoltEntity component = body.GetComponent<BoltEntity>();
			string arg = ((component != null) ? component.NetworkId.ToString() : "Unknown");
			BoltLog.Warn("You BoltHitboxBody of Entity {0} has {1}, but it will be stored only the first {2} Hitboxes.", arg, body._hitboxes.Length, 32);
		}
		_count = Mathf.Min(body._hitboxes.Length, _hitboxWorldToLocal.Length);
		if ((bool)body._proximity)
		{
			_proximityWorldToLocal = WorldToLocalMatrixUnscaled(body._proximity.transform);
			_proximityLocalToWorld = LocalToWorldMatrixUnscaled(body._proximity.transform);
		}
		for (int i = 0; i < _count; i++)
		{
			_hitboxScale[i] = body._hitboxes[i].transform.lossyScale;
			_hitboxWorldToLocal[i] = WorldToLocalMatrixUnscaled(body._hitboxes[i].transform);
			_hitboxLocalToWorld[i] = LocalToWorldMatrixUnscaled(body._hitboxes[i].transform);
		}
	}

	public void Dispose()
	{
		HitBoxBody = null;
		_count = 0;
		_proximityWorldToLocal = Matrix4x4.identity;
		_proximityLocalToWorld = Matrix4x4.identity;
		Array.Clear(_hitboxScale, 0, _hitboxScale.Length);
		Array.Clear(_hitboxWorldToLocal, 0, _hitboxWorldToLocal.Length);
		Array.Clear(_hitboxLocalToWorld, 0, _hitboxLocalToWorld.Length);
		_pool.Return(this);
	}

	public Vector3 GetPosition()
	{
		if (!HitBoxBody)
		{
			return Vector3.zero;
		}
		if ((bool)HitBoxBody._proximity)
		{
			return _proximityLocalToWorld.MultiplyPoint(Vector3.zero);
		}
		int num = 0;
		if (num < _count)
		{
			return _hitboxLocalToWorld[num].MultiplyPoint(Vector3.zero);
		}
		return Vector3.zero;
	}

	public void OverlapSphere(Vector3 center, float radius, BoltPhysicsHits hits)
	{
		if (!HitBoxBody)
		{
			return;
		}
		if ((bool)HitBoxBody._proximity)
		{
			if (!HitBoxBody._proximity.OverlapSphere(ref _proximityWorldToLocal, center, radius))
			{
				return;
			}
			hits.AddHit(HitBoxBody, HitBoxBody._proximity, (center - _proximityLocalToWorld.MultiplyPoint(Vector3.zero)).magnitude);
		}
		for (int i = 0; i < _count; i++)
		{
			BoltHitbox boltHitbox = HitBoxBody._hitboxes[i];
			if (boltHitbox.OverlapSphere(ref _hitboxWorldToLocal[i], center, radius))
			{
				hits.AddHit(HitBoxBody, boltHitbox, (center - _hitboxLocalToWorld[i].MultiplyPoint(Vector3.zero)).magnitude);
			}
		}
	}

	public void Raycast(Vector3 origin, Vector3 direction, BoltPhysicsHits hits)
	{
		if (!HitBoxBody)
		{
			return;
		}
		float distance = float.NegativeInfinity;
		if ((bool)HitBoxBody._proximity)
		{
			Vector3 scale = HitBoxBody.transform.localScale;
			if (!HitBoxBody._proximity.Raycast(ref _proximityWorldToLocal, ref scale, origin, direction, out distance))
			{
				return;
			}
			hits.AddHit(HitBoxBody, HitBoxBody._proximity, distance);
		}
		for (int i = 0; i < _count; i++)
		{
			BoltHitbox boltHitbox = HitBoxBody._hitboxes[i];
			if (boltHitbox.Raycast(ref _hitboxWorldToLocal[i], ref _hitboxScale[i], origin, direction, out distance))
			{
				hits.AddHit(HitBoxBody, boltHitbox, distance);
			}
		}
	}

	private Matrix4x4 LocalToWorldMatrixUnscaled(Transform transform)
	{
		return Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
	}

	private Matrix4x4 WorldToLocalMatrixUnscaled(Transform transform)
	{
		return Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse;
	}

	public void Draw()
	{
		if ((bool)HitBoxBody && HitBoxBody.ShowSnapshotHistory)
		{
			if ((bool)HitBoxBody._proximity)
			{
				HitBoxBody._proximity.Draw(_proximityLocalToWorld);
			}
			for (int i = 0; i < _count; i++)
			{
				HitBoxBody._hitboxes[i].Draw(_hitboxLocalToWorld[i]);
			}
		}
	}
}
