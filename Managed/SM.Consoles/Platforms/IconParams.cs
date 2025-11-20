namespace Platforms;

public class IconParams
{
	public string IconPath;

	public byte[] RawPNG;

	public IconParams(string iconPath)
	{
		IconPath = iconPath;
	}

	public IconParams(byte[] rawPNG)
	{
		RawPNG = rawPNG;
	}
}
