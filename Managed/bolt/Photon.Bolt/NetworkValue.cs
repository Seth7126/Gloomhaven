using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Photon.Bolt;

[StructLayout(LayoutKind.Explicit)]
internal struct NetworkValue
{
	[FieldOffset(0)]
	public ulong Packed0;

	[FieldOffset(8)]
	public ulong Packed1;

	[FieldOffset(0)]
	public bool Bool;

	[FieldOffset(0)]
	public int Int0;

	[FieldOffset(0)]
	public float Float0;

	[FieldOffset(4)]
	public float Float1;

	[FieldOffset(8)]
	public float Float2;

	[FieldOffset(12)]
	public float Float3;

	[FieldOffset(0)]
	public Guid Guid;

	[FieldOffset(0)]
	public NetworkId NetworkId;

	[FieldOffset(0)]
	public PrefabId PrefabId;

	[FieldOffset(0)]
	public NetworkTrigger TriggerLocal;

	[FieldOffset(8)]
	public NetworkTrigger TriggerSend;

	[FieldOffset(0)]
	public Vector2 Vector2;

	[FieldOffset(0)]
	public Vector3 Vector3;

	[FieldOffset(0)]
	public Quaternion Quaternion;

	[FieldOffset(0)]
	public Color Color;

	[FieldOffset(0)]
	public Color32 Color32;

	[FieldOffset(12)]
	internal bool HasCopiedFromTransform;

	[FieldOffset(16)]
	public object Object;

	public bool HasNonDefaultValue => Packed0 != 0L || Packed1 != 0L || Object != null;

	public Matrix4x4 Matrix4x4
	{
		get
		{
			return (Object != null) ? ((Matrix4x4)Object) : Matrix4x4.zero;
		}
		set
		{
			Object = value;
		}
	}

	public string String
	{
		get
		{
			return (string)Object;
		}
		set
		{
			Object = value;
		}
	}

	public IProtocolToken ProtocolToken
	{
		get
		{
			return (IProtocolToken)Object;
		}
		set
		{
			Object = value;
		}
	}

	public NetworkTransform Transform
	{
		get
		{
			return (NetworkTransform)Object;
		}
		set
		{
			Object = value;
		}
	}

	public Action Action
	{
		get
		{
			return (Action)Object;
		}
		set
		{
			Object = value;
		}
	}

	public BoltEntity Entity
	{
		get
		{
			return BoltNetwork.FindEntity(NetworkId);
		}
		set
		{
			if ((bool)value && value.IsAttached)
			{
				NetworkId = value.NetworkId;
			}
			else
			{
				NetworkId = default(NetworkId);
			}
		}
	}

	public static bool Diff(bool a, bool b)
	{
		return a != b;
	}

	public static bool Diff(int a, int b)
	{
		return a != b;
	}

	public static bool Diff(Guid a, Guid b)
	{
		return a != b;
	}

	public static bool Diff(float a, float b)
	{
		return a != b;
	}

	public static bool Diff(NetworkId a, NetworkId b)
	{
		return a != b;
	}

	public static bool Diff(PrefabId a, PrefabId b)
	{
		return a != b;
	}

	public static bool Diff(NetworkTrigger a, NetworkTrigger b)
	{
		return a != b;
	}

	public static bool Diff(object a, object b)
	{
		return a != b;
	}

	public static bool Diff(Vector2 a, Vector2 b)
	{
		return a != b;
	}

	public static bool Diff(Vector3 a, Vector3 b)
	{
		return a != b;
	}

	public static bool Diff(Quaternion a, Quaternion b)
	{
		return a != b;
	}

	public static bool Diff(Color a, Color b)
	{
		return a != b;
	}

	public static bool Diff(Color32 a, Color32 b)
	{
		return a.r != b.r || a.b != b.b || a.g != b.g || a.a != b.a;
	}

	public static bool Diff(Matrix4x4 a, Matrix4x4 b)
	{
		return a != b;
	}

	public static bool Diff_Strict(BoltEntity a, BoltEntity b)
	{
		return ((bool)a ^ (bool)b) || a?.NetworkId != b?.NetworkId;
	}

	public static bool Diff_Strict(Vector2 a, Vector2 b)
	{
		return a.x != b.x || a.y != b.y;
	}

	public static bool Diff_Strict(Vector3 a, Vector3 b)
	{
		return a.x != b.x || a.y != b.y || a.z != b.z;
	}

	public static bool Diff_Strict(Quaternion a, Quaternion b)
	{
		return a.x != b.x || a.y != b.y || a.z != b.z || a.w != b.w;
	}
}
