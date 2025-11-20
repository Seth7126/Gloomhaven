using Apparance.Net;
using UnityEngine;

public static class Conversion
{
	public static UnityEngine.Vector3 UVfromAV(Apparance.Net.Vector3 v)
	{
		return new UnityEngine.Vector3(v.X, v.Z, v.Y);
	}

	public static UnityEngine.Vector2 UVfromAV(Apparance.Net.Vector2 v)
	{
		return new UnityEngine.Vector2(v.X, v.Y);
	}

	public static Color UCfromAC(Colour c)
	{
		return new Color(c.R, c.G, c.B, c.A);
	}

	public static Color32 UC32fromUInt(uint i)
	{
		return new Color32((byte)(i & 0xFF), (byte)((i >> 8) & 0xFF), (byte)((i >> 16) & 0xFF), (byte)(i >> 24));
	}

	public static UnityEngine.Vector3[] UVsfromAVs(Apparance.Net.Vector3[] av, UnityEngine.Vector3[] optionally_provide_output_array = null)
	{
		int num = av.Length;
		UnityEngine.Vector3[] array = optionally_provide_output_array;
		if (array == null)
		{
			array = new UnityEngine.Vector3[num];
		}
		for (int i = 0; i < num; i++)
		{
			array[i].x = av[i].X;
			array[i].y = av[i].Z;
			array[i].z = av[i].Y;
		}
		return array;
	}

	public static UnityEngine.Vector2[] UVsfromAVs(Apparance.Net.Vector2[] av, UnityEngine.Vector2[] optionally_provide_output_array = null)
	{
		int num = av.Length;
		UnityEngine.Vector2[] array = optionally_provide_output_array;
		if (array == null)
		{
			array = new UnityEngine.Vector2[num];
		}
		for (int i = 0; i < num; i++)
		{
			array[i].x = av[i].X;
			array[i].y = av[i].Y;
		}
		return array;
	}

	public static Color32[] UC32sfromUInts(uint[] ia, Color32[] optionally_provide_output_array = null)
	{
		int num = ia.Length;
		Color32[] array = optionally_provide_output_array;
		if (array == null)
		{
			array = new Color32[num];
		}
		for (int i = 0; i < num; i++)
		{
			array[i].r = (byte)(ia[i] & 0xFF);
			array[i].g = (byte)((ia[i] >> 8) & 0xFF);
			array[i].b = (byte)((ia[i] >> 16) & 0xFF);
			array[i].a = (byte)((ia[i] >> 24) & 0xFF);
		}
		return array;
	}

	public static int[] ReverseWinding(int[] indices, int optionally_specify_num_elements = -1)
	{
		int num = indices.Length;
		if (optionally_specify_num_elements >= 0)
		{
			num = optionally_specify_num_elements;
		}
		if (num % 3 == 0)
		{
			for (int i = 0; i < num; i += 3)
			{
				int num2 = indices[i];
				int num3 = indices[i + 1];
				indices[i] = num3;
				indices[i + 1] = num2;
			}
		}
		else
		{
			ApparanceEngine.Instance.LogError("Incorrect number of vertices, " + num + " is not a multiple of 3");
		}
		return indices;
	}

	public static Matrix4x4 MatrixFromFrame(Frame frame)
	{
		Matrix4x4 identity = Matrix4x4.identity;
		identity.m00 = frame.AxisX.X;
		identity.m10 = frame.AxisX.Z;
		identity.m20 = frame.AxisX.Y;
		identity.m01 = frame.AxisZ.X;
		identity.m11 = frame.AxisZ.Z;
		identity.m21 = frame.AxisZ.Y;
		identity.m02 = frame.AxisY.X;
		identity.m12 = frame.AxisY.Z;
		identity.m22 = frame.AxisY.Y;
		Matrix4x4 matrix4x = Matrix4x4.Scale(UVfromAV(frame.Size));
		Matrix4x4 matrix4x2 = identity * matrix4x;
		UnityEngine.Vector3 vector = matrix4x2.MultiplyPoint(new UnityEngine.Vector3(0.5f, 0.5f, 0.5f));
		return Matrix4x4.Translate(UVfromAV(frame.Origin) + vector) * matrix4x2;
	}

	public static Apparance.Net.Vector3 AVfromUV(UnityEngine.Vector3 v)
	{
		return new Apparance.Net.Vector3(v.x, v.z, v.y);
	}

	public static Colour ACfromUC(Color c)
	{
		return new Colour(c.r, c.g, c.b, c.a);
	}

	public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
	{
		Quaternion result = new Quaternion
		{
			w = Mathf.Sqrt(Mathf.Max(0f, 1f + m[0, 0] + m[1, 1] + m[2, 2])) / 2f,
			x = Mathf.Sqrt(Mathf.Max(0f, 1f + m[0, 0] - m[1, 1] - m[2, 2])) / 2f,
			y = Mathf.Sqrt(Mathf.Max(0f, 1f - m[0, 0] + m[1, 1] - m[2, 2])) / 2f,
			z = Mathf.Sqrt(Mathf.Max(0f, 1f - m[0, 0] - m[1, 1] + m[2, 2])) / 2f
		};
		result.x *= Mathf.Sign(result.x * (m[2, 1] - m[1, 2]));
		result.y *= Mathf.Sign(result.y * (m[0, 2] - m[2, 0]));
		result.z *= Mathf.Sign(result.z * (m[1, 0] - m[0, 1]));
		return result;
	}

	public static int HashVector(UnityEngine.Vector3 v, int seed)
	{
		return v.x.GetHashCode() ^ (v.y.GetHashCode() << 8) ^ (v.z.GetHashCode() << 16) ^ (seed * 5479);
	}

	public static int HashQuaternion(Quaternion q, int seed)
	{
		return HashVector(q.eulerAngles, seed);
	}

	public static int HashString(string s, int seed)
	{
		int num = 0;
		foreach (int num2 in s)
		{
			num += num2 * seed;
			seed = seed * 1235 + 139;
		}
		return num;
	}
}
