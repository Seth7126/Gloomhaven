using System.Runtime.InteropServices;

namespace Photon.Bolt;

[StructLayout(LayoutKind.Explicit)]
internal struct NetworkTrigger
{
	[FieldOffset(0)]
	public int Frame;

	[FieldOffset(4)]
	public int History;

	public override bool Equals(object obj)
	{
		if (!(obj is NetworkTrigger networkTrigger))
		{
			return false;
		}
		return networkTrigger == this;
	}

	public override int GetHashCode()
	{
		int num = -1414952011;
		num = num * -1521134295 + Frame.GetHashCode();
		return num * -1521134295 + History.GetHashCode();
	}

	public void Update(int frame, bool set)
	{
		if (frame > Frame)
		{
			int num = frame - Frame;
			History = ((num < 32) ? (History << num) : 0);
			if (set)
			{
				History |= 1;
			}
			Frame = frame;
		}
		else if (frame == Frame && set)
		{
			History |= 1;
		}
	}

	public static bool operator ==(NetworkTrigger a, NetworkTrigger b)
	{
		return a.Frame == b.Frame && a.History == b.History;
	}

	public static bool operator !=(NetworkTrigger a, NetworkTrigger b)
	{
		return a.Frame != b.Frame || a.History != b.History;
	}
}
