using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;

namespace System.Drawing.Imaging;

/// <summary>Contains information about how bitmap and metafile colors are manipulated during rendering. </summary>
[StructLayout(LayoutKind.Sequential)]
public sealed class ImageAttributes : ICloneable, IDisposable
{
	internal IntPtr nativeImageAttributes;

	internal void SetNativeImageAttributes(IntPtr handle)
	{
		if (handle == IntPtr.Zero)
		{
			throw new ArgumentNullException("handle");
		}
		nativeImageAttributes = handle;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Imaging.ImageAttributes" /> class.</summary>
	public ImageAttributes()
	{
		IntPtr imageattr = IntPtr.Zero;
		int num = GDIPlus.GdipCreateImageAttributes(out imageattr);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
		SetNativeImageAttributes(imageattr);
	}

	internal ImageAttributes(IntPtr newNativeImageAttributes)
	{
		SetNativeImageAttributes(newNativeImageAttributes);
	}

	/// <summary>Releases all resources used by this <see cref="T:System.Drawing.Imaging.ImageAttributes" /> object.</summary>
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
		if (!(nativeImageAttributes != IntPtr.Zero))
		{
			return;
		}
		try
		{
			GDIPlus.GdipDisposeImageAttributes(new HandleRef(this, nativeImageAttributes));
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
			nativeImageAttributes = IntPtr.Zero;
		}
	}

	~ImageAttributes()
	{
		Dispose(disposing: false);
	}

	/// <summary>Creates an exact copy of this <see cref="T:System.Drawing.Imaging.ImageAttributes" /> object.</summary>
	/// <returns>The <see cref="T:System.Drawing.Imaging.ImageAttributes" /> object this class creates, cast as an object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public object Clone()
	{
		IntPtr cloneImageattr = IntPtr.Zero;
		int num = GDIPlus.GdipCloneImageAttributes(new HandleRef(this, nativeImageAttributes), out cloneImageattr);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
		return new ImageAttributes(cloneImageattr);
	}

	/// <summary>Sets the color-adjustment matrix for the default category.</summary>
	/// <param name="newColorMatrix">The color-adjustment matrix. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void SetColorMatrix(ColorMatrix newColorMatrix)
	{
		SetColorMatrix(newColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Default);
	}

	/// <summary>Sets the color-adjustment matrix for the default category.</summary>
	/// <param name="newColorMatrix">The color-adjustment matrix. </param>
	/// <param name="flags">An element of <see cref="T:System.Drawing.Imaging.ColorMatrixFlag" /> that specifies the type of image and color that will be affected by the color-adjustment matrix. </param>
	public void SetColorMatrix(ColorMatrix newColorMatrix, ColorMatrixFlag flags)
	{
		SetColorMatrix(newColorMatrix, flags, ColorAdjustType.Default);
	}

	/// <summary>Sets the color-adjustment matrix for a specified category.</summary>
	/// <param name="newColorMatrix">The color-adjustment matrix. </param>
	/// <param name="mode">An element of <see cref="T:System.Drawing.Imaging.ColorMatrixFlag" /> that specifies the type of image and color that will be affected by the color-adjustment matrix. </param>
	/// <param name="type">An element of <see cref="T:System.Drawing.Imaging.ColorAdjustType" /> that specifies the category for which the color-adjustment matrix is set. </param>
	public void SetColorMatrix(ColorMatrix newColorMatrix, ColorMatrixFlag mode, ColorAdjustType type)
	{
		int num = GDIPlus.GdipSetImageAttributesColorMatrix(new HandleRef(this, nativeImageAttributes), type, enableFlag: true, newColorMatrix, null, mode);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
	}

	/// <summary>Clears the color-adjustment matrix for the default category.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void ClearColorMatrix()
	{
		ClearColorMatrix(ColorAdjustType.Default);
	}

	/// <summary>Clears the color-adjustment matrix for a specified category.</summary>
	/// <param name="type">An element of <see cref="T:System.Drawing.Imaging.ColorAdjustType" /> that specifies the category for which the color-adjustment matrix is cleared. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void ClearColorMatrix(ColorAdjustType type)
	{
		int num = GDIPlus.GdipSetImageAttributesColorMatrix(new HandleRef(this, nativeImageAttributes), type, enableFlag: false, null, null, ColorMatrixFlag.Default);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
	}

	/// <summary>Sets the color-adjustment matrix and the grayscale-adjustment matrix for the default category.</summary>
	/// <param name="newColorMatrix">The color-adjustment matrix. </param>
	/// <param name="grayMatrix">The grayscale-adjustment matrix. </param>
	public void SetColorMatrices(ColorMatrix newColorMatrix, ColorMatrix grayMatrix)
	{
		SetColorMatrices(newColorMatrix, grayMatrix, ColorMatrixFlag.Default, ColorAdjustType.Default);
	}

	/// <summary>Sets the color-adjustment matrix and the grayscale-adjustment matrix for the default category.</summary>
	/// <param name="newColorMatrix">The color-adjustment matrix. </param>
	/// <param name="grayMatrix">The grayscale-adjustment matrix. </param>
	/// <param name="flags">An element of <see cref="T:System.Drawing.Imaging.ColorMatrixFlag" /> that specifies the type of image and color that will be affected by the color-adjustment and grayscale-adjustment matrices. </param>
	public void SetColorMatrices(ColorMatrix newColorMatrix, ColorMatrix grayMatrix, ColorMatrixFlag flags)
	{
		SetColorMatrices(newColorMatrix, grayMatrix, flags, ColorAdjustType.Default);
	}

	/// <summary>Sets the color-adjustment matrix and the grayscale-adjustment matrix for a specified category.</summary>
	/// <param name="newColorMatrix">The color-adjustment matrix. </param>
	/// <param name="grayMatrix">The grayscale-adjustment matrix. </param>
	/// <param name="mode">An element of <see cref="T:System.Drawing.Imaging.ColorMatrixFlag" /> that specifies the type of image and color that will be affected by the color-adjustment and grayscale-adjustment matrices. </param>
	/// <param name="type">An element of <see cref="T:System.Drawing.Imaging.ColorAdjustType" /> that specifies the category for which the color-adjustment and grayscale-adjustment matrices are set. </param>
	public void SetColorMatrices(ColorMatrix newColorMatrix, ColorMatrix grayMatrix, ColorMatrixFlag mode, ColorAdjustType type)
	{
		int num = GDIPlus.GdipSetImageAttributesColorMatrix(new HandleRef(this, nativeImageAttributes), type, enableFlag: true, newColorMatrix, grayMatrix, mode);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
	}

	/// <summary>Sets the threshold (transparency range) for the default category.</summary>
	/// <param name="threshold">A real number that specifies the threshold value. </param>
	public void SetThreshold(float threshold)
	{
		SetThreshold(threshold, ColorAdjustType.Default);
	}

	/// <summary>Sets the threshold (transparency range) for a specified category.</summary>
	/// <param name="threshold">A threshold value from 0.0 to 1.0 that is used as a breakpoint to sort colors that will be mapped to either a maximum or a minimum value. </param>
	/// <param name="type">An element of <see cref="T:System.Drawing.Imaging.ColorAdjustType" /> that specifies the category for which the color threshold is set. </param>
	public void SetThreshold(float threshold, ColorAdjustType type)
	{
		int num = GDIPlus.GdipSetImageAttributesThreshold(new HandleRef(this, nativeImageAttributes), type, enableFlag: true, threshold);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
	}

	/// <summary>Clears the threshold value for the default category.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void ClearThreshold()
	{
		ClearThreshold(ColorAdjustType.Default);
	}

	/// <summary>Clears the threshold value for a specified category.</summary>
	/// <param name="type">An element of <see cref="T:System.Drawing.Imaging.ColorAdjustType" /> that specifies the category for which the threshold is cleared. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void ClearThreshold(ColorAdjustType type)
	{
		int num = GDIPlus.GdipSetImageAttributesThreshold(new HandleRef(this, nativeImageAttributes), type, enableFlag: false, 0f);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
	}

	/// <summary>Sets the gamma value for the default category.</summary>
	/// <param name="gamma">The gamma correction value. </param>
	public void SetGamma(float gamma)
	{
		SetGamma(gamma, ColorAdjustType.Default);
	}

	/// <summary>Sets the gamma value for a specified category.</summary>
	/// <param name="gamma">The gamma correction value. </param>
	/// <param name="type">An element of the <see cref="T:System.Drawing.Imaging.ColorAdjustType" /> enumeration that specifies the category for which the gamma value is set. </param>
	public void SetGamma(float gamma, ColorAdjustType type)
	{
		int num = GDIPlus.GdipSetImageAttributesGamma(new HandleRef(this, nativeImageAttributes), type, enableFlag: true, gamma);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
	}

	/// <summary>Disables gamma correction for the default category.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void ClearGamma()
	{
		ClearGamma(ColorAdjustType.Default);
	}

	/// <summary>Disables gamma correction for a specified category.</summary>
	/// <param name="type">An element of <see cref="T:System.Drawing.Imaging.ColorAdjustType" /> that specifies the category for which gamma correction is disabled. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void ClearGamma(ColorAdjustType type)
	{
		int num = GDIPlus.GdipSetImageAttributesGamma(new HandleRef(this, nativeImageAttributes), type, enableFlag: false, 0f);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
	}

	/// <summary>Turns off color adjustment for the default category. You can call the <see cref="Overload:System.Drawing.Imaging.ImageAttributes.ClearNoOp" /> method to reinstate the color-adjustment settings that were in place before the call to the <see cref="Overload:System.Drawing.Imaging.ImageAttributes.SetNoOp" /> method.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void SetNoOp()
	{
		SetNoOp(ColorAdjustType.Default);
	}

	/// <summary>Turns off color adjustment for a specified category. You can call the <see cref="Overload:System.Drawing.Imaging.ImageAttributes.ClearNoOp" /> method to reinstate the color-adjustment settings that were in place before the call to the <see cref="Overload:System.Drawing.Imaging.ImageAttributes.SetNoOp" /> method.</summary>
	/// <param name="type">An element of <see cref="T:System.Drawing.Imaging.ColorAdjustType" /> that specifies the category for which color correction is turned off. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void SetNoOp(ColorAdjustType type)
	{
		int num = GDIPlus.GdipSetImageAttributesNoOp(new HandleRef(this, nativeImageAttributes), type, enableFlag: true);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
	}

	/// <summary>Clears the NoOp setting for the default category.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void ClearNoOp()
	{
		ClearNoOp(ColorAdjustType.Default);
	}

	/// <summary>Clears the NoOp setting for a specified category.</summary>
	/// <param name="type">An element of <see cref="T:System.Drawing.Imaging.ColorAdjustType" /> that specifies the category for which the NoOp setting is cleared. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void ClearNoOp(ColorAdjustType type)
	{
		int num = GDIPlus.GdipSetImageAttributesNoOp(new HandleRef(this, nativeImageAttributes), type, enableFlag: false);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
	}

	/// <summary>Sets the color key for the default category.</summary>
	/// <param name="colorLow">The low color-key value. </param>
	/// <param name="colorHigh">The high color-key value. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void SetColorKey(Color colorLow, Color colorHigh)
	{
		SetColorKey(colorLow, colorHigh, ColorAdjustType.Default);
	}

	/// <summary>Sets the color key (transparency range) for a specified category.</summary>
	/// <param name="colorLow">The low color-key value. </param>
	/// <param name="colorHigh">The high color-key value. </param>
	/// <param name="type">An element of <see cref="T:System.Drawing.Imaging.ColorAdjustType" /> that specifies the category for which the color key is set. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void SetColorKey(Color colorLow, Color colorHigh, ColorAdjustType type)
	{
		int colorLow2 = colorLow.ToArgb();
		int colorHigh2 = colorHigh.ToArgb();
		int num = GDIPlus.GdipSetImageAttributesColorKeys(new HandleRef(this, nativeImageAttributes), type, enableFlag: true, colorLow2, colorHigh2);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
	}

	/// <summary>Clears the color key (transparency range) for the default category.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void ClearColorKey()
	{
		ClearColorKey(ColorAdjustType.Default);
	}

	/// <summary>Clears the color key (transparency range) for a specified category.</summary>
	/// <param name="type">An element of <see cref="T:System.Drawing.Imaging.ColorAdjustType" /> that specifies the category for which the color key is cleared. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void ClearColorKey(ColorAdjustType type)
	{
		int num = 0;
		int num2 = GDIPlus.GdipSetImageAttributesColorKeys(new HandleRef(this, nativeImageAttributes), type, enableFlag: false, num, num);
		if (num2 != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num2);
		}
	}

	/// <summary>Sets the CMYK (cyan-magenta-yellow-black) output channel for the default category.</summary>
	/// <param name="flags">An element of <see cref="T:System.Drawing.Imaging.ColorChannelFlag" /> that specifies the output channel. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void SetOutputChannel(ColorChannelFlag flags)
	{
		SetOutputChannel(flags, ColorAdjustType.Default);
	}

	/// <summary>Sets the CMYK (cyan-magenta-yellow-black) output channel for a specified category.</summary>
	/// <param name="flags">An element of <see cref="T:System.Drawing.Imaging.ColorChannelFlag" /> that specifies the output channel. </param>
	/// <param name="type">An element of <see cref="T:System.Drawing.Imaging.ColorAdjustType" /> that specifies the category for which the output channel is set. </param>
	public void SetOutputChannel(ColorChannelFlag flags, ColorAdjustType type)
	{
		int num = GDIPlus.GdipSetImageAttributesOutputChannel(new HandleRef(this, nativeImageAttributes), type, enableFlag: true, flags);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
	}

	/// <summary>Clears the CMYK (cyan-magenta-yellow-black) output channel setting for the default category.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void ClearOutputChannel()
	{
		ClearOutputChannel(ColorAdjustType.Default);
	}

	/// <summary>Clears the (cyan-magenta-yellow-black) output channel setting for a specified category.</summary>
	/// <param name="type">An element of <see cref="T:System.Drawing.Imaging.ColorAdjustType" /> that specifies the category for which the output channel setting is cleared. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void ClearOutputChannel(ColorAdjustType type)
	{
		int num = GDIPlus.GdipSetImageAttributesOutputChannel(new HandleRef(this, nativeImageAttributes), type, enableFlag: false, ColorChannelFlag.ColorChannelLast);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
	}

	/// <summary>Sets the output channel color-profile file for the default category.</summary>
	/// <param name="colorProfileFilename">The path name of a color-profile file. If the color-profile file is in the %SystemRoot%\System32\Spool\Drivers\Color directory, this parameter can be the file name. Otherwise, this parameter must be the fully qualified path name. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void SetOutputChannelColorProfile(string colorProfileFilename)
	{
		SetOutputChannelColorProfile(colorProfileFilename, ColorAdjustType.Default);
	}

	/// <summary>Sets the output channel color-profile file for a specified category.</summary>
	/// <param name="colorProfileFilename">The path name of a color-profile file. If the color-profile file is in the %SystemRoot%\System32\Spool\Drivers\Color directory, this parameter can be the file name. Otherwise, this parameter must be the fully qualified path name. </param>
	/// <param name="type">An element of <see cref="T:System.Drawing.Imaging.ColorAdjustType" /> that specifies the category for which the output channel color-profile file is set. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void SetOutputChannelColorProfile(string colorProfileFilename, ColorAdjustType type)
	{
		Path.GetFullPath(colorProfileFilename);
		int num = GDIPlus.GdipSetImageAttributesOutputChannelColorProfile(new HandleRef(this, nativeImageAttributes), type, enableFlag: true, colorProfileFilename);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
	}

	/// <summary>Clears the output channel color profile setting for the default category.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void ClearOutputChannelColorProfile()
	{
		ClearOutputChannel(ColorAdjustType.Default);
	}

	/// <summary>Clears the output channel color profile setting for a specified category.</summary>
	/// <param name="type">An element of <see cref="T:System.Drawing.Imaging.ColorAdjustType" /> that specifies the category for which the output channel profile setting is cleared. </param>
	public void ClearOutputChannelColorProfile(ColorAdjustType type)
	{
		int num = GDIPlus.GdipSetImageAttributesOutputChannel(new HandleRef(this, nativeImageAttributes), type, enableFlag: false, ColorChannelFlag.ColorChannelLast);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
	}

	/// <summary>Sets the color-remap table for the default category.</summary>
	/// <param name="map">An array of color pairs of type <see cref="T:System.Drawing.Imaging.ColorMap" />. Each color pair contains an existing color (the first value) and the color that it will be mapped to (the second value). </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void SetRemapTable(ColorMap[] map)
	{
		SetRemapTable(map, ColorAdjustType.Default);
	}

	/// <summary>Sets the color-remap table for a specified category.</summary>
	/// <param name="map">An array of color pairs of type <see cref="T:System.Drawing.Imaging.ColorMap" />. Each color pair contains an existing color (the first value) and the color that it will be mapped to (the second value). </param>
	/// <param name="type">An element of <see cref="T:System.Drawing.Imaging.ColorAdjustType" /> that specifies the category for which the color-remap table is set. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void SetRemapTable(ColorMap[] map, ColorAdjustType type)
	{
		int num = 0;
		int num2 = map.Length;
		int num3 = 4;
		IntPtr intPtr = Marshal.AllocHGlobal(checked(num2 * num3 * 2));
		try
		{
			for (num = 0; num < num2; num++)
			{
				Marshal.StructureToPtr(map[num].OldColor.ToArgb(), (IntPtr)((long)intPtr + num * num3 * 2), fDeleteOld: false);
				Marshal.StructureToPtr(map[num].NewColor.ToArgb(), (IntPtr)((long)intPtr + num * num3 * 2 + num3), fDeleteOld: false);
			}
			int num4 = GDIPlus.GdipSetImageAttributesRemapTable(new HandleRef(this, nativeImageAttributes), type, enableFlag: true, num2, new HandleRef(null, intPtr));
			if (num4 != 0)
			{
				throw SafeNativeMethods.Gdip.StatusException(num4);
			}
		}
		finally
		{
			Marshal.FreeHGlobal(intPtr);
		}
	}

	/// <summary>Clears the color-remap table for the default category.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void ClearRemapTable()
	{
		ClearRemapTable(ColorAdjustType.Default);
	}

	/// <summary>Clears the color-remap table for a specified category.</summary>
	/// <param name="type">An element of <see cref="T:System.Drawing.Imaging.ColorAdjustType" /> that specifies the category for which the remap table is cleared. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void ClearRemapTable(ColorAdjustType type)
	{
		int num = GDIPlus.GdipSetImageAttributesRemapTable(new HandleRef(this, nativeImageAttributes), type, enableFlag: false, 0, NativeMethods.NullHandleRef);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
	}

	/// <summary>Sets the color-remap table for the brush category.</summary>
	/// <param name="map">An array of <see cref="T:System.Drawing.Imaging.ColorMap" /> objects. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void SetBrushRemapTable(ColorMap[] map)
	{
		SetRemapTable(map, ColorAdjustType.Brush);
	}

	/// <summary>Clears the brush color-remap table of this <see cref="T:System.Drawing.Imaging.ImageAttributes" /> object.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void ClearBrushRemapTable()
	{
		ClearRemapTable(ColorAdjustType.Brush);
	}

	/// <summary>Sets the wrap mode that is used to decide how to tile a texture across a shape, or at shape boundaries. A texture is tiled across a shape to fill it in when the texture is smaller than the shape it is filling.</summary>
	/// <param name="mode">An element of <see cref="T:System.Drawing.Drawing2D.WrapMode" /> that specifies how repeated copies of an image are used to tile an area. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void SetWrapMode(WrapMode mode)
	{
		SetWrapMode(mode, default(Color), clamp: false);
	}

	/// <summary>Sets the wrap mode and color used to decide how to tile a texture across a shape, or at shape boundaries. A texture is tiled across a shape to fill it in when the texture is smaller than the shape it is filling.</summary>
	/// <param name="mode">An element of <see cref="T:System.Drawing.Drawing2D.WrapMode" /> that specifies how repeated copies of an image are used to tile an area. </param>
	/// <param name="color">An <see cref="T:System.Drawing.Imaging.ImageAttributes" /> object that specifies the color of pixels outside of a rendered image. This color is visible if the mode parameter is set to <see cref="F:System.Drawing.Drawing2D.WrapMode.Clamp" /> and the source rectangle passed to <see cref="Overload:System.Drawing.Graphics.DrawImage" /> is larger than the image itself. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void SetWrapMode(WrapMode mode, Color color)
	{
		SetWrapMode(mode, color, clamp: false);
	}

	/// <summary>Sets the wrap mode and color used to decide how to tile a texture across a shape, or at shape boundaries. A texture is tiled across a shape to fill it in when the texture is smaller than the shape it is filling.</summary>
	/// <param name="mode">An element of <see cref="T:System.Drawing.Drawing2D.WrapMode" /> that specifies how repeated copies of an image are used to tile an area. </param>
	/// <param name="color">A color object that specifies the color of pixels outside of a rendered image. This color is visible if the mode parameter is set to <see cref="F:System.Drawing.Drawing2D.WrapMode.Clamp" /> and the source rectangle passed to <see cref="Overload:System.Drawing.Graphics.DrawImage" /> is larger than the image itself. </param>
	/// <param name="clamp">This parameter has no effect. Set it to false. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void SetWrapMode(WrapMode mode, Color color, bool clamp)
	{
		int num = GDIPlus.GdipSetImageAttributesWrapMode(new HandleRef(this, nativeImageAttributes), (int)mode, color.ToArgb(), clamp);
		if (num != 0)
		{
			throw SafeNativeMethods.Gdip.StatusException(num);
		}
	}

	/// <summary>Adjusts the colors in a palette according to the adjustment settings of a specified category.</summary>
	/// <param name="palette">A <see cref="T:System.Drawing.Imaging.ColorPalette" /> that on input contains the palette to be adjusted, and on output contains the adjusted palette. </param>
	/// <param name="type">An element of <see cref="T:System.Drawing.Imaging.ColorAdjustType" /> that specifies the category whose adjustment settings will be applied to the palette. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public void GetAdjustedPalette(ColorPalette palette, ColorAdjustType type)
	{
		IntPtr intPtr = palette.ConvertToMemory();
		try
		{
			int num = GDIPlus.GdipGetImageAttributesAdjustedPalette(new HandleRef(this, nativeImageAttributes), new HandleRef(null, intPtr), type);
			if (num != 0)
			{
				throw SafeNativeMethods.Gdip.StatusException(num);
			}
			palette.ConvertFromMemory(intPtr);
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}
	}
}
