using System;

namespace UdpKit;

public abstract class UdpSession
{
	public abstract Guid Id { get; internal set; }

	public abstract UdpSessionSource Source { get; internal set; }

	public abstract UdpEndPoint WanEndPoint { get; internal set; }

	public abstract UdpEndPoint LanEndPoint { get; internal set; }

	public abstract byte[] HostData { get; internal set; }

	public abstract object HostObject { get; set; }

	public abstract int ConnectionsMax { get; internal set; }

	public abstract int ConnectionsCurrent { get; internal set; }

	public abstract string HostName { get; internal set; }

	public abstract bool IsDedicatedServer { get; internal set; }

	public abstract bool HasWan { get; }

	public abstract bool HasLan { get; }

	public abstract UdpSession Clone();
}
