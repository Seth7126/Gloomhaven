using System.Drawing.Imaging;
using System.Threading;

namespace System.Drawing;

internal class AnimateEventArgs : EventArgs
{
	private int frameCount;

	private int activeFrame;

	private Thread thread;

	public Thread RunThread
	{
		get
		{
			return thread;
		}
		set
		{
			thread = value;
		}
	}

	public AnimateEventArgs(Image image)
	{
		frameCount = image.GetFrameCount(FrameDimension.Time);
	}

	public int GetNextFrame()
	{
		if (activeFrame < frameCount - 1)
		{
			activeFrame++;
		}
		else
		{
			activeFrame = 0;
		}
		return activeFrame;
	}
}
