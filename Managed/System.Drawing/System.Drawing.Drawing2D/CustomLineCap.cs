using System.Runtime.InteropServices;

namespace System.Drawing.Drawing2D;

/// <summary>Encapsulates a custom user-defined line cap.</summary>
public class CustomLineCap : MarshalByRefObject, ICloneable, IDisposable
{
	internal SafeCustomLineCapHandle nativeCap;

	private bool _disposed;

	/// <summary>Gets or sets the <see cref="T:System.Drawing.Drawing2D.LineJoin" /> enumeration that determines how lines that compose this <see cref="T:System.Drawing.Drawing2D.CustomLineCap" /> object are joined.</summary>
	/// <returns>The <see cref="T:System.Drawing.Drawing2D.LineJoin" /> enumeration this <see cref="T:System.Drawing.Drawing2D.CustomLineCap" /> object uses to join lines.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public LineJoin StrokeJoin
	{
		get
		{
			LineJoin lineJoin;
			int num = GDIPlus.GdipGetCustomLineCapStrokeJoin(new HandleRef(this, nativeCap), out lineJoin);
			if (num != 0)
			{
				throw SafeNativeMethods.Gdip.StatusException(num);
			}
			return lineJoin;
		}
		set
		{
			int num = GDIPlus.GdipSetCustomLineCapStrokeJoin(new HandleRef(this, nativeCap), value);
			if (num != 0)
			{
				throw SafeNativeMethods.Gdip.StatusException(num);
			}
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Drawing.Drawing2D.LineCap" /> enumeration on which this <see cref="T:System.Drawing.Drawing2D.CustomLineCap" /> is based.</summary>
	/// <returns>The <see cref="T:System.Drawing.Drawing2D.LineCap" /> enumeration on which this <see cref="T:System.Drawing.Drawing2D.CustomLineCap" /> is based.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public LineCap BaseCap
	{
		get
		{
			LineCap baseCap;
			int num = GDIPlus.GdipGetCustomLineCapBaseCap(new HandleRef(this, nativeCap), out baseCap);
			if (num != 0)
			{
				throw SafeNativeMethods.Gdip.StatusException(num);
			}
			return baseCap;
		}
		set
		{
			int num = GDIPlus.GdipSetCustomLineCapBaseCap(new HandleRef(this, nativeCap), value);
			if (num != 0)
			{
				throw SafeNativeMethods.Gdip.StatusException(num);
			}
		}
	}

	/// <summary>Gets or sets the distance between the cap and the line.</summary>
	/// <returns>The distance between the beginning of the cap and the end of the line.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public float BaseInset
	{
		get
		{
			float inset;
			int num = GDIPlus.GdipGetCustomLineCapBaseInset(new HandleRef(this, nativeCap), out inset);
			if (num != 0)
			{
				throw SafeNativeMethods.Gdip.StatusException(num);
			}
			return inset;
		}
		set
		{
			int num = GDIPlus.GdipSetCustomLineCapBaseInset(new HandleRef(this, nativeCap), value);
			if (num != 0)
			{
				throw SafeNativeMethods.Gdip.StatusException(num);
			}
		}
	}

	/// <summary>Gets or sets the amount by which to scale this <see cref="T:System.Drawing.Drawing2D.CustomLineCap" /> Class object with respect to the width of the <see cref="T:System.Drawing.Pen" /> object.</summary>
	/// <returns>The amount by which to scale the cap.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public float WidthScale
	{
		get
		{
			float widthScale;
			int num = GDIPlus.GdipGetCustomLineCapWidthScale(new HandleRef(this, nativeCap), out widthScale);
			if (num != 0)
			{
				throw SafeNativeMethods.Gdip.StatusException(num);
			}
			return widthScale;
		}
		set
		{
			int num = GDIPlus.GdipSetCustomLineCapWidthScale(new HandleRef(this, nativeCap), value);
			if (num != 0)
			{
				throw SafeNativeMethods.Gdip.StatusException(num);
			}
		}
	}

	internal static CustomLineCap CreateCustomLineCapObject(IntPtr cap)
	{
		return new CustomLineCap(cap);
	}

	internal CustomLineCap()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Drawing2D.CustomLineCap" /> class with the specified outline and fill.</summary>
	/// <param name="fillPath">A <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> object that defines the fill for the custom cap. </param>
	/// <param name="strokePath">A <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> object that defines the outline of the custom cap. </param>
	public CustomLineCap(GraphicsPath fillPath, GraphicsPath strokePath)
		: this(fillPath, strokePath, LineCap.Flat)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Drawing2D.CustomLineCap" /> class from the specified existing <see cref="T:System.Drawing.Drawing2D.LineCap" /> enumeration with the specified outline and fill.</summary>
	/// <param name="fillPath">A <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> object that defines the fill for the custom cap. </param>
	/// <param name="strokePath">A <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> object that defines the outline of the custom cap. </param>
	/// <param name="baseCap">The line cap from which to create the custom cap. </param>
	public CustomLineCap(GraphicsPath fillPath, GraphicsPath strokePath, LineCap baseCap)
		: this(fillPath, strokePath, baseCap, 0f)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Drawing2D.CustomLineCap" /> class from the specified existing <see cref="T:System.Drawing.Drawing2D.LineCap" /> enumeration with the specified outline, fill, and inset.</summary>
	/// <param name="fillPath">A <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> object that defines the fill for the custom cap. </param>
	/// <param name="strokePath">A <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> object that defines the outline of the custom cap. </param>
	/// <param name="baseCap">The line cap from which to create the custom cap. </param>
	/// <param name="baseInset">The distance between the cap and the line. </param>
	public CustomLineCap(GraphicsPath fillPath, GraphicsPath strokePath, LineCap baseCap, float baseInset)
	{
		IntPtr customCap;
		int num = GDIPlus.GdipCreateCustomLineCap(new HandleRef(fillPath, fillPath?.nativePath ?? IntPtr.Zero), new HandleRef(strokePath, strokePath?.nativePath ?? IntPtr.Zero), baseCap, baseInset, out customCap);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
		SetNativeLineCap(customCap);
	}

	internal CustomLineCap(IntPtr nativeLineCap)
	{
		SetNativeLineCap(nativeLineCap);
	}

	internal void SetNativeLineCap(IntPtr handle)
	{
		if (handle == IntPtr.Zero)
		{
			throw new ArgumentNullException("handle");
		}
		nativeCap = new SafeCustomLineCapHandle(handle);
	}

	/// <summary>Releases all resources used by this <see cref="T:System.Drawing.Drawing2D.CustomLineCap" /> object.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Drawing.Drawing2D.CustomLineCap" /> and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing && nativeCap != null)
			{
				nativeCap.Dispose();
			}
			_disposed = true;
		}
	}

	/// <summary>Allows an <see cref="T:System.Drawing.Drawing2D.CustomLineCap" /> to attempt to free resources and perform other cleanup operations before the <see cref="T:System.Drawing.Drawing2D.CustomLineCap" /> is reclaimed by garbage collection.</summary>
	~CustomLineCap()
	{
		Dispose(disposing: false);
	}

	/// <summary>Creates an exact copy of this <see cref="T:System.Drawing.Drawing2D.CustomLineCap" />.</summary>
	/// <returns>The <see cref="T:System.Drawing.Drawing2D.CustomLineCap" /> this method creates, cast as an object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public object Clone()
	{
		return CoreClone();
	}

	internal virtual object CoreClone()
	{
		IntPtr clonedCap;
		int num = GDIPlus.GdipCloneCustomLineCap(new HandleRef(this, nativeCap), out clonedCap);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
		return CreateCustomLineCapObject(clonedCap);
	}

	/// <summary>Sets the caps used to start and end lines that make up this custom cap.</summary>
	/// <param name="startCap">The <see cref="T:System.Drawing.Drawing2D.LineCap" /> enumeration used at the beginning of a line within this cap. </param>
	/// <param name="endCap">The <see cref="T:System.Drawing.Drawing2D.LineCap" /> enumeration used at the end of a line within this cap. </param>
	public void SetStrokeCaps(LineCap startCap, LineCap endCap)
	{
		int num = GDIPlus.GdipSetCustomLineCapStrokeCaps(new HandleRef(this, nativeCap), startCap, endCap);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
	}

	/// <summary>Gets the caps used to start and end lines that make up this custom cap.</summary>
	/// <param name="startCap">The <see cref="T:System.Drawing.Drawing2D.LineCap" /> enumeration used at the beginning of a line within this cap. </param>
	/// <param name="endCap">The <see cref="T:System.Drawing.Drawing2D.LineCap" /> enumeration used at the end of a line within this cap. </param>
	public void GetStrokeCaps(out LineCap startCap, out LineCap endCap)
	{
		int num = GDIPlus.GdipGetCustomLineCapStrokeCaps(new HandleRef(this, nativeCap), out startCap, out endCap);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
	}
}
