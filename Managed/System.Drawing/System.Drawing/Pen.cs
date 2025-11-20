using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace System.Drawing;

/// <summary>Defines an object used to draw lines and curves. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
/// <completionlist cref="T:System.Drawing.Pens" />
public sealed class Pen : MarshalByRefObject, ICloneable, IDisposable
{
	internal IntPtr nativeObject;

	internal bool isModifiable = true;

	private Color color;

	private CustomLineCap startCap;

	private CustomLineCap endCap;

	/// <summary>Gets or sets the alignment for this <see cref="T:System.Drawing.Pen" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.Drawing2D.PenAlignment" /> that represents the alignment for this <see cref="T:System.Drawing.Pen" />.</returns>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">The specified value is not a member of <see cref="T:System.Drawing.Drawing2D.PenAlignment" />.</exception>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Drawing.Pen.Alignment" /> property is set on an immutable <see cref="T:System.Drawing.Pen" />, such as those returned by the <see cref="T:System.Drawing.Pens" /> class.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	[System.MonoLimitation("Libgdiplus doesn't use this property for rendering")]
	public PenAlignment Alignment
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetPenMode(nativeObject, out var alignment));
			return alignment;
		}
		set
		{
			if (value < PenAlignment.Center || value > PenAlignment.Right)
			{
				throw new InvalidEnumArgumentException("Alignment", (int)value, typeof(PenAlignment));
			}
			if (isModifiable)
			{
				GDIPlus.CheckStatus(GDIPlus.GdipSetPenMode(nativeObject, value));
				return;
			}
			throw new ArgumentException(global::Locale.GetText("This Pen object can't be modified."));
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Drawing.Brush" /> that determines attributes of this <see cref="T:System.Drawing.Pen" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.Brush" /> that determines attributes of this <see cref="T:System.Drawing.Pen" />.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Drawing.Pen.Brush" /> property is set on an immutable <see cref="T:System.Drawing.Pen" />, such as those returned by the <see cref="T:System.Drawing.Pens" /> class.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public Brush Brush
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetPenBrushFill(nativeObject, out var brush));
			return new SolidBrush(brush);
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Brush");
			}
			if (!isModifiable)
			{
				throw new ArgumentException(global::Locale.GetText("This Pen object can't be modified."));
			}
			GDIPlus.CheckStatus(GDIPlus.GdipSetPenBrushFill(nativeObject, value.NativeBrush));
			color = Color.Empty;
		}
	}

	/// <summary>Gets or sets the color of this <see cref="T:System.Drawing.Pen" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.Color" /> structure that represents the color of this <see cref="T:System.Drawing.Pen" />.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Drawing.Pen.Color" /> property is set on an immutable <see cref="T:System.Drawing.Pen" />, such as those returned by the <see cref="T:System.Drawing.Pens" /> class.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public Color Color
	{
		get
		{
			if (color.Equals(Color.Empty))
			{
				GDIPlus.CheckStatus(GDIPlus.GdipGetPenColor(nativeObject, out var argb));
				color = Color.FromArgb(argb);
			}
			return color;
		}
		set
		{
			if (!isModifiable)
			{
				throw new ArgumentException(global::Locale.GetText("This Pen object can't be modified."));
			}
			GDIPlus.CheckStatus(GDIPlus.GdipSetPenColor(nativeObject, value.ToArgb()));
			color = value;
		}
	}

	/// <summary>Gets or sets an array of values that specifies a compound pen. A compound pen draws a compound line made up of parallel lines and spaces.</summary>
	/// <returns>An array of real numbers that specifies the compound array. The elements in the array must be in increasing order, not less than 0, and not greater than 1.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Drawing.Pen.CompoundArray" /> property is set on an immutable <see cref="T:System.Drawing.Pen" />, such as those returned by the <see cref="T:System.Drawing.Pens" /> class.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public float[] CompoundArray
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetPenCompoundCount(nativeObject, out var count));
			float[] array = new float[count];
			GDIPlus.CheckStatus(GDIPlus.GdipGetPenCompoundArray(nativeObject, array, count));
			return array;
		}
		set
		{
			if (isModifiable)
			{
				if (value.Length < 2)
				{
					throw new ArgumentException("Invalid parameter.");
				}
				foreach (float num in value)
				{
					if (num < 0f || num > 1f)
					{
						throw new ArgumentException("Invalid parameter.");
					}
				}
				GDIPlus.CheckStatus(GDIPlus.GdipSetPenCompoundArray(nativeObject, value, value.Length));
				return;
			}
			throw new ArgumentException(global::Locale.GetText("This Pen object can't be modified."));
		}
	}

	/// <summary>Gets or sets a custom cap to use at the end of lines drawn with this <see cref="T:System.Drawing.Pen" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.Drawing2D.CustomLineCap" /> that represents the cap used at the end of lines drawn with this <see cref="T:System.Drawing.Pen" />.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Drawing.Pen.CustomEndCap" /> property is set on an immutable <see cref="T:System.Drawing.Pen" />, such as those returned by the <see cref="T:System.Drawing.Pens" /> class.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public CustomLineCap CustomEndCap
	{
		get
		{
			return endCap;
		}
		set
		{
			if (isModifiable)
			{
				GDIPlus.CheckStatus(GDIPlus.GdipSetPenCustomEndCap(nativeObject, value.nativeCap));
				endCap = value;
				return;
			}
			throw new ArgumentException(global::Locale.GetText("This Pen object can't be modified."));
		}
	}

	/// <summary>Gets or sets a custom cap to use at the beginning of lines drawn with this <see cref="T:System.Drawing.Pen" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.Drawing2D.CustomLineCap" /> that represents the cap used at the beginning of lines drawn with this <see cref="T:System.Drawing.Pen" />.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Drawing.Pen.CustomStartCap" /> property is set on an immutable <see cref="T:System.Drawing.Pen" />, such as those returned by the <see cref="T:System.Drawing.Pens" /> class.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public CustomLineCap CustomStartCap
	{
		get
		{
			return startCap;
		}
		set
		{
			if (isModifiable)
			{
				GDIPlus.CheckStatus(GDIPlus.GdipSetPenCustomStartCap(nativeObject, value.nativeCap));
				startCap = value;
				return;
			}
			throw new ArgumentException(global::Locale.GetText("This Pen object can't be modified."));
		}
	}

	/// <summary>Gets or sets the cap style used at the end of the dashes that make up dashed lines drawn with this <see cref="T:System.Drawing.Pen" />.</summary>
	/// <returns>One of the <see cref="T:System.Drawing.Drawing2D.DashCap" /> values that represents the cap style used at the beginning and end of the dashes that make up dashed lines drawn with this <see cref="T:System.Drawing.Pen" />.</returns>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">The specified value is not a member of <see cref="T:System.Drawing.Drawing2D.DashCap" />.</exception>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Drawing.Pen.DashCap" /> property is set on an immutable <see cref="T:System.Drawing.Pen" />, such as those returned by the <see cref="T:System.Drawing.Pens" /> class.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public DashCap DashCap
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetPenDashCap197819(nativeObject, out var dashCap));
			return dashCap;
		}
		set
		{
			if (value < DashCap.Flat || value > DashCap.Triangle)
			{
				throw new InvalidEnumArgumentException("DashCap", (int)value, typeof(DashCap));
			}
			if (isModifiable)
			{
				GDIPlus.CheckStatus(GDIPlus.GdipSetPenDashCap197819(nativeObject, value));
				return;
			}
			throw new ArgumentException(global::Locale.GetText("This Pen object can't be modified."));
		}
	}

	/// <summary>Gets or sets the distance from the start of a line to the beginning of a dash pattern.</summary>
	/// <returns>The distance from the start of a line to the beginning of a dash pattern.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Drawing.Pen.DashOffset" /> property is set on an immutable <see cref="T:System.Drawing.Pen" />, such as those returned by the <see cref="T:System.Drawing.Pens" /> class.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public float DashOffset
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetPenDashOffset(nativeObject, out var offset));
			return offset;
		}
		set
		{
			if (isModifiable)
			{
				GDIPlus.CheckStatus(GDIPlus.GdipSetPenDashOffset(nativeObject, value));
				return;
			}
			throw new ArgumentException(global::Locale.GetText("This Pen object can't be modified."));
		}
	}

	/// <summary>Gets or sets an array of custom dashes and spaces.</summary>
	/// <returns>An array of real numbers that specifies the lengths of alternating dashes and spaces in dashed lines.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Drawing.Pen.DashPattern" /> property is set on an immutable <see cref="T:System.Drawing.Pen" />, such as those returned by the <see cref="T:System.Drawing.Pens" /> class.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public float[] DashPattern
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetPenDashCount(nativeObject, out var count));
			float[] array;
			if (count <= 0)
			{
				array = ((DashStyle != DashStyle.Custom) ? new float[0] : new float[1] { 1f });
			}
			else
			{
				array = new float[count];
				GDIPlus.CheckStatus(GDIPlus.GdipGetPenDashArray(nativeObject, array, count));
			}
			return array;
		}
		set
		{
			if (isModifiable)
			{
				if (value.Length == 0)
				{
					throw new ArgumentException("Invalid parameter.");
				}
				for (int i = 0; i < value.Length; i++)
				{
					if (value[i] <= 0f)
					{
						throw new ArgumentException("Invalid parameter.");
					}
				}
				GDIPlus.CheckStatus(GDIPlus.GdipSetPenDashArray(nativeObject, value, value.Length));
				return;
			}
			throw new ArgumentException(global::Locale.GetText("This Pen object can't be modified."));
		}
	}

	/// <summary>Gets or sets the style used for dashed lines drawn with this <see cref="T:System.Drawing.Pen" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.Drawing2D.DashStyle" /> that represents the style used for dashed lines drawn with this <see cref="T:System.Drawing.Pen" />.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Drawing.Pen.DashStyle" /> property is set on an immutable <see cref="T:System.Drawing.Pen" />, such as those returned by the <see cref="T:System.Drawing.Pens" /> class.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public DashStyle DashStyle
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetPenDashStyle(nativeObject, out var dashStyle));
			return dashStyle;
		}
		set
		{
			if (value < DashStyle.Solid || value > DashStyle.Custom)
			{
				throw new InvalidEnumArgumentException("DashStyle", (int)value, typeof(DashStyle));
			}
			if (isModifiable)
			{
				GDIPlus.CheckStatus(GDIPlus.GdipSetPenDashStyle(nativeObject, value));
				return;
			}
			throw new ArgumentException(global::Locale.GetText("This Pen object can't be modified."));
		}
	}

	/// <summary>Gets or sets the cap style used at the beginning of lines drawn with this <see cref="T:System.Drawing.Pen" />.</summary>
	/// <returns>One of the <see cref="T:System.Drawing.Drawing2D.LineCap" /> values that represents the cap style used at the beginning of lines drawn with this <see cref="T:System.Drawing.Pen" />.</returns>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">The specified value is not a member of <see cref="T:System.Drawing.Drawing2D.LineCap" />.</exception>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Drawing.Pen.StartCap" /> property is set on an immutable <see cref="T:System.Drawing.Pen" />, such as those returned by the <see cref="T:System.Drawing.Pens" /> class.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public LineCap StartCap
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetPenStartCap(nativeObject, out var result));
			return result;
		}
		set
		{
			if (value < LineCap.Flat || value > LineCap.Custom)
			{
				throw new InvalidEnumArgumentException("StartCap", (int)value, typeof(LineCap));
			}
			if (isModifiable)
			{
				GDIPlus.CheckStatus(GDIPlus.GdipSetPenStartCap(nativeObject, value));
				return;
			}
			throw new ArgumentException(global::Locale.GetText("This Pen object can't be modified."));
		}
	}

	/// <summary>Gets or sets the cap style used at the end of lines drawn with this <see cref="T:System.Drawing.Pen" />.</summary>
	/// <returns>One of the <see cref="T:System.Drawing.Drawing2D.LineCap" /> values that represents the cap style used at the end of lines drawn with this <see cref="T:System.Drawing.Pen" />.</returns>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">The specified value is not a member of <see cref="T:System.Drawing.Drawing2D.LineCap" />.</exception>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Drawing.Pen.EndCap" /> property is set on an immutable <see cref="T:System.Drawing.Pen" />, such as those returned by the <see cref="T:System.Drawing.Pens" /> class.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public LineCap EndCap
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetPenEndCap(nativeObject, out var result));
			return result;
		}
		set
		{
			if (value < LineCap.Flat || value > LineCap.Custom)
			{
				throw new InvalidEnumArgumentException("EndCap", (int)value, typeof(LineCap));
			}
			if (isModifiable)
			{
				GDIPlus.CheckStatus(GDIPlus.GdipSetPenEndCap(nativeObject, value));
				return;
			}
			throw new ArgumentException(global::Locale.GetText("This Pen object can't be modified."));
		}
	}

	/// <summary>Gets or sets the join style for the ends of two consecutive lines drawn with this <see cref="T:System.Drawing.Pen" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.Drawing2D.LineJoin" /> that represents the join style for the ends of two consecutive lines drawn with this <see cref="T:System.Drawing.Pen" />.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Drawing.Pen.LineJoin" /> property is set on an immutable <see cref="T:System.Drawing.Pen" />, such as those returned by the <see cref="T:System.Drawing.Pens" /> class.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public LineJoin LineJoin
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetPenLineJoin(nativeObject, out var lineJoin));
			return lineJoin;
		}
		set
		{
			if (value < LineJoin.Miter || value > LineJoin.MiterClipped)
			{
				throw new InvalidEnumArgumentException("LineJoin", (int)value, typeof(LineJoin));
			}
			if (isModifiable)
			{
				GDIPlus.CheckStatus(GDIPlus.GdipSetPenLineJoin(nativeObject, value));
				return;
			}
			throw new ArgumentException(global::Locale.GetText("This Pen object can't be modified."));
		}
	}

	/// <summary>Gets or sets the limit of the thickness of the join on a mitered corner.</summary>
	/// <returns>The limit of the thickness of the join on a mitered corner.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Drawing.Pen.MiterLimit" /> property is set on an immutable <see cref="T:System.Drawing.Pen" />, such as those returned by the <see cref="T:System.Drawing.Pens" /> class.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public float MiterLimit
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetPenMiterLimit(nativeObject, out var miterLimit));
			return miterLimit;
		}
		set
		{
			if (isModifiable)
			{
				GDIPlus.CheckStatus(GDIPlus.GdipSetPenMiterLimit(nativeObject, value));
				return;
			}
			throw new ArgumentException(global::Locale.GetText("This Pen object can't be modified."));
		}
	}

	/// <summary>Gets the style of lines drawn with this <see cref="T:System.Drawing.Pen" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.Drawing2D.PenType" /> enumeration that specifies the style of lines drawn with this <see cref="T:System.Drawing.Pen" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public PenType PenType
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetPenFillType(nativeObject, out var type));
			return type;
		}
	}

	/// <summary>Gets or sets a copy of the geometric transformation for this <see cref="T:System.Drawing.Pen" />.</summary>
	/// <returns>A copy of the <see cref="T:System.Drawing.Drawing2D.Matrix" /> that represents the geometric transformation for this <see cref="T:System.Drawing.Pen" />.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Drawing.Pen.Transform" /> property is set on an immutable <see cref="T:System.Drawing.Pen" />, such as those returned by the <see cref="T:System.Drawing.Pens" /> class.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public Matrix Transform
	{
		get
		{
			Matrix matrix = new Matrix();
			GDIPlus.CheckStatus(GDIPlus.GdipGetPenTransform(nativeObject, matrix.nativeMatrix));
			return matrix;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Transform");
			}
			if (isModifiable)
			{
				GDIPlus.CheckStatus(GDIPlus.GdipSetPenTransform(nativeObject, value.nativeMatrix));
				return;
			}
			throw new ArgumentException(global::Locale.GetText("This Pen object can't be modified."));
		}
	}

	/// <summary>Gets or sets the width of this <see cref="T:System.Drawing.Pen" />, in units of the <see cref="T:System.Drawing.Graphics" /> object used for drawing.</summary>
	/// <returns>The width of this <see cref="T:System.Drawing.Pen" />.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Drawing.Pen.Width" /> property is set on an immutable <see cref="T:System.Drawing.Pen" />, such as those returned by the <see cref="T:System.Drawing.Pens" /> class.</exception>
	/// <filterpriority>1</filterpriority>
	public float Width
	{
		get
		{
			GDIPlus.CheckStatus(GDIPlus.GdipGetPenWidth(nativeObject, out var width));
			return width;
		}
		set
		{
			if (isModifiable)
			{
				GDIPlus.CheckStatus(GDIPlus.GdipSetPenWidth(nativeObject, value));
				return;
			}
			throw new ArgumentException(global::Locale.GetText("This Pen object can't be modified."));
		}
	}

	internal IntPtr NativePen => nativeObject;

	internal Pen(IntPtr p)
	{
		nativeObject = p;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Pen" /> class with the specified <see cref="T:System.Drawing.Brush" />.</summary>
	/// <param name="brush">A <see cref="T:System.Drawing.Brush" /> that determines the fill properties of this <see cref="T:System.Drawing.Pen" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.</exception>
	public Pen(Brush brush)
		: this(brush, 1f)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Pen" /> class with the specified color.</summary>
	/// <param name="color">A <see cref="T:System.Drawing.Color" /> structure that indicates the color of this <see cref="T:System.Drawing.Pen" />. </param>
	public Pen(Color color)
		: this(color, 1f)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Pen" /> class with the specified <see cref="T:System.Drawing.Brush" /> and <see cref="P:System.Drawing.Pen.Width" />.</summary>
	/// <param name="brush">A <see cref="T:System.Drawing.Brush" /> that determines the characteristics of this <see cref="T:System.Drawing.Pen" />. </param>
	/// <param name="width">The width of the new <see cref="T:System.Drawing.Pen" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="brush" /> is null.</exception>
	public Pen(Brush brush, float width)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		GDIPlus.CheckStatus(GDIPlus.GdipCreatePen2(brush.NativeBrush, width, GraphicsUnit.World, out nativeObject));
		color = Color.Empty;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Pen" /> class with the specified <see cref="T:System.Drawing.Color" /> and <see cref="P:System.Drawing.Pen.Width" /> properties.</summary>
	/// <param name="color">A <see cref="T:System.Drawing.Color" /> structure that indicates the color of this <see cref="T:System.Drawing.Pen" />. </param>
	/// <param name="width">A value indicating the width of this <see cref="T:System.Drawing.Pen" />. </param>
	public Pen(Color color, float width)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipCreatePen1(color.ToArgb(), width, GraphicsUnit.World, out nativeObject));
		this.color = color;
	}

	/// <summary>Creates an exact copy of this <see cref="T:System.Drawing.Pen" />.</summary>
	/// <returns>An <see cref="T:System.Object" /> that can be cast to a <see cref="T:System.Drawing.Pen" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public object Clone()
	{
		GDIPlus.CheckStatus(GDIPlus.GdipClonePen(nativeObject, out var clonepen));
		return new Pen(clonepen)
		{
			startCap = startCap,
			endCap = endCap
		};
	}

	/// <summary>Releases all resources used by this <see cref="T:System.Drawing.Pen" />.</summary>
	/// <filterpriority>1</filterpriority>
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
		if (disposing && !isModifiable)
		{
			throw new ArgumentException(global::Locale.GetText("This Pen object can't be modified."));
		}
		if (nativeObject != IntPtr.Zero)
		{
			Status status = GDIPlus.GdipDeletePen(nativeObject);
			nativeObject = IntPtr.Zero;
			GDIPlus.CheckStatus(status);
		}
	}

	~Pen()
	{
		Dispose(disposing: false);
	}

	/// <summary>Multiplies the transformation matrix for this <see cref="T:System.Drawing.Pen" /> by the specified <see cref="T:System.Drawing.Drawing2D.Matrix" />.</summary>
	/// <param name="matrix">The <see cref="T:System.Drawing.Drawing2D.Matrix" /> object by which to multiply the transformation matrix. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void MultiplyTransform(Matrix matrix)
	{
		MultiplyTransform(matrix, MatrixOrder.Prepend);
	}

	/// <summary>Multiplies the transformation matrix for this <see cref="T:System.Drawing.Pen" /> by the specified <see cref="T:System.Drawing.Drawing2D.Matrix" /> in the specified order.</summary>
	/// <param name="matrix">The <see cref="T:System.Drawing.Drawing2D.Matrix" /> by which to multiply the transformation matrix. </param>
	/// <param name="order">The order in which to perform the multiplication operation. </param>
	/// <filterpriority>1</filterpriority>
	public void MultiplyTransform(Matrix matrix, MatrixOrder order)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipMultiplyPenTransform(nativeObject, matrix.nativeMatrix, order));
	}

	/// <summary>Resets the geometric transformation matrix for this <see cref="T:System.Drawing.Pen" /> to identity.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void ResetTransform()
	{
		GDIPlus.CheckStatus(GDIPlus.GdipResetPenTransform(nativeObject));
	}

	/// <summary>Rotates the local geometric transformation by the specified angle. This method prepends the rotation to the transformation.</summary>
	/// <param name="angle">The angle of rotation. </param>
	/// <filterpriority>1</filterpriority>
	public void RotateTransform(float angle)
	{
		RotateTransform(angle, MatrixOrder.Prepend);
	}

	/// <summary>Rotates the local geometric transformation by the specified angle in the specified order.</summary>
	/// <param name="angle">The angle of rotation. </param>
	/// <param name="order">A <see cref="T:System.Drawing.Drawing2D.MatrixOrder" /> that specifies whether to append or prepend the rotation matrix. </param>
	/// <filterpriority>1</filterpriority>
	public void RotateTransform(float angle, MatrixOrder order)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipRotatePenTransform(nativeObject, angle, order));
	}

	/// <summary>Scales the local geometric transformation by the specified factors. This method prepends the scaling matrix to the transformation.</summary>
	/// <param name="sx">The factor by which to scale the transformation in the x-axis direction. </param>
	/// <param name="sy">The factor by which to scale the transformation in the y-axis direction. </param>
	/// <filterpriority>1</filterpriority>
	public void ScaleTransform(float sx, float sy)
	{
		ScaleTransform(sx, sy, MatrixOrder.Prepend);
	}

	/// <summary>Scales the local geometric transformation by the specified factors in the specified order.</summary>
	/// <param name="sx">The factor by which to scale the transformation in the x-axis direction. </param>
	/// <param name="sy">The factor by which to scale the transformation in the y-axis direction. </param>
	/// <param name="order">A <see cref="T:System.Drawing.Drawing2D.MatrixOrder" /> that specifies whether to append or prepend the scaling matrix. </param>
	/// <filterpriority>1</filterpriority>
	public void ScaleTransform(float sx, float sy, MatrixOrder order)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipScalePenTransform(nativeObject, sx, sy, order));
	}

	/// <summary>Sets the values that determine the style of cap used to end lines drawn by this <see cref="T:System.Drawing.Pen" />.</summary>
	/// <param name="startCap">A <see cref="T:System.Drawing.Drawing2D.LineCap" /> that represents the cap style to use at the beginning of lines drawn with this <see cref="T:System.Drawing.Pen" />. </param>
	/// <param name="endCap">A <see cref="T:System.Drawing.Drawing2D.LineCap" /> that represents the cap style to use at the end of lines drawn with this <see cref="T:System.Drawing.Pen" />. </param>
	/// <param name="dashCap">A <see cref="T:System.Drawing.Drawing2D.LineCap" /> that represents the cap style to use at the beginning or end of dashed lines drawn with this <see cref="T:System.Drawing.Pen" />. </param>
	/// <filterpriority>1</filterpriority>
	public void SetLineCap(LineCap startCap, LineCap endCap, DashCap dashCap)
	{
		if (isModifiable)
		{
			GDIPlus.CheckStatus(GDIPlus.GdipSetPenLineCap197819(nativeObject, startCap, endCap, dashCap));
			return;
		}
		throw new ArgumentException(global::Locale.GetText("This Pen object can't be modified."));
	}

	/// <summary>Translates the local geometric transformation by the specified dimensions. This method prepends the translation to the transformation.</summary>
	/// <param name="dx">The value of the translation in x. </param>
	/// <param name="dy">The value of the translation in y. </param>
	/// <filterpriority>1</filterpriority>
	public void TranslateTransform(float dx, float dy)
	{
		TranslateTransform(dx, dy, MatrixOrder.Prepend);
	}

	/// <summary>Translates the local geometric transformation by the specified dimensions in the specified order.</summary>
	/// <param name="dx">The value of the translation in x. </param>
	/// <param name="dy">The value of the translation in y. </param>
	/// <param name="order">The order (prepend or append) in which to apply the translation. </param>
	/// <filterpriority>1</filterpriority>
	public void TranslateTransform(float dx, float dy, MatrixOrder order)
	{
		GDIPlus.CheckStatus(GDIPlus.GdipTranslatePenTransform(nativeObject, dx, dy, order));
	}
}
