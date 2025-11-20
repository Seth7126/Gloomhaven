using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Drawing.Drawing2D;

/// <summary>Encapsulates a <see cref="T:System.Drawing.Brush" /> object that fills the interior of a <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> object with a gradient. This class cannot be inherited.</summary>
[System.MonoTODO("libgdiplus/cairo doesn't support path gradients - unless it can be mapped to a radial gradient")]
public sealed class PathGradientBrush : Brush
{
	/// <summary>Gets or sets a <see cref="T:System.Drawing.Drawing2D.Blend" /> that specifies positions and factors that define a custom falloff for the gradient.</summary>
	/// <returns>A <see cref="T:System.Drawing.Drawing2D.Blend" /> that represents a custom falloff for the gradient.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public Blend Blend
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetPathGradientBlendCount(base.NativeBrush, out var count));
			float[] array = new float[count];
			float[] positions = new float[count];
			GDIPlus.CheckStatus(GDIPlus.GdipGetPathGradientBlend(base.NativeBrush, array, positions, count));
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
			GDIPlus.CheckStatus(GDIPlus.GdipSetPathGradientBlend(base.NativeBrush, factors, positions, num));
		}
	}

	/// <summary>Gets or sets the color at the center of the path gradient.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> that represents the color at the center of the path gradient.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public Color CenterColor
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetPathGradientCenterColor(base.NativeBrush, out var color));
			return Color.FromArgb(color);
		}
		set
		{
			GDIPlus.CheckStatus(GDIPlus.GdipSetPathGradientCenterColor(base.NativeBrush, value.ToArgb()));
		}
	}

	/// <summary>Gets or sets the center point of the path gradient.</summary>
	/// <returns>A <see cref="T:System.Drawing.PointF" /> that represents the center point of the path gradient.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public PointF CenterPoint
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetPathGradientCenterPoint(base.NativeBrush, out var point));
			return point;
		}
		set
		{
			PointF point = value;
			GDIPlus.CheckStatus(GDIPlus.GdipSetPathGradientCenterPoint(base.NativeBrush, ref point));
		}
	}

	/// <summary>Gets or sets the focus point for the gradient falloff.</summary>
	/// <returns>A <see cref="T:System.Drawing.PointF" /> that represents the focus point for the gradient falloff.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public PointF FocusScales
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetPathGradientFocusScales(base.NativeBrush, out var xScale, out var yScale));
			return new PointF(xScale, yScale);
		}
		set
		{
			GDIPlus.CheckStatus(GDIPlus.GdipSetPathGradientFocusScales(base.NativeBrush, value.X, value.Y));
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
			GDIPlus.CheckStatus(GDIPlus.GdipGetPathGradientPresetBlendCount(base.NativeBrush, out var count));
			if (count < 1)
			{
				count = 1;
			}
			int[] array = new int[count];
			float[] positions = new float[count];
			if (count > 1)
			{
				GDIPlus.CheckStatus(GDIPlus.GdipGetPathGradientPresetBlend(base.NativeBrush, array, positions, count));
			}
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
			GDIPlus.CheckStatus(GDIPlus.GdipSetPathGradientPresetBlend(base.NativeBrush, array, positions, num));
		}
	}

	/// <summary>Gets a bounding rectangle for this <see cref="T:System.Drawing.Drawing2D.PathGradientBrush" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.RectangleF" /> that represents a rectangular region that bounds the path this <see cref="T:System.Drawing.Drawing2D.PathGradientBrush" /> fills.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public RectangleF Rectangle
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetPathGradientRect(base.NativeBrush, out var rect));
			return rect;
		}
	}

	/// <summary>Gets or sets an array of colors that correspond to the points in the path this <see cref="T:System.Drawing.Drawing2D.PathGradientBrush" /> fills.</summary>
	/// <returns>An array of <see cref="T:System.Drawing.Color" /> structures that represents the colors associated with each point in the path this <see cref="T:System.Drawing.Drawing2D.PathGradientBrush" /> fills.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public Color[] SurroundColors
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetPathGradientSurroundColorCount(base.NativeBrush, out var count));
			int[] array = new int[count];
			GDIPlus.CheckStatus(GDIPlus.GdipGetPathGradientSurroundColorsWithCount(base.NativeBrush, array, ref count));
			Color[] array2 = new Color[count];
			for (int i = 0; i < count; i++)
			{
				array2[i] = Color.FromArgb(array[i]);
			}
			return array2;
		}
		set
		{
			int count = value.Length;
			int[] array = new int[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = value[i].ToArgb();
			}
			GDIPlus.CheckStatus(GDIPlus.GdipSetPathGradientSurroundColorsWithCount(base.NativeBrush, array, ref count));
		}
	}

	/// <summary>Gets or sets a copy of the <see cref="T:System.Drawing.Drawing2D.Matrix" /> that defines a local geometric transform for this <see cref="T:System.Drawing.Drawing2D.PathGradientBrush" />.</summary>
	/// <returns>A copy of the <see cref="T:System.Drawing.Drawing2D.Matrix" /> that defines a geometric transform that applies only to fills drawn with this <see cref="T:System.Drawing.Drawing2D.PathGradientBrush" />.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public Matrix Transform
	{
		get
		{
			Matrix matrix = new Matrix();
			GDIPlus.CheckStatus(GDIPlus.GdipGetPathGradientTransform(base.NativeBrush, matrix.nativeMatrix));
			return matrix;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Transform");
			}
			GDIPlus.CheckStatus(GDIPlus.GdipSetPathGradientTransform(base.NativeBrush, value.nativeMatrix));
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Drawing.Drawing2D.WrapMode" /> that indicates the wrap mode for this <see cref="T:System.Drawing.Drawing2D.PathGradientBrush" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.Drawing2D.WrapMode" /> that specifies how fills drawn with this <see cref="T:System.Drawing.Drawing2D.PathGradientBrush" /> are tiled.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public WrapMode WrapMode
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetPathGradientWrapMode(base.NativeBrush, out var wrapMode));
			return wrapMode;
		}
		set
		{
			if (value < WrapMode.Tile || value > WrapMode.Clamp)
			{
				throw new InvalidEnumArgumentException("WrapMode");
			}
			GDIPlus.CheckStatus(GDIPlus.GdipSetPathGradientWrapMode(base.NativeBrush, value));
		}
	}

	internal PathGradientBrush(IntPtr native)
	{
		SetNativeBrush(native);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Drawing2D.PathGradientBrush" /> class with the specified path.</summary>
	/// <param name="path">The <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> that defines the area filled by this <see cref="T:System.Drawing.Drawing2D.PathGradientBrush" />. </param>
	public PathGradientBrush(GraphicsPath path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipCreatePathGradientFromPath(path.nativePath, out var brush));
		SetNativeBrush(brush);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Drawing2D.PathGradientBrush" /> class with the specified points.</summary>
	/// <param name="points">An array of <see cref="T:System.Drawing.Point" /> structures that represents the points that make up the vertices of the path. </param>
	public PathGradientBrush(Point[] points)
		: this(points, WrapMode.Clamp)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Drawing2D.PathGradientBrush" /> class with the specified points.</summary>
	/// <param name="points">An array of <see cref="T:System.Drawing.PointF" /> structures that represents the points that make up the vertices of the path. </param>
	public PathGradientBrush(PointF[] points)
		: this(points, WrapMode.Clamp)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Drawing2D.PathGradientBrush" /> class with the specified points and wrap mode.</summary>
	/// <param name="points">An array of <see cref="T:System.Drawing.Point" /> structures that represents the points that make up the vertices of the path. </param>
	/// <param name="wrapMode">A <see cref="T:System.Drawing.Drawing2D.WrapMode" /> that specifies how fills drawn with this <see cref="T:System.Drawing.Drawing2D.PathGradientBrush" /> are tiled. </param>
	public PathGradientBrush(Point[] points, WrapMode wrapMode)
	{
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		if (wrapMode < WrapMode.Tile || wrapMode > WrapMode.Clamp)
		{
			throw new InvalidEnumArgumentException("WrapMode");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipCreatePathGradientI(points, points.Length, wrapMode, out var brush));
		SetNativeBrush(brush);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Drawing2D.PathGradientBrush" /> class with the specified points and wrap mode.</summary>
	/// <param name="points">An array of <see cref="T:System.Drawing.PointF" /> structures that represents the points that make up the vertices of the path. </param>
	/// <param name="wrapMode">A <see cref="T:System.Drawing.Drawing2D.WrapMode" /> that specifies how fills drawn with this <see cref="T:System.Drawing.Drawing2D.PathGradientBrush" /> are tiled. </param>
	public PathGradientBrush(PointF[] points, WrapMode wrapMode)
	{
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		if (wrapMode < WrapMode.Tile || wrapMode > WrapMode.Clamp)
		{
			throw new InvalidEnumArgumentException("WrapMode");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipCreatePathGradient(points, points.Length, wrapMode, out var brush));
		SetNativeBrush(brush);
	}

	/// <summary>Updates the brush's transformation matrix with the product of brush's transformation matrix multiplied by another matrix.</summary>
	/// <param name="matrix">The <see cref="T:System.Drawing.Drawing2D.Matrix" /> that will be multiplied by the brush's current transformation matrix. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void MultiplyTransform(Matrix matrix)
	{
		MultiplyTransform(matrix, MatrixOrder.Prepend);
	}

	/// <summary>Updates the brush's transformation matrix with the product of the brush's transformation matrix multiplied by another matrix.</summary>
	/// <param name="matrix">The <see cref="T:System.Drawing.Drawing2D.Matrix" /> that will be multiplied by the brush's current transformation matrix. </param>
	/// <param name="order">A <see cref="T:System.Drawing.Drawing2D.MatrixOrder" /> that specifies in which order to multiply the two matrices. </param>
	public void MultiplyTransform(Matrix matrix, MatrixOrder order)
	{
		if (matrix == null)
		{
			throw new ArgumentNullException("matrix");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipMultiplyPathGradientTransform(base.NativeBrush, matrix.nativeMatrix, order));
	}

	/// <summary>Resets the <see cref="P:System.Drawing.Drawing2D.PathGradientBrush.Transform" /> property to identity.</summary>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void ResetTransform()
	{
		GDIPlus.CheckStatus(GDIPlus.GdipResetPathGradientTransform(base.NativeBrush));
	}

	/// <summary>Rotates the local geometric transform by the specified amount. This method prepends the rotation to the transform.</summary>
	/// <param name="angle">The angle (extent) of rotation. </param>
	public void RotateTransform(float angle)
	{
		RotateTransform(angle, MatrixOrder.Prepend);
	}

	/// <summary>Rotates the local geometric transform by the specified amount in the specified order.</summary>
	/// <param name="angle">The angle (extent) of rotation. </param>
	/// <param name="order">A <see cref="T:System.Drawing.Drawing2D.MatrixOrder" /> that specifies whether to append or prepend the rotation matrix. </param>
	public void RotateTransform(float angle, MatrixOrder order)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipRotatePathGradientTransform(base.NativeBrush, angle, order));
	}

	/// <summary>Scales the local geometric transform by the specified amounts. This method prepends the scaling matrix to the transform.</summary>
	/// <param name="sx">The transform scale factor in the x-axis direction. </param>
	/// <param name="sy">The transform scale factor in the y-axis direction. </param>
	public void ScaleTransform(float sx, float sy)
	{
		ScaleTransform(sx, sy, MatrixOrder.Prepend);
	}

	/// <summary>Scales the local geometric transform by the specified amounts in the specified order.</summary>
	/// <param name="sx">The transform scale factor in the x-axis direction. </param>
	/// <param name="sy">The transform scale factor in the y-axis direction. </param>
	/// <param name="order">A <see cref="T:System.Drawing.Drawing2D.MatrixOrder" /> that specifies whether to append or prepend the scaling matrix. </param>
	public void ScaleTransform(float sx, float sy, MatrixOrder order)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipScalePathGradientTransform(base.NativeBrush, sx, sy, order));
	}

	/// <summary>Creates a gradient with a center color and a linear falloff to one surrounding color.</summary>
	/// <param name="focus">A value from 0 through 1 that specifies where, along any radial from the center of the path to the path's boundary, the center color will be at its highest intensity. A value of 1 (the default) places the highest intensity at the center of the path. </param>
	public void SetBlendTriangularShape(float focus)
	{
		SetBlendTriangularShape(focus, 1f);
	}

	/// <summary>Creates a gradient with a center color and a linear falloff to each surrounding color.</summary>
	/// <param name="focus">A value from 0 through 1 that specifies where, along any radial from the center of the path to the path's boundary, the center color will be at its highest intensity. A value of 1 (the default) places the highest intensity at the center of the path. </param>
	/// <param name="scale">A value from 0 through 1 that specifies the maximum intensity of the center color that gets blended with the boundary color. A value of 1 causes the highest possible intensity of the center color, and it is the default value. </param>
	public void SetBlendTriangularShape(float focus, float scale)
	{
		if (focus < 0f || focus > 1f || scale < 0f || scale > 1f)
		{
			throw new ArgumentException("Invalid parameter passed.");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipSetPathGradientLinearBlend(base.NativeBrush, focus, scale));
	}

	/// <summary>Creates a gradient brush that changes color starting from the center of the path outward to the path's boundary. The transition from one color to another is based on a bell-shaped curve.</summary>
	/// <param name="focus">A value from 0 through 1 that specifies where, along any radial from the center of the path to the path's boundary, the center color will be at its highest intensity. A value of 1 (the default) places the highest intensity at the center of the path. </param>
	public void SetSigmaBellShape(float focus)
	{
		SetSigmaBellShape(focus, 1f);
	}

	/// <summary>Creates a gradient brush that changes color starting from the center of the path outward to the path's boundary. The transition from one color to another is based on a bell-shaped curve.</summary>
	/// <param name="focus">A value from 0 through 1 that specifies where, along any radial from the center of the path to the path's boundary, the center color will be at its highest intensity. A value of 1 (the default) places the highest intensity at the center of the path. </param>
	/// <param name="scale">A value from 0 through 1 that specifies the maximum intensity of the center color that gets blended with the boundary color. A value of 1 causes the highest possible intensity of the center color, and it is the default value. </param>
	public void SetSigmaBellShape(float focus, float scale)
	{
		if (focus < 0f || focus > 1f || scale < 0f || scale > 1f)
		{
			throw new ArgumentException("Invalid parameter passed.");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipSetPathGradientSigmaBlend(base.NativeBrush, focus, scale));
	}

	/// <summary>Applies the specified translation to the local geometric transform. This method prepends the translation to the transform.</summary>
	/// <param name="dx">The value of the translation in x. </param>
	/// <param name="dy">The value of the translation in y. </param>
	public void TranslateTransform(float dx, float dy)
	{
		TranslateTransform(dx, dy, MatrixOrder.Prepend);
	}

	/// <summary>Applies the specified translation to the local geometric transform in the specified order.</summary>
	/// <param name="dx">The value of the translation in x. </param>
	/// <param name="dy">The value of the translation in y. </param>
	/// <param name="order">The order (prepend or append) in which to apply the translation. </param>
	public void TranslateTransform(float dx, float dy, MatrixOrder order)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipTranslatePathGradientTransform(base.NativeBrush, dx, dy, order));
	}

	/// <summary>Creates an exact copy of this <see cref="T:System.Drawing.Drawing2D.PathGradientBrush" />.</summary>
	/// <returns>The <see cref="T:System.Drawing.Drawing2D.PathGradientBrush" /> this method creates, cast as an object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override object Clone()
	{
		GDIPlus.CheckStatus((Status)GDIPlus.GdipCloneBrush(new HandleRef(this, base.NativeBrush), out var clonedBrush));
		return new PathGradientBrush(clonedBrush);
	}
}
