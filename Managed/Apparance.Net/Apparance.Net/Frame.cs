using System.IO;

namespace Apparance.Net;

public struct Frame
{
	public Vector3 AxisX;

	public Vector3 AxisY;

	public Vector3 AxisZ;

	public Vector3 Origin;

	public Vector3 Size;

	public static bool operator ==(Frame a, Frame b)
	{
		if (a.AxisX == b.AxisX && a.AxisY == b.AxisY && a.AxisZ == b.AxisZ && a.Origin == b.Origin)
		{
			return a.Size == b.Size;
		}
		return false;
	}

	public static bool operator !=(Frame a, Frame b)
	{
		if (!(a.AxisX != b.AxisX) && !(a.AxisY != b.AxisY) && !(a.AxisZ != b.AxisZ) && !(a.Origin != b.Origin))
		{
			return a.Size != b.Size;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		Frame frame = (Frame)obj;
		if (AxisX == frame.AxisX && AxisY == frame.AxisY && AxisZ == frame.AxisZ && Origin == frame.Origin)
		{
			return Size == frame.Size;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return AxisX.GetHashCode() ^ AxisY.GetHashCode() ^ AxisZ.GetHashCode() ^ Origin.GetHashCode() ^ Size.GetHashCode();
	}

	internal void Store(StreamWriter store)
	{
		store.Write(AxisX);
		store.Write(AxisY);
		store.Write(AxisZ);
		store.Write(Origin);
		store.Write(Size);
	}

	public override string ToString()
	{
		return $"{Origin} {Size.X:F1}x{Size.Y:F1}x{Size.Z:F1} [X={AxisX}|Y={AxisY}|Z={AxisZ}]";
	}

	internal float[] AsFloats()
	{
		return new float[15]
		{
			AxisX.X, AxisX.Y, AxisX.Z, AxisY.X, AxisY.Y, AxisY.Z, AxisZ.X, AxisZ.Y, AxisZ.Z, Origin.X,
			Origin.Y, Origin.Z, Size.X, Size.Y, Size.Z
		};
	}

	internal static Frame Stream(BinaryReader stream)
	{
		return new Frame
		{
			AxisX = Vector3.Stream(stream),
			AxisY = Vector3.Stream(stream),
			AxisZ = Vector3.Stream(stream),
			Origin = Vector3.Stream(stream),
			Size = Vector3.Stream(stream)
		};
	}
}
