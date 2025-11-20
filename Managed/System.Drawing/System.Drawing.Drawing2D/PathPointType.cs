namespace System.Drawing.Drawing2D;

/// <summary>Specifies the type of point in a <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> object.</summary>
public enum PathPointType
{
	/// <summary>The starting point of a <see cref="T:System.Drawing.Drawing2D.GraphicsPath" /> object.</summary>
	Start = 0,
	/// <summary>A line segment.</summary>
	Line = 1,
	/// <summary>A default Bézier curve.</summary>
	Bezier = 3,
	/// <summary>A mask point.</summary>
	PathTypeMask = 7,
	/// <summary>The corresponding segment is dashed.</summary>
	DashMode = 16,
	/// <summary>A path marker.</summary>
	PathMarker = 32,
	/// <summary>The endpoint of a subpath.</summary>
	CloseSubpath = 128,
	/// <summary>A cubic Bézier curve.</summary>
	Bezier3 = 3
}
