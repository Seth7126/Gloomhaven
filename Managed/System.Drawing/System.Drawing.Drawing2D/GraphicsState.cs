using Unity;

namespace System.Drawing.Drawing2D;

/// <summary>Represents the state of a <see cref="T:System.Drawing.Graphics" /> object. This object is returned by a call to the <see cref="M:System.Drawing.Graphics.Save" /> methods. This class cannot be inherited.</summary>
public sealed class GraphicsState : MarshalByRefObject
{
	internal int nativeState;

	internal GraphicsState(int nativeState)
	{
		this.nativeState = nativeState;
	}

	internal GraphicsState()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
