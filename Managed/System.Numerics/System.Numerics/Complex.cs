using System.Globalization;

namespace System.Numerics;

/// <summary>Represents a complex number.</summary>
[Serializable]
public struct Complex : IEquatable<Complex>, IFormattable
{
	/// <summary>Returns a new <see cref="T:System.Numerics.Complex" /> instance with a real number equal to zero and an imaginary number equal to zero.</summary>
	public static readonly Complex Zero = new Complex(0.0, 0.0);

	/// <summary>Returns a new <see cref="T:System.Numerics.Complex" /> instance with a real number equal to one and an imaginary number equal to zero.</summary>
	public static readonly Complex One = new Complex(1.0, 0.0);

	/// <summary>Returns a new <see cref="T:System.Numerics.Complex" /> instance with a real number equal to zero and an imaginary number equal to one.</summary>
	public static readonly Complex ImaginaryOne = new Complex(0.0, 1.0);

	private const double InverseOfLog10 = 0.43429448190325;

	private static readonly double s_sqrtRescaleThreshold = double.MaxValue / (Math.Sqrt(2.0) + 1.0);

	private static readonly double s_asinOverflowThreshold = Math.Sqrt(double.MaxValue) / 2.0;

	private static readonly double s_log2 = Math.Log(2.0);

	private double m_real;

	private double m_imaginary;

	/// <summary>Gets the real component of the current <see cref="T:System.Numerics.Complex" /> object.</summary>
	/// <returns>The real component of a complex number.</returns>
	public double Real => m_real;

	/// <summary>Gets the imaginary component of the current <see cref="T:System.Numerics.Complex" /> object.</summary>
	/// <returns>The imaginary component of a complex number.</returns>
	public double Imaginary => m_imaginary;

	/// <summary>Gets the magnitude (or absolute value) of a complex number.</summary>
	/// <returns>The magnitude of the current instance.</returns>
	public double Magnitude => Abs(this);

	/// <summary>Gets the phase of a complex number.</summary>
	/// <returns>The phase of a complex number, in radians.</returns>
	public double Phase => Math.Atan2(m_imaginary, m_real);

	/// <summary>Initializes a new instance of the <see cref="T:System.Numerics.Complex" /> structure using the specified real and imaginary values.</summary>
	/// <param name="real">The real part of the complex number.</param>
	/// <param name="imaginary">The imaginary part of the complex number.</param>
	public Complex(double real, double imaginary)
	{
		m_real = real;
		m_imaginary = imaginary;
	}

	/// <summary>Creates a complex number from a point's polar coordinates.</summary>
	/// <returns>A complex number.</returns>
	/// <param name="magnitude">The magnitude, which is the distance from the origin (the intersection of the x-axis and the y-axis) to the number.</param>
	/// <param name="phase">The phase, which is the angle from the line to the horizontal axis, measured in radians.</param>
	public static Complex FromPolarCoordinates(double magnitude, double phase)
	{
		return new Complex(magnitude * Math.Cos(phase), magnitude * Math.Sin(phase));
	}

	/// <summary>Returns the additive inverse of a specified complex number.</summary>
	/// <returns>The result of the <see cref="P:System.Numerics.Complex.Real" /> and <see cref="P:System.Numerics.Complex.Imaginary" /> components of the <paramref name="value" /> parameter multiplied by -1.</returns>
	/// <param name="value">A complex number.</param>
	public static Complex Negate(Complex value)
	{
		return -value;
	}

	/// <summary>Adds two complex numbers and returns the result.</summary>
	/// <returns>The sum of <paramref name="left" /> and <paramref name="right" />.</returns>
	/// <param name="left">The first complex number to add.</param>
	/// <param name="right">The second complex number to add.</param>
	public static Complex Add(Complex left, Complex right)
	{
		return left + right;
	}

	/// <summary>Subtracts one complex number from another and returns the result.</summary>
	/// <returns>The result of subtracting <paramref name="right" /> from <paramref name="left" />.</returns>
	/// <param name="left">The value to subtract from (the minuend).</param>
	/// <param name="right">The value to subtract (the subtrahend).</param>
	public static Complex Subtract(Complex left, Complex right)
	{
		return left - right;
	}

	/// <summary>Returns the product of two complex numbers.</summary>
	/// <returns>The product of the <paramref name="left" /> and <paramref name="right" /> parameters.</returns>
	/// <param name="left">The first complex number to multiply.</param>
	/// <param name="right">The second complex number to multiply.</param>
	public static Complex Multiply(Complex left, Complex right)
	{
		return left * right;
	}

	/// <summary>Divides one complex number by another and returns the result.</summary>
	/// <returns>The quotient of the division.</returns>
	/// <param name="dividend">The complex number to be divided.</param>
	/// <param name="divisor">The complex number to divide by.</param>
	public static Complex Divide(Complex dividend, Complex divisor)
	{
		return dividend / divisor;
	}

	/// <summary>Returns the additive inverse of a specified complex number.</summary>
	/// <returns>The result of the <see cref="P:System.Numerics.Complex.Real" /> and <see cref="P:System.Numerics.Complex.Imaginary" /> components of the <paramref name="value" /> parameter multiplied by -1.</returns>
	/// <param name="value">The value to negate.</param>
	public static Complex operator -(Complex value)
	{
		return new Complex(0.0 - value.m_real, 0.0 - value.m_imaginary);
	}

	/// <summary>Adds two complex numbers.</summary>
	/// <returns>The sum of <paramref name="left" /> and <paramref name="right" />.</returns>
	/// <param name="left">The first value to add.</param>
	/// <param name="right">The second value to add.</param>
	public static Complex operator +(Complex left, Complex right)
	{
		return new Complex(left.m_real + right.m_real, left.m_imaginary + right.m_imaginary);
	}

	/// <summary>Subtracts a complex number from another complex number.</summary>
	/// <returns>The result of subtracting <paramref name="right" /> from <paramref name="left" />.</returns>
	/// <param name="left">The value to subtract from (the minuend).</param>
	/// <param name="right">The value to subtract (the subtrahend).</param>
	public static Complex operator -(Complex left, Complex right)
	{
		return new Complex(left.m_real - right.m_real, left.m_imaginary - right.m_imaginary);
	}

	/// <summary>Multiplies two specified complex numbers.</summary>
	/// <returns>The product of <paramref name="left" /> and <paramref name="right" />.</returns>
	/// <param name="left">The first value to multiply.</param>
	/// <param name="right">The second value to multiply.</param>
	public static Complex operator *(Complex left, Complex right)
	{
		double real = left.m_real * right.m_real - left.m_imaginary * right.m_imaginary;
		double imaginary = left.m_imaginary * right.m_real + left.m_real * right.m_imaginary;
		return new Complex(real, imaginary);
	}

	/// <summary>Divides a specified complex number by another specified complex number.</summary>
	/// <returns>The result of dividing <paramref name="left" /> by <paramref name="right" />.</returns>
	/// <param name="left">The value to be divided.</param>
	/// <param name="right">The value to divide by.</param>
	public static Complex operator /(Complex left, Complex right)
	{
		double real = left.m_real;
		double imaginary = left.m_imaginary;
		double real2 = right.m_real;
		double imaginary2 = right.m_imaginary;
		if (Math.Abs(imaginary2) < Math.Abs(real2))
		{
			double num = imaginary2 / real2;
			return new Complex((real + imaginary * num) / (real2 + imaginary2 * num), (imaginary - real * num) / (real2 + imaginary2 * num));
		}
		double num2 = real2 / imaginary2;
		return new Complex((imaginary + real * num2) / (imaginary2 + real2 * num2), (0.0 - real + imaginary * num2) / (imaginary2 + real2 * num2));
	}

	/// <summary>Gets the absolute value (or magnitude) of a complex number.</summary>
	/// <returns>The absolute value of <paramref name="value" />.</returns>
	/// <param name="value">A complex number.</param>
	public static double Abs(Complex value)
	{
		return Hypot(value.m_real, value.m_imaginary);
	}

	private static double Hypot(double a, double b)
	{
		a = Math.Abs(a);
		b = Math.Abs(b);
		double num;
		double num2;
		if (a < b)
		{
			num = a;
			num2 = b;
		}
		else
		{
			num = b;
			num2 = a;
		}
		if (num == 0.0)
		{
			return num2;
		}
		if (double.IsPositiveInfinity(num2) && !double.IsNaN(num))
		{
			return double.PositiveInfinity;
		}
		double num3 = num / num2;
		return num2 * Math.Sqrt(1.0 + num3 * num3);
	}

	private static double Log1P(double x)
	{
		double num = 1.0 + x;
		if (num == 1.0)
		{
			return x;
		}
		if (x < 0.75)
		{
			return x * Math.Log(num) / (num - 1.0);
		}
		return Math.Log(num);
	}

	/// <summary>Computes the conjugate of a complex number and returns the result.</summary>
	/// <returns>The conjugate of <paramref name="value" />.</returns>
	/// <param name="value">A complex number.</param>
	public static Complex Conjugate(Complex value)
	{
		return new Complex(value.m_real, 0.0 - value.m_imaginary);
	}

	/// <summary>Returns the multiplicative inverse of a complex number.</summary>
	/// <returns>The reciprocal of <paramref name="value" />.</returns>
	/// <param name="value">A complex number.</param>
	public static Complex Reciprocal(Complex value)
	{
		if (value.m_real == 0.0 && value.m_imaginary == 0.0)
		{
			return Zero;
		}
		return One / value;
	}

	/// <summary>Returns a value that indicates whether two complex numbers are equal.</summary>
	/// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
	/// <param name="left">The first complex number to compare.</param>
	/// <param name="right">The second complex number to compare.</param>
	public static bool operator ==(Complex left, Complex right)
	{
		if (left.m_real == right.m_real)
		{
			return left.m_imaginary == right.m_imaginary;
		}
		return false;
	}

	/// <summary>Returns a value that indicates whether two complex numbers are not equal.</summary>
	/// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
	/// <param name="left">The first value to compare.</param>
	/// <param name="right">The second value to compare.</param>
	public static bool operator !=(Complex left, Complex right)
	{
		if (left.m_real == right.m_real)
		{
			return left.m_imaginary != right.m_imaginary;
		}
		return true;
	}

	/// <summary>Returns a value that indicates whether the current instance and a specified object have the same value. </summary>
	/// <returns>true if the <paramref name="obj" /> parameter is a <see cref="T:System.Numerics.Complex" /> object or a type capable of implicit conversion to a <see cref="T:System.Numerics.Complex" /> object, and its value is equal to the current <see cref="T:System.Numerics.Complex" /> object; otherwise, false.</returns>
	/// <param name="obj">The object to compare.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is Complex))
		{
			return false;
		}
		return Equals((Complex)obj);
	}

	/// <summary>Returns a value that indicates whether the current instance and a specified complex number have the same value.</summary>
	/// <returns>true if this complex number and <paramref name="value" /> have the same value; otherwise, false.</returns>
	/// <param name="value">The complex number to compare.</param>
	public bool Equals(Complex value)
	{
		if (m_real.Equals(value.m_real))
		{
			return m_imaginary.Equals(value.m_imaginary);
		}
		return false;
	}

	/// <summary>Returns the hash code for the current <see cref="T:System.Numerics.Complex" /> object.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		int num = 99999997;
		int num2 = m_real.GetHashCode() % num;
		int hashCode = m_imaginary.GetHashCode();
		return num2 ^ hashCode;
	}

	/// <summary>Converts the value of the current complex number to its equivalent string representation in Cartesian form.</summary>
	/// <returns>The string representation of the current instance in Cartesian form.</returns>
	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "({0}, {1})", m_real, m_imaginary);
	}

	/// <summary>Converts the value of the current complex number to its equivalent string representation in Cartesian form by using the specified format for its real and imaginary parts.</summary>
	/// <returns>The string representation of the current instance in Cartesian form.</returns>
	/// <param name="format">A standard or custom numeric format string.</param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is not a valid format string.</exception>
	public string ToString(string format)
	{
		return string.Format(CultureInfo.CurrentCulture, "({0}, {1})", m_real.ToString(format, CultureInfo.CurrentCulture), m_imaginary.ToString(format, CultureInfo.CurrentCulture));
	}

	/// <summary>Converts the value of the current complex number to its equivalent string representation in Cartesian form by using the specified culture-specific formatting information.</summary>
	/// <returns>The string representation of the current instance in Cartesian form, as specified by <paramref name="provider" />.</returns>
	/// <param name="provider">An object that supplies culture-specific formatting information.</param>
	public string ToString(IFormatProvider provider)
	{
		return string.Format(provider, "({0}, {1})", m_real, m_imaginary);
	}

	/// <summary>Converts the value of the current complex number to its equivalent string representation in Cartesian form by using the specified format and culture-specific format information for its real and imaginary parts.</summary>
	/// <returns>The string representation of the current instance in Cartesian form, as specified by <paramref name="format" /> and <paramref name="provider" />.</returns>
	/// <param name="format">A standard or custom numeric format string.</param>
	/// <param name="provider">An object that supplies culture-specific formatting information.</param>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is not a valid format string.</exception>
	public string ToString(string format, IFormatProvider provider)
	{
		return string.Format(provider, "({0}, {1})", m_real.ToString(format, provider), m_imaginary.ToString(format, provider));
	}

	/// <summary>Returns the sine of the specified complex number.</summary>
	/// <returns>The sine of <paramref name="value" />.</returns>
	/// <param name="value">A complex number.</param>
	public static Complex Sin(Complex value)
	{
		double num = Math.Exp(value.m_imaginary);
		double num2 = 1.0 / num;
		double num3 = (num - num2) * 0.5;
		double num4 = (num + num2) * 0.5;
		return new Complex(Math.Sin(value.m_real) * num4, Math.Cos(value.m_real) * num3);
	}

	/// <summary>Returns the hyperbolic sine of the specified complex number.</summary>
	/// <returns>The hyperbolic sine of <paramref name="value" />.</returns>
	/// <param name="value">A complex number.</param>
	public static Complex Sinh(Complex value)
	{
		Complex complex = Sin(new Complex(0.0 - value.m_imaginary, value.m_real));
		return new Complex(complex.m_imaginary, 0.0 - complex.m_real);
	}

	/// <summary>Returns the angle that is the arc sine of the specified complex number.</summary>
	/// <returns>The angle which is the arc sine of <paramref name="value" />.</returns>
	/// <param name="value">A complex number.</param>
	public static Complex Asin(Complex value)
	{
		Asin_Internal(Math.Abs(value.Real), Math.Abs(value.Imaginary), out var b, out var bPrime, out var v);
		double num = ((!(bPrime < 0.0)) ? Math.Atan(bPrime) : Math.Asin(b));
		if (value.Real < 0.0)
		{
			num = 0.0 - num;
		}
		if (value.Imaginary < 0.0)
		{
			v = 0.0 - v;
		}
		return new Complex(num, v);
	}

	/// <summary>Returns the cosine of the specified complex number.</summary>
	/// <returns>The cosine of <paramref name="value" />.</returns>
	/// <param name="value">A complex number.</param>
	public static Complex Cos(Complex value)
	{
		double num = Math.Exp(value.m_imaginary);
		double num2 = 1.0 / num;
		double num3 = (num - num2) * 0.5;
		double num4 = (num + num2) * 0.5;
		return new Complex(Math.Cos(value.m_real) * num4, (0.0 - Math.Sin(value.m_real)) * num3);
	}

	/// <summary>Returns the hyperbolic cosine of the specified complex number.</summary>
	/// <returns>The hyperbolic cosine of <paramref name="value" />.</returns>
	/// <param name="value">A complex number.</param>
	public static Complex Cosh(Complex value)
	{
		return Cos(new Complex(0.0 - value.m_imaginary, value.m_real));
	}

	/// <summary>Returns the angle that is the arc cosine of the specified complex number.</summary>
	/// <returns>The angle, measured in radians, which is the arc cosine of <paramref name="value" />.</returns>
	/// <param name="value">A complex number that represents a cosine.</param>
	public static Complex Acos(Complex value)
	{
		Asin_Internal(Math.Abs(value.Real), Math.Abs(value.Imaginary), out var b, out var bPrime, out var v);
		double num = ((!(bPrime < 0.0)) ? Math.Atan(1.0 / bPrime) : Math.Acos(b));
		if (value.Real < 0.0)
		{
			num = Math.PI - num;
		}
		if (value.Imaginary > 0.0)
		{
			v = 0.0 - v;
		}
		return new Complex(num, v);
	}

	/// <summary>Returns the tangent of the specified complex number.</summary>
	/// <returns>The tangent of <paramref name="value" />.</returns>
	/// <param name="value">A complex number.</param>
	public static Complex Tan(Complex value)
	{
		double num = 2.0 * value.m_real;
		double num2 = 2.0 * value.m_imaginary;
		double num3 = Math.Exp(num2);
		double num4 = 1.0 / num3;
		double num5 = (num3 + num4) * 0.5;
		if (Math.Abs(value.m_imaginary) <= 4.0)
		{
			double num6 = (num3 - num4) * 0.5;
			double num7 = Math.Cos(num) + num5;
			return new Complex(Math.Sin(num) / num7, num6 / num7);
		}
		double num8 = 1.0 + Math.Cos(num) / num5;
		return new Complex(Math.Sin(num) / num5 / num8, Math.Tanh(num2) / num8);
	}

	/// <summary>Returns the hyperbolic tangent of the specified complex number.</summary>
	/// <returns>The hyperbolic tangent of <paramref name="value" />.</returns>
	/// <param name="value">A complex number.</param>
	public static Complex Tanh(Complex value)
	{
		Complex complex = Tan(new Complex(0.0 - value.m_imaginary, value.m_real));
		return new Complex(complex.m_imaginary, 0.0 - complex.m_real);
	}

	/// <summary>Returns the angle that is the arc tangent of the specified complex number.</summary>
	/// <returns>The angle that is the arc tangent of <paramref name="value" />.</returns>
	/// <param name="value">A complex number.</param>
	public static Complex Atan(Complex value)
	{
		Complex complex = new Complex(2.0, 0.0);
		return ImaginaryOne / complex * (Log(One - ImaginaryOne * value) - Log(One + ImaginaryOne * value));
	}

	private static void Asin_Internal(double x, double y, out double b, out double bPrime, out double v)
	{
		if (x > s_asinOverflowThreshold || y > s_asinOverflowThreshold)
		{
			b = -1.0;
			bPrime = x / y;
			double num;
			double num2;
			if (x < y)
			{
				num = x;
				num2 = y;
			}
			else
			{
				num = y;
				num2 = x;
			}
			double num3 = num / num2;
			v = s_log2 + Math.Log(num2) + 0.5 * Log1P(num3 * num3);
			return;
		}
		double num4 = Hypot(x + 1.0, y);
		double num5 = Hypot(x - 1.0, y);
		double num6 = (num4 + num5) * 0.5;
		b = x / num6;
		if (b > 0.75)
		{
			if (x <= 1.0)
			{
				double num7 = (y * y / (num4 + (x + 1.0)) + (num5 + (1.0 - x))) * 0.5;
				bPrime = x / Math.Sqrt((num6 + x) * num7);
			}
			else
			{
				double num8 = (1.0 / (num4 + (x + 1.0)) + 1.0 / (num5 + (x - 1.0))) * 0.5;
				bPrime = x / y / Math.Sqrt((num6 + x) * num8);
			}
		}
		else
		{
			bPrime = -1.0;
		}
		if (num6 < 1.5)
		{
			if (x < 1.0)
			{
				double num9 = (1.0 / (num4 + (x + 1.0)) + 1.0 / (num5 + (1.0 - x))) * 0.5;
				double num10 = y * y * num9;
				v = Log1P(num10 + y * Math.Sqrt(num9 * (num6 + 1.0)));
			}
			else
			{
				double num11 = (y * y / (num4 + (x + 1.0)) + (num5 + (x - 1.0))) * 0.5;
				v = Log1P(num11 + Math.Sqrt(num11 * (num6 + 1.0)));
			}
		}
		else
		{
			v = Math.Log(num6 + Math.Sqrt((num6 - 1.0) * (num6 + 1.0)));
		}
	}

	/// <summary>Returns the natural (base e) logarithm of a specified complex number.</summary>
	/// <returns>The natural (base e) logarithm of <paramref name="value" />.</returns>
	/// <param name="value">A complex number.</param>
	public static Complex Log(Complex value)
	{
		return new Complex(Math.Log(Abs(value)), Math.Atan2(value.m_imaginary, value.m_real));
	}

	/// <summary>Returns the logarithm of a specified complex number in a specified base.</summary>
	/// <returns>The logarithm of <paramref name="value" /> in base <paramref name="baseValue" />.</returns>
	/// <param name="value">A complex number.</param>
	/// <param name="baseValue">The base of the logarithm.</param>
	public static Complex Log(Complex value, double baseValue)
	{
		return Log(value) / Log(baseValue);
	}

	/// <summary>Returns the base-10 logarithm of a specified complex number.</summary>
	/// <returns>The base-10 logarithm of <paramref name="value" />.</returns>
	/// <param name="value">A complex number.</param>
	public static Complex Log10(Complex value)
	{
		return Scale(Log(value), 0.43429448190325);
	}

	/// <summary>Returns e raised to the power specified by a complex number.</summary>
	/// <returns>The number e raised to the power <paramref name="value" />.</returns>
	/// <param name="value">A complex number that specifies a power.</param>
	public static Complex Exp(Complex value)
	{
		double num = Math.Exp(value.m_real);
		double real = num * Math.Cos(value.m_imaginary);
		double imaginary = num * Math.Sin(value.m_imaginary);
		return new Complex(real, imaginary);
	}

	/// <summary>Returns the square root of a specified complex number.</summary>
	/// <returns>The square root of <paramref name="value" />.</returns>
	/// <param name="value">A complex number.</param>
	public static Complex Sqrt(Complex value)
	{
		if (value.m_imaginary == 0.0)
		{
			if (value.m_real < 0.0)
			{
				return new Complex(0.0, Math.Sqrt(0.0 - value.m_real));
			}
			return new Complex(Math.Sqrt(value.m_real), 0.0);
		}
		bool flag = false;
		if (Math.Abs(value.m_real) >= s_sqrtRescaleThreshold || Math.Abs(value.m_imaginary) >= s_sqrtRescaleThreshold)
		{
			if (double.IsInfinity(value.m_imaginary) && !double.IsNaN(value.m_real))
			{
				return new Complex(double.PositiveInfinity, value.m_imaginary);
			}
			value.m_real *= 0.25;
			value.m_imaginary *= 0.25;
			flag = true;
		}
		double num;
		double num2;
		if (value.m_real >= 0.0)
		{
			num = Math.Sqrt((Hypot(value.m_real, value.m_imaginary) + value.m_real) * 0.5);
			num2 = value.m_imaginary / (2.0 * num);
		}
		else
		{
			num2 = Math.Sqrt((Hypot(value.m_real, value.m_imaginary) - value.m_real) * 0.5);
			if (value.m_imaginary < 0.0)
			{
				num2 = 0.0 - num2;
			}
			num = value.m_imaginary / (2.0 * num2);
		}
		if (flag)
		{
			num *= 2.0;
			num2 *= 2.0;
		}
		return new Complex(num, num2);
	}

	/// <summary>Returns a specified complex number raised to a power specified by a complex number.</summary>
	/// <returns>The complex number <paramref name="value" /> raised to the power <paramref name="power" />.</returns>
	/// <param name="value">A complex number to be raised to a power.</param>
	/// <param name="power">A complex number that specifies a power.</param>
	public static Complex Pow(Complex value, Complex power)
	{
		if (power == Zero)
		{
			return One;
		}
		if (value == Zero)
		{
			return Zero;
		}
		double real = value.m_real;
		double imaginary = value.m_imaginary;
		double real2 = power.m_real;
		double imaginary2 = power.m_imaginary;
		double num = Abs(value);
		double num2 = Math.Atan2(imaginary, real);
		double num3 = real2 * num2 + imaginary2 * Math.Log(num);
		double num4 = Math.Pow(num, real2) * Math.Pow(Math.E, (0.0 - imaginary2) * num2);
		return new Complex(num4 * Math.Cos(num3), num4 * Math.Sin(num3));
	}

	/// <summary>Returns a specified complex number raised to a power specified by a double-precision floating-point number.</summary>
	/// <returns>The complex number <paramref name="value" /> raised to the power <paramref name="power" />.</returns>
	/// <param name="value">A complex number to be raised to a power.</param>
	/// <param name="power">A double-precision floating-point number that specifies a power.</param>
	public static Complex Pow(Complex value, double power)
	{
		return Pow(value, new Complex(power, 0.0));
	}

	private static Complex Scale(Complex value, double factor)
	{
		double real = factor * value.m_real;
		double imaginary = factor * value.m_imaginary;
		return new Complex(real, imaginary);
	}

	/// <summary>Defines an implicit conversion of a 16-bit signed integer to a complex number.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter as its real part and zero as its imaginary part.</returns>
	/// <param name="value">The value to convert to a complex number.</param>
	public static implicit operator Complex(short value)
	{
		return new Complex(value, 0.0);
	}

	/// <summary>Defines an implicit conversion of a 32-bit signed integer to a complex number.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter as its real part and zero as its imaginary part.</returns>
	/// <param name="value">The value to convert to a complex number.</param>
	public static implicit operator Complex(int value)
	{
		return new Complex(value, 0.0);
	}

	/// <summary>Defines an implicit conversion of a 64-bit signed integer to a complex number.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter as its real part and zero as its imaginary part.</returns>
	/// <param name="value">The value to convert to a complex number.</param>
	public static implicit operator Complex(long value)
	{
		return new Complex(value, 0.0);
	}

	/// <summary>Defines an implicit conversion of a 16-bit unsigned integer to a complex number.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter as its real part and zero as its imaginary part.</returns>
	/// <param name="value">The value to convert to a complex number.</param>
	[CLSCompliant(false)]
	public static implicit operator Complex(ushort value)
	{
		return new Complex((int)value, 0.0);
	}

	/// <summary>Defines an implicit conversion of a 32-bit unsigned integer to a complex number.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter as its real part and zero as its imaginary part.</returns>
	/// <param name="value">The value to convert to a complex number.</param>
	[CLSCompliant(false)]
	public static implicit operator Complex(uint value)
	{
		return new Complex(value, 0.0);
	}

	/// <summary>Defines an implicit conversion of a 64-bit unsigned integer to a complex number.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter as its real part and zero as its imaginary part.</returns>
	/// <param name="value">The value to convert to a complex number.</param>
	[CLSCompliant(false)]
	public static implicit operator Complex(ulong value)
	{
		return new Complex(value, 0.0);
	}

	/// <summary>Defines an implicit conversion of a signed byte to a complex number.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter as its real part and zero as its imaginary part.</returns>
	/// <param name="value">The value to convert to a complex number.</param>
	[CLSCompliant(false)]
	public static implicit operator Complex(sbyte value)
	{
		return new Complex(value, 0.0);
	}

	/// <summary>Defines an implicit conversion of an unsigned byte to a complex number.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter as its real part and zero as its imaginary part.</returns>
	/// <param name="value">The value to convert to a complex number.</param>
	public static implicit operator Complex(byte value)
	{
		return new Complex((int)value, 0.0);
	}

	/// <summary>Defines an implicit conversion of a single-precision floating-point number to a complex number.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter as its real part and zero as its imaginary part.</returns>
	/// <param name="value">The value to convert to a complex number.</param>
	public static implicit operator Complex(float value)
	{
		return new Complex(value, 0.0);
	}

	/// <summary>Defines an implicit conversion of a double-precision floating-point number to a complex number.</summary>
	/// <returns>An object that contains the value of the <paramref name="value" /> parameter as its real part and zero as its imaginary part.</returns>
	/// <param name="value">The value to convert to a complex number.</param>
	public static implicit operator Complex(double value)
	{
		return new Complex(value, 0.0);
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Numerics.BigInteger" /> value to a complex number. </summary>
	/// <returns>A complex number that has a real component equal to <paramref name="value" /> and an imaginary component equal to zero. </returns>
	/// <param name="value">The value to convert to a complex number.</param>
	public static explicit operator Complex(BigInteger value)
	{
		return new Complex((double)value, 0.0);
	}

	/// <summary>Defines an explicit conversion of a <see cref="T:System.Decimal" /> value to a complex number.</summary>
	/// <returns>A complex number that has a real component equal to <paramref name="value" /> and an imaginary component equal to zero. </returns>
	/// <param name="value">The value to convert to a complex number.</param>
	public static explicit operator Complex(decimal value)
	{
		return new Complex((double)value, 0.0);
	}
}
