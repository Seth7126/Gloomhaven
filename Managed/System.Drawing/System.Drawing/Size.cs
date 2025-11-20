using System.ComponentModel;
using System.Numerics.Hashing;

namespace System.Drawing;

/// <summary>Stores an ordered pair of integers, which specify a <see cref="P:System.Drawing.Size.Height" /> and <see cref="P:System.Drawing.Size.Width" />.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[TypeConverter(typeof(SizeConverter))]
public struct Size : IEquatable<Size>
{
	/// <summary>Gets a <see cref="T:System.Drawing.Size" /> structure that has a <see cref="P:System.Drawing.Size.Height" /> and <see cref="P:System.Drawing.Size.Width" /> value of 0. </summary>
	/// <returns>A <see cref="T:System.Drawing.Size" /> that has a <see cref="P:System.Drawing.Size.Height" /> and <see cref="P:System.Drawing.Size.Width" /> value of 0.</returns>
	/// <filterpriority>1</filterpriority>
	public static readonly Size Empty;

	private int width;

	private int height;

	/// <summary>Tests whether this <see cref="T:System.Drawing.Size" /> structure has width and height of 0.</summary>
	/// <returns>This property returns true when this <see cref="T:System.Drawing.Size" /> structure has both a width and height of 0; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	public bool IsEmpty
	{
		get
		{
			if (width == 0)
			{
				return height == 0;
			}
			return false;
		}
	}

	/// <summary>Gets or sets the horizontal component of this <see cref="T:System.Drawing.Size" /> structure.</summary>
	/// <returns>The horizontal component of this <see cref="T:System.Drawing.Size" /> structure, typically measured in pixels.</returns>
	/// <filterpriority>1</filterpriority>
	public int Width
	{
		get
		{
			return width;
		}
		set
		{
			width = value;
		}
	}

	/// <summary>Gets or sets the vertical component of this <see cref="T:System.Drawing.Size" /> structure.</summary>
	/// <returns>The vertical component of this <see cref="T:System.Drawing.Size" /> structure, typically measured in pixels.</returns>
	/// <filterpriority>1</filterpriority>
	public int Height
	{
		get
		{
			return height;
		}
		set
		{
			height = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Size" /> structure from the specified <see cref="T:System.Drawing.Point" /> structure.</summary>
	/// <param name="pt">The <see cref="T:System.Drawing.Point" /> structure from which to initialize this <see cref="T:System.Drawing.Size" /> structure. </param>
	public Size(Point pt)
	{
		width = pt.X;
		height = pt.Y;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Size" /> structure from the specified dimensions.</summary>
	/// <param name="width">The width component of the new <see cref="T:System.Drawing.Size" />. </param>
	/// <param name="height">The height component of the new <see cref="T:System.Drawing.Size" />. </param>
	public Size(int width, int height)
	{
		this.width = width;
		this.height = height;
	}

	/// <summary>Converts the specified <see cref="T:System.Drawing.Size" /> structure to a <see cref="T:System.Drawing.SizeF" /> structure.</summary>
	/// <returns>The <see cref="T:System.Drawing.SizeF" /> structure to which this operator converts.</returns>
	/// <param name="p">The <see cref="T:System.Drawing.Size" /> structure to convert. </param>
	/// <filterpriority>3</filterpriority>
	public static implicit operator SizeF(Size p)
	{
		return new SizeF(p.Width, p.Height);
	}

	/// <summary>Adds the width and height of one <see cref="T:System.Drawing.Size" /> structure to the width and height of another <see cref="T:System.Drawing.Size" /> structure.</summary>
	/// <returns>A <see cref="T:System.Drawing.Size" /> structure that is the result of the addition operation.</returns>
	/// <param name="sz1">The first <see cref="T:System.Drawing.Size" /> to add. </param>
	/// <param name="sz2">The second <see cref="T:System.Drawing.Size" /> to add. </param>
	/// <filterpriority>3</filterpriority>
	public static Size operator +(Size sz1, Size sz2)
	{
		return Add(sz1, sz2);
	}

	/// <summary>Subtracts the width and height of one <see cref="T:System.Drawing.Size" /> structure from the width and height of another <see cref="T:System.Drawing.Size" /> structure.</summary>
	/// <returns>A <see cref="T:System.Drawing.Size" /> structure that is the result of the subtraction operation.</returns>
	/// <param name="sz1">The <see cref="T:System.Drawing.Size" /> structure on the left side of the subtraction operator. </param>
	/// <param name="sz2">The <see cref="T:System.Drawing.Size" /> structure on the right side of the subtraction operator. </param>
	/// <filterpriority>3</filterpriority>
	public static Size operator -(Size sz1, Size sz2)
	{
		return Subtract(sz1, sz2);
	}

	public static Size operator *(int left, Size right)
	{
		return Multiply(right, left);
	}

	public static Size operator *(Size left, int right)
	{
		return Multiply(left, right);
	}

	public static Size operator /(Size left, int right)
	{
		return new Size(left.width / right, left.height / right);
	}

	public static SizeF operator *(float left, Size right)
	{
		return Multiply(right, left);
	}

	public static SizeF operator *(Size left, float right)
	{
		return Multiply(left, right);
	}

	public static SizeF operator /(Size left, float right)
	{
		return new SizeF((float)left.width / right, (float)left.height / right);
	}

	/// <summary>Tests whether two <see cref="T:System.Drawing.Size" /> structures are equal.</summary>
	/// <returns>true if <paramref name="sz1" /> and <paramref name="sz2" /> have equal width and height; otherwise, false.</returns>
	/// <param name="sz1">The <see cref="T:System.Drawing.Size" /> structure on the left side of the equality operator. </param>
	/// <param name="sz2">The <see cref="T:System.Drawing.Size" /> structure on the right of the equality operator. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator ==(Size sz1, Size sz2)
	{
		if (sz1.Width == sz2.Width)
		{
			return sz1.Height == sz2.Height;
		}
		return false;
	}

	/// <summary>Tests whether two <see cref="T:System.Drawing.Size" /> structures are different.</summary>
	/// <returns>true if <paramref name="sz1" /> and <paramref name="sz2" /> differ either in width or height; false if <paramref name="sz1" /> and <paramref name="sz2" /> are equal.</returns>
	/// <param name="sz1">The <see cref="T:System.Drawing.Size" /> structure on the left of the inequality operator. </param>
	/// <param name="sz2">The <see cref="T:System.Drawing.Size" /> structure on the right of the inequality operator. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator !=(Size sz1, Size sz2)
	{
		return !(sz1 == sz2);
	}

	/// <summary>Converts the specified <see cref="T:System.Drawing.Size" /> structure to a <see cref="T:System.Drawing.Point" /> structure.</summary>
	/// <returns>The <see cref="T:System.Drawing.Point" /> structure to which this operator converts.</returns>
	/// <param name="size">The <see cref="T:System.Drawing.Size" /> structure to convert. </param>
	/// <filterpriority>3</filterpriority>
	public static explicit operator Point(Size size)
	{
		return new Point(size.Width, size.Height);
	}

	/// <summary>Adds the width and height of one <see cref="T:System.Drawing.Size" /> structure to the width and height of another <see cref="T:System.Drawing.Size" /> structure.</summary>
	/// <returns>A <see cref="T:System.Drawing.Size" /> structure that is the result of the addition operation.</returns>
	/// <param name="sz1">The first <see cref="T:System.Drawing.Size" /> structure to add.</param>
	/// <param name="sz2">The second <see cref="T:System.Drawing.Size" /> structure to add.</param>
	public static Size Add(Size sz1, Size sz2)
	{
		return new Size(sz1.Width + sz2.Width, sz1.Height + sz2.Height);
	}

	/// <summary>Converts the specified <see cref="T:System.Drawing.SizeF" /> structure to a <see cref="T:System.Drawing.Size" /> structure by rounding the values of the <see cref="T:System.Drawing.Size" /> structure to the next higher integer values.</summary>
	/// <returns>The <see cref="T:System.Drawing.Size" /> structure this method converts to.</returns>
	/// <param name="value">The <see cref="T:System.Drawing.SizeF" /> structure to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static Size Ceiling(SizeF value)
	{
		return new Size((int)Math.Ceiling(value.Width), (int)Math.Ceiling(value.Height));
	}

	/// <summary>Subtracts the width and height of one <see cref="T:System.Drawing.Size" /> structure from the width and height of another <see cref="T:System.Drawing.Size" /> structure.</summary>
	/// <returns>A <see cref="T:System.Drawing.Size" /> structure that is a result of the subtraction operation.</returns>
	/// <param name="sz1">The <see cref="T:System.Drawing.Size" /> structure on the left side of the subtraction operator. </param>
	/// <param name="sz2">The <see cref="T:System.Drawing.Size" /> structure on the right side of the subtraction operator. </param>
	public static Size Subtract(Size sz1, Size sz2)
	{
		return new Size(sz1.Width - sz2.Width, sz1.Height - sz2.Height);
	}

	/// <summary>Converts the specified <see cref="T:System.Drawing.SizeF" /> structure to a <see cref="T:System.Drawing.Size" /> structure by truncating the values of the <see cref="T:System.Drawing.SizeF" /> structure to the next lower integer values.</summary>
	/// <returns>The <see cref="T:System.Drawing.Size" /> structure this method converts to.</returns>
	/// <param name="value">The <see cref="T:System.Drawing.SizeF" /> structure to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static Size Truncate(SizeF value)
	{
		return new Size((int)value.Width, (int)value.Height);
	}

	/// <summary>Converts the specified <see cref="T:System.Drawing.SizeF" /> structure to a <see cref="T:System.Drawing.Size" /> structure by rounding the values of the <see cref="T:System.Drawing.SizeF" /> structure to the nearest integer values.</summary>
	/// <returns>The <see cref="T:System.Drawing.Size" /> structure this method converts to.</returns>
	/// <param name="value">The <see cref="T:System.Drawing.SizeF" /> structure to convert. </param>
	/// <filterpriority>1</filterpriority>
	public static Size Round(SizeF value)
	{
		return new Size((int)Math.Round(value.Width), (int)Math.Round(value.Height));
	}

	/// <summary>Tests to see whether the specified object is a <see cref="T:System.Drawing.Size" /> structure with the same dimensions as this <see cref="T:System.Drawing.Size" /> structure.</summary>
	/// <returns>true if <paramref name="obj" /> is a <see cref="T:System.Drawing.Size" /> and has the same width and height as this <see cref="T:System.Drawing.Size" />; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.Object" /> to test. </param>
	/// <filterpriority>1</filterpriority>
	public override bool Equals(object obj)
	{
		if (obj is Size)
		{
			return Equals((Size)obj);
		}
		return false;
	}

	public bool Equals(Size other)
	{
		return this == other;
	}

	/// <summary>Returns a hash code for this <see cref="T:System.Drawing.Size" /> structure.</summary>
	/// <returns>An integer value that specifies a hash value for this <see cref="T:System.Drawing.Size" /> structure.</returns>
	/// <filterpriority>1</filterpriority>
	public override int GetHashCode()
	{
		return System.Numerics.Hashing.HashHelpers.Combine(Width, Height);
	}

	/// <summary>Creates a human-readable string that represents this <see cref="T:System.Drawing.Size" /> structure.</summary>
	/// <returns>A string that represents this <see cref="T:System.Drawing.Size" />.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override string ToString()
	{
		return "{Width=" + width + ", Height=" + height + "}";
	}

	private static Size Multiply(Size size, int multiplier)
	{
		return new Size(size.width * multiplier, size.height * multiplier);
	}

	private static SizeF Multiply(Size size, float multiplier)
	{
		return new SizeF((float)size.width * multiplier, (float)size.height * multiplier);
	}
}
