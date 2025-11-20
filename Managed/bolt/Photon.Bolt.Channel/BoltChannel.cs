using System;
using Photon.Bolt.Internal;

namespace Photon.Bolt.Channel;

internal abstract class BoltChannel
{
	private BoltConnection _cn;

	public BoltConnection connection
	{
		get
		{
			return _cn;
		}
		internal set
		{
			if (_cn == null)
			{
				_cn = value;
				return;
			}
			throw new InvalidOperationException();
		}
	}

	public abstract void Pack(Packet packet);

	public abstract void Read(Packet packet);

	public virtual void Lost(Packet packet)
	{
	}

	public virtual void Delivered(Packet packet)
	{
	}

	public virtual void Disconnected()
	{
	}
}
