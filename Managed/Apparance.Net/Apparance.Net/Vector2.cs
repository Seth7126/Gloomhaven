using System.IO;

namespace Apparance.Net;

public struct Vector2
{
	public float X;

	public float Y;

	public static Vector2 Zero = new Vector2(0f, 0f);

	public Vector2(float x, float y)
	{
		X = x;
		Y = y;
	}

	public static bool operator ==(Vector2 a, Vector2 b)
	{
		if (a.X == b.X)
		{
			return a.Y == b.Y;
		}
		return false;
	}

	public static bool operator !=(Vector2 a, Vector2 b)
	{
		if (a.X == b.X)
		{
			return a.Y != b.Y;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		Vector2 vector = (Vector2)obj;
		if (X == vector.X)
		{
			return Y == vector.Y;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return X.GetHashCode() ^ Y.GetHashCode();
	}

	internal void Store(StreamWriter store)
	{
		store.Write(X);
		store.Write(Y);
	}

	internal static Vector2 Stream(BinaryReader stream)
	{
		float x = stream.ReadSingle();
		float y = stream.ReadSingle();
		return new Vector2(x, y);
	}
}
