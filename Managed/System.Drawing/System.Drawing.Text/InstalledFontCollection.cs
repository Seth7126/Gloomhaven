namespace System.Drawing.Text;

/// <summary>Represents the fonts installed on the system. This class cannot be inherited. </summary>
public sealed class InstalledFontCollection : FontCollection
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Text.InstalledFontCollection" /> class. </summary>
	public InstalledFontCollection()
	{
		SafeNativeMethods.Gdip.CheckStatus(GDIPlus.GdipNewInstalledFontCollection(out _nativeFontCollection));
	}
}
