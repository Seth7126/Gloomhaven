using System.ComponentModel;

namespace System.Drawing.Design;

/// <summary>Provides data for the <see cref="M:System.Drawing.Design.UITypeEditor.PaintValue(System.Object,System.Drawing.Graphics,System.Drawing.Rectangle)" /> method.</summary>
public class PaintValueEventArgs : EventArgs
{
	private readonly ITypeDescriptorContext context;

	private readonly object valueToPaint;

	private readonly Graphics graphics;

	private readonly Rectangle bounds;

	/// <summary>Gets the rectangle that indicates the area in which the painting should be done.</summary>
	/// <returns>The rectangle that indicates the area in which the painting should be done.</returns>
	public Rectangle Bounds => bounds;

	/// <summary>Gets the <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> interface to be used to gain additional information about the context this value appears in.</summary>
	/// <returns>An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that indicates the context of the event.</returns>
	public ITypeDescriptorContext Context => context;

	/// <summary>Gets the <see cref="T:System.Drawing.Graphics" /> object with which painting should be done.</summary>
	/// <returns>A <see cref="T:System.Drawing.Graphics" /> object to use for painting.</returns>
	public Graphics Graphics => graphics;

	/// <summary>Gets the value to paint.</summary>
	/// <returns>An object indicating what to paint.</returns>
	public object Value => valueToPaint;

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Design.PaintValueEventArgs" /> class using the specified values.</summary>
	/// <param name="context">The context in which the value appears. </param>
	/// <param name="value">The value to paint. </param>
	/// <param name="graphics">The <see cref="T:System.Drawing.Graphics" /> object with which drawing is to be done. </param>
	/// <param name="bounds">The <see cref="T:System.Drawing.Rectangle" /> in which drawing is to be done. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="graphics" /> is null.</exception>
	public PaintValueEventArgs(ITypeDescriptorContext context, object value, Graphics graphics, Rectangle bounds)
	{
		this.context = context;
		valueToPaint = value;
		this.graphics = graphics;
		if (graphics == null)
		{
			throw new ArgumentNullException("graphics");
		}
		this.bounds = bounds;
	}
}
