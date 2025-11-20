using System.ComponentModel;
using System.Numerics.Hashing;

namespace System.Drawing;

/// <summary>Stores a set of four integers that represent the location and size of a rectangle</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[TypeConverter(typeof(RectangleConverter))]
public struct Rectangle : IEquatable<Rectangle>
{
	/// <summary>Represents a <see cref="T:System.Drawing.Rectangle" /> structure with its properties left uninitialized.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly Rectangle Empty;

	private int x;

	private int y;

	private int width;

	private int height;

	/// <summary>Gets or sets the coordinates of the upper-left corner of this <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <returns>A <see cref="T:System.Drawing.Point" /> that represents the upper-left corner of this <see cref="T:System.Drawing.Rectangle" /> structure.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	public Point Location
	{
		get
		{
			return new Point(X, Y);
		}
		set
		{
			X = value.X;
			Y = value.Y;
		}
	}

	/// <summary>Gets or sets the size of this <see cref="T:System.Drawing.Rectangle" />.</summary>
	/// <returns>A <see cref="T:System.Drawing.Size" /> that represents the width and height of this <see cref="T:System.Drawing.Rectangle" /> structure.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	public Size Size
	{
		get
		{
			return new Size(Width, Height);
		}
		set
		{
			Width = value.Width;
			Height = value.Height;
		}
	}

	/// <summary>Gets or sets the x-coordinate of the upper-left corner of this <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <returns>The x-coordinate of the upper-left corner of this <see cref="T:System.Drawing.Rectangle" /> structure. The default is 0.</returns>
	/// <filterpriority>1</filterpriority>
	public int X
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

	/// <summary>Gets or sets the y-coordinate of the upper-left corner of this <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <returns>The y-coordinate of the upper-left corner of this <see cref="T:System.Drawing.Rectangle" /> structure. The default is 0.</returns>
	/// <filterpriority>1</filterpriority>
	public int Y
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

	/// <summary>Gets or sets the width of this <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <returns>The width of this <see cref="T:System.Drawing.Rectangle" /> structure. The default is 0.</returns>
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

	/// <summary>Gets or sets the height of this <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <returns>The height of this <see cref="T:System.Drawing.Rectangle" /> structure. The default is 0.</returns>
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

	/// <summary>Gets the x-coordinate of the left edge of this <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <returns>The x-coordinate of the left edge of this <see cref="T:System.Drawing.Rectangle" /> structure.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	public int Left => X;

	/// <summary>Gets the y-coordinate of the top edge of this <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <returns>The y-coordinate of the top edge of this <see cref="T:System.Drawing.Rectangle" /> structure.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	public int Top => Y;

	/// <summary>Gets the x-coordinate that is the sum of <see cref="P:System.Drawing.Rectangle.X" /> and <see cref="P:System.Drawing.Rectangle.Width" /> property values of this <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <returns>The x-coordinate that is the sum of <see cref="P:System.Drawing.Rectangle.X" /> and <see cref="P:System.Drawing.Rectangle.Width" /> of this <see cref="T:System.Drawing.Rectangle" />.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	public int Right => X + Width;

	/// <summary>Gets the y-coordinate that is the sum of the <see cref="P:System.Drawing.Rectangle.Y" /> and <see cref="P:System.Drawing.Rectangle.Height" /> property values of this <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <returns>The y-coordinate that is the sum of <see cref="P:System.Drawing.Rectangle.Y" /> and <see cref="P:System.Drawing.Rectangle.Height" /> of this <see cref="T:System.Drawing.Rectangle" />.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	public int Bottom => Y + Height;

	/// <summary>Tests whether all numeric properties of this <see cref="T:System.Drawing.Rectangle" /> have values of zero.</summary>
	/// <returns>This property returns true if the <see cref="P:System.Drawing.Rectangle.Width" />, <see cref="P:System.Drawing.Rectangle.Height" />, <see cref="P:System.Drawing.Rectangle.X" />, and <see cref="P:System.Drawing.Rectangle.Y" /> properties of this <see cref="T:System.Drawing.Rectangle" /> all have values of zero; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	[Browsable(false)]
	public bool IsEmpty
	{
		get
		{
			if (height == 0 && width == 0 && x == 0)
			{
				return y == 0;
			}
			return false;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Rectangle" /> class with the specified location and size.</summary>
	/// <param name="x">The x-coordinate of the upper-left corner of the rectangle. </param>
	/// <param name="y">The y-coordinate of the upper-left corner of the rectangle. </param>
	/// <param name="width">The width of the rectangle. </param>
	/// <param name="height">The height of the rectangle. </param>
	public Rectangle(int x, int y, int width, int height)
	{
		this.x = x;
		this.y = y;
		this.width = width;
		this.height = height;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Rectangle" /> class with the specified location and size.</summary>
	/// <param name="location">A <see cref="T:System.Drawing.Point" /> that represents the upper-left corner of the rectangular region. </param>
	/// <param name="size">A <see cref="T:System.Drawing.Size" /> that represents the width and height of the rectangular region. </param>
	public Rectangle(Point location, Size size)
	{
		x = location.X;
		y = location.Y;
		width = size.Width;
		height = size.Height;
	}

	/// <summary>Creates a <see cref="T:System.Drawing.Rectangle" /> structure with the specified edge locations.</summary>
	/// <returns>The new <see cref="T:System.Drawing.Rectangle" /> that this method creates.</returns>
	/// <param name="left">The x-coordinate of the upper-left corner of this <see cref="T:System.Drawing.Rectangle" /> structure. </param>
	/// <param name="top">The y-coordinate of the upper-left corner of this <see cref="T:System.Drawing.Rectangle" /> structure. </param>
	/// <param name="right">The x-coordinate of the lower-right corner of this <see cref="T:System.Drawing.Rectangle" /> structure. </param>
	/// <param name="bottom">The y-coordinate of the lower-right corner of this <see cref="T:System.Drawing.Rectangle" /> structure. </param>
	/// <filterpriority>1</filterpriority>
	public static Rectangle FromLTRB(int left, int top, int right, int bottom)
	{
		return new Rectangle(left, top, right - left, bottom - top);
	}

	/// <summary>Tests whether <paramref name="obj" /> is a <see cref="T:System.Drawing.Rectangle" /> structure with the same location and size of this <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <returns>This method returns true if <paramref name="obj" /> is a <see cref="T:System.Drawing.Rectangle" /> structure and its <see cref="P:System.Drawing.Rectangle.X" />, <see cref="P:System.Drawing.Rectangle.Y" />, <see cref="P:System.Drawing.Rectangle.Width" />, and <see cref="P:System.Drawing.Rectangle.Height" /> properties are equal to the corresponding properties of this <see cref="T:System.Drawing.Rectangle" /> structure; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.Object" /> to test. </param>
	/// <filterpriority>1</filterpriority>
	public override bool Equals(object obj)
	{
		if (obj is Rectangle)
		{
			return Equals((Rectangle)obj);
		}
		return false;
	}

	public bool Equals(Rectangle other)
	{
		return this == other;
	}

	/// <summary>Tests whether two <see cref="T:System.Drawing.Rectangle" /> structures have equal location and size.</summary>
	/// <returns>This operator returns true if the two <see cref="T:System.Drawing.Rectangle" /> structures have equal <see cref="P:System.Drawing.Rectangle.X" />, <see cref="P:System.Drawing.Rectangle.Y" />, <see cref="P:System.Drawing.Rectangle.Width" />, and <see cref="P:System.Drawing.Rectangle.Height" /> properties.</returns>
	/// <param name="left">The <see cref="T:System.Drawing.Rectangle" /> structure that is to the left of the equality operator. </param>
	/// <param name="right">The <see cref="T:System.Drawing.Rectangle" /> structure that is to the right of the equality operator. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator ==(Rectangle left, Rectangle right)
	{
		if (left.X == right.X && left.Y == right.Y && left.Width == right.Width)
		{
			return left.Height == right.Height;
		}
		return false;
	}

	/// <summary>Tests whether two <see cref="T:System.Drawing.Rectangle" /> structures differ in location or size.</summary>
	/// <returns>This operator returns true if any of the <see cref="P:System.Drawing.Rectangle.X" />, <see cref="P:System.Drawing.Rectangle.Y" />, <see cref="P:System.Drawing.Rectangle.Width" /> or <see cref="P:System.Drawing.Rectangle.Height" /> properties of the two <see cref="T:System.Drawing.Rectangle" /> structures are unequal; otherwise false.</returns>
	/// <param name="left">The <see cref="T:System.Drawing.Rectangle" /> structure that is to the left of the inequality operator. </param>
	/// <param name="right">The <see cref="T:System.Drawing.Rectangle" /> structure that is to the right of the inequality operator. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator !=(Rectangle left, Rectangle right)
	{
		return !(left == right);
	}

	/// <summary>Converts the specified <see cref="T:System.Drawing.RectangleF" /> structure to a <see cref="T:System.Drawing.Rectangle" /> structure by rounding the <see cref="T:System.Drawing.RectangleF" /> values to the next higher integer values.</summary>
	/// <returns>Returns a <see cref="T:System.Drawing.Rectangle" />.</returns>
	/// <param name="value">The <see cref="T:System.Drawing.RectangleF" /> structure to be converted. </param>
	/// <filterpriority>1</filterpriority>
	public static Rectangle Ceiling(RectangleF value)
	{
		return new Rectangle((int)Math.Ceiling(value.X), (int)Math.Ceiling(value.Y), (int)Math.Ceiling(value.Width), (int)Math.Ceiling(value.Height));
	}

	/// <summary>Converts the specified <see cref="T:System.Drawing.RectangleF" /> to a <see cref="T:System.Drawing.Rectangle" /> by truncating the <see cref="T:System.Drawing.RectangleF" /> values.</summary>
	/// <returns>The truncated value of the  <see cref="T:System.Drawing.Rectangle" />.</returns>
	/// <param name="value">The <see cref="T:System.Drawing.RectangleF" /> to be converted. </param>
	/// <filterpriority>1</filterpriority>
	public static Rectangle Truncate(RectangleF value)
	{
		return new Rectangle((int)value.X, (int)value.Y, (int)value.Width, (int)value.Height);
	}

	/// <summary>Converts the specified <see cref="T:System.Drawing.RectangleF" /> to a <see cref="T:System.Drawing.Rectangle" /> by rounding the <see cref="T:System.Drawing.RectangleF" /> values to the nearest integer values.</summary>
	/// <returns>The rounded interger value of the <see cref="T:System.Drawing.Rectangle" />.</returns>
	/// <param name="value">The <see cref="T:System.Drawing.RectangleF" /> to be converted. </param>
	/// <filterpriority>1</filterpriority>
	public static Rectangle Round(RectangleF value)
	{
		return new Rectangle((int)Math.Round(value.X), (int)Math.Round(value.Y), (int)Math.Round(value.Width), (int)Math.Round(value.Height));
	}

	/// <summary>Determines if the specified point is contained within this <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <returns>This method returns true if the point defined by <paramref name="x" /> and <paramref name="y" /> is contained within this <see cref="T:System.Drawing.Rectangle" /> structure; otherwise false.</returns>
	/// <param name="x">The x-coordinate of the point to test. </param>
	/// <param name="y">The y-coordinate of the point to test. </param>
	/// <filterpriority>1</filterpriority>
	public bool Contains(int x, int y)
	{
		if (X <= x && x < X + Width && Y <= y)
		{
			return y < Y + Height;
		}
		return false;
	}

	/// <summary>Determines if the specified point is contained within this <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <returns>This method returns true if the point represented by <paramref name="pt" /> is contained within this <see cref="T:System.Drawing.Rectangle" /> structure; otherwise false.</returns>
	/// <param name="pt">The <see cref="T:System.Drawing.Point" /> to test. </param>
	/// <filterpriority>1</filterpriority>
	public bool Contains(Point pt)
	{
		return Contains(pt.X, pt.Y);
	}

	/// <summary>Determines if the rectangular region represented by <paramref name="rect" /> is entirely contained within this <see cref="T:System.Drawing.Rectangle" /> structure.</summary>
	/// <returns>This method returns true if the rectangular region represented by <paramref name="rect" /> is entirely contained within this <see cref="T:System.Drawing.Rectangle" /> structure; otherwise false.</returns>
	/// <param name="rect">The <see cref="T:System.Drawing.Rectangle" /> to test. </param>
	/// <filterpriority>1</filterpriority>
	public bool Contains(Rectangle rect)
	{
		if (X <= rect.X && rect.X + rect.Width <= X + Width && Y <= rect.Y)
		{
			return rect.Y + rect.Height <= Y + Height;
		}
		return false;
	}

	/// <summary>Returns the hash code for this <see cref="T:System.Drawing.Rectangle" /> structure. For information about the use of hash codes, see <see cref="M:System.Object.GetHashCode" /> .</summary>
	/// <returns>An integer that represents the hash code for this rectangle.</returns>
	/// <filterpriority>1</filterpriority>
	public override int GetHashCode()
	{
		return System.Numerics.Hashing.HashHelpers.Combine(System.Numerics.Hashing.HashHelpers.Combine(System.Numerics.Hashing.HashHelpers.Combine(X, Y), Width), Height);
	}

	/// <summary>Enlarges this <see cref="T:System.Drawing.Rectangle" /> by the specified amount.</summary>
	/// <param name="width">The amount to inflate this <see cref="T:System.Drawing.Rectangle" /> horizontally. </param>
	/// <param name="height">The amount to inflate this <see cref="T:System.Drawing.Rectangle" /> vertically. </param>
	/// <filterpriority>1</filterpriority>
	public void Inflate(int width, int height)
	{
		X -= width;
		Y -= height;
		Width += 2 * width;
		Height += 2 * height;
	}

	/// <summary>Enlarges this <see cref="T:System.Drawing.Rectangle" /> by the specified amount.</summary>
	/// <param name="size">The amount to inflate this rectangle. </param>
	/// <filterpriority>1</filterpriority>
	public void Inflate(Size size)
	{
		Inflate(size.Width, size.Height);
	}

	/// <summary>Creates and returns an enlarged copy of the specified <see cref="T:System.Drawing.Rectangle" /> structure. The copy is enlarged by the specified amount. The original <see cref="T:System.Drawing.Rectangle" /> structure remains unmodified.</summary>
	/// <returns>The enlarged <see cref="T:System.Drawing.Rectangle" />.</returns>
	/// <param name="rect">The <see cref="T:System.Drawing.Rectangle" /> with which to start. This rectangle is not modified. </param>
	/// <param name="x">The amount to inflate this <see cref="T:System.Drawing.Rectangle" /> horizontally. </param>
	/// <param name="y">The amount to inflate this <see cref="T:System.Drawing.Rectangle" /> vertically. </param>
	/// <filterpriority>1</filterpriority>
	public static Rectangle Inflate(Rectangle rect, int x, int y)
	{
		Rectangle result = rect;
		result.Inflate(x, y);
		return result;
	}

	/// <summary>Replaces this <see cref="T:System.Drawing.Rectangle" /> with the intersection of itself and the specified <see cref="T:System.Drawing.Rectangle" />.</summary>
	/// <param name="rect">The <see cref="T:System.Drawing.Rectangle" /> with which to intersect. </param>
	/// <filterpriority>1</filterpriority>
	public void Intersect(Rectangle rect)
	{
		Rectangle rectangle = Intersect(rect, this);
		X = rectangle.X;
		Y = rectangle.Y;
		Width = rectangle.Width;
		Height = rectangle.Height;
	}

	/// <summary>Returns a third <see cref="T:System.Drawing.Rectangle" /> structure that represents the intersection of two other <see cref="T:System.Drawing.Rectangle" /> structures. If there is no intersection, an empty <see cref="T:System.Drawing.Rectangle" /> is returned.</summary>
	/// <returns>A <see cref="T:System.Drawing.Rectangle" /> that represents the intersection of <paramref name="a" /> and <paramref name="b" />.</returns>
	/// <param name="a">A rectangle to intersect. </param>
	/// <param name="b">A rectangle to intersect. </param>
	/// <filterpriority>1</filterpriority>
	public static Rectangle Intersect(Rectangle a, Rectangle b)
	{
		int num = Math.Max(a.X, b.X);
		int num2 = Math.Min(a.X + a.Width, b.X + b.Width);
		int num3 = Math.Max(a.Y, b.Y);
		int num4 = Math.Min(a.Y + a.Height, b.Y + b.Height);
		if (num2 >= num && num4 >= num3)
		{
			return new Rectangle(num, num3, num2 - num, num4 - num3);
		}
		return Empty;
	}

	/// <summary>Determines if this rectangle intersects with <paramref name="rect" />.</summary>
	/// <returns>This method returns true if there is any intersection, otherwise false.</returns>
	/// <param name="rect">The rectangle to test. </param>
	/// <filterpriority>1</filterpriority>
	public bool IntersectsWith(Rectangle rect)
	{
		if (rect.X < X + Width && X < rect.X + rect.Width && rect.Y < Y + Height)
		{
			return Y < rect.Y + rect.Height;
		}
		return false;
	}

	/// <summary>Gets a <see cref="T:System.Drawing.Rectangle" /> structure that contains the union of two <see cref="T:System.Drawing.Rectangle" /> structures.</summary>
	/// <returns>A <see cref="T:System.Drawing.Rectangle" /> structure that bounds the union of the two <see cref="T:System.Drawing.Rectangle" /> structures.</returns>
	/// <param name="a">A rectangle to union. </param>
	/// <param name="b">A rectangle to union. </param>
	/// <filterpriority>1</filterpriority>
	public static Rectangle Union(Rectangle a, Rectangle b)
	{
		int num = Math.Min(a.X, b.X);
		int num2 = Math.Max(a.X + a.Width, b.X + b.Width);
		int num3 = Math.Min(a.Y, b.Y);
		int num4 = Math.Max(a.Y + a.Height, b.Y + b.Height);
		return new Rectangle(num, num3, num2 - num, num4 - num3);
	}

	/// <summary>Adjusts the location of this rectangle by the specified amount.</summary>
	/// <param name="pos">Amount to offset the location. </param>
	/// <filterpriority>1</filterpriority>
	public void Offset(Point pos)
	{
		Offset(pos.X, pos.Y);
	}

	/// <summary>Adjusts the location of this rectangle by the specified amount.</summary>
	/// <param name="x">The horizontal offset. </param>
	/// <param name="y">The vertical offset. </param>
	/// <filterpriority>1</filterpriority>
	public void Offset(int x, int y)
	{
		X += x;
		Y += y;
	}

	/// <summary>Converts the attributes of this <see cref="T:System.Drawing.Rectangle" /> to a human-readable string.</summary>
	/// <returns>A string that contains the position, width, and height of this <see cref="T:System.Drawing.Rectangle" /> structure ¾ for example, {X=20, Y=20, Width=100, Height=50} </returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override string ToString()
	{
		return "{X=" + X + ",Y=" + Y + ",Width=" + Width + ",Height=" + Height + "}";
	}
}
