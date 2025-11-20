using System;
using UdpKit;

namespace Photon.Bolt.Internal;

public class PooledProtocolTokenBase : IProtocolToken, IDisposable
{
	internal ObjectPool<PooledProtocolTokenBase> Pool;

	internal bool IsPooled;

	public virtual void Read(UdpPacket packet)
	{
		throw new NotImplementedException();
	}

	public virtual void Write(UdpPacket packet)
	{
		throw new NotImplementedException();
	}

	public virtual void Reset()
	{
		throw new NotImplementedException();
	}

	private void Dispose(bool disposing)
	{
		if (!IsPooled)
		{
			if (disposing && Pool != null)
			{
				Reset();
				Pool.Return(this);
			}
			IsPooled = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
