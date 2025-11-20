namespace System.Drawing;

internal struct CGRect64
{
	public CGPoint64 origin;

	public CGSize64 size;

	public CGRect64(double x, double y, double width, double height)
	{
		origin.x = x;
		origin.y = y;
		size.width = width;
		size.height = height;
	}
}
