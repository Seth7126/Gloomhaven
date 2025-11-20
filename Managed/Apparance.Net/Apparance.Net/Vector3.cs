using System.IO;

namespace Apparance.Net;

public struct Vector3
{
	public float X;

	public float Y;

	public float Z;

	public static Vector3 Zero = new Vector3(0f, 0f, 0f);

	public Vector3(float x, float y, float z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public static bool operator ==(Vector3 a, Vector3 b)
	{
		if (a.X == b.X && a.Y == b.Y)
		{
			return a.Z == b.Z;
		}
		return false;
	}

	public static bool operator !=(Vector3 a, Vector3 b)
	{
		if (a.X == b.X && a.Y == b.Y)
		{
			return a.Z != b.Z;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		Vector3 vector = (Vector3)obj;
		if (X == vector.X && Y == vector.Y)
		{
			return Z == vector.Z;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
	}

	public override string ToString()
	{
		return $"{X:F3},{Y:F3},{Z:F3}";
	}

	internal void Store(StreamWriter store)
	{
		store.Write(X);
		store.Write(Y);
		store.Write(Z);
	}

	internal static Vector3 Stream(BinaryReader stream)
	{
		float x = stream.ReadSingle();
		float y = stream.ReadSingle();
		float z = stream.ReadSingle();
		return new Vector3(x, y, z);
	}
}
