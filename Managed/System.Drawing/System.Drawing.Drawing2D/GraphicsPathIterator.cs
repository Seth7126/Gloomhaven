using System.Drawing.Internal;
using System.Runtime.InteropServices;

namespace System.Drawing.Drawing2D;

/// <summary>Provides the ability to iterate through subpaths in a <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> and test the types of shapes contained in each subpath. This class cannot be inherited.</summary>
public sealed class GraphicsPathIterator : MarshalByRefObject, IDisposable
{
	internal IntPtr nativeIter;

	/// <summary>Gets the number of points in the path.</summary>
	/// <returns>The number of points in the path.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public int Count
	{
		get
		{
			int count;
			int num = GDIPlus.GdipPathIterGetCount(new HandleRef(this, nativeIter), out count);
			if (num != 0)
			{
				throw SafeNativeMethods.Gdip.StatusException(num);
			}
			return count;
		}
	}

	/// <summary>Gets the number of subpaths in the path.</summary>
	/// <returns>The number of subpaths in the path.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public int SubpathCount
	{
		get
		{
			int count;
			int num = GDIPlus.GdipPathIterGetSubpathCount(new HandleRef(this, nativeIter), out count);
			if (num != 0)
			{
				throw SafeNativeMethods.Gdip.StatusException(num);
			}
			return count;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Drawing2D.GraphicsPathIterator" /> class with the specified <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> object.</summary>
	/// <param name="path">The <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> object for which this helper class is to be initialized. </param>
	public GraphicsPathIterator(GraphicsPath path)
	{
		IntPtr iterator = IntPtr.Zero;
		int num = GDIPlus.GdipCreatePathIter(out iterator, new HandleRef(path, path?.nativePath ?? IntPtr.Zero));
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
		nativeIter = iterator;
	}

	/// <summary>Releases all resources used by this <see cref="T:System.Drawing.Drawing2D.GraphicsPathIterator" /> object.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (!(nativeIter != IntPtr.Zero))
		{
			return;
		}
		try
		{
			GDIPlus.GdipDeletePathIter(new HandleRef(this, nativeIter));
		}
		catch (Exception ex)
		{
			if (ClientUtils.IsSecurityOrCriticalException(ex))
			{
				throw;
			}
		}
		finally
		{
			nativeIter = IntPtr.Zero;
		}
	}

	~GraphicsPathIterator()
	{
		Dispose(disposing: false);
	}

	/// <summary>Moves the <see cref="T:System.Drawing.Drawing2D.GraphicsPathIterator" /> to the next subpath in the path. The start index and end index of the next subpath are contained in the [out] parameters.</summary>
	/// <returns>The number of subpaths in the <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> object.</returns>
	/// <param name="startIndex">[out] Receives the starting index of the next subpath. </param>
	/// <param name="endIndex">[out] Receives the ending index of the next subpath. </param>
	/// <param name="isClosed">[out] Indicates whether the subpath is closed. </param>
	public int NextSubpath(out int startIndex, out int endIndex, out bool isClosed)
	{
		int resultCount;
		int startIndex2;
		int endIndex2;
		int num = GDIPlus.GdipPathIterNextSubpath(new HandleRef(this, nativeIter), out resultCount, out startIndex2, out endIndex2, out isClosed);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
		startIndex = startIndex2;
		endIndex = endIndex2;
		return resultCount;
	}

	/// <summary>Gets the next figure (subpath) from the associated path of this <see cref="T:System.Drawing.Drawing2D.GraphicsPathIterator" />.</summary>
	/// <returns>The number of data points in the retrieved figure (subpath). If there are no more figures to retrieve, zero is returned.</returns>
	/// <param name="path">A <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> that is to have its data points set to match the data points of the retrieved figure (subpath) for this iterator. </param>
	/// <param name="isClosed">[out] Indicates whether the current subpath is closed. It is true if the if the figure is closed, otherwise it is false. </param>
	public int NextSubpath(GraphicsPath path, out bool isClosed)
	{
		int resultCount;
		int num = GDIPlus.GdipPathIterNextSubpathPath(new HandleRef(this, nativeIter), out resultCount, new HandleRef(path, path?.nativePath ?? IntPtr.Zero), out isClosed);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
		return resultCount;
	}

	/// <summary>Gets the starting index and the ending index of the next group of data points that all have the same type.</summary>
	/// <returns>This method returns the number of data points in the group. If there are no more groups in the path, this method returns 0.</returns>
	/// <param name="pathType">[out] Receives the point type shared by all points in the group. Possible types can be retrieved from the <see cref="T:System.Drawing.Drawing2D.PathPointType" /> enumeration. </param>
	/// <param name="startIndex">[out] Receives the starting index of the group of points. </param>
	/// <param name="endIndex">[out] Receives the ending index of the group of points. </param>
	public int NextPathType(out byte pathType, out int startIndex, out int endIndex)
	{
		int resultCount;
		int num = GDIPlus.GdipPathIterNextPathType(new HandleRef(this, nativeIter), out resultCount, out pathType, out startIndex, out endIndex);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
		return resultCount;
	}

	/// <summary>Increments the <see cref="T:System.Drawing.Drawing2D.GraphicsPathIterator" /> to the next marker in the path and returns the start and stop indexes by way of the [out] parameters.</summary>
	/// <returns>The number of points between this marker and the next.</returns>
	/// <param name="startIndex">[out] The integer reference supplied to this parameter receives the index of the point that starts a subpath. </param>
	/// <param name="endIndex">[out] The integer reference supplied to this parameter receives the index of the point that ends the subpath to which <paramref name="startIndex" /> points. </param>
	public int NextMarker(out int startIndex, out int endIndex)
	{
		int resultCount;
		int num = GDIPlus.GdipPathIterNextMarker(new HandleRef(this, nativeIter), out resultCount, out startIndex, out endIndex);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
		return resultCount;
	}

	/// <summary>This <see cref="T:System.Drawing.Drawing2D.GraphicsPathIterator" /> object has a <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> object associated with it. The <see cref="M:System.Drawing.Drawing2D.GraphicsPathIterator.NextMarker(System.Drawing.Drawing2D.GraphicsPath)" /> method increments the associated <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> to the next marker in its path and copies all the points contained between the current marker and the next marker (or end of path) to a second <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> object passed in to the parameter.</summary>
	/// <returns>The number of points between this marker and the next.</returns>
	/// <param name="path">The <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> object to which the points will be copied. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public int NextMarker(GraphicsPath path)
	{
		int resultCount;
		int num = GDIPlus.GdipPathIterNextMarkerPath(new HandleRef(this, nativeIter), out resultCount, new HandleRef(path, path?.nativePath ?? IntPtr.Zero));
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
		return resultCount;
	}

	/// <summary>Indicates whether the path associated with this <see cref="T:System.Drawing.Drawing2D.GraphicsPathIterator" /> contains a curve.</summary>
	/// <returns>This method returns true if the current subpath contains a curve; otherwise, false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool HasCurve()
	{
		bool curve;
		int num = GDIPlus.GdipPathIterHasCurve(new HandleRef(this, nativeIter), out curve);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
		return curve;
	}

	/// <summary>Rewinds this <see cref="T:System.Drawing.Drawing2D.GraphicsPathIterator" /> to the beginning of its associated path.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void Rewind()
	{
		int num = GDIPlus.GdipPathIterRewind(new HandleRef(this, nativeIter));
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
	}

	/// <summary>Copies the <see cref="P:System.Drawing.Drawing2D.GraphicsPath.PathPoints" /> property and <see cref="P:System.Drawing.Drawing2D.GraphicsPath.PathTypes" /> property arrays of the associated <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> into the two specified arrays.</summary>
	/// <returns>The number of points copied.</returns>
	/// <param name="points">Upon return, contains an array of <see cref="T:System.Drawing.PointF" /> structures that represents the points in the path. </param>
	/// <param name="types">Upon return, contains an array of bytes that represents the types of points in the path. </param>
	public unsafe int Enumerate(ref PointF[] points, ref byte[] types)
	{
		if (points.Length != types.Length)
		{
			throw SafeNativeMethods.Gdip.StatusException(2);
		}
		int resultCount = 0;
		int num = Marshal.SizeOf(typeof(GPPOINTF));
		int num2 = points.Length;
		byte[] array = new byte[num2];
		IntPtr intPtr = Marshal.AllocHGlobal(checked(num2 * num));
		try
		{
			int num3 = GDIPlus.GdipPathIterEnumerate(new HandleRef(this, nativeIter), out resultCount, intPtr, array, num2);
			if (num3 != 0)
			{
				throw SafeNativeMethods.Gdip.StatusException(num3);
			}
			if (resultCount < num2)
			{
				SafeNativeMethods.ZeroMemory((byte*)checked((long)intPtr + resultCount * num), (ulong)((num2 - resultCount) * num));
			}
			points = SafeNativeMethods.Gdip.ConvertGPPOINTFArrayF(intPtr, num2);
			array.CopyTo(types, 0);
			return resultCount;
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr);
		}
	}

	/// <summary>Copies the <see cref="P:System.Drawing.Drawing2D.GraphicsPath.PathPoints" /> property and <see cref="P:System.Drawing.Drawing2D.GraphicsPath.PathTypes" /> property arrays of the associated <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> into the two specified arrays.</summary>
	/// <returns>The number of points copied.</returns>
	/// <param name="points">Upon return, contains an array of <see cref="T:System.Drawing.PointF" /> structures that represents the points in the path. </param>
	/// <param name="types">Upon return, contains an array of bytes that represents the types of points in the path. </param>
	/// <param name="startIndex">Specifies the starting index of the arrays. </param>
	/// <param name="endIndex">Specifies the ending index of the arrays. </param>
	public unsafe int CopyData(ref PointF[] points, ref byte[] types, int startIndex, int endIndex)
	{
		if (points.Length != types.Length || endIndex - startIndex + 1 > points.Length)
		{
			throw SafeNativeMethods.Gdip.StatusException(2);
		}
		int resultCount = 0;
		int num = Marshal.SizeOf(typeof(GPPOINTF));
		int num2 = points.Length;
		byte[] array = new byte[num2];
		IntPtr intPtr = Marshal.AllocHGlobal(checked(num2 * num));
		try
		{
			int num3 = GDIPlus.GdipPathIterCopyData(new HandleRef(this, nativeIter), out resultCount, intPtr, array, startIndex, endIndex);
			if (num3 != 0)
			{
				throw SafeNativeMethods.Gdip.StatusException(num3);
			}
			if (resultCount < num2)
			{
				SafeNativeMethods.ZeroMemory((byte*)checked((long)intPtr + resultCount * num), (ulong)((num2 - resultCount) * num));
			}
			points = SafeNativeMethods.Gdip.ConvertGPPOINTFArrayF(intPtr, num2);
			array.CopyTo(types, 0);
			return resultCount;
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr);
		}
	}
}
