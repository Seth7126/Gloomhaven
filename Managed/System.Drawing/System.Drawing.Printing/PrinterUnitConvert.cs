namespace System.Drawing.Printing;

/// <summary>Specifies a series of conversion methods that are useful when interoperating with the Win32 printing API. This class cannot be inherited.</summary>
public sealed class PrinterUnitConvert
{
	private PrinterUnitConvert()
	{
	}

	/// <summary>Converts a double-precision floating-point number from one <see cref="T:System.Drawing.Printing.PrinterUnit" /> type to another <see cref="T:System.Drawing.Printing.PrinterUnit" /> type.</summary>
	/// <returns>A double-precision floating-point number that represents the converted <see cref="T:System.Drawing.Printing.PrinterUnit" />.</returns>
	/// <param name="value">The <see cref="T:System.Drawing.Point" /> being converted. </param>
	/// <param name="fromUnit">The unit to convert from. </param>
	/// <param name="toUnit">The unit to convert to. </param>
	public static double Convert(double value, PrinterUnit fromUnit, PrinterUnit toUnit)
	{
		double num = UnitsPerDisplay(fromUnit);
		double num2 = UnitsPerDisplay(toUnit);
		return value * num2 / num;
	}

	/// <summary>Converts a 32-bit signed integer from one <see cref="T:System.Drawing.Printing.PrinterUnit" /> type to another <see cref="T:System.Drawing.Printing.PrinterUnit" /> type.</summary>
	/// <returns>A 32-bit signed integer that represents the converted <see cref="T:System.Drawing.Printing.PrinterUnit" />.</returns>
	/// <param name="value">The value being converted. </param>
	/// <param name="fromUnit">The unit to convert from. </param>
	/// <param name="toUnit">The unit to convert to. </param>
	public static int Convert(int value, PrinterUnit fromUnit, PrinterUnit toUnit)
	{
		return (int)Math.Round(Convert((double)value, fromUnit, toUnit));
	}

	/// <summary>Converts a <see cref="T:System.Drawing.Point" /> from one <see cref="T:System.Drawing.Printing.PrinterUnit" /> type to another <see cref="T:System.Drawing.Printing.PrinterUnit" /> type.</summary>
	/// <returns>A <see cref="T:System.Drawing.Point" /> that represents the converted <see cref="T:System.Drawing.Printing.PrinterUnit" />.</returns>
	/// <param name="value">The <see cref="T:System.Drawing.Point" /> being converted. </param>
	/// <param name="fromUnit">The unit to convert from. </param>
	/// <param name="toUnit">The unit to convert to. </param>
	public static Point Convert(Point value, PrinterUnit fromUnit, PrinterUnit toUnit)
	{
		return new Point(Convert(value.X, fromUnit, toUnit), Convert(value.Y, fromUnit, toUnit));
	}

	/// <summary>Converts a <see cref="T:System.Drawing.Size" /> from one <see cref="T:System.Drawing.Printing.PrinterUnit" /> type to another <see cref="T:System.Drawing.Printing.PrinterUnit" /> type.</summary>
	/// <returns>A <see cref="T:System.Drawing.Size" /> that represents the converted <see cref="T:System.Drawing.Printing.PrinterUnit" />.</returns>
	/// <param name="value">The <see cref="T:System.Drawing.Size" /> being converted. </param>
	/// <param name="fromUnit">The unit to convert from. </param>
	/// <param name="toUnit">The unit to convert to. </param>
	public static Size Convert(Size value, PrinterUnit fromUnit, PrinterUnit toUnit)
	{
		return new Size(Convert(value.Width, fromUnit, toUnit), Convert(value.Height, fromUnit, toUnit));
	}

	/// <summary>Converts a <see cref="T:System.Drawing.Rectangle" /> from one <see cref="T:System.Drawing.Printing.PrinterUnit" /> type to another <see cref="T:System.Drawing.Printing.PrinterUnit" /> type.</summary>
	/// <returns>A <see cref="T:System.Drawing.Rectangle" /> that represents the converted <see cref="T:System.Drawing.Printing.PrinterUnit" />.</returns>
	/// <param name="value">The <see cref="T:System.Drawing.Rectangle" /> being converted. </param>
	/// <param name="fromUnit">The unit to convert from. </param>
	/// <param name="toUnit">The unit to convert to. </param>
	public static Rectangle Convert(Rectangle value, PrinterUnit fromUnit, PrinterUnit toUnit)
	{
		return new Rectangle(Convert(value.X, fromUnit, toUnit), Convert(value.Y, fromUnit, toUnit), Convert(value.Width, fromUnit, toUnit), Convert(value.Height, fromUnit, toUnit));
	}

	/// <summary>Converts a <see cref="T:System.Drawing.Printing.Margins" /> from one <see cref="T:System.Drawing.Printing.PrinterUnit" /> type to another <see cref="T:System.Drawing.Printing.PrinterUnit" /> type.</summary>
	/// <returns>A <see cref="T:System.Drawing.Printing.Margins" /> that represents the converted <see cref="T:System.Drawing.Printing.PrinterUnit" />.</returns>
	/// <param name="value">The <see cref="T:System.Drawing.Printing.Margins" /> being converted. </param>
	/// <param name="fromUnit">The unit to convert from. </param>
	/// <param name="toUnit">The unit to convert to. </param>
	public static Margins Convert(Margins value, PrinterUnit fromUnit, PrinterUnit toUnit)
	{
		return new Margins
		{
			DoubleLeft = Convert(value.DoubleLeft, fromUnit, toUnit),
			DoubleRight = Convert(value.DoubleRight, fromUnit, toUnit),
			DoubleTop = Convert(value.DoubleTop, fromUnit, toUnit),
			DoubleBottom = Convert(value.DoubleBottom, fromUnit, toUnit)
		};
	}

	private static double UnitsPerDisplay(PrinterUnit unit)
	{
		return unit switch
		{
			PrinterUnit.Display => 1.0, 
			PrinterUnit.ThousandthsOfAnInch => 10.0, 
			PrinterUnit.HundredthsOfAMillimeter => 25.4, 
			PrinterUnit.TenthsOfAMillimeter => 2.54, 
			_ => 1.0, 
		};
	}
}
