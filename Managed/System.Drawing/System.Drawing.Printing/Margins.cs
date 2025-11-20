using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Drawing.Printing;

/// <summary>Specifies the dimensions of the margins of a printed page.</summary>
[Serializable]
[TypeConverter(typeof(MarginsConverter))]
public class Margins : ICloneable
{
	private int _left;

	private int _right;

	private int _bottom;

	private int _top;

	[OptionalField]
	private double _doubleLeft;

	[OptionalField]
	private double _doubleRight;

	[OptionalField]
	private double _doubleTop;

	[OptionalField]
	private double _doubleBottom;

	/// <summary>Gets or sets the left margin width, in hundredths of an inch.</summary>
	/// <returns>The left margin width, in hundredths of an inch.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Drawing.Printing.Margins.Left" /> property is set to a value that is less than 0. </exception>
	public int Left
	{
		get
		{
			return _left;
		}
		set
		{
			CheckMargin(value, "Left");
			_left = value;
			_doubleLeft = value;
		}
	}

	/// <summary>Gets or sets the right margin width, in hundredths of an inch.</summary>
	/// <returns>The right margin width, in hundredths of an inch.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Drawing.Printing.Margins.Right" /> property is set to a value that is less than 0. </exception>
	public int Right
	{
		get
		{
			return _right;
		}
		set
		{
			CheckMargin(value, "Right");
			_right = value;
			_doubleRight = value;
		}
	}

	/// <summary>Gets or sets the top margin width, in hundredths of an inch.</summary>
	/// <returns>The top margin width, in hundredths of an inch.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Drawing.Printing.Margins.Top" /> property is set to a value that is less than 0. </exception>
	public int Top
	{
		get
		{
			return _top;
		}
		set
		{
			CheckMargin(value, "Top");
			_top = value;
			_doubleTop = value;
		}
	}

	/// <summary>Gets or sets the bottom margin, in hundredths of an inch.</summary>
	/// <returns>The bottom margin, in hundredths of an inch.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Drawing.Printing.Margins.Bottom" /> property is set to a value that is less than 0. </exception>
	public int Bottom
	{
		get
		{
			return _bottom;
		}
		set
		{
			CheckMargin(value, "Bottom");
			_bottom = value;
			_doubleBottom = value;
		}
	}

	internal double DoubleLeft
	{
		get
		{
			return _doubleLeft;
		}
		set
		{
			Left = (int)Math.Round(value);
			_doubleLeft = value;
		}
	}

	internal double DoubleRight
	{
		get
		{
			return _doubleRight;
		}
		set
		{
			Right = (int)Math.Round(value);
			_doubleRight = value;
		}
	}

	internal double DoubleTop
	{
		get
		{
			return _doubleTop;
		}
		set
		{
			Top = (int)Math.Round(value);
			_doubleTop = value;
		}
	}

	internal double DoubleBottom
	{
		get
		{
			return _doubleBottom;
		}
		set
		{
			Bottom = (int)Math.Round(value);
			_doubleBottom = value;
		}
	}

	[OnDeserialized]
	private void OnDeserializedMethod(StreamingContext context)
	{
		if (_doubleLeft == 0.0 && _left != 0)
		{
			_doubleLeft = _left;
		}
		if (_doubleRight == 0.0 && _right != 0)
		{
			_doubleRight = _right;
		}
		if (_doubleTop == 0.0 && _top != 0)
		{
			_doubleTop = _top;
		}
		if (_doubleBottom == 0.0 && _bottom != 0)
		{
			_doubleBottom = _bottom;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Printing.Margins" /> class with 1-inch wide margins.</summary>
	public Margins()
		: this(100, 100, 100, 100)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Printing.Margins" /> class with the specified left, right, top, and bottom margins.</summary>
	/// <param name="left">The left margin, in hundredths of an inch. </param>
	/// <param name="right">The right margin, in hundredths of an inch. </param>
	/// <param name="top">The top margin, in hundredths of an inch. </param>
	/// <param name="bottom">The bottom margin, in hundredths of an inch. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="left" /> parameter value is less than 0.-or- The <paramref name="right" /> parameter value is less than 0.-or- The <paramref name="top" /> parameter value is less than 0.-or- The <paramref name="bottom" /> parameter value is less than 0. </exception>
	public Margins(int left, int right, int top, int bottom)
	{
		CheckMargin(left, "left");
		CheckMargin(right, "right");
		CheckMargin(top, "top");
		CheckMargin(bottom, "bottom");
		_left = left;
		_right = right;
		_top = top;
		_bottom = bottom;
		_doubleLeft = left;
		_doubleRight = right;
		_doubleTop = top;
		_doubleBottom = bottom;
	}

	private void CheckMargin(int margin, string name)
	{
		if (margin < 0)
		{
			throw new ArgumentException(global::SR.Format("Value of '{1}' is not valid for '{0}'. '{0}' must be greater than or equal to {2}.", name, margin, "0"));
		}
	}

	/// <summary>Retrieves a duplicate of this object, member by member.</summary>
	/// <returns>A duplicate of this object.</returns>
	public object Clone()
	{
		return MemberwiseClone();
	}

	/// <summary>Compares this <see cref="T:System.Drawing.Printing.Margins" /> to the specified <see cref="T:System.Object" /> to determine whether they have the same dimensions.</summary>
	/// <returns>true if the specified object is a <see cref="T:System.Drawing.Printing.Margins" /> and has the same <see cref="P:System.Drawing.Printing.Margins.Top" />, <see cref="P:System.Drawing.Printing.Margins.Bottom" />, <see cref="P:System.Drawing.Printing.Margins.Right" /> and <see cref="P:System.Drawing.Printing.Margins.Left" /> values as this <see cref="T:System.Drawing.Printing.Margins" />; otherwise, false.</returns>
	/// <param name="obj">The object to which to compare this <see cref="T:System.Drawing.Printing.Margins" />. </param>
	public override bool Equals(object obj)
	{
		Margins margins = obj as Margins;
		if (margins == this)
		{
			return true;
		}
		if (margins == null)
		{
			return false;
		}
		if (margins.Left == Left && margins.Right == Right && margins.Top == Top)
		{
			return margins.Bottom == Bottom;
		}
		return false;
	}

	/// <summary>Calculates and retrieves a hash code based on the width of the left, right, top, and bottom margins.</summary>
	/// <returns>A hash code based on the left, right, top, and bottom margins.</returns>
	public override int GetHashCode()
	{
		int left = Left;
		uint right = (uint)Right;
		uint top = (uint)Top;
		uint bottom = (uint)Bottom;
		return (int)((uint)left ^ ((right << 13) | (right >> 19)) ^ ((top << 26) | (top >> 6)) ^ ((bottom << 7) | (bottom >> 25)));
	}

	/// <summary>Compares two <see cref="T:System.Drawing.Printing.Margins" /> to determine if they have the same dimensions.</summary>
	/// <returns>true to indicate the <see cref="P:System.Drawing.Printing.Margins.Left" />, <see cref="P:System.Drawing.Printing.Margins.Right" />, <see cref="P:System.Drawing.Printing.Margins.Top" />, and <see cref="P:System.Drawing.Printing.Margins.Bottom" /> properties of both margins have the same value; otherwise, false.</returns>
	/// <param name="m1">The first <see cref="T:System.Drawing.Printing.Margins" /> to compare for equality.</param>
	/// <param name="m2">The second <see cref="T:System.Drawing.Printing.Margins" /> to compare for equality.</param>
	public static bool operator ==(Margins m1, Margins m2)
	{
		if ((object)m1 == null != ((object)m2 == null))
		{
			return false;
		}
		if ((object)m1 != null)
		{
			if (m1.Left == m2.Left && m1.Top == m2.Top && m1.Right == m2.Right)
			{
				return m1.Bottom == m2.Bottom;
			}
			return false;
		}
		return true;
	}

	/// <summary>Compares two <see cref="T:System.Drawing.Printing.Margins" /> to determine whether they are of unequal width.</summary>
	/// <returns>true to indicate if the <see cref="P:System.Drawing.Printing.Margins.Left" />, <see cref="P:System.Drawing.Printing.Margins.Right" />, <see cref="P:System.Drawing.Printing.Margins.Top" />, or <see cref="P:System.Drawing.Printing.Margins.Bottom" /> properties of both margins are not equal; otherwise, false.</returns>
	/// <param name="m1">The first <see cref="T:System.Drawing.Printing.Margins" /> to compare for inequality.</param>
	/// <param name="m2">The second <see cref="T:System.Drawing.Printing.Margins" /> to compare for inequality.</param>
	public static bool operator !=(Margins m1, Margins m2)
	{
		return !(m1 == m2);
	}

	/// <summary>Converts the <see cref="T:System.Drawing.Printing.Margins" /> to a string.</summary>
	/// <returns>A <see cref="T:System.String" /> representation of the <see cref="T:System.Drawing.Printing.Margins" />. </returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public override string ToString()
	{
		return "[Margins Left=" + Left.ToString(CultureInfo.InvariantCulture) + " Right=" + Right.ToString(CultureInfo.InvariantCulture) + " Top=" + Top.ToString(CultureInfo.InvariantCulture) + " Bottom=" + Bottom.ToString(CultureInfo.InvariantCulture) + "]";
	}
}
