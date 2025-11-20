using System.IO;

namespace Apparance.Net;

public struct Colour
{
	public float R;

	public float G;

	public float B;

	public float A;

	public static Colour Zero = new Colour(0f, 0f, 0f, 0f);

	public Colour(float r, float g, float b, float a = 1f)
	{
		R = r;
		G = g;
		B = b;
		A = a;
	}

	public static bool operator ==(Colour a, Colour b)
	{
		if (a.R == b.R && a.G == b.G && a.B == b.B)
		{
			return a.A == b.A;
		}
		return false;
	}

	public static bool operator !=(Colour a, Colour b)
	{
		if (a.R == b.R && a.G == b.G && a.B == b.B)
		{
			return a.A != b.A;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		Colour colour = (Colour)obj;
		if (R == colour.R && G == colour.G && B == colour.B)
		{
			return A == colour.A;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return R.GetHashCode() ^ G.GetHashCode() ^ B.GetHashCode() ^ A.GetHashCode();
	}

	internal void Store(StreamWriter store)
	{
		store.Write(R);
		store.Write(G);
		store.Write(B);
		store.Write(A);
	}

	internal static Colour Stream(BinaryReader stream)
	{
		float r = stream.ReadSingle();
		float g = stream.ReadSingle();
		float b = stream.ReadSingle();
		float a = stream.ReadSingle();
		return new Colour(r, g, b, a);
	}
}
