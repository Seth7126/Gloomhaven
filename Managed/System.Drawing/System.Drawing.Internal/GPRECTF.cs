namespace System.Drawing.Internal;

internal struct GPRECTF
{
	internal float X;

	internal float Y;

	internal float Width;

	internal float Height;

	internal SizeF SizeF => new SizeF(Width, Height);

	internal GPRECTF(float x, float y, float width, float height)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
	}

	internal GPRECTF(RectangleF rect)
	{
		X = rect.X;
		Y = rect.Y;
		Width = rect.Width;
		Height = rect.Height;
	}

	internal RectangleF ToRectangleF()
	{
		return new RectangleF(X, Y, Width, Height);
	}
}
