namespace System.Drawing.Printing;

/// <summary>Specifies print preview information for a single page. This class cannot be inherited.</summary>
public sealed class PreviewPageInfo
{
	private Image _image;

	private Size _physicalSize = Size.Empty;

	/// <summary>Gets the image of the printed page.</summary>
	/// <returns>An <see cref="T:System.Drawing.Image" /> representing the printed page.</returns>
	public Image Image => _image;

	/// <summary>Gets the size of the printed page, in hundredths of an inch.</summary>
	/// <returns>A <see cref="T:System.Drawing.Size" /> that specifies the size of the printed page, in hundredths of an inch.</returns>
	public Size PhysicalSize => _physicalSize;

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Printing.PreviewPageInfo" /> class.</summary>
	/// <param name="image">The image of the printed page. </param>
	/// <param name="physicalSize">The size of the printed page, in hundredths of an inch. </param>
	public PreviewPageInfo(Image image, Size physicalSize)
	{
		_image = image;
		_physicalSize = physicalSize;
	}
}
