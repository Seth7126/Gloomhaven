#define DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Photon.Bolt.Exceptions;
using Photon.Bolt.Internal;
using UdpKit;
using UnityEngine;

namespace Photon.Bolt.Utils;

[Documentation(Ignore = true)]
public static class BoltUtils
{
	public static bool TryGetValue<T>(this Dictionary<string, object> data, string key, out T value)
	{
		value = default(T);
		if (data.TryGetValue(key, out var value2))
		{
			value = (T)Convert.ChangeType(value2, typeof(T));
			return true;
		}
		return false;
	}

	public static string ToStringDetailed(this Quaternion q)
	{
		return $"({q.x}, {q.y}, {q.z}, {q.w})";
	}

	public static IProtocolToken GetProtocolToken(this UdpSession session)
	{
		if (session == null || session.HostData == null || session.HostData.Length == 0)
		{
			return null;
		}
		if (session.HostObject == null)
		{
			session.HostObject = session.HostData.ToToken();
		}
		return (IProtocolToken)session.HostObject;
	}

	public static bool NullOrEmpty(this Array array)
	{
		return array == null || array.Length == 0;
	}

	public static bool Has<T>(this T[] array, int index) where T : class
	{
		return index < array.Length && array[index] != null;
	}

	public static bool Has<T>(this T[] array, uint index) where T : class
	{
		return index < array.Length && array[index] != null;
	}

	public static bool TryGetIndex<T>(this T[] array, int index, out T value) where T : class
	{
		if (index < array.Length)
		{
			return (value = array[index]) != null;
		}
		value = null;
		return false;
	}

	public static bool TryGetIndex<T>(this T[] array, uint index, out T value) where T : class
	{
		if (index < array.Length)
		{
			return (value = array[index]) != null;
		}
		value = null;
		return false;
	}

	public static T FindComponent<T>(this Component component) where T : Component
	{
		return component.transform.FindComponent<T>();
	}

	public static T FindComponent<T>(this GameObject gameObject) where T : Component
	{
		return gameObject.transform.FindComponent<T>();
	}

	public static T FindComponent<T>(this Transform transform) where T : Component
	{
		T val = null;
		while ((bool)transform && !val)
		{
			val = transform.GetComponent<T>();
			transform = transform.parent;
		}
		return val;
	}

	public static BoltConnection GetBoltConnection(this UdpConnection self)
	{
		return (BoltConnection)self.UserToken;
	}

	public static string Join<T>(this IEnumerable<T> items, string seperator)
	{
		return string.Join(seperator, items.Select((T x) => x.ToString()).ToArray());
	}

	public static bool ViewPointIsOnScreen(this Vector3 vp)
	{
		return vp.z >= 0f && vp.x >= 0f && vp.x <= 1f && vp.y >= 0f && vp.y <= 1f;
	}

	public static T[] CloneArray<T>(this T[] array)
	{
		T[] array2 = new T[array.Length];
		Array.Copy(array, 0, array2, 0, array.Length);
		return array2;
	}

	public static T[] AddFirst<T>(this T[] array, T item)
	{
		if (array == null)
		{
			return new T[1] { item };
		}
		T[] array2 = new T[array.Length + 1];
		Array.Copy(array, 0, array2, 1, array.Length);
		array2[0] = item;
		return array2;
	}

	public static void WriteUniqueId(this UdpPacket stream, UniqueId id)
	{
		stream.WriteUInt(id.uint0);
		stream.WriteUInt(id.uint1);
		stream.WriteUInt(id.uint2);
		stream.WriteUInt(id.uint3);
	}

	public static UniqueId ReadUniqueId(this UdpPacket stream)
	{
		return new UniqueId
		{
			uint0 = stream.ReadUInt(),
			uint1 = stream.ReadUInt(),
			uint2 = stream.ReadUInt(),
			uint3 = stream.ReadUInt()
		};
	}

	public static void WriteByteArraySimple(this UdpPacket stream, byte[] array, int maxLength)
	{
		if (stream.WriteBool(array != null))
		{
			int num = Mathf.Min(array.Length, maxLength);
			if (num < array.Length)
			{
				BoltLog.Warn("Only sendig {0}/{1} bytes from byte array", num, array.Length);
			}
			stream.WriteUShort((ushort)num);
			stream.WriteByteArray(array, 0, num);
		}
	}

	public static byte[] ReadByteArraySimple(this UdpPacket stream)
	{
		if (stream.ReadBool())
		{
			int num = stream.ReadUShort();
			byte[] array = new byte[num];
			stream.ReadByteArray(array, 0, array.Length);
			return array;
		}
		return null;
	}

	public static void WriteColor32RGBA(this UdpPacket stream, Color32 value)
	{
		stream.WriteByte(value.r, 8);
		stream.WriteByte(value.g, 8);
		stream.WriteByte(value.b, 8);
		stream.WriteByte(value.a, 8);
	}

	public static Color32 ReadColor32RGBA(this UdpPacket stream)
	{
		return new Color32(stream.ReadByte(8), stream.ReadByte(8), stream.ReadByte(8), stream.ReadByte(8));
	}

	public static void WriteColor32RGB(this UdpPacket stream, Color32 value)
	{
		stream.WriteByte(value.r, 8);
		stream.WriteByte(value.g, 8);
		stream.WriteByte(value.b, 8);
	}

	public static Color32 ReadColor32RGB(this UdpPacket stream)
	{
		return new Color32(stream.ReadByte(8), stream.ReadByte(8), stream.ReadByte(8), byte.MaxValue);
	}

	public static void WriteVector2(this UdpPacket stream, Vector2 value)
	{
		stream.WriteFloat(value.x);
		stream.WriteFloat(value.y);
	}

	public static Vector2 ReadVector2(this UdpPacket stream)
	{
		return new Vector2(stream.ReadFloat(), stream.ReadFloat());
	}

	public static void WriteVector3(this UdpPacket stream, Vector3 value)
	{
		stream.WriteFloat(value.x);
		stream.WriteFloat(value.y);
		stream.WriteFloat(value.z);
	}

	public static Vector3 ReadVector3(this UdpPacket stream)
	{
		return new Vector3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
	}

	public static void WriteColorRGB(this UdpPacket stream, Color value)
	{
		stream.WriteFloat(value.r);
		stream.WriteFloat(value.g);
		stream.WriteFloat(value.b);
	}

	public static Color ReadColorRGB(this UdpPacket stream)
	{
		return new Color(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
	}

	public static void WriteVector4(this UdpPacket stream, Vector4 value)
	{
		stream.WriteFloat(value.x);
		stream.WriteFloat(value.y);
		stream.WriteFloat(value.z);
		stream.WriteFloat(value.w);
	}

	public static Vector4 ReadVector4(this UdpPacket stream)
	{
		return new Vector4(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
	}

	public static void WriteColorRGBA(this UdpPacket stream, Color value)
	{
		stream.WriteFloat(value.r);
		stream.WriteFloat(value.g);
		stream.WriteFloat(value.b);
		stream.WriteFloat(value.a);
	}

	public static Color ReadColorRGBA(this UdpPacket stream)
	{
		return new Color(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
	}

	public static void WriteQuaternion(this UdpPacket stream, Quaternion value)
	{
		stream.WriteFloat(value.x);
		stream.WriteFloat(value.y);
		stream.WriteFloat(value.z);
		stream.WriteFloat(value.w);
	}

	public static Quaternion ReadQuaternion(this UdpPacket stream)
	{
		return new Quaternion(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
	}

	public static void WriteTransform(this UdpPacket stream, Transform transform)
	{
		stream.WriteVector3(transform.position);
		stream.WriteQuaternion(transform.rotation);
	}

	public static void ReadTransform(this UdpPacket stream, Transform transform)
	{
		transform.position = stream.ReadVector3();
		transform.rotation = stream.ReadQuaternion();
	}

	public static void ReadTransform(this UdpPacket stream, out Vector3 position, out Quaternion rotation)
	{
		position = stream.ReadVector3();
		rotation = stream.ReadQuaternion();
	}

	public static void WriteRigidbody(this UdpPacket stream, Rigidbody rigidbody)
	{
		stream.WriteVector3(rigidbody.position);
		stream.WriteQuaternion(rigidbody.rotation);
		stream.WriteVector3(rigidbody.velocity);
		stream.WriteVector3(rigidbody.angularVelocity);
	}

	public static void ReadRigidbody(this UdpPacket stream, Rigidbody rigidbody)
	{
		rigidbody.position = stream.ReadVector3();
		rigidbody.rotation = stream.ReadQuaternion();
		rigidbody.velocity = stream.ReadVector3();
		rigidbody.angularVelocity = stream.ReadVector3();
	}

	public static void ReadRigidbody(this UdpPacket stream, out Vector3 position, out Quaternion rotation, out Vector3 velocity, out Vector3 angularVelocity)
	{
		position = stream.ReadVector3();
		rotation = stream.ReadQuaternion();
		velocity = stream.ReadVector3();
		angularVelocity = stream.ReadVector3();
	}

	public static void WriteBounds(this UdpPacket stream, Bounds b)
	{
		stream.WriteVector3(b.center);
		stream.WriteVector3(b.size);
	}

	public static Bounds ReadBounds(this UdpPacket stream)
	{
		return new Bounds(stream.ReadVector3(), stream.ReadVector3());
	}

	public static void WriteRect(this UdpPacket stream, Rect rect)
	{
		stream.WriteFloat(rect.xMin);
		stream.WriteFloat(rect.yMin);
		stream.WriteFloat(rect.width);
		stream.WriteFloat(rect.height);
	}

	public static Rect ReadRect(this UdpPacket stream)
	{
		return new Rect(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
	}

	public static void WriteRay(this UdpPacket stream, Ray ray)
	{
		stream.WriteVector3(ray.origin);
		stream.WriteVector3(ray.direction);
	}

	public static Ray ReadRay(this UdpPacket stream)
	{
		return new Ray(stream.ReadVector3(), stream.ReadVector3());
	}

	public static void WritePlane(this UdpPacket stream, Plane plane)
	{
		stream.WriteVector3(plane.normal);
		stream.WriteFloat(plane.distance);
	}

	public static Plane ReadPlane(this UdpPacket stream)
	{
		return new Plane(stream.ReadVector3(), stream.ReadFloat());
	}

	public static void WriteLayerMask(this UdpPacket stream, LayerMask mask)
	{
		stream.WriteInt(mask.value, 32);
	}

	public static LayerMask ReadLayerMask(this UdpPacket stream)
	{
		return stream.ReadInt(32);
	}

	public static void WriteMatrix4x4(this UdpPacket stream, Matrix4x4 m)
	{
		stream.WriteFloat(m.m00);
		stream.WriteFloat(m.m01);
		stream.WriteFloat(m.m02);
		stream.WriteFloat(m.m03);
		stream.WriteFloat(m.m10);
		stream.WriteFloat(m.m11);
		stream.WriteFloat(m.m12);
		stream.WriteFloat(m.m13);
		stream.WriteFloat(m.m20);
		stream.WriteFloat(m.m21);
		stream.WriteFloat(m.m22);
		stream.WriteFloat(m.m23);
		stream.WriteFloat(m.m30);
		stream.WriteFloat(m.m31);
		stream.WriteFloat(m.m32);
		stream.WriteFloat(m.m33);
	}

	public static Matrix4x4 ReadMatrix4x4(this UdpPacket stream)
	{
		return new Matrix4x4
		{
			m00 = stream.ReadFloat(),
			m01 = stream.ReadFloat(),
			m02 = stream.ReadFloat(),
			m03 = stream.ReadFloat(),
			m10 = stream.ReadFloat(),
			m11 = stream.ReadFloat(),
			m12 = stream.ReadFloat(),
			m13 = stream.ReadFloat(),
			m20 = stream.ReadFloat(),
			m21 = stream.ReadFloat(),
			m22 = stream.ReadFloat(),
			m23 = stream.ReadFloat(),
			m30 = stream.ReadFloat(),
			m31 = stream.ReadFloat(),
			m32 = stream.ReadFloat(),
			m33 = stream.ReadFloat()
		};
	}

	public static void WriteIntVB(this UdpPacket packet, int v)
	{
		packet.WriteUIntVB((uint)v);
	}

	public static int ReadIntVB(this UdpPacket packet)
	{
		return (int)packet.ReadUIntVB();
	}

	public static void WriteUIntVB(this UdpPacket packet, uint v)
	{
		uint num = 0u;
		do
		{
			num = v & 0x7F;
			v >>= 7;
			if (v != 0)
			{
				num |= 0x80;
			}
			packet.WriteByte((byte)num);
		}
		while (v != 0);
	}

	public static uint ReadUIntVB(this UdpPacket packet)
	{
		uint num = 0u;
		uint num2 = 0u;
		int num3 = 0;
		do
		{
			num2 = packet.ReadByte();
			num |= (num2 & 0x7F) << num3;
			num3 += 7;
		}
		while (num2 > 127);
		return num;
	}

	public static void WriteLongVB(this UdpPacket p, long v)
	{
		p.WriteULongVB((ulong)v);
	}

	public static long ReadLongVB(this UdpPacket p)
	{
		return (long)p.ReadULongVB();
	}

	public static void WriteULongVB(this UdpPacket p, ulong v)
	{
		ulong num = 0uL;
		do
		{
			num = v & 0x7F;
			v >>= 7;
			if (v != 0)
			{
				num |= 0x80;
			}
			p.WriteByte((byte)num);
		}
		while (v != 0);
	}

	public static ulong ReadULongVB(this UdpPacket p)
	{
		ulong num = 0uL;
		ulong num2 = 0uL;
		int num3 = 0;
		do
		{
			num2 = p.ReadByte();
			num |= (num2 & 0x7F) << num3;
			num3 += 7;
		}
		while (num2 > 127);
		return num;
	}

	public static void WriteBoltEntity(this UdpPacket packet, BoltEntity entity)
	{
		packet.WriteEntity((entity == null) ? null : entity.Entity);
	}

	public static BoltEntity ReadBoltEntity(this UdpPacket packet)
	{
		Entity entity = packet.ReadEntity();
		if ((bool)entity)
		{
			return entity.UnityObject;
		}
		return null;
	}

	internal static void WriteEntity(this UdpPacket packet, Entity entity)
	{
		if (packet.WriteBool(entity != null && entity.IsAttached))
		{
			packet.WriteNetworkId(entity.NetworkId);
		}
	}

	internal static Entity ReadEntity(this UdpPacket packet)
	{
		if (packet.ReadBool())
		{
			return BoltCore.FindEntity(packet.ReadNetworkId());
		}
		return null;
	}

	public static void WriteNetworkId(this UdpPacket packet, NetworkId id)
	{
		Assert.True(id.Connection != uint.MaxValue);
		packet.WriteUIntVB(id.Connection);
		packet.WriteUIntVB(id.Entity);
	}

	public static NetworkId ReadNetworkId(this UdpPacket packet)
	{
		uint num = packet.ReadUIntVB();
		uint entity = packet.ReadUIntVB();
		Assert.True(num != uint.MaxValue);
		return new NetworkId(num, entity);
	}

	internal static void WriteContinueMarker(this UdpPacket stream)
	{
		if (stream.CanWrite())
		{
			stream.WriteBool(value: true);
		}
	}

	internal static void WriteStopMarker(this UdpPacket stream)
	{
		if (stream.CanWrite())
		{
			stream.WriteBool(value: false);
		}
	}

	internal static bool ReadStopMarker(this UdpPacket stream)
	{
		if (stream.CanRead())
		{
			return stream.ReadBool();
		}
		return false;
	}

	private static void ByteToString(byte value, StringBuilder sb)
	{
		ByteToString(value, 8, sb);
	}

	private static void ByteToString(byte value, int bits, StringBuilder sb)
	{
		if (bits < 1 || bits > 8)
		{
			throw new ArgumentOutOfRangeException("bits", "Must be between 1 and 8");
		}
		for (int num = bits - 1; num >= 0; num--)
		{
			if (((1 << num) & value) == 0)
			{
				sb.Append('0');
			}
			else
			{
				sb.Append('1');
			}
		}
	}

	public static string ByteToString(byte value, int bits)
	{
		StringBuilder stringBuilder = new StringBuilder(8);
		ByteToString(value, bits, stringBuilder);
		return stringBuilder.ToString();
	}

	public static string ByteToString(byte value)
	{
		return ByteToString(value, 8);
	}

	public static string UShortToString(ushort value)
	{
		StringBuilder stringBuilder = new StringBuilder(17);
		ByteToString((byte)(value >> 8), stringBuilder);
		stringBuilder.Append(' ');
		ByteToString((byte)value, stringBuilder);
		return stringBuilder.ToString();
	}

	public static string IntToString(int value)
	{
		return UIntToString((uint)value);
	}

	public static string UIntToString(uint value)
	{
		StringBuilder stringBuilder = new StringBuilder(35);
		ByteToString((byte)(value >> 24), stringBuilder);
		stringBuilder.Append(' ');
		ByteToString((byte)(value >> 16), stringBuilder);
		stringBuilder.Append(' ');
		ByteToString((byte)(value >> 8), stringBuilder);
		stringBuilder.Append(' ');
		ByteToString((byte)value, stringBuilder);
		return stringBuilder.ToString();
	}

	public static string ULongToString(ulong value)
	{
		StringBuilder stringBuilder = new StringBuilder(71);
		ByteToString((byte)(value >> 56), stringBuilder);
		stringBuilder.Append(' ');
		ByteToString((byte)(value >> 48), stringBuilder);
		stringBuilder.Append(' ');
		ByteToString((byte)(value >> 40), stringBuilder);
		stringBuilder.Append(' ');
		ByteToString((byte)(value >> 32), stringBuilder);
		stringBuilder.Append(' ');
		ByteToString((byte)(value >> 24), stringBuilder);
		stringBuilder.Append(' ');
		ByteToString((byte)(value >> 16), stringBuilder);
		stringBuilder.Append(' ');
		ByteToString((byte)(value >> 8), stringBuilder);
		stringBuilder.Append(' ');
		ByteToString((byte)value, stringBuilder);
		return stringBuilder.ToString();
	}

	public static string BytesToString(byte[] values)
	{
		StringBuilder stringBuilder = new StringBuilder(values.Length * 8 + System.Math.Max(0, values.Length - 1));
		for (int num = values.Length - 1; num >= 0; num--)
		{
			stringBuilder.Append(ByteToString(values[num]));
			if (num != 0)
			{
				stringBuilder.Append(' ');
			}
		}
		return stringBuilder.ToString();
	}

	public static bool IsValidAppId(string val)
	{
		try
		{
			Guid guid = new Guid(val);
		}
		catch
		{
			return false;
		}
		return true;
	}
}
