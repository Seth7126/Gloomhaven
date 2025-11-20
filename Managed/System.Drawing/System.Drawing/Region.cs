using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace System.Drawing;

/// <summary>Describes the interior of a graphics shape composed of rectangles and paths. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
public sealed class Region : MarshalByRefObject, IDisposable
{
	private IntPtr nativeRegion = IntPtr.Zero;

	internal IntPtr NativeObject
	{
		get
		{
			return nativeRegion;
		}
		set
		{
			nativeRegion = value;
		}
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.Region" />.</summary>
	public Region()
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCreateRegion(out nativeRegion));
	}

	internal Region(IntPtr native)
	{
		nativeRegion = native;
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.Region" /> with the specified <see cref="T:System.Drawing.Drawing2D.GraphicsPath" />.</summary>
	/// <param name="path">A <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> that defines the new <see cref="T:System.Drawing.Region" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> isnull.</exception>
	public Region(GraphicsPath path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipCreateRegionPath(path.nativePath, out nativeRegion));
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.Region" /> from the specified <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <param name="rect">A <see cref="T:System.Drawing.Rectangle" /> structure that defines the interior of the new <see cref="T:System.Drawing.Region" />. </param>
	public Region(Rectangle rect)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCreateRegionRectI(ref rect, out nativeRegion));
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.Region" /> from the specified <see cref="T:System.Drawing.RectangleF" /> structure.</summary>
	/// <param name="rect">A <see cref="T:System.Drawing.RectangleF" /> structure that defines the interior of the new <see cref="T:System.Drawing.Region" />. </param>
	public Region(RectangleF rect)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCreateRegionRect(ref rect, out nativeRegion));
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.Region" /> from the specified data.</summary>
	/// <param name="rgnData">A <see cref="T:System.Drawing.Drawing2D.RegionData" /> that defines the interior of the new <see cref="T:System.Drawing.Region" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="rgnData" /> isnull.</exception>
	public Region(RegionData rgnData)
	{
		if (rgnData == null)
		{
			throw new ArgumentNullException("rgnData");
		}
		if (rgnData.Data.Length == 0)
		{
			throw new ArgumentException("rgnData");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipCreateRegionRgnData(rgnData.Data, rgnData.Data.Length, out nativeRegion));
	}

	/// <summary>Updates this <see cref="T:System.Drawing.Region" /> to the union of itself and the specified <see cref="T:System.Drawing.Drawing2D.GraphicsPath" />.</summary>
	/// <param name="path">The <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> to unite with this <see cref="T:System.Drawing.Region" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Union(GraphicsPath path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipCombineRegionPath(nativeRegion, path.nativePath, CombineMode.Union));
	}

	/// <summary>Updates this <see cref="T:System.Drawing.Region" /> to the union of itself and the specified <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <param name="rect">The <see cref="T:System.Drawing.Rectangle" /> structure to unite with this <see cref="T:System.Drawing.Region" />. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Union(Rectangle rect)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCombineRegionRectI(nativeRegion, ref rect, CombineMode.Union));
	}

	/// <summary>Updates this <see cref="T:System.Drawing.Region" /> to the union of itself and the specified <see cref="T:System.Drawing.RectangleF" /> structure.</summary>
	/// <param name="rect">The <see cref="T:System.Drawing.RectangleF" /> structure to unite with this <see cref="T:System.Drawing.Region" />. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Union(RectangleF rect)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCombineRegionRect(nativeRegion, ref rect, CombineMode.Union));
	}

	/// <summary>Updates this <see cref="T:System.Drawing.Region" /> to the union of itself and the specified <see cref="T:System.Drawing.Region" />.</summary>
	/// <param name="region">The <see cref="T:System.Drawing.Region" /> to unite with this <see cref="T:System.Drawing.Region" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="region" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Union(Region region)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipCombineRegionRegion(nativeRegion, region.NativeObject, CombineMode.Union));
	}

	/// <summary>Updates this <see cref="T:System.Drawing.Region" /> to the intersection of itself with the specified <see cref="T:System.Drawing.Drawing2D.GraphicsPath" />.</summary>
	/// <param name="path">The <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> to intersect with this <see cref="T:System.Drawing.Region" />. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Intersect(GraphicsPath path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipCombineRegionPath(nativeRegion, path.nativePath, CombineMode.Intersect));
	}

	/// <summary>Updates this <see cref="T:System.Drawing.Region" /> to the intersection of itself with the specified <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <param name="rect">The <see cref="T:System.Drawing.Rectangle" /> structure to intersect with this <see cref="T:System.Drawing.Region" />. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Intersect(Rectangle rect)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCombineRegionRectI(nativeRegion, ref rect, CombineMode.Intersect));
	}

	/// <summary>Updates this <see cref="T:System.Drawing.Region" /> to the intersection of itself with the specified <see cref="T:System.Drawing.RectangleF" /> structure.</summary>
	/// <param name="rect">The <see cref="T:System.Drawing.RectangleF" /> structure to intersect with this <see cref="T:System.Drawing.Region" />. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Intersect(RectangleF rect)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCombineRegionRect(nativeRegion, ref rect, CombineMode.Intersect));
	}

	/// <summary>Updates this <see cref="T:System.Drawing.Region" /> to the intersection of itself with the specified <see cref="T:System.Drawing.Region" />.</summary>
	/// <param name="region">The <see cref="T:System.Drawing.Region" /> to intersect with this <see cref="T:System.Drawing.Region" />. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Intersect(Region region)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipCombineRegionRegion(nativeRegion, region.NativeObject, CombineMode.Intersect));
	}

	/// <summary>Updates this <see cref="T:System.Drawing.Region" /> to contain the portion of the specified <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> that does not intersect with this <see cref="T:System.Drawing.Region" />.</summary>
	/// <param name="path">The <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> to complement this <see cref="T:System.Drawing.Region" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> isnull.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Complement(GraphicsPath path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipCombineRegionPath(nativeRegion, path.nativePath, CombineMode.Complement));
	}

	/// <summary>Updates this <see cref="T:System.Drawing.Region" /> to contain the portion of the specified <see cref="T:System.Drawing.Rectangle" /> structure that does not intersect with this <see cref="T:System.Drawing.Region" />.</summary>
	/// <param name="rect">The <see cref="T:System.Drawing.Rectangle" /> structure to complement this <see cref="T:System.Drawing.Region" />. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Complement(Rectangle rect)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCombineRegionRectI(nativeRegion, ref rect, CombineMode.Complement));
	}

	/// <summary>Updates this <see cref="T:System.Drawing.Region" /> to contain the portion of the specified <see cref="T:System.Drawing.RectangleF" /> structure that does not intersect with this <see cref="T:System.Drawing.Region" />.</summary>
	/// <param name="rect">The <see cref="T:System.Drawing.RectangleF" /> structure to complement this <see cref="T:System.Drawing.Region" />. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Complement(RectangleF rect)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCombineRegionRect(nativeRegion, ref rect, CombineMode.Complement));
	}

	/// <summary>Updates this <see cref="T:System.Drawing.Region" /> to contain the portion of the specified <see cref="T:System.Drawing.Region" /> that does not intersect with this <see cref="T:System.Drawing.Region" />.</summary>
	/// <param name="region">The <see cref="T:System.Drawing.Region" /> object to complement this <see cref="T:System.Drawing.Region" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="region" /> isnull.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Complement(Region region)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipCombineRegionRegion(nativeRegion, region.NativeObject, CombineMode.Complement));
	}

	/// <summary>Updates this <see cref="T:System.Drawing.Region" /> to contain only the portion of its interior that does not intersect with the specified <see cref="T:System.Drawing.Drawing2D.GraphicsPath" />.</summary>
	/// <param name="path">The <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> to exclude from this <see cref="T:System.Drawing.Region" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Exclude(GraphicsPath path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipCombineRegionPath(nativeRegion, path.nativePath, CombineMode.Exclude));
	}

	/// <summary>Updates this <see cref="T:System.Drawing.Region" /> to contain only the portion of its interior that does not intersect with the specified <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <param name="rect">The <see cref="T:System.Drawing.Rectangle" /> structure to exclude from this <see cref="T:System.Drawing.Region" />. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Exclude(Rectangle rect)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCombineRegionRectI(nativeRegion, ref rect, CombineMode.Exclude));
	}

	/// <summary>Updates this <see cref="T:System.Drawing.Region" /> to contain only the portion of its interior that does not intersect with the specified <see cref="T:System.Drawing.RectangleF" /> structure.</summary>
	/// <param name="rect">The <see cref="T:System.Drawing.RectangleF" /> structure to exclude from this <see cref="T:System.Drawing.Region" />. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Exclude(RectangleF rect)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCombineRegionRect(nativeRegion, ref rect, CombineMode.Exclude));
	}

	/// <summary>Updates this <see cref="T:System.Drawing.Region" /> to contain only the portion of its interior that does not intersect with the specified <see cref="T:System.Drawing.Region" />.</summary>
	/// <param name="region">The <see cref="T:System.Drawing.Region" /> to exclude from this <see cref="T:System.Drawing.Region" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="region" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Exclude(Region region)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipCombineRegionRegion(nativeRegion, region.NativeObject, CombineMode.Exclude));
	}

	/// <summary>Updates this <see cref="T:System.Drawing.Region" /> to the union minus the intersection of itself with the specified <see cref="T:System.Drawing.Drawing2D.GraphicsPath" />.</summary>
	/// <param name="path">The <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> to <see cref="Overload:System.Drawing.Region.Xor" /> with this <see cref="T:System.Drawing.Region" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Xor(GraphicsPath path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipCombineRegionPath(nativeRegion, path.nativePath, CombineMode.Xor));
	}

	/// <summary>Updates this <see cref="T:System.Drawing.Region" /> to the union minus the intersection of itself with the specified <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <param name="rect">The <see cref="T:System.Drawing.Rectangle" /> structure to <see cref="Overload:System.Drawing.Region.Xor" /> with this <see cref="T:System.Drawing.Region" />. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Xor(Rectangle rect)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCombineRegionRectI(nativeRegion, ref rect, CombineMode.Xor));
	}

	/// <summary>Updates this <see cref="T:System.Drawing.Region" /> to the union minus the intersection of itself with the specified <see cref="T:System.Drawing.RectangleF" /> structure.</summary>
	/// <param name="rect">The <see cref="T:System.Drawing.RectangleF" /> structure to <see cref="M:System.Drawing.Region.Xor(System.Drawing.Drawing2D.GraphicsPath)" /> with this <see cref="T:System.Drawing.Region" />. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Xor(RectangleF rect)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCombineRegionRect(nativeRegion, ref rect, CombineMode.Xor));
	}

	/// <summary>Updates this <see cref="T:System.Drawing.Region" /> to the union minus the intersection of itself with the specified <see cref="T:System.Drawing.Region" />.</summary>
	/// <param name="region">The <see cref="T:System.Drawing.Region" /> to <see cref="Overload:System.Drawing.Region.Xor" /> with this <see cref="T:System.Drawing.Region" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="region" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Xor(Region region)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipCombineRegionRegion(nativeRegion, region.NativeObject, CombineMode.Xor));
	}

	/// <summary>Gets a <see cref="T:System.Drawing.RectangleF" /> structure that represents a rectangle that bounds this <see cref="T:System.Drawing.Region" /> on the drawing surface of a <see cref="T:System.Drawing.Graphics" /> object.</summary>
	/// <returns>A <see cref="T:System.Drawing.RectangleF" /> structure that represents the bounding rectangle for this <see cref="T:System.Drawing.Region" /> on the specified drawing surface.</returns>
	/// <param name="g">The <see cref="T:System.Drawing.Graphics" /> on which this <see cref="T:System.Drawing.Region" /> is drawn. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="g" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public RectangleF GetBounds(Graphics g)
	{
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		RectangleF rect = default(Rectangle);
		GDIPlus.CheckStatus(GDIPlus.GdipGetRegionBounds(nativeRegion, g.NativeObject, ref rect));
		return rect;
	}

	/// <summary>Offsets the coordinates of this <see cref="T:System.Drawing.Region" /> by the specified amount.</summary>
	/// <param name="dx">The amount to offset this <see cref="T:System.Drawing.Region" /> horizontally. </param>
	/// <param name="dy">The amount to offset this <see cref="T:System.Drawing.Region" /> vertically. </param>
	/// <filterpriority>1</filterpriority>
	public void Translate(int dx, int dy)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipTranslateRegionI(nativeRegion, dx, dy));
	}

	/// <summary>Offsets the coordinates of this <see cref="T:System.Drawing.Region" /> by the specified amount.</summary>
	/// <param name="dx">The amount to offset this <see cref="T:System.Drawing.Region" /> horizontally. </param>
	/// <param name="dy">The amount to offset this <see cref="T:System.Drawing.Region" /> vertically. </param>
	/// <filterpriority>1</filterpriority>
	public void Translate(float dx, float dy)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipTranslateRegion(nativeRegion, dx, dy));
	}

	/// <summary>Tests whether the specified point is contained within this <see cref="T:System.Drawing.Region" /> object when drawn using the specified <see cref="T:System.Drawing.Graphics" /> object.</summary>
	/// <returns>true when the specified point is contained within this <see cref="T:System.Drawing.Region" />; otherwise, false.</returns>
	/// <param name="x">The x-coordinate of the point to test. </param>
	/// <param name="y">The y-coordinate of the point to test. </param>
	/// <param name="g">A <see cref="T:System.Drawing.Graphics" /> that represents a graphics context. </param>
	/// <filterpriority>1</filterpriority>
	public bool IsVisible(int x, int y, Graphics g)
	{
		IntPtr graphics = g?.NativeObject ?? IntPtr.Zero;
		GDIPlus.CheckStatus(GDIPlus.GdipIsVisibleRegionPointI(nativeRegion, x, y, graphics, out var result));
		return result;
	}

	/// <summary>Tests whether any portion of the specified rectangle is contained within this <see cref="T:System.Drawing.Region" />.</summary>
	/// <returns>true when any portion of the specified rectangle is contained within this <see cref="T:System.Drawing.Region" />; otherwise, false.</returns>
	/// <param name="x">The x-coordinate of the upper-left corner of the rectangle to test. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the rectangle to test. </param>
	/// <param name="width">The width of the rectangle to test. </param>
	/// <param name="height">The height of the rectangle to test. </param>
	/// <filterpriority>1</filterpriority>
	public bool IsVisible(int x, int y, int width, int height)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipIsVisibleRegionRectI(nativeRegion, x, y, width, height, IntPtr.Zero, out var result));
		return result;
	}

	/// <summary>Tests whether any portion of the specified rectangle is contained within this <see cref="T:System.Drawing.Region" /> when drawn using the specified <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>true when any portion of the specified rectangle is contained within this <see cref="T:System.Drawing.Region" />; otherwise, false.</returns>
	/// <param name="x">The x-coordinate of the upper-left corner of the rectangle to test. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the rectangle to test. </param>
	/// <param name="width">The width of the rectangle to test. </param>
	/// <param name="height">The height of the rectangle to test. </param>
	/// <param name="g">A <see cref="T:System.Drawing.Graphics" /> that represents a graphics context. </param>
	/// <filterpriority>1</filterpriority>
	public bool IsVisible(int x, int y, int width, int height, Graphics g)
	{
		IntPtr graphics = g?.NativeObject ?? IntPtr.Zero;
		GDIPlus.CheckStatus(GDIPlus.GdipIsVisibleRegionRectI(nativeRegion, x, y, width, height, graphics, out var result));
		return result;
	}

	/// <summary>Tests whether the specified <see cref="T:System.Drawing.Point" /> structure is contained within this <see cref="T:System.Drawing.Region" />.</summary>
	/// <returns>true when <paramref name="point" /> is contained within this <see cref="T:System.Drawing.Region" />; otherwise, false.</returns>
	/// <param name="point">The <see cref="T:System.Drawing.Point" /> structure to test. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool IsVisible(Point point)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipIsVisibleRegionPointI(nativeRegion, point.X, point.Y, IntPtr.Zero, out var result));
		return result;
	}

	/// <summary>Tests whether the specified <see cref="T:System.Drawing.PointF" /> structure is contained within this <see cref="T:System.Drawing.Region" />.</summary>
	/// <returns>true when <paramref name="point" /> is contained within this <see cref="T:System.Drawing.Region" />; otherwise, false.</returns>
	/// <param name="point">The <see cref="T:System.Drawing.PointF" /> structure to test. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool IsVisible(PointF point)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipIsVisibleRegionPoint(nativeRegion, point.X, point.Y, IntPtr.Zero, out var result));
		return result;
	}

	/// <summary>Tests whether the specified <see cref="T:System.Drawing.Point" /> structure is contained within this <see cref="T:System.Drawing.Region" /> when drawn using the specified <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>true when <paramref name="point" /> is contained within this <see cref="T:System.Drawing.Region" />; otherwise, false.</returns>
	/// <param name="point">The <see cref="T:System.Drawing.Point" /> structure to test. </param>
	/// <param name="g">A <see cref="T:System.Drawing.Graphics" /> that represents a graphics context. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool IsVisible(Point point, Graphics g)
	{
		IntPtr graphics = g?.NativeObject ?? IntPtr.Zero;
		GDIPlus.CheckStatus(GDIPlus.GdipIsVisibleRegionPointI(nativeRegion, point.X, point.Y, graphics, out var result));
		return result;
	}

	/// <summary>Tests whether the specified <see cref="T:System.Drawing.PointF" /> structure is contained within this <see cref="T:System.Drawing.Region" /> when drawn using the specified <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>true when <paramref name="point" /> is contained within this <see cref="T:System.Drawing.Region" />; otherwise, false.</returns>
	/// <param name="point">The <see cref="T:System.Drawing.PointF" /> structure to test. </param>
	/// <param name="g">A <see cref="T:System.Drawing.Graphics" /> that represents a graphics context. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool IsVisible(PointF point, Graphics g)
	{
		IntPtr graphics = g?.NativeObject ?? IntPtr.Zero;
		GDIPlus.CheckStatus(GDIPlus.GdipIsVisibleRegionPoint(nativeRegion, point.X, point.Y, graphics, out var result));
		return result;
	}

	/// <summary>Tests whether any portion of the specified <see cref="T:System.Drawing.Rectangle" /> structure is contained within this <see cref="T:System.Drawing.Region" />.</summary>
	/// <returns>This method returns true when any portion of <paramref name="rect" /> is contained within this <see cref="T:System.Drawing.Region" />; otherwise, false.</returns>
	/// <param name="rect">The <see cref="T:System.Drawing.Rectangle" /> structure to test. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool IsVisible(Rectangle rect)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipIsVisibleRegionRectI(nativeRegion, rect.X, rect.Y, rect.Width, rect.Height, IntPtr.Zero, out var result));
		return result;
	}

	/// <summary>Tests whether any portion of the specified <see cref="T:System.Drawing.RectangleF" /> structure is contained within this <see cref="T:System.Drawing.Region" />.</summary>
	/// <returns>true when any portion of <paramref name="rect" /> is contained within this <see cref="T:System.Drawing.Region" />; otherwise, false.</returns>
	/// <param name="rect">The <see cref="T:System.Drawing.RectangleF" /> structure to test. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool IsVisible(RectangleF rect)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipIsVisibleRegionRect(nativeRegion, rect.X, rect.Y, rect.Width, rect.Height, IntPtr.Zero, out var result));
		return result;
	}

	/// <summary>Tests whether any portion of the specified <see cref="T:System.Drawing.Rectangle" /> structure is contained within this <see cref="T:System.Drawing.Region" /> when drawn using the specified <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>true when any portion of the <paramref name="rect" /> is contained within this <see cref="T:System.Drawing.Region" />; otherwise, false.</returns>
	/// <param name="rect">The <see cref="T:System.Drawing.Rectangle" /> structure to test. </param>
	/// <param name="g">A <see cref="T:System.Drawing.Graphics" /> that represents a graphics context. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool IsVisible(Rectangle rect, Graphics g)
	{
		IntPtr graphics = g?.NativeObject ?? IntPtr.Zero;
		GDIPlus.CheckStatus(GDIPlus.GdipIsVisibleRegionRectI(nativeRegion, rect.X, rect.Y, rect.Width, rect.Height, graphics, out var result));
		return result;
	}

	/// <summary>Tests whether any portion of the specified <see cref="T:System.Drawing.RectangleF" /> structure is contained within this <see cref="T:System.Drawing.Region" /> when drawn using the specified <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>true when <paramref name="rect" /> is contained within this <see cref="T:System.Drawing.Region" />; otherwise, false.</returns>
	/// <param name="rect">The <see cref="T:System.Drawing.RectangleF" /> structure to test. </param>
	/// <param name="g">A <see cref="T:System.Drawing.Graphics" /> that represents a graphics context. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool IsVisible(RectangleF rect, Graphics g)
	{
		IntPtr graphics = g?.NativeObject ?? IntPtr.Zero;
		GDIPlus.CheckStatus(GDIPlus.GdipIsVisibleRegionRect(nativeRegion, rect.X, rect.Y, rect.Width, rect.Height, graphics, out var result));
		return result;
	}

	/// <summary>Tests whether the specified point is contained within this <see cref="T:System.Drawing.Region" />.</summary>
	/// <returns>true when the specified point is contained within this <see cref="T:System.Drawing.Region" />; otherwise, false.</returns>
	/// <param name="x">The x-coordinate of the point to test. </param>
	/// <param name="y">The y-coordinate of the point to test. </param>
	/// <filterpriority>1</filterpriority>
	public bool IsVisible(float x, float y)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipIsVisibleRegionPoint(nativeRegion, x, y, IntPtr.Zero, out var result));
		return result;
	}

	/// <summary>Tests whether the specified point is contained within this <see cref="T:System.Drawing.Region" /> when drawn using the specified <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>true when the specified point is contained within this <see cref="T:System.Drawing.Region" />; otherwise, false.</returns>
	/// <param name="x">The x-coordinate of the point to test. </param>
	/// <param name="y">The y-coordinate of the point to test. </param>
	/// <param name="g">A <see cref="T:System.Drawing.Graphics" /> that represents a graphics context. </param>
	/// <filterpriority>1</filterpriority>
	public bool IsVisible(float x, float y, Graphics g)
	{
		IntPtr graphics = g?.NativeObject ?? IntPtr.Zero;
		GDIPlus.CheckStatus(GDIPlus.GdipIsVisibleRegionPoint(nativeRegion, x, y, graphics, out var result));
		return result;
	}

	/// <summary>Tests whether any portion of the specified rectangle is contained within this <see cref="T:System.Drawing.Region" />.</summary>
	/// <returns>true when any portion of the specified rectangle is contained within this <see cref="T:System.Drawing.Region" /> object; otherwise, false.</returns>
	/// <param name="x">The x-coordinate of the upper-left corner of the rectangle to test. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the rectangle to test. </param>
	/// <param name="width">The width of the rectangle to test. </param>
	/// <param name="height">The height of the rectangle to test. </param>
	/// <filterpriority>1</filterpriority>
	public bool IsVisible(float x, float y, float width, float height)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipIsVisibleRegionRect(nativeRegion, x, y, width, height, IntPtr.Zero, out var result));
		return result;
	}

	/// <summary>Tests whether any portion of the specified rectangle is contained within this <see cref="T:System.Drawing.Region" /> when drawn using the specified <see cref="T:System.Drawing.Graphics" />.</summary>
	/// <returns>true when any portion of the specified rectangle is contained within this <see cref="T:System.Drawing.Region" />; otherwise, false.</returns>
	/// <param name="x">The x-coordinate of the upper-left corner of the rectangle to test. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the rectangle to test. </param>
	/// <param name="width">The width of the rectangle to test. </param>
	/// <param name="height">The height of the rectangle to test. </param>
	/// <param name="g">A <see cref="T:System.Drawing.Graphics" /> that represents a graphics context. </param>
	/// <filterpriority>1</filterpriority>
	public bool IsVisible(float x, float y, float width, float height, Graphics g)
	{
		IntPtr graphics = g?.NativeObject ?? IntPtr.Zero;
		GDIPlus.CheckStatus(GDIPlus.GdipIsVisibleRegionRect(nativeRegion, x, y, width, height, graphics, out var result));
		return result;
	}

	/// <summary>Tests whether this <see cref="T:System.Drawing.Region" /> has an empty interior on the specified drawing surface.</summary>
	/// <returns>true if the interior of this <see cref="T:System.Drawing.Region" /> is empty when the transformation associated with <paramref name="g" /> is applied; otherwise, false.</returns>
	/// <param name="g">A <see cref="T:System.Drawing.Graphics" /> that represents a drawing surface. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="g" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool IsEmpty(Graphics g)
	{
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipIsEmptyRegion(nativeRegion, g.NativeObject, out var result));
		return result;
	}

	/// <summary>Tests whether this <see cref="T:System.Drawing.Region" /> has an infinite interior on the specified drawing surface.</summary>
	/// <returns>true if the interior of this <see cref="T:System.Drawing.Region" /> is infinite when the transformation associated with <paramref name="g" /> is applied; otherwise, false.</returns>
	/// <param name="g">A <see cref="T:System.Drawing.Graphics" /> that represents a drawing surface. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="g" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool IsInfinite(Graphics g)
	{
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipIsInfiniteRegion(nativeRegion, g.NativeObject, out var result));
		return result;
	}

	/// <summary>Initializes this <see cref="T:System.Drawing.Region" /> to an empty interior.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void MakeEmpty()
	{
		GDIPlus.CheckStatus(GDIPlus.GdipSetEmpty(nativeRegion));
	}

	/// <summary>Initializes this <see cref="T:System.Drawing.Region" /> object to an infinite interior.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void MakeInfinite()
	{
		GDIPlus.CheckStatus(GDIPlus.GdipSetInfinite(nativeRegion));
	}

	/// <summary>Tests whether the specified <see cref="T:System.Drawing.Region" /> is identical to this <see cref="T:System.Drawing.Region" /> on the specified drawing surface.</summary>
	/// <returns>true if the interior of region is identical to the interior of this region when the transformation associated with the <paramref name="g" /> parameter is applied; otherwise, false.</returns>
	/// <param name="region">The <see cref="T:System.Drawing.Region" /> to test. </param>
	/// <param name="g">A <see cref="T:System.Drawing.Graphics" /> that represents a drawing surface. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="g" /> or <paramref name="region" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool Equals(Region region, Graphics g)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipIsEqualRegion(nativeRegion, region.NativeObject, g.NativeObject, out var result));
		return result;
	}

	/// <summary>Initializes a new <see cref="T:System.Drawing.Region" /> from a handle to the specified existing GDI region.</summary>
	/// <returns>The new <see cref="T:System.Drawing.Region" />.</returns>
	/// <param name="hrgn">A handle to an existing <see cref="T:System.Drawing.Region" />. </param>
	/// <filterpriority>1</filterpriority>
	public static Region FromHrgn(IntPtr hrgn)
	{
		if (hrgn == IntPtr.Zero)
		{
			throw new ArgumentException("hrgn");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipCreateRegionHrgn(hrgn, out var region));
		return new Region(region);
	}

	/// <summary>Returns a Windows handle to this <see cref="T:System.Drawing.Region" /> in the specified graphics context.</summary>
	/// <returns>A Windows handle to this <see cref="T:System.Drawing.Region" />.</returns>
	/// <param name="g">The <see cref="T:System.Drawing.Graphics" /> on which this <see cref="T:System.Drawing.Region" /> is drawn. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="g" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public IntPtr GetHrgn(Graphics g)
	{
		if (g == null)
		{
			return nativeRegion;
		}
		IntPtr hRgn = IntPtr.Zero;
		GDIPlus.CheckStatus(GDIPlus.GdipGetRegionHRgn(nativeRegion, g.NativeObject, ref hRgn));
		return hRgn;
	}

	/// <summary>Returns a <see cref="T:System.Drawing.Drawing2D.RegionData" /> that represents the information that describes this <see cref="T:System.Drawing.Region" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.Drawing2D.RegionData" /> that represents the information that describes this <see cref="T:System.Drawing.Region" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public RegionData GetRegionData()
	{
		GDIPlus.CheckStatus(GDIPlus.GdipGetRegionDataSize(nativeRegion, out var bufferSize));
		byte[] array = new byte[bufferSize];
		GDIPlus.CheckStatus(GDIPlus.GdipGetRegionData(nativeRegion, array, bufferSize, out var _));
		return new RegionData(array);
	}

	/// <summary>Returns an array of <see cref="T:System.Drawing.RectangleF" /> structures that approximate this <see cref="T:System.Drawing.Region" /> after the specified matrix transformation is applied.</summary>
	/// <returns>An array of <see cref="T:System.Drawing.RectangleF" /> structures that approximate this <see cref="T:System.Drawing.Region" /> after the specified matrix transformation is applied.</returns>
	/// <param name="matrix">A <see cref="T:System.Drawing.Drawing2D.Matrix" /> that represents a geometric transformation to apply to the region. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="matrix" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public RectangleF[] GetRegionScans(Matrix matrix)
	{
		if (matrix == null)
		{
			throw new ArgumentNullException("matrix");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipGetRegionScansCount(nativeRegion, out var count, matrix.NativeObject));
		if (count == 0)
		{
			return new RectangleF[0];
		}
		RectangleF[] array = new RectangleF[count];
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(array[0]) * count);
		try
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetRegionScans(nativeRegion, intPtr, out count, matrix.NativeObject));
		}
		finally
		{
			GDIPlus.FromUnManagedMemoryToRectangles(intPtr, array);
		}
		return array;
	}

	/// <summary>Transforms this <see cref="T:System.Drawing.Region" /> by the specified <see cref="T:System.Drawing.Drawing2D.Matrix" />.</summary>
	/// <param name="matrix">The <see cref="T:System.Drawing.Drawing2D.Matrix" /> by which to transform this <see cref="T:System.Drawing.Region" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="matrix" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Transform(Matrix matrix)
	{
		if (matrix == null)
		{
			throw new ArgumentNullException("matrix");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipTransformRegion(nativeRegion, matrix.NativeObject));
	}

	/// <summary>Creates an exact copy of this <see cref="T:System.Drawing.Region" />.</summary>
	/// <returns>The <see cref="T:System.Drawing.Region" /> that this method creates.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public Region Clone()
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCloneRegion(nativeRegion, out var cloned));
		return new Region(cloned);
	}

	/// <summary>Releases all resources used by this <see cref="T:System.Drawing.Region" />.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Dispose()
	{
		DisposeHandle();
		GC.SuppressFinalize(this);
	}

	private void DisposeHandle()
	{
		if (nativeRegion != IntPtr.Zero)
		{
			GDIPlus.GdipDeleteRegion(nativeRegion);
			nativeRegion = IntPtr.Zero;
		}
	}

	~Region()
	{
		DisposeHandle();
	}

	/// <summary>Releases the handle of the <see cref="T:System.Drawing.Region" />.</summary>
	/// <param name="regionHandle">The handle to the <see cref="T:System.Drawing.Region" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="regionHandle" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public void ReleaseHrgn(IntPtr regionHandle)
	{
		if (regionHandle == IntPtr.Zero)
		{
			throw new ArgumentNullException("regionHandle");
		}
		Status status = Status.Ok;
		if (GDIPlus.RunningOnUnix())
		{
			status = GDIPlus.GdipDeleteRegion(regionHandle);
		}
		else if (!GDIPlus.DeleteObject(regionHandle))
		{
			status = Status.InvalidParameter;
		}
		GDIPlus.CheckStatus(status);
	}
}
