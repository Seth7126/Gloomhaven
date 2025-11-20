#define DEBUG
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Photon.Bolt.Exceptions;
using Photon.Bolt.Utils;

namespace Photon.Bolt;

[StructLayout(LayoutKind.Explicit)]
[Documentation]
public struct UniqueId : IEquatable<UniqueId>
{
	public class EqualityComparer : IEqualityComparer<UniqueId>
	{
		public static readonly EqualityComparer Instance = new EqualityComparer();

		private EqualityComparer()
		{
		}

		bool IEqualityComparer<UniqueId>.Equals(UniqueId x, UniqueId y)
		{
			return x.guid == y.guid;
		}

		int IEqualityComparer<UniqueId>.GetHashCode(UniqueId x)
		{
			return x.guid.GetHashCode();
		}
	}

	[FieldOffset(0)]
	internal Guid guid;

	[FieldOffset(0)]
	internal uint uint0;

	[FieldOffset(4)]
	internal uint uint1;

	[FieldOffset(8)]
	internal uint uint2;

	[FieldOffset(12)]
	internal uint uint3;

	[FieldOffset(0)]
	private byte byte0;

	[FieldOffset(1)]
	private byte byte1;

	[FieldOffset(2)]
	private byte byte2;

	[FieldOffset(3)]
	private byte byte3;

	[FieldOffset(4)]
	private byte byte4;

	[FieldOffset(5)]
	private byte byte5;

	[FieldOffset(6)]
	private byte byte6;

	[FieldOffset(7)]
	private byte byte7;

	[FieldOffset(8)]
	private byte byte8;

	[FieldOffset(9)]
	private byte byte9;

	[FieldOffset(10)]
	private byte byte10;

	[FieldOffset(11)]
	private byte byte11;

	[FieldOffset(12)]
	private byte byte12;

	[FieldOffset(13)]
	private byte byte13;

	[FieldOffset(14)]
	private byte byte14;

	[FieldOffset(15)]
	private byte byte15;

	public string IdString
	{
		get
		{
			if (IsNone)
			{
				return "NONE";
			}
			return guid.ToString();
		}
	}

	public bool IsNone => guid == Guid.Empty;

	public static UniqueId None => default(UniqueId);

	public UniqueId(string guid)
	{
		Assert.NotNull(guid);
		this = default(UniqueId);
		this.guid = new Guid(guid);
	}

	public UniqueId(byte[] bytes)
	{
		Assert.NotNull(bytes);
		Assert.True(bytes.Length == 16);
		this = default(UniqueId);
		byte0 = bytes[0];
		byte1 = bytes[1];
		byte2 = bytes[2];
		byte3 = bytes[3];
		byte4 = bytes[4];
		byte5 = bytes[5];
		byte6 = bytes[6];
		byte7 = bytes[7];
		byte8 = bytes[8];
		byte9 = bytes[9];
		byte10 = bytes[10];
		byte11 = bytes[11];
		byte12 = bytes[12];
		byte13 = bytes[13];
		byte14 = bytes[14];
		byte15 = bytes[15];
	}

	public UniqueId(byte byte0, byte byte1, byte byte2, byte byte3, byte byte4, byte byte5, byte byte6, byte byte7, byte byte8, byte byte9, byte byte10, byte byte11, byte byte12, byte byte13, byte byte14, byte byte15)
	{
		this = default(UniqueId);
		this.byte0 = byte0;
		this.byte1 = byte1;
		this.byte2 = byte2;
		this.byte3 = byte3;
		this.byte4 = byte4;
		this.byte5 = byte5;
		this.byte6 = byte6;
		this.byte7 = byte7;
		this.byte8 = byte8;
		this.byte9 = byte9;
		this.byte10 = byte10;
		this.byte11 = byte11;
		this.byte12 = byte12;
		this.byte13 = byte13;
		this.byte14 = byte14;
		this.byte15 = byte15;
	}

	public byte[] ToByteArray()
	{
		return guid.ToByteArray();
	}

	public override int GetHashCode()
	{
		return guid.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is UniqueId)
		{
			return ((UniqueId)obj).guid == guid;
		}
		return false;
	}

	public override string ToString()
	{
		if (IsNone)
		{
			return "[UniqueId NONE]";
		}
		return $"[UniqueId {guid.ToString()}]";
	}

	public static UniqueId New()
	{
		return new UniqueId
		{
			guid = Guid.NewGuid()
		};
	}

	public static UniqueId Parse(string text)
	{
		if (text == null || text == "" || text == "NONE")
		{
			return None;
		}
		try
		{
			return new UniqueId
			{
				guid = new Guid(text)
			};
		}
		catch
		{
			BoltLog.Warn("Could not parse '{0}' as a UniqueId", text);
			return None;
		}
	}

	bool IEquatable<UniqueId>.Equals(UniqueId other)
	{
		return guid == other.guid;
	}

	public static bool operator ==(UniqueId a, UniqueId b)
	{
		return a.guid == b.guid;
	}

	public static bool operator !=(UniqueId a, UniqueId b)
	{
		return a.guid != b.guid;
	}
}
