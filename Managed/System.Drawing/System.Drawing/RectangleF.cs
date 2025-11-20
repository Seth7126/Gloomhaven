using System.ComponentModel;
using System.Numerics.Hashing;

namespace System.Drawing;

/// <summary>Stores a set of four floating-point numbers that represent the location and size of a rectangle. For more advanced region functions, use a <see cref="T:System.Drawing.Region" /> object.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
public struct RectangleF : IEquatable<RectangleF>
{
	/// <summary>Represents an instance of the <see cref="T:System.Drawing.RectangleF" /> class with its members uninitialized.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly RectangleF Empty;

	private float x;

	private float y;

	private float width;

	private float height;

	/// <summary>Gets or sets the coordinates of the upper-left corner of this <see cref="T:System.Drawing.RectangleF" /> structure.</summary>
	/// <returns>A <see cref="T:System.Drawing.PointF" /> that represents the upper-left corner of this <see cref="T:System.Drawing.RectangleF" /> structure.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	public PointF Location
	{
		get
		{
			return new PointF(X, Y);
		}
		set
		{
			X = value.X;
			Y = value.Y;
		}
	}

	/// <summary>Gets or sets the size of this <see cref="T:System.Drawing.RectangleF" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.SizeF" /> that represents the width and height of this <see cref="T:System.Drawing.RectangleF" /> structure.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	public SizeF Size
	{
		get
		{
			return new SizeF(Width, Height);
		}
		set
		{
			Width = value.Width;
			Height = value.Height;
		}
	}

	/// <summary>Gets or sets the x-coordinate of the upper-left corner of this <see cref="T:System.Drawing.RectangleF" /> structure.</summary>
	/// <returns>The x-coordinate of the upper-left corner of this <see cref="T:System.Drawing.RectangleF" /> structure. The default is 0.</returns>
	/// <filterpriority>1</filterpriority>
	public float X
	{
		get
		{
			return x;
		}
		set
		{
			x = value;
		}
	}

	/// <summary>Gets or sets the y-coordinate of the upper-left corner of this <see cref="T:System.Drawing.RectangleF" /> structure.</summary>
	/// <returns>The y-coordinate of the upper-left corner of this <see cref="T:System.Drawing.RectangleF" /> structure. The default is 0.</returns>
	/// <filterpriority>1</filterpriority>
	public float Y
	{
		get
		{
			return y;
		}
		set
		{
			y = value;
		}
	}

	/// <summary>Gets or sets the width of this <see cref="T:System.Drawing.RectangleF" /> structure.</summary>
	/// <returns>The width of this <see cref="T:System.Drawing.RectangleF" /> structure. The default is 0.</returns>
	/// <filterpriority>1</filterpriority>
	public float Width
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

	/// <summary>Gets or sets the height of this <see cref="T:System.Drawing.RectangleF" /> structure.</summary>
	/// <returns>The height of this <see cref="T:System.Drawing.RectangleF" /> structure. The default is 0.</returns>
	/// <filterpriority>1</filterpriority>
	public float Height
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

	/// <summary>Gets the x-coordinate of the left edge of this <see cref="T:System.Drawing.RectangleF" /> structure.</summary>
	/// <returns>The x-coordinate of the left edge of this <see cref="T:System.Drawing.RectangleF" /> structure. </returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	public float Left => X;

	/// <summary>Gets the y-coordinate of the top edge of this <see cref="T:System.Drawing.RectangleF" /> structure.</summary>
	/// <returns>The y-coordinate of the top edge of this <see cref="T:System.Drawing.RectangleF" /> structure.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	public float Top => Y;

	/// <summary>Gets the x-coordinate that is the sum of <see cref="P:System.Drawing.RectangleF.X" /> and <see cref="P:System.Drawing.RectangleF.Width" /> of this <see cref="T:System.Drawing.RectangleF" /> structure.</summary>
	/// <returns>The x-coordinate that is the sum of <see cref="P:System.Drawing.RectangleF.X" /> and <see cref="P:System.Drawing.RectangleF.Width" /> of this <see cref="T:System.Drawing.RectangleF" /> structure. </returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	public float Right => X + Width;

	/// <summary>Gets the y-coordinate that is the sum of <see cref="P:System.Drawing.RectangleF.Y" /> and <see cref="P:System.Drawing.RectangleF.Height" /> of this <see cref="T:System.Drawing.RectangleF" /> structure.</summary>
	/// <returns>The y-coordinate that is the sum of <see cref="P:System.Drawing.RectangleF.Y" /> and <see cref="P:System.Drawing.RectangleF.Height" /> of this <see cref="T:System.Drawing.RectangleF" /> structure.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	public float Bottom => Y + Height;

	/// <summary>Tests whether the <see cref="P:System.Drawing.RectangleF.Width" /> or <see cref="P:System.Drawing.RectangleF.Height" /> property of this <see cref="T:System.Drawing.RectangleF" /> has a value of zero.</summary>
	/// <returns>This property returns true if the <see cref="P:System.Drawing.RectangleF.Width" /> or <see cref="P:System.Drawing.RectangleF.Height" /> property of this <see cref="T:System.Drawing.RectangleF" /> has a value of zero; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	public bool IsEmpty
	{
		get
		{
			if (!(Width <= 0f))
			{
				return Height <= 0f;
			}
			return true;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.RectangleF" /> class with the specified location and size.</summary>
	/// <param name="x">The x-coordinate of the upper-left corner of the rectangle. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the rectangle. </param>
	/// <param name="width">The width of the rectangle. </param>
	/// <param name="height">The height of the rectangle. </param>
	public RectangleF(float x, float y, float width, float height)
	{
		this.x = x;
		this.y = y;
		this.width = width;
		this.height = height;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.RectangleF" /> class with the specified location and size.</summary>
	/// <param name="location">A <see cref="T:System.Drawing.PointF" /> that represents the upper-left corner of the rectangular region. </param>
	/// <param name="size">A <see cref="T:System.Drawing.SizeF" /> that represents the width and height of the rectangular region. </param>
	public RectangleF(PointF location, SizeF size)
	{
		x = location.X;
		y = location.Y;
		width = size.Width;
		height = size.Height;
	}

	/// <summary>Creates a <see cref="T:System.Drawing.RectangleF" /> structure with upper-left corner and lower-right corner at the specified locations.</summary>
	/// <returns>The new <see cref="T:System.Drawing.RectangleF" /> that this method creates.</returns>
	/// <param name="left">The x-coordinate of the upper-left corner of the rectangular region. </param>
	/// <param name="top">The y-coordinate of the upper-left corner of the rectangular region. </param>
	/// <param name="right">The x-coordinate of the lower-right corner of the rectangular region. </param>
	/// <param name="bottom">The y-coordinate of the lower-right corner of the rectangular region. </param>
	/// <filterpriority>1</filterpriority>
	public static RectangleF FromLTRB(float left, float top, float right, float bottom)
	{
		return new RectangleF(left, top, right - left, bottom - top);
	}

	/// <summary>Tests whether <paramref name="obj" /> is a <see cref="T:System.Drawing.RectangleF" /> with the same location and size of this <see cref="T:System.Drawing.RectangleF" />.</summary>
	/// <returns>This method returns true if <paramref name="obj" /> is a <see cref="T:System.Drawing.RectangleF" /> and its X, Y, Width, and Height properties are equal to the corresponding properties of this <see cref="T:System.Drawing.RectangleF" />; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.Object" /> to test. </param>
	/// <filterpriority>1</filterpriority>
	public override bool Equals(object obj)
	{
		if (obj is RectangleF)
		{
			return Equals((RectangleF)obj);
		}
		return false;
	}

	public bool Equals(RectangleF other)
	{
		return this == other;
	}

	/// <summary>Tests whether two <see cref="T:System.Drawing.RectangleF" /> structures have equal location and size.</summary>
	/// <returns>This operator returns true if the two specified <see cref="T:System.Drawing.RectangleF" /> structures have equal <see cref="P:System.Drawing.RectangleF.X" />, <see cref="P:System.Drawing.RectangleF.Y" />, <see cref="P:System.Drawing.RectangleF.Width" />, and <see cref="P:System.Drawing.RectangleF.Height" /> properties.</returns>
	/// <param name="left">The <see cref="T:System.Drawing.RectangleF" /> structure that is to the left of the equality operator. </param>
	/// <param name="right">The <see cref="T:System.Drawing.RectangleF" /> structure that is to the right of the equality operator. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator ==(RectangleF left, RectangleF right)
	{
		if (left.X == right.X && left.Y == right.Y && left.Width == right.Width)
		{
			return left.Height == right.Height;
		}
		return false;
	}

	/// <summary>Tests whether two <see cref="T:System.Drawing.RectangleF" /> structures differ in location or size.</summary>
	/// <returns>This operator returns true if any of the <see cref="P:System.Drawing.RectangleF.X" /> , <see cref="P:System.Drawing.RectangleF.Y" />, <see cref="P:System.Drawing.RectangleF.Width" />, or <see cref="P:System.Drawing.RectangleF.Height" /> properties of the two <see cref="T:System.Drawing.Rectangle" /> structures are unequal; otherwise false.</returns>
	/// <param name="left">The <see cref="T:System.Drawing.RectangleF" /> structure that is to the left of the inequality operator. </param>
	/// <param name="right">The <see cref="T:System.Drawing.RectangleF" /> structure that is to the right of the inequality operator. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator !=(RectangleF left, RectangleF right)
	{
		return !(left == right);
	}

	/// <summary>Determines if the specified point is contained within this <see cref="T:System.Drawing.RectangleF" /> structure.</summary>
	/// <returns>This method returns true if the point defined by <paramref name="x" /> and <paramref name="y" /> is contained within this <see cref="T:System.Drawing.RectangleF" /> structure; otherwise false.</returns>
	/// <param name="x">The x-coordinate of the point to test. </param>
	/// <param name="y">The y-coordinate of the point to test. </param>
	/// <filterpriority>1</filterpriority>
	public bool Contains(float x, float y)
	{
		if (X <= x && x < X + Width && Y <= y)
		{
			return y < Y + Height;
		}
		return false;
	}

	/// <summary>Determines if the specified point is contained within this <see cref="T:System.Drawing.RectangleF" /> structure.</summary>
	/// <returns>This method returns true if the point represented by the <paramref name="pt" /> parameter is contained within this <see cref="T:System.Drawing.RectangleF" /> structure; otherwise false.</returns>
	/// <param name="pt">The <see cref="T:System.Drawing.PointF" /> to test. </param>
	/// <filterpriority>1</filterpriority>
	public bool Contains(PointF pt)
	{
		return Contains(pt.X, pt.Y);
	}

	/// <summary>Determines if the rectangular region represented by <paramref name="rect" /> is entirely contained within this <see cref="T:System.Drawing.RectangleF" /> structure.</summary>
	/// <returns>This method returns true if the rectangular region represented by <paramref name="rect" /> is entirely contained within the rectangular region represented by this <see cref="T:System.Drawing.RectangleF" />; otherwise false.</returns>
	/// <param name="rect">The <see cref="T:System.Drawing.RectangleF" /> to test. </param>
	/// <filterpriority>1</filterpriority>
	public bool Contains(RectangleF rect)
	{
		if (X <= rect.X && rect.X + rect.Width <= X + Width && Y <= rect.Y)
		{
			return rect.Y + rect.Height <= Y + Height;
		}
		return false;
	}

	/// <summary>Gets the hash code for this <see cref="T:System.Drawing.RectangleF" /> structure. For information about the use of hash codes, see Object.GetHashCode.</summary>
	/// <returns>The hash code for this <see cref="T:System.Drawing.RectangleF" />.</returns>
	/// <filterpriority>1</filterpriority>
	public override int GetHashCode()
	{
		return System.Numerics.Hashing.HashHelpers.Combine(System.Numerics.Hashing.HashHelpers.Combine(System.Numerics.Hashing.HashHelpers.Combine(X.GetHashCode(), Y.GetHashCode()), Width.GetHashCode()), Height.GetHashCode());
	}

	/// <summary>Enlarges this <see cref="T:System.Drawing.RectangleF" /> structure by the specified amount.</summary>
	/// <param name="x">The amount to inflate this <see cref="T:System.Drawing.RectangleF" /> structure horizontally. </param>
	/// <param name="y">The amount to inflate this <see cref="T:System.Drawing.RectangleF" /> structure vertically. </param>
	/// <filterpriority>1</filterpriority>
	public void Inflate(float x, float y)
	{
		X -= x;
		Y -= y;
		Width += 2f * x;
		Height += 2f * y;
	}

	/// <summary>Enlarges this <see cref="T:System.Drawing.RectangleF" /> by the specified amount.</summary>
	/// <param name="size">The amount to inflate this rectangle. </param>
	/// <filterpriority>1</filterpriority>
	public void Inflate(SizeF size)
	{
		Inflate(size.Width, size.Height);
	}

	/// <summary>Creates and returns an enlarged copy of the specified <see cref="T:System.Drawing.RectangleF" /> structure. The copy is enlarged by the specified amount and the original rectangle remains unmodified.</summary>
	/// <returns>The enlarged <see cref="T:System.Drawing.RectangleF" />.</returns>
	/// <param name="rect">The <see cref="T:System.Drawing.RectangleF" /> to be copied. This rectangle is not modified. </param>
	/// <param name="x">The amount to enlarge the copy of the rectangle horizontally. </param>
	/// <param name="y">The amount to enlarge the copy of the rectangle vertically. </param>
	/// <filterpriority>1</filterpriority>
	public static RectangleF Inflate(RectangleF rect, float x, float y)
	{
		RectangleF result = rect;
		result.Inflate(x, y);
		return result;
	}

	/// <summary>Replaces this <see cref="T:System.Drawing.RectangleF" /> structure with the intersection of itself and the specified <see cref="T:System.Drawing.RectangleF" /> structure.</summary>
	/// <param name="rect">The rectangle to intersect. </param>
	/// <filterpriority>1</filterpriority>
	public void Intersect(RectangleF rect)
	{
		RectangleF rectangleF = Intersect(rect, this);
		X = rectangleF.X;
		Y = rectangleF.Y;
		Width = rectangleF.Width;
		Height = rectangleF.Height;
	}

	/// <summary>Returns a <see cref="T:System.Drawing.RectangleF" /> structure that represents the intersection of two rectangles. If there is no intersection, and empty <see cref="T:System.Drawing.RectangleF" /> is returned.</summary>
	/// <returns>A third <see cref="T:System.Drawing.RectangleF" /> structure the size of which represents the overlapped area of the two specified rectangles.</returns>
	/// <param name="a">A rectangle to intersect. </param>
	/// <param name="b">A rectangle to intersect. </param>
	/// <filterpriority>1</filterpriority>
	public static RectangleF Intersect(RectangleF a, RectangleF b)
	{
		float num = Math.Max(a.X, b.X);
		float num2 = Math.Min(a.X + a.Width, b.X + b.Width);
		float num3 = Math.Max(a.Y, b.Y);
		float num4 = Math.Min(a.Y + a.Height, b.Y + b.Height);
		if (num2 >= num && num4 >= num3)
		{
			return new RectangleF(num, num3, num2 - num, num4 - num3);
		}
		return Empty;
	}

	/// <summary>Determines if this rectangle intersects with <paramref name="rect" />.</summary>
	/// <returns>This method returns true if there is any intersection.</returns>
	/// <param name="rect">The rectangle to test. </param>
	/// <filterpriority>1</filterpriority>
	public bool IntersectsWith(RectangleF rect)
	{
		if (rect.X < X + Width && X < rect.X + rect.Width && rect.Y < Y + Height)
		{
			return Y < rect.Y + rect.Height;
		}
		return false;
	}

	/// <summary>Creates the smallest possible third rectangle that can contain both of two rectangles that form a union.</summary>
	/// <returns>A third <see cref="T:System.Drawing.RectangleF" /> structure that contains both of the two rectangles that form the union.</returns>
	/// <param name="a">A rectangle to union. </param>
	/// <param name="b">A rectangle to union. </param>
	/// <filterpriority>1</filterpriority>
	public static RectangleF Union(RectangleF a, RectangleF b)
	{
		float num = Math.Min(a.X, b.X);
		float num2 = Math.Max(a.X + a.Width, b.X + b.Width);
		float num3 = Math.Min(a.Y, b.Y);
		float num4 = Math.Max(a.Y + a.Height, b.Y + b.Height);
		return new RectangleF(num, num3, num2 - num, num4 - num3);
	}

	/// <summary>Adjusts the location of this rectangle by the specified amount.</summary>
	/// <param name="pos">The amount to offset the location. </param>
	/// <filterpriority>1</filterpriority>
	public void Offset(PointF pos)
	{
		Offset(pos.X, pos.Y);
	}

	/// <summary>Adjusts the location of this rectangle by the specified amount.</summary>
	/// <param name="x">The amount to offset the location horizontally. </param>
	/// <param name="y">The amount to offset the location vertically. </param>
	/// <filterpriority>1</filterpriority>
	public void Offset(float x, float y)
	{
		X += x;
		Y += y;
	}

	/// <summary>Converts the specified <see cref="T:System.Drawing.Rectangle" /> structure to a <see cref="T:System.Drawing.RectangleF" /> structure.</summary>
	/// <returns>The <see cref="T:System.Drawing.RectangleF" /> structure that is converted from the specified <see cref="T:System.Drawing.Rectangle" /> structure.</returns>
	/// <param name="r">The <see cref="T:System.Drawing.Rectangle" /> structure to convert. </param>
	/// <filterpriority>3</filterpriority>
	public static implicit operator RectangleF(Rectangle r)
	{
		return new RectangleF(r.X, r.Y, r.Width, r.Height);
	}

	/// <summary>Converts the Location and <see cref="T:System.Drawing.Size" /> of this <see cref="T:System.Drawing.RectangleF" /> to a human-readable string.</summary>
	/// <returns>A string that contains the position, width, and height of this <see cref="T:System.Drawing.RectangleF" /> structure. For example, "{X=20, Y=20, Width=100, Height=50}".</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override string ToString()
	{
		return "{X=" + X + ",Y=" + Y + ",Width=" + Width + ",Height=" + Height + "}";
	}
}
