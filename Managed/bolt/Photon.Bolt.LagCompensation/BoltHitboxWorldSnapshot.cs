using System;
using Photon.Bolt.Collections;

namespace Photon.Bolt.LagCompensation;

internal class BoltHitboxWorldSnapshot : BoltObject, IDisposable, IBoltListNode<BoltHitboxWorldSnapshot>
{
	internal static readonly ObjectPool<BoltHitboxWorldSnapshot> _pool = new ObjectPool<BoltHitboxWorldSnapshot>();

	internal int _frame;

	internal BoltSingleList<BoltHitboxBodySnapshot> _bodySnapshots = new BoltSingleList<BoltHitboxBodySnapshot>();

	BoltHitboxWorldSnapshot IBoltListNode<BoltHitboxWorldSnapshot>.prev { get; set; }

	BoltHitboxWorldSnapshot IBoltListNode<BoltHitboxWorldSnapshot>.next { get; set; }

	object IBoltListNode<BoltHitboxWorldSnapshot>.list { get; set; }

	internal void Snapshot(BoltHitboxBody body)
	{
		_bodySnapshots.AddLast(BoltHitboxBodySnapshot.Create(body));
	}

	public void Dispose()
	{
		while (_bodySnapshots.count > 0)
		{
			_bodySnapshots.RemoveFirst().Dispose();
		}
		_frame = 0;
		_pool.Return(this);
	}

	public void Draw()
	{
		BoltIterator<BoltHitboxBodySnapshot> iterator = _bodySnapshots.GetIterator();
		while (iterator.Next())
		{
			iterator.val.Draw();
		}
	}
}
