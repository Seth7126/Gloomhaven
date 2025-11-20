namespace System.Drawing;

/// <summary>Specifies the units of measure for a text string.</summary>
/// <filterpriority>2</filterpriority>
public enum StringUnit
{
	/// <summary>Specifies world units as the unit of measure.</summary>
	World = 0,
	/// <summary>Specifies the device unit as the unit of measure.</summary>
	Display = 1,
	/// <summary>Specifies a pixel as the unit of measure.</summary>
	Pixel = 2,
	/// <summary>Specifies a printer's point (1/72 inch) as the unit of measure.</summary>
	Point = 3,
	/// <summary>Specifies an inch as the unit of measure.</summary>
	Inch = 4,
	/// <summary>Specifies 1/300 of an inch as the unit of measure.</summary>
	Document = 5,
	/// <summary>Specifies a millimeter as the unit of measure </summary>
	Millimeter = 6,
	/// <summary>Specifies a printer's em size of 32 as the unit of measure.</summary>
	Em = 32
}
