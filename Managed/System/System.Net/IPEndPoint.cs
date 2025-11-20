using System.Globalization;
using System.Net.Sockets;

namespace System.Net;

/// <summary>Represents a network endpoint as an IP address and a port number.</summary>
[Serializable]
public class IPEndPoint : EndPoint
{
	/// <summary>Specifies the minimum value that can be assigned to the <see cref="P:System.Net.IPEndPoint.Port" /> property. This field is read-only.</summary>
	public const int MinPort = 0;

	/// <summary>Specifies the maximum value that can be assigned to the <see cref="P:System.Net.IPEndPoint.Port" /> property. The MaxPort value is set to 0x0000FFFF. This field is read-only.</summary>
	public const int MaxPort = 65535;

	private IPAddress _address;

	private int _port;

	internal const int AnyPort = 0;

	internal static IPEndPoint Any = new IPEndPoint(IPAddress.Any, 0);

	internal static IPEndPoint IPv6Any = new IPEndPoint(IPAddress.IPv6Any, 0);

	/// <summary>Gets the Internet Protocol (IP) address family.</summary>
	/// <returns>Returns <see cref="F:System.Net.Sockets.AddressFamily.InterNetwork" />.</returns>
	public override AddressFamily AddressFamily => _address.AddressFamily;

	/// <summary>Gets or sets the IP address of the endpoint.</summary>
	/// <returns>An <see cref="T:System.Net.IPAddress" /> instance containing the IP address of the endpoint.</returns>
	public IPAddress Address
	{
		get
		{
			return _address;
		}
		set
		{
			_address = value;
		}
	}

	/// <summary>Gets or sets the port number of the endpoint.</summary>
	/// <returns>An integer value in the range <see cref="F:System.Net.IPEndPoint.MinPort" /> to <see cref="F:System.Net.IPEndPoint.MaxPort" /> indicating the port number of the endpoint.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value that was specified for a set operation is less than <see cref="F:System.Net.IPEndPoint.MinPort" /> or greater than <see cref="F:System.Net.IPEndPoint.MaxPort" />. </exception>
	public int Port
	{
		get
		{
			return _port;
		}
		set
		{
			if (!TcpValidationHelpers.ValidatePortNumber(value))
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_port = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.IPEndPoint" /> class with the specified address and port number.</summary>
	/// <param name="address">The IP address of the Internet host. </param>
	/// <param name="port">The port number associated with the <paramref name="address" />, or 0 to specify any available port. <paramref name="port" /> is in host order.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="port" /> is less than <see cref="F:System.Net.IPEndPoint.MinPort" />.-or- <paramref name="port" /> is greater than <see cref="F:System.Net.IPEndPoint.MaxPort" />.-or- <paramref name="address" /> is less than 0 or greater than 0x00000000FFFFFFFF. </exception>
	public IPEndPoint(long address, int port)
	{
		if (!TcpValidationHelpers.ValidatePortNumber(port))
		{
			throw new ArgumentOutOfRangeException("port");
		}
		_port = port;
		_address = new IPAddress(address);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.IPEndPoint" /> class with the specified address and port number.</summary>
	/// <param name="address">An <see cref="T:System.Net.IPAddress" />. </param>
	/// <param name="port">The port number associated with the <paramref name="address" />, or 0 to specify any available port. <paramref name="port" /> is in host order.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="address" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="port" /> is less than <see cref="F:System.Net.IPEndPoint.MinPort" />.-or- <paramref name="port" /> is greater than <see cref="F:System.Net.IPEndPoint.MaxPort" />.-or- <paramref name="address" /> is less than 0 or greater than 0x00000000FFFFFFFF. </exception>
	public IPEndPoint(IPAddress address, int port)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (!TcpValidationHelpers.ValidatePortNumber(port))
		{
			throw new ArgumentOutOfRangeException("port");
		}
		_port = port;
		_address = address;
	}

	/// <summary>Returns the IP address and port number of the specified endpoint.</summary>
	/// <returns>A string containing the IP address and the port number of the specified endpoint (for example, 192.168.1.2:80).</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override string ToString()
	{
		return string.Format((_address.AddressFamily == AddressFamily.InterNetworkV6) ? "[{0}]:{1}" : "{0}:{1}", _address.ToString(), Port.ToString(NumberFormatInfo.InvariantInfo));
	}

	/// <summary>Serializes endpoint information into a <see cref="T:System.Net.SocketAddress" /> instance.</summary>
	/// <returns>A <see cref="T:System.Net.SocketAddress" /> instance containing the socket address for the endpoint.</returns>
	public override SocketAddress Serialize()
	{
		return new SocketAddress(Address, Port);
	}

	/// <summary>Creates an endpoint from a socket address.</summary>
	/// <returns>An <see cref="T:System.Net.EndPoint" /> instance using the specified socket address.</returns>
	/// <param name="socketAddress">The <see cref="T:System.Net.SocketAddress" /> to use for the endpoint. </param>
	/// <exception cref="T:System.ArgumentException">The AddressFamily of <paramref name="socketAddress" /> is not equal to the AddressFamily of the current instance.-or- <paramref name="socketAddress" />.Size &lt; 8. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override EndPoint Create(SocketAddress socketAddress)
	{
		if (socketAddress.Family != AddressFamily)
		{
			throw new ArgumentException(global::SR.Format("The AddressFamily {0} is not valid for the {1} end point, use {2} instead.", socketAddress.Family.ToString(), GetType().FullName, AddressFamily.ToString()), "socketAddress");
		}
		if (socketAddress.Size < 8)
		{
			throw new ArgumentException(global::SR.Format("The supplied {0} is an invalid size for the {1} end point.", socketAddress.GetType().FullName, GetType().FullName), "socketAddress");
		}
		return socketAddress.GetIPEndPoint();
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Net.IPEndPoint" /> instance.</summary>
	/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	/// <param name="comparand">The specified <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Net.IPEndPoint" /> instance.</param>
	public override bool Equals(object comparand)
	{
		if (comparand is IPEndPoint iPEndPoint && iPEndPoint._address.Equals(_address))
		{
			return iPEndPoint._port == _port;
		}
		return false;
	}

	/// <summary>Returns a hash value for a <see cref="T:System.Net.IPEndPoint" /> instance.</summary>
	/// <returns>An integer hash value.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override int GetHashCode()
	{
		return _address.GetHashCode() ^ _port;
	}
}
