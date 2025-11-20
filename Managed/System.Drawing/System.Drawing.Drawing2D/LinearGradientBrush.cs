using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Drawing.Drawing2D;

/// <summary>Encapsulates a <see cref="T:System.Drawing.Brush" /> with a linear gradient. This class cannot be inherited.</summary>
public sealed class LinearGradientBrush : Brush
{
	private RectangleF rectangle;

	private bool _interpolationColorsWasSet;

	/// <summary>Gets or sets a <see cref="T:System.Drawing.Drawing2D.Blend" /> that specifies positions and factors that define a custom falloff for the gradient.</summary>
	/// <returns>A <see cref="T:System.Drawing.Drawing2D.Blend" /> that represents a custom falloff for the gradient.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public Blend Blend
	{
		get
		{
			if (_interpolationColorsWasSet)
			{
				return null;
			}
			GDIPlus.CheckStatus(GDIPlus.GdipGetLineBlendCount(base.NativeBrush, out var count));
			float[] array = new float[count];
			float[] positions = new float[count];
			GDIPlus.CheckStatus(GDIPlus.GdipGetLineBlend(base.NativeBrush, array, positions, count));
			return new Blend
			{
				Factors = array,
				Positions = positions
			};
		}
		set
		{
			float[] factors = value.Factors;
			float[] positions = value.Positions;
			int num = factors.Length;
			if (num == 0 || positions.Length == 0)
			{
				throw new ArgumentException("Invalid Blend object. It should have at least 2 elements in each of the factors and positions arrays.");
			}
			if (num != positions.Length)
			{
				throw new ArgumentException("Invalid Blend object. It should contain the same number of factors and positions values.");
			}
			if (positions[0] != 0f)
			{
				throw new ArgumentException("Invalid Blend object. The positions array must have 0.0 as its first element.");
			}
			if (positions[num - 1] != 1f)
			{
				throw new ArgumentException("Invalid Blend object. The positions array must have 1.0 as its last element.");
			}
			GDIPlus.CheckStatus(GDIPlus.GdipSetLineBlend(base.NativeBrush, factors, positions, num));
		}
	}

	/// <summary>Gets or sets a value indicating whether gamma correction is enabled for this <see cref="T:System.Drawing.Drawing2D.LinearGradientBrush" />.</summary>
	/// <returns>The value is true if gamma correction is enabled for this <see cref="T:System.Drawing.Drawing2D.LinearGradientBrush" />; otherwise, false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[System.MonoTODO("The GammaCorrection value is ignored when using libgdiplus.")]
	public bool GammaCorrection
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetLineGammaCorrection(base.NativeBrush, out var useGammaCorrection));
			return useGammaCorrection;
		}
		set
		{
			GDIPlus.CheckStatus(GDIPlus.GdipSetLineGammaCorrection(base.NativeBrush, value));
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Drawing.Drawing2D.ColorBlend" /> that defines a multicolor linear gradient.</summary>
	/// <returns>A <see cref="T:System.Drawing.Drawing2D.ColorBlend" /> that defines a multicolor linear gradient.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public ColorBlend InterpolationColors
	{
		get
		{
			if (!_interpolationColorsWasSet)
			{
				throw new ArgumentException("Property must be set to a valid ColorBlend object to use interpolation colors.");
			}
			GDIPlus.CheckStatus(GDIPlus.GdipGetLinePresetBlendCount(base.NativeBrush, out var count));
			int[] array = new int[count];
			float[] positions = new float[count];
			GDIPlus.CheckStatus(GDIPlus.GdipGetLinePresetBlend(base.NativeBrush, array, positions, count));
			ColorBlend colorBlend = new ColorBlend();
			Color[] array2 = new Color[count];
			for (int i = 0; i < count; i++)
			{
				array2[i] = Color.FromArgb(array[i]);
			}
			colorBlend.Colors = array2;
			colorBlend.Positions = positions;
			return colorBlend;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentException("InterpolationColors is null");
			}
			Color[] colors = value.Colors;
			float[] positions = value.Positions;
			int num = colors.Length;
			if (num == 0 || positions.Length == 0)
			{
				throw new ArgumentException("Invalid ColorBlend object. It should have at least 2 elements in each of the colors and positions arrays.");
			}
			if (num != positions.Length)
			{
				throw new ArgumentException("Invalid ColorBlend object. It should contain the same number of positions and color values.");
			}
			if (positions[0] != 0f)
			{
				throw new ArgumentException("Invalid ColorBlend object. The positions array must have 0.0 as its first element.");
			}
			if (positions[num - 1] != 1f)
			{
				throw new ArgumentException("Invalid ColorBlend object. The positions array must have 1.0 as its last element.");
			}
			int[] array = new int[colors.Length];
			for (int i = 0; i < colors.Length; i++)
			{
				array[i] = colors[i].ToArgb();
			}
			GDIPlus.CheckStatus(GDIPlus.GdipSetLinePresetBlend(base.NativeBrush, array, positions, num));
			_interpolationColorsWasSet = true;
		}
	}

	/// <summary>Gets or sets the starting and ending colors of the gradient.</summary>
	/// <returns>An array of two <see cref="T:System.Drawing.Color" /> structures that represents the starting and ending colors of the gradient.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public Color[] LinearColors
	{
		get
		{
			int[] array = new int[2];
			GDIPlus.CheckStatus(GDIPlus.GdipGetLineColors(base.NativeBrush, array));
			return new Color[2]
			{
				Color.FromArgb(array[0]),
				Color.FromArgb(array[1])
			};
		}
		set
		{
			GDIPlus.CheckStatus(GDIPlus.GdipSetLineColors(base.NativeBrush, value[0].ToArgb(), value[1].ToArgb()));
		}
	}

	/// <summary>Gets a rectangular region that defines the starting and ending points of the gradient.</summary>
	/// <returns>A <see cref="T:System.Drawing.RectangleF" /> structure that specifies the starting and ending points of the gradient.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public RectangleF Rectangle => rectangle;

	/// <summary>Gets or sets a copy <see cref="T:System.Drawing.Drawing2D.Matrix" /> that defines a local geometric transform for this <see cref="T:System.Drawing.Drawing2D.LinearGradientBrush" />.</summary>
	/// <returns>A copy of the <see cref="T:System.Drawing.Drawing2D.Matrix" /> that defines a geometric transform that applies only to fills drawn with this <see cref="T:System.Drawing.Drawing2D.LinearGradientBrush" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public Matrix Transform
	{
		get
		{
			Matrix matrix = new Matrix();
			GDIPlus.CheckStatus(GDIPlus.GdipGetLineTransform(base.NativeBrush, matrix.nativeMatrix));
			return matrix;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Transform");
			}
			GDIPlus.CheckStatus(GDIPlus.GdipSetLineTransform(base.NativeBrush, value.nativeMatrix));
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Drawing.Drawing2D.WrapMode" /> enumeration that indicates the wrap mode for this <see cref="T:System.Drawing.Drawing2D.LinearGradientBrush" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.Drawing2D.WrapMode" /> that specifies how fills drawn with this <see cref="T:System.Drawing.Drawing2D.LinearGradientBrush" /> are tiled.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public WrapMode WrapMode
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetLineWrapMode(base.NativeBrush, out var wrapMode));
			return wrapMode;
		}
		set
		{
			if (value < WrapMode.Tile || value > WrapMode.Clamp)
			{
				throw new InvalidEnumArgumentException("WrapMode");
			}
			GDIPlus.CheckStatus(GDIPlus.GdipSetLineWrapMode(base.NativeBrush, value));
		}
	}

	internal LinearGradientBrush(IntPtr native)
	{
		Status status = GDIPlus.GdipGetLineRect(native, out rectangle);
		SetNativeBrush(native);
		GDIPlus.CheckStatus(status);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Drawing2D.LinearGradientBrush" /> class with the specified points and colors.</summary>
	/// <param name="point1">A <see cref="T:System.Drawing.Point" /> structure that represents the starting point of the linear gradient. </param>
	/// <param name="point2">A <see cref="T:System.Drawing.Point" /> structure that represents the endpoint of the linear gradient. </param>
	/// <param name="color1">A <see cref="T:System.Drawing.Color" /> structure that represents the starting color of the linear gradient. </param>
	/// <param name="color2">A <see cref="T:System.Drawing.Color" /> structure that represents the ending color of the linear gradient. </param>
	public LinearGradientBrush(Point point1, Point point2, Color color1, Color color2)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCreateLineBrushI(ref point1, ref point2, color1.ToArgb(), color2.ToArgb(), WrapMode.Tile, out var brush));
		SetNativeBrush(brush);
		GDIPlus.CheckStatus(GDIPlus.GdipGetLineRect(brush, out rectangle));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Drawing2D.LinearGradientBrush" /> class with the specified points and colors.</summary>
	/// <param name="point1">A <see cref="T:System.Drawing.PointF" /> structure that represents the starting point of the linear gradient. </param>
	/// <param name="point2">A <see cref="T:System.Drawing.PointF" /> structure that represents the endpoint of the linear gradient. </param>
	/// <param name="color1">A <see cref="T:System.Drawing.Color" /> structure that represents the starting color of the linear gradient. </param>
	/// <param name="color2">A <see cref="T:System.Drawing.Color" /> structure that represents the ending color of the linear gradient. </param>
	public LinearGradientBrush(PointF point1, PointF point2, Color color1, Color color2)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCreateLineBrush(ref point1, ref point2, color1.ToArgb(), color2.ToArgb(), WrapMode.Tile, out var brush));
		SetNativeBrush(brush);
		GDIPlus.CheckStatus(GDIPlus.GdipGetLineRect(brush, out rectangle));
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Drawing.Drawing2D.LinearGradientBrush" /> class based on a rectangle, starting and ending colors, and orientation.</summary>
	/// <param name="rect">A <see cref="T:System.Drawing.Rectangle" /> structure that specifies the bounds of the linear gradient. </param>
	/// <param name="color1">A <see cref="T:System.Drawing.Color" /> structure that represents the starting color for the gradient. </param>
	/// <param name="color2">A <see cref="T:System.Drawing.Color" /> structure that represents the ending color for the gradient. </param>
	/// <param name="linearGradientMode">A <see cref="T:System.Drawing.Drawing2D.LinearGradientMode" /> enumeration element that specifies the orientation of the gradient. The orientation determines the starting and ending points of the gradient. For example, LinearGradientMode.ForwardDiagonal specifies that the starting point is the upper-left corner of the rectangle and the ending point is the lower-right corner of the rectangle. </param>
	public LinearGradientBrush(Rectangle rect, Color color1, Color color2, LinearGradientMode linearGradientMode)
	{
		if (linearGradientMode < LinearGradientMode.Horizontal || linearGradientMode > LinearGradientMode.BackwardDiagonal)
		{
			throw new InvalidEnumArgumentException("linearGradientMode", (int)linearGradientMode, typeof(LinearGradientMode));
		}
		if (rect.Width == 0 || rect.Height == 0)
		{
			throw new ArgumentException($"Rectangle '{rect.ToString()}' cannot have a width or height equal to 0.");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipCreateLineBrushFromRectI(ref rect, color1.ToArgb(), color2.ToArgb(), linearGradientMode, WrapMode.Tile, out var brush));
		SetNativeBrush(brush);
		rectangle = rect;
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Drawing.Drawing2D.LinearGradientBrush" /> class based on a rectangle, starting and ending colors, and an orientation angle.</summary>
	/// <param name="rect">A <see cref="T:System.Drawing.Rectangle" /> structure that specifies the bounds of the linear gradient. </param>
	/// <param name="color1">A <see cref="T:System.Drawing.Color" /> structure that represents the starting color for the gradient. </param>
	/// <param name="color2">A <see cref="T:System.Drawing.Color" /> structure that represents the ending color for the gradient. </param>
	/// <param name="angle">The angle, measured in degrees clockwise from the x-axis, of the gradient's orientation line. </param>
	public LinearGradientBrush(Rectangle rect, Color color1, Color color2, float angle)
		: this(rect, color1, color2, angle, isAngleScaleable: false)
	{
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Drawing.Drawing2D.LinearGradientBrush" /> based on a rectangle, starting and ending colors, and an orientation mode.</summary>
	/// <param name="rect">A <see cref="T:System.Drawing.RectangleF" /> structure that specifies the bounds of the linear gradient. </param>
	/// <param name="color1">A <see cref="T:System.Drawing.Color" /> structure that represents the starting color for the gradient. </param>
	/// <param name="color2">A <see cref="T:System.Drawing.Color" /> structure that represents the ending color for the gradient. </param>
	/// <param name="linearGradientMode">A <see cref="T:System.Drawing.Drawing2D.LinearGradientMode" /> enumeration element that specifies the orientation of the gradient. The orientation determines the starting and ending points of the gradient. For example, LinearGradientMode.ForwardDiagonal specifies that the starting point is the upper-left corner of the rectangle and the ending point is the lower-right corner of the rectangle. </param>
	public LinearGradientBrush(RectangleF rect, Color color1, Color color2, LinearGradientMode linearGradientMode)
	{
		if (linearGradientMode < LinearGradientMode.Horizontal || linearGradientMode > LinearGradientMode.BackwardDiagonal)
		{
			throw new InvalidEnumArgumentException("linearGradientMode", (int)linearGradientMode, typeof(LinearGradientMode));
		}
		if ((double)rect.Width == 0.0 || (double)rect.Height == 0.0)
		{
			throw new ArgumentException($"Rectangle '{rect.ToString()}' cannot have a width or height equal to 0.");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipCreateLineBrushFromRect(ref rect, color1.ToArgb(), color2.ToArgb(), linearGradientMode, WrapMode.Tile, out var brush));
		SetNativeBrush(brush);
		rectangle = rect;
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Drawing.Drawing2D.LinearGradientBrush" /> class based on a rectangle, starting and ending colors, and an orientation angle.</summary>
	/// <param name="rect">A <see cref="T:System.Drawing.RectangleF" /> structure that specifies the bounds of the linear gradient. </param>
	/// <param name="color1">A <see cref="T:System.Drawing.Color" /> structure that represents the starting color for the gradient. </param>
	/// <param name="color2">A <see cref="T:System.Drawing.Color" /> structure that represents the ending color for the gradient. </param>
	/// <param name="angle">The angle, measured in degrees clockwise from the x-axis, of the gradient's orientation line. </param>
	public LinearGradientBrush(RectangleF rect, Color color1, Color color2, float angle)
		: this(rect, color1, color2, angle, isAngleScaleable: false)
	{
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Drawing.Drawing2D.LinearGradientBrush" /> class based on a rectangle, starting and ending colors, and an orientation angle.</summary>
	/// <param name="rect">A <see cref="T:System.Drawing.Rectangle" /> structure that specifies the bounds of the linear gradient. </param>
	/// <param name="color1">A <see cref="T:System.Drawing.Color" /> structure that represents the starting color for the gradient. </param>
	/// <param name="color2">A <see cref="T:System.Drawing.Color" /> structure that represents the ending color for the gradient. </param>
	/// <param name="angle">The angle, measured in degrees clockwise from the x-axis, of the gradient's orientation line. </param>
	/// <param name="isAngleScaleable">Set to true to specify that the angle is affected by the transform associated with this <see cref="T:System.Drawing.Drawing2D.LinearGradientBrush" />; otherwise, false. </param>
	public LinearGradientBrush(Rectangle rect, Color color1, Color color2, float angle, bool isAngleScaleable)
	{
		if (rect.Width == 0 || rect.Height == 0)
		{
			throw new ArgumentException($"Rectangle '{rect.ToString()}' cannot have a width or height equal to 0.");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipCreateLineBrushFromRectWithAngleI(ref rect, color1.ToArgb(), color2.ToArgb(), angle, isAngleScaleable, WrapMode.Tile, out var brush));
		SetNativeBrush(brush);
		rectangle = rect;
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Drawing.Drawing2D.LinearGradientBrush" /> class based on a rectangle, starting and ending colors, and an orientation angle.</summary>
	/// <param name="rect">A <see cref="T:System.Drawing.RectangleF" /> structure that specifies the bounds of the linear gradient. </param>
	/// <param name="color1">A <see cref="T:System.Drawing.Color" /> structure that represents the starting color for the gradient. </param>
	/// <param name="color2">A <see cref="T:System.Drawing.Color" /> structure that represents the ending color for the gradient. </param>
	/// <param name="angle">The angle, measured in degrees clockwise from the x-axis, of the gradient's orientation line. </param>
	/// <param name="isAngleScaleable">Set to true to specify that the angle is affected by the transform associated with this <see cref="T:System.Drawing.Drawing2D.LinearGradientBrush" />; otherwise, false. </param>
	public LinearGradientBrush(RectangleF rect, Color color1, Color color2, float angle, bool isAngleScaleable)
	{
		if (rect.Width == 0f || rect.Height == 0f)
		{
			throw new ArgumentException($"Rectangle '{rect.ToString()}' cannot have a width or height equal to 0.");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipCreateLineBrushFromRectWithAngle(ref rect, color1.ToArgb(), color2.ToArgb(), angle, isAngleScaleable, WrapMode.Tile, out var brush));
		SetNativeBrush(brush);
		rectangle = rect;
	}

	/// <summary>Multiplies the <see cref="T:System.Drawing.Drawing2D.Matrix" /> that represents the local geometric transform of this <see cref="T:System.Drawing.Drawing2D.LinearGradientBrush" /> by the specified <see cref="T:System.Drawing.Drawing2D.Matrix" /> by prepending the specified <see cref="T:System.Drawing.Drawing2D.Matrix" />.</summary>
	/// <param name="matrix">The <see cref="T:System.Drawing.Drawing2D.Matrix" /> by which to multiply the geometric transform. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void MultiplyTransform(Matrix matrix)
	{
		MultiplyTransform(matrix, MatrixOrder.Prepend);
	}

	/// <summary>Multiplies the <see cref="T:System.Drawing.Drawing2D.Matrix" /> that represents the local geometric transform of this <see cref="T:System.Drawing.Drawing2D.LinearGradientBrush" /> by the specified <see cref="T:System.Drawing.Drawing2D.Matrix" /> in the specified order.</summary>
	/// <param name="matrix">The <see cref="T:System.Drawing.Drawing2D.Matrix" /> by which to multiply the geometric transform. </param>
	/// <param name="order">A <see cref="T:System.Drawing.Drawing2D.MatrixOrder" /> that specifies in which order to multiply the two matrices. </param>
	public void MultiplyTransform(Matrix matrix, MatrixOrder order)
	{
		if (matrix == null)
		{
			throw new ArgumentNullException("matrix");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipMultiplyLineTransform(base.NativeBrush, matrix.nativeMatrix, order));
	}

	/// <summary>Resets the <see cref="P:System.Drawing.Drawing2D.LinearGradientBrush.Transform" /> property to identity.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void ResetTransform()
	{
		GDIPlus.CheckStatus(GDIPlus.GdipResetLineTransform(base.NativeBrush));
	}

	/// <summary>Rotates the local geometric transform by the specified amount. This method prepends the rotation to the transform.</summary>
	/// <param name="angle">The angle of rotation. </param>
	public void RotateTransform(float angle)
	{
		RotateTransform(angle, MatrixOrder.Prepend);
	}

	/// <summary>Rotates the local geometric transform by the specified amount in the specified order.</summary>
	/// <param name="angle">The angle of rotation. </param>
	/// <param name="order">A <see cref="T:System.Drawing.Drawing2D.MatrixOrder" /> that specifies whether to append or prepend the rotation matrix. </param>
	public void RotateTransform(float angle, MatrixOrder order)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipRotateLineTransform(base.NativeBrush, angle, order));
	}

	/// <summary>Scales the local geometric transform by the specified amounts. This method prepends the scaling matrix to the transform.</summary>
	/// <param name="sx">The amount by which to scale the transform in the x-axis direction. </param>
	/// <param name="sy">The amount by which to scale the transform in the y-axis direction. </param>
	public void ScaleTransform(float sx, float sy)
	{
		ScaleTransform(sx, sy, MatrixOrder.Prepend);
	}

	/// <summary>Scales the local geometric transform by the specified amounts in the specified order.</summary>
	/// <param name="sx">The amount by which to scale the transform in the x-axis direction. </param>
	/// <param name="sy">The amount by which to scale the transform in the y-axis direction. </param>
	/// <param name="order">A <see cref="T:System.Drawing.Drawing2D.MatrixOrder" /> that specifies whether to append or prepend the scaling matrix. </param>
	public void ScaleTransform(float sx, float sy, MatrixOrder order)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipScaleLineTransform(base.NativeBrush, sx, sy, order));
	}

	/// <summary>Creates a linear gradient with a center color and a linear falloff to a single color on both ends.</summary>
	/// <param name="focus">A value from 0 through 1 that specifies the center of the gradient (the point where the gradient is composed of only the ending color). </param>
	public void SetBlendTriangularShape(float focus)
	{
		SetBlendTriangularShape(focus, 1f);
	}

	/// <summary>Creates a linear gradient with a center color and a linear falloff to a single color on both ends.</summary>
	/// <param name="focus">A value from 0 through 1 that specifies the center of the gradient (the point where the gradient is composed of only the ending color). </param>
	/// <param name="scale">A value from 0 through1 that specifies how fast the colors falloff from the starting color to <paramref name="focus" /> (ending color) </param>
	public void SetBlendTriangularShape(float focus, float scale)
	{
		if (focus < 0f || focus > 1f || scale < 0f || scale > 1f)
		{
			throw new ArgumentException("Invalid parameter passed.");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipSetLineLinearBlend(base.NativeBrush, focus, scale));
		_interpolationColorsWasSet = false;
	}

	/// <summary>Creates a gradient falloff based on a bell-shaped curve.</summary>
	/// <param name="focus">A value from 0 through 1 that specifies the center of the gradient (the point where the starting color and ending color are blended equally). </param>
	public void SetSigmaBellShape(float focus)
	{
		SetSigmaBellShape(focus, 1f);
	}

	/// <summary>Creates a gradient falloff based on a bell-shaped curve.</summary>
	/// <param name="focus">A value from 0 through 1 that specifies the center of the gradient (the point where the gradient is composed of only the ending color). </param>
	/// <param name="scale">A value from 0 through 1 that specifies how fast the colors falloff from the <paramref name="focus" />. </param>
	public void SetSigmaBellShape(float focus, float scale)
	{
		if (focus < 0f || focus > 1f || scale < 0f || scale > 1f)
		{
			throw new ArgumentException("Invalid parameter passed.");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipSetLineSigmaBlend(base.NativeBrush, focus, scale));
		_interpolationColorsWasSet = false;
	}

	/// <summary>Translates the local geometric transform by the specified dimensions. This method prepends the translation to the transform.</summary>
	/// <param name="dx">The value of the translation in x. </param>
	/// <param name="dy">The value of the translation in y. </param>
	public void TranslateTransform(float dx, float dy)
	{
		TranslateTransform(dx, dy, MatrixOrder.Prepend);
	}

	/// <summary>Translates the local geometric transform by the specified dimensions in the specified order.</summary>
	/// <param name="dx">The value of the translation in x. </param>
	/// <param name="dy">The value of the translation in y. </param>
	/// <param name="order">The order (prepend or append) in which to apply the translation. </param>
	public void TranslateTransform(float dx, float dy, MatrixOrder order)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipTranslateLineTransform(base.NativeBrush, dx, dy, order));
	}

	/// <summary>Creates an exact copy of this <see cref="T:System.Drawing.Drawing2D.LinearGradientBrush" />.</summary>
	/// <returns>The <see cref="T:System.Drawing.Drawing2D.LinearGradientBrush" /> this method creates, cast as an object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override object Clone()
	{
		GDIPlus.CheckStatus((Status)GDIPlus.GdipCloneBrush(new HandleRef(this, base.NativeBrush), out var clonedBrush));
		return new LinearGradientBrush(clonedBrush);
	}
}
