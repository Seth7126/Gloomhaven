using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Drawing;

internal static class MacSupport
{
	internal static Hashtable contextReference;

	internal static object lockobj;

	internal static Delegate hwnd_delegate;

	static MacSupport()
	{
		contextReference = new Hashtable();
		lockobj = new object();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (string.Equals(assembly.GetName().Name, "System.Windows.Forms"))
			{
				Type type = assembly.GetType("System.Windows.Forms.XplatUICarbon");
				if (type != null)
				{
					hwnd_delegate = (Delegate)type.GetTypeInfo().GetField("HwndDelegate", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
				}
			}
		}
	}

	internal static CocoaContext GetCGContextForNSView(IntPtr handle)
	{
		if (handle == IntPtr.Zero)
		{
			return null;
		}
		IntPtr intPtr = objc_msgSend(objc_getClass("NSView"), sel_registerName("focusView"));
		IntPtr focusHandle = IntPtr.Zero;
		if (intPtr != handle)
		{
			if (!bool_objc_msgSend(handle, sel_registerName("lockFocusIfCanDraw")))
			{
				return null;
			}
			focusHandle = handle;
		}
		IntPtr intPtr2 = objc_msgSend(objc_msgSend(objc_msgSend(handle, sel_registerName("window")), sel_registerName("graphicsContext")), sel_registerName("graphicsPort"));
		bool flag = bool_objc_msgSend(handle, sel_registerName("isFlipped"));
		CGContextSaveGState(intPtr2);
		Size size;
		if (IntPtr.Size == 4)
		{
			CGRect32 arect = default(CGRect32);
			objc_msgSend_stret(ref arect, handle, sel_registerName("bounds"));
			if (flag)
			{
				CGContextTranslateCTM32(intPtr2, arect.origin.x, arect.size.height);
				CGContextScaleCTM32(intPtr2, 1f, -1f);
			}
			size = new Size((int)arect.size.width, (int)arect.size.height);
		}
		else
		{
			CGRect64 arect2 = default(CGRect64);
			objc_msgSend_stret(ref arect2, handle, sel_registerName("bounds"));
			if (flag)
			{
				CGContextTranslateCTM64(intPtr2, arect2.origin.x, arect2.size.height);
				CGContextScaleCTM64(intPtr2, 1.0, -1.0);
			}
			size = new Size((int)arect2.size.width, (int)arect2.size.height);
		}
		return new CocoaContext(focusHandle, intPtr2, size.Width, size.Height);
	}

	internal static CarbonContext GetCGContextForView(IntPtr handle)
	{
		IntPtr context = IntPtr.Zero;
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		if (IntPtr.Size == 8)
		{
			throw new NotSupportedException();
		}
		zero2 = GetControlOwner(handle);
		if (handle == IntPtr.Zero || zero2 == IntPtr.Zero)
		{
			zero = GetQDGlobalsThePort();
			CreateCGContextForPort(zero, ref context);
			CGRect32 cGRect = CGDisplayBounds32(CGMainDisplayID());
			return new CarbonContext(zero, context, (int)cGRect.size.width, (int)cGRect.size.height);
		}
		QDRect rect = default(QDRect);
		CGRect32 r = default(CGRect32);
		zero = GetWindowPort(zero2);
		context = GetContext(zero);
		GetWindowBounds(zero2, 32u, ref rect);
		HIViewGetBounds(handle, ref r);
		HIViewConvertRect(ref r, handle, IntPtr.Zero);
		if (r.size.height < 0f)
		{
			r.size.height = 0f;
		}
		if (r.size.width < 0f)
		{
			r.size.width = 0f;
		}
		CGContextTranslateCTM32(context, r.origin.x, (float)(rect.bottom - rect.top) - (r.origin.y + r.size.height));
		CGRect32 rect2 = new CGRect32(0f, 0f, r.size.width, r.size.height);
		CGContextSaveGState(context);
		Rectangle[] array = (Rectangle[])hwnd_delegate.DynamicInvoke(handle);
		if (array != null && array.Length != 0)
		{
			int num = array.Length;
			CGContextBeginPath(context);
			CGContextAddRect32(context, rect2);
			for (int i = 0; i < num; i++)
			{
				CGContextAddRect32(context, new CGRect32(array[i].X, r.size.height - (float)array[i].Y - (float)array[i].Height, array[i].Width, array[i].Height));
			}
			CGContextClosePath(context);
			CGContextEOClip(context);
		}
		else
		{
			CGContextBeginPath(context);
			CGContextAddRect32(context, rect2);
			CGContextClosePath(context);
			CGContextClip(context);
		}
		return new CarbonContext(zero, context, (int)r.size.width, (int)r.size.height);
	}

	internal static IntPtr GetContext(IntPtr port)
	{
		IntPtr context = IntPtr.Zero;
		lock (lockobj)
		{
			CreateCGContextForPort(port, ref context);
			return context;
		}
	}

	internal static void ReleaseContext(IntPtr port, IntPtr context)
	{
		CGContextRestoreGState(context);
		lock (lockobj)
		{
			CFRelease(context);
		}
	}

	[DllImport("libobjc.dylib")]
	public static extern IntPtr objc_getClass(string className);

	[DllImport("libobjc.dylib")]
	public static extern IntPtr objc_msgSend(IntPtr basePtr, IntPtr selector, string argument);

	[DllImport("libobjc.dylib")]
	public static extern IntPtr objc_msgSend(IntPtr basePtr, IntPtr selector);

	[DllImport("libobjc.dylib")]
	public static extern void objc_msgSend_stret(ref CGRect32 arect, IntPtr basePtr, IntPtr selector);

	[DllImport("libobjc.dylib")]
	public static extern void objc_msgSend_stret(ref CGRect64 arect, IntPtr basePtr, IntPtr selector);

	[DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
	public static extern bool bool_objc_msgSend(IntPtr handle, IntPtr selector);

	[DllImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
	public static extern bool bool_objc_msgSend_IntPtr(IntPtr handle, IntPtr selector, IntPtr argument);

	[DllImport("libobjc.dylib")]
	public static extern IntPtr sel_registerName(string selectorName);

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
	internal static extern IntPtr CGMainDisplayID();

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon", EntryPoint = "CGDisplayBounds")]
	internal static extern CGRect32 CGDisplayBounds32(IntPtr display);

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
	internal static extern int HIViewGetBounds(IntPtr vHnd, ref CGRect32 r);

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
	internal static extern int HIViewConvertRect(ref CGRect32 r, IntPtr a, IntPtr b);

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
	internal static extern IntPtr GetControlOwner(IntPtr aView);

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
	internal static extern int GetWindowBounds(IntPtr wHnd, uint reg, ref QDRect rect);

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
	internal static extern IntPtr GetWindowPort(IntPtr hWnd);

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
	internal static extern IntPtr GetQDGlobalsThePort();

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
	internal static extern void CreateCGContextForPort(IntPtr port, ref IntPtr context);

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
	internal static extern void CFRelease(IntPtr context);

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
	internal static extern void QDBeginCGContext(IntPtr port, ref IntPtr context);

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
	internal static extern void QDEndCGContext(IntPtr port, ref IntPtr context);

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon", EntryPoint = "CGContextTranslateCTM")]
	internal static extern void CGContextTranslateCTM32(IntPtr context, float tx, float ty);

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon", EntryPoint = "CGContextScaleCTM")]
	internal static extern void CGContextScaleCTM32(IntPtr context, float x, float y);

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon", EntryPoint = "CGContextTranslateCTM")]
	internal static extern void CGContextTranslateCTM64(IntPtr context, double tx, double ty);

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon", EntryPoint = "CGContextScaleCTM")]
	internal static extern void CGContextScaleCTM64(IntPtr context, double x, double y);

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
	internal static extern void CGContextFlush(IntPtr context);

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
	internal static extern void CGContextSynchronize(IntPtr context);

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
	internal static extern IntPtr CGPathCreateMutable();

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon", EntryPoint = "CGContextAddRect")]
	internal static extern void CGContextAddRect32(IntPtr context, CGRect32 rect);

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
	internal static extern void CGContextBeginPath(IntPtr context);

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
	internal static extern void CGContextClosePath(IntPtr context);

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
	internal static extern void CGContextAddPath(IntPtr context, IntPtr path);

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
	internal static extern void CGContextClip(IntPtr context);

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
	internal static extern void CGContextEOClip(IntPtr context);

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
	internal static extern void CGContextEOFillPath(IntPtr context);

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
	internal static extern void CGContextSaveGState(IntPtr context);

	[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
	internal static extern void CGContextRestoreGState(IntPtr context);
}
