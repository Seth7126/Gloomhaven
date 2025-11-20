namespace System.Drawing;

internal class CocoaContext : IMacContext
{
	public IntPtr focusHandle;

	public IntPtr ctx;

	public int width;

	public int height;

	public CocoaContext(IntPtr focusHandle, IntPtr ctx, int width, int height)
	{
		this.focusHandle = focusHandle;
		this.ctx = ctx;
		this.width = width;
		this.height = height;
	}

	public void Synchronize()
	{
		MacSupport.CGContextSynchronize(ctx);
	}

	public void Release()
	{
		if (IntPtr.Zero != focusHandle)
		{
			MacSupport.CGContextFlush(ctx);
		}
		MacSupport.CGContextRestoreGState(ctx);
		if (IntPtr.Zero != focusHandle)
		{
			MacSupport.objc_msgSend(focusHandle, MacSupport.sel_registerName("unlockFocus"));
		}
	}
}
