namespace System.Drawing;

internal struct CGRect32
{
	public CGPoint32 origin;

	public CGSize32 size;

	public CGRect32(float x, float y, float width, float height)
	{
		origin.x = x;
		origin.y = y;
		size.width = width;
		size.height = height;
	}
}
