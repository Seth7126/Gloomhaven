using System.Buffers.Binary;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Net;

/// <summary>Provides an Internet Protocol (IP) address.</summary>
[Serializable]
public class IPAddress
{
	private sealed class ReadOnlyIPAddress : IPAddress
	{
		public ReadOnlyIPAddress(long newAddress)
			: base(newAddress)
		{
		}
	}

	/// <summary>Provides an IP address that indicates that the server must listen for client activity on all network interfaces. This field is read-only.</summary>
	public static readonly IPAddress Any = new ReadOnlyIPAddress(0L);

	/// <summary>Provides the IP loopback address. This field is read-only.</summary>
	public static readonly IPAddress Loopback = new ReadOnlyIPAddress(16777343L);

	/// <summary>Provides the IP broadcast address. This field is read-only.</summary>
	public static readonly IPAddress Broadcast = new ReadOnlyIPAddress(4294967295L);

	/// <summary>Provides an IP address that indicates that no network interface should be used. This field is read-only.</summary>
	public static readonly IPAddress None = Broadcast;

	internal const long LoopbackMask = 255L;

	/// <summary>The <see cref="M:System.Net.Sockets.Socket.Bind(System.Net.EndPoint)" /> method uses the <see cref="F:System.Net.IPAddress.IPv6Any" /> field to indicate that a <see cref="T:System.Net.Sockets.Socket" /> must listen for client activity on all network interfaces.</summary>
	public static readonly IPAddress IPv6Any = new IPAddress(new byte[16], 0L);

	/// <summary>Provides the IP loopback address. This property is read-only.</summary>
	public static readonly IPAddress IPv6Loopback = new IPAddress(new byte[16]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 1
	}, 0L);

	/// <summary>Provides an IP address that indicates that no network interface should be used. This property is read-only.</summary>
	public static readonly IPAddress IPv6None = new IPAddress(new byte[16], 0L);

	private uint _addressOrScopeId;

	private readonly ushort[] _numbers;

	private string _toString;

	private int _hashCode;

	internal const int NumberOfLabels = 8;

	private bool IsIPv4 => _numbers == null;

	private bool IsIPv6 => _numbers != null;

	private uint PrivateAddress
	{
		get
		{
			return _addressOrScopeId;
		}
		set
		{
			_toString = null;
			_hashCode = 0;
			_addressOrScopeId = value;
		}
	}

	private uint PrivateScopeId
	{
		get
		{
			return _addressOrScopeId;
		}
		set
		{
			_toString = null;
			_hashCode = 0;
			_addressOrScopeId = value;
		}
	}

	/// <summary>Gets the address family of the IP address.</summary>
	/// <returns>Returns <see cref="F:System.Net.Sockets.AddressFamily.InterNetwork" /> for IPv4 or <see cref="F:System.Net.Sockets.AddressFamily.InterNetworkV6" /> for IPv6.</returns>
	public AddressFamily AddressFamily
	{
		get
		{
			if (!IsIPv4)
			{
				return AddressFamily.InterNetworkV6;
			}
			return AddressFamily.InterNetwork;
		}
	}

	/// <summary>Gets or sets the IPv6 address scope identifier.</summary>
	/// <returns>A long integer that specifies the scope of the address.</returns>
	/// <exception cref="T:System.Net.Sockets.SocketException">AddressFamily = InterNetwork. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="scopeId" /> &lt; 0- or -<paramref name="scopeId" /> &gt; 0x00000000FFFFFFFF  </exception>
	public long ScopeId
	{
		get
		{
			if (IsIPv4)
			{
				throw new SocketException(SocketError.OperationNotSupported);
			}
			return PrivateScopeId;
		}
		set
		{
			if (IsIPv4)
			{
				throw new SocketException(SocketError.OperationNotSupported);
			}
			if (value < 0 || value > uint.MaxValue)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			PrivateScopeId = (uint)value;
		}
	}

	/// <summary>Gets whether the address is an IPv6 multicast global address.</summary>
	/// <returns>true if the IP address is an IPv6 multicast global address; otherwise, false.</returns>
	public bool IsIPv6Multicast
	{
		get
		{
			if (IsIPv6)
			{
				return (_numbers[0] & 0xFF00) == 65280;
			}
			return false;
		}
	}

	/// <summary>Gets whether the address is an IPv6 link local address.</summary>
	/// <returns>true if the IP address is an IPv6 link local address; otherwise, false.</returns>
	public bool IsIPv6LinkLocal
	{
		get
		{
			if (IsIPv6)
			{
				return (_numbers[0] & 0xFFC0) == 65152;
			}
			return false;
		}
	}

	/// <summary>Gets whether the address is an IPv6 site local address.</summary>
	/// <returns>true if the IP address is an IPv6 site local address; otherwise, false.</returns>
	public bool IsIPv6SiteLocal
	{
		get
		{
			if (IsIPv6)
			{
				return (_numbers[0] & 0xFFC0) == 65216;
			}
			return false;
		}
	}

	/// <summary>Gets whether the address is an IPv6 Teredo address.</summary>
	/// <returns>true if the IP address is an IPv6 Teredo address; otherwise, false.</returns>
	public bool IsIPv6Teredo
	{
		get
		{
			if (IsIPv6 && _numbers[0] == 8193)
			{
				return _numbers[1] == 0;
			}
			return false;
		}
	}

	/// <summary>Gets whether the IP address is an IPv4-mapped IPv6 address.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the IP address is an IPv4-mapped IPv6 address; otherwise, false.</returns>
	public bool IsIPv4MappedToIPv6
	{
		get
		{
			if (IsIPv4)
			{
				return false;
			}
			for (int i = 0; i < 5; i++)
			{
				if (_numbers[i] != 0)
				{
					return false;
				}
			}
			return _numbers[5] == ushort.MaxValue;
		}
	}

	/// <summary>An Internet Protocol (IP) address.</summary>
	/// <returns>The long value of the IP address.</returns>
	/// <exception cref="T:System.Net.Sockets.SocketException">The address family is <see cref="F:System.Net.Sockets.AddressFamily.InterNetworkV6" />. </exception>
	[Obsolete("This property has been deprecated. It is address family dependent. Please use IPAddress.Equals method to perform comparisons. https://go.microsoft.com/fwlink/?linkid=14202")]
	public long Address
	{
		get
		{
			if (AddressFamily == AddressFamily.InterNetworkV6)
			{
				throw new SocketException(SocketError.OperationNotSupported);
			}
			return PrivateAddress;
		}
		set
		{
			if (AddressFamily == AddressFamily.InterNetworkV6)
			{
				throw new SocketException(SocketError.OperationNotSupported);
			}
			if (PrivateAddress != value)
			{
				if (this is ReadOnlyIPAddress)
				{
					throw new SocketException(SocketError.OperationNotSupported);
				}
				PrivateAddress = (uint)value;
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.IPAddress" /> class with the address specified as an <see cref="T:System.Int64" />.</summary>
	/// <param name="newAddress">The long value of the IP address. For example, the value 0x2414188f in big-endian format would be the IP address "143.24.20.36". </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="newAddress" /> &lt; 0 or <paramref name="newAddress" /> &gt; 0x00000000FFFFFFFF </exception>
	public IPAddress(long newAddress)
	{
		if (newAddress < 0 || newAddress > uint.MaxValue)
		{
			throw new ArgumentOutOfRangeException("newAddress");
		}
		PrivateAddress = (uint)newAddress;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.IPAddress" /> class with the address specified as a <see cref="T:System.Byte" /> array and the specified scope identifier.</summary>
	/// <param name="address">The byte array value of the IP address. </param>
	/// <param name="scopeid">The long value of the scope identifier. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="address" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="address" /> contains a bad IP address. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="scopeid" /> &lt; 0 or <paramref name="scopeid" /> &gt; 0x00000000FFFFFFFF </exception>
	public IPAddress(byte[] address, long scopeid)
		: this(new ReadOnlySpan<byte>(address ?? ThrowAddressNullException()), scopeid)
	{
	}

	public IPAddress(ReadOnlySpan<byte> address, long scopeid)
	{
		if (address.Length != 16)
		{
			throw new ArgumentException("An invalid IP address was specified.", "address");
		}
		if (scopeid < 0 || scopeid > uint.MaxValue)
		{
			throw new ArgumentOutOfRangeException("scopeid");
		}
		_numbers = new ushort[8];
		for (int i = 0; i < 8; i++)
		{
			_numbers[i] = (ushort)(address[i * 2] * 256 + address[i * 2 + 1]);
		}
		PrivateScopeId = (uint)scopeid;
	}

	internal unsafe IPAddress(ushort* numbers, int numbersLength, uint scopeid)
	{
		ushort[] array = new ushort[8];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = numbers[i];
		}
		_numbers = array;
		PrivateScopeId = scopeid;
	}

	private IPAddress(ushort[] numbers, uint scopeid)
	{
		_numbers = numbers;
		PrivateScopeId = scopeid;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.IPAddress" /> class with the address specified as a <see cref="T:System.Byte" /> array.</summary>
	/// <param name="address">The byte array value of the IP address. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="address" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="address" /> contains a bad IP address. </exception>
	public IPAddress(byte[] address)
		: this(new ReadOnlySpan<byte>(address ?? ThrowAddressNullException()))
	{
	}

	public IPAddress(ReadOnlySpan<byte> address)
	{
		if (address.Length == 4)
		{
			PrivateAddress = (uint)(((address[3] << 24) | (address[2] << 16) | (address[1] << 8) | address[0]) & 0xFFFFFFFFu);
			return;
		}
		if (address.Length == 16)
		{
			_numbers = new ushort[8];
			for (int i = 0; i < 8; i++)
			{
				_numbers[i] = (ushort)(address[i * 2] * 256 + address[i * 2 + 1]);
			}
			return;
		}
		throw new ArgumentException("An invalid IP address was specified.", "address");
	}

	internal IPAddress(int newAddress)
	{
		PrivateAddress = (uint)newAddress;
	}

	/// <summary>Determines whether a string is a valid IP address.</summary>
	/// <returns>true if <paramref name="ipString" /> is a valid IP address; otherwise, false.</returns>
	/// <param name="ipString">The string to validate.</param>
	/// <param name="address">The <see cref="T:System.Net.IPAddress" /> version of the string.</param>
	public static bool TryParse(string ipString, out IPAddress address)
	{
		if (ipString == null)
		{
			address = null;
			return false;
		}
		address = IPAddressParser.Parse(ipString.AsSpan(), tryParse: true);
		return address != null;
	}

	public static bool TryParse(ReadOnlySpan<char> ipSpan, out IPAddress address)
	{
		address = IPAddressParser.Parse(ipSpan, tryParse: true);
		return address != null;
	}

	/// <summary>Converts an IP address string to an <see cref="T:System.Net.IPAddress" /> instance.</summary>
	/// <returns>An <see cref="T:System.Net.IPAddress" /> instance.</returns>
	/// <param name="ipString">A string that contains an IP address in dotted-quad notation for IPv4 and in colon-hexadecimal notation for IPv6. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="ipString" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="ipString" /> is not a valid IP address. </exception>
	public static IPAddress Parse(string ipString)
	{
		if (ipString == null)
		{
			throw new ArgumentNullException("ipString");
		}
		return IPAddressParser.Parse(ipString.AsSpan(), tryParse: false);
	}

	public static IPAddress Parse(ReadOnlySpan<char> ipSpan)
	{
		return IPAddressParser.Parse(ipSpan, tryParse: false);
	}

	public bool TryWriteBytes(Span<byte> destination, out int bytesWritten)
	{
		if (IsIPv6)
		{
			if (destination.Length < 16)
			{
				bytesWritten = 0;
				return false;
			}
			WriteIPv6Bytes(destination);
			bytesWritten = 16;
		}
		else
		{
			if (destination.Length < 4)
			{
				bytesWritten = 0;
				return false;
			}
			WriteIPv4Bytes(destination);
			bytesWritten = 4;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void WriteIPv6Bytes(Span<byte> destination)
	{
		int num = 0;
		for (int i = 0; i < 8; i++)
		{
			destination[num++] = (byte)((_numbers[i] >> 8) & 0xFF);
			destination[num++] = (byte)(_numbers[i] & 0xFF);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void WriteIPv4Bytes(Span<byte> destination)
	{
		uint privateAddress = PrivateAddress;
		destination[0] = (byte)privateAddress;
		destination[1] = (byte)(privateAddress >> 8);
		destination[2] = (byte)(privateAddress >> 16);
		destination[3] = (byte)(privateAddress >> 24);
	}

	/// <summary>Provides a copy of the <see cref="T:System.Net.IPAddress" /> as an array of bytes.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array.</returns>
	public byte[] GetAddressBytes()
	{
		if (IsIPv6)
		{
			byte[] array = new byte[16];
			WriteIPv6Bytes(array);
			return array;
		}
		byte[] array2 = new byte[4];
		WriteIPv4Bytes(array2);
		return array2;
	}

	/// <summary>Converts an Internet address to its standard notation.</summary>
	/// <returns>A string that contains the IP address in either IPv4 dotted-quad or in IPv6 colon-hexadecimal notation.</returns>
	/// <exception cref="T:System.Net.Sockets.SocketException">The address family is <see cref="F:System.Net.Sockets.AddressFamily.InterNetworkV6" /> and the address is bad. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override string ToString()
	{
		if (_toString == null)
		{
			_toString = (IsIPv4 ? IPAddressParser.IPv4AddressToString(PrivateAddress) : IPAddressParser.IPv6AddressToString(_numbers, PrivateScopeId));
		}
		return _toString;
	}

	public bool TryFormat(Span<char> destination, out int charsWritten)
	{
		if (!IsIPv4)
		{
			return IPAddressParser.IPv6AddressToString(_numbers, PrivateScopeId, destination, out charsWritten);
		}
		return IPAddressParser.IPv4AddressToString(PrivateAddress, destination, out charsWritten);
	}

	/// <summary>Converts a long value from host byte order to network byte order.</summary>
	/// <returns>A long value, expressed in network byte order.</returns>
	/// <param name="host">The number to convert, expressed in host byte order. </param>
	public static long HostToNetworkOrder(long host)
	{
		if (!BitConverter.IsLittleEndian)
		{
			return host;
		}
		return BinaryPrimitives.ReverseEndianness(host);
	}

	/// <summary>Converts an integer value from host byte order to network byte order.</summary>
	/// <returns>An integer value, expressed in network byte order.</returns>
	/// <param name="host">The number to convert, expressed in host byte order. </param>
	public static int HostToNetworkOrder(int host)
	{
		if (!BitConverter.IsLittleEndian)
		{
			return host;
		}
		return BinaryPrimitives.ReverseEndianness(host);
	}

	/// <summary>Converts a short value from host byte order to network byte order.</summary>
	/// <returns>A short value, expressed in network byte order.</returns>
	/// <param name="host">The number to convert, expressed in host byte order. </param>
	public static short HostToNetworkOrder(short host)
	{
		if (!BitConverter.IsLittleEndian)
		{
			return host;
		}
		return BinaryPrimitives.ReverseEndianness(host);
	}

	/// <summary>Converts a long value from network byte order to host byte order.</summary>
	/// <returns>A long value, expressed in host byte order.</returns>
	/// <param name="network">The number to convert, expressed in network byte order. </param>
	public static long NetworkToHostOrder(long network)
	{
		return HostToNetworkOrder(network);
	}

	/// <summary>Converts an integer value from network byte order to host byte order.</summary>
	/// <returns>An integer value, expressed in host byte order.</returns>
	/// <param name="network">The number to convert, expressed in network byte order. </param>
	public static int NetworkToHostOrder(int network)
	{
		return HostToNetworkOrder(network);
	}

	/// <summary>Converts a short value from network byte order to host byte order.</summary>
	/// <returns>A short value, expressed in host byte order.</returns>
	/// <param name="network">The number to convert, expressed in network byte order. </param>
	public static short NetworkToHostOrder(short network)
	{
		return HostToNetworkOrder(network);
	}

	/// <summary>Indicates whether the specified IP address is the loopback address.</summary>
	/// <returns>true if <paramref name="address" /> is the loopback address; otherwise, false.</returns>
	/// <param name="address">An IP address. </param>
	public static bool IsLoopback(IPAddress address)
	{
		if (address == null)
		{
			ThrowAddressNullException();
		}
		if (address.IsIPv6)
		{
			return address.Equals(IPv6Loopback);
		}
		return ((ulong)address.PrivateAddress & 0xFFuL) == ((ulong)Loopback.PrivateAddress & 0xFFuL);
	}

	internal bool Equals(object comparandObj, bool compareScopeId)
	{
		if (!(comparandObj is IPAddress iPAddress))
		{
			return false;
		}
		if (AddressFamily != iPAddress.AddressFamily)
		{
			return false;
		}
		if (IsIPv6)
		{
			for (int i = 0; i < 8; i++)
			{
				if (iPAddress._numbers[i] != _numbers[i])
				{
					return false;
				}
			}
			if (iPAddress.PrivateScopeId != PrivateScopeId)
			{
				return !compareScopeId;
			}
			return true;
		}
		return iPAddress.PrivateAddress == PrivateAddress;
	}

	/// <summary>Compares two IP addresses.</summary>
	/// <returns>true if the two addresses are equal; otherwise, false.</returns>
	/// <param name="comparand">An <see cref="T:System.Net.IPAddress" /> instance to compare to the current instance. </param>
	public override bool Equals(object comparand)
	{
		return Equals(comparand, compareScopeId: true);
	}

	/// <summary>Returns a hash value for an IP address.</summary>
	/// <returns>An integer hash value.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override int GetHashCode()
	{
		if (_hashCode != 0)
		{
			return _hashCode;
		}
		int hashCode;
		if (IsIPv6)
		{
			Span<byte> span = stackalloc byte[20];
			MemoryMarshal.AsBytes(new ReadOnlySpan<ushort>(_numbers)).CopyTo(span);
			BitConverter.TryWriteBytes(span.Slice(16), _addressOrScopeId);
			hashCode = Marvin.ComputeHash32(span, Marvin.DefaultSeed);
		}
		else
		{
			hashCode = Marvin.ComputeHash32(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref _addressOrScopeId, 1)), Marvin.DefaultSeed);
		}
		_hashCode = hashCode;
		return _hashCode;
	}

	/// <summary>Maps the <see cref="T:System.Net.IPAddress" /> object to an IPv6 address.</summary>
	/// <returns>Returns <see cref="T:System.Net.IPAddress" />.An IPv6 address.</returns>
	public IPAddress MapToIPv6()
	{
		if (IsIPv6)
		{
			return this;
		}
		uint privateAddress = PrivateAddress;
		return new IPAddress(new ushort[8]
		{
			0,
			0,
			0,
			0,
			0,
			65535,
			(ushort)(((privateAddress & 0xFF00) >> 8) | ((privateAddress & 0xFF) << 8)),
			(ushort)(((privateAddress & 0xFF000000u) >> 24) | ((privateAddress & 0xFF0000) >> 8))
		}, 0u);
	}

	/// <summary>Maps the <see cref="T:System.Net.IPAddress" /> object to an IPv4 address.</summary>
	/// <returns>Returns <see cref="T:System.Net.IPAddress" />.An IPv4 address.</returns>
	public IPAddress MapToIPv4()
	{
		if (IsIPv4)
		{
			return this;
		}
		return new IPAddress((uint)(((_numbers[6] & 0xFF00) >>> 8) | ((_numbers[6] & 0xFF) << 8) | ((((_numbers[7] & 0xFF00) >>> 8) | ((_numbers[7] & 0xFF) << 8)) << 16)));
	}

	private static byte[] ThrowAddressNullException()
	{
		throw new ArgumentNullException("address");
	}
}
